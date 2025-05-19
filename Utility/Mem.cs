using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
// ReSharper disable UnusedMember.Global

namespace Hi3Helper.Plugin.Core.Utility;

/// <summary>
/// Mem, mem-mem... Mem mem-mem!!<br/>
/// Mem-mem mem, mem mem-mem mem-mem.<br/>
/// Mem mem-mem, mem-mem mem mem-mem~<br/>
/// <br/><br/>
/// - Mem
/// </summary>
public static class Mem
{
    public static T[] CreateArrayFromSelector<T>(Func<int> countCallback, Func<int, T> selectorCallback)
    {
        int count = countCallback();
        if (count == 0)
        {
            return [];
        }

        T[] values = new T[count];
        for (int i = 0; i < count; i++)
        {
            values[i] = selectorCallback(i);
        }

        return values;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe T* AllocZeroed<T>(int count = 1)
        where T : unmanaged
    {
        nuint sizeOf = EnsureGetSizeOf<T>(count);
        return (T*)NativeMemory.AllocZeroed(sizeOf);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe T* Alloc<T>(int count = 1)
        where T : unmanaged
    {
        nuint sizeOf = EnsureGetSizeOf<T>(count);
        return (T*)NativeMemory.Alloc(sizeOf);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void Free(nint ptr)
        => Free((void*)ptr);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void Free(void* ptr)
    {
        if (ptr == null)
        {
            return;
        }

        NativeMemory.Free(ptr);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Span<T> CreateSpanFromNullTerminated<T>(nint handle)
        where T : unmanaged
        => CreateSpanFromNullTerminated<T>((void*)handle);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Span<T> CreateSpanFromNullTerminated<T>(void* ptr)
        where T : unmanaged
    {
        ThrowIfNotByteOrChar<T>(out bool isChar);

        if (ptr == null)
        {
            return Span<T>.Empty;
        }

        return isChar ?
            new Span<T>(ptr, SpanHelpers.IndexOfNullCharacter((char*)ptr)) :
            new Span<T>(ptr, SpanHelpers.IndexOfNullByte((byte*)ptr));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe string CreateStringFromNullTerminated<T>(nint handle)
        where T : unmanaged
        => CreateStringFromNullTerminated((T*)handle);

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe nuint EnsureGetSizeOf<T>(int count)
        where T : unmanaged
    {
        int sizeOf = sizeof(T);
        if (sizeOf == 0)
        {
            throw new ArgumentException("Size of T cannot be zero.", nameof(T));
        }

        if (count <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), "Count cannot be less than 1.");
        }

        return (nuint)(sizeOf * count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe T* AsPointer<T>(this nint ptr)
        where T : unmanaged
        => (T*)ptr;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe T* AsPointer<T>(this scoped ref T source)
        where T : unmanaged => (T*)Unsafe.AsPointer(ref source);
}
