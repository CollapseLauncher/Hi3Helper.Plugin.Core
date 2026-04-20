#if !USELIGHTWEIGHTJSONPARSER
using System;
using System.Buffers;
using System.Buffers.Text;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hi3Helper.Plugin.Core.Utility.Json.Converters;

/// <summary>
/// Converts a string of bytes (in either Hex or Base64 format) into an array of bytes.<br/>
/// This converter writes back to Base64 format for its JSON value.
/// </summary>
public class BytesStringToArrayJsonConverter : BytesStringToArrayJsonConverter<byte>;

/// <summary>
/// Converts a string of bytes (in either Hex or Base64 format) into an array of structs that are bit-able (unmanaged).<br/>
/// This converter writes back to Base64 format for its JSON value.
/// </summary>
/// <typeparam name="TStruct">Type of struct that's bit-able (unmanaged).</typeparam>
public class BytesStringToArrayJsonConverter<TStruct> : JsonConverter<TStruct[]?>
    where TStruct : unmanaged
{
    public override bool CanConvert(Type typeToConvert) => true;

    public override unsafe TStruct[]? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.ValueSpan.IsEmpty)
        {
            return [];
        }

        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }

        int sizeOfStruct = sizeof(TStruct);
        int utf8DataLength = GetUtf8DataSize(ref reader);
        ReadOnlySpan<byte> utf8SampleData = TryGetSampleUtf8Data(ref reader);

        Unsafe.SkipInit(out byte[]? utf8DataBuffer);
        try
        {
            // Try decode in Hex first.
            if (utf8DataLength % 2 == 0)
            {
                // If the sample data length is odd, then trim the length (to make it even).
                if (utf8SampleData.Length % 2 != 0)
                {
                    // Cut to up-to 16 bytes for sample data
                    utf8SampleData = utf8SampleData[..Math.Min(utf8SampleData.Length - 1, 16)];
                }

                // Try to decode the sample data.
                Span<byte> hexTestDecodeBuffer = stackalloc byte[utf8SampleData.Length / 2];
                OperationStatus hexTestDecodeStatus = Convert.FromHexString(utf8SampleData, hexTestDecodeBuffer, out _, out _);
                if (hexTestDecodeStatus != OperationStatus.Done)
                {
                    goto ResumeToBase64;
                }

                // Alloc buffer
                utf8DataBuffer = utf8DataLength > 2048 ? ArrayPool<byte>.Shared.Rent(utf8DataLength) : null;
                Span<byte> utf8DataSpan = utf8DataBuffer ?? stackalloc byte[utf8DataLength];

                GetUtf8Data(ref reader, utf8DataSpan);
                utf8DataSpan = utf8DataSpan[..utf8DataLength];

                // Decode the hex string
                int hexBytesToDecodeLength = utf8DataLength / 2;
                TStruct[] hexDecodedStructBuffer = GC.AllocateUninitializedArray<TStruct>(hexBytesToDecodeLength / sizeof(TStruct));
                Span<byte> hexDecodedStructSpan = MemoryMarshal.AsBytes(hexDecodedStructBuffer.AsSpan());

                hexTestDecodeStatus = Convert.FromHexString(utf8DataSpan, hexDecodedStructSpan, out _, out _);
                if (hexTestDecodeStatus != OperationStatus.Done)
                {
                    ThrowFullHexDecodeFailedException(hexTestDecodeStatus);
                }

                return hexDecodedStructBuffer;
            }

        ResumeToBase64:
            // Then try Base64Url.
            if (Base64Url.IsValid(utf8SampleData))
            {
                // Alloc buffer
                utf8DataBuffer = utf8DataLength > 2048 ? ArrayPool<byte>.Shared.Rent(utf8DataLength) : null;
                Span<byte> utf8DataSpan = utf8DataBuffer ?? stackalloc byte[utf8DataLength];

                GetUtf8Data(ref reader, utf8DataSpan);
                utf8DataSpan = utf8DataSpan[..utf8DataLength];

                // Decode in place
                int base64DecodedLength = Base64Url.DecodeFromUtf8InPlace(utf8DataSpan);
                if (base64DecodedLength % sizeOfStruct != 0)
                {
                    ThrowInvalidStructSizeToBufferException(base64DecodedLength, sizeOfStruct);
                }
                utf8DataSpan = utf8DataSpan[..base64DecodedLength];

                // Copy to heap buffer
                TStruct[] base64DecodedStructBuffer = GC.AllocateUninitializedArray<TStruct>(base64DecodedLength / sizeof(TStruct));
                Span<byte> base64DecodedStructSpan = MemoryMarshal.AsBytes(base64DecodedStructBuffer.AsSpan());

                utf8DataSpan.CopyTo(base64DecodedStructSpan);
                return base64DecodedStructBuffer;
            }

            // Lastly, try Base64Url.
            if (Base64.IsValid(utf8SampleData))
            {
                // Alloc buffer
                utf8DataBuffer = utf8DataLength > 2048 ? ArrayPool<byte>.Shared.Rent(utf8DataLength) : null;
                Span<byte> utf8DataSpan = utf8DataBuffer ?? stackalloc byte[utf8DataLength];

                GetUtf8Data(ref reader, utf8DataSpan);
                utf8DataSpan = utf8DataSpan[..utf8DataLength];

                // Decode in place
                OperationStatus base64DecodeStatus = Base64.DecodeFromUtf8InPlace(utf8DataSpan, out int base64DecodedLength);
                if (base64DecodeStatus != OperationStatus.Done)
                {
                    ThrowFullBase64DecodeFailedException(base64DecodeStatus);
                }

                if (base64DecodedLength % sizeOfStruct != 0)
                {
                    ThrowInvalidStructSizeToBufferException(base64DecodedLength, sizeOfStruct);
                }
                utf8DataSpan = utf8DataSpan[..base64DecodedLength];

                // Copy to heap buffer
                TStruct[] base64DecodedStructBuffer = GC.AllocateUninitializedArray<TStruct>(base64DecodedLength / sizeof(TStruct));
                Span<byte> base64DecodedStructSpan = MemoryMarshal.AsBytes(base64DecodedStructBuffer.AsSpan());

                utf8DataSpan.CopyTo(base64DecodedStructSpan);
                return base64DecodedStructBuffer;
            }
        }
        finally
        {
            if (utf8DataBuffer != null)
            {
                ArrayPool<byte>.Shared.Return(utf8DataBuffer);
            }
        }

        // Gave up :(
        throw new InvalidOperationException("Value must be in Base64 or Hex string format!");

        static void ThrowInvalidStructSizeToBufferException(int sizeOfUtf8Data, int sizeOfStruct) =>
            throw new InvalidOperationException($"The decoded data length is not a multiple of the struct size ({sizeOfUtf8Data} % {sizeOfStruct} = {sizeOfUtf8Data % sizeOfStruct}).");

        static void ThrowFullHexDecodeFailedException(OperationStatus status) =>
            throw new InvalidOperationException($"Failed while trying to fully decode the Hex value with status: {status}.");

        static void ThrowFullBase64DecodeFailedException(OperationStatus status) =>
            throw new InvalidOperationException($"Failed while trying to fully decode the Base64 value with status: {status}.");
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

        Span<byte> dataBytesSpan = MemoryMarshal.AsBytes(value.AsSpan());

        // Encode the data bytes in-place to base64.
        OperationStatus status = Base64.EncodeToUtf8InPlace(dataBytesSpan, dataBytesSpan.Length, out int base64EncodedLength);
        if (status != OperationStatus.Done)
        {
            ThrowFullBase64EncodeFailedException(status);
        }

        writer.WriteStringValue(dataBytesSpan[..base64EncodedLength]);
        return;

        static void ThrowFullBase64EncodeFailedException(OperationStatus status) =>
            throw new InvalidOperationException($"Failed while trying to fully encode the struct value into Base64 with status: {status}.");
    }

    private static int GetUtf8DataSize(ref Utf8JsonReader reader)
        => reader.HasValueSequence ? (int)reader.ValueSequence.Length : reader.ValueSpan.Length;

    private static void GetUtf8Data(ref Utf8JsonReader reader, scoped Span<byte> buffer)
    { 
        if (reader.HasValueSequence)
        {
            reader.ValueSequence.CopyTo(buffer);
        }
        else
        {
            reader.ValueSpan.CopyTo(buffer);
        }
    }

    private static ReadOnlySpan<byte> TryGetSampleUtf8Data(ref Utf8JsonReader reader)
        => reader.HasValueSequence ? reader.ValueSequence.FirstSpan : reader.ValueSpan;
}
#endif