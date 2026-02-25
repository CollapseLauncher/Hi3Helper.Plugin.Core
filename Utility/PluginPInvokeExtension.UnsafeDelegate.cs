using System.Runtime.CompilerServices;

namespace Hi3Helper.Plugin.Core.Utility;

public static partial class PluginPInvokeExtension
{
    [SkipLocalsInit]
    public static bool TryGetExportUnsafe(this nint handle, string exportName, out nint callbackP)
        => TryGetExportAddressCore(handle, exportName, out callbackP);

    [SkipLocalsInit]
    public static unsafe bool TryGetExportUnsafe(this nint handle, string exportName, out delegate* unmanaged[Cdecl]<void> delegateC)
    {
        delegateC = null;
        if (!TryGetExportAddressCore(handle, exportName, out nint callbackP))
        {
            return false;
        }

        delegateC = (delegate* unmanaged[Cdecl]<void>)callbackP;
        return true;
    }

    [SkipLocalsInit]
    public static unsafe bool TryGetExportUnsafe<TReturn>(this nint handle, string exportName, out delegate* unmanaged[Cdecl]<TReturn> delegateC)
    {
        delegateC = null;
        if (!TryGetExportAddressCore(handle, exportName, out nint callbackP))
        {
            return false;
        }

        delegateC = (delegate* unmanaged[Cdecl]<TReturn>)callbackP;
        return true;
    }

    [SkipLocalsInit]
    public static unsafe bool TryGetExportUnsafe<T1>(this nint handle, string exportName, out delegate* unmanaged[Cdecl]<T1, void> delegateC)
        where T1 : unmanaged, allows ref struct
    {
        delegateC = null;
        if (!TryGetExportAddressCore(handle, exportName, out nint callbackP))
        {
            return false;
        }

        delegateC = (delegate* unmanaged[Cdecl]<T1, void>)callbackP;
        return true;
    }

    [SkipLocalsInit]
    public static unsafe bool TryGetExportUnsafe<T1, TReturn>(this nint handle, string exportName, out delegate* unmanaged[Cdecl]<T1, TReturn> delegateC)
        where T1 : unmanaged, allows ref struct
    {
        delegateC = null;
        if (!TryGetExportAddressCore(handle, exportName, out nint callbackP))
        {
            return false;
        }

        delegateC = (delegate* unmanaged[Cdecl]<T1, TReturn>)callbackP;
        return true;
    }

    [SkipLocalsInit]
    public static unsafe bool TryGetExportUnsafe<T1, T2>(this nint handle, string exportName, out delegate* unmanaged[Cdecl]<T1, T2, void> delegateC)
        where T1 : unmanaged, allows ref struct
        where T2 : unmanaged, allows ref struct
    {
        delegateC = null;
        if (!TryGetExportAddressCore(handle, exportName, out nint callbackP))
        {
            return false;
        }

        delegateC = (delegate* unmanaged[Cdecl]<T1, T2, void>)callbackP;
        return true;
    }

    [SkipLocalsInit]
    public static unsafe bool TryGetExportUnsafe<T1, T2, TReturn>(this nint handle, string exportName, out delegate* unmanaged[Cdecl]<T1, T2, TReturn> delegateC)
        where T1 : unmanaged, allows ref struct
        where T2 : unmanaged, allows ref struct
    {
        delegateC = null;
        if (!TryGetExportAddressCore(handle, exportName, out nint callbackP))
        {
            return false;
        }

        delegateC = (delegate* unmanaged[Cdecl]<T1, T2, TReturn>)callbackP;
        return true;
    }

    [SkipLocalsInit]
    public static unsafe bool TryGetExportUnsafe<T1, T2, T3>(this nint handle, string exportName, out delegate* unmanaged[Cdecl]<T1, T2, T3, void> delegateC)
        where T1 : unmanaged, allows ref struct
        where T2 : unmanaged, allows ref struct
        where T3 : unmanaged, allows ref struct
    {
        delegateC = null;
        if (!TryGetExportAddressCore(handle, exportName, out nint callbackP))
        {
            return false;
        }

        delegateC = (delegate* unmanaged[Cdecl]<T1, T2, T3, void>)callbackP;
        return true;
    }

    [SkipLocalsInit]
    public static unsafe bool TryGetExportUnsafe<T1, T2, T3, TReturn>(this nint handle, string exportName, out delegate* unmanaged[Cdecl]<T1, T2, T3, TReturn> delegateC)
        where T1 : unmanaged, allows ref struct
        where T2 : unmanaged, allows ref struct
        where T3 : unmanaged, allows ref struct
    {
        delegateC = null;
        if (!TryGetExportAddressCore(handle, exportName, out nint callbackP))
        {
            return false;
        }

        delegateC = (delegate* unmanaged[Cdecl]<T1, T2, T3, TReturn>)callbackP;
        return true;
    }
}
