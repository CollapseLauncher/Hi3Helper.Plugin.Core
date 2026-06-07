using System;
using System.Runtime.InteropServices;

namespace Hi3Helper.Plugin.Core.UI;

/// <summary>
/// Represents the position of an element within a canvas, defined by its left, top, right, and bottom coordinates.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct DoublePosition : IEquatable<DoublePosition>
{
    public double Left;
    public double Top;
    public double Right;
    public double Bottom;

    public DoublePosition() { }

    public DoublePosition(double left, double top, double right, double bottom)
    {
        Left   = left;
        Top    = top;
        Right  = right;
        Bottom = bottom;
    }

    public bool Equals(DoublePosition other) =>
        Left.Equals(other.Left) &&
        Top.Equals(other.Top) &&
        Right.Equals(other.Right) &&
        Bottom.Equals(other.Bottom);

    public override bool Equals(object? obj) => obj is DoublePosition other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Left, Top, Right, Bottom);

    public static bool operator ==(DoublePosition left, DoublePosition right) => left.Equals(right);

    public static bool operator !=(DoublePosition left, DoublePosition right) => !(left == right);
}
