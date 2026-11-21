using Hi3Helper.Plugin.Core.Utility.Windows;
using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
// ReSharper disable CommentTypo
#pragma warning disable CS8500 // This takes the address of, gets the size of, or declares a pointer to a managed type

namespace Hi3Helper.Plugin.Core.Utility;

public static partial class ComAsyncExtension
{
    /// <summary>
    /// A wait handle context to registers and returns a result once async operation is completed.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private sealed unsafe class ValueTaskRegisteredWaitHandleContext : IValueTaskSource
    {
        private readonly nint                  _asyncResultP;
        private readonly nint                  _unmanagedWaitHandleP;
        private readonly EventWaitHandle?      _event;
        private readonly RegisteredWaitHandle? _registeredWaitHandle;

        private ManualResetValueTaskSourceCore<bool> _core;

        private ValueTaskRegisteredWaitHandleContext(nint asyncResultP)
        {
            _asyncResultP                        = asyncResultP;
            _unmanagedWaitHandleP                = ComAsyncResult.GetWaitHandle(asyncResultP);
            _core.RunContinuationsAsynchronously = true;

            // Second-check: Check whether the task is returning after split seconds and eventually set the result early without allocating wait handles.
            if (ComAsyncResult.GetTaskCompletionState(asyncResultP,
                                                      out _,
                                                      out _,
                                                      out _))
            {
                SetResultCallbackCore();
                return;
            }

            // Wait approach.
            _event = new EventWaitHandle(false, EventResetMode.ManualReset)
            {
                SafeWaitHandle = new SafeWaitHandle(_unmanagedWaitHandleP, false)
            };
            _registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(_event, SetResultCallback, this, -1, true);
        }

        /// <summary>
        /// Closes wait handles and free <see cref="ComAsyncResult"/> struct.
        /// </summary>
        private void CloseHandles()
        {
            _event?.Dispose();
            ComAsyncResult.FreeResult(_asyncResultP);

            if (_unmanagedWaitHandleP != nint.Zero)
            {
                PInvoke.CloseHandle(_unmanagedWaitHandleP);
            }

            _registeredWaitHandle?.Unregister(null);
        }

        private static unsafe void SetResultCallback(object? state, bool isTimedOut)
        {
            ValueTaskRegisteredWaitHandleContext stateP = (ValueTaskRegisteredWaitHandleContext)state!;
            stateP.SetResultCallbackCore();
        }

        private unsafe void SetResultCallbackCore()
        {
            var             coreP   = (ManualResetValueTaskSourceCore<bool>*)Unsafe.AsPointer(ref _core);
            ComAsyncResult* resultP = _asyncResultP.AsPointer<ComAsyncResult>();
            TaskRegisteredWaitHandleContext.SetResultInner(resultP, () => coreP->SetResult(true), coreP->SetException, null);
            CloseHandles();
        }

        public static ValueTask CreateValueTask(nint asyncResultP)
        {
            if (ComAsyncResult.GetTaskCompletionState(asyncResultP,
                                                      out _,
                                                      out _,
                                                      out _))
            {
                ComAsyncResult* resultP = asyncResultP.AsPointer<ComAsyncResult>();
                nint waitHandle = ComAsyncResult.GetWaitHandle(asyncResultP);

                try
                {
                    ValueTask taskResult = default;
                    TaskRegisteredWaitHandleContext.SetResultInner(resultP,
                                                                   () => taskResult = ValueTask.CompletedTask,
                                                                   ex => taskResult = ValueTask.FromException(ex),
                                                                   null);
                    return taskResult;
                }
                finally
                {
                    ComAsyncResult.FreeResult(asyncResultP);
                    if (waitHandle != nint.Zero) PInvoke.CloseHandle(waitHandle);
                }
            }

            ValueTaskRegisteredWaitHandleContext context = new(asyncResultP);
            return new ValueTask(context, context._core.Version);
        }

        public void GetResult(short token)
            => _core.GetResult(token);

        public ValueTaskSourceStatus GetStatus(short token)
            => _core.GetStatus(token);

        public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
            => _core.OnCompleted(continuation, state, token, flags);
    }

