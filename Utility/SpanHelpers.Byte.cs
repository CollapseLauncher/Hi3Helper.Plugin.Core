/*
 * Most of the codes are copied from .NET Runtime source code.
 * All the codes are belong to Microsoft under MIT License.
 * 
 * https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/SpanHelpers.cs
 */

using System.Diagnostics.CodeAnalysis;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
// ReSharper disable All

namespace Hi3Helper.Plugin.Core.Utility;

public static partial class SpanHelpers
{
    // IndexOfNullByte processes memory in aligned chunks, and thus it won't crash even if it accesses memory beyond the null terminator.
    // This behavior is an implementation detail of the runtime and callers outside System.Private.CoreLib must not depend on it.
    public static unsafe int IndexOfNullByte(byte* searchSpace)
    {
        const int Length = int.MaxValue;
        const uint uValue = 0; // Use uint for comparisons to avoid unnecessary 8->32 extensions
        nuint offset = 0; // Use nuint for arithmetic to avoid unnecessary 64->32->64 truncations
        nuint lengthToExamine = (nuint)(uint)Length;

        if (Vector128.IsHardwareAccelerated)
        {
            // Avx2 branch also operates on Sse2 sizes, so check is combined.
            lengthToExamine = UnalignedCountVector128(searchSpace);
        }

    SequentialScan:
        while (lengthToExamine >= 8)
        {
            lengthToExamine -= 8;

            if (uValue == searchSpace[offset])
                goto Found;
            if (uValue == searchSpace[offset + 1])
                goto Found1;
            if (uValue == searchSpace[offset + 2])
                goto Found2;
            if (uValue == searchSpace[offset + 3])
                goto Found3;
            if (uValue == searchSpace[offset + 4])
                goto Found4;
            if (uValue == searchSpace[offset + 5])
                goto Found5;
            if (uValue == searchSpace[offset + 6])
                goto Found6;
            if (uValue == searchSpace[offset + 7])
                goto Found7;

            offset += 8;
        }

        if (lengthToExamine >= 4)
        {
            lengthToExamine -= 4;

            if (uValue == searchSpace[offset])
                goto Found;
            if (uValue == searchSpace[offset + 1])
                goto Found1;
            if (uValue == searchSpace[offset + 2])
                goto Found2;
            if (uValue == searchSpace[offset + 3])
                goto Found3;

            offset += 4;
        }

        while (lengthToExamine > 0)
        {
            lengthToExamine -= 1;

            if (uValue == searchSpace[offset])
                goto Found;

            offset += 1;
        }

        // We get past SequentialScan only if IsHardwareAccelerated is true; and remain length is greater than Vector length.
        // However, we still have the redundant check to allow the JIT to see that the code is unreachable and eliminate it when the platform does not
        // have hardware accelerated. After processing Vector lengths we return to SequentialScan to finish any remaining.
        if (Vector512.IsHardwareAccelerated)
        {
            if (offset < (nuint)(uint)Length)
            {
                if ((((nuint)(uint)searchSpace + offset) & (nuint)(Vector256<byte>.Count - 1)) != 0)
                {
                    // Not currently aligned to Vector256 (is aligned to Vector128); this can cause a problem for searches
                    // with no upper bound e.g. String.strlen.
                    // Start with a check on Vector128 to align to Vector256, before moving to processing Vector256.
                    // This ensures we do not fault across memory pages while searching for an end of string.
                    Vector128<byte> search = Vector128.Load(searchSpace + offset);

                    // Same method as below
                    uint matches = Vector128.Equals(Vector128<byte>.Zero, search).ExtractMostSignificantBits();
                    if (matches == 0)
                    {
                        // Zero flags set so no matches
                        offset += (nuint)Vector128<byte>.Count;
                    }
                    else
                    {
                        // Find bitflag offset of first match and add to current offset
                        return (int)(offset + (uint)BitOperations.TrailingZeroCount(matches));
                    }
                }

                if ((((nuint)(uint)searchSpace + offset) & (nuint)(Vector512<byte>.Count - 1)) != 0)
                {
                    // Not currently aligned to Vector512 (is aligned to Vector256); this can cause a problem for searches
                    // with no upper bound e.g. String.strlen.
                    // Start with a check on Vector256 to align to Vector512, before moving to processing Vector256.
                    // This ensures we do not fault across memory pages while searching for an end of string.
                    Vector256<byte> search = Vector256.Load(searchSpace + offset);

                    // Same method as below
                    uint matches = Vector256.Equals(Vector256<byte>.Zero, search).ExtractMostSignificantBits();
                    if (matches == 0)
                    {
                        // Zero flags set so no matches
                        offset += (nuint)Vector256<byte>.Count;
                    }
                    else
                    {
                        // Find bitflag offset of first match and add to current offset
                        return (int)(offset + (uint)BitOperations.TrailingZeroCount(matches));
                    }
                }
                lengthToExamine = GetByteVector512SpanLength(offset, Length);
                if (lengthToExamine > offset)
                {
                    do
                    {
                        Vector512<byte> search = Vector512.Load(searchSpace + offset);
                        ulong matches = Vector512.Equals(Vector512<byte>.Zero, search).ExtractMostSignificantBits();
                        // Note that MoveMask has converted the equal vector elements into a set of bit flags,
                        // So the bit position in 'matches' corresponds to the element offset.
                        if (matches == 0)
                        {
                            // Zero flags set so no matches
                            offset += (nuint)Vector512<byte>.Count;
                            continue;
                        }

                        // Find bitflag offset of first match and add to current offset
                        return (int)(offset + (uint)BitOperations.TrailingZeroCount(matches));
                    } while (lengthToExamine > offset);
                }

                lengthToExamine = GetByteVector256SpanLength(offset, Length);
                if (lengthToExamine > offset)
                {
                    Vector256<byte> search = Vector256.Load(searchSpace + offset);

                    // Same method as above
                    uint matches = Vector256.Equals(Vector256<byte>.Zero, search).ExtractMostSignificantBits();
                    if (matches == 0)
                    {
                        // Zero flags set so no matches
                        offset += (nuint)Vector256<byte>.Count;
                    }
                    else
                    {
                        // Find bitflag offset of first match and add to current offset
                        return (int)(offset + (uint)BitOperations.TrailingZeroCount(matches));
                    }
                }

                lengthToExamine = GetByteVector128SpanLength(offset, Length);
                if (lengthToExamine > offset)
                {
                    Vector128<byte> search = Vector128.Load(searchSpace + offset);

                    // Same method as above
                    uint matches = Vector128.Equals(Vector128<byte>.Zero, search).ExtractMostSignificantBits();
                    if (matches == 0)
                    {
                        // Zero flags set so no matches
                        offset += (nuint)Vector128<byte>.Count;
                    }
                    else
                    {
                        // Find bitflag offset of first match and add to current offset
                        return (int)(offset + (uint)BitOperations.TrailingZeroCount(matches));
                    }
                }

                if (offset < (nuint)(uint)Length)
                {
                    lengthToExamine = ((nuint)(uint)Length - offset);
                    goto SequentialScan;
                }
            }
        }
        else if (Vector256.IsHardwareAccelerated)
        {
            if (offset < (nuint)(uint)Length)
            {
                if ((((nuint)(uint)searchSpace + offset) & (nuint)(Vector256<byte>.Count - 1)) != 0)
                {
                    // Not currently aligned to Vector256 (is aligned to Vector128); this can cause a problem for searches
                    // with no upper bound e.g. String.strlen.
                    // Start with a check on Vector128 to align to Vector256, before moving to processing Vector256.
                    // This ensures we do not fault across memory pages while searching for an end of string.
                    Vector128<byte> search = Vector128.Load(searchSpace + offset);

                    // Same method as below
                    uint matches = Vector128.Equals(Vector128<byte>.Zero, search).ExtractMostSignificantBits();
                    if (matches == 0)
                    {
                        // Zero flags set so no matches
                        offset += (nuint)Vector128<byte>.Count;
                    }
                    else
                    {
                        // Find bitflag offset of first match and add to current offset
                        return (int)(offset + (uint)BitOperations.TrailingZeroCount(matches));
                    }
                }

                lengthToExamine = GetByteVector256SpanLength(offset, Length);
                if (lengthToExamine > offset)
                {
                    do
                    {
                        Vector256<byte> search = Vector256.Load(searchSpace + offset);
                        uint matches = Vector256.Equals(Vector256<byte>.Zero, search).ExtractMostSignificantBits();
                        // Note that MoveMask has converted the equal vector elements into a set of bit flags,
                        // So the bit position in 'matches' corresponds to the element offset.
                        if (matches == 0)
                        {
                            // Zero flags set so no matches
                            offset += (nuint)Vector256<byte>.Count;
                            continue;
                        }

                        // Find bitflag offset of first match and add to current offset
                        return (int)(offset + (uint)BitOperations.TrailingZeroCount(matches));
                    } while (lengthToExamine > offset);
                }

                lengthToExamine = GetByteVector128SpanLength(offset, Length);
                if (lengthToExamine > offset)
                {
                    Vector128<byte> search = Vector128.Load(searchSpace + offset);

                    // Same method as above
                    uint matches = Vector128.Equals(Vector128<byte>.Zero, search).ExtractMostSignificantBits();
                    if (matches == 0)
                    {
                        // Zero flags set so no matches
                        offset += (nuint)Vector128<byte>.Count;
                    }
                    else
                    {
                        // Find bitflag offset of first match and add to current offset
                        return (int)(offset + (uint)BitOperations.TrailingZeroCount(matches));
                    }
                }

                if (offset < (nuint)(uint)Length)
                {
                    lengthToExamine = ((nuint)(uint)Length - offset);
                    goto SequentialScan;
                }
            }
        }
        else if (Vector128.IsHardwareAccelerated)
        {
            if (offset < (nuint)(uint)Length)
            {
                lengthToExamine = GetByteVector128SpanLength(offset, Length);

                while (lengthToExamine > offset)
                {
                    Vector128<byte> search = Vector128.Load(searchSpace + offset);

                    // Same method as above
                    Vector128<byte> compareResult = Vector128.Equals(Vector128<byte>.Zero, search);
                    if (compareResult == Vector128<byte>.Zero)
                    {
                        // Zero flags set so no matches
                        offset += (nuint)Vector128<byte>.Count;
                        continue;
                    }

                    // Find bitflag offset of first match and add to current offset
                    uint matches = compareResult.ExtractMostSignificantBits();
                    return (int)(offset + (uint)BitOperations.TrailingZeroCount(matches));
                }

                if (offset < (nuint)(uint)Length)
                {
                    lengthToExamine = ((nuint)(uint)Length - offset);
                    goto SequentialScan;
                }
            }
        }

        ThrowMustBeNullTerminatedString();
    Found: // Workaround for https://github.com/dotnet/runtime/issues/8795
        return (int)offset;
    Found1:
        return (int)(offset + 1);
    Found2:
        return (int)(offset + 2);
    Found3:
        return (int)(offset + 3);
    Found4:
        return (int)(offset + 4);
    Found5:
        return (int)(offset + 5);
    Found6:
        return (int)(offset + 6);
    Found7:
        return (int)(offset + 7);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static nuint GetByteVector128SpanLength(nuint offset, int length)
        => (nuint)(uint)((length - (int)offset) & ~(Vector128<byte>.Count - 1));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static nuint GetByteVector256SpanLength(nuint offset, int length)
        => (nuint)(uint)((length - (int)offset) & ~(Vector256<byte>.Count - 1));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static nuint GetByteVector512SpanLength(nuint offset, int length)
        => (nuint)(uint)((length - (int)offset) & ~(Vector512<byte>.Count - 1));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe nuint UnalignedCountVector128(byte* searchSpace)
    {
        nint unaligned = (nint)searchSpace & (Vector128<byte>.Count - 1);
        return (nuint)(uint)((Vector128<byte>.Count - unaligned) & (Vector128<byte>.Count - 1));
    }

    [DoesNotReturn]
    private static void ThrowMustBeNullTerminatedString()
    {
        throw new ArgumentException("The string must be null-terminated.");
    }
}
