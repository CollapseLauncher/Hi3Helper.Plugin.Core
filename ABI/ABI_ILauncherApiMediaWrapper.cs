#if MANUALCOM

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Hi3Helper.Plugin.Core.ABI;

// ReSharper disable once InconsistentNaming
internal sealed unsafe class ABI_ILauncherApiMediaWrapper
{
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_GetBackgroundEntries(ComWrappers.ComInterfaceDispatch* thisNative, nint* handleNativeParam, int* countNativeParam, int* isDisposableNativeParam, int* isAllocatedNativeParam)
    {
        ref nint handleNative = ref *handleNativeParam;
        ref int countNative = ref *countNativeParam;
        ref int isDisposableNative = ref *isDisposableNativeParam;
        ref int isAllocatedNative = ref *isAllocatedNativeParam;
        int retVal;
        try
        {
            // Unmarshal - Convert native data to managed data.
            var @this = ComWrappers.ComInterfaceDispatch.GetInstance<Management.Api.ILauncherApiMedia>(thisNative);
            @this.GetBackgroundEntries(out var handle, out var count, out var isDisposable, out var isAllocated);
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
    internal static int ABI_GetLogoOverlayEntries(ComWrappers.ComInterfaceDispatch* thisNative, nint* handleNativeParam, int* countNativeParam, int* isDisposableNativeParam, int* isAllocatedNativeParam)
    {
        ref nint handleNative = ref *handleNativeParam;
        ref int countNative = ref *countNativeParam;
        ref int isDisposableNative = ref *isDisposableNativeParam;
        ref int isAllocatedNative = ref *isAllocatedNativeParam;
        int retVal;
        try
        {
            // Unmarshal - Convert native data to managed data.
            var @this = ComWrappers.ComInterfaceDispatch.GetInstance<Management.Api.ILauncherApiMedia>(thisNative);
            @this.GetLogoOverlayEntries(out var handle, out var count, out var isDisposable, out var isAllocated);
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
    internal static int ABI_GetBackgroundFlag(ComWrappers.ComInterfaceDispatch* thisNative, Management.Api.LauncherBackgroundFlag* resultNativeParam)
    {
        ref Management.Api.LauncherBackgroundFlag resultNative = ref *resultNativeParam;
        int retVal;
        try
        {
            // Unmarshal - Convert native data to managed data.
            var @this = ComWrappers.ComInterfaceDispatch.GetInstance<Management.Api.ILauncherApiMedia>(thisNative);
            @this.GetBackgroundFlag(out var result);
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

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_GetLogoFlag(ComWrappers.ComInterfaceDispatch* thisNative, Management.Api.LauncherBackgroundFlag* resultNativeParam)
    {
        ref Management.Api.LauncherBackgroundFlag resultNative = ref *resultNativeParam;
        int retVal;
        try
        {
            // Unmarshal - Convert native data to managed data.
            var @this = ComWrappers.ComInterfaceDispatch.GetInstance<Management.Api.ILauncherApiMedia>(thisNative);
            @this.GetLogoFlag(out var result);
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

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_GetBackgroundSpriteFps(ComWrappers.ComInterfaceDispatch* thisNative, float* resultNativeParam)
    {
        ref float resultNative = ref *resultNativeParam;
        int retVal;
        try
        {
            // Unmarshal - Convert native data to managed data.
            var @this = ComWrappers.ComInterfaceDispatch.GetInstance<Management.Api.ILauncherApiMedia>(thisNative);
            @this.GetBackgroundSpriteFps(out var result);
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