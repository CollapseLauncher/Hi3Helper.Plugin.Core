using Hi3Helper.Plugin.Core.Utility.Windows;
using Microsoft.Win32.SafeHandles;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace Hi3Helper.Plugin.Core.Utility;

public static partial class ComAsyncExtension
{
    /// <summary>
    /// A wait handle context to registers and returns a result once async operation is completed.
    /// </summary>
    private class TaskRegisteredWaitHandleContext
    {
        protected nint                  AsyncResultP         { get; }
        private   nint                  UnmanagedWaitHandleP { get; }
        private   EventWaitHandle?      WaitHandle           { get; }
        private   RegisteredWaitHandle? RegisteredWaitHandle { get; }

        private TaskCompletionSource _taskSourceResult = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public virtual Task Task => _taskSourceResult.Task;

        public TaskRegisteredWaitHandleContext(nint asyncResultP)
        {
            AsyncResultP         = asyncResultP;
            UnmanagedWaitHandleP = ComAsyncResult.GetWaitHandle(asyncResultP);

            // Check whether the task is returning earlier and set the result early without allocating wait handles.
            if (ComAsyncResult.GetTaskCompletionState(asyncResultP,
                                                      out _,
                                                      out _,
                                                      out _))
            {
                SetResultCallback(this, false);
                return;
            }

            WaitHandle = new EventWaitHandle(false, EventResetMode.ManualReset)
            {
                SafeWaitHandle = new SafeWaitHandle(UnmanagedWaitHandleP, false)
            };
            RegisteredWaitHandle = ThreadPool.RegisterWaitForSingleObject(WaitHandle, SetResultCallback, this, -1, true);
        }

        private static unsafe void SetResultCallback(object? state, bool isTimedOut)
        {
            TaskRegisteredWaitHandleContext ctx = (TaskRegisteredWaitHandleContext)state!;
            ctx.SetResultCallbackCore();
        }

        protected virtual unsafe void SetResultCallbackCore()
        {
            SetResult<TaskCompletionSource, nint>(AsyncResultP.AsPointer<ComAsyncResult>(), ref _taskSourceResult);
            CloseHandles();
        }

        /// <summary>
        /// Closes wait handles and free <see cref="ComAsyncResult"/> struct.
        /// </summary>
        protected void CloseHandles()
        {
            WaitHandle?.Dispose();
            ComAsyncResult.FreeResult(AsyncResultP);

            if (UnmanagedWaitHandleP != nint.Zero)
            {
                PInvoke.CloseHandle(UnmanagedWaitHandleP);
            }

            RegisteredWaitHandle?.Unregister(null);
        }

        /// <summary>
        /// Sets result of the async operation based on its return type into the <see cref="TaskCompletionSource"/>.
        /// </summary>
        /// <typeparam name="TSource">A type of <see cref="TaskCompletionSource"/> or <see cref="TaskCompletionSource{TResult}"/></typeparam>
        /// <typeparam name="T">A type of the return value from <see cref="ComAsyncResult"/>.</typeparam>
        /// <param name="asyncResult">A pointer to the <see cref="ComAsyncResult"/> struct.</param>
        /// <param name="tcs">An instance of <see cref="TaskCompletionSource"/> or <see cref="TaskCompletionSource{TResult}"/> to set the exception or result to.</param>
        /// <exception cref="InvalidCastException">Whether the <typeparamref name="TSource"/> is not either <see cref="TaskCompletionSource"/> or <see cref="TaskCompletionSource{TResult}"/></exception>
        internal static unsafe void SetResult<TSource, T>(ComAsyncResult* asyncResult, ref readonly TSource tcs)
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
                case ManualResetValueTaskSourceCore<T>:
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                    var asVTcsRes = (ManualResetValueTaskSourceCore<T>*)Unsafe.AsPointer(in tcs);
 #pragma warning restore CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type
                    SetResultInner(asyncResult, () => asVTcsRes->SetResult(*(T*)asyncResult->_resultP), asVTcsRes->SetException, null);
                    return;
            }

            // Bada bing, bada bong. Throw the cast exception.
            throw new InvalidCastException($"Cannot cast {nameof(TSource)} to either {nameof(TaskCompletionSource<>)} or {nameof(TaskCompletionSource)}");
        }

        private static unsafe void SetResultInner(ComAsyncResult* asyncResult, Action actionSetRes, SetExceptionDelegate actionSetExc, Action? actionSetCancel)
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
        private static unsafe void SetException(ComAsyncResult* asyncResult, SetExceptionDelegate actionSetExc, Action? actionSetCancel)
        {
            StackTrace currentStackTrace = new(true);
            Exception? exception = ComAsyncException.GetExceptionFromHandle(asyncResult->ExceptionMemory);

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

            if (exception is not OperationCanceledException)
            {
                return;
            }

            if (actionSetCancel == null)
            {
                actionSetExc(exception);
                return;
            }

            actionSetCancel();
        }
    }

    /// <inheritdoc/>
    private class TaskRegisteredWaitHandleContext<T>(nint asyncResultP) : TaskRegisteredWaitHandleContext(asyncResultP)
        where T : unmanaged
    {
        private TaskCompletionSource<T> _taskSourceResult = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public override Task Task => _taskSourceResult.Task;

        protected override unsafe void SetResultCallbackCore()
        {
            SetResult<TaskCompletionSource<T>, T>(AsyncResultP.AsPointer<ComAsyncResult>(), ref _taskSourceResult);
            CloseHandles();
        }
    }
}