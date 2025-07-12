using Hi3Helper.Plugin.Core.Management;
using System;
using System.Collections.Generic;

#if !USELIGHTWEIGHTJSONPARSER
using Hi3Helper.Plugin.Core.Utility.Json.Converters;
using System.Text.Json.Serialization;
#else
using Hi3Helper.Plugin.Core.Utility.Json;
using System.IO;
using System.Security.Cryptography;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
#endif

namespace Hi3Helper.Plugin.Core.Update;

#if !USELIGHTWEIGHTJSONPARSER
[JsonSerializable(typeof(SelfUpdateReferenceInfo))]
[JsonSourceGenerationOptions(IndentSize = 2, NewLine = "\n", WriteIndented = true, IndentCharacter = ' ')]
public partial class SelfUpdateReferenceInfoContext : JsonSerializerContext;
#endif

/// <summary>
/// Contains information about the update data reference for the plugin.
/// </summary>
public class SelfUpdateReferenceInfo
#if USELIGHTWEIGHTJSONPARSER
    : IJsonElementParsable<SelfUpdateReferenceInfo>,
      IJsonStreamParsable<SelfUpdateReferenceInfo>
#endif
{
    /// <summary>
    /// Gets the version of plugin.
    /// </summary>
#if !USELIGHTWEIGHTJSONPARSER
    [JsonConverter(typeof(Utf8SpanParsableJsonConverter<GameVersion>))]
#endif
    public GameVersion PluginVersion { get; set; }

    /// <summary>
    /// Gets the standard/core library version of the API of the plugin.
    /// </summary>
#if !USELIGHTWEIGHTJSONPARSER
    [JsonConverter(typeof(Utf8SpanParsableJsonConverter<GameVersion>))]
#endif
    public GameVersion PluginStandardVersion { get; set; }

    /// <summary>
    /// Creation date of the plugin.
    /// </summary>
#if !USELIGHTWEIGHTJSONPARSER
    [JsonConverter(typeof(Utf16SpanParsableJsonConverter<DateTimeOffset>))]
#endif
    public DateTimeOffset PluginCreationDate { get; set; }

    /// <summary>
    /// Gets the compilation date of the plugin.
    /// </summary>
#if !USELIGHTWEIGHTJSONPARSER
    [JsonConverter(typeof(Utf16SpanParsableJsonConverter<DateTimeOffset>))]
#endif
    public DateTimeOffset ManifestDate { get; set; }

    /// <summary>
    /// Gets the path of the main library file.
    /// </summary>
    public required string MainLibraryName { get; set; }

    /// <summary>
    /// Gets the plugin name info.
    /// </summary>
    public string? MainPluginName { get; set; }

    /// <summary>
    /// Gets the plugin's author info.
    /// </summary>
    public string? MainPluginAuthor { get; set; }

    /// <summary>
    /// Gets the plugin's description info.
    /// </summary>
    public string? MainPluginDescription { get; set; }

    /// <summary>
    /// Gets the list of the plugin files.
    /// </summary>
    public required List<SelfUpdateAssetInfo> Assets { get; set; }

#if USELIGHTWEIGHTJSONPARSER
    public static SelfUpdateReferenceInfo ParseFrom(Stream stream, bool isDisposeStream = false, JsonDocumentOptions options = default)
        => ParseFromAsync(stream, isDisposeStream, options, CancellationToken.None).Result;

    public static async Task<SelfUpdateReferenceInfo> ParseFromAsync(Stream stream, bool isDisposeStream = false, JsonDocumentOptions options = default, CancellationToken token = default)
    {
        try
        {
            using JsonDocument document = await JsonDocument.ParseAsync(stream, options, token).ConfigureAwait(false);
            return await Task.Factory.StartNew(() => ParseFrom(document.RootElement), token);
        }
        finally
        {
            if (isDisposeStream)
            {
                await stream.DisposeAsync();
            }
        }
    }

    public static SelfUpdateReferenceInfo ParseFrom(JsonElement rootElement)
    {
        string mainLibraryNameValue = rootElement.GetStringNonNullOrEmpty(nameof(MainLibraryName)); // Parse MainLibraryName
        string? mainPluginNameValue = rootElement.GetString(nameof(MainPluginName)); // Parse MainPluginName
        string? mainPluginAuthorValue = rootElement.GetString(nameof(MainPluginAuthor)); // Parse MainPluginAuthor
        string? mainPluginDescriptionValue = rootElement.GetString(nameof(MainPluginDescription)); // Parse MainPluginDescription
        GameVersion pluginVersionValue = rootElement.GetValue<GameVersion>(nameof(PluginVersion)); // Parse PluginVersion
        GameVersion pluginStandardVersionValue = rootElement.GetValue<GameVersion>(nameof(PluginStandardVersion)); // Parse PluginStandardVersion
        DateTimeOffset pluginCreationDateValue = rootElement.GetValue<DateTimeOffset>(nameof(PluginCreationDate)); // Parse PluginCreationDate
        DateTimeOffset manifestDateValue = rootElement.GetValue<DateTimeOffset>(nameof(ManifestDate)); // Parse ManifestDate

        if (!rootElement.TryGetProperty(nameof(Assets), out JsonElement assetsArray))
        {
            throw new JsonException("Assets property cannot be undefined!");
        }

        List<SelfUpdateAssetInfo> assetList = [];
        foreach (JsonElement asset in assetsArray.EnumerateArray())
        {
            string filePathValueAsString = asset.GetStringNonNullOrEmpty(nameof(SelfUpdateAssetInfo.FilePath));

            if (!asset.TryGetValue(nameof(SelfUpdateAssetInfo.Size), out long sizeAsLong))
            {
                throw new JsonException("Assets.Size cannot be empty or null!");
            }

            if (!asset.TryGetBytesFromBase64NonNull(nameof(SelfUpdateAssetInfo.FileHash), out byte[] fileHashValueAsBytes))
            {
                throw new JsonException("Assets.FileHash cannot be empty or null!");
            }

            assetList.Add(new SelfUpdateAssetInfo
            {
                FilePath = filePathValueAsString,
                FileHash = fileHashValueAsBytes,
                Size = sizeAsLong
            });
        }

        if (assetList.Count == 0)
        {
            throw new JsonException("Assets cannot be empty or null!");
        }

        SelfUpdateReferenceInfo refInfoRet = new SelfUpdateReferenceInfo
        {
            MainLibraryName = mainLibraryNameValue,
            MainPluginName = mainPluginNameValue,
            MainPluginAuthor = mainPluginAuthorValue,
            MainPluginDescription = mainPluginDescriptionValue,
            Assets = assetList,
            PluginVersion = pluginVersionValue,
            PluginStandardVersion = pluginStandardVersionValue,
            PluginCreationDate = pluginCreationDateValue,
            ManifestDate = manifestDateValue
        };

        return refInfoRet;
    }
#endif
}

/// <summary>
/// Contains the information about asset files related with the plugin.
/// </summary>
public class SelfUpdateAssetInfo
{
    /// <summary>
    /// Relative path of the asset file.
    /// </summary>
    public required string FilePath { get; set; }

    /// <summary>
    /// Size of the asset file.
    /// </summary>
#if !USELIGHTWEIGHTJSONPARSER
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
#endif
    public long Size { get; set; }

    /// <summary>
    /// Hash of the asset file in <see cref="MD5"/> format.
    /// </summary>
    public required byte[] FileHash { get; set; }
}
