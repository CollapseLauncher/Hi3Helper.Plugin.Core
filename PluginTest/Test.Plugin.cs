using Hi3Helper.Plugin.Core;
using Hi3Helper.Plugin.Core.Management;
using Hi3Helper.Plugin.Core.Management.PresetConfig;
using Hi3Helper.Plugin.Core.Utility;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace PluginTest;

internal static partial class Test
{
    private static void ThrowIfPluginIsNull([NotNull] IPlugin? plugin)
    {
        if (plugin == null)
        {
            throw new NullReferenceException("IPlugin cannot be null!");
        }
    }

    internal static async Task TestGetPlugin(PluginGetPlugin delegateIn, ILogger logger)
    {
        PluginInterface = GetPlugin(delegateIn, out nint pluginInterfaceAddress);
        CancelToken = Guid.CreateVersion7();

        _ = Task.Run(() =>
        {
            _ = Console.ReadLine();
            PluginInterface?.CancelAsync(in CancelToken);
        });

        if (PluginInterface == null)
        {
            throw new InvalidOperationException($"Cannot marshal pointer at: 0x{pluginInterfaceAddress:x8} into IPlugin interface");
        }
        logger.LogInformation("IPlugin marshalled at: 0x{PluginInterfaceAddress:x8}", pluginInterfaceAddress);

        DateTime pluginCreationDate = GetDateTime(PluginInterface);
        logger.LogInformation("IPlugin->GetPluginCreationDate(): {PluginCreationDate} UTC", pluginCreationDate.ToString("F"));

        string pluginName = PluginInterface.GetPluginName();
        logger.LogInformation("IPlugin->GetPluginName(): {PluginName}", pluginName);

        string pluginAuthor = PluginInterface.GetPluginAuthor();
        logger.LogInformation("IPlugin->GetPluginAuthor(): {PluginAuthor}", pluginAuthor);

        string pluginDescription = PluginInterface.GetPluginDescription();
        logger.LogInformation("IPlugin->GetPluginDescription(): {PluginDescription}", pluginDescription);

        int pluginPresetConfigCount = PluginInterface.GetPresetConfigCount();
        logger.LogInformation("IPlugin->GetPresetConfigCount(): Found {PluginPresetConfigCount} preset config!", pluginPresetConfigCount);
        for (int i = 0; i < pluginPresetConfigCount; i++)
        {
            IPluginPresetConfig presetConfig = GetPresetConfig(PluginInterface, i, out nint presetConfigAddress);
            logger.LogInformation("  IPlugin->GetPresetConfig({PresetConfigIndex}): Preset config found at: 0x{PresetConfigAddress:x8}", i, presetConfigAddress);

            string presetConfigGameName = presetConfig.get_GameName();
            logger.LogInformation("    IPlugin->GetPresetConfig({PresetConfigIndex})->get_GameName(): {GameName}", i, presetConfigGameName);

            string presetConfigProfileName = presetConfig.get_ProfileName();
            logger.LogInformation("    IPlugin->GetPresetConfig({PresetConfigIndex})->get_ProfileName(): {ProfileName}", i, presetConfigProfileName);

            string presetConfigZoneDescription = presetConfig.get_ZoneDescription();
            logger.LogInformation("    IPlugin->GetPresetConfig({PresetConfigIndex})->get_ZoneDescription(): {ZoneDescription}", i, presetConfigZoneDescription);

            string presetConfigZoneName = presetConfig.get_ZoneName();
            logger.LogInformation("    IPlugin->GetPresetConfig({PresetConfigIndex})->get_ZoneName(): {ZoneName}", i, presetConfigZoneName);

            string presetConfigZoneFullName = presetConfig.get_ZoneFullName();
            logger.LogInformation("    IPlugin->GetPresetConfig({PresetConfigIndex})->get_ZoneFullName(): {ZoneFullName}", i, presetConfigZoneFullName);

            string presetConfigZoneLogoUrl = presetConfig.get_ZoneLogoUrl();
            logger.LogInformation("    IPlugin->GetPresetConfig({PresetConfigIndex})->get_ZoneLogoUrl(): {ZoneLogoUrl}", i, presetConfigZoneLogoUrl);

            string presetConfigZonePosterUrl = presetConfig.get_ZonePosterUrl();
            logger.LogInformation("    IPlugin->GetPresetConfig({PresetConfigIndex})->get_ZonePosterUrl(): {ZonePosterUrl}", i, presetConfigZonePosterUrl);

            string presetConfigZoneHomePageUrl = presetConfig.get_ZoneHomePageUrl();
            logger.LogInformation("    IPlugin->GetPresetConfig({PresetConfigIndex})->get_ZoneHomePageUrl(): {ZoneHomePageUrl}", i, presetConfigZoneHomePageUrl);

            GameReleaseChannel presetConfigGameReleaseChannel = presetConfig.get_ReleaseChannel();
            logger.LogInformation("    IPlugin->GetPresetConfig({PresetConfigIndex})->get_ReleaseChannel(): {GameReleaseChannel}", i, presetConfigGameReleaseChannel);

            string presetConfigGameMainLanguage = presetConfig.get_GameMainLanguage();
            logger.LogInformation("    IPlugin->GetPresetConfig({PresetConfigIndex})->get_GameMainLanguage(): {GameMainLanguage}", i, presetConfigGameMainLanguage);

            int langSupported = presetConfig.get_GameSupportedLanguagesCount();
            logger.LogInformation("    IPlugin->GetPresetConfig({PresetConfigIndex})->get_GameSupportedLanguagesCount(): {GameLanguageCount}", i, langSupported);
            for (int j = 0; j < langSupported; j++)
            {
                string supportedLanguage = presetConfig.get_GameSupportedLanguages(j);
                logger.LogInformation("      IPlugin->GetPresetConfig({PresetConfigIndex})->get_GameSupportedLanguages({LanguageIndex}): {GameLanguage}", i, j, supportedLanguage);
            }

            string presetConfigGameExecutableName = presetConfig.get_GameExecutableName();
            logger.LogInformation("    IPlugin->GetPresetConfig({PresetConfigIndex})->get_GameExecutableName(): {GameExecutableName}", i, presetConfigGameExecutableName);

            string presetConfigLauncherGameDirectoryName = presetConfig.get_LauncherGameDirectoryName();
            logger.LogInformation("    IPlugin->GetPresetConfig({PresetConfigIndex})->get_LauncherGameDirectoryName(): {LauncherGameDirName}", i, presetConfigLauncherGameDirectoryName);

            logger.LogInformation("    IPlugin->GetPresetConfig({PresetConfigIndex})->InitAsync(): Invoking Asynchronously...", i);
            int value = await presetConfig.InitAsync(in CancelToken).WaitFromHandle<int>();
            logger.LogInformation("Return value: {Value}", value);
        }
    }
}
