using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Hi3Helper.Plugin.Core.Utility;

public static class PluginPInvokeExtension
{
    public static unsafe bool TryGetExport<T>(this nint handle, string exportName, out T callback)
        where T : Delegate
    {
        const string tryGetApiExportName = "TryGetApiExport";

        Unsafe.SkipInit(out callback);

        if (!NativeLibrary.TryGetExport(handle, tryGetApiExportName, out nint tryGetApiExportP) ||
            tryGetApiExportP == nint.Zero)
        {
            return false;
        }

        delegate* unmanaged[Cdecl]<char*, void**, int> tryGetApiExportCallback =
            (delegate* unmanaged[Cdecl]<char*, void**, int>)tryGetApiExportP;

        nint  exportP     = nint.Zero;
        char* exportNameP = exportName.GetPinnableStringPointer();
        int   tryResult   = tryGetApiExportCallback(exportNameP, (void**)&exportP);

        if (tryResult != 0 ||
            exportP == nint.Zero)
        {
            return false;
        }

        callback = Marshal.GetDelegateForFunctionPointer<T>(exportP);
        return true;
    }
}
