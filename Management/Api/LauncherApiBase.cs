using System;
using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices.Marshalling;
using System.Threading;
using System.Threading.Tasks;
using Hi3Helper.Plugin.Core.Utility;
using Microsoft.Win32.SafeHandles;

namespace Hi3Helper.Plugin.Core.Management.Api;

[GeneratedComClass]
public abstract partial class LauncherApiBase : InitializableTask, ILauncherApi
{
    protected readonly Lock       ThisInstanceLock      = new();
    protected abstract HttpClient ApiResponseHttpClient { get; set; }
    protected abstract string     ApiResponseBaseUrl    { get; }

    protected bool IsDisposed;

    public virtual nint DownloadAssetAsync(LauncherPathEntry                     entry,
                                           nint                                  outputStreamHandle,
                                           PluginFiles.FileReadProgressDelegate? downloadProgress,
                                           in Guid                               cancelToken)
    {
        CancellationTokenSource tokenSource = ComCancellationTokenVault.RegisterToken(in cancelToken);
        CancellationToken       token       = tokenSource.Token;

        SafeFileHandle safeFileHandle   = new(outputStreamHandle, false);
        FileStream     outputFileStream = new(safeFileHandle, FileAccess.ReadWrite);

        PluginDisposableMemory<byte> fileHash = entry.FileHash;
        string?                      fileUrl  = entry.Path;
        if (string.IsNullOrEmpty(fileUrl))
        {
            throw new NullReferenceException("Path of the LauncherPathEntry cannot be null!");
        }

        return DownloadAssetAsyncInner(null, fileUrl, outputFileStream, fileHash, downloadProgress, token).AsResult();
    }

    public virtual nint DownloadAssetAsync(string                                fileUrl,
                                           nint                                  outputStreamHandle,
                                           PluginFiles.FileReadProgressDelegate? downloadProgress,
                                           in Guid                               cancelToken)
    {
        CancellationTokenSource tokenSource = ComCancellationTokenVault.RegisterToken(in cancelToken);
        CancellationToken       token       = tokenSource.Token;

        SafeFileHandle safeFileHandle   = new(outputStreamHandle, false);
        FileStream     outputFileStream = new(safeFileHandle, FileAccess.ReadWrite);

        if (string.IsNullOrEmpty(fileUrl))
        {
            throw new NullReferenceException("Path of the LauncherPathEntry cannot be null!");
        }

        return DownloadAssetAsyncInner(null, fileUrl, outputFileStream, PluginDisposableMemory<byte>.Empty, downloadProgress, token).AsResult();
    }

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

        await PluginFiles.DownloadFilesAsync(client, fileUrl, outputStream, downloadProgress, token).ConfigureAwait(false);
    }

    public virtual void Dispose()
    {
        if (IsDisposed)
        {
            return;
        }

        using (ThisInstanceLock.EnterScope())
        {
            IsDisposed = true;
            ApiResponseHttpClient.Dispose();
            ApiResponseHttpClient = null!;
            GC.SuppressFinalize(this);
        }
    }
}
