using Hi3Helper.Plugin.Core.Management;
using Hi3Helper.Plugin.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.InteropServices.Marshalling;

#pragma warning disable CA2253
namespace PluginTest
{
    internal static unsafe class Test
    {
        internal static void TestGetPluginStandardVersion(PluginGetPluginVersion delegateIn, ILogger logger)
        {
            logger.LogInformation("Plugin Standard Version: {DelegateGetPluginVersion}", delegateIn()->ToString());
        }

        internal static void TestGetPluginVersion(PluginGetPluginVersion delegateIn, ILogger logger)
        {
            logger.LogInformation("Plugin Version: {DelegateGetPluginVersion}", delegateIn()->ToString());
        }

        internal static void TestGetPlugin(PluginGetPlugin delegateIn, ILogger logger)
        {
            void* pluginInterfaceAddress = delegateIn();
            IPlugin? pluginInterface = ComInterfaceMarshaller<IPlugin>.ConvertToManaged(pluginInterfaceAddress);

            if (pluginInterface == null)
            {
                throw new InvalidOperationException($"Cannot marshal pointer at: 0x{(nint)pluginInterfaceAddress:x8} into IPlugin interface");
            }
            logger.LogInformation("IPlugin marshalled at: 0x{0:x8}", (nint)pluginInterfaceAddress);

            DateTime* pluginCreationDate = pluginInterface.GetPluginCreationDate();
            logger.LogInformation("IPlugin->GetPluginCreationDate(): {0} UTC", pluginCreationDate->ToString("F"));

            string pluginName = pluginInterface.GetPluginName();
            logger.LogInformation("IPlugin->GetPluginName(): {0}", pluginName);

            string pluginAuthor = pluginInterface.GetPluginAuthor();
            logger.LogInformation("IPlugin->GetPluginAuthor(): {0}", pluginAuthor);

            string pluginDescription = pluginInterface.GetPluginDescription();
            logger.LogInformation("IPlugin->GetPluginDescription(): {0}", pluginDescription);

            int pluginPresetConfigCount = pluginInterface.GetPresetConfigCount();
            logger.LogInformation("IPlugin->GetPresetConfigCount(): Found {0} preset config!", pluginPresetConfigCount);
            for (int i = 0; i < pluginPresetConfigCount; i++)
            {
                IPluginPresetConfig presetConfig = pluginInterface.GetPresetConfig(i);
                nint presetConfigAddress = (nint)ComInterfaceMarshaller<IPluginPresetConfig>.ConvertToUnmanaged(presetConfig);
                logger.LogInformation("  IPlugin->GetPresetConfig({0}): Preset config found at: 0x{1:x8}", i, presetConfigAddress);

                string presetConfigGameName = presetConfig.get_GameName();
                logger.LogInformation("    IPlugin->GetPresetConfig({0})->get_GameName(): {1}", i, presetConfigGameName);

                string presetConfigProfileName = presetConfig.get_ProfileName();
                logger.LogInformation("    IPlugin->GetPresetConfig({0})->get_ProfileName(): {1}", i, presetConfigProfileName);

                string presetConfigZoneDescription = presetConfig.get_ZoneDescription();
                logger.LogInformation("    IPlugin->GetPresetConfig({0})->get_ZoneDescription(): {1}", i, presetConfigZoneDescription);

                string presetConfigZoneName = presetConfig.get_ZoneName();
                logger.LogInformation("    IPlugin->GetPresetConfig({0})->get_ZoneName(): {1}", i, presetConfigZoneName);

                string presetConfigZoneFullName = presetConfig.get_ZoneFullName();
                logger.LogInformation("    IPlugin->GetPresetConfig({0})->get_ZoneFullName(): {1}", i, presetConfigZoneFullName);

                string presetConfigZoneLogoUrl = presetConfig.get_ZoneLogoUrl();
                logger.LogInformation("    IPlugin->GetPresetConfig({0})->get_ZoneLogoUrl(): {1}", i, presetConfigZoneLogoUrl);

                string presetConfigZonePosterUrl = presetConfig.get_ZonePosterUrl();
                logger.LogInformation("    IPlugin->GetPresetConfig({0})->get_ZonePosterUrl(): {1}", i, presetConfigZonePosterUrl);

                string presetConfigZoneHomePageUrl = presetConfig.get_ZoneHomePageUrl();
                logger.LogInformation("    IPlugin->GetPresetConfig({0})->get_ZoneHomePageUrl(): {1}", i, presetConfigZoneHomePageUrl);

                GameReleaseChannel presetConfigGameReleaseChannel = presetConfig.get_ReleaseChannel();
                logger.LogInformation("    IPlugin->GetPresetConfig({0})->get_ReleaseChannel(): {1}", i, presetConfigGameReleaseChannel);

                string presetConfigGameMainLanguage = presetConfig.get_GameMainLanguage();
                logger.LogInformation("    IPlugin->GetPresetConfig({0})->get_GameMainLanguage(): {1}", i, presetConfigGameMainLanguage);

                int langSupported = presetConfig.get_GameSupportedLanguagesCount();
                logger.LogInformation("    IPlugin->GetPresetConfig({0})->get_GameSupportedLanguagesCount(): {1}", i, langSupported);
                for (int j = 0; j < langSupported; j++)
                {
                    string supportedLanguage = presetConfig.get_GameSupportedLanguages(j);
                    logger.LogInformation("      IPlugin->GetPresetConfig({0})->get_GameSupportedLanguages({1}): {2}", i, j, supportedLanguage);
                }

                string presetConfigGameExecutableName = presetConfig.get_GameExecutableName();
                logger.LogInformation("    IPlugin->GetPresetConfig({0})->get_GameExecutableName(): {1}", i, presetConfigGameExecutableName);

                string presetConfigLauncherGameDirectoryName = presetConfig.get_LauncherGameDirectoryName();
                logger.LogInformation("    IPlugin->GetPresetConfig({0})->get_LauncherGameDirectoryName(): {1}", i, presetConfigLauncherGameDirectoryName);
            }
        }
    }
}
