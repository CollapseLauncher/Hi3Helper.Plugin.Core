using Hi3Helper.Plugin.Core.Utility;
using System;
using System.Runtime.CompilerServices;

namespace Hi3Helper.Plugin.Core;

[method: MethodImpl(MethodImplOptions.AggressiveInlining)]
public unsafe struct PluginDisposableMemory<T>(T* handle, int count, bool isDisposable = true) : IDisposable
    where T : unmanaged
{
    public int  Length       = count;
    public bool IsDisposable = isDisposable;

    public  readonly bool IsEmpty => Length == 0;
    private          int  _isFreed = 0;

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
        if (IsDisposable)
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
}
