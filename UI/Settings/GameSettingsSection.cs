using System.Collections.Generic;

namespace Hi3Helper.Plugin.Core.UI.Settings;

/// <summary>
/// Groups related entries on a plugin-provided game settings page.
/// </summary>
public sealed class GameSettingsSection(string title, IReadOnlyList<GameSettingEntry> entries)
{
    public string                      Title       { get; init; } = title;
    public string?                     Description { get; init; }
    public IReadOnlyList<GameSettingEntry> Entries { get; init; } = entries;
}
