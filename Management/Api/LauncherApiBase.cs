using Hi3Helper.Plugin.Core.Utility;
using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices.Marshalling;
using System.Threading;
using System.Threading.Tasks;
// ReSharper disable UnusedMemberInSuper.Global

namespace Hi3Helper.Plugin.Core.Management.Api;

/// <summary>
/// A base class for launcher API related instance (such as: Resource, Media, News, etc.)
/// </summary>
[GeneratedComClass]
public abstract partial class LauncherApiBase : InitializableTask, ILauncherApi
{
    protected readonly Lock       ThisInstanceLock      = new();
    protected abstract HttpClient ApiResponseHttpClient { get; set; }
    protected virtual  string?    ApiResponseBaseUrl    => null;

    protected bool IsDisposed;

    /// <inheritdoc/>
    public virtual void DownloadAssetAsync(LauncherPathEntry                     entry,
                                           FileHandle                            outputStreamHandle,
                                           PluginFiles.FileReadProgressDelegate? downloadProgress,
                                           in  Guid                              cancelToken,
                                           out nint                              result)
    {
        CancellationTokenSource tokenSource = ComCancellationTokenVault.RegisterToken(in cancelToken);
        CancellationToken       token       = tokenSource.Token;

        SafeFileHandle safeFileHandle   = new(outputStreamHandle.UnsafeHandle, false);
        FileStream     outputFileStream = new(safeFileHandle, FileAccess.ReadWrite);

        PluginDisposableMemory<byte> fileHash = entry.FileHash;
        string?                      fileUrl  = entry.Path;
        if (string.IsNullOrEmpty(fileUrl))
        {
            throw new NullReferenceException("Path of the LauncherPathEntry cannot be null!");
        }

        result = DownloadAssetAsyncInner(null, fileUrl, outputFileStream, fileHash, downloadProgress, token).AsResult();
    }

    /// <inheritdoc/>
    public virtual void DownloadAssetAsync(string                                fileUrl,
                                           FileHandle                            outputStreamHandle,
                                           PluginFiles.FileReadProgressDelegate? downloadProgress,
                                           in  Guid                              cancelToken,
                                           out nint                              result)
    {
        CancellationTokenSource tokenSource = ComCancellationTokenVault.RegisterToken(in cancelToken);
        CancellationToken       token       = tokenSource.Token;

        SafeFileHandle safeFileHandle   = new(outputStreamHandle.UnsafeHandle, false);
        FileStream     outputFileStream = new(safeFileHandle, FileAccess.ReadWrite);

        if (string.IsNullOrEmpty(fileUrl))
        {
            throw new NullReferenceException("Path of the LauncherPathEntry cannot be null!");
        }

        result = DownloadAssetAsyncInner(null, fileUrl, outputFileStream, PluginDisposableMemory<byte>.Empty, downloadProgress, token).AsResult();
    }

    /// <summary>
    /// Perform inner asynchronous download operation.
    /// </summary>
    /// <param name="client"></param>
    /// <param name="fileUrl"></param>
    /// <param name="outputStream"></param>
    /// <param name="fileChecksum"></param>
    /// <param name="downloadProgress"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    protected virtual async Task DownloadAssetAsyncInner(HttpClient?                           client,
                                                         string                                fileUrl,
                                                         Stream                                outputStream,
                                                         PluginDisposableMemory<byte>          fileChecksum,
                                                         PluginFiles.FileReadProgressDelegate? downloadProgress,
                                                         CancellationToken                     token)
    {
        client ??= ApiResponseHttpClient;
        if (client == null)
        {
            throw new ArgumentNullException(nameof(client), "HttpClient cannot be null!");
        }

        await client.DownloadFilesAsync(fileUrl, outputStream, downloadProgress, token: token).ConfigureAwait(false);
        await outputStream.FlushAsync(token);
    }

    /// <inheritdoc cref="IFree.Free"/>
    public override void Free() => Dispose();

    public virtual void Dispose()
    {
        using (ThisInstanceLock.EnterScope())
        {
            if (IsDisposed)
            {
                return;
            }

            IsDisposed = true;
            ApiResponseHttpClient.Dispose();
            ApiResponseHttpClient = null!;
            GC.SuppressFinalize(this);
        }
    }
}
