using System.Runtime.InteropServices;

namespace Hi3Helper.Plugin.Core.Management;

/// <summary>
/// Defines the status of each of the download progress, including: Asset Download Size, Asset Count and State Count.
/// </summary>
[StructLayout(LayoutKind.Explicit)] // Fit to 32 bytes
public struct InstallProgress
{
    /// <summary>
    /// Indicates how many bytes already downloaded.
    /// </summary>
    [FieldOffset(0)]
    public long DownloadedBytes;

    /// <summary>
    /// Indicates how many bytes need to be downloaded in total.
    /// </summary>
    [FieldOffset(8)]
    public long TotalBytesToDownload;

    /// <summary>
    /// Indicates how many assets/files already downloaded.
    /// </summary>
    [FieldOffset(16)]
    public int DownloadedCount;

    /// <summary>
    /// Indicates how many assets/files need to be downloaded in total.
    /// </summary>
    [FieldOffset(20)]
    public int TotalCountToDownload;

    /// <summary>
    /// Indicates how many process/states the download progress is currently taken.
    /// </summary>
    [FieldOffset(24)]
    public int StateCount;

    /// <summary>
    /// Indicates how many process/states the download progress need to be finished.
    /// </summary>
    [FieldOffset(28)]
    public int TotalStateToComplete;
}