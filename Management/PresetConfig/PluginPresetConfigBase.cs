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

    /// <inheritdoc cref="IPluginPresetConfig.comGet_GameName(out string)"/>
    public abstract string? GameName { get; }

    /// <inheritdoc cref="IPluginPresetConfig.comGet_GameExecutableName(out string)"/>
    public abstract string GameExecutableName { get; }

    /// <inheritdoc cref="IPluginPresetConfig.comGet_GameAppDataPath(out string?)"/>
    public abstract string GameAppDataPath { get; }

    /// <inheritdoc cref="IPluginPresetConfig.comGet_GameLogFileName(out string?)"/>
    public abstract string GameLogFileName { get; }

    /// <inheritdoc cref="IPluginPresetConfig.comGet_GameVendorName(out string)"/>
    public abstract string GameVendorName { get; }

    /// <inheritdoc cref="IPluginPresetConfig.comGet_GameRegistryKeyName(out string)"/>
    public abstract string GameRegistryKeyName { get; }

    /// <inheritdoc cref="IPluginPresetConfig.comGet_ProfileName(out string)"/>
    public abstract string ProfileName { get; }

    /// <inheritdoc cref="IPluginPresetConfig.comGet_ZoneDescription(out string)"/>
    public abstract string ZoneDescription { get; }

    /// <inheritdoc cref="IPluginPresetConfig.comGet_ZoneName(out string)"/>
    public abstract string ZoneName { get; }

    /// <inheritdoc cref="IPluginPresetConfig.comGet_ZoneFullName(out string)"/>
    public abstract string ZoneFullName { get; }

    /// <inheritdoc cref="IPluginPresetConfig.comGet_ZoneLogoUrl(out string)"/>
    public abstract string ZoneLogoUrl { get; }

    /// <inheritdoc cref="IPluginPresetConfig.comGet_ZonePosterUrl(out string)"/>
    public abstract string ZonePosterUrl { get; }

    /// <inheritdoc cref="IPluginPresetConfig.comGet_ZoneHomePageUrl(out string)"/>
    public abstract string ZoneHomePageUrl { get; }

    /// <inheritdoc cref="IPluginPresetConfig.comGet_ReleaseChannel(out GameReleaseChannel)"/>
    public abstract GameReleaseChannel ReleaseChannel { get; }

    /// <inheritdoc cref="IPluginPresetConfig.comGet_GameMainLanguage(out string)"/>
    public abstract string GameMainLanguage { get; }

    /// <inheritdoc cref="IPluginPresetConfig.comGet_LauncherGameDirectoryName(out string)"/>
    public abstract string LauncherGameDirectoryName { get; }

    /// <inheritdoc cref="IPluginPresetConfig.comGet_GameSupportedLanguages(int, out string)"/>
    public abstract List<string> SupportedLanguages { get; }

    /// <inheritdoc cref="IPluginPresetConfig.comGet_LauncherApiMedia(out ILauncherApiMedia?)"/>
    public abstract ILauncherApiMedia? LauncherApiMedia { get; set; }

    /// <inheritdoc cref="IPluginPresetConfig.comGet_LauncherApiNews(out ILauncherApiNews?)"/>
    public abstract ILauncherApiNews? LauncherApiNews { get; set; }

    /// <inheritdoc cref="IPluginPresetConfig.comGet_GameManager(out IGameManager?)"/>
    public abstract IGameManager? GameManager { get; set; }

    /// <inheritdoc cref="IPluginPresetConfig.comGet_GameInstaller(out IGameInstaller?)"/>
    public abstract IGameInstaller? GameInstaller { get; set; }

    #region Generic Read-only Properties Callbacks
    /// <inheritdoc/>
    public void comGet_GameSupportedLanguages(int index, out string result)
    {
        if (index < 0 || index >= SupportedLanguages.Count)
        {
            result = string.Empty;
            return;
        }

        result = SupportedLanguages[index];
    }

    /// <inheritdoc/>
    public void comGet_GameSupportedLanguagesCount(out int result) => result = SupportedLanguages.Count;

    /// <inheritdoc/>
    public void comGet_GameExecutableName(out string result) => result = GameExecutableName;

    /// <inheritdoc/>
    public void comGet_GameAppDataPath(out string result) => result = GameAppDataPath;

    /// <inheritdoc/>
    public void comGet_GameLogFileName(out string result) => result = GameLogFileName;

    /// <inheritdoc/>
    public void comGet_LauncherGameDirectoryName(out string result) => result = LauncherGameDirectoryName;

    /// <inheritdoc/>
    public void comGet_GameName(out string result) => result = GameName ?? string.Empty;

    /// <inheritdoc/>
    public void comGet_ProfileName(out string result) => result = ProfileName;

    /// <inheritdoc/>
    public void comGet_ZoneDescription(out string result) => result = ZoneDescription;

    /// <inheritdoc/>
    public void comGet_ZoneName(out string result) => result = ZoneName;

    /// <inheritdoc/>
    public void comGet_ZoneFullName(out string result) => result = ZoneFullName;

    /// <inheritdoc/>
    public void comGet_ZoneLogoUrl(out string result) => result = ZoneLogoUrl;

    /// <inheritdoc/>
    public void comGet_ZonePosterUrl(out string result) => result = ZonePosterUrl;

    /// <inheritdoc/>
    public void comGet_ZoneHomePageUrl(out string result) => result = ZoneHomePageUrl;

    /// <inheritdoc/>
    public void comGet_ReleaseChannel(out GameReleaseChannel result) => result = ReleaseChannel;

    /// <inheritdoc/>
    public void comGet_GameMainLanguage(out string result) => result = GameMainLanguage;

    /// <inheritdoc/>
    public void comGet_GameVendorName(out string result) => result = GameVendorName;

    /// <inheritdoc/>
    public void comGet_GameRegistryKeyName(out string result) => result = GameRegistryKeyName;
    #endregion

    # region Generic Read-only API Instance Callbacks
    /// <inheritdoc/>
    public void comGet_LauncherApiMedia(out ILauncherApiMedia? result) => result = LauncherApiMedia;

    /// <inheritdoc/>
    public void comGet_LauncherApiNews(out ILauncherApiNews? result) => result = LauncherApiNews;

    /// <inheritdoc/>
    public void comGet_GameManager(out IGameManager? result) => result = GameManager;

    /// <inheritdoc/>
    public void comGet_GameInstaller(out IGameInstaller? result) => result = GameInstaller;
    #endregion

    /// <inheritdoc cref="IFree.Free"/>
    public override void Free() => Dispose();

    public virtual void Dispose()
    {
        if (_isDisposed) return;

        LauncherApiMedia?.Free();
        LauncherApiNews?.Free();
        GameManager?.Free();

        LauncherApiMedia = null;
        LauncherApiNews = null;
        GameManager = null;

        GC.SuppressFinalize(this);

        _isDisposed = true;
    }
}
