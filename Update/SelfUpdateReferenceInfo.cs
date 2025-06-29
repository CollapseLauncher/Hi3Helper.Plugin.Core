using Hi3Helper.Plugin.Core.Management;
using System;
using System.Collections.Generic;

#if !USELIGHTWEIGHTJSONPARSER
using Hi3Helper.Plugin.Core.Utility.Json.Converters;
using System.Text.Json.Serialization;
#else
using Hi3Helper.Plugin.Core.Utility.Json;
using System.IO;
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

public class SelfUpdateReferenceInfo
#if USELIGHTWEIGHTJSONPARSER
    : IJsonElementParsable<SelfUpdateReferenceInfo>,
      IJsonStreamParsable<SelfUpdateReferenceInfo>
#endif
{
#if !USELIGHTWEIGHTJSONPARSER
    [JsonConverter(typeof(Utf8SpanParsableJsonConverter<GameVersion>))]
#endif
    public GameVersion PluginVersion { get; set; }

#if !USELIGHTWEIGHTJSONPARSER
    [JsonConverter(typeof(Utf16SpanParsableJsonConverter<DateTimeOffset>))]
#endif
    public DateTimeOffset PluginCreationDate { get; set; }

#if !USELIGHTWEIGHTJSONPARSER
    [JsonConverter(typeof(Utf16SpanParsableJsonConverter<DateTimeOffset>))]
#endif
    public DateTimeOffset ManifestDate { get; set; }

    public required string MainLibraryName { get; set; }

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
        GameVersion pluginVersionValue = rootElement.GetValue<GameVersion>(nameof(PluginVersion)); // Parse PluginVersion
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
            Assets = assetList,
            PluginVersion = pluginVersionValue,
            PluginCreationDate = pluginCreationDateValue,
            ManifestDate = manifestDateValue
        };

        return refInfoRet;
    }
#endif
}

public class SelfUpdateAssetInfo
{
    public required string FilePath { get; set; }

#if !USELIGHTWEIGHTJSONPARSER
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
#endif
    public long Size { get; set; }

    public required byte[] FileHash { get; set; }
}
