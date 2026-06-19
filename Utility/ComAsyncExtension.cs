using Hi3Helper.Plugin.Core.Utility.Windows;
using Microsoft.Win32.SafeHandles;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Hi3Helper.Plugin.Core.Utility;

/// <summary>
/// An extension contains tools to marshal plugin's <see cref="Task"/> to <see cref="ComAsyncResult"/> and vice versa.
/// </summary>
public static partial class ComAsyncExtension
{
    /// <summary>
    /// A delegate which used to pass an <see cref="Exception"/> to a callback.
    /// </summary>
    /// <param name="ex">An exception to pass into callback.</param>
    private delegate void SetExceptionDelegate(Exception ex);

    /// <summary>
    /// A thread-lock used by the plugin to perform marshalling.
    /// </summary>
    private static readonly Lock CurrentThreadLock = new();

    /// <summary>
    /// Marshal a <see cref="Task"/> to <see cref="ComAsyncResult"/> struct (as a pointer).
    /// </summary>
    /// <param name="task">A <see cref="Task"/> to marshal into.</param>
    /// <returns>A pointer into <see cref="ComAsyncResult"/> struct.</returns>
    public static nint AsResult(this Task task)
        => ComAsyncResult.Alloc(CurrentThreadLock, task);

    /// <summary>
    /// Marshal a <see cref="Task{T}"/> to <see cref="ComAsyncResult"/> struct (as a pointer).
    /// </summary>
    /// <param name="task">A <see cref="Task{T}"/> to marshal into.</param>
    /// <returns>A pointer into <see cref="ComAsyncResult"/> struct.</returns>
    public static nint AsResult<T>(this Task<T> task)
        where T : unmanaged
        => ComAsyncResult.Alloc(CurrentThreadLock, task);

    /// <summary>
    /// Re-marshal a pointer of <see cref="ComAsyncResult"/> struct into a managed <see cref="Task"/>.
    /// </summary>
    /// <param name="resultP">A pointer of the <see cref="ComAsyncResult"/> struct.</param>
    /// <returns>An awaitable managed <see cref="Task"/>.</returns>
    public static Task AsTask(this nint resultP)
        => new RegisteredWaitHandleContext(resultP).Task;

    /// <summary>
    /// Re-marshal a pointer of <see cref="ComAsyncResult"/> struct into a managed <see cref="Task"/>.
    /// </summary>
    /// <param name="resultP">A pointer of the <see cref="ComAsyncResult"/> struct.</param>
    /// <returns>An awaitable managed <see cref="Task"/>.</returns>
    public static Task<T> AsTask<T>(this nint resultP)
        where T : unmanaged => (Task<T>)new RegisteredWaitHandleContext<T>(resultP).Task;

    /// <summary>
    /// A wait handle context to registers and returns a result once async operation is completed.
    /// </summary>
    private class RegisteredWaitHandleContext
    {
        protected nint                 AsyncResultP         { get; }
        private   nint                 UnmanagedWaitHandleP { get; }
        private   SafeWaitHandle       SafeWaitHandle       { get; }
        private   WaitHandle           WaitHandle           { get; }
        private   RegisteredWaitHandle RegisteredWaitHandle { get; }

        private TaskCompletionSource TaskSourceResult { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public virtual Task Task => TaskSourceResult.Task;

        public RegisteredWaitHandleContext(nint asyncResultP)
        {
            AsyncResultP         = asyncResultP;
            UnmanagedWaitHandleP = ComAsyncResult.GetWaitHandle(asyncResultP);

            SafeWaitHandle = new SafeWaitHandle(UnmanagedWaitHandleP, false);
            WaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset)
            {
                SafeWaitHandle = SafeWaitHandle
            };
            RegisteredWaitHandle = ThreadPool.RegisterWaitForSingleObject(WaitHandle, SetResultCallback, this, -1, true);
        }

        private static void SetResultCallback(object? state, bool isTimedOut)
        {
            RegisteredWaitHandleContext ctx = (RegisteredWaitHandleContext)state!;
            ctx.SetResultCallbackCore();
        }

        protected virtual unsafe void SetResultCallbackCore()
        {
            SetResult<TaskCompletionSource, nint>(AsyncResultP.AsPointer<ComAsyncResult>(), TaskSourceResult);
            CloseHandles();
        }

        /// <summary>
        /// Closes wait handles and free <see cref="ComAsyncResult"/> struct.
        /// </summary>
        protected void CloseHandles()
        {
            SafeWaitHandle.Dispose();
            WaitHandle.Dispose();
            ComAsyncResult.FreeResult(AsyncResultP);

            if (UnmanagedWaitHandleP != nint.Zero)
            {
                PInvoke.CloseHandle(UnmanagedWaitHandleP);
            }

            RegisteredWaitHandle.Unregister(null);
        }

