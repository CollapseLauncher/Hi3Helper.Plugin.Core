#if MANUALCOM

using System;
using System.Runtime.InteropServices;

namespace Hi3Helper.Plugin.Core.Utility;

// ReSharper disable once InconsistentNaming
internal static unsafe class ABIExtension<T>
    where T : ComWrappers, new()
{
    private static T? _cachedWrappers;

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

// ReSharper disable once InconsistentNaming
internal static unsafe class ABIExtension
{
    internal delegate void AllocStrFromFuncDelegate(out string? str);

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