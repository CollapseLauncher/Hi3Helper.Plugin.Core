using Microsoft.Win32.SafeHandles;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Hi3Helper.Plugin.Core.Utility.Windows;

// ReSharper disable AccessToModifiedClosure
namespace Hi3Helper.Plugin.Core.Utility;

public static partial class ComAsyncExtension
{
    private static readonly Lock CurrentThreadLock = new();

    public static nint AsResult(this Task task)
        => ComAsyncResult.Alloc(CurrentThreadLock, task);

    public static nint AsResult<T>(this Task<T> task)
        where T : unmanaged
        => ComAsyncResult.Alloc(CurrentThreadLock, task);

    public static async Task WaitFromHandle(this nint handle)
    {
        SafeWaitHandle? asyncSafeHandle = null;
        EventWaitHandle? waitHandle = null;
        nint waitHandleP = nint.Zero;
        try
        {
            waitHandleP = ComAsyncResult.GetWaitHandle(handle);
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

            if (waitHandleP != nint.Zero)
            {
                PInvoke.CloseHandle(waitHandleP);
            }
        }
    }

    public static async Task<T> WaitFromHandle<T>(this nint handle)
        where T : unmanaged
    {
        SafeWaitHandle? asyncSafeHandle = null;
        EventWaitHandle? waitHandle = null;
        nint waitHandleP = nint.Zero;
        try
        {
            waitHandleP = ComAsyncResult.GetWaitHandle(handle);
            asyncSafeHandle = new SafeWaitHandle(waitHandleP, false);
            waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset)
            {
                SafeWaitHandle = asyncSafeHandle
            };

            await Task.Factory.StartNew(waitHandle.WaitOne);
            return EnsureSuccessResult<T>(handle);
        }
        finally
        {
            asyncSafeHandle?.Dispose();
            waitHandle?.Dispose();
            ComAsyncResult.DisposeHandle(handle);

            if (waitHandleP != nint.Zero)
            {
                PInvoke.CloseHandle(waitHandleP);
            }
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
                exception.SetExceptionStackTrace() = currentStackTrace.ToString();
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

    private static unsafe T EnsureSuccessResult<T>(nint handle)
        where T : unmanaged
    {
        EnsureSuccessResult(handle);
        return *(T*)((ComAsyncResult*)handle)->_resultP;
    }
}
