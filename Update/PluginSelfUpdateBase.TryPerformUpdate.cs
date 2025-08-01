﻿using Hi3Helper.Plugin.Core.Management;
using Hi3Helper.Plugin.Core.Utility;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

#if !USELIGHTWEIGHTJSONPARSER
using System.Net.Http.Json;
#endif

namespace Hi3Helper.Plugin.Core.Update;

public partial class PluginSelfUpdateBase
{
    /// <summary>
    /// Asynchronously perform update on the plugin.
    /// </summary>
    /// <param name="outputDir">An output directory which the new version of library will be downloaded to.</param>
    /// <param name="checkForUpdatesOnly">Whether to perform the check only or also to perform the update at the same time.</param>
    /// <param name="progressDelegate">A delegate which pass <see cref="InstallProgress"/> to indicate update progress into the callback.</param>
    /// <param name="token">A token for cancelling asynchronous update operation</param>
    protected virtual async Task<nint> TryPerformUpdateAsync(string? outputDir, bool checkForUpdatesOnly, InstallProgressDelegate? progressDelegate, CancellationToken token)
    {
        if (BaseCdnUrlSpan.IsEmpty)
        {
            return SelfUpdateReturnInfo.CreateToNativeMemory(SelfUpdateReturnCode.NoAvailableUpdate);
        }

        try
        {
            ArgumentException.ThrowIfNullOrEmpty(outputDir, nameof(outputDir));

            (PluginManifest? info, string? baseUrl) = await TryGetAvailableCdn(token);
            if (info == null || baseUrl == null)
            {
                return SelfUpdateReturnInfo.CreateToNativeMemory(SelfUpdateReturnCode.NoReachableCdn);
            }

            GameVersion currentPluginVersion = SharedStatic.CurrentPluginVersion;
            GameVersion cdnPluginVersion     = info.PluginVersion;
            if (cdnPluginVersion == currentPluginVersion)
            {
                return SelfUpdateReturnInfo.CreateToNativeMemory(SelfUpdateReturnCode.NoAvailableUpdate);
            }

            SelfUpdateReturnCode successReturnCode = SelfUpdateReturnCode.UpdateIsAvailable;
            if (checkForUpdatesOnly)
            {
                return SelfUpdateReturnInfo.CreateToNativeMemory(
                                                                 successReturnCode,
                                                                 info.MainPluginName,
                                                                 info.MainPluginAuthor,
                                                                 info.MainPluginDescription,
                                                                 info.PluginVersion,
                                                                 info.PluginStandardVersion,
                                                                 info.PluginCreationDate,
                                                                 info.ManifestDate
                                                                );
            }

            bool isRollback = cdnPluginVersion < currentPluginVersion;
            await StartUpdateRoutineAsync(outputDir, info, baseUrl, progressDelegate, token);
            successReturnCode = isRollback ? SelfUpdateReturnCode.RollingBackSuccess : SelfUpdateReturnCode.UpdateSuccess;

            return SelfUpdateReturnInfo.CreateToNativeMemory(
                                                             successReturnCode,
                                                             info.MainPluginName,
                                                             info.MainPluginAuthor,
                                                             info.MainPluginDescription,
                                                             info.PluginVersion,
                                                             info.PluginStandardVersion,
                                                             info.PluginCreationDate,
                                                             info.ManifestDate
                                                            );
        }
        catch (Exception ex)
        {
            SharedStatic.InstanceLogger.LogError(ex, "[PluginSelfUpdateBase::TryPerformUpdateAsync] An error has occurred while trying to update the plugin.");
            return GetReturnCodeFromException(ex, token);
        }
    }

