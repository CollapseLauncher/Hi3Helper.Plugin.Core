using Hi3Helper.Plugin.Core.UI.Settings;
using System;

namespace PluginTest;

internal static partial class Test
{
    internal static void TestGameSettingsContract()
    {
        GameSettingsPage source = new([
            new GameSettingsSection("General", [
                GameSettingEntry.Toggle("enabled", "Enabled", true),
                GameSettingEntry.Text("name", "Name", "Collapse"),
                GameSettingEntry.Number("count", "Count", 3, 0, 10),
                GameSettingEntry.Slider("volume", "Volume", 75, 0, 100),
                GameSettingEntry.Choice("language", "Language", "en", [
                    new GameSettingChoice("en", "English")
                ])
            ])
        ]) { Title = "Settings" };

        string json = GameSettingsPageSerializer.Serialize(source);
        GameSettingsPage result = GameSettingsPageSerializer.Deserialize(json)
            ?? throw new InvalidOperationException("Deserialization returned null");

        if (result.Title != source.Title || result.Sections.Count != 1 || result.Sections[0].Entries.Count != 5)
        {
            throw new InvalidOperationException("The game settings contract did not round-trip");
        }
    }
}
