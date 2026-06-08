#if MANUALCOM

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Hi3Helper.Plugin.Core.Management.Api;
using static System.Runtime.InteropServices.ComWrappers;

namespace Hi3Helper.Plugin.Core.ABI;

// ReSharper disable once InconsistentNaming
internal sealed unsafe class ABI_ILauncherApiNewsWrapper
{
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_GetNewsEntries(ComInterfaceDispatch* thisNative, nint* handleNativeParam, int* countNativeParam, int* isDisposableNativeParam, int* isAllocatedNativeParam)
    {
        ref nint handleNative = ref *handleNativeParam;
        ref int countNative = ref *countNativeParam;
        ref int isDisposableNative = ref *isDisposableNativeParam;
        ref int isAllocatedNative = ref *isAllocatedNativeParam;
        int retVal;
        try
        {
            // Unmarshal - Convert native data to managed data.
            ILauncherApiNews @this = ComInterfaceDispatch.GetInstance<Management.Api.ILauncherApiNews>(thisNative);
            @this.GetNewsEntries(out nint handle, out int count, out bool isDisposable, out bool isAllocated);
            // NotifyForSuccessfulInvoke - Keep alive any managed objects that need to stay alive across the call.
            retVal = 0; // S_OK
            // Marshal - Convert managed data to native data.
            isAllocatedNative = isAllocated ? 1 : 0;
            isDisposableNative = isDisposable ? 1 : 0;
            countNative = count;
            handleNative = handle;
        }
        catch (Exception exception)
        {
            retVal = Marshal.GetHRForException(exception);
        }

        return retVal;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_GetCarouselEntries(ComInterfaceDispatch* thisNative, nint* handleNativeParam, int* countNativeParam, int* isDisposableNativeParam, int* isAllocatedNativeParam)
    {
        ref nint handleNative = ref *handleNativeParam;
        ref int countNative = ref *countNativeParam;
        ref int isDisposableNative = ref *isDisposableNativeParam;
        ref int isAllocatedNative = ref *isAllocatedNativeParam;
        int retVal;
        try
        {
            // Unmarshal - Convert native data to managed data.
            ILauncherApiNews @this = ComInterfaceDispatch.GetInstance<Management.Api.ILauncherApiNews>(thisNative);
            @this.GetCarouselEntries(out nint handle, out int count, out bool isDisposable, out bool isAllocated);
            // NotifyForSuccessfulInvoke - Keep alive any managed objects that need to stay alive across the call.
            retVal = 0; // S_OK
            // Marshal - Convert managed data to native data.
            isAllocatedNative = isAllocated ? 1 : 0;
            isDisposableNative = isDisposable ? 1 : 0;
            countNative = count;
            handleNative = handle;
        }
        catch (Exception exception)
        {
            retVal = Marshal.GetHRForException(exception);
        }

        return retVal;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_GetSocialMediaEntries(ComInterfaceDispatch* thisNative, nint* handleNativeParam, int* countNativeParam, int* isDisposableNativeParam, int* isAllocatedNativeParam)
    {
        ref nint handleNative = ref *handleNativeParam;
        ref int countNative = ref *countNativeParam;
        ref int isDisposableNative = ref *isDisposableNativeParam;
        ref int isAllocatedNative = ref *isAllocatedNativeParam;
        int retVal;
        try
        {
            // Unmarshal - Convert native data to managed data.
            ILauncherApiNews @this = ComInterfaceDispatch.GetInstance<Management.Api.ILauncherApiNews>(thisNative);
            @this.GetSocialMediaEntries(out nint handle, out int count, out bool isDisposable, out bool isAllocated);
            // NotifyForSuccessfulInvoke - Keep alive any managed objects that need to stay alive across the call.
            retVal = 0; // S_OK
            // Marshal - Convert managed data to native data.
            isAllocatedNative = isAllocated ? 1 : 0;
            isDisposableNative = isDisposable ? 1 : 0;
            countNative = count;
            handleNative = handle;
        }
        catch (Exception exception)
        {
            retVal = Marshal.GetHRForException(exception);
        }

        return retVal;
    }
}

#endif