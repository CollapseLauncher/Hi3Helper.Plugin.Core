using Microsoft.Extensions.Logging;
using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using static Hi3Helper.Plugin.Core.SharedStatic;

// ReSharper disable AccessToModifiedClosure
namespace Hi3Helper.Plugin.Core.Utility;

public delegate void ComAsyncGetResultDelegate<in T>(T result);

public static partial class ComAsyncExtension
{
    public static nint AsHandle(this Task task)
    {
        IAsyncResult asyncResult = task;
        ComAsyncResult result = new ComAsyncResult
        {
            Handle = asyncResult.AsyncWaitHandle.SafeWaitHandle.DangerousGetHandle()
        };

        task.GetAwaiter().OnCompleted(() => AttachAfterCallState(task, ref result));
        return GCHandle.ToIntPtr(GCHandle.Alloc(result, GCHandleType.Normal));
    }

    [OverloadResolutionPriority(1)]
    public static unsafe nint AsHandle<T>(this Task<T> task, ComAsyncGetResultDelegate<T>? getResultDelegate)
    {
        task.ContinueWith(t =>
        {
            if (t.IsCompletedSuccessfully)
            {
                getResultDelegate?.Invoke(t.Result);
            }

#if DEBUG
            InstanceLogger?.LogDebug("ContinueWith executed!");
#endif
        });

        IAsyncResult asyncResult = task;
        ComAsyncResult result = new ComAsyncResult
        {
            Handle = asyncResult.AsyncWaitHandle.SafeWaitHandle.DangerousGetHandle(),
        };

        task.GetAwaiter().OnCompleted(() => AttachAfterCallState(task, ref result));
        // return GCHandle.ToIntPtr(GCHandle.Alloc(result, GCHandleType.Normal));
        return (nint)Unsafe.AsPointer(ref Unsafe.AsRef(ref result));
    }

    private static void AttachAfterCallState(Task task, ref ComAsyncResult result)
    {
        Interlocked.Exchange(ref result.IsSuccessful, task.IsCompletedSuccessfully);
        Interlocked.Exchange(ref result.IsCancelled, task.IsCanceled);
        Interlocked.Exchange(ref result.IsFaulty, task.IsFaulted);

        Exception? exception = task.Exception?.Flatten();
        if (exception != null)
        {
            WriteExceptionInfo(exception, ref result);
        }

#if DEBUG
        InstanceLogger?.LogDebug("AttachAfterCallState executed!");
#endif
    }

    private static unsafe void WriteExceptionInfo(Exception exception, ref ComAsyncResult result)
    {
        exception = exception.InnerException ?? exception;

        string exceptionName    = exception.GetType().Name;
        string exceptionMessage = exception.Message;

        fixed (byte* exceptionNameAddress    = result.ExceptionTypeByName)
        fixed (byte* exceptionMessageAddress = result.ExceptionMessage)
        {
            Span<byte> exceptionNameSpan    = new(exceptionNameAddress, ComAsyncResult.ExceptionTypeNameMaxLength - 1);
            Span<byte> exceptionMessageSpan = new(exceptionMessageAddress, ComAsyncResult.ExceptionMessageMaxLength - 1);

            Encoding.UTF8.GetBytes(exceptionName,    exceptionNameSpan);
            Encoding.UTF8.GetBytes(exceptionMessage, exceptionMessageSpan);
        }

        InstanceLogger?.LogError(exception, "An exception was thrown by a task! {ExName}: {ExMessage}", exceptionName, exceptionMessage);
    }

    private static unsafe nint GetWaitHandle(nint handle)
    {
        ComAsyncResult* asyncResult = (ComAsyncResult*)handle;
        return asyncResult->Handle;
    }

    public static async Task WaitFromHandle(this nint handle)
    {
        SafeWaitHandle? asyncSafeHandle = null;
        EventWaitHandle? waitHandle = null;
        try
        {
            asyncSafeHandle = new SafeWaitHandle(GetWaitHandle(handle), false);
            waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset)
            {
                SafeWaitHandle = asyncSafeHandle
            };

            await Task.Run(waitHandle.WaitOne);
            EnsureSuccessResult(handle);
        }
        finally
        {
            asyncSafeHandle?.Dispose();
            waitHandle?.Dispose();
        }
    }

    private static unsafe void EnsureSuccessResult(nint handle)
    {
        ComAsyncResult* asyncResult = (ComAsyncResult*)handle;

        if (*asyncResult->ExceptionTypeByName != 0)
        {
            ThrowExceptionFromInfo(asyncResult);
        }

        if (asyncResult->IsCancelled)
        {
            throw new TaskCanceledException();
        }

        if (asyncResult->IsFaulty)
        {
            throw new COMException();
        }
    }
}
