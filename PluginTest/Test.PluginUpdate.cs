using Hi3Helper.Plugin.Core.Management;
using Hi3Helper.Plugin.Core.Update;
using Hi3Helper.Plugin.Core.Utility;
using Microsoft.Extensions.Logging;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

#if !USELIGHTWEIGHTJSONPARSER
using Hi3Helper.Plugin.Core.Utility.Json.Converters;
#endif

namespace PluginTest;

internal static partial class Test
{
    internal static async Task TestSelfUpdate(PluginGetPlugin delegateIn, ILogger logger)
    {
        PluginInterface ??= GetPlugin(delegateIn, out _);
        ThrowIfPluginIsNull(PluginInterface);

        string curDir = Path.Combine(Environment.CurrentDirectory, "UpdateTest");

        logger.LogInformation("IPlugin->GetPluginSelfUpdater(): Getting plugin self-updater");
        PluginInterface.GetPluginSelfUpdater(out IPluginSelfUpdate? updater);
        if (updater == null)
        {
            logger.LogInformation("IPlugin->GetPluginSelfUpdater(): IPluginSelfUpdate returns null! Ignoring...");
            return;
        }


        logger.LogInformation("IPluginSelfUpdate->TryPerformUpdateAsync(): Checking for update...");
        updater.TryPerformUpdateAsync(curDir, false, null, in CancelToken, out nint result);
        nint updateInfo = await result.AsTask<nint>();

        using SelfUpdateReturnInfo updateInfoP = updateInfo.AsRef<SelfUpdateReturnInfo>();
        logger.LogInformation("IPluginSelfUpdate->TryPerformUpdateAsync(): Successfully invoking update test with return code: {code}", updateInfoP.ReturnCode);
        updateInfo.Free();
    }

