using Hi3Helper.Plugin.Core.Utility;
using System.Runtime.CompilerServices;

namespace Hi3Helper.Plugin.Core;

public static class PluginDisposableMemoryExtension
{
    public delegate bool MarshalToMemorySelectorDelegate(out nint handle, out int count, out bool isDisposable);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe PluginDisposableMemory<T> ToManagedSpan<T>(this ref PluginDisposableMemoryMarshal memory)
        where T : unmanaged
    {
        PluginDisposableMemory<T> span = new((T*)memory.Handle, memory.Length, memory.IsDisposable);
        return span;
    }

    public static unsafe PluginDisposableMemory<T> ToManagedSpan<T>(this MarshalToMemorySelectorDelegate selector)
        where T : unmanaged
    {
        if (!selector(out nint handle, out int length, out bool isDisposable) || length <= 0)
        {
            return PluginDisposableMemory<T>.Empty;
        }

        PluginDisposableMemory<T> span = new((T*)handle, length, isDisposable);
        return span;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe ref PluginDisposableMemoryMarshal ToUnmanagedMarshal<T>(this PluginDisposableMemory<T> memory)
        where T : unmanaged
    {
        nint handle       = (nint)memory.AsPointer();
        int  length       = memory.Length;
        bool isDisposable = memory.IsDisposable == 1;

        PluginDisposableMemoryMarshal* ptr = Mem.Alloc<PluginDisposableMemoryMarshal>();
        ptr->Handle = handle;
        ptr->Length = length;
        ptr->IsDisposable = isDisposable;

        return ref Mem.AsRef<PluginDisposableMemoryMarshal>(ptr);
    }

    public static unsafe void FreeMarshal(this ref PluginDisposableMemoryMarshal marshal) => FreeMarshal(marshal.AsPointer());

    public static unsafe void FreeMarshal(PluginDisposableMemoryMarshal* marshal) => Mem.Free(marshal);
}
