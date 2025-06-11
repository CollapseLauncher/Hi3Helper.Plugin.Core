using System.Runtime.InteropServices;

namespace Hi3Helper.Plugin.Core.Management;

[StructLayout(LayoutKind.Explicit)] // Fit to 32 bytes
public struct InstallProgress
{
    [FieldOffset(0)]
    public long DownloadedBytes;
    [FieldOffset(8)]
    public long TotalBytesToDownload;

    [FieldOffset(16)]
    public int DownloadedCount;
    [FieldOffset(20)]
    public int TotalCountToDownload;

    [FieldOffset(24)]
    public int StateCount;
    [FieldOffset(28)]
    public int TotalStateToComplete;
}