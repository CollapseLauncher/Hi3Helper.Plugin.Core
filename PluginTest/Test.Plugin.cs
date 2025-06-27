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

        PluginInterface.GetPluginName(out string? pluginName);
        logger.LogInformation("IPlugin->GetPluginName(): {PluginName}", pluginName);

        PluginInterface.GetPluginAuthor(out string? pluginAuthor);
        logger.LogInformation("IPlugin->GetPluginAuthor(): {PluginAuthor}", pluginAuthor);

        PluginInterface.GetPluginDescription(out string? pluginDescription);
        logger.LogInformation("IPlugin->GetPluginDescription(): {PluginDescription}", pluginDescription);

        PluginInterface.GetPresetConfigCount(out int pluginPresetConfigCount);
        logger.LogInformation("IPlugin->GetPresetConfigCount(): Found {PluginPresetConfigCount} preset config!", pluginPresetConfigCount);
        for (int i = 0; i < pluginPresetConfigCount; i++)
        {
            IPluginPresetConfig presetConfig = GetPresetConfig(PluginInterface, i, out nint presetConfigAddress);
            logger.LogInformation("  IPlugin->GetPresetConfig({PresetConfigIndex}): Preset config found at: 0x{PresetConfigAddress:x8}", i, presetConfigAddress);

            presetConfig.comGet_GameName(out string presetConfigGameName);
            logger.LogInformation("    IPlugin->GetPresetConfig({PresetConfigIndex})->comGet_GameName(): {GameName}", i, presetConfigGameName);

            presetConfig.comGet_ProfileName(out string presetConfigProfileName);
            logger.LogInformation("    IPlugin->GetPresetConfig({PresetConfigIndex})->comGet_ProfileName(): {ProfileName}", i, presetConfigProfileName);

            presetConfig.comGet_ZoneDescription(out string presetConfigZoneDescription);
            logger.LogInformation("    IPlugin->GetPresetConfig({PresetConfigIndex})->comGet_ZoneDescription(): {ZoneDescription}", i, presetConfigZoneDescription);

            presetConfig.comGet_ZoneName(out string presetConfigZoneName);
            logger.LogInformation("    IPlugin->GetPresetConfig({PresetConfigIndex})->comGet_ZoneName(): {ZoneName}", i, presetConfigZoneName);

            presetConfig.comGet_ZoneFullName(out string presetConfigZoneFullName);
            logger.LogInformation("    IPlugin->GetPresetConfig({PresetConfigIndex})->comGet_ZoneFullName(): {ZoneFullName}", i, presetConfigZoneFullName);

            presetConfig.comGet_ZoneLogoUrl(out string presetConfigZoneLogoUrl);
            logger.LogInformation("    IPlugin->GetPresetConfig({PresetConfigIndex})->comGet_ZoneLogoUrl(): {ZoneLogoUrl}", i, presetConfigZoneLogoUrl);

            presetConfig.comGet_ZonePosterUrl(out string presetConfigZonePosterUrl);
            logger.LogInformation("    IPlugin->GetPresetConfig({PresetConfigIndex})->comGet_ZonePosterUrl(): {ZonePosterUrl}", i, presetConfigZonePosterUrl);

            presetConfig.comGet_ZoneHomePageUrl(out string presetConfigZoneHomePageUrl);
            logger.LogInformation("    IPlugin->GetPresetConfig({PresetConfigIndex})->comGet_ZoneHomePageUrl(): {ZoneHomePageUrl}", i, presetConfigZoneHomePageUrl);

            presetConfig.comGet_ReleaseChannel(out GameReleaseChannel presetConfigGameReleaseChannel);
            logger.LogInformation("    IPlugin->GetPresetConfig({PresetConfigIndex})->comGet_ReleaseChannel(): {GameReleaseChannel}", i, presetConfigGameReleaseChannel);

            presetConfig.comGet_GameMainLanguage(out string presetConfigGameMainLanguage);
            logger.LogInformation("    IPlugin->GetPresetConfig({PresetConfigIndex})->comGet_GameMainLanguage(): {GameMainLanguage}", i, presetConfigGameMainLanguage);

            presetConfig.comGet_GameSupportedLanguagesCount(out int langSupported);
            logger.LogInformation("    IPlugin->GetPresetConfig({PresetConfigIndex})->comGet_GameSupportedLanguagesCount(): {GameLanguageCount}", i, langSupported);
            for (int j = 0; j < langSupported; j++)
            {
                presetConfig.comGet_GameSupportedLanguages(j, out string supportedLanguage);
                logger.LogInformation("      IPlugin->GetPresetConfig({PresetConfigIndex})->comGet_GameSupportedLanguages({LanguageIndex}): {GameLanguage}", i, j, supportedLanguage);
            }

            presetConfig.comGet_GameExecutableName(out string presetConfigGameExecutableName);
            logger.LogInformation("    IPlugin->GetPresetConfig({PresetConfigIndex})->comGet_GameExecutableName(): {GameExecutableName}", i, presetConfigGameExecutableName);

            presetConfig.comGet_LauncherGameDirectoryName(out string presetConfigLauncherGameDirectoryName);
            logger.LogInformation("    IPlugin->GetPresetConfig({PresetConfigIndex})->comGet_LauncherGameDirectoryName(): {LauncherGameDirName}", i, presetConfigLauncherGameDirectoryName);

            logger.LogInformation("    IPlugin->GetPresetConfig({PresetConfigIndex})->InitAsync(): Invoking Asynchronously...", i);
            int value = await presetConfig.InitAsync(in CancelToken).WaitFromHandle<int>();
            logger.LogInformation("Return value: {Value}", value);
        }
    }
}
