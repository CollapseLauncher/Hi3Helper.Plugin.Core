using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;

namespace Hi3Helper.Plugin.Core.Utility;

/// <summary>
/// Provides a helper for using Speed Limiter Service.
/// </summary>
/// <remarks>
/// In order to use the service, make sure the main application has registered the service's callback using RegisterSpeedThrottlerService export API.
/// Once the service is successfully registered, you can call <see cref="AddBytesOrWaitAsync"/> method to pass the read bytes to the service and then throttle the speed for you.
/// <br/><br/>
/// Usage example on your code:
/// <code>
/// async Task ReadDataAsync(Stream inputStream, Stream outputStream, CancellationToken token)
/// {
///     byte[] buffer = new byte[8192];
///     int read;
///
///     nint context = SpeedLimiterService.CreateServiceContext();
///     while ((read = await inputStream.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
///     {
///         // Do anything...
///         ...
///
///         // Pass the read bytes to the speed limiter service, and wait if necessary until the service allows more bytes to be processed.
///         await SpeedLimiterService.AddBytesOrWaitAsync(context, read, token);
///     }
/// }
/// </code>
/// </remarks>
public static class SpeedLimiterService
{
    internal static unsafe delegate* unmanaged[Cdecl]<nint, long, nint, out nint, int> AddBytesOrWaitAsyncCallback =
        null;

    internal static unsafe delegate* unmanaged[Cdecl]<ref long, ref long, void> GetSharedThrottleBytesCallback =
        null;

    /// <summary>
    /// Creates a context to be used for the speed limiter service. This context can be used into multiple instances or threads of your downloader.
    /// </summary>
    /// <returns></returns>
    public static unsafe nint CreateServiceContext()
    {
        ThrottleServiceContext* alloc = Mem.Alloc<ThrottleServiceContext>();
        alloc->AvailableTokens = 0;
        alloc->LastTimestamp   = Environment.TickCount64;

        if (GetSharedThrottleBytesCallback == null)
            return (nint)alloc;

        try
        {
            long bytesPerSecond = 0;
            long burstBytes     = 0;

            GetSharedThrottleBytesCallback(ref bytesPerSecond, ref burstBytes);

            if (bytesPerSecond < burstBytes)
            {
                bytesPerSecond = burstBytes;
            }

            alloc->AvailableTokens = bytesPerSecond;
        }
        catch
        {
            // ignored
        }

        return (nint)alloc;
    }

    /// <summary>
    /// Free the speed limiter service context.
    /// </summary>
    /// <param name="context"></param>
    public static void FreeServiceContext(nint context)
        => context.Free();

    /// <summary>
    /// Adds-up counter of the consumed bytes into the service, and await (throttle) if the target speed is already reached.<br/>
    /// If the service is not registered or the callback is not set, this method will simply return immediately without awaiting.
    /// </summary>
    /// <param name="context">The pointer of the service context.</param>
    /// <param name="readBytes">How many bytes consumed on current operation.</param>
    /// <param name="token">Cancellation token for the async operation.</param>
    [SkipLocalsInit]
    public static unsafe ValueTask AddBytesOrWaitAsync(
        nint              context,
        long              readBytes,
        CancellationToken token = default)
    {
        if (context == nint.Zero || AddBytesOrWaitAsyncCallback == null)
            return ValueTask.CompletedTask;

        int hr = AddBytesOrWaitAsyncCallback(context,
                                             readBytes,
                                             nint.Zero,
                                             out nint completionHandle);

        if (Marshal.GetExceptionForHR(hr) is { } ex)
            return ValueTask.FromException(ex);

        NativeThrottleOperation op = new();
        op.Initialize(completionHandle, token);

        return op.AsValueTask();
    }

    [StructLayout(LayoutKind.Sequential, Pack = 8)] // Pack to 8 bytes to ensure aligning
    private struct ThrottleServiceContext
    {
        public long AvailableTokens;
        public long LastTimestamp;
    }

    private sealed class NativeThrottleOperation : IValueTaskSource
    {
        private ManualResetValueTaskSourceCore<bool> _core = new()
        {
            RunContinuationsAsynchronously = true
        };

        private int _isCompleted;
        private EventWaitHandle? _completionWait;
        private SafeWaitHandle? _completionSafe;
        private RegisteredWaitHandle? _registeredWait;
        private CancellationTokenRegistration _ctr;

        public ValueTask AsValueTask()
            => new(this, _core.Version);

        public void Initialize(
            nint completionHandle,
            CancellationToken token)
        {
            _completionSafe = new SafeWaitHandle(completionHandle, true);
            _completionWait = new EventWaitHandle(false, EventResetMode.ManualReset)
            {
                SafeWaitHandle = _completionSafe
            };

            _registeredWait =
                ThreadPool.RegisterWaitForSingleObject(_completionWait,
                                                       OnWaitSingleCompleted,
                                                       this,
                                                       -1,
                                                       true);

            if (token.CanBeCanceled)
            {
                _ctr = token.Register(OnCancellationRequested, this);
            }
        }

        private static void OnWaitSingleCompleted(object? state, bool isTimedOut)
        {
            NativeThrottleOperation op = (NativeThrottleOperation)state!;
            op.Complete();
        }

        private static void OnCancellationRequested(object? state)
        {
            NativeThrottleOperation op = (NativeThrottleOperation)state!;
            op.Cancel();
        }

        private void Complete()
        {
            if (Interlocked.Exchange(ref _isCompleted, 1) == 1)
            {
                return;
            }

            Cleanup();
            _core.SetResult(true);
        }

        private void Cancel()
        {
            if (Interlocked.Exchange(ref _isCompleted, 1) == 1)
            {
                return;
            }

            Cleanup();
            _core.SetException(new OperationCanceledException());
        }

        private void Cleanup()
        {
            _registeredWait?.Unregister(null);
            _registeredWait = null;

            _ctr.Dispose();

            _completionWait?.Dispose();
            _completionWait = null;
            _completionSafe = null;
        }

        public void GetResult(short token)
            => _core.GetResult(token);

        public ValueTaskSourceStatus GetStatus(short token)
            => _core.GetStatus(token);

        public void OnCompleted(
            Action<object?> continuation,
            object? state,
            short token,
            ValueTaskSourceOnCompletedFlags flags)
            => _core.OnCompleted(continuation, state, token, flags);
    }
}
