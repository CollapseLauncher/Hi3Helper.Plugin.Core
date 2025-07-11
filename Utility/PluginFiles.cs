using System;
using System.Buffers;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
// ReSharper disable UnusedMember.Global

namespace Hi3Helper.Plugin.Core.Utility;

/// <summary>
/// Contains extensions related with Download operations and Copy-to routine.
/// </summary>
public static class PluginFiles
{
    internal const int ExCopyToBufferSize   = 65536;
    internal const int ExCopyToRetryAttempt = 5;

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

    /// <summary>
    /// Perform an asynchronous download operation to a target <see cref="Stream"/>.
    /// </summary>
    /// <remarks>
    /// The first argument of <see cref="FileReadProgressDelegate"/> delegate (<c>read</c>) passed by <paramref name="downloadProgress"/> callback will send a negative number if a retry attempt is triggered.<br/>
    /// This to avoid overflowed number if you're counting how many bytes are progressed using its <c>read</c> argument.
    /// </remarks>
    /// <param name="client">An HTTP client to be used for downloading the file.</param>
    /// <param name="fileUrl">The URL of the file.</param>
    /// <param name="outputStream">The target <see cref="Stream"/> in which the data will be downloaded to.</param>
    /// <param name="downloadProgress">A callback which sends the progress of the download operation.</param>
    /// <param name="bufferSize">How many bytes being used as a buffer for the download operation.</param>
    /// <param name="retryAttempt">How many retry attempts are used in case of failure.</param>
    /// <param name="token">A cancellation token for the async operation</param>
    public static async Task DownloadFilesAsync(this HttpClient           client,
                                                string                    fileUrl,
                                                Stream                    outputStream,
                                                FileReadProgressDelegate? downloadProgress,
                                                int                       bufferSize   = ExCopyToBufferSize,
                                                int                       retryAttempt = ExCopyToRetryAttempt,
                                                CancellationToken         token        = default)
    {
    StartOver:
        long bytesRead = 0;
        long totalBytes = 0;
        try
        {
            await using Stream networkStream = await client.CreateHttpBridgedStream(fileUrl, 0, null, token).ConfigureAwait(false);
            await networkStream.CopyToAsyncProgress(outputStream,
                (inRead, inBytesRead, inTotalBytes) =>
                {
                    bytesRead  = inBytesRead;
                    totalBytes = inTotalBytes;
                    downloadProgress?.Invoke(inRead, inBytesRead, inTotalBytes);
                },
                bufferSize,
                token).ConfigureAwait(false);
        }
        catch (Exception ex) when (!token.IsCancellationRequested)
        {
            downloadProgress?.Invoke(-bytesRead, bytesRead, totalBytes);
            if (retryAttempt <= 0)
            {
                throw;
            }

            SharedStatic.InstanceLogger.LogError(ex, "An error has occured while trying to download file: {FileUrl} (Retrying attempt left: {RetryAttempt})", fileUrl, retryAttempt);
            --retryAttempt;
            goto StartOver;
        }
    }

    /// <summary>
    /// Performing copy operation from and to <see cref="Stream"/> asynchronously.
    /// </summary>
    /// <param name="source">The source <see cref="Stream"/> to copy from.</param>
    /// <param name="destination">The target <see cref="Stream"/> to copy into.</param>
    /// <param name="progress">A callback which reports the progress of the copy operation.</param>
    /// <param name="bufferSize">The buffer size required to perform the copy operation.</param>
    /// <param name="token">A cancellation token for the async operation</param>
    public static async Task CopyToAsyncProgress(this Stream               source,
                                                 Stream                    destination,
                                                 FileReadProgressDelegate? progress,
                                                 int                       bufferSize = ExCopyToBufferSize,
                                                 CancellationToken         token      = default)
    {
        byte[] buffer     = ArrayPool<byte>.Shared.Rent(bufferSize);
        long   bytesRead  = 0;
        source.TryGetLength(out long totalBytes);

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
            throw;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}