        /// <summary>
        /// Sets result of the async operation based on its return type into the <see cref="TaskCompletionSource"/>.
        /// </summary>
        /// <typeparam name="TSource">A type of <see cref="TaskCompletionSource"/> or <see cref="TaskCompletionSource{TResult}"/></typeparam>
        /// <typeparam name="T">A type of the return value from <see cref="ComAsyncResult"/>.</typeparam>
        /// <param name="asyncResult">A pointer to the <see cref="ComAsyncResult"/> struct.</param>
        /// <param name="tcs">An instance of <see cref="TaskCompletionSource"/> or <see cref="TaskCompletionSource{TResult}"/> to set the exception or result to.</param>
        /// <exception cref="InvalidCastException">Whether the <typeparamref name="TSource"/> is not either <see cref="TaskCompletionSource"/> or <see cref="TaskCompletionSource{TResult}"/></exception>
        protected static unsafe void SetResult<TSource, T>(ComAsyncResult* asyncResult, TSource tcs)
            where T : unmanaged
        {
            // Try check the TCS kind.
            switch (tcs)
            {
                case TaskCompletionSource<T> asTcsRes:
                    SetResultInner(asyncResult, () => asTcsRes.SetResult(*(T*)asyncResult->_resultP), asTcsRes.SetException, asTcsRes.SetCanceled);
                    return;
                case TaskCompletionSource asTcs:
                    SetResultInner(asyncResult, asTcs.SetResult, asTcs.SetException, asTcs.SetCanceled);
                    return;
            }

            // Bada bing, bada bong. Throw the cast exception.
            throw new InvalidCastException($"Cannot cast {nameof(TSource)} to either {nameof(TaskCompletionSource<>)} or {nameof(TaskCompletionSource)}");
        }

        private static unsafe void SetResultInner(ComAsyncResult* asyncResult, Action actionSetRes, SetExceptionDelegate actionSetExc, Action actionSetCancel)
        {
            // IsCancelled is set without IsFaulty being set. So, try check if the exception is not empty. If yes, then set the default exception.
            if (asyncResult->IsCancelled)
            {
                if (asyncResult->ExceptionMemory.IsEmpty)
                {
                    actionSetExc(new TaskCanceledException());
                    return;
                }

                // Otherwise, set the exception from ExceptionMemory
                SetException(asyncResult, actionSetExc, actionSetCancel);
                return;
            }

            // If it isn't faulty, invoke result.
            if (!asyncResult->IsFaulty)
            {
                actionSetRes();
                return;
            }

            // Otherwise, invoke the exception.
            SetException(asyncResult, actionSetExc, actionSetCancel);
        }

        /// <summary>
        /// Set the exception provided by the <see cref="ComAsyncResult.ExceptionMemory"/>.
        /// </summary>
        /// <param name="asyncResult">A pointer to the <see cref="ComAsyncResult"/> struct.</param>
        /// <param name="actionSetExc">A callback to set the exception into the <see cref="TaskCompletionSource"/></param>
        /// <param name="actionSetCancel">A callback to set the canceled status into the <see cref="TaskCompletionSource"/></param>
        /// <remarks>
        /// If the buffer provided by the <see cref="ComAsyncResult.ExceptionMemory"/> is empty, then a generic <see cref="COMException"/> will be set.
        /// </remarks>
        private static unsafe void SetException(ComAsyncResult* asyncResult, SetExceptionDelegate actionSetExc, Action actionSetCancel)
        {
            StackTrace currentStackTrace = new(true);
            Exception? exception         = ComAsyncException.GetExceptionFromHandle(asyncResult->ExceptionMemory);

#if DEBUG
            Debug.Assert(exception != null, $"Exception shouldn't be null! Result address is: 0x{(nint)asyncResult:x8}");
#endif

            // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
            if (exception == null)
            {
                actionSetExc(new COMException());
                return;
            }

            exception.SetExceptionRemoteStackTrace() += Environment.NewLine;
            exception.SetExceptionStackTrace()       =  currentStackTrace.ToString();
            actionSetExc(exception);

            if (exception is OperationCanceledException)
            {
                actionSetCancel();
            }
        }
    }
    
    /// <inheritdoc/>
    private class RegisteredWaitHandleContext<T>(nint asyncResultP) : RegisteredWaitHandleContext(asyncResultP)
        where T : unmanaged
    {
        private TaskCompletionSource<T> TaskSourceResult { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public override Task Task => TaskSourceResult.Task;

        protected override unsafe void SetResultCallbackCore()
        {
            SetResult<TaskCompletionSource<T>, T>(AsyncResultP.AsPointer<ComAsyncResult>(), TaskSourceResult);
            CloseHandles();
        }
    }
}
