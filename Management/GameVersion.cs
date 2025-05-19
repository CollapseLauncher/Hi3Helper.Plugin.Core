using Hi3Helper.Plugin.Core.Utility;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
// ReSharper disable UnusedMember.Global

namespace Hi3Helper.Plugin.Core.Management;

[StructLayout(LayoutKind.Sequential)]
public struct GameVersion : IVersion
{
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
        if (!TryParse(version, out GameVersion? versionOut) || !versionOut.HasValue)
        {
            throw new ArgumentException($"Version in the config.ini should be either in \"x\", \"x.x\", \"x.x.x\" or \"x.x.x.x\" format or all the values aren't numbers! (current value: \"{version}\")");
        }

        Major = versionOut.Value.Major;
        Minor = versionOut.Value.Minor;
        Build = versionOut.Value.Build;
        Revision = versionOut.Value.Revision;
    }

    public static bool TryParse(string? version, [NotNullWhen(true)] out GameVersion? result)
    {
        const string separators = ",.;|";

        result = null;
        if (string.IsNullOrEmpty(version))
        {
            return false;
        }

        Span<Range> ranges = stackalloc Range[8];
        ReadOnlySpan<char> versionSpan = version.AsSpan();
        int splitRanges = versionSpan.SplitAny(ranges, separators, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

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

    public GameVersion GetIncrementedVersion()
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

    public unsafe ReadOnlySpan<int> AsSpan() => new(this.AsPointer(), 4);
    public Version ToVersion() => new(Major, Minor, Build, Revision);
    public override string ToString() => $"{Major}.{Minor}.{Build}.{Revision}";

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
        if (!left.HasValue || !TryParse(right, out GameVersion? rightAsParsed))
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

    public bool Equals(IVersion? other) =>
        EqualsInner(this, other);

    public override bool Equals([NotNullWhen(true)] object? obj) =>
        EqualsInner(this, obj);

    public override int GetHashCode() =>
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
            TryParse(toVersionStr, out GameVersion? toVersionParsed))
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

    public string VersionString => string.Join('.', VersionArray);
    public int[] VersionArrayManifest => [Major, Minor, Build, Revision];
    public int[] VersionArray => [Major, Minor, Build];
    public int get_Major() => Major;
    public int get_Minor() => Major;
    public int get_Build() => Build;
    public int get_Revision() => Revision;

    public readonly int Major;
    public readonly int Minor;
    public readonly int Build;
    public readonly int Revision;
}
