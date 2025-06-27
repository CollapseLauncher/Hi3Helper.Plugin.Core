#if MANUALCOM

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Hi3Helper.Plugin.Core.ABI;

// ReSharper disable once InconsistentNaming
internal unsafe class ABI_IFreeWrapper
{
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_Free(ComWrappers.ComInterfaceDispatch* thisNative)
    {
        int retVal;
        try
        {
            // Unmarshal - Convert native data to managed data.
            var @this = ComWrappers.ComInterfaceDispatch.GetInstance<IFree>(thisNative);
            @this.Free();
            // NotifyForSuccessfulInvoke - Keep alive any managed objects that need to stay alive across the call.
            retVal = 0; // S_OK
        }
        catch (Exception exception)
        {
            retVal = Marshal.GetHRForException(exception);
        }

        return retVal;
    }
}

#endif