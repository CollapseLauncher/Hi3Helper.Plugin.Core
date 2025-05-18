using System;
using Microsoft.Extensions.Logging;
using Microsoft.Win32.SafeHandles;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using static Hi3Helper.Plugin.Core.SharedStatic;

// ReSharper disable AccessToModifiedClosure
namespace Hi3Helper.Plugin.Core.Utility;

public delegate void ComAsyncGetResultDelegate<in T>(T result);
public unsafe delegate void ComAsyncResultAttachAfterCallStateDelegate(Task task, Lock threadLock, ComAsyncResult* resultP);

public static partial class ComAsyncExtension
{
    private static readonly Lock CurrentThreadLock = new();

    public static unsafe nint AsResult(this Task task)
        => ComAsyncResult.Alloc(CurrentThreadLock, task, AttachAfterCallState);

    [OverloadResolutionPriority(1)]
    public static unsafe nint AsResult<T>(this Task<T> task, ComAsyncGetResultDelegate<T>? getResultDelegate)
    {
        task.ContinueWith(t =>
        {
            using (CurrentThreadLock.EnterScope())
            {
                if (t.IsCompletedSuccessfully)
                {
                    getResultDelegate?.Invoke(t.Result);
                }


#if DEBUG
                InstanceLogger?.LogDebug("[ComAsyncExtension::AsResult::ContinueWith] Executed!");
#endif
            }
        });

        return ComAsyncResult.Alloc(CurrentThreadLock, task, AttachAfterCallState);
    }

    private static unsafe void AttachAfterCallState(Task task, Lock threadLock, ComAsyncResult* result)
    {
        result->SetResult(threadLock, task);

#if DEBUG
        InstanceLogger?.LogDebug("[ComAsyncExtension::AttachAfterCallState] AsyncResult state attached!");
#endif
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
            ComAsyncResult.Free(handle);
        }
    }

    private static unsafe void EnsureSuccessResult(nint handle)
    {
        ComAsyncResult* asyncResult = (ComAsyncResult*)handle;

        if (asyncResult->IsCancelled || asyncResult->IsFaulty)
        {
            StackTrace currentStackTrace = new StackTrace(true);
            Exception? exception = ComAsyncException.GetExceptionFromHandle(asyncResult->ExceptionHandle);

            if (exception != null)
            {
                exception.SetExceptionStackTrace() = "\r\n" + currentStackTrace;
                throw exception;
            }

            if (asyncResult->IsCancelled)
            {
                throw new TaskCanceledException();
            }
        }

        if (asyncResult->IsFaulty)
        {
            throw new COMException();
        }
    }
}
