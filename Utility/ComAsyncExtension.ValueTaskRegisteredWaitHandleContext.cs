using Hi3Helper.Plugin.Core.Utility.Windows;
using Microsoft.Win32.SafeHandles;
using System;
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
    private unsafe struct ValueTaskRegisteredWaitHandleContext : IValueTaskSource
    {
        private readonly nint                  _asyncResultP;
        private readonly nint                  _unmanagedWaitHandleP;
        private readonly EventWaitHandle?      _event;
        private readonly RegisteredWaitHandle? _registeredWaitHandle;

        private readonly ManualResetValueTaskSourceCore<bool>  _core;
        private readonly ManualResetValueTaskSourceCore<bool>* _coreP;
        private          GCHandle                              _coreGcHandle;

        private readonly ValueTaskRegisteredWaitHandleContext* _thisP;
        private          GCHandle                              _thisGcHandle;

        private ValueTaskRegisteredWaitHandleContext(nint asyncResultP)
        {
            _asyncResultP                        = asyncResultP;
            _unmanagedWaitHandleP                = ComAsyncResult.GetWaitHandle(asyncResultP);
            _core.RunContinuationsAsynchronously = true;

            // We choose unsafe way. Pin the VTSC and this struct to the GCHandle and get the pointer to be used for callbacks.
            _thisGcHandle = GCHandle.Alloc(this,  GCHandleType.Pinned);
            _coreGcHandle = GCHandle.Alloc(_core, GCHandleType.Pinned);
            _thisP        = (ValueTaskRegisteredWaitHandleContext*)_thisGcHandle.AddrOfPinnedObject();
            _coreP        = (ManualResetValueTaskSourceCore<bool>*)_coreGcHandle.AddrOfPinnedObject();

            // Check whether the task is returning earlier and set the result early without allocating wait handles.
            if (ComAsyncResult.GetTaskCompletionState(asyncResultP,
                                                      out _,
                                                      out _,
                                                      out _))
            {
                SetResultCallbackCore();
                return;
            }

            _event = new EventWaitHandle(false, EventResetMode.ManualReset)
            {
                SafeWaitHandle = new SafeWaitHandle(_unmanagedWaitHandleP, false)
            };
            _registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(_event, SetResultCallback, (nint)_thisP, -1, true);
        }

        /// <summary>
        /// Closes wait handles and free <see cref="ComAsyncResult"/> struct.
        /// </summary>
        private readonly void CloseHandles()
        {
            _thisP->_coreGcHandle.Free();
            _thisP->_thisGcHandle.Free();

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
            nint ctx = (nint)state!;
            ((ValueTaskRegisteredWaitHandleContext*)ctx)->SetResultCallbackCore();
        }

        private unsafe void SetResultCallbackCore()
        {
            TaskRegisteredWaitHandleContext.SetResult<ManualResetValueTaskSourceCore<bool>, nint>(_asyncResultP.AsPointer<ComAsyncResult>(), in _core);
            CloseHandles();
        }

        public static ValueTask CreateValueTask(nint asyncResultP)
        {
            ValueTaskRegisteredWaitHandleContext context = new(asyncResultP);
            return new ValueTask(context, context._core.Version);
        }

        public void GetResult(short token)
            => _coreP->GetResult(token);

        public ValueTaskSourceStatus GetStatus(short token)
            => _coreP->GetStatus(token);

        public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
            => _coreP->OnCompleted(continuation, state, token, flags);
    }


    /// <summary>
    /// A wait handle context to registers and returns a result once async operation is completed.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private unsafe struct ValueTaskRegisteredWaitHandleContext<T> : IValueTaskSource<T>
    {
        private readonly nint                  _asyncResultP;
        private readonly nint                  _unmanagedWaitHandleP;
        private readonly EventWaitHandle?      _event;
        private readonly RegisteredWaitHandle? _registeredWaitHandle;

        private readonly ManualResetValueTaskSourceCore<T>  _core;
        private readonly ManualResetValueTaskSourceCore<T>* _coreP;
        private          GCHandle                           _coreGcHandle;

        private readonly ValueTaskRegisteredWaitHandleContext<T>* _thisP;
        private          GCHandle                                 _thisGcHandle;

        private ValueTaskRegisteredWaitHandleContext(nint asyncResultP)
        {
            _asyncResultP                        = asyncResultP;
            _unmanagedWaitHandleP                = ComAsyncResult.GetWaitHandle(asyncResultP);
            _core.RunContinuationsAsynchronously = true;

            // We choose unsafe way. Pin the VTSC and this struct to the GCHandle and get the pointer to be used for callbacks.
            _thisGcHandle = GCHandle.Alloc(this,  GCHandleType.Pinned);
            _coreGcHandle = GCHandle.Alloc(_core, GCHandleType.Pinned);
            _thisP        = (ValueTaskRegisteredWaitHandleContext<T>*)_thisGcHandle.AddrOfPinnedObject();
            _coreP        = (ManualResetValueTaskSourceCore<T>*)_coreGcHandle.AddrOfPinnedObject();

            // Check whether the task is returning earlier and set the result early without allocating wait handles.
            if (ComAsyncResult.GetTaskCompletionState(asyncResultP,
                                                      out _,
                                                      out _,
                                                      out _))
            {
                SetResultCallbackCore();
                return;
            }

            _event = new EventWaitHandle(false, EventResetMode.ManualReset)
            {
                SafeWaitHandle = new SafeWaitHandle(_unmanagedWaitHandleP, false)
            };
            _registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(_event, SetResultCallback, (nint)_thisP, -1, true);
        }

        /// <summary>
        /// Closes wait handles and free <see cref="ComAsyncResult"/> struct.
        /// </summary>
        private readonly void CloseHandles()
        {
            _thisP->_coreGcHandle.Free();
            _thisP->_thisGcHandle.Free();

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
            nint ctx = (nint)state!;
            ((ValueTaskRegisteredWaitHandleContext<T>*)ctx)->SetResultCallbackCore();
        }

        private unsafe void SetResultCallbackCore()
        {
            TaskRegisteredWaitHandleContext.SetResult<ManualResetValueTaskSourceCore<T>, nint>(_asyncResultP.AsPointer<ComAsyncResult>(), in _core);
            CloseHandles();
        }

        public static ValueTask<T> CreateValueTask(nint asyncResultP)
        {
            ValueTaskRegisteredWaitHandleContext<T> context = new(asyncResultP);
            return new ValueTask<T>(context, context._core.Version);
        }

        public T GetResult(short token)
            => _coreP->GetResult(token);

        public ValueTaskSourceStatus GetStatus(short token)
            => _coreP->GetStatus(token);

        public void OnCompleted(Action<object?> continuation, object? state, short token, ValueTaskSourceOnCompletedFlags flags)
            => _coreP->OnCompleted(continuation, state, token, flags);
    }
}
