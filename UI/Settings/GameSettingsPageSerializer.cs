using System.Text.Json;
using System.Text.Json.Serialization;

namespace Hi3Helper.Plugin.Core.UI.Settings;

/// <summary>
/// Serializes the declarative settings contract passed across the plugin ABI.
/// </summary>
public static class GameSettingsPageSerializer
{
    public static string Serialize(GameSettingsPage page) =>
        JsonSerializer.Serialize(page, GameSettingsPageJsonContext.Default.GameSettingsPage);

    public static GameSettingsPage? Deserialize(string json) =>
        JsonSerializer.Deserialize(json, GameSettingsPageJsonContext.Default.GameSettingsPage);
}

[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(GameSettingsPage))]
internal sealed partial class GameSettingsPageJsonContext : JsonSerializerContext;
