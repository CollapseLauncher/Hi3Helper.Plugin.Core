using System;
using System.Runtime.InteropServices;
// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMember.Global

namespace Hi3Helper.Plugin.Core.Management;

/// <summary>
/// Represents a version with major, minor, build, and revision components.
/// Provides methods for comparison, conversion, and string representation.
/// </summary>
/// <remarks>
/// This interface is designed for use in plugin management scenarios where versioning is required.
/// It supports comparison operators and conversion to the standard <see cref="Version"/> type.
/// </remarks>
public interface IVersion : IEquatable<IVersion>
{
    /// <summary>
    /// Gets the major component of the version.
    /// </summary>
    /// <returns>The major version number.</returns>
    int get_Major();

    /// <summary>
    /// Gets the minor component of the version.
    /// </summary>
    /// <returns>The minor version number.</returns>
    int get_Minor();

    /// <summary>
    /// Gets the build component of the version.
    /// </summary>
    /// <returns>The build version number.</returns>
    int get_Build();

    /// <summary>
    /// Gets the revision component of the version.
    /// </summary>
    /// <returns>The revision version number.</returns>
    int get_Revision();

    /// <summary>
    /// Returns the version components as a read-only span of integers.
    /// </summary>
    /// <returns>A <see cref="ReadOnlySpan{Int32}"/> containing major, minor, build, and revision.</returns>
    ReadOnlySpan<int> AsSpan();

    /// <summary>
    /// Returns the string representation of the version.
    /// </summary>
    /// <returns>
    /// A string representing the version, or <c>null</c> if not available.
    /// </returns>
    [return: MarshalAs(UnmanagedType.LPWStr)]
    string? ToString();

    /// <summary>
    /// Converts this instance to a <see cref="Version"/> object.
    /// </summary>
    /// <returns>A <see cref="Version"/> representing this version.</returns>
    Version ToVersion();

    /// <summary>
    /// Determines whether one <see cref="IVersion"/> is greater than another.
    /// </summary>
    /// <param name="left">The first version to compare.</param>
    /// <param name="right">The second version to compare.</param>
    /// <returns>
    /// <c>true</c> if <paramref name="left"/> is greater than <paramref name="right"/>; otherwise, <c>false</c>.
    /// </returns>
    public static bool operator >(IVersion? left, IVersion? right)
    {
        if (left == null || right == null)
        {
            return false;
        }

        return left.get_Major() > right.get_Major() ||
               (left.get_Major() == right.get_Major() && left.get_Minor() > right.get_Minor()) ||
               (left.get_Major() == right.get_Major() && left.get_Minor() == right.get_Minor() && left.get_Build() > right.get_Build()) ||
               (left.get_Major() == right.get_Major() && left.get_Minor() == right.get_Minor() && left.get_Build() == right.get_Build() && left.get_Revision() > right.get_Revision());
    }

    /// <summary>
    /// Determines whether one <see cref="IVersion"/> is less than another.
    /// </summary>
    /// <param name="left">The first version to compare.</param>
    /// <param name="right">The second version to compare.</param>
    /// <returns>
    /// <c>true</c> if <paramref name="left"/> is less than <paramref name="right"/>; otherwise, <c>false</c>.
    /// </returns>
    public static bool operator <(IVersion? left, IVersion? right) =>
        right > left;
}
