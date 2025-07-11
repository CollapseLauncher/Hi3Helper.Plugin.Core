#if MANUALCOM

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Hi3Helper.Plugin.Core.Utility;

/// <summary>
/// Contains an extension which converts a <see cref="ComObject"/> instance member into a pointer of IUnknown implementation by using a member of <see cref="ComWrappers"/>.
/// </summary>
/// <typeparam name="T">A wrapper member of <see cref="ComWrappers"/>.</typeparam>
internal static unsafe class ComWrappersExtension<T>
    where T : ComWrappers, new()
{
    private static T? _cachedWrappers;

    /// <summary>
    /// Gets a pointer of IUnknown implementation from a member of <see cref="ComObject"/>.
    /// </summary>
    /// <param name="obj">An object which is a member of <see cref="ComObject"/>.</param>
    /// <param name="flags">Flags used to configure the generated interface.</param>
    /// <returns>A pointer to the representation of IUnknown of the current object.</returns>
    internal static void* GetComInterfacePtrFromWrappers(object? obj, CreateComInterfaceFlags flags = CreateComInterfaceFlags.None)
    {
        if (obj == null)
        {
            return null;
        }

        _cachedWrappers ??= new T();
        return (void*)_cachedWrappers.GetOrCreateComInterfaceForObject(obj, flags);
    }
}

/// <summary>
/// Contains an extension needed for marshalling on the ABI functions.
/// </summary>
// ReSharper disable once InconsistentNaming
internal static unsafe class ABIExtension
{
    /// <summary>
    /// A delegate which output the string of the callback.
    /// </summary>
    /// <param name="str">An output string from the callback.</param>
    internal delegate void AllocStrFromFuncDelegate(out string? str);

    /// <summary>
    /// Allocate string into an unmanaged memory of a COM unmanaged allocator.
    /// </summary>
    /// <param name="func">A callback which output a string to allocate into.</param>
    /// <param name="resultNativeParam">An output pointer which contains the address of the UTF-16 char data.</param>
    /// <returns>0 if the string is allocated successfully. Otherwise, returns an HRESULT error code.</returns>
    internal static int AllocUtf16StringFromFunc(AllocStrFromFuncDelegate func, ushort** resultNativeParam)
    {
        try
        {
            func(out string? outStr);
            *resultNativeParam = (ushort*)Marshal.StringToCoTaskMemUni(outStr);

            return 0;
        }
        catch (Exception ex)
        {
            return Marshal.GetHRForException(ex);
        }
    }
}

#endif