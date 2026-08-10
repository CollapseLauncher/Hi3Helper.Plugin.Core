using System.Collections.Generic;

namespace Hi3Helper.Plugin.Core.UI.Settings;

/// <summary>
/// Declaratively describes a game settings page rendered by the launcher.
/// </summary>
public sealed class GameSettingsPage(IReadOnlyList<GameSettingsSection> sections)
{
    public string?                         Title    { get; init; }
    public IReadOnlyList<GameSettingsSection> Sections { get; init; } = sections;
}
