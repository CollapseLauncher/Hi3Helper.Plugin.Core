#if MANUALCOM

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
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
            var @this = ComInterfaceDispatch.GetInstance<Management.Api.ILauncherApiNews>(thisNative);
            @this.GetNewsEntries(out var handle, out var count, out var isDisposable, out var isAllocated);
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
            var @this = ComInterfaceDispatch.GetInstance<Management.Api.ILauncherApiNews>(thisNative);
            @this.GetCarouselEntries(out var handle, out var count, out var isDisposable, out var isAllocated);
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
            var @this = ComInterfaceDispatch.GetInstance<Management.Api.ILauncherApiNews>(thisNative);
            @this.GetSocialMediaEntries(out var handle, out var count, out var isDisposable, out var isAllocated);
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