using Hi3Helper.Plugin.Core.Management.Api;
using Hi3Helper.Plugin.Core.Utility;
using System;
using System.Runtime.InteropServices.Marshalling;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Hi3Helper.Plugin.Core.Management;

[GeneratedComClass]
public abstract partial class GameManagerBase : LauncherApiBase, IGameManager
{
    protected virtual string? CurrentGameInstallPath { get; set; }

    protected virtual GameVersion CurrentGameVersion    { get; set; } = GameVersion.Empty;
    protected virtual GameVersion ApiGameVersion        { get; set; } = GameVersion.Empty;
    protected virtual GameVersion ApiPreloadGameVersion { get; set; } = GameVersion.Empty;

    protected abstract bool HasPreload   { get; }
    protected abstract bool HasUpdate    { get; }
    protected abstract bool IsInstalled  { get; }

    void IGameManager.GetCurrentGameVersion(out GameVersion gameVersion)     => gameVersion = CurrentGameVersion;
    void IGameManager.GetApiGameVersion(out GameVersion gameVersion)         => gameVersion = ApiGameVersion;
    void IGameManager.GetApiPreloadGameVersion(out GameVersion gameVersion)  => gameVersion = ApiPreloadGameVersion;
    void IGameManager.SetCurrentGameVersion(in GameVersion gameVersion, bool isSave)
    {
        CurrentGameVersion = gameVersion;
        SetCurrentGameVersionInner(in gameVersion, isSave);
    }
    protected abstract void SetCurrentGameVersionInner(in GameVersion gameVersion, bool isSave);

    string? IGameManager.GetGamePath() => CurrentGameInstallPath;
    void IGameManager.SetGamePath(string gamePath, bool isSave)
    {
        CurrentGameInstallPath = gamePath;
        SetGamePathInner(gamePath, isSave);
    }
    protected abstract void SetGamePathInner(string gamePath, bool isSave);

    public abstract void LoadConfig();
    public abstract void SaveConfig(bool updatePathOnly = false);

    bool IGameManager.IsGameHasPreload() => HasPreload;
    bool IGameManager.IsGameHasUpdate()  => HasUpdate;
    bool IGameManager.IsGameInstalled()  => IsInstalled;

    nint IGameManager.FindExistingInstallPathAsync(in Guid cancelToken)
    {
        CancellationTokenSource tokenSource = ComCancellationTokenVault.RegisterToken(in cancelToken);
        CancellationToken token = tokenSource.Token;
        return Impl(token).AsResult();

        async Task<PluginDisposableMemoryMarshal> Impl(CancellationToken innerToken)
        {
            string? existedPath = await FindExistingInstallPathAsyncInner(innerToken);
            if (!string.IsNullOrEmpty(existedPath))
            {
                SharedStatic.InstanceLogger?.LogTrace("[GameManagerBase::FindExistingInstallPathAsync] Found existing game installation path: {Path}", existedPath);
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
