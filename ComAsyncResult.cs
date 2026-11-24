using Hi3Helper.Plugin.Core.Utility;
using Hi3Helper.Plugin.Core.Utility.Windows;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

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
    [FieldOffset(2)]
    private byte _returnFlags;
    [FieldOffset(8)]
    private int _exceptionCount;

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => new((ComAsyncException*)_exception, _exceptionCount);
        set
        {
            nint newExceptionHandle = value.IsEmpty ? nint.Zero : (nint)Unsafe.AsPointer(ref value[0]);
            nint oldException       = Interlocked.Exchange(ref _exception,      newExceptionHandle);
            int  oldExceptionCount  = Interlocked.Exchange(ref _exceptionCount, value.Length);

            if (oldException == nint.Zero || oldExceptionCount == 0)
            {
                return;
            }

            // Dispose old exception
            DisposeException(ref oldException, ref oldExceptionCount);
        }
    }

    /// <summary>
    /// Whether the task is cancelled or not.
    /// </summary>
    public bool IsCancelled
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => (_statusFlags & 0b001) != 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => Interlocked.Exchange(ref _statusFlags, value
                                        ? (byte)(_statusFlags | 0b001)
                                        : (byte)(_statusFlags & ~0b001));
    }

    /// <summary>
    /// Whether the task is successfully executed or not.
    /// </summary>
    public bool IsSuccessful
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => (_statusFlags & 0b010) != 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => Interlocked.Exchange(ref _statusFlags, value
                                        ? (byte)(_statusFlags | 0b010)
                                        : (byte)(_statusFlags & ~0b010));
    }

    /// <summary>
    /// Whether the task is faulted or not.
    /// </summary>
    public bool IsFaulty
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => (_statusFlags & 0b100) != 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => Interlocked.Exchange(ref _statusFlags, value
                                        ? (byte)(_statusFlags | 0b100)
                                        : (byte)(_statusFlags & ~0b100));
    }

    /// <summary>
    /// Whether this instance of task is a ValueTask type.<br/>
    /// Note: To maintain compatibility, both plugins or clients with API version older than 0.1.7.0 will not perform type check for this field. Therefore, all the
    ///       task will be using <see cref="Task"/> anyways with a drawback of allocation overhead in a short, frequently accessed calls.<br/><br/>
    ///
    ///       Starting with API version 0.1.7.0 and newer, the call between the producers (plugins) and consumers (clients) are restricted. The caller and the
    ///       callee must respect and choose the correct task type whether it's a <see cref="ValueTask"/> or <see cref="Task"/>. The type check will be
    ///       performed to ensure the right contract and avoid any unnecessary memory allocation overhead in each call. If you choose a wrong type of the
    ///       task type marshaller, it will throw an <see cref="InvalidOperationException"/> exception on the consumer (client) side.
    /// </summary>
    public bool IsValueTask
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => (_returnFlags & 0b001) != 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => Interlocked.Exchange(ref _returnFlags, value
                                        ? (byte)(_returnFlags | 0b001)
                                        : (byte)(_returnFlags & ~0b001));
    }

    /// <summary>
    /// Whether this instance returns a generic type.<br/>
    /// Note: To maintain compatibility, both plugins or clients with API version older than 0.1.7.0 will not perform type check for this field. Therefore, all the
    ///       task will return a <see langword="default"/> value based on what generic type is it containing.
    /// </summary>
    public bool IsHasReturn
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => (_returnFlags & 0b010) != 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => Interlocked.Exchange(ref _returnFlags, value
                                        ? (byte)(_returnFlags | 0b010)
                                        : (byte)(_returnFlags & ~0b010));
    }

    /// <summary>
    /// Whether this instance requires to perform task type check.
    /// Note: To maintain compatibility, both plugins or clients with API version older than 0.1.7.0 will not perform type check for this field. If not set, all the
    ///       checks against <see cref="IsValueTask"/> or <see cref="IsHasReturn"/> will not be performed.
    /// </summary>
    public bool IsRequireTaskTypeCheck
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        readonly get => (_returnFlags & 0b100) != 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => Interlocked.Exchange(ref _returnFlags, value
                                        ? (byte)(_returnFlags | 0b100)
                                        : (byte)(_returnFlags & ~0b100));
    }

    /// <summary>
    /// Set the result of the task. This method is also used to write the information about the <see cref="Task"/> execution status/result to then being passed to managed code which loads the plugin.
    /// </summary>
    /// <param name="task">The <see cref="Task"/> in which the status/result is being written from.</param>
    internal void SetResult(Task task)
    {
        try
        {
            _ = WriteTaskState(task);
        }
        finally
        {
            PInvoke.SetEvent(Handle);
        }
    }

    /// <summary>
    /// Set the result of the task. This method is also used to write the information about the <see cref="ValueTask"/> execution status/result to then being passed to managed code which loads the plugin.
    /// </summary>
    /// <param name="task">The <see cref="ValueTask"/> in which the status/result is being written from.</param>
    internal void SetResult(ValueTask task)
    {
        try
        {
            _ = WriteTaskState(task);
        }
        finally
        {
            PInvoke.SetEvent(Handle);
        }
    }

    /// <summary>
    /// Set the result of the task. This method is also used to write the information about the <see cref="Task"/> execution status/result to then being passed to managed code which loads the plugin.
    /// </summary>
    /// <param name="task">The <see cref="Task"/> in which the status/result is being written from.</param>
    internal unsafe void SetResult<T>(Task<T> task)
        where T : unmanaged
    {
        try
        {
            if (!WriteTaskState(task))
                return;

#if DEBUG
#if MANUALCOM
            string nameOfT = $"<unknown>_sizeof({sizeof(T)})";
#else
            string nameOfT = typeof(T).Name;
#endif
#endif

            // Allocate result pointer
            Interlocked.Exchange(ref _resultP, (nint)Mem.Alloc<T>());
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
            PInvoke.SetEvent(Handle);
        }
    }

    /// <summary>
    /// Set the result of the task. This method is also used to write the information about the <see cref="ValueTask{T}"/> execution status/result to then being passed to managed code which loads the plugin.
    /// </summary>
    /// <param name="task">The <see cref="ValueTask{T}"/> in which the status/result is being written from.</param>
    internal unsafe void SetResult<T>(ValueTask<T> task)
        where T : unmanaged
    {
        try
        {
            if (!WriteTaskState(task))
                return;

#if DEBUG
#if MANUALCOM
            string nameOfT = $"<unknown>_sizeof({sizeof(T)})";
#else
            string nameOfT = typeof(T).Name;
#endif
#endif

            // Allocate result pointer
            Interlocked.Exchange(ref _resultP, (nint)Mem.Alloc<T>());
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
            PInvoke.SetEvent(Handle);
        }
    }

    private bool WriteTaskState(Task task)
    {
        IsFaulty     = task.IsFaulted;
        IsCancelled  = task.IsCanceled;
        IsSuccessful = task.IsCompletedSuccessfully;

        if (!IsFaulty && !IsCancelled)
        {
            return true; // The Task has completed successfully.
        }

        WriteTaskException(task);
        return false;
    }

    private bool WriteTaskState(ValueTask task)
    {
        IsFaulty     = task.IsFaulted;
        IsCancelled  = task.IsCanceled;
        IsSuccessful = task.IsCompletedSuccessfully;

        if (!IsFaulty && !IsCancelled && IsSuccessful)
        {
            return true; // The Task has completed successfully.
        }

        AssertEnsureValueTaskGetInnerTask(task, out Task asTask);
        WriteTaskException(asTask);
        return false;
    }

    private bool WriteTaskState<T>(ValueTask<T> task)
    {
        IsFaulty     = task.IsFaulted;
        IsCancelled  = task.IsCanceled;
        IsSuccessful = task.IsCompletedSuccessfully;

        if (!IsFaulty && !IsCancelled && IsSuccessful)
        {
            return true; // The Task has completed successfully.
        }

        AssertEnsureValueTaskGetInnerTask(task, out Task<T> asTask);
        WriteTaskException(asTask);
        return false;
    }

    private void WriteTaskException(Task task)
    {
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
#if MANUALCOM
        const string exceptionName = "<unknown>";
#else
        string exceptionName = exception.GetType().Name;
#endif
        // Log the exception info
        SharedStatic.InstanceLogger.LogDebug("[ComAsyncResult::WriteExceptionRecursive]: Writing parent exception: {ExceptionName}", exceptionName);
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
            SharedStatic.InstanceLogger.LogDebug("[ComAsyncResult::WriteExceptionRecursive]: Writing inner exception: {ExceptionName}", exceptionName);
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

    private void AssertEnsureValueTaskGetInnerTask(ValueTask task, out Task innerTask)
    {
        Unsafe.SkipInit(out innerTask);

        Task asTask = task.AsTask();
#if DEBUG
        Debug.Assert(asTask != null, "The ValueTask does not contain an inner Task or IValueTaskSource");
#endif

        if (asTask == null!)
            throw new InvalidOperationException("The ValueTask.AsTask() SHOULD returns a Task!");

        innerTask = asTask;
    }

    private void AssertEnsureValueTaskGetInnerTask<T>(ValueTask<T> task, out Task<T> innerTask)
    {
        Unsafe.SkipInit(out innerTask);

        Task<T> asTask = task.AsTask();
#if DEBUG
        Debug.Assert(asTask != null, "The ValueTask does not contain an inner Task or IValueTaskSource");
#endif

        if (asTask == null!)
            throw new InvalidOperationException("The ValueTask.AsTask() SHOULD returns a Task!");

        innerTask = asTask;
    }

    /// <summary>
    /// Create/Alloc an instance of <see cref="ComAsyncResult"/> struct.
    /// </summary>
    /// <param name="task">The <see cref="Task"/> instance in which the result being passed to <see cref="ComAsyncResult"/></param>
    /// <returns>A handle of the <see cref="ComAsyncResult"/> struct.</returns>
    public static unsafe nint Alloc(Task task)
    {
        // Get the result and allocate the ComAsyncResult handle
        ComAsyncResult* resultP = Mem.Alloc<ComAsyncResult>();

        // Allocate wait handle
        resultP->Handle = PInvoke.CreateEvent(nint.Zero, 1, 0, null);

        //  0.1.7.0: Set the flag to perform task type checks.
        resultP->IsRequireTaskTypeCheck = true;

        // If the task is completed before the OnCompleted was triggered, then set the result earlier
        // and return the pointer.
        if (task.IsCompleted || task.IsCanceled || task.IsFaulted)
        {
            resultP->SetResult(task);
            return (nint)resultP;
        }

        // Set the "attach status" callback to the task completion, then return the async result handle
        task.GetAwaiter().OnCompleted(() => resultP->SetResult(task));
        return (nint)resultP;
    }

    /// <summary>
    /// Create/Alloc an instance of <see cref="ComAsyncResult"/> struct.
    /// </summary>
    /// <param name="task">The <see cref="Task"/> instance in which the result being passed to <see cref="ComAsyncResult"/></param>
    /// <returns>A handle of the <see cref="ComAsyncResult"/> struct.</returns>
    public static unsafe nint Alloc<T>(Task<T> task)
        where T : unmanaged
    {
        // Get the result and allocate the ComAsyncResult handle
        ComAsyncResult* resultP = Mem.Alloc<ComAsyncResult>();

        // Allocate wait handle
        resultP->Handle = PInvoke.CreateEvent(nint.Zero, 1, 0, null);

        //  0.1.7.0: Set the flag to perform task type checks.
        resultP->IsRequireTaskTypeCheck = true;

        //  0.1.7.0: Set the flag to tell the task has return value.
        resultP->IsHasReturn = true;

        // If the task is completed before the OnCompleted was triggered, then set the result earlier
        // and return the pointer.
        if (task.IsCompleted || task.IsCanceled || task.IsFaulted)
        {
            resultP->SetResult(task);
            return (nint)resultP;
        }

        // Set the "attach status" callback to the task completion, then return the async result handle
        task.GetAwaiter().OnCompleted(() => resultP->SetResult(task));
        return (nint)resultP;
    }

    /// <summary>
    /// Create/Alloc an instance of <see cref="ComAsyncResult"/> struct.
    /// </summary>
    /// <param name="task">The <see cref="ValueTask"/> instance in which the result being passed to <see cref="ComAsyncResult"/></param>
    /// <returns>A handle of the <see cref="ComAsyncResult"/> struct.</returns>
    public static unsafe nint Alloc(ValueTask task)
    {
        // Get the result and allocate the ComAsyncResult handle
        ComAsyncResult* resultP = Mem.Alloc<ComAsyncResult>();
        resultP->IsValueTask = true; // 0.1.7.0: Set the ValueTask contract.

        // Allocate wait handle
        resultP->Handle = PInvoke.CreateEvent(nint.Zero, 1, 0, null);

        //  0.1.7.0: Set the flag to perform task type checks.
        resultP->IsRequireTaskTypeCheck = true;

        // If the task is completed before the OnCompleted was triggered, then set the result earlier
        // and return the pointer.
        if (task.IsCompleted || task.IsCanceled || task.IsFaulted)
        {
            resultP->SetResult(task);
            return (nint)resultP;
        }

        // Set the "attach status" callback to the task completion, then return the async result handle
        task.GetAwaiter().OnCompleted(() => resultP->SetResult(task));
        return (nint)resultP;
    }

    /// <summary>
    /// Create/Alloc an instance of <see cref="ComAsyncResult"/> struct.
    /// </summary>
    /// <param name="task">The <see cref="ValueTask{T}"/> instance in which the result being passed to <see cref="ComAsyncResult"/></param>
    /// <returns>A handle of the <see cref="ComAsyncResult"/> struct.</returns>
    public static unsafe nint Alloc<T>(ValueTask<T> task)
        where T : unmanaged
    {
        // Get the result and allocate the ComAsyncResult handle
        ComAsyncResult* resultP = Mem.Alloc<ComAsyncResult>();
        resultP->IsValueTask = true; // 0.1.7.0: Set the ValueTask contract.

        // Allocate wait handle
        resultP->Handle = PInvoke.CreateEvent(nint.Zero, 1, 0, null);

        //  0.1.7.0: Set the flag to perform task type checks.
        resultP->IsRequireTaskTypeCheck = true;

        //  0.1.7.0: Set the flag to tell the task has return value.
        resultP->IsHasReturn = true;

        // If the task is completed before the OnCompleted was triggered, then set the result earlier
        // and return the pointer.
        if (task.IsCompleted || task.IsCanceled || task.IsFaulted)
        {
            resultP->SetResult(task);
            return (nint)resultP;
        }

        // Set the "attach status" callback to the task completion, then return the async result handle
        task.GetAwaiter().OnCompleted(() => resultP->SetResult(task));
        return (nint)resultP;
    }

    /// <summary>
    /// Gets the <see cref="Microsoft.Win32.SafeHandles.SafeWaitHandle"/> handle from <see cref="ComAsyncResult"/>'s handle.
    /// </summary>
    /// <param name="handle">A handle of the <see cref="ComAsyncResult"/> struct.</param>
    /// <returns>A <see cref="Microsoft.Win32.SafeHandles.SafeWaitHandle"/> handle.</returns>

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe nint GetWaitHandle(nint handle)
        => ((ComAsyncResult*)handle)->Handle;

    /// <summary>
    /// Gets the task completion of the <see cref="ComAsyncResult"/>'s handle.
    /// </summary>
    /// <param name="handle">A handle of the <see cref="ComAsyncResult"/> struct.</param>
    /// <param name="isSuccessful">Whether the task has been completed successfully.</param>
    /// <param name="isCancelled">Whether the task has been completed due to getting cancelled.</param>
    /// <param name="isFaulty">Whether the task has been completed due to being faulty (throwing an <see cref="Exception"/>/error).</param>
    /// <returns>
    /// <see langword="true"/> if the task has been completed and either one of <paramref name="isSuccessful"/>/<paramref name="isCancelled"/>/<paramref name="isFaulty"/> should return <see langword="true"/>. <see langword="false"/> if the task is still running.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe bool GetTaskCompletionState(nint     handle,
                                                     out bool isSuccessful,
                                                     out bool isCancelled,
                                                     out bool isFaulty)
    {
        var result = (ComAsyncResult*)handle;
        if (result == null)
            throw new NullReferenceException("Cannot get task completion state as the handle/pointer is null.");

        isSuccessful = result->IsSuccessful;
        isCancelled  = result->IsCancelled;
        isFaulty     = result->IsFaulty;

        return isSuccessful || isCancelled || isFaulty;
    }

    /// <summary>
    /// Dispose/Free the <see cref="ComAsyncResult"/> struct from its pointer.
    /// </summary>
    /// <param name="resultP">A pointer of the <see cref="ComAsyncResult"/> struct.</param>

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void FreeResult(nint resultP)
        => ((ComAsyncResult*)resultP)->Dispose();

    /// <summary>
    /// Dispose this instance of the current <see cref="ComAsyncResult"/> struct.
    /// </summary>
    public unsafe void Dispose()
    {
        if (Interlocked.Exchange(ref _isFreed, 1) == 1)
            return;

        DisposeException(ref _exception, ref _exceptionCount);
        if (_resultP != nint.Zero)
        {
            _resultP.Free();
            _resultP = nint.Zero;
        }

        this.Free();
    }

    private static unsafe void DisposeException(ref nint exceptionHandle, ref int exceptionCount)
    {
        if (exceptionHandle == nint.Zero || exceptionCount == 0)
            return;

        using PluginDisposableMemory<ComAsyncException> span = new((ComAsyncException*)exceptionHandle, exceptionCount);
        for (int i = 0; i < span.Length; i++)
        {
            ref ComAsyncException current = ref span[i];
            current.Dispose();
        }

        exceptionHandle = 0;
        exceptionCount  = 0;
    }
}
