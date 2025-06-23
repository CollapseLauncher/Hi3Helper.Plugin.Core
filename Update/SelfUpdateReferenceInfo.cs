using Hi3Helper.Plugin.Core.Management;
using Hi3Helper.Plugin.Core.Utility.Json.Converters;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Hi3Helper.Plugin.Core.Update;

[JsonSerializable(typeof(SelfUpdateReferenceInfo))]
[JsonSourceGenerationOptions(IndentSize = 2, NewLine = "\n", WriteIndented = true, IndentCharacter = ' ')]
public partial class SelfUpdateReferenceInfoContext : JsonSerializerContext;

public class SelfUpdateReferenceInfo
{
    [JsonConverter(typeof(Utf8SpanParsableJsonConverter<GameVersion>))]
    public GameVersion PluginVersion { get; set; }

    [JsonConverter(typeof(Utf16SpanParsableJsonConverter<DateTimeOffset>))]
    public DateTimeOffset PluginCreationDate { get; set; }

    [JsonConverter(typeof(Utf16SpanParsableJsonConverter<DateTimeOffset>))]
    public DateTimeOffset ManifestDate { get; set; }

    public required string MainLibraryName { get; set; }

    public required List<SelfUpdateAssetInfo> Assets { get; set; }
}

public class SelfUpdateAssetInfo
{
    public required string FilePath { get; set; }

    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long Size { get; set; }

    public required byte[] FileHash { get; set; }
}
