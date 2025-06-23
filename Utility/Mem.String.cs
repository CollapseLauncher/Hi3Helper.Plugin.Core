using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
// ReSharper disable UnusedMember.Global
// ReSharper disable GrammarMistakeInComment

namespace Hi3Helper.Plugin.Core.Utility;

public static partial class Mem
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Span<T> CreateSpanFromNullTerminated<T>(this PluginDisposableMemory<T> memory)
        where T : unmanaged
        => CreateSpanFromNullTerminated<T>(memory.AsPointer());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Span<T> CreateSpanFromNullTerminated<T>(void* ptr)
        where T : unmanaged
    {
        ThrowIfNotByteOrChar<T>(out bool isChar);

        if (ptr == null)
        {
            return [];
        }

        return isChar ?
            new Span<T>(ptr, SpanHelpers.IndexOfNullCharacter((char*)ptr)) :
            new Span<T>(ptr, SpanHelpers.IndexOfNullByte((byte*)ptr));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe string? CreateStringFromNullTerminated<T>(this PluginDisposableMemory<T> memory)
        where T : unmanaged
        => memory.IsEmpty ? null : CreateStringFromNullTerminated(memory.AsPointer());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe string CreateStringFromNullTerminated<T>(T* source)
        where T : unmanaged
    {
        ThrowIfNotByteOrChar<T>(out bool isChar);

        int len;
        if (isChar)
        {
            len = SpanHelpers.IndexOfNullCharacter((char*)source);
            return new string((char*)source, 0, len);
        }

        len = SpanHelpers.IndexOfNullByte((byte*)source);
        return Encoding.UTF8.GetString((byte*)source, len);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyToUtf8(this ReadOnlySpan<char> source, PluginDisposableMemory<byte> destination)
        => source.CopyToUtf8(destination.AsSpan());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CopyToUtf8(this ReadOnlySpan<char> source, Span<byte> destination)
    {
        if (destination.Length < Encoding.UTF8.GetByteCount(source))
        {
            throw new ArgumentException("Destination span is not large enough to hold the UTF-8 encoded string.", nameof(destination));
        }

        _ = Encoding.UTF8.GetBytes(source, destination);
    }

    public static unsafe byte* Utf16SpanToUtf8Unmanaged(this ReadOnlySpan<char> span)
    {
        if (span.IsEmpty)
            return null;

        int exactByteCount = checked(Encoding.UTF8.GetByteCount(span) + 1); // + 1 for null terminator
        byte* mem = (byte*)Marshal.AllocCoTaskMem(exactByteCount);
        Span<byte> buffer = new(mem, exactByteCount);

        int byteCount = Encoding.UTF8.GetBytes(span, buffer);
        buffer[byteCount] = 0; // null-terminate
        return mem;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ThrowIfNotByteOrChar<T>(out bool isChar)
        where T : unmanaged
    {
        isChar = false;
        Type currentType = typeof(T);

        if (currentType == typeof(char))
        {
            isChar = true;
            return;
        }

        if (currentType != typeof(byte)) throw new InvalidOperationException("Type must be a char or byte!");
    }

    public static string CombineUrlFromString(this string? baseUrl, params ReadOnlySpan<string?> segments)
        => CombineUrlFromString(baseUrl.AsSpan(), segments);

    public static unsafe string CombineUrlFromString(ReadOnlySpan<char> baseUrl, params ReadOnlySpan<string?> segments)
    {
        // Assign the size of a char as constant
        const uint sizeOfChar = sizeof(char);

        // Get the base URL length and decrement by 1 if the end of the index (^1)
        // is a '/' character. Otherwise, nothing to decrement.
        // 
        // Once we get a length of the base URL, get a sum of all lengths
        // of the segment's span.
        int baseUrlLen = baseUrl.Length - (baseUrl[^1] == '/' ? 1 : 0);
        int bufferLen = baseUrlLen + SumSegmentsLength(segments);
        uint toWriteBase = (uint)baseUrlLen;

        // Allocate temporary buffer from the shared ArrayPool<T>
        char[] buffer = ArrayPool<char>.Shared.Rent(bufferLen);

        // Here we start to do something UNSAFE >:)
        // Get the base and last (to written position) pointers of the buffer array.
        fixed (char* bufferPtr = &MemoryMarshal.GetArrayDataReference(buffer))
        {
            char* bufferWrittenPtr = bufferPtr;

            // Get a base pointer of the baseUrl span
            fixed (char* baseUrlPtr = &MemoryMarshal.GetReference(baseUrl))
            {
                // Perform intrinsic copy for the specific block of memory from baseUrlPtr
                // into the buffer pointer.
                Unsafe.CopyBlock(bufferWrittenPtr, baseUrlPtr, toWriteBase * sizeOfChar);
                bufferWrittenPtr += toWriteBase;
                try
                {
                    // Set the initial position of the segment index
                    int i = 0;

                // Perform the segment copy loop routine
                CopySegments:
                    // If the index is equal to the length of the segment, which means...
                    // due to i being 0, it should expect the length of the segments span as well.
                    // Means, if 0 == 0, then quit from CopySegments routine and jump right
                    // into CreateStringFast routine.
                    if (i == segments.Length)
                        goto CreateStringFast;

                    // Get a span of the current segment while in the meantime, trim '/' character
                    // from the start and the end of the span. In the meantime, increment
                    // the index of the segments span.
                    ReadOnlySpan<char> segment = segments[i++].AsSpan().Trim('/');
                    // If the segment span is actually empty, (means either the initial value or
                    // after it's getting trimmed [for example, "//"]), then move to another
                    // segment to merge.
                    if (segment.IsEmpty) goto CopySegments;

                    // Check if the segment starts with '?' character (means the segment is a query
                    // and not a relative path), then write a '/' character into the buffer and moving
                    // by 1 byte of the index.
                    bool isQuery = segment[0] == '?';
                    if (!isQuery)
                        *bufferWrittenPtr++ = '/';

                    // Get a base pointer of the current segment and get its length.
                    uint segmentLen = (uint)segment.Length;
                    fixed (void* segmentPtr = &MemoryMarshal.GetReference(segment))
                    {
                        // Perform the intrinsic copy for the specific block of memory from the
                        // current segment pointer into the buffer pointer.
                        Unsafe.CopyBlock(bufferWrittenPtr, segmentPtr, segmentLen * sizeOfChar);
                        // Move the position of the written buffer pointer
                        bufferWrittenPtr += segmentLen;
                        // Back to the start of the loop routine
                        goto CopySegments;
                    }

                CreateStringFast:
                    // Perform a return string creation by how much data being written into the buffer by decrementing
                    // bufferWrittenPtr with initial base pointer, bufferPtr.
                    string returnString = new string(bufferPtr, 0, (int)(bufferWrittenPtr - bufferPtr));
                    // Then return the string
                    return returnString;
                }
                finally
                {
                    // Return the write buffer to save memory from being unnecessarily allocated.
                    ArrayPool<char>.Shared.Return(buffer);
                }
            }
        }

        static int SumSegmentsLength(ReadOnlySpan<string?> segmentsInner)
        {
            // If the span is empty, then return 0 (as no segments to be merged)
            if (segmentsInner.IsEmpty)
                return 0;

            // Start incrementing sum in backward
            int sum = 0;
            int i = segmentsInner.Length;

        // Do the loop.
        LenSum:
            // ?? as means if the current index of span is null, nothing to increment (0).
            // Also, decrement the index as we are summing the length backwards.
            sum += segmentsInner[--i]?.Length ?? 0;
            if (i > 0)
                // Back to the loop if the index is not yet zero.
                goto LenSum;

            // If no routines left, return the total sum.
            return sum;
        }
    }
}
