using Hi3Helper.Plugin.Core.Management.Api;
using Hi3Helper.Plugin.Core.Utility;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Runtime.InteropServices.Marshalling;
using System.Threading;
using System.Threading.Tasks;

namespace Hi3Helper.Plugin.Core.Management;

[GeneratedComClass]
public abstract partial class GameManagerBase : LauncherApiBase, IGameManager
{
    /// <summary>
    /// Gets the current game installation path.
    /// </summary>
    protected virtual string? CurrentGameInstallPath { get; set; }

    /// <summary>
    /// Gets the current version of installed game.
    /// </summary>
    /// <remarks>
    /// The property should expect to return <see cref="GameVersion.Empty"/> if the game isn't installed or the version isn't set.
    /// </remarks>
    protected virtual GameVersion CurrentGameVersion { get; set; } = GameVersion.Empty;

    /// <summary>
    /// Gets the current version of available game on the API.
    /// </summary>
    /// <remarks>
    /// The property should expect to NEVER return <see cref="GameVersion.Empty"/>.
    /// </remarks>
    protected virtual GameVersion ApiGameVersion { get; set; } = GameVersion.Empty;

    /// <summary>
    /// Gets the upcoming version of available game on the API.
    /// </summary>
    /// <remarks>
    /// The property should expect to return <see cref="GameVersion.Empty"/> if preload/upcoming game version isn't available on the API.
    /// </remarks>
    protected virtual GameVersion ApiPreloadGameVersion { get; set; } = GameVersion.Empty;

    /// <summary>
    /// Gets whether the preload/upcoming game version is available on the API.
    /// </summary>
    /// <remarks>
    /// The property should expect to follow this logic formula in order to return the exact value:<br/>
    /// <see cref="IsInstalled"/> == <c>true</c> AND<br/>
    /// <see cref="ApiPreloadGameVersion"/> != <see cref="GameVersion.Empty"/>
    /// </remarks>
    protected abstract bool HasPreload { get; }

    /// <summary>
    /// Gets whether the game update is available on the API.
    /// </summary>
    /// <remarks>
    /// The property should expect to follow this logic formula in order to return the exact value:<br/>
    /// <see cref="IsInstalled"/> == <c>true</c> AND<br/>
    /// <see cref="CurrentGameVersion"/> != <see cref="ApiGameVersion"/>
    /// </remarks>
    protected abstract bool HasUpdate { get; }

    /// <summary>
    /// Gets whether the game is currently installed.
    /// </summary>
    /// <remarks>
    /// The property should expect to follow this logic formula in order to return the exact value:<br/>
    /// <see cref="string.IsNullOrEmpty"/>(<see cref="CurrentGameInstallPath"/>) != <c>true</c> AND<br/>
    /// <see cref="CurrentGameVersion"/> != <see cref="GameVersion.Empty"/> AND<br/>
    /// <see cref="File.Exists"/>(Game Executable Path) AND<br/>
    /// So-on...
    /// </remarks>
    protected abstract bool IsInstalled { get; }

    /// <inheritdoc/>
    public void GetCurrentGameVersion(out GameVersion gameVersion) => gameVersion = CurrentGameVersion;

    /// <inheritdoc/>
    public void GetApiGameVersion(out GameVersion gameVersion) => gameVersion = ApiGameVersion;

    /// <inheritdoc/>
    public void GetApiPreloadGameVersion(out GameVersion gameVersion) => gameVersion = ApiPreloadGameVersion;

    /// <inheritdoc/>
    public void SetCurrentGameVersion(in GameVersion gameVersion)
    {
        CurrentGameVersion = gameVersion;
        SetCurrentGameVersionInner(in gameVersion);
    }

    /// <inheritdoc cref="IGameManager.SetCurrentGameVersion(in GameVersion)"/>
    protected abstract void SetCurrentGameVersionInner(in GameVersion gameVersion);


    /// <inheritdoc/>
    public void GetGamePath(out string? result) => result = CurrentGameInstallPath;

    /// <inheritdoc/>
    public void SetGamePath(string gamePath)
    {
        CurrentGameInstallPath = gamePath;
        SetGamePathInner(gamePath);
    }

    /// <inheritdoc cref="IGameManager.SetGamePath(string)"/>
    protected abstract void SetGamePathInner(string gamePath);

    /// <inheritdoc/>
    public abstract void LoadConfig();

    /// <inheritdoc/>
    public abstract void SaveConfig();


    /// <inheritdoc/>
    public void IsGameHasPreload(out bool result) => result = HasPreload;

    /// <inheritdoc/>
    public void IsGameHasUpdate(out bool result) => result = HasUpdate;

    /// <inheritdoc/>
    public void IsGameInstalled(out bool result) => result = IsInstalled;

    /// <inheritdoc/>
    public void FindExistingInstallPathAsync(in Guid cancelToken, out nint result)
    {
        CancellationTokenSource tokenSource = ComCancellationTokenVault.RegisterToken(in cancelToken);
        CancellationToken token = tokenSource.Token;
        result = Impl(token).AsResult();

        return;

        async Task<PluginDisposableMemoryMarshal> Impl(CancellationToken innerToken)
        {
            string? existedPath = await FindExistingInstallPathAsyncInner(innerToken);
            if (!string.IsNullOrEmpty(existedPath))
            {
                SharedStatic.InstanceLogger.LogTrace("[GameManagerBase::FindExistingInstallPathAsync] Found existing game installation path: {Path}", existedPath);
            }

            return existedPath;
        }
    }

    /// <summary>
    /// Find the existing installation path of the game asynchronously.
    /// </summary>
    /// <param name="token">Cancel token for async operations.</param>
    /// <returns>Returns <c>null</c> if none was found. Otherwise, returns the path of the directory which contains the main game executable.</returns>
    protected abstract Task<string?> FindExistingInstallPathAsyncInner(CancellationToken token);
}
