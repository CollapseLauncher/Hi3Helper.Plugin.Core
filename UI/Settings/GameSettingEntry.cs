using System;
using System.Collections.Generic;
using System.Globalization;

namespace Hi3Helper.Plugin.Core.UI.Settings;

/// <summary>
/// Defines one editable setting in a plugin-provided game settings page.
/// </summary>
public sealed class GameSettingEntry
{
    public required string          Key         { get; init; }
    public required string          Title       { get; init; }
    public          string?         Description { get; init; }
    public required GameSettingKind Kind        { get; init; }
    public required string          Value       { get; init; }
    public          string?         Placeholder { get; init; }
    public          double          Minimum     { get; init; }
    public          double          Maximum     { get; init; } = 100;
    public          double          Step        { get; init; } = 1;
    public          IReadOnlyList<GameSettingChoice>? Choices { get; init; }

    public static GameSettingEntry Toggle(string key, string title, bool value, string? description = null) =>
        new()
        {
            Key = key,
            Title = title,
            Description = description,
            Kind = GameSettingKind.Toggle,
            Value = value ? bool.TrueString : bool.FalseString
        };

    public static GameSettingEntry Text(string key, string title, string? value = null, string? description = null,
                                        string? placeholder = null) =>
        new()
        {
            Key = key,
            Title = title,
            Description = description,
            Kind = GameSettingKind.Text,
            Value = value ?? string.Empty,
            Placeholder = placeholder
        };

    public static GameSettingEntry Number(string key, string title, double value, double minimum = double.MinValue,
                                          double maximum = double.MaxValue, double step = 1,
                                          string? description = null) =>
        Numeric(key, title, value, minimum, maximum, step, description, GameSettingKind.Number);

    public static GameSettingEntry Slider(string key, string title, double value, double minimum, double maximum,
                                          double step = 1, string? description = null) =>
        Numeric(key, title, value, minimum, maximum, step, description, GameSettingKind.Slider);

    public static GameSettingEntry Choice(string key, string title, string value,
                                          IReadOnlyList<GameSettingChoice> choices,
                                          string? description = null) =>
        new()
        {
            Key = key,
            Title = title,
            Description = description,
            Kind = GameSettingKind.Choice,
            Value = value,
            Choices = choices
        };

    private static GameSettingEntry Numeric(string key, string title, double value, double minimum, double maximum,
                                            double step, string? description, GameSettingKind kind)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(minimum, maximum);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(step);

        return new GameSettingEntry
        {
            Key = key,
            Title = title,
            Description = description,
            Kind = kind,
            Value = value.ToString(CultureInfo.InvariantCulture),
            Minimum = minimum,
            Maximum = maximum,
            Step = step
        };
    }
}
