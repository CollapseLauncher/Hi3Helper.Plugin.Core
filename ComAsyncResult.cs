using System.Runtime.InteropServices;

namespace Hi3Helper.Plugin.Core;

[StructLayout(LayoutKind.Sequential)]
public struct ComAsyncResult
{
    public nint Handle;
    public bool IsCancelled;
    public bool IsSuccessful;
    public bool IsFaulty;
}
