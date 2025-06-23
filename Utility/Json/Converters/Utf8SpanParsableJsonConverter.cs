using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hi3Helper.Plugin.Core.Utility.Json.Converters;

/// <summary>
/// Converts any struct with both <see cref="IUtf8SpanParsable{TSelf}"/> and <see cref="IUtf8SpanFormattable"/> into the string representation.
/// </summary>
/// <typeparam name="TSpan">Where it's a member of both <see cref="IUtf8SpanParsable{TSelf}"/> and <see cref="IUtf8SpanFormattable"/></typeparam>
public class Utf8SpanParsableJsonConverter<TSpan> : JsonConverter<TSpan>
    where TSpan : struct,
                  IUtf8SpanParsable<TSpan>,
                  IUtf8SpanFormattable,
                  IEquatable<TSpan>
{
    private readonly TSpan _defaultValue = default;

    public override TSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (TSpan.TryParse(reader.ValueSpan, null, out TSpan spanOutput))
        {
            return spanOutput;
        }

        throw new InvalidOperationException($"Value is not a valid {typeof(TSpan).Name}");
    }

    public override void Write(Utf8JsonWriter writer, TSpan value, JsonSerializerOptions options)
    {
        if ((options.DefaultIgnoreCondition.HasFlag(JsonIgnoreCondition.WhenWritingDefault) ||
             options.DefaultIgnoreCondition.HasFlag(JsonIgnoreCondition.WhenWritingNull)) &&
            value.Equals(_defaultValue))
        {
            return;
        }

        Span<byte> writeBuffer = stackalloc byte[256];
        if (value.TryFormat(writeBuffer, out int written, ReadOnlySpan<char>.Empty, null))
        {
            writer.WriteStringValue(writeBuffer[..written]);
        }
    }
}
