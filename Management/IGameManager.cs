using Hi3Helper.Plugin.Core.Utility;
using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

#pragma warning disable CA1816
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
    /// <returns>The file system path to the game installation directory.</returns>
    [return: MarshalAs(UnmanagedType.LPWStr)]
    string? GetGamePath();

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
    /// <returns><c>true</c> if the game is installed; otherwise, <c>false</c>.</returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Bool)]
    bool IsGameInstalled();

    /// <summary>
    /// Determines whether an update is available for the game.
    /// </summary>
    /// <returns><c>true</c> if an update is available; otherwise, <c>false</c>.</returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Bool)]
    bool IsGameHasUpdate();

    /// <summary>
    /// Determines whether a preload version is available for the game.
    /// </summary>
    /// <returns><c>true</c> if a preload version is available; otherwise, <c>false</c>.</returns>
    [PreserveSig]
    [return: MarshalAs(UnmanagedType.Bool)]
    bool IsGameHasPreload();

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
    /// <returns>
    /// A safe pointer to the <see cref="ComAsyncResult"/>.<br/>
    /// The pointer needs to be passed to <see cref="ComAsyncExtension.WaitFromHandle{T}(nint)"/> and the generic type must be <see cref="PluginDisposableMemoryMarshal"/> of <see cref="byte"/>
    /// </returns>
    nint FindExistingInstallPathAsync(in Guid cancelToken);

    #region DynamicInterfaceCastable Explicit Calls
    /// <inheritdoc/>
    nint IInitializableTask.InitAsync(in Guid cancelToken) => InitAsync(in cancelToken);

    /// <inheritdoc/>
    void IDisposable.Dispose() => Dispose();
    #endregion
}
