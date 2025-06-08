using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;

namespace Hi3Helper.Plugin.Core.Utility;

public static partial class Mem
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Span<T> CreateSpanFromNullTerminated<T>(this PluginDisposableMemory<T> memory)
        where T : unmanaged
        => CreateSpanFromNullTerminated<T>(memory.AsPointer());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Span<T> CreateSpanFromNullTerminated<T>(void* ptr)
        where T : unmanaged
    {
        ThrowIfNotByteOrChar<T>(out bool isChar);

        if (ptr == null)
        {
            return [];
        }

        return isChar ?
            new Span<T>(ptr, SpanHelpers.IndexOfNullCharacter((char*)ptr)) :
            new Span<T>(ptr, SpanHelpers.IndexOfNullByte((byte*)ptr));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe string? CreateStringFromNullTerminated<T>(this PluginDisposableMemory<T> memory)
        where T : unmanaged
        => memory.IsEmpty ? null : CreateStringFromNullTerminated(memory.AsPointer());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe string CreateStringFromNullTerminated<T>(T* source)
        where T : unmanaged
    {
        ThrowIfNotByteOrChar<T>(out bool isChar);

        int len;
        if (isChar)
        {
            len = SpanHelpers.IndexOfNullCharacter((char*)source);
            return new string((char*)source, 0, len);
        }

        len = SpanHelpers.IndexOfNullByte((byte*)source);
        return Encoding.UTF8.GetString((byte*)source, len);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyToUtf8(this ReadOnlySpan<char> source, PluginDisposableMemory<byte> destination)
        => source.CopyToUtf8(destination.AsSpan());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyToUtf8(this ReadOnlySpan<char> source, Span<byte> destination)
    {
        if (destination.Length < Encoding.UTF8.GetByteCount(source))
        {
            throw new ArgumentException("Destination span is not large enough to hold the UTF-8 encoded string.", nameof(destination));
        }

        _ = Encoding.UTF8.GetBytes(source, destination);
    }

    public static unsafe byte* Utf16SpanToUtf8Unmanaged(this ReadOnlySpan<char> span)
    {
        if (span.IsEmpty)
            return null;

        int exactByteCount = checked(Encoding.UTF8.GetByteCount(span) + 1); // + 1 for null terminator
        byte* mem = (byte*)Marshal.AllocCoTaskMem(exactByteCount);
        Span<byte> buffer = new(mem, exactByteCount);

        int byteCount = Encoding.UTF8.GetBytes(span, buffer);
        buffer[byteCount] = 0; // null-terminate
        return mem;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ThrowIfNotByteOrChar<T>(out bool isChar)
        where T : unmanaged
    {
        isChar = false;
        Type currentType = typeof(T);

        if (currentType == typeof(char))
        {
            isChar = true;
            return;
        }

        if (currentType != typeof(byte)) throw new InvalidOperationException("Type must be a char or byte!");
    }
}
