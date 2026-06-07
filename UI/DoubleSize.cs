using System;
using System.Runtime.InteropServices;

namespace Hi3Helper.Plugin.Core.UI;

/// <summary>
/// Represents the size of an element, defined by its width and height.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct DoubleSize : IEquatable<DoubleSize>
{
    public double Width;
    public double Height;

    public DoubleSize() { }

    public DoubleSize(double width, double height)
    {
        Width  = width;
        Height = height;
    }

    public bool Equals(DoubleSize other) =>
        Width.Equals(other.Width) &&
        Height.Equals(other.Height);

    public override bool Equals(object? obj) => obj is DoubleSize other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Width, Height);

    public static bool operator ==(DoubleSize left, DoubleSize right) => left.Equals(right);

    public static bool operator !=(DoubleSize left, DoubleSize right) => !(left == right);
}
