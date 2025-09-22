#if !USELIGHTWEIGHTJSONPARSER
using System;
using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hi3Helper.Plugin.Core.Utility.Json.Converters;

/// <summary>
/// Converts Hex String to Array of any struct type that's bit-able.
/// </summary>
/// <typeparam name="TStruct">A type of struct that's bit-able.</typeparam>
public class HexStringToArrayJsonConverter<TStruct> : JsonConverter<TStruct[]?>
        where TStruct : unmanaged
{
    public override bool CanConvert(Type typeToConvert) => true;

    public override unsafe TStruct[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.ValueSequence.IsEmpty)
        {
            return [];
        }

        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        if (reader.ValueSpan.Length % 2 != 0)
        {
            throw new InvalidOperationException("The hex value is malformed due to uneven length (cannot be divided by 2)");
        }

        int        bufferSize = reader.ValueSpan.Length / 2;
        TStruct[]  result     = GC.AllocateUninitializedArray<TStruct>(bufferSize);
        Span<byte> byteBuffer = MemoryMarshal.AsBytes(result.AsSpan());

        // For now on .NET, we don't have methods to explicitly convert Hex from UTF-8 string so....
        // we need to convert it to Unicode string first then parse it.
        char[]? charBuffer = reader.ValueSpan.Length > 2048
                ? ArrayPool<char>.Shared.Rent(reader.ValueSpan.Length)
                : null;

        scoped Span<char> charSpan = charBuffer ?? stackalloc char[reader.ValueSpan.Length];
        try
        {
            if (!Encoding.UTF8.TryGetChars(reader.ValueSpan, charSpan, out int written))
            {
                throw new InvalidOperationException("The hex value is malformed as it is not a valid UTF-8 string!");
            }

            OperationStatus operationStatus =
                    Convert.FromHexString(charSpan[..written], byteBuffer, out _, out written);

            return operationStatus == OperationStatus.Done
                    ? result
                    : throw new InvalidOperationException($"FromHexString returns an unsuccessful return value! {operationStatus}");
        }
        finally
        {
            if (charBuffer != null)
            {
                ArrayPool<char>.Shared.Return(charBuffer);
            }
        }
    }

    public override unsafe void Write(Utf8JsonWriter writer, TStruct[]? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        if (value.Length == 0)
        {
            writer.WriteStringValue(ReadOnlySpan<byte>.Empty);
            return;
        }

        int sizeOf     = sizeof(TStruct);
        int bufferSize = sizeOf * value.Length;

        Span<byte> valueAsBytes = MemoryMarshal.AsBytes(value.AsSpan());

        // Same as read process, we need to convert the string from TryToHexStringLower from Unicode to UTF-8
        int stringBufferSize = bufferSize * 2;

        char[]? buffer = stringBufferSize > 2048
                ? ArrayPool<char>.Shared.Rent(stringBufferSize)
                : null;

        byte[]? bufferUtf8 = stringBufferSize > 2048
                ? ArrayPool<byte>.Shared.Rent(stringBufferSize)
                : null;

        scoped Span<char> bufferSpan     = buffer ?? stackalloc char[stringBufferSize];
        scoped Span<byte> bufferUtf8Span = bufferUtf8 ?? stackalloc byte[stringBufferSize];

        try
        {
            if (!Convert.TryToHexStringLower(valueAsBytes, bufferSpan, out int written))
            {
                throw new InvalidOperationException("Failed to write struct to Hex");
            }

            if (!Encoding.UTF8.TryGetBytes(bufferSpan[..written], bufferUtf8Span, out written))
            {
                throw new InvalidOperationException("Failed to post-write the Hex to UTF-8 string");
            }

            writer.WriteStringValue(bufferUtf8Span[..written]);
        }
        finally
        {
            if (buffer != null)
            {
                ArrayPool<char>.Shared.Return(buffer);
            }

            if (bufferUtf8 != null)
            {
                ArrayPool<byte>.Shared.Return(bufferUtf8);
            }
        }
    }
}
#endif