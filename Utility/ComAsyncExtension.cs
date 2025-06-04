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

    public static unsafe nint AsResult<T>(this Task<T> task)
        where T : unmanaged
        => ComAsyncResult.Alloc(CurrentThreadLock, task, AttachAfterCallState);

    private static unsafe void AttachAfterCallState(Task task, Lock threadLock, ComAsyncResult* result)
    {
        if (result == null)
        {
#if DEBUG
            InstanceLogger?.LogError("[ComAsyncExtension::AttachAfterCallState] ComAsyncResult* has unexpectedly set to null!");
#endif
            return;
        }
        result->SetResult(threadLock, task);

#if DEBUG
        InstanceLogger?.LogDebug("[ComAsyncExtension::AttachAfterCallState] AsyncResult state attached!");
#endif
    }

    public static async Task WaitFromHandle(this nint handle)
    {
        SafeWaitHandle? asyncSafeHandle = null;
        EventWaitHandle? waitHandle = null;
        try
        {
            asyncSafeHandle = new SafeWaitHandle(ComAsyncResult.GetWaitHandle(handle), false);
            waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset)
            {
                SafeWaitHandle = asyncSafeHandle
            };

            await Task.Factory.StartNew(waitHandle.WaitOne);
            EnsureSuccessResult(handle);
        }
        finally
        {
            asyncSafeHandle?.Dispose();
            waitHandle?.Dispose();
            ComAsyncResult.DisposeHandle(handle);
        }
    }

    private static void EnsureSuccessResult(nint handle)
    {
        ref ComAsyncResult asyncResult = ref handle.AsRef<ComAsyncResult>();

        if (asyncResult.IsCancelled || asyncResult.IsFaulty)
        {
            StackTrace currentStackTrace = new(true);
            Exception? exception = ComAsyncException.GetExceptionFromHandle(asyncResult.ExceptionMemory);

            if (exception != null)
            {
                exception.SetExceptionRemoteStackTrace() += Environment.NewLine;
                exception.SetExceptionStackTrace() = currentStackTrace;
                throw exception;
            }

            if (asyncResult.IsCancelled)
            {
                throw new TaskCanceledException();
            }
        }

        if (asyncResult.IsFaulty)
        {
            throw new COMException();
        }
    }
}
