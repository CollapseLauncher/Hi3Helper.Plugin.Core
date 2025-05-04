using System.Collections.Generic;

namespace Hi3Helper.Plugin.Core.Management;

public interface IPluginPresetConfig
{
    #region Generic Read-only Properties
    public string             GameName        { get; }
    public string             ProfileName     { get; }
    public string             ZoneDescription { get; }
    public string             ZoneShortName   { get; }
    public string             ZoneFullName    { get; }
    public string             ZoneLogoUrl     { get; }
    public string             ZonePosterUrl   { get; }
    public string             ZoneHomePageUrl { get; }
    public GameReleaseChannel ReleaseChannel  { get; }

    public string         GameMainLanguage       { get; }
    public IList<string>? GameSupportedLanguages { get; }

    public string  GameExecutableName        { get; }
    public string? LauncherGameDirectoryName { get; }
    #endregion
}
