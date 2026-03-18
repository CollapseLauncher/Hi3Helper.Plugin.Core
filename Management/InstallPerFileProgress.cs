using System.Runtime.InteropServices;

namespace Hi3Helper.Plugin.Core.Management;

/// <summary>
/// Defines the per-file download progress reported via the V1Ext_Update5 callback.
/// </summary>
[StructLayout(LayoutKind.Explicit)] // Fit to 16 bytes
public struct InstallPerFileProgress
{
    /// <summary>
    /// Indicates how many bytes have been downloaded for the current file.
    /// </summary>
    [FieldOffset(0)]
    public long PerFileDownloadedBytes;

    /// <summary>
    /// Indicates the total size in bytes of the current file.
    /// </summary>
    [FieldOffset(8)]
    public long PerFileTotalBytes;
}
