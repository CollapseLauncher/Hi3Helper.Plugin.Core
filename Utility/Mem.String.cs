using System;
using System.Buffers;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;
// ReSharper disable UnusedMember.Global
// ReSharper disable GrammarMistakeInComment

namespace Hi3Helper.Plugin.Core.Utility;

public static partial class Mem
{
    /// <summary>
    /// Creates a modifiable <see cref="Span{T}"/> by scanning the end of the null character of either <see cref="char"/> (UTF-16) or <see cref="byte"/> (UTF-8/ANSI) string from <see cref="PluginDisposableMemory{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of character (either <see cref="char"/> (UTF-16) or <see cref="byte"/> (UTF-8/ANSI) string)</typeparam>
    /// <param name="memory">A plugin memory to be converted from.</param>
    /// <returns>A modifiable span of the string.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Span<T> CreateSpanFromNullTerminated<T>(this PluginDisposableMemory<T> memory)
        where T : unmanaged
        => CreateSpanFromNullTerminated<T>(memory.AsPointer());

    /// <summary>
    /// Creates a modifiable <see cref="Span{T}"/> by scanning the end of the null character of either <see cref="char"/> (UTF-16) or <see cref="byte"/> (UTF-8/ANSI) string from a pointer of unmanaged native memory.
    /// </summary>
    /// <typeparam name="T">The type of character (either <see cref="char"/> (UTF-16) or <see cref="byte"/> (UTF-8/ANSI) string)</typeparam>
    /// <param name="ptr">A pointer of the unmanaged native memory.</param>
    /// <returns>A modifiable span of the string.</returns>
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

    /// <summary>
    /// Creates a managed <see cref="string"/> from <see cref="PluginDisposableMemory{T}"/>
    /// </summary>
    /// <typeparam name="T">The type of character (either <see cref="char"/> (UTF-16) or <see cref="byte"/> (UTF-8/ANSI) string)</typeparam>
    /// <param name="memory">A struct of <see cref="PluginDisposableMemory{T}"/>.</param>
    /// <returns>A managed .NET <see cref="string"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe string? CreateStringFromNullTerminated<T>(this ref PluginDisposableMemory<T> memory)
        where T : unmanaged
        => memory.IsEmpty ? null : CreateStringFromNullTerminated(memory.AsPointer());

    /// <summary>
    /// Creates a managed <see cref="string"/> from the pointer of unmanaged native memory of <typeparamref name="T"/> type.
    /// </summary>
    /// <typeparam name="T">The type of character (either <see cref="char"/> (UTF-16) or <see cref="byte"/> (UTF-8/ANSI) string)</typeparam>
    /// <param name="source">An unmanaged native memory pointer of <typeparamref name="T"/> type.</param>
    /// <returns>A managed .NET <see cref="string"/>.</returns>
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

    /// <summary>
    /// Creates an unmanaged UTF-8 (null-terminated) string from .NET Managed <see cref="ReadOnlySpan{T}"/> of <see cref="char"/> (or <see cref="string"/>)
    /// </summary>
    /// <param name="span">A Span of char to create the string from.</param>
    /// <returns>An unmanaged UTF-8 (null-terminated) string stored into native memory.</returns>
    public static unsafe byte* Utf16SpanToUtf8Unmanaged(this ReadOnlySpan<char> span)
    {
        if (span.IsEmpty)
            return null;

        int        exactByteCount = checked(Encoding.UTF8.GetByteCount(span) + 1); // + 1 for null terminator
        byte*      mem            = (byte*)Marshal.AllocCoTaskMem(exactByteCount);
        Span<byte> buffer         = new(mem, exactByteCount);

        int byteCount = Encoding.UTF8.GetBytes(span, buffer);
        buffer[byteCount] = 0; // null-terminate
        return mem;
    }

    /// <summary>
    /// Throws if either the size of <typeparamref name="T"/> is not 1 (<see cref="byte"/>) or 2 (<see cref="char"/>)
    /// </summary>
    /// <typeparam name="T">The type of character type to check.</typeparam>
    /// <param name="isChar">Outputs whether the character is <see cref="char"/> type.</param>
    /// <exception cref="InvalidOperationException">If the size of <typeparamref name="T"/> is not 1 (<see cref="byte"/>) or 2 (<see cref="char"/>)</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe void ThrowIfNotByteOrChar<T>(out bool isChar)
        where T : unmanaged
    {
        isChar = false;
        int sizeofT = sizeof(T);

        if (sizeofT == 2)
        {
            isChar = true;
            return;
        }

        if (sizeofT != 1) throw new InvalidOperationException("Type must be a char or byte!");
    }

    /// <summary>
    /// Combines string of URL and path into one combined <see cref="string"/>.
    /// </summary>
    /// <param name="baseUrl">The base of the URL string.</param>
    /// <param name="segments">Segments of path to combine with.</param>
    /// <returns>Combined URL string.</returns>
    public static string CombineUrlFromString(this string? baseUrl, params ReadOnlySpan<string?> segments)
        => CombineUrlFromString(baseUrl.AsSpan(), segments);

    /// <summary>
    /// Combines string of URL and path into one combined <see cref="string"/>.
    /// </summary>
    /// <param name="baseUrl">The base of the URL string.</param>
    /// <param name="segments">Segments of path to combine with.</param>
    /// <returns>Combined URL string.</returns>
    public static unsafe string CombineUrlFromString(ReadOnlySpan<char> baseUrl, params ReadOnlySpan<string?> segments)
    {
        // Assign the size of a char as constant
        const uint sizeOfChar = sizeof(char);

        // Get the base URL length and decrement by 1 if the end of the index (^1)
        // is a '/' character. Otherwise, nothing to decrement.
        // 
        // Once we get a length of the base URL, get a sum of all lengths
        // of the segment's span.
        int  baseUrlLen  = baseUrl.Length - (baseUrl[^1] == '/' ? 1 : 0);
        int  bufferLen   = baseUrlLen + SumSegmentsLength(segments);
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
            int i   = segmentsInner.Length;

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

    // ReSharper disable once IdentifierTypo
    // ReSharper disable once CommentTypo
    /// <summary>
    /// Get pinnable pointer of the string.
    /// </summary>
    /// <param name="str">A string to get its pointer from.</param>
    /// <returns>A pinned pointer of the string.</returns>
    public static unsafe char* GetPinnableStringPointer(this string? str)
    {
        fixed (char* ptr = &Utf16StringMarshaller.GetPinnableReference(str))
        {
            return ptr;
        }
    }

    // ReSharper disable once IdentifierTypo
    // ReSharper disable once CommentTypo
    public static unsafe nint GetPinnableStringPointerSafe(this string? str)
    {
        char* p = str.GetPinnableStringPointer();
        return (nint)p;
    }
}
