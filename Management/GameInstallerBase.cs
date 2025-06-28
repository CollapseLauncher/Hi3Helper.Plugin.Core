using Hi3Helper.Plugin.Core.Utility;
using System;
using System.Runtime.InteropServices.Marshalling;
using System.Threading;
using System.Threading.Tasks;

namespace Hi3Helper.Plugin.Core.Management;

[GeneratedComClass]
public abstract partial class GameInstallerBase(IGameManager? gameManager) : InitializableTask, IGameInstaller
{
    protected readonly IGameManager GameManager = gameManager ?? throw new NullReferenceException("Game Manager is null!");

    public void GetGameSizeAsync(GameInstallerKind gameInstallerKind, in Guid cancelToken, out nint result)
    {
        CancellationTokenSource tokenSource = ComCancellationTokenVault.RegisterToken(in cancelToken);
        CancellationToken token = tokenSource.Token;
        result = GetGameSizeAsyncInner(gameInstallerKind, token).AsResult();
    }

    public void GetGameDownloadedSizeAsync(GameInstallerKind gameInstallerKind, in Guid cancelToken, out nint result)
    {
        CancellationTokenSource tokenSource = ComCancellationTokenVault.RegisterToken(in cancelToken);
        CancellationToken token = tokenSource.Token;
        result = GetGameDownloadedSizeAsyncInner(gameInstallerKind, token).AsResult();
    }

    public void StartInstallAsync(InstallProgressDelegate? progressDelegate, InstallProgressStateDelegate? progressStateDelegate, in Guid cancelToken, out nint result)
    {
        CancellationTokenSource tokenSource = ComCancellationTokenVault.RegisterToken(in cancelToken);
        CancellationToken token = tokenSource.Token;
        result = StartInstallAsyncInner(progressDelegate, progressStateDelegate, token).AsResult();
    }

    public void StartUpdateAsync(InstallProgressDelegate? progressDelegate, InstallProgressStateDelegate? progressStateDelegate, in Guid cancelToken, out nint result)
    {
        CancellationTokenSource tokenSource = ComCancellationTokenVault.RegisterToken(in cancelToken);
        CancellationToken token = tokenSource.Token;
        result = StartUpdateAsyncInner(progressDelegate, progressStateDelegate, token).AsResult();
    }

    public void StartPreloadAsync(InstallProgressDelegate? progressDelegate, InstallProgressStateDelegate? progressStateDelegate, in Guid cancelToken, out nint result)
    {
        CancellationTokenSource tokenSource = ComCancellationTokenVault.RegisterToken(in cancelToken);
        CancellationToken token = tokenSource.Token;
        result = StartPreloadAsyncInner(progressDelegate, progressStateDelegate, token).AsResult();
    }

    public void UninstallAsync(in Guid cancelToken, out nint result)
    {
        CancellationTokenSource tokenSource = ComCancellationTokenVault.RegisterToken(in cancelToken);
        CancellationToken token = tokenSource.Token;
        result = UninstallAsyncInner(token).AsResult();
    }

    protected abstract Task<long> GetGameSizeAsyncInner(GameInstallerKind gameInstallerKind, CancellationToken token);
    protected abstract Task<long> GetGameDownloadedSizeAsyncInner(GameInstallerKind gameInstallerKind, CancellationToken token);
    protected abstract Task StartInstallAsyncInner(InstallProgressDelegate? progressDelegate, InstallProgressStateDelegate? progressStateDelegate, CancellationToken token);
    protected abstract Task StartUpdateAsyncInner(InstallProgressDelegate? progressDelegate, InstallProgressStateDelegate? progressStateDelegate, CancellationToken token);
    protected abstract Task StartPreloadAsyncInner(InstallProgressDelegate? progressDelegate, InstallProgressStateDelegate? progressStateDelegate, CancellationToken token);
    protected abstract Task UninstallAsyncInner(CancellationToken token);

    protected string EnsureAndGetGamePath()
    {
        GameManager.GetGamePath(out string? gamePath);
        if (string.IsNullOrEmpty(gamePath))
        {
            throw new InvalidOperationException("To Developer: Please set the game path using IGameManager.SetGamePath(gamePath, false) before starting StartInstallAsync!");
        }

        return gamePath;
    }

    public override void Free() => Dispose();

    public abstract void Dispose();
}
