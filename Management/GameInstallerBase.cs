using Hi3Helper.Plugin.Core.Utility;
using System;
using System.Runtime.InteropServices.Marshalling;
using System.Threading;
using System.Threading.Tasks;

namespace Hi3Helper.Plugin.Core.Management;

[GeneratedComClass]
public abstract partial class GameInstallerBase(IGameManager? gameManager) : InitializableTask, IGameInstaller, IGameUninstaller
{
    protected readonly IGameManager GameManager = gameManager ?? throw new NullReferenceException("Game Manager is null!");

    public nint GetGameSizeAsync(GameInstallerKind gameInstallerKind, in Guid cancelToken)
    {
        CancellationTokenSource tokenSource = ComCancellationTokenVault.RegisterToken(in cancelToken);
        CancellationToken token = tokenSource.Token;
        return GetGameSizeAsyncInner(gameInstallerKind, token).AsResult();
    }

    public nint StartInstallAsync(InstallProgressDelegate? progressDelegate, InstallProgressStateDelegate? progressStateDelegate, in Guid cancelToken)
    {
        CancellationTokenSource tokenSource = ComCancellationTokenVault.RegisterToken(in cancelToken);
        CancellationToken token = tokenSource.Token;
        return StartInstallAsyncInner(progressDelegate, progressStateDelegate, token).AsResult();
    }

    public nint StartUpdateAsync(InstallProgressDelegate? progressDelegate, InstallProgressStateDelegate? progressStateDelegate, in Guid cancelToken)
    {
        CancellationTokenSource tokenSource = ComCancellationTokenVault.RegisterToken(in cancelToken);
        CancellationToken token = tokenSource.Token;
        return StartUpdateAsyncInner(progressDelegate, progressStateDelegate, token).AsResult();
    }

    public nint StartPreloadAsync(InstallProgressDelegate? progressDelegate, InstallProgressStateDelegate? progressStateDelegate, in Guid cancelToken)
    {
        CancellationTokenSource tokenSource = ComCancellationTokenVault.RegisterToken(in cancelToken);
        CancellationToken token = tokenSource.Token;
        return StartPreloadAsyncInner(progressDelegate, progressStateDelegate, token).AsResult();
    }

    public nint UninstallAsync(in Guid cancelToken)
    {
        CancellationTokenSource tokenSource = ComCancellationTokenVault.RegisterToken(in cancelToken);
        CancellationToken token = tokenSource.Token;
        return UninstallAsyncInner(token).AsResult();
    }

    protected abstract Task<long> GetGameSizeAsyncInner(GameInstallerKind gameInstallerKind, CancellationToken token);
    protected abstract Task StartInstallAsyncInner(InstallProgressDelegate? progressDelegate, InstallProgressStateDelegate? progressStateDelegate, CancellationToken token);
    protected abstract Task StartUpdateAsyncInner(InstallProgressDelegate? progressDelegate, InstallProgressStateDelegate? progressStateDelegate, CancellationToken token);
    protected abstract Task StartPreloadAsyncInner(InstallProgressDelegate? progressDelegate, InstallProgressStateDelegate? progressStateDelegate, CancellationToken token);
    protected abstract Task UninstallAsyncInner(CancellationToken token);

    protected string EnsureAndGetGamePath()
    {
        string? gamePath = GameManager.GetGamePath();
        if (string.IsNullOrEmpty(gamePath))
        {
            throw new InvalidOperationException("To Developer: Please set the game path using IGameManager.SetGamePath(gamePath, false) before starting StartInstallAsync!");
        }

        return gamePath;
    }

    public abstract void Dispose();
}
