#if MANUALCOM

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.ComWrappers;

namespace Hi3Helper.Plugin.Core.ABI;

// ReSharper disable once InconsistentNaming
// ReSharper disable once IdentifierTypo
internal unsafe class ABI_IInitializableTask
{
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_InitAsync(ComInterfaceDispatch* thisNative, Guid* cancelTokenNativeParam, nint* resultNativeParam)
    {
        ref Guid cancelTokenNative = ref *cancelTokenNativeParam;
        ref nint resultNative = ref *resultNativeParam;
        int retVal;
        try
        {
            // Unmarshal - Convert native data to managed data.
            var cancelToken = cancelTokenNative;
            var @this = ComInterfaceDispatch.GetInstance<IInitializableTask>(thisNative);
            @this.InitAsync(in cancelToken, out var result);
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