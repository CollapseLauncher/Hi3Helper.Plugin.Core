using System;
using System.Runtime.InteropServices;
// ReSharper disable UnusedMemberInSuper.Global
// ReSharper disable UnusedMember.Global

namespace Hi3Helper.Plugin.Core.Management;

public interface IVersion : IEquatable<IVersion>
{
    int get_Major();
    int get_Minor();
    int get_Build();
    int get_Revision();

    ReadOnlySpan<int> AsSpan();

    [return: MarshalAs(UnmanagedType.LPWStr)]
    string? ToString();
    Version ToVersion();

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

    public static bool operator <(IVersion? left, IVersion? right) =>
        right > left;
}
