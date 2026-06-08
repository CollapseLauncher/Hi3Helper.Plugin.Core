using Hi3Helper.Plugin.Core.Management;
using Hi3Helper.Plugin.Core.Utility;
using Microsoft.Extensions.Logging;

namespace Hi3Helper.Plugin.Core;

public partial class SharedStaticV1Ext
{
    // Update5
    /// <summary>
    /// Sets the per-file progress callback from the Main Application.
    /// To disable the callback, set <paramref name="perFileProgressCallback"/> to <see cref="nint.Zero"/>.
    /// </summary>
    /// <param name="perFileProgressCallback">The address of the per-file progress callback function.</param>
    internal delegate HResult SetPerFileProgressCallbackDelegate(nint perFileProgressCallback);

    /// <summary>
    /// The stored function pointer for the per-file progress callback from the Main Application.
    /// </summary>
    internal static nint PerFileProgressCallbackAddr;

    /// <summary>
    /// Invokes the per-file progress callback if it has been registered by the Main Application.
    /// </summary>
    /// <param name="perFileDownloadedBytes">Bytes downloaded for the current file.</param>
    /// <param name="perFileTotalBytes">Total size of the current file.</param>
    public static unsafe void InvokePerFileProgress(long perFileDownloadedBytes, long perFileTotalBytes)
    {
        if (PerFileProgressCallbackAddr == nint.Zero)
        {
            return;
        }

        // Call the callback
        InstallPerFileProgress progress = new()
        {
            PerFileDownloadedBytes = perFileDownloadedBytes,
            PerFileTotalBytes = perFileTotalBytes
        };
        ((delegate* unmanaged[Cdecl]<ref readonly InstallPerFileProgress, void>)
            PerFileProgressCallbackAddr)(in progress);
    }
}

public partial class SharedStaticV1Ext<T>
{
    private static void InitExtension_Update5Exports()
    {
        /* ----------------------------------------------------------------------
         * Update 5 Feature Sets
         * ----------------------------------------------------------------------
         * This feature sets includes the following feature:
         *  - Per-file Install Progress Callback
         *    - SetPerFileProgressCallback
         */

        // -> Plugin Per-file Progress Callback Setter
        TryRegisterApiExport<SetPerFileProgressCallbackDelegate>("SetPerFileProgressCallback", SetPerFileProgressCallback);
    }

    #region ABI Proxies
    /// <summary>
    /// This method is an ABI proxy and installer for the Per-file Progress Callback functionality.
    /// </summary>
    private static HResult SetPerFileProgressCallback(nint perFileProgressCallback)
    {
        if (perFileProgressCallback == nint.Zero)
        {
            PerFileProgressCallbackAddr = nint.Zero;
            InstanceLogger.LogTrace("[SetPerFileProgressCallback] Per-file progress callback has been uninstalled");
            return HResult.Ok;
        }

        PerFileProgressCallbackAddr = perFileProgressCallback;
        InstanceLogger.LogTrace("[SetPerFileProgressCallback] Per-file progress callback has been installed at address: 0x{Ptr:x8}", perFileProgressCallback);
        return HResult.Ok;
    }
    #endregion
}