    internal static async Task TestManagedUpdate(PluginGetUpdateCdnListDelegate delegateIn, ILogger logger)
    {
        string[] urlList = GetUpdateCdnUrlList(delegateIn, logger);

        if (urlList.Length == 0)
        {
            logger.LogInformation("GetPluginUpdateCdnList(): No CDN strings returned!");
            return;
        }

        logger.LogInformation("GetPluginUpdateCdnList(): Plugin returned {count} CDN strings!", urlList.Length);
        foreach (string url in urlList)
        {
            logger.LogInformation("  -> {url}", url);
        }

        string curDir = Path.Combine(Environment.CurrentDirectory, "UpdateTest");
        using SocketsHttpHandler handle = new();
        handle.AllowAutoRedirect = true;
        handle.MaxConnectionsPerServer = 32;

        using HttpClient client = new(handle, false);
        (string? cdnUrl, PluginManifest? manifest) = await FindPossibleUpdateCdn(client, urlList, logger);
        if (cdnUrl == null || manifest == null)
        {
            logger.LogWarning("  + Cannot find reachable CDN! Though, the test will be skipped anyway");
            return;
        }

        logger.LogInformation("  + Testing download from CDN: {cdn}...", cdnUrl);

        try
        {
            manifest.Assets.Add(new PluginManifestAsset
            {
                FileHash = [],
                FilePath = "manifest.json",
                Size = 0
            });
            await Parallel.ForEachAsync(manifest.Assets, ImplDownload);
            logger.LogInformation("  + Download completed!");
        }
        catch (Exception e)
        {
            if (e is AggregateException eg)
            {
                e = eg.Flatten().InnerExceptions.FirstOrDefault() ?? e;
            }

            logger.LogError(e, "  + Error while downloading one or more assets! Though, the test will be skipped anyway");
            throw;
        }

        return;

        async ValueTask ImplDownload(PluginManifestAsset asset, CancellationToken token)
        {
            bool isManifest = asset.FilePath!.EndsWith("manifest.json", StringComparison.OrdinalIgnoreCase);
            if (!isManifest)
            {
                logger.LogInformation("    + Downloading asset: {assetName} ({assetSize} bytes)...", asset.FilePath, asset.Size);
            }

            byte[] buffer = ArrayPool<byte>.Shared.Rent(8192);

            try
            {
                string assetUrl = cdnUrl.CombineUrlFromString(asset.FilePath);
                using HttpResponseMessage response = await client.GetAsync(assetUrl, token);
                await using Stream assetStream = await response.Content.ReadAsStreamAsync(token);

                string filePath = Path.Combine(curDir, asset.FilePath!);
                string fileDir = Path.GetDirectoryName(filePath)!;

                Directory.CreateDirectory(fileDir);

                await using FileStream fileStream = new(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
                using MD5 md5 = MD5.Create();

                int read;
                while ((read = await assetStream.ReadAsync(buffer, token)) > 0)
                {
                    md5.TransformBlock(buffer, 0, read, buffer, 0);
                    await fileStream.WriteAsync(buffer.AsMemory(0, read), token);
                }

                md5.TransformFinalBlock(buffer, 0, read);
                byte[] hash = md5.Hash!;

                if (isManifest)
                {
                    return;
                }

                if (asset.FileHash.SequenceEqual(hash))
                {
                    return;
                }

                Array.Reverse(hash);
                if (asset.FileHash.SequenceEqual(hash))
                {
                    return;
                }

                Array.Reverse(hash);
                throw new InvalidOperationException($"Hash of the asset file: {asset.FilePath} isn't match! ({Convert.ToHexStringLower(hash)} Local != {Convert.ToHexStringLower(asset.FileHash)} Remote)");
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }

    internal static async Task<(string?, PluginManifest?)> FindPossibleUpdateCdn(HttpClient client, string[] cdnUrls, ILogger logger)
    {
        string[] randomizedUrls = new string[cdnUrls.Length];
        Random.Shared.GetItems(cdnUrls, randomizedUrls.AsSpan());

        const string manifestFile = "manifest.json";
        foreach (string currentCdnUrl in randomizedUrls)
        {
            string manifestUrl = currentCdnUrl.CombineUrlFromString(manifestFile);
            using HttpResponseMessage response =
                await client.GetAsync(manifestUrl, HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
            {
                continue;
            }

            logger.LogInformation("  + Found manifest file at {url}, trying to parse...", manifestUrl);

            try
            {
                PluginManifest? manifest = await JsonSerializer.DeserializeAsync<PluginManifest>(
                    await response.Content.ReadAsStreamAsync(),
                    PluginManifestJsonContext.Default.PluginManifest);

                if (manifest == null)
                {
                    throw new NullReferenceException("Manifest returns a null response!");
                }

                string logMessage = "  + Manifest Parsed!\r\n" +
                                    $"                + MainLibraryName: {manifest.MainLibraryName}\r\n" +
                                    $"                + MainPluginName: {manifest.MainPluginName}\r\n" +
                                    $"                + MainPluginAuthor: {manifest.MainPluginAuthor}\r\n" +
                                    $"                + MainPluginDescription: {manifest.MainPluginDescription}\r\n" +
                                    $"                + PluginStandardVersion: {manifest.PluginStandardVersion}\r\n" +
                                    $"                + PluginVersion: {manifest.PluginVersion}\r\n" +
                                    $"                + PluginCreationDate: {manifest.PluginCreationDate}\r\n" +
                                    $"                + ManifestDate: {manifest.ManifestDate}\r\n" +
                                    $"                + Assets: {manifest.Assets.Count} assets found";
                logger.LogInformation(logMessage);
                return (currentCdnUrl, manifest);
            }
            catch (Exception e)
            {
                logger.LogError(e, "  + Failed while trying to parse manifest from {manifest}", manifestUrl);
            }
        }

        return (null, null);
    }
}

[JsonSerializable(typeof(PluginManifest))]
public partial class PluginManifestJsonContext : JsonSerializerContext;

public class PluginManifest
{
    public required string MainLibraryName { get; init; }
    public required string MainPluginName { get; init; }
    public string? MainPluginAuthor { get; init; }
    public string? MainPluginDescription { get; init; }

#if !USELIGHTWEIGHTJSONPARSER
    [JsonConverter(typeof(Utf8SpanParsableJsonConverter<GameVersion>))]
#endif
    public GameVersion PluginStandardVersion { get; init; }

#if !USELIGHTWEIGHTJSONPARSER
    [JsonConverter(typeof(Utf8SpanParsableJsonConverter<GameVersion>))]
#endif
    public GameVersion PluginVersion { get; init; }

#if !USELIGHTWEIGHTJSONPARSER
    [JsonConverter(typeof(Utf16SpanParsableJsonConverter<DateTimeOffset>))]
#endif
    public DateTimeOffset PluginCreationDate { get; init; }

#if !USELIGHTWEIGHTJSONPARSER
    [JsonConverter(typeof(Utf16SpanParsableJsonConverter<DateTimeOffset>))]
#endif
    public DateTimeOffset ManifestDate { get; init; }

    public required List<PluginManifestAsset> Assets { get; init; }
}

public class PluginManifestAsset
{
    public required string? FilePath { get; set; }
    public required byte[] FileHash { get; set; }
    public long Size { get; set; }
}