#if MANUALCOM

using Hi3Helper.Plugin.Core.Utility;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using static System.Runtime.InteropServices.ComWrappers;

namespace Hi3Helper.Plugin.Core.ABI;

// ReSharper disable once InconsistentNaming
internal sealed unsafe class ABI_IPluginWrapper
{
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_GetPluginName(ComInterfaceDispatch* thisNative, ushort** resultNativeParam)
        => ABIExtension.AllocUtf16StringFromFunc(ComInterfaceDispatch.GetInstance<IPlugin>(thisNative).GetPluginName, resultNativeParam);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_GetPluginDescription(ComInterfaceDispatch* thisNative, ushort** resultNativeParam)
        => ABIExtension.AllocUtf16StringFromFunc(ComInterfaceDispatch.GetInstance<IPlugin>(thisNative).GetPluginDescription, resultNativeParam);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_GetPluginAuthor(ComInterfaceDispatch* thisNative, ushort** resultNativeParam)
        => ABIExtension.AllocUtf16StringFromFunc(ComInterfaceDispatch.GetInstance<IPlugin>(thisNative).GetPluginAuthor, resultNativeParam);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_GetPluginCreationDate(ComInterfaceDispatch* thisNative, DateTime** resultNativeParam)
    {
        ref DateTime* resultNative = ref *resultNativeParam;
        int retVal;
        try
        {
            // Unmarshal - Convert native data to managed data.
            var @this = ComInterfaceDispatch.GetInstance<IPlugin>(thisNative);
            @this.GetPluginCreationDate(out DateTime* result);
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
    internal static int ABI_GetPresetConfigCount(ComInterfaceDispatch* thisNative, int* countNativeParam)
    {
        ref int countNative = ref *countNativeParam;
        int retVal;
        try
        {
            // Unmarshal - Convert native data to managed data.
            var @this = ComInterfaceDispatch.GetInstance<IPlugin>(thisNative);
            @this.GetPresetConfigCount(out var count);
            // NotifyForSuccessfulInvoke - Keep alive any managed objects that need to stay alive across the call.
            retVal = 0; // S_OK
            // Marshal - Convert managed data to native data.
            countNative = count;
        }
        catch (Exception exception)
        {
            retVal = Marshal.GetHRForException(exception);
        }

        return retVal;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_GetPluginAppIconUrl(ComInterfaceDispatch* thisNative, ushort** resultNativeParam)
        => ABIExtension.AllocUtf16StringFromFunc(ComInterfaceDispatch.GetInstance<IPlugin>(thisNative).GetPluginAppIconUrl, resultNativeParam);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_GetNotificationPosterUrl(ComInterfaceDispatch* thisNative, ushort** resultNativeParam)
        => ABIExtension.AllocUtf16StringFromFunc(ComInterfaceDispatch.GetInstance<IPlugin>(thisNative).GetNotificationPosterUrl, resultNativeParam);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_GetPresetConfig(ComInterfaceDispatch* thisNative, int index, void** presetConfigNativeParam)
    {
        ref void* presetConfigNative = ref *presetConfigNativeParam;
        int retVal;
        try
        {
            // Unmarshal - Convert native data to managed data.
            var @this = ComInterfaceDispatch.GetInstance<IPlugin>(thisNative);
            @this.GetPresetConfig(index, out var presetConfig);
            // NotifyForSuccessfulInvoke - Keep alive any managed objects that need to stay alive across the call.
            retVal = 0; // S_OK
            // Marshal - Convert managed data to native data.
            presetConfigNative = ComWrappersExtension<PluginPresetConfigWrappers>.GetComInterfacePtrFromWrappers(presetConfig);
        }
        catch (Exception exception)
        {
            retVal = Marshal.GetHRForException(exception);
        }

        return retVal;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_CancelAsync(ComInterfaceDispatch* thisNative, Guid* cancelTokenNativeParam)
    {
        ref Guid cancelTokenNative = ref *cancelTokenNativeParam;
        int retVal;
        try
        {
            // Unmarshal - Convert native data to managed data.
            var cancelToken = cancelTokenNative;
            var @this = ComInterfaceDispatch.GetInstance<IPlugin>(thisNative);
            @this.CancelAsync(in cancelToken);
            // NotifyForSuccessfulInvoke - Keep alive any managed objects that need to stay alive across the call.
            retVal = 0; // S_OK
        }
        catch (Exception exception)
        {
            retVal = Marshal.GetHRForException(exception);
        }

        return retVal;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_SetPluginProxySettings(ComInterfaceDispatch* thisNative, ushort* hostUriNative, ushort* usernameNative, ushort* passwordNative, int* isSuccessNativeParam)
    {
        ref int isSuccessNative = ref *isSuccessNativeParam;
        int retVal;
        try
        {
            // Unmarshal - Convert native data to managed data.
            var password = Utf16StringMarshaller.ConvertToManaged(passwordNative);
            var username = Utf16StringMarshaller.ConvertToManaged(usernameNative);
            var hostUri = Utf16StringMarshaller.ConvertToManaged(hostUriNative);
            var @this = ComInterfaceDispatch.GetInstance<IPlugin>(thisNative);
            @this.SetPluginProxySettings(hostUri, username, password, out var isSuccess);
            // NotifyForSuccessfulInvoke - Keep alive any managed objects that need to stay alive across the call.
            retVal = 0; // S_OK
            // Marshal - Convert managed data to native data.
            isSuccessNative = isSuccess ? 1 : 0;
        }
        catch (Exception exception)
        {
            retVal = Marshal.GetHRForException(exception);
        }

        return retVal;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_SetPluginLocaleId(ComInterfaceDispatch* thisNative, ushort* localeIdNative)
    {
        int retVal;
        try
        {
            // Unmarshal - Convert native data to managed data.
            var localeId = Utf16StringMarshaller.ConvertToManaged(localeIdNative);
            var @this = ComInterfaceDispatch.GetInstance<IPlugin>(thisNative);
            @this.SetPluginLocaleId(localeId);
            // NotifyForSuccessfulInvoke - Keep alive any managed objects that need to stay alive across the call.
            retVal = 0; // S_OK
        }
        catch (Exception exception)
        {
            retVal = Marshal.GetHRForException(exception);
        }

        return retVal;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_GetPluginSelfUpdater(ComInterfaceDispatch* thisNative, void** selfUpdateNativeParam)
    {
        ref void* selfUpdateNative = ref *selfUpdateNativeParam;
        int retVal;
        try
        {
            // Unmarshal - Convert native data to managed data.
            var @this = ComInterfaceDispatch.GetInstance<IPlugin>(thisNative);
            @this.GetPluginSelfUpdater(out var selfUpdate);
            // NotifyForSuccessfulInvoke - Keep alive any managed objects that need to stay alive across the call.
            retVal = 0; // S_OK
            // Marshal - Convert managed data to native data.
            selfUpdateNative = ComWrappersExtension<PluginSelfUpdateWrappers>.GetComInterfacePtrFromWrappers(selfUpdate);
        }
        catch (Exception exception)
        {
            retVal = Marshal.GetHRForException(exception);
        }

        return retVal;
    }
}

#endif