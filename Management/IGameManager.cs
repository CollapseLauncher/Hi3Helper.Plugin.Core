using Hi3Helper.Plugin.Core.Utility;
using System;
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
[Guid(ComInterfaceId.ExGameManager)]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public partial interface IGameManager : IInitializableTask, IDisposable
{
    /// <summary>
    /// Gets the current path where the game is installed.
    /// </summary>
    /// <param name="result">The file system path to the game installation directory.</param>
    void GetGamePath([MarshalAs(UnmanagedType.LPWStr)] out string? result);

    /// <summary>
    /// Set-only or Set-Save the path of the game installation.
    /// </summary>
    /// <param name="gamePath">The new file system path to the game installation directory.</param>
    void SetGamePath([MarshalAs(UnmanagedType.LPWStr)] string gamePath);

    /// <summary>
    /// Gets the current version of the installed game.
    /// </summary>
    /// <param name="gameVersion">An <see cref="GameVersion"/> representing the installed game version.</param>
    void GetCurrentGameVersion(out GameVersion gameVersion);

    /// <summary>
    /// Sets the current version of the installed game.
    /// </summary>
    /// <param name="version">The <see cref="IVersion"/> to set as the current game version.</param>
    void SetCurrentGameVersion(in GameVersion version);

    /// <summary>
    /// Gets the latest game version available from the API.
    /// </summary>
    /// <param name="gameVersion">An <see cref="GameVersion"/> representing the latest available game version.</param>
    void GetApiGameVersion(out GameVersion gameVersion);

    /// <summary>
    /// Gets the preload version of the game available from the API, if any.
    /// </summary>
    /// <param name="gameVersion">An <see cref="GameVersion"/> representing the preload version, or <see cref="GameVersion.Empty"/> if not available.</param>
    void GetApiPreloadGameVersion(out GameVersion gameVersion);

    /// <summary>
    /// Determines whether the game is currently installed.
    /// </summary>
    /// <param name="result"><c>true</c> if the game is installed; otherwise, <c>false</c>.</param>
    void IsGameInstalled([MarshalAs(UnmanagedType.Bool)] out bool result);

    /// <summary>
    /// Determines whether an update is available for the game.
    /// </summary>
    /// <param name="result"><c>true</c> if an update is available; otherwise, <c>false</c>.</param>
    void IsGameHasUpdate([MarshalAs(UnmanagedType.Bool)] out bool result);

    /// <summary>
    /// Determines whether a preload version is available for the game.
    /// </summary>
    /// <param name="result"><c>true</c> if a preload version is available; otherwise, <c>false</c>.</param>
    void IsGameHasPreload([MarshalAs(UnmanagedType.Bool)] out bool result);

    /// <summary>
    /// Perform config loading mechanism. Before calling this method, ensure that you have set the game path using <see cref="SetGamePath(string)"/>.
    /// </summary>
    void LoadConfig();

    /// <summary>
    /// Perform config saving mechanism. Before calling this method, ensure that you have set the game path using <see cref="SetGamePath(string)"/>.
    /// </summary>
    void SaveConfig();

    /// <summary>
    /// Finds the existing installation path of the game asynchronously.
    /// </summary>
    /// <param name="result">A safe pointer to the <see cref="ComAsyncResult"/>.</param>
    /// <param name="cancelToken">A <see cref="Guid"/> value token for the cancelling asynchronous operation.</param>
    /// <remarks>
    /// This method returns a safe pointer to the <see cref="ComAsyncResult"/> via <paramref name="result"/>.<br/>
    /// The pointer needs to be passed to <see cref="ComAsyncExtension.AsTask{T}(nint)"/> and the generic type must be <see cref="PluginDisposableMemoryMarshal"/> of <see cref="byte"/>
    /// </remarks>
    void FindExistingInstallPathAsync(in Guid cancelToken, out nint result);
}
