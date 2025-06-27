using Hi3Helper.Plugin.Core.Utility;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace Hi3Helper.Plugin.Core;

/// <summary>
/// Represents the result of a COM asynchronous operation.
/// </summary>
[StructLayout(LayoutKind.Explicit)] // Fits to 48 bytes
public struct ComAsyncResult() : IDisposable
{
    [FieldOffset(0)]
    private byte _isFreed = 0;
    [FieldOffset(1)]
    private byte _statusFlags;
    [FieldOffset(8)]
    private int  _exceptionCount;

    /// <summary>
    /// The handle to the <see cref="Microsoft.Win32.SafeHandles.SafeWaitHandle"/>.
    /// </summary>
    [FieldOffset(16)]
    public nint Handle;
    [FieldOffset(24)]
    private nint _exception;
    [FieldOffset(32)]
    internal nint _resultP;
    [FieldOffset(40)]
    internal nint _reserved;

    /// <summary>
    /// The span handle in which stores the information of the exceptions on <see cref="ComAsyncException"/> struct.
    /// </summary>
    public unsafe PluginDisposableMemory<ComAsyncException> ExceptionMemory
    {
        readonly get => new((ComAsyncException*)_exception, _exceptionCount);
        set
        {
            _exceptionCount = value.Length;
            _exception = (nint)Unsafe.AsPointer(ref value[0]);
        }
    }

    /// <summary>
    /// Whether the task is cancelled or not.
    /// </summary>
    public bool IsCancelled
    {
        readonly get => (_statusFlags & 0b001) != 0;
        set => _statusFlags = value
            ? (byte)(_statusFlags | 0b001)
            : (byte)(_statusFlags & ~0b001);
    }

    /// <summary>
    /// Whether the task is successfully executed or not.
    /// </summary>
    public bool IsSuccessful
    {
        readonly get => (_statusFlags & 0b010) != 0;
        set => _statusFlags = value
            ? (byte)(_statusFlags | 0b010)
            : (byte)(_statusFlags & ~0b010);
    }

    /// <summary>
    /// Whether the task is faulted or not.
    /// </summary>
    public bool IsFaulty
    {
        readonly get => (_statusFlags & 0b100) != 0;
        set => _statusFlags = value
            ? (byte)(_statusFlags | 0b100)
            : (byte)(_statusFlags & ~0b100);
    }

    /// <summary>
    /// Set the result of the task. This method is also used to write the information about the <see cref="Task"/> execution status/result to then being passed to managed code which loads the plugin.
    /// </summary>
    /// <param name="threadLock">Thread lock to be used to set the result.</param>
    /// <param name="task">The <see cref="Task"/> in which the status/result is being written from.</param>
    public void SetResult(Lock threadLock, Task task)
    {
        using (threadLock.EnterScope())
        {
            try
            {
                WriteTaskState(task);
            }
            finally
            {
                ComAsyncExtension.SetEvent(Handle);
            }
        }
    }

    /// <summary>
    /// Set the result of the task. This method is also used to write the information about the <see cref="Task"/> execution status/result to then being passed to managed code which loads the plugin.
    /// </summary>
    /// <param name="threadLock">Thread lock to be used to set the result.</param>
    /// <param name="task">The <see cref="Task"/> in which the status/result is being written from.</param>
    public unsafe void SetResult<T>(Lock threadLock, Task<T> task)
        where T : unmanaged
    {
        using (threadLock.EnterScope())
        {
            try
            {
                WriteTaskState(task);

                if (task.IsFaulted || task.IsCanceled)
                {
                    return;
                }

#if DEBUG
#if MANUALCOM
                string nameOfT = $"<unknown>_sizeof({sizeof(T)})";
#else
                string nameOfT = typeof(T).Name;
#endif
#endif

                // Allocate result pointer
                _resultP = (nint)Mem.Alloc<T>();
#if DEBUG
                // Log the exception info
                SharedStatic.InstanceLogger.LogDebug("[ComAsyncResult::SetResult<{nameOfT}>] Result will be written at ptr: 0x{RetAddress:x8}", nameOfT, _resultP);
#endif

                if (_resultP == nint.Zero)
                {
#if DEBUG
                    SharedStatic.InstanceLogger.LogDebug("[ComAsyncResult::SetResult<{nameOfT}>] AsyncResult _resultP isn't allocated. The return value will not be written!", nameOfT);
#endif
                    return;
                }

                if (task.IsCompletedSuccessfully)
                {
                    Unsafe.Write((void*)_resultP, task.Result);
                }

#if DEBUG
                SharedStatic.InstanceLogger.LogDebug("[ComAsyncResult::SetResult<{nameOfT}>] AsyncResult return value value has been written!", nameOfT);
#endif
            }
            finally
            {
                ComAsyncExtension.SetEvent(Handle);
            }
        }
    }

    private void WriteTaskState(Task task)
    {
        IsFaulty = task.IsFaulted;
        IsCancelled = task.IsCanceled;
        IsSuccessful = task.IsCompletedSuccessfully;

        if (!IsFaulty && !IsCancelled)
        {
            return;
        }

        Exception? exception = task.Exception?.Flatten();
        exception = exception is AggregateException ? exception.InnerException : exception;

        int exceptionCount = GetExceptionCount(exception);
        if (exceptionCount == 0)
        {
            return;
        }

        ExceptionMemory = PluginDisposableMemory<ComAsyncException>.Alloc(exceptionCount);
        WriteExceptionRecursive(exception, ExceptionMemory);
    }