    private async Task StartUpdateRoutineAsync(string outputDir, PluginManifest info, string baseUrl, InstallProgressDelegate? progressDelegate, CancellationToken token)
    {
        Directory.CreateDirectory(outputDir);

        InstallProgress progress = new()
        {
            TotalBytesToDownload = info.Assets.Sum(x => x.Size),
            TotalCountToDownload = info.Assets.Count
        };

        RetryableCopyToStreamTaskOptions taskOptions = new()
        {
            IsDisposeTargetStream = true,
            MaxBufferSize = 8 << 10
        };

        try
        {
            // Additionally downloads manifest.json file too.
            await Impl(new PluginManifestAssetInfo
            {
                FileHash = [],
                FilePath = "manifest.json",
                Size = 0
            }, token);
            await Parallel.ForEachAsync(info.Assets, token, Impl);
        }
        catch (AggregateException ex)
        {
            throw ex.Flatten().InnerExceptions.FirstOrDefault() ?? ex.InnerException ?? ex;
        }
        return;

        async ValueTask Impl(PluginManifestAssetInfo assetInfo, CancellationToken innerToken)
        {
            string fileUrl = baseUrl.CombineUrlFromString(assetInfo.FilePath);
            string filePath = Path.Combine(outputDir, assetInfo.FilePath.TrimStart("/\\").ToString());
            FileInfo fileInfo = new(filePath);

            if (fileInfo.Exists)
            {
                fileInfo.IsReadOnly = false;
            }

            Interlocked.Increment(ref progress.DownloadedCount);

            fileInfo.Directory?.Create();
            await using FileStream fileStream = fileInfo.Open(FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite);

            if (fileStream.Length > 0 &&
                await IsHashMatchedAsync(
                    fileStream,
                    assetInfo.FileHash,
                    read =>
                    {
                        Interlocked.Add(ref progress.DownloadedBytes, read);
                        progressDelegate?.Invoke(progress);
                    },
                    innerToken))
            {
                SharedStatic.InstanceLogger.LogTrace("[PluginSelfUpdateBase::StartUpdateRoutineAsync] An existed file: {} has already been downloaded. Skipping!", filePath);
                return;
            }

            fileStream.SetLength(0);

            RetryableCopyToStreamTask downloadTask = RetryableCopyToStreamTask
                .CreateTask(
                    async (pos, ctx) => await UpdateHttpClient.CreateHttpBridgedStream(fileUrl, pos, null, ctx),
                    fileStream,
                    taskOptions);

            await downloadTask.StartTaskAsync(read =>
            {
                Interlocked.Add(ref progress.DownloadedBytes, read);
                progressDelegate?.Invoke(progress);
            }, innerToken);

            SharedStatic.InstanceLogger.LogTrace("[PluginSelfUpdateBase::StartUpdateRoutineAsync] Update file: {} has been downloaded to {}", fileUrl, filePath);
        }
    }

    private static async ValueTask<bool> IsHashMatchedAsync(
        FileStream fileStream,
        byte[] remoteHash,
        Action<long> readStatusCallback,
        CancellationToken token)
    {
        MD5 hasher = MD5.Create();
        const int bufferSize = 256 << 10;
        byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize); // 256 KB of buffer

