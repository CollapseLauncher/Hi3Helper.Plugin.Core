using Hi3Helper.Plugin.Core.Management;
using Hi3Helper.Plugin.Core.Utility.Json.Converters;
using System;
using System.Text.Json.Serialization;

namespace Hi3Helper.Plugin.Core.Update;

[JsonSerializable(typeof(SelfUpdateReferenceInfo))]
public partial class SelfUpdateReferenceInfoContext : JsonSerializerContext;

public class SelfUpdateReferenceInfo
{
    public required string FilePath { get; set; }

    [JsonConverter(typeof(Utf8SpanParsableJsonConverter<GameVersion>))]
    public GameVersion PluginVersion { get; set; }

    [JsonConverter(typeof(Utf16SpanParsableJsonConverter<DateTimeOffset>))]
    public DateTimeOffset ReleaseDate { get; set; }

    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public long Size { get; set; }
}