    private static int GetExceptionCount(Exception? exception)
    {
        if (exception == null)
        {
            return 0;
        }

        int count = 1;
        while ((exception = exception.InnerException) != null)
        {
            count++;
        }

        return count;
    }

    private static void WriteExceptionRecursive(Exception? exception, PluginDisposableMemory<ComAsyncException> exceptionMemory)
    {
        if (exception == null)
        {
            return;
        }

#if DEBUG
        // Log the exception info
        SharedStatic.InstanceLogger.LogDebug("[ComAsyncResult::WriteExceptionRecursive]: Writing parent exception: {ExceptionName}", exception.GetType().Name);
#endif

        // Write parent exception
        ComAsyncExtension.WriteExceptionInfo(exception, ref exceptionMemory[0]);

        // If inner exception is null, return.
        exception = exception.InnerException;
        if (exception == null)
        {
            return;
        }

        // Write inner exception
        for (int i = 1; i < exceptionMemory.Length && exception != null; i++)
        {
#if DEBUG
            // Log the exception info
            SharedStatic.InstanceLogger.LogDebug("[ComAsyncResult::WriteExceptionRecursive]: Writing inner exception: {ExceptionName}", exception.GetType().Name);
#endif

            // Write current inner exception
            ComAsyncExtension.WriteExceptionInfo(exception, ref exceptionMemory[i]);

            // Go to the next exception
            exception = exception.InnerException;
        }

#if DEBUG
        // Log the exception info
        SharedStatic.InstanceLogger.LogDebug("[ComAsyncResult::WriteExceptionRecursive]: Write completed!");
#endif
    }

    /// <summary>
    /// Create/Alloc an instance of <see cref="ComAsyncResult"/> struct.
    /// </summary>
    /// <param name="threadLock">Thread lock to be used to create the <see cref="ComAsyncResult"/> struct.</param>
    /// <param name="task">The <see cref="Task"/> instance in which the result being passed to <see cref="ComAsyncResult"/></param>
    /// <returns>A handle of the <see cref="ComAsyncResult"/> struct.</returns>
    public static unsafe nint Alloc(Lock threadLock, Task task)
    {
        // Enter and lock the current thread
        using (threadLock.EnterScope())
        {
            // Get the result and allocate the ComAsyncResult handle
            ComAsyncResult* resultP = Mem.Alloc<ComAsyncResult>();

            // Allocate wait handle
            resultP->Handle = ComAsyncExtension.CreateEvent(nint.Zero, true, false, null);

            // Set the "attach status" callback to the task completion, then return the async result handle
            task.GetAwaiter().OnCompleted(() => resultP->SetResult(threadLock, task));
            return (nint)resultP;
        }
    }

    /// <summary>
    /// Create/Alloc an instance of <see cref="ComAsyncResult"/> struct.
    /// </summary>
    /// <param name="threadLock">Thread lock to be used to create the <see cref="ComAsyncResult"/> struct.</param>
    /// <param name="task">The <see cref="Task"/> instance in which the result being passed to <see cref="ComAsyncResult"/></param>
    /// <returns>A handle of the <see cref="ComAsyncResult"/> struct.</returns>
    public static unsafe nint Alloc<T>(Lock threadLock, Task<T> task)
        where T : unmanaged
    {
        // Enter and lock the current thread
        using (threadLock.EnterScope())
        {
            // Get the result and allocate the ComAsyncResult handle
            ComAsyncResult* resultP = Mem.Alloc<ComAsyncResult>();

            // Allocate wait handle
            resultP->Handle = ComAsyncExtension.CreateEvent(nint.Zero, true, false, null);

            // Set the "attach status" callback to the task completion, then return the async result handle
            task.GetAwaiter().OnCompleted(() => resultP->SetResult(threadLock, task));
            return (nint)resultP;
        }
    }

    /// <summary>
    /// Gets the <see cref="Microsoft.Win32.SafeHandles.SafeWaitHandle"/> handle from <see cref="ComAsyncResult"/>'s handle.
    /// </summary>
    /// <param name="handle">A handle of the <see cref="ComAsyncResult"/> struct.</param>
    /// <returns>A <see cref="Microsoft.Win32.SafeHandles.SafeWaitHandle"/> handle.</returns>
    public static unsafe nint GetWaitHandle(nint handle)
    {
        ref ComAsyncResult asyncResult = ref Unsafe.AsRef<ComAsyncResult>((void*)handle);
        return asyncResult.Handle;
    }

    /// <summary>
    /// Dispose/Free the handle of the <see cref="ComAsyncResult"/> struct.
    /// </summary>
    /// <param name="handle">A handle of the <see cref="ComAsyncResult"/> struct.</param>
    public static unsafe void DisposeHandle(nint handle)
    {
        ref ComAsyncResult asyncResult = ref Unsafe.AsRef<ComAsyncResult>((void*)handle);
        asyncResult.Dispose();
    }

    /// <summary>
    /// Dispose this instance of <see cref="ComAsyncResult"/> struct.
    /// </summary>
    public unsafe void Dispose()
    {
        if (_isFreed == 1) return;

        ExceptionMemory.Dispose();
        if (_resultP != nint.Zero)
        {
            _resultP.Free();
            _resultP = nint.Zero;
        }

        void* ptr = Unsafe.AsPointer(ref this);
        Mem.Free(ptr);

        _isFreed = 1;
    }
}
