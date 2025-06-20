using System;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hi3Helper.Plugin.Core.Utility.Json.Converters;

/// <summary>
/// Converts any struct with both <see cref="IUtf8SpanParsable{TSelf}"/> and <see cref="IUtf8SpanFormattable"/> into <see cref="byte"/> array and vice versa.
/// </summary>
/// <typeparam name="TSpan">Where it's a member of both <see cref="IUtf8SpanParsable{TSelf}"/> and <see cref="IUtf8SpanFormattable"/></typeparam>
public class Utf8SpanParsableToBytesJsonConverter<TSpan> : JsonConverter<byte[]?>
    where TSpan : unmanaged,
                  IUtf8SpanParsable<TSpan>,
                  IUtf8SpanFormattable,
                  IEquatable<TSpan>
{
    public override bool CanConvert(Type typeToConvert) => true;

    public override unsafe byte[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (!TSpan.TryParse(reader.ValueSpan, null, out TSpan value))
        {
            throw new InvalidOperationException("Cannot parse UTF-8 JSON value!");
        }

        // Try to convert the value based ont its byte size instead of checking the type one-by-one
        // since also the IUtf8SpanParsable<T> members are simple primitive structs as well.
        int sizeOf = sizeof(TSpan);
        void* valueP = Unsafe.AsPointer(ref value);

        byte[] resultBuffer = new byte[sizeOf];
        if (!new Span<byte>(valueP, sizeOf).TryCopyTo(resultBuffer))
        {
            throw new InvalidOperationException("Cannot unsafely copy the value buffer to result!");
        }

        return resultBuffer;
    }

    public override unsafe void Write(Utf8JsonWriter writer, byte[]? value, JsonSerializerOptions options)
    {
        // Handles null data
        if (value == null)
        {
            // If the null is ignored, then ignore writing the value.
            if (options.DefaultIgnoreCondition.HasFlag(JsonIgnoreCondition.WhenWritingNull))
                return;

            // Otherwise, write the null value
            writer.WriteNullValue();
            return;
        }

        // Throw if the length of the value buffer isn't the same as the struct size.
        if (value.Length != sizeof(TSpan))
        {
            throw new ArgumentOutOfRangeException(nameof(value), $"Size of the buffer isn't equal to the size of the struct type! Struct Type Size: {sizeof(TSpan)} != Data Size: {value.Length}");
        }

        // Get the pointer of the data.
        void* valueP = Unsafe.AsPointer(ref value[0]);
        TSpan* dataP = (TSpan*)valueP;

        // Return if the dataP is null.
        if (dataP == null)
        {
            return;
        }

        // If the ignore condition for writing default value is enabled, then ignore writing the value.
        if (options.DefaultIgnoreCondition.HasFlag(JsonIgnoreCondition.WhenWritingDefault) &&
            dataP->Equals(default))
        {
            return;
        }

        // Box the value to object
        object dataBoxed = *dataP;

        // Check if the type is a boolean
        if (dataBoxed is bool dataAsBool)
        {
            writer.WriteBooleanValue(dataAsBool);
            return;
        }

        // Handles the number if it has WriteAsString flag
        if (!options.NumberHandling.HasFlag(JsonNumberHandling.WriteAsString))
        {
            switch (dataBoxed)
            {
                // First, check if the data is floating type numbers.
                case decimal asDecimal:
                    writer.WriteNumberValue(asDecimal);
                    return;
                case double asDouble:
                    writer.WriteNumberValue(asDouble);
                    return;
                case float asFloat:
                    writer.WriteNumberValue(asFloat);
                    return;
                // Second, check if the data is an integer number (signed or unsigned).
                case long asLong:
                    writer.WriteNumberValue(asLong);
                    return;
                case ulong asULong:
                    writer.WriteNumberValue(asULong);
                    return;
                case int asInt:
                    writer.WriteNumberValue(asInt);
                    return;
                case uint asUInt:
                    writer.WriteNumberValue(asUInt);
                    return;
                case short asShort:
                    writer.WriteNumberValue(asShort);
                    return;
                case ushort asUShort:
                    writer.WriteNumberValue(asUShort);
                    return;
                case sbyte asSByte:
                    writer.WriteNumberValue(asSByte);
                    return;
                case byte asByte:
                    writer.WriteNumberValue(asByte);
                    return;
            }
        }

        // Otherwise, if the data isn't a number and is formattable, then write as formattable UTF-8 string.
        Span<byte> resultWriteP = stackalloc byte[256];
        if (!dataP->TryFormat(resultWriteP, out int bytesWritten, ReadOnlySpan<char>.Empty, null))
        {
            throw new InvalidOperationException($"Cannot write a format data type of {typeof(TSpan).Name}");
        }

        // Write to the JSON directly
        writer.WriteStringValue(resultWriteP[..bytesWritten]);
    }
}