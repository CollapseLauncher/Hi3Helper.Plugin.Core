using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Hi3Helper.Plugin.Core.Utility;

public static class PluginPInvokeExtension
{
    public static bool TryGetExport<T>(this nint handle, string exportName, out T callback)
        where T : Delegate
    {
        Unsafe.SkipInit(out callback);
        if (!TryGetExportAddressCore(handle, exportName, out nint exportP)) return false;

        callback = Marshal.GetDelegateForFunctionPointer<T>(exportP);
        return true;
    }

    public static bool TryGetExportUnsafe(this nint handle, string exportName, out nint callbackP)
    {
        Unsafe.SkipInit(out callbackP);
        return TryGetExportAddressCore(handle, exportName, out callbackP);
    }

    private static unsafe bool TryGetExportAddressCore(
        nint     handle,
        string   exportName,
        out nint exportP)
    {
        const string tryGetApiExportName = "TryGetApiExport";

        if (!NativeLibrary.TryGetExport(handle, tryGetApiExportName, out nint tryGetApiExportP) ||
            tryGetApiExportP == nint.Zero)
        {
            exportP = 0;
            return false;
        }

        delegate* unmanaged[Cdecl]<nint, out nint, int> tryGetApiExportCallback =
            (delegate* unmanaged[Cdecl]<nint, out nint, int>)tryGetApiExportP;

        int tryResult = tryGetApiExportCallback(exportName.GetPinnableStringPointerSafe(), out exportP);

        return tryResult == 0 &&
               exportP != nint.Zero;
    }
}
