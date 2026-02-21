using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Hi3Helper.Plugin.Core.Utility.Windows;
using Microsoft.Win32.SafeHandles;

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
    internal static unsafe delegate* unmanaged[Stdcall]<nint, long, nint, out nint, int> AddBytesOrWaitAsyncCallback =
        null;

    /// <summary>
    /// Creates a context to be used for the speed limiter service. This context can be used into multiple instances or threads of your downloader.
    /// </summary>
    /// <returns></returns>
    public static unsafe nint CreateServiceContext()
        => (nint)Mem.Alloc<long>(2); // Context struct is 16 bytes in size.

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
        if (AddBytesOrWaitAsyncCallback == null)
        {
            return ValueTask.CompletedTask;
        }

        nint tokenHandle = token.WaitHandle.SafeWaitHandle.DangerousGetHandle();
        int hr = AddBytesOrWaitAsyncCallback(context,
                                         readBytes,
                                         tokenHandle,
                                         out nint asyncWaitHandle);

        AsyncValueTaskMethodBuilder valueTaskCs = new();
        if (Marshal.GetExceptionForHR(hr) is { } exception)
        {
            valueTaskCs.SetException(exception);
            return valueTaskCs.Task;
        }

        SafeWaitHandle safeHandle = new(asyncWaitHandle, false);
        WaitHandle waitHandle = new EventWaitHandle(false, EventResetMode.ManualReset)
        {
            SafeWaitHandle = safeHandle
        };

        ThreadPool.UnsafeRegisterWaitForSingleObject(waitHandle,
            DisposeWaitHandleCallback,
            null,
            -1,
            true);

        return valueTaskCs.Task;

        void DisposeWaitHandleCallback(object? state, bool isTimedOut)
        {
            safeHandle.Dispose();
            waitHandle.Dispose();

            if (asyncWaitHandle != nint.Zero)
            {
                _ = PInvoke.CloseHandle(asyncWaitHandle);
            }

            valueTaskCs.SetResult();
        }
    }
}
