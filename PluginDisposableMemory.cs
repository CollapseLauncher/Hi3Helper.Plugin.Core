using Hi3Helper.Plugin.Core.Utility;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
// ReSharper disable UnusedMember.Global

namespace Hi3Helper.Plugin.Core;

[StructLayout(LayoutKind.Sequential, Pack = 8)]
[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public unsafe struct PluginDisposableMemory<T>(T* handle, int count, bool isDisposable = true) : IDisposable
    where T : unmanaged
{
    private byte _isFreed     = 0;
    public  byte IsDisposable = (byte)(isDisposable ? 1 : 0);
    public  int  Length       = count;

    public  readonly bool IsEmpty => Length == 0;

    public static PluginDisposableMemory<T> Empty => new(null, 0, false);

    public readonly ref T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get
        {
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, Length, nameof(index));

            return ref Unsafe.AsRef<T>(Unsafe.Add<T>(handle, index));
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ref T AsRef() => ref Unsafe.AsRef<T>(handle);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly T* AsPointer() => handle;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly nint AsSafePointer() => (nint)AsPointer();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly UnmanagedMemoryStream AsStream() => new((byte*)AsPointer(), Length * sizeof(T));

    public void* AsSpanPointer() => Unsafe.AsPointer(ref this);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Span<T> AsSpan(int offset = 0)
        => AsSpan(offset, Length - offset);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly Span<T> AsSpan(int offset, int length)
        => new(Unsafe.Add<T>(handle, offset), length);

    /// <summary>
    /// Dispose the handle inside the span
    /// </summary>
    public void Dispose()
    {
        if (IsDisposable == 1)
        {
            ForceDispose();
        }
    }

    /// <summary>
    /// Forcefully freed the span, even though the object is not disposable.
    /// </summary>
    public void ForceDispose()
    {
        if (_isFreed == 1) return;

        Mem.Free(handle);
        _isFreed = 1;
    }

    /// <summary>
    /// Create and allocate span of <typeparamref name="T"/>.<br/>
    /// </summary>
    /// <param name="count">How much object to allocate to unmanaged memory</param>
    /// <param name="isZeroed">Whether the memory is zeroed</param>
    /// <param name="isDisposable">Whether the </param>
    /// <returns>Disposable Span of <typeparamref name="T"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static PluginDisposableMemory<T> Alloc(int count = 1, bool isZeroed = true, bool isDisposable = true)
    {
        // Allocate the instance of T and create a span from it.
        PluginDisposableMemory<T> memory = new(Mem.Alloc<T>(count, isZeroed), count, isDisposable);
        return memory;
    }

    public static implicit operator string?(PluginDisposableMemory<T> memory)
    {
        if (memory.IsEmpty)
        {
            return null;
        }

        int sizeofT = sizeof(T);
        if (sizeofT > 2)
        {
            throw new InvalidCastException("Cannot convert the span type to string. You must pass PluginDisposableMemory<byte> or PluginDisposableMemory<char> to convert into string implicitly.");
        }

        return memory.CreateStringFromNullTerminated();
    }
}
