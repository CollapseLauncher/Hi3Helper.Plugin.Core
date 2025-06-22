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

    public abstract string GameName { get; }
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
    public string comGet_GameSupportedLanguages(int index)
    {
        if (index < 0 || index >= SupportedLanguages.Count)
        {
            return string.Empty;
        }

        return SupportedLanguages[index];
    }

    public int    comGet_GameSupportedLanguagesCount() => SupportedLanguages.Count;
    public string comGet_GameExecutableName() => GameExecutableName;
    public string comGet_GameAppDataPath() => GameAppDataPath;
    public string comGet_GameLogFileName() => GameLogFileName;
    public string comGet_LauncherGameDirectoryName() => LauncherGameDirectoryName;
    public string comGet_GameName() => GameName;
    public string comGet_ProfileName() => ProfileName;
    public string comGet_ZoneDescription() => ZoneDescription;
    public string comGet_ZoneName() => ZoneName;
    public string comGet_ZoneFullName() => ZoneFullName;
    public string comGet_ZoneLogoUrl() => ZoneLogoUrl;
    public string comGet_ZonePosterUrl() => ZonePosterUrl;
    public string comGet_ZoneHomePageUrl() => ZoneHomePageUrl;
    public GameReleaseChannel comGet_ReleaseChannel() => ReleaseChannel;
    public string comGet_GameMainLanguage() => GameMainLanguage;
    public string comGet_GameVendorName() => GameVendorName;
    public string comGet_GameRegistryKeyName() => GameRegistryKeyName;
    #endregion

    # region Generic Read-only API Instance Callbacks
    public ILauncherApiMedia? comGet_LauncherApiMedia() => LauncherApiMedia;
    public ILauncherApiNews? comGet_LauncherApiNews() => LauncherApiNews;
    public IGameManager? comGet_GameManager() => GameManager;
    public IGameInstaller? comGet_GameInstaller() => GameInstaller;
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
