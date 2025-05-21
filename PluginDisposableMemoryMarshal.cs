using System.Runtime.InteropServices;

namespace Hi3Helper.Plugin.Core;

[StructLayout(LayoutKind.Sequential, Size = 16)]
public struct PluginDisposableMemoryMarshal(nint handle, int length, bool isDisposable)
{
    public nint Handle       = handle;
    public int  Length       = length;
    public bool IsDisposable = isDisposable;

    public static PluginDisposableMemoryMarshal Empty => new(0, 0, false);
}
