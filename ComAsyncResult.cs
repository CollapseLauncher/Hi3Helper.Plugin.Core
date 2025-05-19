using Hi3Helper.Plugin.Core.Utility;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Hi3Helper.Plugin.Core;

[StructLayout(LayoutKind.Sequential)]
public unsafe struct ComAsyncResult
{
    public nint Handle;
    public nint ExceptionHandle;

    private byte _statusFlags;

    public bool IsCancelled
    {
        get => (_statusFlags & 0b001) != 0;
        set => _statusFlags = value
            ? (byte)(_statusFlags | 0b001)
            : (byte)(_statusFlags & ~0b001);
    }

    public bool IsSuccessful
    {
        get => (_statusFlags & 0b010) != 0;
        set => _statusFlags = value
            ? (byte)(_statusFlags | 0b010)
            : (byte)(_statusFlags & ~0b010);
    }

    public bool IsFaulty
    {
        get => (_statusFlags & 0b100) != 0;
        set => _statusFlags = value
            ? (byte)(_statusFlags | 0b100)
            : (byte)(_statusFlags & ~0b100);
    }

    public void SetResult(Lock threadLock, Task task)
    {
        using (threadLock.EnterScope())
        {
            IsFaulty     = task.IsFaulted;
            IsCancelled  = task.IsCanceled;
            IsSuccessful = task.IsCompletedSuccessfully;

            if (!IsFaulty && !IsCancelled)
            {
                return;
            }

            Exception? exception = task.Exception?.Flatten();
            exception = exception is AggregateException ? exception.InnerException : exception;

            if (exception == null)
            {
                return;
            }

            ComAsyncException* exceptionHandleP = Mem.AllocZeroed<ComAsyncException>();
            ExceptionHandle = (nint)exceptionHandleP;

            WriteExceptionRecursive(exception, exceptionHandleP);
        }
    }

    private static void WriteExceptionRecursive(Exception? exception, ComAsyncException* exceptionHandle)
    {
        if (exception == null)
        {
            return;
        }

#if DEBUG
        // Log the exception info
        SharedStatic.InstanceLogger?.LogDebug("[ComAsyncResult::WriteExceptionRecursive]: Writing parent exception: {ExceptionName}", exception.GetType().Name);
#endif

        // Write parent exception
        ComAsyncExtension.WriteExceptionInfo(exception, exceptionHandle);

        // Write inner exception recursively
        ComAsyncException* lastException = exceptionHandle;
        exception = exception.InnerException;
        while (exception != null)
        {
            // Write current inner exception
            ComAsyncException* innerExceptionHandleP = Mem.AllocZeroed<ComAsyncException>();
            ComAsyncExtension.WriteExceptionInfo(exception, innerExceptionHandleP);

            // Write previous handle pointer
            innerExceptionHandleP->PreviousExceptionHandle = (nint)lastException;

            // Set this exception as the next one
            lastException->NextExceptionHandle = (nint)innerExceptionHandleP;

#if DEBUG
            // Log the exception info
            SharedStatic.InstanceLogger?.LogDebug("[ComAsyncResult::WriteExceptionRecursive]: Writing inner exception: {ExceptionName}", exception.GetType().Name);
#endif
            // Move to the next inner exception
            exception = exception.InnerException;

            // Set the last inner exception pointer to the current one
            lastException = innerExceptionHandleP;
        }

#if DEBUG
        // Log the exception info
        SharedStatic.InstanceLogger?.LogDebug("[ComAsyncResult::WriteExceptionRecursive]: Write completed!");
#endif
    }

    public static nint Alloc(Lock threadLock, Task task, ComAsyncResultAttachAfterCallStateDelegate attachCallback)
    {
        // Enter and lock the current thread
        using (threadLock.EnterScope())
        {
            // Get the result and allocate the ComAsyncResult handle
            IAsyncResult    asyncResult = task;
            ComAsyncResult* resultP     = Mem.Alloc<ComAsyncResult>();

            // Set the WaitHandle to the handle of the async result
            resultP->Handle = asyncResult.AsyncWaitHandle.SafeWaitHandle.DangerousGetHandle();

            // Set the "attach status" callback to the task completion, then return the async result handle
            task.GetAwaiter().OnCompleted(() => attachCallback(task, threadLock, resultP));
            return (nint)resultP;
        }
    }

    public static void Free(nint handle)
    {
        // Get the handle as pointer
        ComAsyncResult*    handleP          = handle.AsPointer<ComAsyncResult>();
        ComAsyncException* exceptionHandleP = handleP->ExceptionHandle.AsPointer<ComAsyncException>();

        // If the exception handle is null, we can just free the main handle
        while (exceptionHandleP != null)
        {
            // Store the inner exception first before freeing the current one
            nint innerExceptionHandle = exceptionHandleP->NextExceptionHandle;

            // Free the current exception and then move to the inner one
            Mem.Free(exceptionHandleP);
            exceptionHandleP = innerExceptionHandle.AsPointer<ComAsyncException>();
        }

        // Once all the exceptions are freed, free the main handle
        Mem.Free(handleP);
    }
}
