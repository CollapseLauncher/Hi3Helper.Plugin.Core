using System;
using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace Hi3Helper.Plugin.Core.UI.Controls;

/// <summary>
/// Represents the position and size of a game event button within a canvas, including its horizontal and vertical alignment, position, icon size, and background frame size.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public struct GameEventButtonPosition : IEquatable<GameEventButtonPosition>
{
    public readonly int                 Version;
    public          HorizontalAlignment _horizontalAlignment;
    public          VerticalAlignment   _verticalAlignment;
    public          DoublePosition      _position;
    public          DoubleSize          _iconSize;
    public          DoubleSize          _frameSize;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameEventButtonPosition"/> struct with the specified version, horizontal alignment, vertical alignment, position, icon size, and frame size.
    /// </summary>
    /// <param name="version">The version of the game event button position.</param>
    /// <param name="horizontalAlignment">The horizontal alignment of the game event button.</param>
    /// <param name="verticalAlignment">The vertical alignment of the game event button.</param>
    /// <param name="position">The position of the game event button within the canvas.</param>
    /// <param name="iconSize">The size of the game event button's icon.</param>
    /// <param name="frameSize">The size of the background frame.</param>
    public GameEventButtonPosition(int                 version,
                                   HorizontalAlignment horizontalAlignment,
                                   VerticalAlignment   verticalAlignment,
                                   DoublePosition      position,
                                   DoubleSize          iconSize,
                                   DoubleSize          frameSize)
    {
        Version              = version;
        _horizontalAlignment = horizontalAlignment;
        _verticalAlignment   = verticalAlignment;
        _position            = position;
        _iconSize            = iconSize;
        _frameSize           = frameSize;
    }

    public GameEventButtonPosition() { }

    /// <summary>
    /// Gets the default <see cref="GameEventButtonPosition"/> instance with predefined values for miHoYo/HoYoverse game event button position in HoYoPlay Launcher.
    /// </summary>
    public static GameEventButtonPosition Default
        => new(1,
               HorizontalAlignment.Left,
               VerticalAlignment.Top,
               new DoublePosition(256, 596, 0, 0),
               new DoubleSize(double.NaN, 80),
               new DoubleSize(2560, 1440));

    /// <summary>
    /// Gets an empty <see cref="GameEventButtonPosition"/> instance with all fields set to their default values (sets to 0).
    /// </summary>
    public static GameEventButtonPosition Empty => new();

    /// <summary>
    /// Whether the GameEventButtonPosition is empty and all the fields are set to default values.
    /// </summary>
    [JsonIgnore]
    public bool IsEmpty => Version == 0 &&
                           _horizontalAlignment == 0 &&
                           _verticalAlignment == 0 &&
                           _position == default &&
                           _iconSize == default &&
                           _frameSize == default;

    /// <summary>
    /// Gets the horizontal alignment of the game event button.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<HorizontalAlignment>))]
    public HorizontalAlignment HorizontalAlignment
    {
        get => _horizontalAlignment;
        init => _horizontalAlignment = value;
    }

    /// <summary>
    /// Gets the vertical alignment of the game event button.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<VerticalAlignment>))]
    public VerticalAlignment VerticalAlignment
    {
        get => _verticalAlignment;
        init => _verticalAlignment = value;
    }

    /// <summary>
    /// Gets the position of the game event button within the canvas, defined by its left, top, right, and bottom coordinates respecting the background's <see cref="FrameSize"/>.
    /// This property, though, can be referred as margin of the button.
    /// </summary>
    public DoublePosition Position
    {
        get => _position;
        init => _position = value;
    }

    /// <summary>
    /// Gets the size of the game event button's icon, defined by its width and height.
    /// The width or height can be set to <see cref="double.NaN"/> to automatically adjust its size based on respecting aspect ratio.
    /// </summary>
    public DoubleSize IconSize
    {
        get => _iconSize;
        init => _iconSize = value;
    }

    /// <summary>
    /// Gets the size of the background canvas.
    /// </summary>
    public DoubleSize FrameSize
    {
        get => _frameSize;
        init => _frameSize = value;
    }

    /// <summary>
    /// Determines whether the specified <see cref="GameEventButtonPosition"/> is equal to the current instance by comparing all its fields.
    /// </summary>
    /// <param name="other">The <see cref="GameEventButtonPosition"/> to compare with the current instance.</param>
    /// <returns><c>true</c> if the specified <see cref="GameEventButtonPosition"/> is equal to the current instance; otherwise, <c>false</c>.</returns>
    public bool Equals(GameEventButtonPosition other)
        => Version == other.Version &&
           _horizontalAlignment == other._horizontalAlignment &&
           _verticalAlignment == other._verticalAlignment &&
           _position.Equals(other._position) &&
           _iconSize.Equals(other._iconSize) &&
           _frameSize.Equals(other._frameSize);

    /// <summary>
    /// Determines whether the specified object is equal to the current instance by checking if it is a <see cref="GameEventButtonPosition"/> and then comparing all its fields.
    /// </summary>
    /// <param name="obj">The object to compare with the current instance.</param>
    /// <returns><c>true</c> if the specified object is a <see cref="GameEventButtonPosition"/> and is equal to the current instance; otherwise, <c>false</c>.</returns>
    public override bool Equals(object? obj) => obj is GameEventButtonPosition other && Equals(other);

    /// <summary>
    /// Returns a hash code for the current instance by combining the hash codes of all its fields.
    /// </summary>
    /// <returns>A hash code for the current instance.</returns>
    public override int GetHashCode()
        => HashCode.Combine(Version,
                            _horizontalAlignment,
                            _verticalAlignment,
                            _position,
                            _iconSize,
                            _frameSize);

    /// <summary>
    /// Determines whether two specified <see cref="GameEventButtonPosition"/> instances are equal by comparing all their fields.
    /// </summary>
    /// <param name="left">The first <see cref="GameEventButtonPosition"/> to compare.</param>
    /// <param name="right">The second <see cref="GameEventButtonPosition"/> to compare.</param>
    /// <returns><c>true</c> if the two <see cref="GameEventButtonPosition"/> instances are equal; otherwise, <c>false</c>.</returns>
    public static bool operator ==(GameEventButtonPosition left, GameEventButtonPosition right) => left.Equals(right);

    /// <summary>
    /// Determines whether two specified <see cref="GameEventButtonPosition"/> instances are not equal by comparing all their fields.
    /// </summary>
    /// <param name="left">The first <see cref="GameEventButtonPosition"/> to compare.</param>
    /// <param name="right">The second <see cref="GameEventButtonPosition"/> to compare.</param>
    /// <returns><c>true</c> if the two <see cref="GameEventButtonPosition"/> instances are not equal; otherwise, <c>false</c>.</returns>
    public static bool operator !=(GameEventButtonPosition left, GameEventButtonPosition right) => !(left == right);
}
