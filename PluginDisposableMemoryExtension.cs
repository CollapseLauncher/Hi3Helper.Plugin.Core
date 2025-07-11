using Hi3Helper.Plugin.Core.Utility;
using System.Runtime.CompilerServices;

namespace Hi3Helper.Plugin.Core;

/// <summary>
/// Provides extension methods for working with <see cref="PluginDisposableMemory{T}"/> and <see cref="PluginDisposableMemoryMarshal"/> objects, enabling conversions between managed and unmanaged memory.
/// </summary>
public static class PluginDisposableMemoryExtension
{
    public delegate void MarshalToMemorySelectorDelegate(out nint handle, out int count, out bool isDisposable, out bool isAllocated);

    /// <summary>
    /// Converts the unmanaged memory represented by the <see cref="PluginDisposableMemoryMarshal"/> instance into a
    /// managed span of the specified type.
    /// </summary>
    /// <remarks>
    /// The returned span provides access to the memory block represented by <paramref name="memory"/>.<br/>
    /// If <paramref name="memory"/> is marked as disposable, the span will also manage its disposal.
    /// </remarks>
    /// <typeparam name="T">The type of elements in the span. Must be an unmanaged type.</typeparam>
    /// <param name="memory">A reference to the <see cref="PluginDisposableMemoryMarshal"/> instance containing the unmanaged memory to
    /// convert.</param>
    /// <returns>A <see cref="PluginDisposableMemory{T}"/> instance representing the managed span of type <typeparamref name="T"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe PluginDisposableMemory<T> ToManagedSpan<T>(this ref PluginDisposableMemoryMarshal memory)
        where T : unmanaged => new((T*)memory.Handle, memory.Length, memory.IsDisposable);

    /// <summary>
    /// Converts a delegate-based memory selector into a managed span of the specified unmanaged type.
    /// </summary>
    /// <remarks>
    /// This method is designed for scenarios where unmanaged memory needs to be safely wrapped in a
    /// managed structure for further processing. The returned span can optionally be disposable, depending on the
    /// information provided by the <paramref name="selector"/>.
    /// </remarks>
    /// <typeparam name="T">The unmanaged type of the elements in the span.</typeparam>
    /// <param name="selector">A delegate that provides memory information, including the memory handle, length, allocation status, and disposability.</param>
    /// <returns
    /// >A <see cref="PluginDisposableMemory{T}"/> instance representing the managed span of memory.<br/>
    /// Returns <see  cref="PluginDisposableMemory{T}.Empty"/> if the memory is not allocated or the length is less than or equal to zero.
    /// </returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe PluginDisposableMemory<T> ToManagedSpan<T>(this MarshalToMemorySelectorDelegate selector)
        where T : unmanaged
    {
        selector(out nint handle, out int length, out bool isDisposable, out bool isAllocated);
        if (!isAllocated || length <= 0)
        {
            return PluginDisposableMemory<T>.Empty;
        }

        PluginDisposableMemory<T> span = new((T*)handle, length, isDisposable);
        return span;
    }

    /// <summary>
    /// Converts a <see cref="PluginDisposableMemory{T}"/> instance to a <see cref="PluginDisposableMemoryMarshal"/> struct.
    /// </summary>
    /// <remarks>
    /// This method allocates unmanaged memory for the <see cref="PluginDisposableMemoryMarshal"/>
    /// structure and initializes it with the handle, length, and disposability state of the provided <see cref="PluginDisposableMemory{T}"/> instance.<br/>
    /// The caller is responsible for ensuring proper disposal of the allocated memory to avoid memory leaks.
    /// </remarks>
    /// <typeparam name="T">The type of the elements in the memory. Must be an unmanaged type.</typeparam>
    /// <param name="memory">The <see cref="PluginDisposableMemory{T}"/> instance to convert.</param>
    /// <returns>
    /// A reference to a newly allocated <see cref="PluginDisposableMemoryMarshal"/> structure that represents the unmanaged memory.
    /// </returns>
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

    /// <summary>
    /// Releases the unmanaged memory associated with the specified <see cref="PluginDisposableMemoryMarshal"/>.
    /// </summary>
    /// <param name="marshal">A reference to the <see cref="PluginDisposableMemoryMarshal"/> instance whose unmanaged memory is to be freed.</param>
    // ReSharper disable once UnusedMember.Global
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void FreeMarshal(this ref PluginDisposableMemoryMarshal marshal) => FreeMarshal(marshal.AsPointer());

    /// <summary>
    /// Releases the unmanaged memory associated with the specified <see cref="PluginDisposableMemoryMarshal"/>.
    /// </summary>
    /// <param name="marshal">A reference to the <see cref="PluginDisposableMemoryMarshal"/> instance whose unmanaged memory is to be freed.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void FreeMarshal(PluginDisposableMemoryMarshal* marshal) => Mem.Free(marshal);
}
