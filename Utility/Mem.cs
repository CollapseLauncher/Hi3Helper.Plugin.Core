using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
// ReSharper disable UnusedMember.Global
// ReSharper disable CommentTypo

namespace Hi3Helper.Plugin.Core.Utility;

/// <summary>
/// Contains tools for Memory management, including allocation, freeing and marshalling.
/// </summary>
/// <remarks>
/// Mem, mem-mem... Mem mem-mem!!<br/>
/// Mem-mem mem, mem mem-mem mem-mem.<br/>
/// Mem mem-mem, mem-mem mem mem-mem~<br/>
/// <br/><br/>
/// - Mem
/// </remarks>
public static partial class Mem
{
    /// <summary>
    /// Allocate native memory using UCRT's malloc.
    /// </summary>
    /// <typeparam name="T">The type of struct to allocate.</typeparam>
    /// <param name="count">Count of elements to allocate.</param>
    /// <param name="zeroed">Whether the memory is zeroed on allocation or not.</param>
    /// <returns>The pointer of the <typeparamref name="T"/> type on native memory.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe T* Alloc<T>(int count = 1, bool zeroed = true)
        where T : unmanaged
    {
        nuint sizeOf = EnsureGetSizeOf<T>(count);
        return zeroed ? (T*)NativeMemory.AllocZeroed(sizeOf) : (T*)NativeMemory.Alloc(sizeOf);
    }

    /// <summary>
    /// Allocate native memory using UCRT's malloc and use it as .NET ref.
    /// </summary>
    /// <typeparam name="T">The type of struct to allocate.</typeparam>
    /// <param name="count">Count of elements to allocate.</param>
    /// <param name="zeroed">Whether the memory is zeroed on allocation or not.</param>
    /// <returns>The pointer of the <typeparamref name="T"/> type on native memory.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe ref T AllocAsRef<T>(int count = 1, bool zeroed = true)
        where T : unmanaged
        => ref Unsafe.AsRef<T>(Alloc<T>(count, zeroed));

    /// <summary>
    /// Free native memory.
    /// </summary>
    /// <param name="ptr">The pointer of the memory.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void Free(this nint ptr) => NativeMemory.Free((void*)ptr);

    /// <summary>
    /// Free native memory.
    /// </summary>
    /// <param name="ptr">The pointer of the memory.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe void Free(void* ptr) => NativeMemory.Free(ptr);

    /// <summary>
    /// Gets the size of <typeparamref name="T"/> type.
    /// </summary>
    /// <typeparam name="T">The type of struct to get size of.</typeparam>
    /// <param name="count">Count of elements.</param>
    /// <returns>The size of the data (in bytes) based on the <c>sizeof(<typeparamref name="T"/>)</c> * <paramref name="count"/></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe nuint EnsureGetSizeOf<T>(int count)
        where T : unmanaged => (nuint)(sizeof(T) * (uint)count);

    /// <summary>
    /// Cast the safe pointer as unmanaged pointer of the <typeparamref name="T"/> type.
    /// </summary>
    /// <typeparam name="T">The type of the struct to cast onto.</typeparam>
    /// <param name="ptr">The safe pointer to cast.</param>
    /// <returns>An unmanaged pointer of the <typeparamref name="T"/> type.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe T* AsPointer<T>(this nint ptr)
        where T : unmanaged
        => (T*)ptr;

    /// <summary>
    /// Cast the .NET object ref as unmanaged pointer of the <typeparamref name="T"/> type.
    /// </summary>
    /// <typeparam name="T">The type of the struct to cast onto.</typeparam>
    /// <param name="source">The ref of the object.</param>
    /// <returns>An unmanaged pointer of the <typeparamref name="T"/> type.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe T* AsPointer<T>(this scoped ref T source)
        where T : unmanaged => (T*)Unsafe.AsPointer(ref source);

    /// <summary>
    /// Cast safe pointer into .NET object ref of the <typeparamref name="T"/> type.
    /// </summary>
    /// <typeparam name="T">The type of the struct to cast onto.</typeparam>
    /// <param name="ptr">The safe pointer to cast into.</param>
    /// <returns>.NET object ref of the <typeparamref name="T"/> type.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe ref T AsRef<T>(this nint ptr)
        where T : unmanaged
        => ref AsRef<T>((void*)ptr);

    /// <summary>
    /// Cast any unmanaged pointer into .NET object ref of the <typeparamref name="T"/> type.
    /// </summary>
    /// <typeparam name="T">The type of the struct to cast onto.</typeparam>
    /// <param name="ptr">Unmanaged pointer to cast into.</param>
    /// <returns>.NET object ref of the <typeparamref name="T"/> type.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe ref T AsRef<T>(void* ptr)
        where T : unmanaged
        => ref Unsafe.AsRef<T>(ptr);

    /// <summary>
    /// Copy managed struct and allocate it into unmanaged native memory.
    /// </summary>
    /// <typeparam name="T">The type of <typeparamref name="T"/> struct to copy from.</typeparam>
    /// <param name="source">The .NET object ref of <typeparamref name="T"/> to copy from.</param>
    /// <returns>An unmanaged native memory pointer to the struct.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe T* CopyStructToUnmanaged<T>(this scoped ref T source)
        where T : unmanaged
    {
        int sizeOf = sizeof(T);
        T* ptrTo = Alloc<T>();

        fixed (T* ptrFrom = &source)
        {
            NativeMemory.Copy(ptrFrom, ptrTo, (nuint)sizeOf);
        }

        return ptrTo;
    }
}
