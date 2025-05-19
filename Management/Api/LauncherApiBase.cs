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
public abstract partial class LauncherApiBase : Initializable, ILauncherApi
{
    protected abstract HttpClient? ApiResponseHttpClient { get; }

    public virtual unsafe nint DownloadAssetAsync(nint pathEntry,
                                                  nint outputStreamHandle,
                                                  PluginFiles.FileReadProgressDelegate? downloadProgress,
                                                  in Guid cancelToken)
    {
        CancellationTokenSource tokenSource = ComCancellationTokenVault.RegisterToken(in cancelToken);
        CancellationToken       token       = tokenSource.Token;

        SafeFileHandle safeFileHandle   = new SafeFileHandle(outputStreamHandle, false);
        FileStream     outputFileStream = new FileStream(safeFileHandle, FileAccess.ReadWrite);

        LauncherPathEntry* entry = pathEntry.AsPointer<LauncherPathEntry>();

        byte[] fileChecksum = new Span<byte>(entry->FileHash, entry->FileHashLength).ToArray();
        string fileUrl      = Mem.CreateSpanFromNullTerminated<char>(entry->Path).ToString();

        return DownloadAssetAsyncInner(null, fileUrl, outputFileStream, fileChecksum, downloadProgress, token).AsResult();
    }

    protected virtual async Task DownloadAssetAsyncInner(HttpClient? client,
                                                         string fileUrl,
                                                         Stream outputStream,
                                                         byte[] fileChecksum,
                                                         PluginFiles.FileReadProgressDelegate? downloadProgress,
                                                         CancellationToken token)
    {
        client ??= ApiResponseHttpClient;
        if (client == null)
        {
            throw new ArgumentNullException(nameof(client), "HttpClient cannot be null!");
        }

        await PluginFiles.DownloadFilesAsync(client, fileUrl, outputStream, downloadProgress, token).ConfigureAwait(false);
    }

    public virtual unsafe bool FreePathEntriesHandle(nint handle)
    {
        if (handle == nint.Zero)
        {
            return false;
        }

        LauncherPathEntry* entries = handle.AsPointer<LauncherPathEntry>();
        while (entries != null)
        {
            LauncherPathEntry* nextEntry = entries->NextEntry.AsPointer<LauncherPathEntry>();
            Mem.Free(entries);
            entries = nextEntry;
        }

        return true;
    }
}
