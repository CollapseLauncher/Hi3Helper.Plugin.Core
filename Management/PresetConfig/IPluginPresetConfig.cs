using System;
using Hi3Helper.Plugin.Core.Management.Api;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Hi3Helper.Plugin.Core.Management.PresetConfig;

/// <summary>
/// Defines a COM interface for accessing game preset configuration data.
/// </summary>
/// <remarks>
/// This interface provides methods to retrieve various properties related to a game's preset configuration,
/// such as game and zone information, supported languages, and executable details.
/// It extends <see cref="IInitializableTask"/> to support asynchronous initialization.
/// </remarks>
[GeneratedComInterface]
[Guid(ComInterfaceId.ExPluginPresetConfig)]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public partial interface IPluginPresetConfig : IInitializableTask
{
    #region Generic Read-only Properties
    /// <summary>
    /// Gets the name of the game.
    /// </summary>
    /// <param name="result">The game name as a string.</param>
    void comGet_GameName([MarshalAs(UnmanagedType.LPWStr)] out string result);

    /// <summary>
    /// Gets the name of the profile.
    /// </summary>
    /// <param name="result">The profile name as a string.</param>
    void comGet_ProfileName([MarshalAs(UnmanagedType.LPWStr)] out string result);

    /// <summary>
    /// Gets the description of the zone.
    /// </summary>
    /// <param name="result">The zone description as a string.</param>
    void comGet_ZoneDescription([MarshalAs(UnmanagedType.LPWStr)] out string result);

    /// <summary>
    /// Gets the short name of the zone.
    /// </summary>
    /// <param name="result">The zone name as a string.</param>
    void comGet_ZoneName([MarshalAs(UnmanagedType.LPWStr)] out string result);

    /// <summary>
    /// Gets the full name of the zone.
    /// </summary>
    /// <param name="result">The zone full name as a string.</param>
    void comGet_ZoneFullName([MarshalAs(UnmanagedType.LPWStr)] out string result);

    /// <summary>
    /// Gets the URL of the zone's logo.
    /// </summary>
    /// <param name="result">The zone logo URL as a string.</param>
    void comGet_ZoneLogoUrl([MarshalAs(UnmanagedType.LPWStr)] out string result);

    /// <summary>
    /// Gets the URL of the zone's poster.
    /// </summary>
    /// <param name="result">The zone poster URL as a string.</param>
    void comGet_ZonePosterUrl([MarshalAs(UnmanagedType.LPWStr)] out string result);

    /// <summary>
    /// Gets the URL of the zone's home page.
    /// </summary>
    /// <param name="result">The zone home page URL as a string.</param>
    void comGet_ZoneHomePageUrl([MarshalAs(UnmanagedType.LPWStr)] out string result);

    /// <summary>
    /// Gets the release channel of the game.
    /// </summary>
    /// <param name="result">The <see cref="GameReleaseChannel"/> value.</param>
    void comGet_ReleaseChannel(out GameReleaseChannel result);

    /// <summary>
    /// Gets the main language of the game.
    /// </summary>
    /// <param name="result">The main language as a string.</param>
    void comGet_GameMainLanguage([MarshalAs(UnmanagedType.LPWStr)] out string result);

    /// <summary>
    /// Gets the number of supported languages for the game.
    /// </summary>
    /// <param name="result">The count of supported languages as an integer.</param>
    void comGet_GameSupportedLanguagesCount(out int result);

    /// <summary>
    /// Gets the supported language at the specified index.
    /// </summary>
    /// <param name="index">The index of the supported language.</param>
    /// <param name="result">The 2-letter locale language code (such as: en) as a string.</param>
    void comGet_GameSupportedLanguages(int index, [MarshalAs(UnmanagedType.LPWStr)] out string result);

    /// <summary>
    /// Gets the name of the game's executable file.
    /// </summary>
    /// <param name="result">The executable name as a string.</param>
    void comGet_GameExecutableName([MarshalAs(UnmanagedType.LPWStr)] out string result);

    /// <summary>
    /// Gets the file name of the game's log file.
    /// </summary>
    /// <param name="result">The filename or path of the log file.</param>
    void comGet_GameLogFileName([MarshalAs(UnmanagedType.LPWStr)] out string? result);

    /// <summary>
    /// Gets the path where the game's cache data is stored (usually, it's used as well as to store the Game's Log File for Unity games),
    /// </summary>
    /// <param name="result">The path of the game's cache directory.</param>
    void comGet_GameAppDataPath([MarshalAs(UnmanagedType.LPWStr)] out string? result);

    /// <summary>
    /// Gets the directory name used by the launcher for the game.
    /// </summary>
    /// <param name="result">The launcher game directory name as a string.</param>
    void comGet_LauncherGameDirectoryName([MarshalAs(UnmanagedType.LPWStr)] out string result);

    /// <summary>
    /// Retrieves the name of the game vendor.
    /// </summary>
    /// <param name="result">The game vendor name.</param>
    void comGet_GameVendorName([MarshalAs(UnmanagedType.LPWStr)] out string result);

    /// <summary>
    /// Retrieves the registry key name for the game.
    /// </summary>
    /// <param name="result">The registry key name for the game.</param>
    void comGet_GameRegistryKeyName([MarshalAs(UnmanagedType.LPWStr)] out string result);

    /// <summary>
    /// Gets an instance to the <see cref="ILauncherApiMedia"/>.
    /// </summary>
    /// <param name="result">The launcher Media API instance.</param>
    void comGet_LauncherApiMedia([MarshalAs(UnmanagedType.Interface)] out ILauncherApiMedia? result);

    /// <summary>
    /// Gets an instance to the <see cref="ILauncherApiNews"/>.
    /// </summary>
    /// <param name="result">The launcher News API instance.</param>
    void comGet_LauncherApiNews([MarshalAs(UnmanagedType.Interface)] out ILauncherApiNews? result);

    /// <summary>
    /// Gets an instance to the <see cref="IGameManager"/>
    /// </summary>
    /// <param name="result">The game manager instance.</param>
    void comGet_GameManager([MarshalAs(UnmanagedType.Interface)] out IGameManager? result);

    /// <summary>
    /// Gets an instance to the <see cref="IGameInstaller"/> and <see cref="IGameUninstaller"/>
    /// </summary>
    /// <param name="result">The game installer and uninstaller instance.</param>
    void comGet_GameInstaller([MarshalAs(UnmanagedType.Interface)] out IGameInstaller? result);
    #endregion
}