    /// <summary>
    /// A wait handle context to registers and returns a result once async operation is completed.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private sealed unsafe class ValueTaskRegisteredWaitHandleContext<T> : IValueTaskSource<T>
    {
        private readonly nint                  _asyncResultP;
        private readonly nint                  _unmanagedWaitHandleP;
        private readonly EventWaitHandle?      _event;
        private readonly RegisteredWaitHandle? _registeredWaitHandle;

        private ManualResetValueTaskSourceCore<T> _core;

        private ValueTaskRegisteredWaitHandleContext(nint asyncResultP)
        {
            _asyncResultP                        = asyncResultP;
            _unmanagedWaitHandleP                = ComAsyncResult.GetWaitHandle(asyncResultP);
            _core.RunContinuationsAsynchronously = true;

            // Second-check: Check whether the task is returning after split seconds and eventually set the result early without allocating wait handles.
            if (ComAsyncResult.GetTaskCompletionState(asyncResultP,
                                                      out _,
                                                      out _,
                                                      out _))
            {
                SetResultCallbackCore();
                return;
            }

            // Wait approach.
            _event = new EventWaitHandle(false, EventResetMode.ManualReset)
            {
                SafeWaitHandle = new SafeWaitHandle(_unmanagedWaitHandleP, false)
            };
            _registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(_event, SetResultCallback, this, -1, true);
        }

        /// <summary>
        /// Closes wait handles and free <see cref="ComAsyncResult"/> struct.
        /// </summary>
        private void CloseHandles()
        {
            _event?.Dispose();
            ComAsyncResult.FreeResult(_asyncResultP);

            if (_unmanagedWaitHandleP != nint.Zero)
            {
                PInvoke.CloseHandle(_unmanagedWaitHandleP);
            }

            _registeredWaitHandle?.Unregister(null);
        }

        private static unsafe void SetResultCallback(object? state, bool isTimedOut)
        {
            var stateP = (ValueTaskRegisteredWaitHandleContext<T>)state!;
            stateP.SetResultCallbackCore();
        }

        private unsafe void SetResultCallbackCore()
        {
            var             coreP   = (ManualResetValueTaskSourceCore<T>*)Unsafe.AsPointer(ref _core);
            ComAsyncResult* resultP = _asyncResultP.AsPointer<ComAsyncResult>();
            TaskRegisteredWaitHandleContext.SetResultInner(resultP, () => coreP->SetResult(*(T*)resultP->_resultP), coreP->SetException, null);
            CloseHandles();
        }

        public static ValueTask<T> CreateValueTask(nint asyncResultP)
        {
            if (ComAsyncResult.GetTaskCompletionState(asyncResultP,
                                                      out _,
                                                      out _,
                                                      out _))
            {
                ComAsyncResult* resultP    = asyncResultP.AsPointer<ComAsyncResult>();
                nint            waitHandle = ComAsyncResult.GetWaitHandle(asyncResultP);

                try
                {
                    ValueTask<T> taskResult = default;
                    TaskRegisteredWaitHandleContext.SetResultInner(resultP,
                                                                   () => taskResult = ValueTask.FromResult(*(T*)resultP->_resultP),
                                                                   ex => taskResult = ValueTask.FromException<T>(ex),
                                                                   null);
                    return taskResult;
                }
                finally
                {
                    ComAsyncResult.FreeResult(asyncResultP);
                    if (waitHandle != nint.Zero) PInvoke.CloseHandle(waitHandle);
                }
            }

            ValueTaskRegisteredWaitHandleContext<T> context = new(asyncResultP);
            return new ValueTask<T>(context, context._core.Version);
        }

        public T GetResult(short token)
            => _core.GetResult(token);

        public ValueTaskSourceStatus GetStatus(short token)
            => _core.GetStatus(token);

        public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
            => _core.OnCompleted(continuation, state, token, flags);
    }
}
