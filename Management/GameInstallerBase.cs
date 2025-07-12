using Hi3Helper.Plugin.Core.Utility;
using System;
using System.Runtime.InteropServices.Marshalling;
using System.Threading;
using System.Threading.Tasks;

namespace Hi3Helper.Plugin.Core.Management;

[GeneratedComClass]
public abstract partial class GameInstallerBase(IGameManager? gameManager) : InitializableTask, IGameInstaller
{
    /// <summary>
    /// The current instance of <see cref="IGameManager"/>.
    /// </summary>
    protected readonly IGameManager GameManager = gameManager ?? throw new NullReferenceException("Game Manager is null!");

    /// <inheritdoc/>
    public void GetGameSizeAsync(GameInstallerKind gameInstallerKind, in Guid cancelToken, out nint result)
    {
        CancellationTokenSource tokenSource = ComCancellationTokenVault.RegisterToken(in cancelToken);
        CancellationToken token = tokenSource.Token;
        result = GetGameSizeAsyncInner(gameInstallerKind, token).AsResult();
    }

    /// <inheritdoc/>
    public void GetGameDownloadedSizeAsync(GameInstallerKind gameInstallerKind, in Guid cancelToken, out nint result)
    {
        CancellationTokenSource tokenSource = ComCancellationTokenVault.RegisterToken(in cancelToken);
        CancellationToken token = tokenSource.Token;
        result = GetGameDownloadedSizeAsyncInner(gameInstallerKind, token).AsResult();
    }

    /// <inheritdoc/>
    public void StartInstallAsync(InstallProgressDelegate? progressDelegate, InstallProgressStateDelegate? progressStateDelegate, in Guid cancelToken, out nint result)
    {
        CancellationTokenSource tokenSource = ComCancellationTokenVault.RegisterToken(in cancelToken);
        CancellationToken token = tokenSource.Token;
        result = StartInstallAsyncInner(progressDelegate, progressStateDelegate, token).AsResult();
    }

    /// <inheritdoc/>
    public void StartUpdateAsync(InstallProgressDelegate? progressDelegate, InstallProgressStateDelegate? progressStateDelegate, in Guid cancelToken, out nint result)
    {
        CancellationTokenSource tokenSource = ComCancellationTokenVault.RegisterToken(in cancelToken);
        CancellationToken token = tokenSource.Token;
        result = StartUpdateAsyncInner(progressDelegate, progressStateDelegate, token).AsResult();
    }

    /// <inheritdoc/>
    public void StartPreloadAsync(InstallProgressDelegate? progressDelegate, InstallProgressStateDelegate? progressStateDelegate, in Guid cancelToken, out nint result)
    {
        CancellationTokenSource tokenSource = ComCancellationTokenVault.RegisterToken(in cancelToken);
        CancellationToken token = tokenSource.Token;
        result = StartPreloadAsyncInner(progressDelegate, progressStateDelegate, token).AsResult();
    }

    /// <inheritdoc cref="IGameUninstaller.UninstallAsync(in Guid, out nint)"/>
    public void UninstallAsync(in Guid cancelToken, out nint result)
    {
        CancellationTokenSource tokenSource = ComCancellationTokenVault.RegisterToken(in cancelToken);
        CancellationToken token = tokenSource.Token;
        result = UninstallAsyncInner(token).AsResult();
    }

    /// <summary>
    /// Gets the game size based on the method of the game installation.
    /// </summary>
    /// <param name="gameInstallerKind">The method of the game installation kind.</param>
    /// <param name="token">A token for async operation cancellation.</param>
    /// <returns>The total size of the game installation based on its method.</returns>
    protected abstract Task<long> GetGameSizeAsyncInner(GameInstallerKind gameInstallerKind, CancellationToken token);

    /// <summary>
    /// Gets only the total size of the downloaded files based on the method of the game installation.
    /// </summary>
    /// <param name="gameInstallerKind">The method of the game installation kind.</param>
    /// <param name="token">A token for async operation cancellation.</param>
    /// <returns>The total size of already downloaded files.</returns>
    protected abstract Task<long> GetGameDownloadedSizeAsyncInner(GameInstallerKind gameInstallerKind, CancellationToken token);

    /// <summary>
    /// Starts performing game installation operation.
    /// </summary>
    /// <param name="progressDelegate">A callback which reports the progress of the current game installation process.</param>
    /// <param name="progressStateDelegate">A callback which reports the state of the game installation process.</param>
    /// <param name="token">A token for async operation cancellation.</param>
    protected abstract Task StartInstallAsyncInner(InstallProgressDelegate? progressDelegate, InstallProgressStateDelegate? progressStateDelegate, CancellationToken token);

    /// <summary>
    /// Starts performing game update operation.
    /// </summary>
    /// <param name="progressDelegate">A callback which reports the progress of the current game update process.</param>
    /// <param name="progressStateDelegate">A callback which reports the state of the game update process.</param>
    /// <param name="token">A token for async operation cancellation.</param>
    protected abstract Task StartUpdateAsyncInner(InstallProgressDelegate? progressDelegate, InstallProgressStateDelegate? progressStateDelegate, CancellationToken token);

    /// <summary>
    /// Starts performing game preloading operation.
    /// </summary>
    /// <param name="progressDelegate">A callback which reports the progress of the current game update process.</param>
    /// <param name="progressStateDelegate">A callback which reports the state of the game update process.</param>
    /// <param name="token">A token for async operation cancellation.</param>
    protected abstract Task StartPreloadAsyncInner(InstallProgressDelegate? progressDelegate, InstallProgressStateDelegate? progressStateDelegate, CancellationToken token);

    /// <summary>
    /// Starts performing game uninstall operation.
    /// </summary>
    /// <param name="token">A token for async operation cancellation.</param>
    protected abstract Task UninstallAsyncInner(CancellationToken token);

    /// <summary>
    /// Gets the current game installation path and throw if it's empty (or unset).
    /// </summary>
    /// <returns>A path of the current game installation.</returns>
    /// <exception cref="InvalidOperationException">If the current game installation path isn't set.</exception>
    protected string EnsureAndGetGamePath()
    {
        GameManager.GetGamePath(out string? gamePath);
        if (string.IsNullOrEmpty(gamePath))
        {
            throw new InvalidOperationException("To Developer: Please set the game path using IGameManager.SetGamePath(gamePath, false) before starting StartInstallAsync!");
        }

        return gamePath;
    }

    /// <inheritdoc cref="IFree.Free"/>
    public override void Free() => Dispose();

    public abstract void Dispose();
}
