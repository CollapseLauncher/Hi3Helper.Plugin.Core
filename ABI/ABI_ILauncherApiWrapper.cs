#if MANUALCOM

using Hi3Helper.Plugin.Core.Management.Api;
using Hi3Helper.Plugin.Core.Utility;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using static System.Runtime.InteropServices.ComWrappers;

namespace Hi3Helper.Plugin.Core.ABI;

// ReSharper disable once InconsistentNaming
internal sealed unsafe class ABI_ILauncherApiWrapper
{
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_DownloadAssetAsync(ComInterfaceDispatch* thisNative, LauncherPathEntry entry, nint outputStreamHandle, nint downloadProgressNative, Guid* cancelTokenNativeParam, nint* resultNativeParam)
    {
        ref Guid cancelTokenNative = ref *cancelTokenNativeParam;
        ref nint resultNative = ref *resultNativeParam;
        int retVal = 0;
        try
        {
            // Unmarshal - Convert native data to managed data.
            var cancelToken = cancelTokenNative;
            var downloadProgress = downloadProgressNative != 0 ? Marshal.GetDelegateForFunctionPointer<PluginFiles.FileReadProgressDelegate>(downloadProgressNative) : null;
            var @this = ComInterfaceDispatch.GetInstance<ILauncherApi>(thisNative);
            @this.DownloadAssetAsync(entry, outputStreamHandle, downloadProgress, in cancelToken, out var result);
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
    internal static int ABI_DownloadAssetAsync(ComInterfaceDispatch* thisNative, ushort* fileUrlNative, nint outputStreamHandle, nint downloadProgressNative, Guid* cancelTokenNativeParam, nint* resultNativeParam)
    {
        ref Guid cancelTokenNative = ref *cancelTokenNativeParam;
        ref nint resultNative = ref *resultNativeParam;
        int retVal = 0;
        try
        {
            // Unmarshal - Convert native data to managed data.
            var cancelToken = cancelTokenNative;
            var downloadProgress = downloadProgressNative != 0 ? Marshal.GetDelegateForFunctionPointer<PluginFiles.FileReadProgressDelegate>(downloadProgressNative) : null;
            var fileUrl = Utf16StringMarshaller.ConvertToManaged(fileUrlNative);
            var @this = ComInterfaceDispatch.GetInstance<ILauncherApi>(thisNative);
            @this.DownloadAssetAsync(fileUrl, outputStreamHandle, downloadProgress, in cancelToken, out var result);
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