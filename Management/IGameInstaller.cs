using Hi3Helper.Plugin.Core.Utility;
using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

#pragma warning disable CA1816
namespace Hi3Helper.Plugin.Core.Management;

/// <summary>
/// Delegate to provide callback to get progress of the game installation.
/// </summary>
/// <param name="progress">A readonly reference struct of the progress</param>
public delegate void InstallProgressDelegate(in InstallProgress progress);

/// <summary>
/// Delegate to provide callback to get the state of the game installation progress.
/// </summary>
/// <param name="state">A readonly reference enum of the installation progress state.</param>
public delegate void InstallProgressStateDelegate(InstallProgressState state);

/// <summary>
/// Defines a method which handles the game's installation and updates.
/// </summary>
/// <remarks>
/// This interface is intended to perform installation, updates and game download size retrieval.<br/>
/// All methods included within this interface are mostly asynchronous which requires awaiting via <see cref="ComAsyncResult"/>.
/// </remarks>
[GeneratedComInterface]
[Guid(ComInterfaceId.ExGameInstaller)]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public partial interface IGameInstaller : IDisposable
{
    /// <summary>
    /// Gets the estimated total size of the game depends on the current state.
    /// </summary>
    /// <param name="gameInstallerKind">Kind of game state of the size to retrieve.</param>
    /// <param name="cancelToken">Cancel token for the async operation.</param>
    /// <returns>
    /// A pointer to the <see cref="ComAsyncResult"/> instance.<br/>
    /// The pointer must be passed to <see cref="ComAsyncExtension.WaitFromHandle{T}(nint)"/> and set the generic<br/>
    /// result to <see cref="long"/> in order to await and get the result from this async function.
    /// </returns>
    nint GetGameSizeAsync(GameInstallerKind gameInstallerKind, in Guid cancelToken);

    /// <summary>
    /// Gets the size of existing/downloaded assets depends on the current state.
    /// </summary>
    /// <param name="gameInstallerKind">Kind of game state of the size to retrieve.</param>
    /// <param name="cancelToken">Cancel token for the async operation.</param>
    /// <returns>
    /// A pointer to the <see cref="ComAsyncResult"/> instance.<br/>
    /// The pointer must be passed to <see cref="ComAsyncExtension.WaitFromHandle{T}(nint)"/> and set the generic<br/>
    /// result to <see cref="long"/> in order to await and get the result from this async function.
    /// </returns>
    nint GetGameDownloadedSizeAsync(GameInstallerKind gameInstallerKind, in Guid cancelToken);

    /// <summary>
    /// Perform installation routine asynchronously.
    /// </summary>
    /// <param name="progressDelegate">A delegate to get the progress of the routine.</param>
    /// <param name="progressStateDelegate">A delegate to get the state of the progress of the routine.</param>
    /// <param name="cancelToken">A cancel token for cancelling the async operation.</param>
    /// <returns>
    /// A pointer to the <see cref="ComAsyncResult"/> instance.<br/>
    /// The pointer must be passed to <see cref="ComAsyncExtension.WaitFromHandle(nint)"/> in order to await the async function.<br/>
    /// The function, however is not-returnable.
    /// </returns>
    nint StartInstallAsync(InstallProgressDelegate? progressDelegate, InstallProgressStateDelegate? progressStateDelegate, in Guid cancelToken);

    /// <summary>
    /// Perform update routine asynchronously. Returns immediately if no update is available.
    /// </summary>
    /// <param name="progressDelegate">A delegate to get the progress of the routine.</param>
    /// <param name="progressStateDelegate">A delegate to get the state of the progress of the routine.</param>
    /// <param name="cancelToken">A cancel token for cancelling the async operation.</param>
    /// <returns>
    /// A pointer to the <see cref="ComAsyncResult"/> instance.<br/>
    /// The pointer must be passed to <see cref="ComAsyncExtension.WaitFromHandle(nint)"/> in order to await the async function.<br/>
    /// The function, however is not-returnable.
    /// </returns>
    nint StartUpdateAsync(InstallProgressDelegate? progressDelegate, InstallProgressStateDelegate? progressStateDelegate, in Guid cancelToken);

    /// <summary>
    /// Perform preload routine asynchronously. Returns immediately if no preload is available.
    /// </summary>
    /// <param name="progressDelegate">A delegate to get the progress of the routine.</param>
    /// <param name="progressStateDelegate">A delegate to get the state of the progress of the routine.</param>
    /// <param name="cancelToken">A cancel token for cancelling the async operation.</param>
    /// <returns>
    /// A pointer to the <see cref="ComAsyncResult"/> instance.<br/>
    /// The pointer must be passed to <see cref="ComAsyncExtension.WaitFromHandle(nint)"/> in order to await the async function.<br/>
    /// The function, however is not-returnable.
    /// </returns>
    nint StartPreloadAsync(InstallProgressDelegate? progressDelegate, InstallProgressStateDelegate? progressStateDelegate, in Guid cancelToken);

    #region DynamicInterfaceCastable Explicit Calls
    /// <inheritdoc/>
    void IDisposable.Dispose() => Dispose();
    #endregion
}
