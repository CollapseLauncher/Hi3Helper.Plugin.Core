using Hi3Helper.Plugin.Core.Utility;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
// ReSharper disable UnusedMember.Global

namespace Hi3Helper.Plugin.Core.Management;

/// <summary>
/// Represents a version of API, Game Installation or Assets.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 8)]
public struct GameVersion :
    IVersion,
    IEquatable<GameVersion>,
    IUtf8SpanFormattable,
    IUtf8SpanParsable<GameVersion>,
    ISpanFormattable,
    ISpanParsable<GameVersion>
{
    private const string ExSeparators = ",.;|";
    private static ReadOnlySpan<byte> ExSeparatorsUtf8 => ",.;|"u8;
    public static readonly GameVersion Empty = new(0, 0, 0, 0);

    public GameVersion(params ReadOnlySpan<int> ver)
    {
        if (ver.Length == 0)
        {
            throw new ArgumentException("Version array entered should have length at least 1 or max. 4!");
        }

        Major = ver[0];
        Minor = ver.Length >= 2 ? ver[1] : 0;
        Build = ver.Length >= 3 ? ver[2] : 0;
        Revision = ver.Length >= 4 ? ver[3] : 0;
    }

    public GameVersion(Version version)
    {
        Major = version.Major;
        Minor = version.Minor;
        Build = version.Build;
        Revision = version.Revision;
    }

    public GameVersion(string? version)
    {
        if (!TryParse(version, null, out GameVersion versionOut))
        {
            throw new ArgumentException($"Version in the config.ini should be either in \"x\", \"x.x\", \"x.x.x\" or \"x.x.x.x\" format or all the values aren't numbers! (current value: \"{version}\")");
        }

        Major = versionOut.Major;
        Minor = versionOut.Minor;
        Build = versionOut.Build;
        Revision = versionOut.Revision;
    }

    public readonly GameVersion GetIncrementedVersion()
    {
        int nextMajor = Major;
        int nextMinor = Minor;

        nextMinor++;
        if (nextMinor < 10)
        {
            return new GameVersion(nextMajor, nextMinor, Build, Revision);
        }

        nextMinor = 0;
        nextMajor++;

        return new GameVersion(nextMajor, nextMinor, Build, Revision);
    }

    /// <summary>
    /// Create a <see cref="Span{T}"/> of <see cref="int"/> representation of this struct.
    /// </summary>
    /// <returns>A <see cref="Span{T}"/> of <see cref="int"/> with 4-int by length.</returns>
    public unsafe ReadOnlySpan<int> AsSpan() => new(this.AsPointer(), 4);

    /// <summary>
    /// Convert this instance into <see cref="Version"/>.
    /// </summary>
    /// <returns>A <see cref="Version"/> instance.</returns>
    public readonly Version ToVersion() => new(Major, Minor, Build, Revision);

    /// <summary>
    /// Create a string representation of <see cref="GameVersion"/> into full "Major.Minor.Build.Revision" format.
    /// </summary>
    public readonly override string ToString() => ToString(string.Empty);

    /// <summary>
    /// Create a string representation of <see cref="GameVersion"/>.
    /// </summary>
    /// <param name="format">
    /// An optional format specifier. If it's 'N' or 'n', the "Major.Minor.Build" format is written. Otherwise or by default, only "Major.Minor.Build.Revision" format is written.
    /// </param>
    /// <param name="formatProvider">A format provider instance to write the result.</param>
    /// <returns>A string representation of <see cref="GameVersion"/>.</returns>
    public readonly string ToString(string? format, IFormatProvider? formatProvider = null)
    {
        Span<char> writeStackalloc = stackalloc char[64];
        if (!TryFormat(writeStackalloc, out int written, format, formatProvider))
        {
            throw new InvalidOperationException("Cannot write string to stackalloc buffer!");
        }

        return new string(writeStackalloc[..written]);
    }

    public static bool operator <(GameVersion? left, GameVersion? right) =>
        left.HasValue && right.HasValue &&
        (left.Value.Major < right.Value.Major ||
        (left.Value.Major == right.Value.Major && left.Value.Minor < right.Value.Minor) ||
        (left.Value.Major == right.Value.Major && left.Value.Minor == right.Value.Minor && left.Value.Build < right.Value.Build) ||
        (left.Value.Major == right.Value.Major && left.Value.Minor == right.Value.Minor && left.Value.Build == right.Value.Build && left.Value.Revision < right.Value.Revision));

    public static bool operator >(GameVersion? left, GameVersion? right) =>
        right < left;

    public static bool operator ==(GameVersion? left, GameVersion? right) =>
        left.HasValue && right.HasValue &&
        left.Value.Major == right.Value.Major &&
        left.Value.Minor == right.Value.Minor &&
        left.Value.Build == right.Value.Build &&
        left.Value.Revision == right.Value.Revision;

    public static bool operator !=(GameVersion? left, GameVersion? right) =>
        !(left == right);

    public static bool operator ==(GameVersion? left, string? right)
    {
        if (!left.HasValue || !TryParse(right, null, out GameVersion rightAsParsed))
        {
            return false;
        }

        return left == rightAsParsed;
    }

    public static bool operator !=(GameVersion? left, string? right) =>
        !(left == right);

    public static bool operator ==(string? left, GameVersion? right) =>
        right == left;

    public static bool operator !=(string? left, GameVersion? right) =>
        !(right == left);

    public readonly bool Equals(IVersion? other) =>
        EqualsInner(this, other);

    public readonly bool Equals(GameVersion other) => this == other;

    public readonly override bool Equals([NotNullWhen(true)] object? obj) =>
        EqualsInner(this, obj);

    public readonly override int GetHashCode() =>
        HashCode.Combine(Major, Minor, Build, Revision);

    private static bool EqualsInner(object? fromVersion, object? toVersion)
    {
        // ReSharper disable once ConvertIfStatementToSwitchStatement
        if (fromVersion is IVersion fromVersionAsI &&
            toVersion is IVersion toVersionAsI)
        {
            return EqualsInner(fromVersionAsI, toVersionAsI);
        }

        if (fromVersion is IVersion fromVersionAsIComp &&
            toVersion is string toVersionStr &&
            TryParse(toVersionStr, null, out GameVersion toVersionParsed))
        {
            return EqualsInner(fromVersionAsIComp, toVersionParsed);
        }

        return false;
    }

    private static bool EqualsInner(IVersion? thisVersion, IVersion? versionToCompare)
    {
        if (versionToCompare == null ||
            thisVersion == null)
        {
            return false;
        }

        ReadOnlySpan<int> thisSpan = thisVersion.AsSpan();
        ReadOnlySpan<int> otherSpan = versionToCompare.AsSpan();

        return otherSpan.Length == 4 &&
               thisSpan[0] == otherSpan[0] &&
               thisSpan[1] == otherSpan[1] &&
               thisSpan[2] == otherSpan[2] &&
               thisSpan[3] == otherSpan[3];
    }

    /// <summary>
    /// Formats the current <see cref="GameVersion"/> instance into a character span.
    /// </summary>
    /// <param name="destination">
    /// The span of characters to which the formatted version string will be written.
    /// </param>
    /// <param name="charsWritten">
    /// When this method returns, contains the number of characters that were written to <paramref name="destination"/>.
    /// </param>
    /// <param name="format">
    /// An optional format specifier. If it's 'N' or 'n', the "Major.Minor.Build" format is written. Otherwise or by default, only "Major.Minor.Build.Revision" format is written.
    /// </param>
    /// <param name="provider">
    /// An optional format provider. This parameter is ignored in this implementation.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if formatting was successful and the destination span was large enough. Otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// The formatted version string will be in the form "Major.Minor.Build" or "Major.Minor.Build.Revision" depending on the format specifier.
    /// </remarks>
    public readonly bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        charsWritten = 0;

        bool isUseMiniFormat = !format.IsEmpty && (format[0] | 0x20) == 'n'; // This compares both 'n' or 'N' as true.
        if (destination.Length < 4)
        {
            return false;
        }

        if (!TryWriteAppend(Major, destination, out int offsetWritten))
        {
            return false;
        }
        charsWritten += offsetWritten;

        if (!TryWriteAppend(Minor, destination[charsWritten..], out offsetWritten))
        {
            return false;
        }
        charsWritten += offsetWritten;

        if (!TryWriteAppend(Build, destination[charsWritten..], out offsetWritten, isUseMiniFormat))
        {
            return false;
        }
        charsWritten += offsetWritten;

        if (isUseMiniFormat)
        {
            return true;
        }

        if (!TryWriteAppend(Revision, destination[charsWritten..], out offsetWritten, true))
        {
            return false;
        }

        charsWritten += offsetWritten;
        return true;

        static bool TryWriteAppend(int cur, Span<char> dest, out int outWritten, bool isFinal = false)
        {
            if (!cur.TryFormat(dest, out outWritten))
            {
                return false;
            }

            switch (isFinal)
            {
                case true:
                    return true;
                case false when dest.Length <= outWritten:
                    return false;
                default:
                    dest[outWritten++] = '.';
                    return true;
            }
        }
    }

    /// <summary>
    /// Formats the current <see cref="GameVersion"/> instance into a character span.
    /// </summary>
    /// <param name="utf8Destination">
    /// The span of UTF-8 characters to which the formatted version string will be written.
    /// </param>
    /// <param name="bytesWritten">
    /// When this method returns, contains the number of characters that were written to <paramref name="utf8Destination"/>.
    /// </param>
    /// <param name="format">
    /// An optional format specifier. If it's 'N' or 'n', the "Major.Minor.Build" format is written. Otherwise or by default, only "Major.Minor.Build.Revision" format is written.
    /// </param>
    /// <param name="provider">
    /// An optional format provider. This parameter is ignored in this implementation.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if formatting was successful and the destination span was large enough. Otherwise, <see langword="false"/>.
    /// </returns>
    /// <remarks>
    /// The formatted version string will be in the form "Major.Minor.Build" or "Major.Minor.Build.Revision" depending on the format specifier.
    /// </remarks>
    public readonly bool TryFormat(Span<byte> utf8Destination, out int bytesWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        bytesWritten = 0;

        bool isUseMiniFormat = !format.IsEmpty && (format[0] | 0x20) == 'n'; // This compares both 'n' or 'N' as true.
        if (utf8Destination.Length < 4)
        {
            return false;
        }

        if (!TryWriteAppend(Major, utf8Destination, out int offsetWritten))
        {
            return false;
        }
        bytesWritten += offsetWritten;

        if (!TryWriteAppend(Minor, utf8Destination[bytesWritten..], out offsetWritten))
        {
            return false;
        }
        bytesWritten += offsetWritten;

        if (!TryWriteAppend(Build, utf8Destination[bytesWritten..], out offsetWritten, isUseMiniFormat))
        {
            return false;
        }
        bytesWritten += offsetWritten;

        if (isUseMiniFormat)
        {
            return true;
        }

        if (!TryWriteAppend(Revision, utf8Destination[bytesWritten..], out offsetWritten, true))
        {
            return false;
        }

        bytesWritten += offsetWritten;
        return true;

        static bool TryWriteAppend(int cur, Span<byte> dest, out int outWritten, bool isFinal = false)
        {
            if (!cur.TryFormat(dest, out outWritten))
            {
                return false;
            }

            switch (isFinal)
            {
                case true:
                    return true;
                case false when dest.Length <= outWritten:
                    return false;
                default:
                    dest[outWritten++] = (byte)'.';
                    return true;
            }
        }
    }

    /// <summary>
    /// Parses a UTF-8 encoded byte span into a <see cref="GameVersion"/> instance.
    /// </summary>
    /// <param name="utf8Text">The UTF-8 encoded byte span representing the version string.</param>
    /// <param name="provider">An optional format provider. This parameter is ignored in this implementation.</param>
    /// <returns>A <see cref="GameVersion"/> instance parsed from the input.</returns>
    /// <exception cref="ArgumentException">Thrown if the input is not a valid GameVersion string.</exception>
    public static GameVersion Parse(ReadOnlySpan<byte> utf8Text, IFormatProvider? provider)
    {
        if (!TryParse(utf8Text, provider, out GameVersion result))
        {
            throw new ArgumentException("Input UTF-8 string is not a valid GameVersion!");
        }

        return result;
    }

    /// <summary>
    /// Attempts to parse a UTF-8 encoded byte span into a <see cref="GameVersion"/> instance.
    /// </summary>
    /// <param name="utf8Text">The UTF-8 encoded byte span representing the version string.</param>
    /// <param name="result">When this method returns, contains the parsed <see cref="GameVersion"/> if successful; otherwise, the default value.</param>
    /// <returns><see langword="true"/> if parsing was successful; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(ReadOnlySpan<byte> utf8Text, out GameVersion result)
        => TryParse(utf8Text, null, out result);

    /// <summary>
    /// Attempts to parse a UTF-8 encoded byte span into a <see cref="GameVersion"/> instance.
    /// </summary>
    /// <param name="utf8Text">The UTF-8 encoded byte span representing the version string.</param>
    /// <param name="provider">An optional format provider. This parameter is ignored in this implementation.</param>
    /// <param name="result">When this method returns, contains the parsed <see cref="GameVersion"/> if successful; otherwise, the default value.</param>
    /// <returns><see langword="true"/> if parsing was successful; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(ReadOnlySpan<byte> utf8Text, IFormatProvider? provider, out GameVersion result)
    {
        if (utf8Text.IsEmpty)
        {
            result = default;
            return false;
        }

        Span<int> versionInt = stackalloc int[4];
        int i = 0;
        foreach (var currentRange in utf8Text.SplitAny(ExSeparatorsUtf8))
        {
            if (!int.TryParse(utf8Text[currentRange], out int currentInt))
            {
                result = default;
                return false;
            }

            versionInt[i++] = currentInt;
        }

        result = new GameVersion(versionInt);
        return true;
    }

    /// <summary>
    /// Parses a string into a <see cref="GameVersion"/> instance.
    /// </summary>
    /// <param name="s">The string representing the version.</param>
    /// <param name="provider">An optional format provider. This parameter is ignored in this implementation.</param>
    /// <returns>A <see cref="GameVersion"/> instance parsed from the input string.</returns>
    /// <exception cref="ArgumentException">Thrown if the input is not a valid GameVersion string.</exception>
    public static GameVersion Parse(string s, IFormatProvider? provider)
        => Parse(s.AsSpan(), provider);

    /// <summary>
    /// Attempts to parse a string into a <see cref="GameVersion"/> instance.
    /// </summary>
    /// <param name="s">The string representing the version.</param>
    /// <param name="provider">An optional format provider. This parameter is ignored in this implementation.</param>
    /// <param name="result">When this method returns, contains the parsed <see cref="GameVersion"/> if successful; otherwise, the default value.</param>
    /// <returns><see langword="true"/> if parsing was successful; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, out GameVersion result)
        => TryParse(s.AsSpan(), provider, out result);

    /// <summary>
    /// Parses a character span into a <see cref="GameVersion"/> instance.
    /// </summary>
    /// <param name="s">The character span representing the version.</param>
    /// <param name="provider">An optional format provider. This parameter is ignored in this implementation.</param>
    /// <returns>A <see cref="GameVersion"/> instance parsed from the input span.</returns>
    /// <exception cref="ArgumentException">Thrown if the input is not a valid GameVersion string.</exception>
    public static GameVersion Parse(ReadOnlySpan<char> s, IFormatProvider? provider)
    {
        if (!TryParse(s, provider, out GameVersion result))
        {
            throw new ArgumentException("Input string is not a valid GameVersion!");
        }

        return result;
    }

    /// <summary>
    /// Attempts to parse a character span into a <see cref="GameVersion"/> instance.
    /// </summary>
    /// <param name="versionSpan">The character span representing the version.</param>
    /// <param name="result">When this method returns, contains the parsed <see cref="GameVersion"/> if successful; otherwise, the default value.</param>
    /// <returns><see langword="true"/> if parsing was successful; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(ReadOnlySpan<char> versionSpan, out GameVersion result)
        => TryParse(versionSpan, null, out result);

    /// <summary>
    /// Attempts to parse a character span into a <see cref="GameVersion"/> instance.
    /// </summary>
    /// <param name="versionSpan">The character span representing the version.</param>
    /// <param name="provider">An optional format provider. This parameter is ignored in this implementation.</param>
    /// <param name="result">When this method returns, contains the parsed <see cref="GameVersion"/> if successful; otherwise, the default value.</param>
    /// <returns><see langword="true"/> if parsing was successful; otherwise, <see langword="false"/>.</returns>
    public static bool TryParse(ReadOnlySpan<char> versionSpan, IFormatProvider? provider, out GameVersion result)
    {
        result = default;
        if (versionSpan.IsEmpty)
        {
            return false;
        }

        Span<Range> ranges = stackalloc Range[8];
        int splitRanges = versionSpan.SplitAny(ranges, ExSeparators, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        if (splitRanges == 0)
        {
            if (!int.TryParse(versionSpan, null, out int majorOnly))
            {
                return false;
            }

            result = new GameVersion(majorOnly);
            return true;
        }

        Span<int> versionSplits = stackalloc int[4];
        for (int i = 0; i < splitRanges; i++)
        {
            if (!int.TryParse(versionSpan[ranges[i]], null, out int versionParsed))
            {
                return false;
            }

            versionSplits[i] = versionParsed;
        }

        result = new GameVersion(versionSplits);
        return true;
    }

    public readonly string VersionString => string.Join('.', VersionArray);
    public readonly int[] VersionArrayManifest => [Major, Minor, Build, Revision];
    public readonly int[] VersionArray => [Major, Minor, Build];
    public readonly int get_Major() => Major;
    public readonly int get_Minor() => Major;
    public readonly int get_Build() => Build;
    public readonly int get_Revision() => Revision;

    public readonly int Major;
    public readonly int Minor;
    public readonly int Build;
    public readonly int Revision;
}
