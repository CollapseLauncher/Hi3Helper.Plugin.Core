using System.Runtime.InteropServices;
using System.Text;

namespace Hi3Helper.Plugin.Core;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
public struct PluginDisposableMemoryMarshal(nint handle, int length, bool isDisposable)
{
    public nint Handle       = handle;
    public int  Length       = length;
    public bool IsDisposable = isDisposable;

    public static PluginDisposableMemoryMarshal Empty => new(0, 0, false);

    public static implicit operator PluginDisposableMemoryMarshal(string? inputString)
    {
        if (string.IsNullOrEmpty(inputString))
        {
            return Empty;
        }

        int len = Encoding.UTF8.GetByteCount(inputString);
        PluginDisposableMemory<byte> memory = PluginDisposableMemory<byte>.Alloc(len + 1);
        Encoding.UTF8.GetBytes(inputString, memory.AsSpan());

        return memory.ToUnmanagedMarshal();
    }

    public static implicit operator string?(PluginDisposableMemoryMarshal marshal)
    {
        if (marshal.Handle == 0 || marshal.Length <= 0)
        {
            return null;
        }

        PluginDisposableMemory<byte> memory = marshal.ToManagedSpan<byte>();
        string? returnValue = memory;

        memory.Dispose();
        return returnValue;
    }
}
