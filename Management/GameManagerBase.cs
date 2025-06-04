using Hi3Helper.Plugin.Core.Management.Api;
using System.Runtime.InteropServices.Marshalling;

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
    public abstract void SaveConfig();

    bool IGameManager.IsGameHasPreload() => HasPreload;
    bool IGameManager.IsGameHasUpdate()  => HasUpdate;
    bool IGameManager.IsGameInstalled()  => IsInstalled;
}
