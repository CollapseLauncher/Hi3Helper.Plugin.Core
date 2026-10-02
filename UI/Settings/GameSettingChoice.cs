namespace Hi3Helper.Plugin.Core.UI.Settings;

/// <summary>
/// Defines one selectable value for a <see cref="GameSettingKind.Choice"/> setting.
/// </summary>
public sealed class GameSettingChoice(string value, string title)
{
    public string Value { get; init; } = value;
    public string Title { get; init; } = title;
}
