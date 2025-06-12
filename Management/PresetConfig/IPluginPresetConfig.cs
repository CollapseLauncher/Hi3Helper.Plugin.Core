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
public partial interface IPluginPresetConfig : IInitializableTask, IDisposable
{
    #region Generic Read-only Properties
    /// <summary>
    /// Gets the name of the game.
    /// </summary>
    /// <returns>The game name as a string.</returns>
    [return: MarshalAs(UnmanagedType.LPWStr)]
    string get_GameName();

    /// <summary>
    /// Gets the name of the profile.
    /// </summary>
    /// <returns>The profile name as a string.</returns>
    [return: MarshalAs(UnmanagedType.LPWStr)]
    string get_ProfileName();

    /// <summary>
    /// Gets the description of the zone.
    /// </summary>
    /// <returns>The zone description as a string.</returns>
    [return: MarshalAs(UnmanagedType.LPWStr)]
    string get_ZoneDescription();

    /// <summary>
    /// Gets the short name of the zone.
    /// </summary>
    /// <returns>The zone name as a string.</returns>
    [return: MarshalAs(UnmanagedType.LPWStr)]
    string get_ZoneName();

    /// <summary>
    /// Gets the full name of the zone.
    /// </summary>
    /// <returns>The zone full name as a string.</returns>
    [return: MarshalAs(UnmanagedType.LPWStr)]
    string get_ZoneFullName();

    /// <summary>
    /// Gets the URL of the zone's logo.
    /// </summary>
    /// <returns>The zone logo URL as a string.</returns>
    [return: MarshalAs(UnmanagedType.LPWStr)]
    string get_ZoneLogoUrl();

    /// <summary>
    /// Gets the URL of the zone's poster.
    /// </summary>
    /// <returns>The zone poster URL as a string.</returns>
    [return: MarshalAs(UnmanagedType.LPWStr)]
    string get_ZonePosterUrl();

    /// <summary>
    /// Gets the URL of the zone's home page.
    /// </summary>
    /// <returns>The zone home page URL as a string.</returns>
    [return: MarshalAs(UnmanagedType.LPWStr)]
    string get_ZoneHomePageUrl();

    /// <summary>
    /// Gets the release channel of the game.
    /// </summary>
    /// <returns>The <see cref="GameReleaseChannel"/> value.</returns>
    [PreserveSig]
    GameReleaseChannel get_ReleaseChannel();

    /// <summary>
    /// Gets the main language of the game.
    /// </summary>
    /// <returns>The main language as a string.</returns>
    [return: MarshalAs(UnmanagedType.LPWStr)]
    string get_GameMainLanguage();

    /// <summary>
    /// Gets the number of supported languages for the game.
    /// </summary>
    /// <returns>The count of supported languages as an integer.</returns>
    [PreserveSig]
    int get_GameSupportedLanguagesCount();

    /// <summary>
    /// Gets the supported language at the specified index.
    /// </summary>
    /// <param name="index">The index of the supported language.</param>
    /// <returns>The 2-letter locale language code (such as: en) as a string.</returns>
    [return: MarshalAs(UnmanagedType.LPWStr)]
    string get_GameSupportedLanguages(int index);

    /// <summary>
    /// Gets the name of the game's executable file.
    /// </summary>
    /// <returns>The executable name as a string.</returns>
    [return: MarshalAs(UnmanagedType.LPWStr)]
    string get_GameExecutableName();

    /// <summary>
    /// Gets the file name of the game's log file.
    /// </summary>
    /// <returns>The filename or path of the log file.</returns>
    [return: MarshalAs(UnmanagedType.LPWStr)]
    string? get_GameLogFileName();

    /// <summary>
    /// Gets the path where the game's cache data is stored (usually, it's used as well as to store the Game's Log File for Unity games),
    /// </summary>
    /// <returns>The path of the game's cache directory.</returns>
    [return : MarshalAs(UnmanagedType.LPWStr)]
    string? get_GameAppDataPath();

    /// <summary>
    /// Gets the directory name used by the launcher for the game.
    /// </summary>
    /// <returns>The launcher game directory name as a string.</returns>
    [return: MarshalAs(UnmanagedType.LPWStr)]
    string get_LauncherGameDirectoryName();

    /// <summary>
    /// Retrieves the name of the game vendor.
    /// </summary>
    /// <returns>The game vendor name.</returns>
    [return: MarshalAs(UnmanagedType.LPWStr)]
    string get_GameVendorName();

    /// <summary>
    /// Retrieves the registry key name for the game.
    /// </summary>
    /// <returns>The registry key name for the game.</returns>
    [return: MarshalAs(UnmanagedType.LPWStr)]
    string get_GameRegistryKeyName();

    /// <summary>
    /// Gets an instance to the <see cref="ILauncherApiMedia"/>.
    /// </summary>
    /// <returns>The launcher Media API instance.</returns>
    [return: MarshalAs(UnmanagedType.Interface)]
    ILauncherApiMedia? get_LauncherApiMedia();

    /// <summary>
    /// Gets an instance to the <see cref="ILauncherApiNews"/>.
    /// </summary>
    /// <returns>The launcher News API instance.</returns>
    [return: MarshalAs(UnmanagedType.Interface)]
    ILauncherApiNews? get_LauncherApiNews();

    /// <summary>
    /// Gets an instance to the <see cref="IGameManager"/>
    /// </summary>
    /// <returns>The game manager instance.</returns>
    [return: MarshalAs(UnmanagedType.Interface)]
    IGameManager? get_GameManager();

    /// <summary>
    /// Gets an instance to the <see cref="IGameInstaller"/> and <see cref="IGameUninstaller"/>
    /// </summary>
    /// <returns>The game installer and uninstaller instance.</returns>
    [return: MarshalAs(UnmanagedType.Interface)]
    IGameInstaller? get_GameInstaller();
    #endregion
}
