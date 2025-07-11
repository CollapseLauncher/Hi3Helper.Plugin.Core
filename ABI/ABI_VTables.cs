#if MANUALCOM

using Hi3Helper.Plugin.Core.Management;
using Hi3Helper.Plugin.Core.Management.Api;
using Hi3Helper.Plugin.Core.Management.PresetConfig;
using Hi3Helper.Plugin.Core.Update;
using Hi3Helper.Plugin.Core.Utility;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.ComWrappers;
// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo
// ReSharper disable InconsistentNaming

namespace Hi3Helper.Plugin.Core.ABI;

internal static unsafe class ABI_VTables
{
    internal static void** IFree;
    internal static void** IInitializableTask;
    internal static void** IPlugin;
    internal static void** IPluginPresetConfig;
    internal static void** IPluginSelfUpdate;
    internal static void** ILauncherApi;
    internal static void** ILauncherApiMedia;
    internal static void** ILauncherApiNews;
    internal static void** IGameManager;
    internal static void** IGameInstaller;
    internal static void** IGameUninstaller;

    static ABI_VTables()
    {
        // Get methods for IUnknown interface query on runtime.
        GetIUnknownImpl(
            out IntPtr fpQueryInterface,
            out IntPtr fpAddRef,
            out IntPtr fpRelease);

        /* =====================================================================================================
         * NOTE FOR PLUGIN DEVELOPERS
         * =====================================================================================================
         * To find which type being used for the delegated arguments, please refer to the source-generated code
         * from GeneratedComInterface
         */

        // IFree
        IFree = (void**)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(IFree), sizeof(void*) * 4);
        {
            IFree[0] = (void*)fpQueryInterface;
            IFree[1] = (void*)fpAddRef;
            IFree[2] = (void*)fpRelease;
            IFree[3] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, int>)&ABI_IFreeWrapper.ABI_Free;
        }

        // IInitializableTask
        IInitializableTask = (void**)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(IInitializableTask), sizeof(void*) * 5);
        {
            // Inherit IFree by copying its VTable
            NativeMemory.Copy(IFree, IInitializableTask, (nuint)(sizeof(void*) * 4));
            IInitializableTask[4] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, Guid*, nint*, int>)&ABI_IInitializableTaskWrapper.ABI_InitAsync;
        }

        // IPlugin
        IPlugin = (void**)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(IPlugin), sizeof(void*) * 16);
        {
            // Inherit IFree by copying its VTable
            NativeMemory.Copy(IFree, IPlugin, (nuint)(sizeof(void*) * 4));
            IPlugin[4] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort**, int>)&ABI_IPluginWrapper.ABI_GetPluginName;
            IPlugin[5] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort**, int>)&ABI_IPluginWrapper.ABI_GetPluginDescription;
            IPlugin[6] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort**, int>)&ABI_IPluginWrapper.ABI_GetPluginAuthor;
            IPlugin[7] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, DateTime**, int>)&ABI_IPluginWrapper.ABI_GetPluginCreationDate;
            IPlugin[8] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, int*, int>)&ABI_IPluginWrapper.ABI_GetPresetConfigCount;
            IPlugin[9] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort**, int>)&ABI_IPluginWrapper.ABI_GetPluginAppIconUrl;
            IPlugin[10] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort**, int>)&ABI_IPluginWrapper.ABI_GetNotificationPosterUrl;
            IPlugin[11] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, int, void**, int>)&ABI_IPluginWrapper.ABI_GetPresetConfig;
            IPlugin[12] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, Guid*, int>)&ABI_IPluginWrapper.ABI_CancelAsync;
            IPlugin[13] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort*, ushort*, ushort*, int*, int>)&ABI_IPluginWrapper.ABI_SetPluginProxySettings;
            IPlugin[14] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort*, int>)&ABI_IPluginWrapper.ABI_SetPluginLocaleId;
            IPlugin[15] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, void**, int>)&ABI_IPluginWrapper.ABI_GetPluginSelfUpdater;
        }

        // IPluginPresetConfig
        IPluginPresetConfig = (void**)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(IPluginPresetConfig), sizeof(void*) * 27);
        {
            // Inherit IInitializableTask by copying its VTable
            NativeMemory.Copy(IInitializableTask, IPluginPresetConfig, (nuint)(sizeof(void*) * 5));
            IPluginPresetConfig[5] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort**, int>)&ABI_IPluginPresetConfigWrapper.ABI_comGet_GameName;
            IPluginPresetConfig[6] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort**, int>)&ABI_IPluginPresetConfigWrapper.ABI_comGet_ProfileName;
            IPluginPresetConfig[7] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort**, int>)&ABI_IPluginPresetConfigWrapper.ABI_comGet_ZoneDescription;
            IPluginPresetConfig[8] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort**, int>)&ABI_IPluginPresetConfigWrapper.ABI_comGet_ZoneName;
            IPluginPresetConfig[9] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort**, int>)&ABI_IPluginPresetConfigWrapper.ABI_comGet_ZoneFullName;
            IPluginPresetConfig[10] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort**, int>)&ABI_IPluginPresetConfigWrapper.ABI_comGet_ZoneLogoUrl;
            IPluginPresetConfig[11] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort**, int>)&ABI_IPluginPresetConfigWrapper.ABI_comGet_ZonePosterUrl;
            IPluginPresetConfig[12] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort**, int>)&ABI_IPluginPresetConfigWrapper.ABI_comGet_ZoneHomePageUrl;
            IPluginPresetConfig[13] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, GameReleaseChannel*, int>)&ABI_IPluginPresetConfigWrapper.ABI_comGet_ReleaseChannel;
            IPluginPresetConfig[14] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort**, int>)&ABI_IPluginPresetConfigWrapper.ABI_comGet_GameMainLanguage;
            IPluginPresetConfig[15] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, int*, int>)&ABI_IPluginPresetConfigWrapper.ABI_comGet_GameSupportedLanguagesCount;
            IPluginPresetConfig[16] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, int, ushort**, int>)&ABI_IPluginPresetConfigWrapper.ABI_comGet_GameSupportedLanguages;
            IPluginPresetConfig[17] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort**, int>)&ABI_IPluginPresetConfigWrapper.ABI_comGet_GameExecutableName;
            IPluginPresetConfig[18] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort**, int>)&ABI_IPluginPresetConfigWrapper.ABI_comGet_GameLogFileName;
            IPluginPresetConfig[19] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort**, int>)&ABI_IPluginPresetConfigWrapper.ABI_comGet_GameAppDataPath;
            IPluginPresetConfig[20] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort**, int>)&ABI_IPluginPresetConfigWrapper.ABI_comGet_LauncherGameDirectoryName;
            IPluginPresetConfig[21] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort**, int>)&ABI_IPluginPresetConfigWrapper.ABI_comGet_GameVendorName;
            IPluginPresetConfig[22] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort**, int>)&ABI_IPluginPresetConfigWrapper.ABI_comGet_GameRegistryKeyName;
            IPluginPresetConfig[23] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, void**, int>)&ABI_IPluginPresetConfigWrapper.ABI_comGet_LauncherApiMedia;
            IPluginPresetConfig[24] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, void**, int>)&ABI_IPluginPresetConfigWrapper.ABI_comGet_LauncherApiNews;
            IPluginPresetConfig[25] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, void**, int>)&ABI_IPluginPresetConfigWrapper.ABI_comGet_GameManager;
            IPluginPresetConfig[26] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, void**, int>)&ABI_IPluginPresetConfigWrapper.ABI_comGet_GameInstaller;
        }

        IPluginSelfUpdate = (void**)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(IPluginSelfUpdate), sizeof(void*) * 5);
        {
            // Inherit IFree by copying its VTable
            NativeMemory.Copy(IFree, IPluginSelfUpdate, (nuint)(sizeof(void*) * 4));
            IPluginSelfUpdate[4] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort*, int, nint, Guid*, nint*, int>)&ABI_IPluginSelfUpdateWrapper.ABI_TryPerformUpdateAsync;
        }

        // ILauncherApi
        ILauncherApi = (void**)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(ILauncherApi), sizeof(void*) * 7);
        {
            // Inherit IInitializableTask by copying its VTable
            NativeMemory.Copy(IInitializableTask, ILauncherApi, (nuint)(sizeof(void*) * 5));
            ILauncherApi[5] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, LauncherPathEntry, FileHandle, nint, Guid*, nint*, int>)&ABI_ILauncherApiWrapper.ABI_DownloadAssetAsync;
            ILauncherApi[6] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort*, FileHandle, nint, Guid*, nint*, int>)&ABI_ILauncherApiWrapper.ABI_DownloadAssetAsync;
        }

        // ILauncherApiMedia
        ILauncherApiMedia = (void**)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(ILauncherApiMedia), sizeof(void*) * 12);
        {
            // Inherit ILauncherApi by copying its VTable
            NativeMemory.Copy(ILauncherApi, ILauncherApiMedia, (nuint)(sizeof(void*) * 7));
            ILauncherApiMedia[7] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, nint*, int*, int*, int*, int>)&ABI_ILauncherApiMediaWrapper.ABI_GetBackgroundEntries;
            ILauncherApiMedia[8] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, nint*, int*, int*, int*, int>)&ABI_ILauncherApiMediaWrapper.ABI_GetLogoOverlayEntries;
            ILauncherApiMedia[9] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, LauncherBackgroundFlag*, int>)&ABI_ILauncherApiMediaWrapper.ABI_GetBackgroundFlag;
            ILauncherApiMedia[10] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, LauncherBackgroundFlag*, int>)&ABI_ILauncherApiMediaWrapper.ABI_GetLogoFlag;
            ILauncherApiMedia[11] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, float*, int>)&ABI_ILauncherApiMediaWrapper.ABI_GetBackgroundSpriteFps;
        }

        // ILauncherApiNews
        ILauncherApiNews = (void**)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(ILauncherApiNews), sizeof(void*) * 10);
        {
            // Inherit ILauncherApi by copying its VTable
            NativeMemory.Copy(ILauncherApi, ILauncherApiNews, (nuint)(sizeof(void*) * 7));
            ILauncherApiNews[7] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, nint*, int*, int*, int*, int>)&ABI_ILauncherApiNewsWrapper.ABI_GetNewsEntries;
            ILauncherApiNews[8] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, nint*, int*, int*, int*, int>)&ABI_ILauncherApiNewsWrapper.ABI_GetCarouselEntries;
            ILauncherApiNews[9] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, nint*, int*, int*, int*, int>)&ABI_ILauncherApiNewsWrapper.ABI_GetSocialMediaEntries;
        }

        // IGameManager
        IGameManager = (void**)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(IGameManager), sizeof(void*) * 17);
        {
            // Inherit IInitializableTask by copying its VTable
            NativeMemory.Copy(IInitializableTask, IGameManager, (nuint)(sizeof(void*) * 5));
            IGameManager[5] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort**, int>)&ABI_IGameManagerWrapper.ABI_GetGamePath;
            IGameManager[6] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort*, int>)&ABI_IGameManagerWrapper.ABI_SetGamePath;
            IGameManager[7] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, GameVersion*, int>)&ABI_IGameManagerWrapper.ABI_GetCurrentGameVersion;
            IGameManager[8] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, GameVersion*, int>)&ABI_IGameManagerWrapper.ABI_SetCurrentGameVersion;
            IGameManager[9] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, GameVersion*, int>)&ABI_IGameManagerWrapper.ABI_GetApiGameVersion;
            IGameManager[10] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, GameVersion*, int>)&ABI_IGameManagerWrapper.ABI_GetApiPreloadGameVersion;
            IGameManager[11] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, int*, int>)&ABI_IGameManagerWrapper.ABI_IsGameInstalled;
            IGameManager[12] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, int*, int>)&ABI_IGameManagerWrapper.ABI_IsGameHasUpdate;
            IGameManager[13] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, int*, int>)&ABI_IGameManagerWrapper.ABI_IsGameHasPreload;
            IGameManager[14] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, int>)&ABI_IGameManagerWrapper.ABI_LoadConfig;
            IGameManager[15] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, int>)&ABI_IGameManagerWrapper.ABI_SaveConfig;
            IGameManager[16] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, Guid*, nint*, int>)&ABI_IGameManagerWrapper.ABI_FindExistingInstallPathAsync;
        }

        // IGameUninstaller
        IGameUninstaller = (void**)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(IGameUninstaller), sizeof(void*) * 6);
        {
            // Inherit IInitializableTask by copying its VTable
            NativeMemory.Copy(IInitializableTask, IGameUninstaller, (nuint)(sizeof(void*) * 5));
            IGameUninstaller[5] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, Guid*, nint*, int>)&ABI_IGameUninstallerWrapper.ABI_UninstallAsync;
        }

        // IGameInstaller
        IGameInstaller = (void**)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(IGameInstaller), sizeof(void*) * 11);
        {
            // Inherit IGameUninstaller by copying its VTable
            NativeMemory.Copy(IGameUninstaller, IGameInstaller, (nuint)(sizeof(void*) * 6));
            IGameInstaller[6] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, GameInstallerKind, Guid*, nint*, int>)&ABI_IGameInstallerWrapper.ABI_GetGameSizeAsync;
            IGameInstaller[7] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, GameInstallerKind, Guid*, nint*, int>)&ABI_IGameInstallerWrapper.ABI_GetGameDownloadedSizeAsync;
            IGameInstaller[8] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, nint, nint, Guid*, nint*, int>)&ABI_IGameInstallerWrapper.ABI_StartInstallAsync;
            IGameInstaller[9] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, nint, nint, Guid*, nint*, int>)&ABI_IGameInstallerWrapper.ABI_StartUpdateAsync;
            IGameInstaller[10] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, nint, nint, Guid*, nint*, int>)&ABI_IGameInstallerWrapper.ABI_StartPreloadAsync;
        }
    }
}

#endif