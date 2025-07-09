using Hi3Helper.Plugin.Core.Utility.Windows;
using Microsoft.Win32.SafeHandles;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable AccessToModifiedClosure
namespace Hi3Helper.Plugin.Core.Utility;

public static partial class ComAsyncExtension
{
    private delegate void SetExceptionDelegate(Exception ex);

    private static readonly Lock CurrentThreadLock = new();

    public static nint AsResult(this Task task)
        => ComAsyncResult.Alloc(CurrentThreadLock, task);

    public static nint AsResult<T>(this Task<T> task)
        where T : unmanaged
        => ComAsyncResult.Alloc(CurrentThreadLock, task);

    public static Task WaitFromHandle(this nint handle)
    {
        nint waitHandleP = ComAsyncResult.GetWaitHandle(handle);

        RegisteredWaitHandle? registeredWaitHandle = null;
        TaskCompletionSource  tcs                  = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        SafeWaitHandle        safeHandle           = new SafeWaitHandle(waitHandleP, false);

        WaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset)
        {
            SafeWaitHandle = safeHandle
        };

        registeredWaitHandle = ThreadPool
           .RegisterWaitForSingleObject(waitHandle,
                                        Impl,
                                        null,
                                        -1,
                                        true);

        return tcs.Task;

        void Impl(object? state, bool isTimedOut)
        {
            ref ComAsyncResult asyncResult = ref handle.AsRef<ComAsyncResult>();
            SetResult<TaskCompletionSource, nint>(ref asyncResult, tcs);

            safeHandle.Dispose();
            waitHandle.Dispose();
            ComAsyncResult.DisposeHandle(handle);

            if (waitHandleP != nint.Zero)
            {
                PInvoke.CloseHandle(waitHandleP);
            }

            registeredWaitHandle!.Unregister(null);
        }
    }

    public static Task<T> WaitFromHandle<T>(this nint handle)
        where T : unmanaged
    {
        nint waitHandleP = ComAsyncResult.GetWaitHandle(handle);

        RegisteredWaitHandle?   registeredWaitHandle = null;
        TaskCompletionSource<T> taskSource           = new(TaskCreationOptions.RunContinuationsAsynchronously);
        SafeWaitHandle          safeHandle           = new SafeWaitHandle(waitHandleP, false);

        WaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset)
        {
            SafeWaitHandle = safeHandle
        };

        registeredWaitHandle = ThreadPool
            .RegisterWaitForSingleObject(waitHandle,
                Impl,
                null,
                -1,
                true);

        return taskSource.Task;

        void Impl(object? state, bool isTimedOut)
        {
            ref ComAsyncResult asyncResult = ref handle.AsRef<ComAsyncResult>();
            SetResult<TaskCompletionSource<T>, T>(ref asyncResult, taskSource);

            safeHandle.Dispose();
            waitHandle.Dispose();
            ComAsyncResult.DisposeHandle(handle);

            if (waitHandleP != nint.Zero)
            {
                PInvoke.CloseHandle(waitHandleP);
            }

            registeredWaitHandle!.Unregister(null);
        }
    }

    private static unsafe void SetResult<TSource, T>(ref ComAsyncResult asyncResult, TSource tcs)
        where T : unmanaged
    {
        switch (tcs)
        {
            case TaskCompletionSource<T> asTcsRes:
                ComAsyncResult* ptrUnsafe = asyncResult.AsPointer();
                SetResultInner(in asyncResult, () => asTcsRes.SetResult(*(T*)ptrUnsafe->_resultP), asTcsRes.SetException, asTcsRes.SetCanceled);
                break;
            case TaskCompletionSource asTcs:
                SetResultInner(in asyncResult, asTcs.SetResult, asTcs.SetException, asTcs.SetCanceled);
                break;
        }

        return;

        static void SetResultInner(in ComAsyncResult asyncResultIn, Action actionSetRes, SetExceptionDelegate actionSetExc, Action actionSetCancel)
        {
            if (asyncResultIn is { IsCancelled: true, IsFaulty: false })
            {
                actionSetExc(new TaskCanceledException());
                return;
            }

            if (!asyncResultIn.IsFaulty)
            {
                actionSetRes();
                return;
            }

            SetException(in asyncResultIn, actionSetExc, actionSetCancel);
        }
    }

    private static void SetException(in ComAsyncResult asyncResult, SetExceptionDelegate setDelegate, Action setCancelled)
    {
        StackTrace currentStackTrace = new(true);
        Exception? exception = ComAsyncException.GetExceptionFromHandle(asyncResult.ExceptionMemory);

        if (exception == null)
        {
            setDelegate(new COMException());
            return;
        }

        exception.SetExceptionRemoteStackTrace() += Environment.NewLine;
        exception.SetExceptionStackTrace() = currentStackTrace.ToString();
        setDelegate(exception);

        if (exception is OperationCanceledException)
        {
            setCancelled();
        }
    }
}
