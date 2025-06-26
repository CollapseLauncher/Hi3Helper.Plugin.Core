#if !USELIGHTWEIGHTJSONPARSER
using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hi3Helper.Plugin.Core.Utility.Json.Converters;

/// <summary>
/// Converts any struct with both <see cref="ISpanParsable{TSelf}"/> and <see cref="ISpanFormattable"/> into the string representation.
/// </summary>
/// <typeparam name="TSpan">Where it's a member of both <see cref="ISpanParsable{TSelf}"/> and <see cref="ISpanFormattable"/></typeparam>
public class Utf16SpanParsableJsonConverter<TSpan> : JsonConverter<TSpan>
    where TSpan : struct,
                  ISpanParsable<TSpan>,
                  ISpanFormattable,
                  IEquatable<TSpan>
{
    private readonly TSpan _defaultValue = default;

    public override TSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        Span<char> chars = stackalloc char[256];
        if (!Encoding.UTF8.TryGetChars(reader.ValueSpan, chars, out int written))
        {
            throw new InvalidOperationException("Buffer is too small to copy from UTF-8 to UTF-16 chars!");
        }

        if (TSpan.TryParse(chars[..written], null, out TSpan spanOutput))
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

        Span<char> writeBuffer = stackalloc char[256];
        if (value.TryFormat(writeBuffer, out int written, ReadOnlySpan<char>.Empty, null))
        {
            writer.WriteStringValue(writeBuffer[..written]);
        }
    }
}
#endif