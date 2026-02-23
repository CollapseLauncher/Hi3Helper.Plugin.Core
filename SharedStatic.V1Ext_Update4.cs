using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Hi3Helper.Plugin.Core.Utility;
using Microsoft.Extensions.Logging;

namespace Hi3Helper.Plugin.Core;

public partial class SharedStaticV1Ext
{
    // Update4
    /// <summary>
    /// Registers the function of the Speed Throttler Service from the Main Application.
    /// To disable the speed throttler service, set the <paramref name="addBytesOrWaitAsyncCallback"/> to <see cref="nint.Zero"/>.
    /// </summary>
    /// <param name="addBytesOrWaitAsyncCallback">The address of the adder wait callback</param>
    /// <param name="addBytesOrWaitAsyncCallback">The address of the current speed getter callback</param>
    internal delegate HResult RegisterSpeedThrottlerServiceDelegate(nint addBytesOrWaitAsyncCallback, nint getSharedThrottleBytesCallback);
}

public partial class SharedStaticV1Ext<T>
{
    private static void InitExtension_Update4Exports()
    {
        /* ----------------------------------------------------------------------
         * Update 4 Feature Sets
         * ----------------------------------------------------------------------
         * This feature sets includes the following feature:
         *  - Download Speed Limiter Service
         *    - RegisterSpeedThrottlerService
         */

        // -> Plugin Speed Throttler Service Installer
        TryRegisterApiExport<RegisterSpeedThrottlerServiceDelegate>("RegisterSpeedThrottlerService", RegisterSpeedThrottlerService);
    }

    #region ABI Proxies
    /// <summary>
    /// This method is an ABI proxy and installer for the Speed Throttler Service functionality. To use Speed Throttler Service functionalities, Use <see cref="SpeedLimiterService"/> instead.
    /// </summary>
    private static unsafe HResult RegisterSpeedThrottlerService(nint addBytesOrWaitAsyncCallback, nint getSharedThrottleBytesCallback)
    {
        if (addBytesOrWaitAsyncCallback == nint.Zero && getSharedThrottleBytesCallback == nint.Zero)
        {
            SpeedLimiterService.AddBytesOrWaitAsyncCallback = null;
            SpeedLimiterService.GetSharedThrottleBytesCallback = null;
            InstanceLogger.LogTrace("[RegisterSpeedThrottlerService] Speed Throttler Service has been uninstalled");

            return HResult.Ok;
        }

        if (addBytesOrWaitAsyncCallback != nint.Zero && getSharedThrottleBytesCallback != nint.Zero)
        {
            SpeedLimiterService.AddBytesOrWaitAsyncCallback = (delegate* unmanaged[Cdecl]<nint, long, nint, out nint, int>)addBytesOrWaitAsyncCallback;

            // Test the delegate first before registering it.
            nint contextP = SpeedLimiterService.CreateServiceContext();
            try
            {
                // Try call the increment function with 16 bytes of load, then check for exception
#pragma warning disable CA2012
                ValueTask task = SpeedLimiterService.AddBytesOrWaitAsync(contextP, 16);
#pragma warning restore CA2012
                if (task is { IsCompleted: true, IsFaulted: false } ||
                    task.IsCompletedSuccessfully)
                {
                    LogSuccess();
                    return HResult.Ok;
                }

                // Try block and await if task is still going.
                task.GetAwaiter().GetResult();

                // If nothing blown up, return OK.
                LogSuccess();
                return HResult.Ok;

                void LogSuccess()
                {
                    InstanceLogger.LogTrace("[RegisterSpeedThrottlerService] Speed Throttler Service has been installed. Service's callback is located at address: 0x{Ptr:x8}", addBytesOrWaitAsyncCallback);
                }
            }
            catch (Exception ex)
            {
                SpeedLimiterService.AddBytesOrWaitAsyncCallback = null; // Reset the callback
                return Marshal.GetHRForException(ex);
            }
            finally
            {
                SpeedLimiterService.FreeServiceContext(contextP);
            }
        }

        InstanceLogger.LogError("[RegisterSpeedThrottlerService] Failed to install/uninstall Speed Throttler Service. You must provide both arguments either all null or not-null!");
        return 0x80070057; // ERROR_INVALID_PARAMETER
    }
    #endregion
}
