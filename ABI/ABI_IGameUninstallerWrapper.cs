#if MANUALCOM

using Hi3Helper.Plugin.Core.Management;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.ComWrappers;

namespace Hi3Helper.Plugin.Core.ABI;

// ReSharper disable once InconsistentNaming
internal sealed unsafe class ABI_IGameUninstallerWrapper
{
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_UninstallAsync(ComInterfaceDispatch* thisNative, Guid* cancelTokenNativeParam, nint* resultNativeParam)
    {
        ref Guid cancelTokenNative = ref *cancelTokenNativeParam;
        ref nint resultNative = ref *resultNativeParam;
        int retVal;
        try
        {
            // Unmarshal - Convert native data to managed data.
            var cancelToken = cancelTokenNative;
            var @this = ComInterfaceDispatch.GetInstance<IGameUninstaller>(thisNative);
            @this.UninstallAsync(in cancelToken, out var result);
            // NotifyForSuccessfulInvoke - Keep alive any managed objects that need to stay alive across the call.
            retVal = 0; // S_OK
            // Marshal - Convert managed data to native data.
            resultNative = result;
        }
        catch (Exception exception)
        {
            retVal = Marshal.GetHRForException(exception);
        }

        return retVal;
    }
}

#endif