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
    public abstract string GameName { get; }
    public abstract string GameExecutableName { get; }
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
    public abstract ILauncherApiMedia? LauncherApiMedia { get; }
    public abstract ILauncherApiNews? LauncherApiNews { get; }

    #region Generic Read-only Properties Callbacks
    string IPluginPresetConfig.get_GameSupportedLanguages(int index)
    {
        if (index < 0 || index >= SupportedLanguages.Count)
        {
            return string.Empty;
        }

        return SupportedLanguages[index];
    }

    int IPluginPresetConfig.get_GameSupportedLanguagesCount() => SupportedLanguages.Count;
    string IPluginPresetConfig.get_GameExecutableName() => GameExecutableName;
    string IPluginPresetConfig.get_LauncherGameDirectoryName() => LauncherGameDirectoryName;
    string IPluginPresetConfig.get_GameName() => GameName;
    string IPluginPresetConfig.get_ProfileName() => ProfileName;
    string IPluginPresetConfig.get_ZoneDescription() => ZoneDescription;
    string IPluginPresetConfig.get_ZoneName() => ZoneName;
    string IPluginPresetConfig.get_ZoneFullName() => ZoneFullName;
    string IPluginPresetConfig.get_ZoneLogoUrl() => ZoneLogoUrl;
    string IPluginPresetConfig.get_ZonePosterUrl() => ZonePosterUrl;
    string IPluginPresetConfig.get_ZoneHomePageUrl() => ZoneHomePageUrl;
    GameReleaseChannel IPluginPresetConfig.get_ReleaseChannel() => ReleaseChannel;
    string IPluginPresetConfig.get_GameMainLanguage() => GameMainLanguage;
    #endregion

    # region Generic Read-only API Instance Callbacks
    ILauncherApiMedia? IPluginPresetConfig.get_LauncherApiMedia() => LauncherApiMedia;
    ILauncherApiNews? IPluginPresetConfig.get_LauncherApiNews() => LauncherApiNews;
    #endregion

    public virtual void Dispose()
    {
        LauncherApiMedia?.Dispose();
        LauncherApiNews?.Dispose();

        GC.SuppressFinalize(this);
    }
}
