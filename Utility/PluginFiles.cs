using System;
using System.Buffers;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
// ReSharper disable UnusedMember.Global

namespace Hi3Helper.Plugin.Core.Utility;

public static class PluginFiles
{
    internal const int ExCopyToBufferSize = 65536;

    /// <summary>
    /// Delegate for reporting progress of file read operations.
    /// </summary>
    /// <param name="read">
    /// How many bytes has been read in this operation.
    /// </param>
    /// <param name="bytesRead">
    /// How many bytes has been read.
    /// </param>
    /// <param name="totalBytes">
    /// How many bytes needs to be read in total.
    /// </param>
    public delegate void FileReadProgressDelegate(long read, long bytesRead, long totalBytes);

    /// <summary>
    /// Delegate for checking if the asset's checksum matches the expected value.
    /// </summary>
    /// <param name="stream">The stream of the file to check</param>
    /// <param name="checksum">The checksum in which the file will be checked</param>
    /// <returns><c>True</c> if the checksum matches, Otherwise, <c>False</c>.</returns>
    public delegate Task<bool> ChecksumCheckDelegate(Stream stream, byte[] checksum, FileReadProgressDelegate? readProgress = null);

    public static async Task DownloadFilesAsync(HttpClient client,
                                                string fileUrl,
                                                Stream outputStream,
                                                FileReadProgressDelegate? downloadProgress,
                                                CancellationToken token)
    {
        int retry = 5;
    StartOver:
        try
        {
            await using Stream networkStream = await HttpResponseInputStream.CreateStreamAsync(client, fileUrl, null, null, token).ConfigureAwait(false);
            await networkStream.CopyToAsyncProgress(outputStream, downloadProgress, token: token).ConfigureAwait(false);
        }
        catch (Exception ex) when (!token.IsCancellationRequested)
        {
            if (retry <= 0)
            {
                throw;
            }

            SharedStatic.InstanceLogger.LogError(ex, "An error has occured while trying to download file: {FileUrl} (Retrying attempt left: {RetryAttempt})", fileUrl, retry);
            --retry;
            goto StartOver;
        }
    }

    public static async Task CopyToAsyncProgress(this Stream source,
                                                 Stream destination,
                                                 FileReadProgressDelegate? progress,
                                                 int bufferSize = ExCopyToBufferSize,
                                                 CancellationToken token = default)
    {
        byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);
        long totalBytes = source.Length;
        long bytesRead = 0;

        try
        {
            int read;
            while ((read = await source.ReadAtLeastAsync(buffer.AsMemory(0, bufferSize), bufferSize, false, token).ConfigureAwait(false)) > 0)
            {
                await destination.WriteAsync(buffer.AsMemory(0, read), token).ConfigureAwait(false);
                bytesRead += read;
                progress?.Invoke(read, bytesRead, totalBytes);
            }
        }
        catch (Exception) when (!token.IsCancellationRequested)
        {
            progress?.Invoke(-bytesRead, 0, totalBytes);
            throw;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}
