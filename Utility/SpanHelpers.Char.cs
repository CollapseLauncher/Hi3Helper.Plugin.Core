/*
 * Most of the codes are copied from .NET Runtime source code.
 * All the codes are belong to Microsoft under MIT License.
 * 
 * https://github.com/dotnet/runtime/blob/main/src/libraries/System.Private.CoreLib/src/System/SpanHelpers.cs
 */

using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.Intrinsics;
// ReSharper disable All

namespace Hi3Helper.Plugin.Core.Utility;

public static partial class SpanHelpers
{
    // IndexOfNullCharacter processes memory in aligned chunks, and thus it won't crash even if it accesses memory beyond the null terminator.
    // This behavior is an implementation detail of the runtime and callers outside System.Private.CoreLib must not depend on it.
    public static unsafe int IndexOfNullCharacter(char* searchSpace)
    {
        const char value = '\0';
        const int length = int.MaxValue;

        nint offset = 0;
        nint lengthToExamine = length;

        if (((int)searchSpace & 1) != 0)
        {
            // Input isn't char aligned, we won't be able to align it to a Vector
        }
        else if (Vector128.IsHardwareAccelerated)
        {
            // Avx2 branch also operates on Sse2 sizes, so check is combined.
            // Needs to be double length to allow us to align the data first.
            lengthToExamine = UnalignedCountVector128(searchSpace);
        }

    SequentialScan:
        // In the non-vector case lengthToExamine is the total length.
        // In the vector case lengthToExamine first aligns to Vector,
        // then in a second pass after the Vector lengths is the
        // remaining data that is shorter than a Vector length.
        while (lengthToExamine >= 4)
        {
            if (value == searchSpace[offset])
                goto Found;
            if (value == searchSpace[offset + 1])
                goto Found1;
            if (value == searchSpace[offset + 2])
                goto Found2;
            if (value == searchSpace[offset + 3])
                goto Found3;

            offset += 4;
            lengthToExamine -= 4;
        }

        while (lengthToExamine > 0)
        {
            if (value == searchSpace[offset])
                goto Found;

            offset++;
            lengthToExamine--;
        }

        // We get past SequentialScan only if IsHardwareAccelerated is true. However, we still have the redundant check to allow
        // the JIT to see that the code is unreachable and eliminate it when the platform does not have hardware accelerated.
        if (Vector512.IsHardwareAccelerated)
        {
            if (offset < length)
            {
                Debug.Assert(length - offset >= Vector128<ushort>.Count);
                if (((nint)(searchSpace + (nint)offset) & (nint)(Vector256<byte>.Count - 1)) != 0)
                {
                    // Not currently aligned to Vector256 (is aligned to Vector128); this can cause a problem for searches
                    // with no upper bound e.g. String.wcslen. Start with a check on Vector128 to align to Vector256,
                    // before moving to processing Vector256.

                    // This ensures we do not fault across memory pages
                    // while searching for an end of string. Specifically that this assumes that the length is either correct
                    // or that the data is pinned otherwise it may cause an AccessViolation from crossing a page boundary into an
                    // unowned page. If the search is unbounded (e.g. null terminator in wcslen) and the search value is not found,
                    // again this will likely cause an AccessViolation. However, correctly bounded searches will return -1 rather
                    // than ever causing an AV.
                    Vector128<ushort> search = *(Vector128<ushort>*)(searchSpace + (nuint)offset);

                    // Same method as below
                    uint matches = Vector128.Equals(Vector128<ushort>.Zero, search).AsByte().ExtractMostSignificantBits();
                    if (matches == 0)
                    {
                        // Zero flags set so no matches
                        offset += Vector128<ushort>.Count;
                    }
                    else
                    {
                        // Find bitflag offset of first match and add to current offset
                        return (int)(offset + ((uint)BitOperations.TrailingZeroCount(matches) / sizeof(char)));
                    }
                }
                if (((nint)(searchSpace + (nint)offset) & (nint)(Vector512<byte>.Count - 1)) != 0)
                {
                    // Not currently aligned to Vector512 (is aligned to Vector256); this can cause a problem for searches
                    // with no upper bound e.g. String.wcslen. Start with a check on Vector256 to align to Vector512,
                    // before moving to processing Vector256.

                    // This ensures we do not fault across memory pages
                    // while searching for an end of string. Specifically that this assumes that the length is either correct
                    // or that the data is pinned otherwise it may cause an AccessViolation from crossing a page boundary into an
                    // unowned page. If the search is unbounded (e.g. null terminator in wcslen) and the search value is not found,
                    // again this will likely cause an AccessViolation. However, correctly bounded searches will return -1 rather
                    // than ever causing an AV.
                    Vector256<ushort> search = *(Vector256<ushort>*)(searchSpace + (nuint)offset);

                    // Same method as below
                    uint matches = Vector256.Equals(Vector256<ushort>.Zero, search).AsByte().ExtractMostSignificantBits();
                    if (matches == 0)
                    {
                        // Zero flags set so no matches
                        offset += Vector256<ushort>.Count;
                    }
                    else
                    {
                        // Find bitflag offset of first match and add to current offset
                        return (int)(offset + ((uint)BitOperations.TrailingZeroCount(matches) / sizeof(char)));
                    }
                }

                lengthToExamine = GetCharVector512SpanLength(offset, length);
                if (lengthToExamine > 0)
                {
                    do
                    {
                        Debug.Assert(lengthToExamine >= Vector512<ushort>.Count);

                        Vector512<ushort> search = *(Vector512<ushort>*)(searchSpace + (nuint)offset);

                        // AVX-512 returns comparison results in a mask register, so we want to optimize
                        // the core check to simply be an "none match" check. This will slightly increase
                        // the cost for the early match case, but greatly improves perf otherwise.

                        if (!Vector512.EqualsAny(search, Vector512<ushort>.Zero))
                        {
                            // Zero flags set so no matches
                            offset += Vector512<ushort>.Count;
                            lengthToExamine -= Vector512<ushort>.Count;
                            continue;
                        }

                        // Note that ExtractMostSignificantBits has converted the equal vector elements into a set of bit flags,
                        // So the bit position in 'matches' corresponds to the element offset.
                        //
                        // Find bitflag offset of first match and add to current offset,
                        // flags are in bytes so divide for chars
                        ulong matches = Vector512.Equals(search, Vector512<ushort>.Zero).ExtractMostSignificantBits();
                        return (int)(offset + (uint)BitOperations.TrailingZeroCount(matches));
                    } while (lengthToExamine > 0);
                }

                lengthToExamine = GetCharVector256SpanLength(offset, length);
                if (lengthToExamine > 0)
                {
                    Debug.Assert(lengthToExamine >= Vector256<ushort>.Count);

                    Vector256<ushort> search = *(Vector256<ushort>*)(searchSpace + (nuint)offset);

                    // Same method as above
                    uint matches = Vector256.Equals(Vector256<ushort>.Zero, search).AsByte().ExtractMostSignificantBits();
                    if (matches == 0)
                    {
                        // Zero flags set so no matches
                        offset += Vector256<ushort>.Count;
                        // Don't need to change lengthToExamine here as we don't use its current value again.
                    }
                    else
                    {
                        // Find bitflag offset of first match and add to current offset,
                        // flags are in bytes so divide for chars
                        return (int)(offset + ((uint)BitOperations.TrailingZeroCount(matches) / sizeof(char)));
                    }
                }

                lengthToExamine = GetCharVector128SpanLength(offset, length);
                if (lengthToExamine > 0)
                {
                    Debug.Assert(lengthToExamine >= Vector128<ushort>.Count);

                    Vector128<ushort> search = *(Vector128<ushort>*)(searchSpace + (nuint)offset);

                    // Same method as above
                    uint matches = Vector128.Equals(Vector128<ushort>.Zero, search).AsByte().ExtractMostSignificantBits();
                    if (matches == 0)
                    {
                        // Zero flags set so no matches
                        offset += Vector128<ushort>.Count;
                        // Don't need to change lengthToExamine here as we don't use its current value again.
                    }
                    else
                    {
                        // Find bitflag offset of first match and add to current offset,
                        // flags are in bytes so divide for chars
                        return (int)(offset + ((uint)BitOperations.TrailingZeroCount(matches) / sizeof(char)));
                    }
                }

                if (offset < length)
                {
                    lengthToExamine = length - offset;
                    goto SequentialScan;
                }
            }
        }
        else if (Vector256.IsHardwareAccelerated)
        {
            if (offset < length)
            {
                Debug.Assert(length - offset >= Vector128<ushort>.Count);
                if (((nint)(searchSpace + (nint)offset) & (nint)(Vector256<byte>.Count - 1)) != 0)
                {
                    // Not currently aligned to Vector256 (is aligned to Vector128); this can cause a problem for searches
                    // with no upper bound e.g. String.wcslen. Start with a check on Vector128 to align to Vector256,
                    // before moving to processing Vector256.

                    // This ensures we do not fault across memory pages
                    // while searching for an end of string. Specifically that this assumes that the length is either correct
                    // or that the data is pinned otherwise it may cause an AccessViolation from crossing a page boundary into an
                    // unowned page. If the search is unbounded (e.g. null terminator in wcslen) and the search value is not found,
                    // again this will likely cause an AccessViolation. However, correctly bounded searches will return -1 rather
                    // than ever causing an AV.
                    Vector128<ushort> search = *(Vector128<ushort>*)(searchSpace + (nuint)offset);

                    // Same method as below
                    uint matches = Vector128.Equals(Vector128<ushort>.Zero, search).AsByte().ExtractMostSignificantBits();
                    if (matches == 0)
                    {
                        // Zero flags set so no matches
                        offset += Vector128<ushort>.Count;
                    }
                    else
                    {
                        // Find bitflag offset of first match and add to current offset
                        return (int)(offset + ((uint)BitOperations.TrailingZeroCount(matches) / sizeof(char)));
                    }
                }

                lengthToExamine = GetCharVector256SpanLength(offset, length);
                if (lengthToExamine > 0)
                {
                    do
                    {
                        Debug.Assert(lengthToExamine >= Vector256<ushort>.Count);

                        Vector256<ushort> search = *(Vector256<ushort>*)(searchSpace + (nuint)offset);
                        uint matches = Vector256.Equals(Vector256<ushort>.Zero, search).AsByte().ExtractMostSignificantBits();
                        // Note that MoveMask has converted the equal vector elements into a set of bit flags,
                        // So the bit position in 'matches' corresponds to the element offset.
                        if (matches == 0)
                        {
                            // Zero flags set so no matches
                            offset += Vector256<ushort>.Count;
                            lengthToExamine -= Vector256<ushort>.Count;
                            continue;
                        }

                        // Find bitflag offset of first match and add to current offset,
                        // flags are in bytes so divide for chars
                        return (int)(offset + ((uint)BitOperations.TrailingZeroCount(matches) / sizeof(char)));
                    } while (lengthToExamine > 0);
                }

                lengthToExamine = GetCharVector128SpanLength(offset, length);
                if (lengthToExamine > 0)
                {
                    Debug.Assert(lengthToExamine >= Vector128<ushort>.Count);

                    Vector128<ushort> search = *(Vector128<ushort>*)(searchSpace + (nuint)offset);

                    // Same method as above
                    uint matches = Vector128.Equals(Vector128<ushort>.Zero, search).AsByte().ExtractMostSignificantBits();
                    if (matches == 0)
                    {
                        // Zero flags set so no matches
                        offset += Vector128<ushort>.Count;
                        // Don't need to change lengthToExamine here as we don't use its current value again.
                    }
                    else
                    {
                        // Find bitflag offset of first match and add to current offset,
                        // flags are in bytes so divide for chars
                        return (int)(offset + ((uint)BitOperations.TrailingZeroCount(matches) / sizeof(char)));
                    }
                }

                if (offset < length)
                {
                    lengthToExamine = length - offset;
                    goto SequentialScan;
                }
            }
        }
        else if (Vector128.IsHardwareAccelerated)
        {
            if (offset < length)
            {
                Debug.Assert(length - offset >= Vector128<ushort>.Count);

                lengthToExamine = GetCharVector128SpanLength(offset, length);
                if (lengthToExamine > 0)
                {
                    do
                    {
                        Debug.Assert(lengthToExamine >= Vector128<ushort>.Count);

                        Vector128<ushort> search = *(Vector128<ushort>*)(searchSpace + (nuint)offset);

                        // Same method as above
                        Vector128<ushort> compareResult = Vector128.Equals(Vector128<ushort>.Zero, search);
                        if (compareResult == Vector128<ushort>.Zero)
                        {
                            // Zero flags set so no matches
                            offset += Vector128<ushort>.Count;
                            lengthToExamine -= Vector128<ushort>.Count;
                            continue;
                        }

                        // Find bitflag offset of first match and add to current offset,
                        // flags are in bytes so divide for chars
                        uint matches = compareResult.AsByte().ExtractMostSignificantBits();
                        return (int)(offset + ((uint)BitOperations.TrailingZeroCount(matches) / sizeof(char)));
                    } while (lengthToExamine > 0);
                }

                if (offset < length)
                {
                    lengthToExamine = length - offset;
                    goto SequentialScan;
                }
            }
        }

        ThrowMustBeNullTerminatedString();
    Found3:
        return (int)(offset + 3);
    Found2:
        return (int)(offset + 2);
    Found1:
        return (int)(offset + 1);
    Found:
        return (int)(offset);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static nint GetCharVector128SpanLength(nint offset, nint length)
        => (length - offset) & ~(Vector128<ushort>.Count - 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static nint GetCharVector256SpanLength(nint offset, nint length)
        => (length - offset) & ~(Vector256<ushort>.Count - 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static nint GetCharVector512SpanLength(nint offset, nint length)
        => (length - offset) & ~(Vector512<ushort>.Count - 1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static unsafe nint UnalignedCountVector128(char* searchSpace)
    {
        const int ElementsPerByte = sizeof(ushort) / sizeof(byte);
        return (nint)(uint)(-(int)searchSpace / ElementsPerByte) & (Vector128<ushort>.Count - 1);
    }
}
