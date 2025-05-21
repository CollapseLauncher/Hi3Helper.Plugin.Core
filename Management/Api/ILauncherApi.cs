using System;
using System.Runtime.InteropServices.Marshalling;
using System.Runtime.InteropServices;
using Hi3Helper.Plugin.Core.Utility;

namespace Hi3Helper.Plugin.Core.Management.Api;

/// <summary>
/// Defines the contract for a launcher API that supports asynchronous asset downloading.
/// </summary>
/// <remarks>
/// This interface extends <see cref="IInitializable"/> and provides methods for downloading assets asynchronously.
/// The download operation supports progress reporting and cancellation.
/// </remarks>
[GeneratedComInterface]
[Guid(ComInterfaceId.LauncherApi)]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public partial interface ILauncherApi : IInitializable
{
    /// <summary>
    /// Asynchronously downloads an asset to the specified output stream.
    /// </summary>
    /// <param name="entry"><see cref="LauncherPathEntry"/> struct for the asset URL to download.</param>
    /// <param name="outputStreamHandle">A handle to the output stream where the asset will be written.</param>
    /// <param name="downloadProgress">An optional callback delegate for reporting download progress.</param>
    /// <param name="cancelToken">A <see cref="Guid"/> used as a cancellation token for the operation.</param>
    /// <returns>
    /// A native pointer (<see cref="nint"/>) to a <see cref="ComAsyncResult"/> representing the asynchronous operation.
    /// </returns>
    nint DownloadAssetAsync(LauncherPathEntry entry,
                            nint outputStreamHandle,
                            PluginFiles.FileReadProgressDelegate? downloadProgress,
                            in Guid cancelToken);
}
