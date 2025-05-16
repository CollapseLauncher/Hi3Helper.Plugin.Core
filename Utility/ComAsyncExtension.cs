using Microsoft.Extensions.Logging;
using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using static Hi3Helper.Plugin.Core.SharedStatic;

// ReSharper disable AccessToModifiedClosure
namespace Hi3Helper.Plugin.Core.Utility;

public delegate void ComAsyncGetResultDelegate<in T>(T result);

public static class ComAsyncExtension
{
    public static unsafe nint AsHandle(this Task task)
    {
        IAsyncResult asyncResult = task;
        ComAsyncResult result = new ComAsyncResult
        {
            Handle = asyncResult.AsyncWaitHandle.SafeWaitHandle.DangerousGetHandle()
        };

        task.GetAwaiter().OnCompleted(() =>
        {
            Interlocked.Exchange(ref result.IsSuccessful, task.IsCompletedSuccessfully);
            Interlocked.Exchange(ref result.IsCancelled, task.IsCanceled);
            Interlocked.Exchange(ref result.IsFaulty, task.IsFaulted);

#if DEBUG
            InstanceLogger?.LogDebug("OnCompleted executed!");
#endif
        });

        return (nint)Unsafe.AsPointer(ref result);
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
            Handle = asyncResult.AsyncWaitHandle.SafeWaitHandle.DangerousGetHandle()
        };

        task.GetAwaiter().OnCompleted(() =>
        {
            Interlocked.Exchange(ref result.IsSuccessful, task.IsCompletedSuccessfully);
            Interlocked.Exchange(ref result.IsCancelled, task.IsCanceled);
            Interlocked.Exchange(ref result.IsFaulty, task.IsFaulted);

#if DEBUG
            InstanceLogger?.LogDebug("OnCompleted executed!");
#endif
        });

        return (nint)Unsafe.AsPointer(ref result);
    }

    public static async Task WaitFromHandle(this nint handle)
    {
        using SafeWaitHandle asyncSafeHandle = new SafeWaitHandle(GetWaitHandle(handle), false);
        EventWaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset)
        {
            SafeWaitHandle = asyncSafeHandle
        };

        await Task.Run(waitHandle.WaitOne);
        EnsureSuccessResult(handle);
    }

    private static unsafe nint GetWaitHandle(nint handle)
    {
        ComAsyncResult* asyncResult = (ComAsyncResult*)handle;
        return asyncResult->Handle;
    }

    private static unsafe void EnsureSuccessResult(nint handle)
    {
        ComAsyncResult* asyncResult = (ComAsyncResult*)handle;

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
