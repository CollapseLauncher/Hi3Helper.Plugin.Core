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
    internal static unsafe delegate* unmanaged[Cdecl]<InstallPerFileProgress*, void> PerFileProgressCallback;

    /// <summary>
    /// Invokes the per-file progress callback if it has been registered by the Main Application.
    /// </summary>
    /// <param name="perFileDownloadedBytes">Bytes downloaded for the current file.</param>
    /// <param name="perFileTotalBytes">Total size of the current file.</param>
    public static unsafe void InvokePerFileProgress(long perFileDownloadedBytes, long perFileTotalBytes)
    {
        var callback = PerFileProgressCallback;
        if (callback == null)
            return;

        InstallPerFileProgress progress = new()
        {
            PerFileDownloadedBytes = perFileDownloadedBytes,
            PerFileTotalBytes = perFileTotalBytes
        };

        callback(&progress);
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
    private static unsafe HResult SetPerFileProgressCallback(nint perFileProgressCallback)
    {
        if (perFileProgressCallback == nint.Zero)
        {
            PerFileProgressCallback = null;
            InstanceLogger.LogTrace("[SetPerFileProgressCallback] Per-file progress callback has been uninstalled");
            return HResult.Ok;
        }

        PerFileProgressCallback = (delegate* unmanaged[Cdecl]<InstallPerFileProgress*, void>)perFileProgressCallback;
        InstanceLogger.LogTrace("[SetPerFileProgressCallback] Per-file progress callback has been installed at address: 0x{Ptr:x8}", perFileProgressCallback);
        return HResult.Ok;
    }
    #endregion
}
