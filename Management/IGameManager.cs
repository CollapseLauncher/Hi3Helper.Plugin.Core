using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Hi3Helper.Plugin.Core.Management;

/// <summary>
/// Defines the details about the current game installation, versioning, and path configuration.
/// </summary>
/// <remarks>
/// This interface is intended for retrieving game installation details, the current version of the game,
/// update status, preload status, and setting the path of the game installation location.
/// </remarks>
[GeneratedComInterface]
[Guid(ComInterfaceId.GameManager)]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public partial interface IGameManager
{
    /// <summary>
    /// Gets the current path where the game is installed.
    /// </summary>
    /// <returns>The file system path to the game installation directory.</returns>
    [return: MarshalAs(UnmanagedType.LPWStr)]
    string GetGamePath();

    /// <summary>
    /// Saves or updates the path to the game installation.
    /// </summary>
    /// <param name="gamePath">The new file system path to the game installation directory.</param>
    void SaveGamePath([MarshalAs(UnmanagedType.LPWStr)] string gamePath);

    /// <summary>
    /// Gets the current version of the installed game.
    /// </summary>
    /// <returns>An <see cref="IVersion"/> representing the installed game version.</returns>
    [return: MarshalAs(UnmanagedType.Interface)]
    IVersion GetCurrentGameVersion();

    /// <summary>
    /// Gets the latest game version available from the API.
    /// </summary>
    /// <returns>An <see cref="IVersion"/> representing the latest available game version.</returns>
    [return: MarshalAs(UnmanagedType.Interface)]
    IVersion GetApiGameVersion();

    /// <summary>
    /// Gets the preload version of the game available from the API, if any.
    /// </summary>
    /// <returns>An <see cref="IVersion"/> representing the preload version, or <c>null</c> if not available.</returns>
    [return: MarshalAs(UnmanagedType.Interface)]
    IVersion? GetApiPreloadGameVersion();

    /// <summary>
    /// Sets the current version of the installed game.
    /// </summary>
    /// <param name="version">The <see cref="IVersion"/> to set as the current game version.</param>
    void SetCurrentGameVersion([MarshalAs(UnmanagedType.Interface)] IVersion version);

    /// <summary>
    /// Determines whether the game is currently installed.
    /// </summary>
    /// <returns><c>true</c> if the game is installed; otherwise, <c>false</c>.</returns>
    [return: MarshalAs(UnmanagedType.Bool)]
    bool IsGameInstalled();

    /// <summary>
    /// Determines whether an update is available for the game.
    /// </summary>
    /// <returns><c>true</c> if an update is available; otherwise, <c>false</c>.</returns>
    [return: MarshalAs(UnmanagedType.Bool)]
    bool IsGameHasUpdate();

    /// <summary>
    /// Determines whether a preload version is available for the game.
    /// </summary>
    /// <returns><c>true</c> if a preload version is available; otherwise, <c>false</c>.</returns>
    [return: MarshalAs(UnmanagedType.Bool)]
    bool IsGameHasPreload();
}