        long totalRead = 0;
        try
        {
            int read;
            while ((read = await fileStream.ReadAtLeastAsync(buffer, bufferSize, false, token)) > 0)
            {
                hasher.TransformBlock(buffer, 0, read, buffer, 0);
                totalRead += read;
                readStatusCallback(read);
            }
            hasher.TransformFinalBlock(buffer, 0, read);

            byte[]? hashResult = hasher.Hash;
            if (hashResult == null)
            {
                return false;
            }

            if (hashResult.SequenceEqual(remoteHash))
            {
                return true;
            }

            // If it's still invalid, try reversing the byte order (assume it's LE->BE)
            Array.Reverse(hashResult);
            if (hashResult.SequenceEqual(remoteHash))
            {
                return true;
            }

            // Otherwise if it keeps failing, then return false and restore the order.
            Array.Reverse(hashResult);
            SharedStatic.InstanceLogger.LogError(
                "Hash for: {FilePath} isn't match! {HashRemote} Remote != {HashLocal} Local. Download will be restarted!",
                fileStream.Name,
                Convert.ToHexStringLower(remoteHash),
                Convert.ToHexStringLower(hashResult));

            // Reset the read count if the hash isn't match.
            readStatusCallback(-totalRead);
            return false;
        }
        catch (Exception)
        {
            // Also reset the read count if an exception occur.
            readStatusCallback(-totalRead);
            throw;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    /// <summary>
    /// Try to get the available CDN for the update routine to use.
    /// </summary>
    /// <param name="token">The cancellation token for the asynchronous operation.</param>
    /// <returns>A <see cref="ValueTuple"/> of Info <see cref="PluginManifest"/> and BaseUrl <see cref="string"/></returns>
    /// <remarks>
    /// Both Info <see cref="PluginManifest"/> and BaseUrl <see cref="string"/> will return <c>null</c> if no CDN is available.
    /// </remarks>
    protected virtual async Task<(PluginManifest? Info, string? BaseUrl)>
        TryGetAvailableCdn(CancellationToken token)
    {
        for (int i = 0; i < BaseCdnUrlSpan.Length; i++)
        {
            string manifestUrl = BaseCdnUrlSpan[i].CombineUrlFromString("manifest.json");

            try
            {
                using HttpResponseMessage jsonMessage = await UpdateHttpClient.GetAsync(manifestUrl, HttpCompletionOption.ResponseHeadersRead, token);
                jsonMessage.EnsureSuccessStatusCode();

#if USELIGHTWEIGHTJSONPARSER
                await using Stream networkStream = await jsonMessage.Content.ReadAsStreamAsync(token);
                PluginManifest info = await PluginManifest.ParseFromAsync(networkStream, token: token);
                if (info.Assets.Count == 0)
                {
                    continue;
                }
#else
                PluginManifest? info = await jsonMessage.Content.ReadFromJsonAsync(PluginManifestContext.Default.PluginManifest, token);
                if (info == null || info.Assets.Count == 0)
                {
                    continue;
                }
#endif

                return (info, BaseCdnUrlSpan[i]);
            }
            catch (Exception ex)
            {
                SharedStatic.InstanceLogger.LogError(ex, "An error has occurred while trying to get CDN details from: {}", manifestUrl);
            }
        }

        return (null, null);
    }

    private static nint GetReturnCodeFromException(Exception ex, CancellationToken token)
        => SelfUpdateReturnInfo.CreateToNativeMemory(ex switch
        {
            OperationCanceledException when token.IsCancellationRequested => SelfUpdateReturnCode.UpdateCancelled,
            IOException asIoException => MapIoExceptionToCode(asIoException),
            HttpRequestException asHttpRequestException => MapHttpRequestExceptionToCode(asHttpRequestException),
            JsonException or InvalidDataException => SelfUpdateReturnCode.JsonParsingError,
            _ => SelfUpdateReturnCode.UnknownError
        });

    private static SelfUpdateReturnCode MapHttpRequestExceptionToCode(HttpRequestException httpRequestEx)
    {
        int? code = (int?)httpRequestEx.StatusCode;
        if (code is null or 0)
        {
            return SelfUpdateReturnCode.NetworkError;
        }

        return code switch
        {
            >= 500 and <= 599 => SelfUpdateReturnCode.CdnInternalError,
            404 => SelfUpdateReturnCode.UpdateFileNotFound,

            _ => SelfUpdateReturnCode.NetworkError
        };
    }

    private static SelfUpdateReturnCode MapIoExceptionToCode(IOException ioEx)
    {
        int code = ioEx.HResult & 0xFFFF;
        return code switch
        {
            0x20 => SelfUpdateReturnCode.FileLocked,
            0x5 => SelfUpdateReturnCode.DiskAccessDenied,
            0x70 => SelfUpdateReturnCode.DiskFull,
            0x3 => SelfUpdateReturnCode.PathNotFound,
            0x15 => SelfUpdateReturnCode.DriveNotReady,
            0x50 => SelfUpdateReturnCode.FileAlreadyExists,
            0xCE => SelfUpdateReturnCode.NameTooLong,
            _ => SelfUpdateReturnCode.LocalError
        };
    }
}
