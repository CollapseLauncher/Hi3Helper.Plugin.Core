#if MANUALCOM

using Hi3Helper.Plugin.Core.Management;
using Hi3Helper.Plugin.Core.Management.PresetConfig;
using Hi3Helper.Plugin.Core.Utility;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.ComWrappers;

namespace Hi3Helper.Plugin.Core.ABI;

// ReSharper disable once InconsistentNaming
internal sealed unsafe class ABI_IPluginPresetConfigWrapper
{
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_comGet_GameName(ComInterfaceDispatch* thisNative, ushort** resultNativeParam)
        => ABIExtension.AllocUtf16StringFromFunc(ComInterfaceDispatch.GetInstance<IPluginPresetConfig>(thisNative).comGet_GameName, resultNativeParam);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_comGet_ProfileName(ComInterfaceDispatch* thisNative, ushort** resultNativeParam)
        => ABIExtension.AllocUtf16StringFromFunc(ComInterfaceDispatch.GetInstance<IPluginPresetConfig>(thisNative).comGet_ProfileName, resultNativeParam);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_comGet_ZoneDescription(ComInterfaceDispatch* thisNative, ushort** resultNativeParam)
        => ABIExtension.AllocUtf16StringFromFunc(ComInterfaceDispatch.GetInstance<IPluginPresetConfig>(thisNative).comGet_ZoneDescription, resultNativeParam);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_comGet_ZoneName(ComInterfaceDispatch* thisNative, ushort** resultNativeParam)
        => ABIExtension.AllocUtf16StringFromFunc(ComInterfaceDispatch.GetInstance<IPluginPresetConfig>(thisNative).comGet_ZoneName, resultNativeParam);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_comGet_ZoneFullName(ComInterfaceDispatch* thisNative, ushort** resultNativeParam)
        => ABIExtension.AllocUtf16StringFromFunc(ComInterfaceDispatch.GetInstance<IPluginPresetConfig>(thisNative).comGet_ZoneFullName, resultNativeParam);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_comGet_ZoneLogoUrl(ComInterfaceDispatch* thisNative, ushort** resultNativeParam)
        => ABIExtension.AllocUtf16StringFromFunc(ComInterfaceDispatch.GetInstance<IPluginPresetConfig>(thisNative).comGet_ZoneLogoUrl, resultNativeParam);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_comGet_ZonePosterUrl(ComInterfaceDispatch* thisNative, ushort** resultNativeParam)
        => ABIExtension.AllocUtf16StringFromFunc(ComInterfaceDispatch.GetInstance<IPluginPresetConfig>(thisNative).comGet_ZonePosterUrl, resultNativeParam);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_comGet_ZoneHomePageUrl(ComInterfaceDispatch* thisNative, ushort** resultNativeParam)
        => ABIExtension.AllocUtf16StringFromFunc(ComInterfaceDispatch.GetInstance<IPluginPresetConfig>(thisNative).comGet_ZoneHomePageUrl, resultNativeParam);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_comGet_ReleaseChannel(ComInterfaceDispatch* thisNative, GameReleaseChannel* resultNativeParam)
    {
        ref GameReleaseChannel resultNative = ref *resultNativeParam;
        int retVal;
        try
        {
            // Unmarshal - Convert native data to managed data.
            var @this = ComInterfaceDispatch.GetInstance<IPluginPresetConfig>(thisNative);
            @this.comGet_ReleaseChannel(out var result);
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
    internal static int ABI_comGet_GameMainLanguage(ComInterfaceDispatch* thisNative, ushort** resultNativeParam)
        => ABIExtension.AllocUtf16StringFromFunc(ComInterfaceDispatch.GetInstance<IPluginPresetConfig>(thisNative).comGet_GameMainLanguage, resultNativeParam);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_comGet_GameSupportedLanguagesCount(ComInterfaceDispatch* thisNative, int* resultNativeParam)
    {
        ref int resultNative = ref *resultNativeParam;
        int retVal;
        try
        {
            // Unmarshal - Convert native data to managed data.
            var @this = ComInterfaceDispatch.GetInstance<IPluginPresetConfig>(thisNative);
            @this.comGet_GameSupportedLanguagesCount(out var result);
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
    internal static int ABI_comGet_GameSupportedLanguages(ComInterfaceDispatch* thisNative, int index, ushort** resultNativeParam)
        => ABIExtension.AllocUtf16StringFromFunc((out result) => {
            ComInterfaceDispatch.GetInstance<IPluginPresetConfig>(thisNative).comGet_GameSupportedLanguages(index, out result);
        }, resultNativeParam);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_comGet_GameExecutableName(ComInterfaceDispatch* thisNative, ushort** resultNativeParam)
        => ABIExtension.AllocUtf16StringFromFunc(ComInterfaceDispatch.GetInstance<IPluginPresetConfig>(thisNative).comGet_GameExecutableName, resultNativeParam);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_comGet_GameLogFileName(ComInterfaceDispatch* thisNative, ushort** resultNativeParam)
        => ABIExtension.AllocUtf16StringFromFunc(ComInterfaceDispatch.GetInstance<IPluginPresetConfig>(thisNative).comGet_GameLogFileName, resultNativeParam);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_comGet_GameAppDataPath(ComInterfaceDispatch* thisNative, ushort** resultNativeParam)
        => ABIExtension.AllocUtf16StringFromFunc(ComInterfaceDispatch.GetInstance<IPluginPresetConfig>(thisNative).comGet_GameAppDataPath, resultNativeParam);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_comGet_LauncherGameDirectoryName(ComInterfaceDispatch* thisNative, ushort** resultNativeParam)
        => ABIExtension.AllocUtf16StringFromFunc(ComInterfaceDispatch.GetInstance<IPluginPresetConfig>(thisNative).comGet_LauncherGameDirectoryName, resultNativeParam);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_comGet_GameVendorName(ComInterfaceDispatch* thisNative, ushort** resultNativeParam)
        => ABIExtension.AllocUtf16StringFromFunc(ComInterfaceDispatch.GetInstance<IPluginPresetConfig>(thisNative).comGet_GameVendorName, resultNativeParam);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_comGet_GameRegistryKeyName(ComInterfaceDispatch* thisNative, ushort** resultNativeParam)
        => ABIExtension.AllocUtf16StringFromFunc(ComInterfaceDispatch.GetInstance<IPluginPresetConfig>(thisNative).comGet_GameRegistryKeyName, resultNativeParam);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_comGet_LauncherApiMedia(ComInterfaceDispatch* thisNative, void** resultNativeParam)
    {
        ref void* resultNative = ref *resultNativeParam;
        int retVal;
        try
        {
            // Unmarshal - Convert native data to managed data.
            var @this = ComInterfaceDispatch.GetInstance<IPluginPresetConfig>(thisNative);
            @this.comGet_LauncherApiMedia(out var result);
            // NotifyForSuccessfulInvoke - Keep alive any managed objects that need to stay alive across the call.
            retVal = 0; // S_OK
            // Marshal - Convert managed data to native data.
            resultNative = ComWrappersExtension<LauncherApiWrappers>.GetComInterfacePtrFromWrappers(result);
        }
        catch (Exception exception)
        {
            retVal = Marshal.GetHRForException(exception);
        }

        return retVal;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_comGet_LauncherApiNews(ComInterfaceDispatch* thisNative, void** resultNativeParam)
    {
        ref void* resultNative = ref *resultNativeParam;
        int retVal;
        try
        {
            // Unmarshal - Convert native data to managed data.
            var @this = ComInterfaceDispatch.GetInstance<IPluginPresetConfig>(thisNative);
            @this.comGet_LauncherApiNews(out var result);
            // NotifyForSuccessfulInvoke - Keep alive any managed objects that need to stay alive across the call.
            retVal = 0; // S_OK
            // Marshal - Convert managed data to native data.
            resultNative = ComWrappersExtension<LauncherApiWrappers>.GetComInterfacePtrFromWrappers(result);
        }
        catch (Exception exception)
        {
            retVal = Marshal.GetHRForException(exception);
        }

        return retVal;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_comGet_GameManager(ComInterfaceDispatch* thisNative, void** resultNativeParam)
    {
        ref void* resultNative = ref *resultNativeParam;
        int retVal;
        try
        {
            // Unmarshal - Convert native data to managed data.
            var @this = ComInterfaceDispatch.GetInstance<IPluginPresetConfig>(thisNative);
            @this.comGet_GameManager(out var result);
            // NotifyForSuccessfulInvoke - Keep alive any managed objects that need to stay alive across the call.
            retVal = 0; // S_OK
            // Marshal - Convert managed data to native data.
            resultNative = ComWrappersExtension<GameManagerWrappers>.GetComInterfacePtrFromWrappers(result);
        }
        catch (Exception exception)
        {
            retVal = Marshal.GetHRForException(exception);
        }

        return retVal;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_comGet_GameInstaller(ComInterfaceDispatch* thisNative, void** resultNativeParam)
    {
        ref void* resultNative = ref *resultNativeParam;
        int retVal;
        try
        {
            // Unmarshal - Convert native data to managed data.
            var @this = ComInterfaceDispatch.GetInstance<IPluginPresetConfig>(thisNative);
            @this.comGet_GameInstaller(out var result);
            // NotifyForSuccessfulInvoke - Keep alive any managed objects that need to stay alive across the call.
            retVal = 0; // S_OK
            // Marshal - Convert managed data to native data.
            resultNative = ComWrappersExtension<GameInstallerWrappers>.GetComInterfacePtrFromWrappers(result);
        }
        catch (Exception exception)
        {
            retVal = Marshal.GetHRForException(exception);
        }

        return retVal;
    }
}

#endif