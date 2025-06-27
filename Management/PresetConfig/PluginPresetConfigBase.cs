using Hi3Helper.Plugin.Core.Management.Api;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.Marshalling;

namespace Hi3Helper.Plugin.Core.Management.PresetConfig;

/// <summary>
/// This class is used to provide the abstract class of the plugin preset config.<br/>
/// PLEASE Do NOT use this class or return this class directly. Instead, use this 
/// class as a base class and override the properties to provide the preset config.
/// </summary>
[GeneratedComClass]
public abstract partial class PluginPresetConfigBase : InitializableTask, IPluginPresetConfig
{
    private bool _isDisposed;

    public abstract string? GameName { get; }
    public abstract string GameExecutableName { get; }
    public abstract string GameAppDataPath { get; }
    public abstract string GameLogFileName { get; }
    public abstract string GameVendorName { get; }
    public abstract string GameRegistryKeyName { get; }
    public abstract string ProfileName { get; }
    public abstract string ZoneDescription { get; }
    public abstract string ZoneName { get; }
    public abstract string ZoneFullName { get; }
    public abstract string ZoneLogoUrl { get; }
    public abstract string ZonePosterUrl { get; }
    public abstract string ZoneHomePageUrl { get; }
    public abstract GameReleaseChannel ReleaseChannel { get; }
    public abstract string GameMainLanguage { get; }
    public abstract string LauncherGameDirectoryName { get; }
    public abstract List<string> SupportedLanguages { get; }
    public abstract ILauncherApiMedia? LauncherApiMedia { get; set; }
    public abstract ILauncherApiNews? LauncherApiNews { get; set; }
    public abstract IGameManager? GameManager { get; set; }
    public abstract IGameInstaller? GameInstaller { get; set; }

    #region Generic Read-only Properties Callbacks
    public void comGet_GameSupportedLanguages(int index, out string result)
    {
        if (index < 0 || index >= SupportedLanguages.Count)
        {
            result = string.Empty;
            return;
        }

        result = SupportedLanguages[index];
    }

    public void comGet_GameSupportedLanguagesCount(out int result) => result = SupportedLanguages.Count;
    public void comGet_GameExecutableName(out string result) => result = GameExecutableName;
    public void comGet_GameAppDataPath(out string result) => result = GameAppDataPath;
    public void comGet_GameLogFileName(out string result) => result = GameLogFileName;
    public void comGet_LauncherGameDirectoryName(out string result) => result = LauncherGameDirectoryName;
    public void comGet_GameName(out string result) => result = GameName ?? string.Empty;
    public void comGet_ProfileName(out string result) => result = ProfileName;
    public void comGet_ZoneDescription(out string result) => result = ZoneDescription;
    public void comGet_ZoneName(out string result) => result = ZoneName;
    public void comGet_ZoneFullName(out string result) => result = ZoneFullName;
    public void comGet_ZoneLogoUrl(out string result) => result = ZoneLogoUrl;
    public void comGet_ZonePosterUrl(out string result) => result = ZonePosterUrl;
    public void comGet_ZoneHomePageUrl(out string result) => result = ZoneHomePageUrl;
    public void comGet_ReleaseChannel(out GameReleaseChannel result) => result = ReleaseChannel;
    public void comGet_GameMainLanguage(out string result) => result = GameMainLanguage;
    public void comGet_GameVendorName(out string result) => result = GameVendorName;
    public void comGet_GameRegistryKeyName(out string result) => result = GameRegistryKeyName;
    #endregion

    # region Generic Read-only API Instance Callbacks
    public void comGet_LauncherApiMedia(out ILauncherApiMedia? result) => result = LauncherApiMedia;
    public void comGet_LauncherApiNews(out ILauncherApiNews? result) => result = LauncherApiNews;
    public void comGet_GameManager(out IGameManager? result) => result = GameManager;
    public void comGet_GameInstaller(out IGameInstaller? result) => result = GameInstaller;
    #endregion

    public override void Free() => Dispose();

    public virtual void Dispose()
    {
        if (_isDisposed) return;

        LauncherApiMedia?.Dispose();
        LauncherApiNews?.Dispose();
        GameManager?.Dispose();

        LauncherApiMedia = null;
        LauncherApiNews = null;
        GameManager = null;

        GC.SuppressFinalize(this);

        _isDisposed = true;
    }
}
