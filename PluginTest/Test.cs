using Hi3Helper.Plugin.Core;
using Hi3Helper.Plugin.Core.Management;
using Hi3Helper.Plugin.Core.Management.Api;
using Hi3Helper.Plugin.Core.Management.PresetConfig;
using Hi3Helper.Plugin.Core.Utility;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Threading.Tasks;

namespace PluginTest
{
    internal static class Test
    {
        private static unsafe IPlugin? GetPlugin(PluginGetPlugin delegateIn, out nint address)
        {
            void* pluginInterfaceAddress = delegateIn();
            address = (nint)pluginInterfaceAddress;
            return ComInterfaceMarshaller<IPlugin>.ConvertToManaged(pluginInterfaceAddress);
        }

        private static unsafe DateTime GetDateTime(IPlugin pluginInterface)
        {
            DateTime* pluginCreationDate = pluginInterface.GetPluginCreationDate();
            return *pluginCreationDate;
        }

        private static unsafe IPluginPresetConfig GetPresetConfig(IPlugin pluginInterface, int index, out nint presetConfigAddress)
        {
            IPluginPresetConfig presetConfig = pluginInterface.GetPresetConfig(index);
            presetConfigAddress = (nint)ComInterfaceMarshaller<IPluginPresetConfig>.ConvertToUnmanaged(presetConfig);

            return presetConfig;
        }

        internal static unsafe void TestGetPluginStandardVersion(PluginGetPluginVersion delegateIn, ILogger logger)
        {
            logger.LogInformation("Plugin Standard Version: {DelegateGetPluginVersion}", delegateIn()->ToString());
        }

        internal static unsafe void TestGetPluginVersion(PluginGetPluginVersion delegateIn, ILogger logger)
        {
            logger.LogInformation("Plugin Version: {DelegateGetPluginVersion}", delegateIn()->ToString());
        }

        internal static void TestSetLoggerCallback(PluginSetLoggerCallback delegateIn, ILogger logger)
        {
            nint pointerToCallback = Marshal.GetFunctionPointerForDelegate<SharedLoggerCallback>(PluginLoggerCallback);
            delegateIn(pointerToCallback);
            logger.LogInformation("Logger attached at address: 0x{DelegateSetLoggerCallback:x8}", pointerToCallback);
        }

        internal static void TestSetDnsResolverCallback(PluginSetDnsResolverCallback delegateIn, ILogger logger)
        {
            nint pointerToCallback = Marshal.GetFunctionPointerForDelegate<SharedDnsResolverCallback>(PluginDnsResolverCallback);
            delegateIn(pointerToCallback);
            logger.LogInformation("DNS Resolver attached at address: 0x{DelegateSetDnsResolverCallback:x8}", pointerToCallback);
        }

        internal static void TestResetSetDnsResolverCallback(PluginSetDnsResolverCallback delegateIn, ILogger logger)
        {
            delegateIn(nint.Zero);
            logger.LogInformation("DNS Resolver has been detached!");
        }

        private static void PluginDnsResolverCallback(string host, out string[] ipAddresses)
        {
            ipAddresses = ["127.0.0.1"];
        }

        private static void PluginLoggerCallback(LogLevel logLevel, EventId eventId, string message)
        {
            switch (logLevel)
            {
                case LogLevel.None:
                    break;
                case LogLevel.Trace:
                    Program.InvokeLogger.LogTrace(eventId, "[Plugin Log: TRACE] {message}", message);
                    break;
                case LogLevel.Information:
                    Program.InvokeLogger.LogInformation(eventId, "[Plugin Log: INFO] {message}", message);
                    break;
                case LogLevel.Debug:
                    Program.InvokeLogger.LogDebug(eventId, "[Plugin Log: DEBUG] {message}", message);
                    break;
                case LogLevel.Warning:
                    Program.InvokeLogger.LogWarning(eventId, "[Plugin Log: WARNING] {message}", message);
                    break;
                case LogLevel.Error:
                    Program.InvokeLogger.LogError(eventId, "[Plugin Log: ERROR] {message}", message);
                    break;
                case LogLevel.Critical:
                    Program.InvokeLogger.LogCritical(eventId, "[Plugin Log: CRITICAL] {message}", message);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel, null);
            }
        }

        private static readonly Guid CancelToken = Guid.CreateVersion7();
        private static IPlugin? _pluginInterface;

        internal static async Task TestGetPlugin(PluginGetPlugin delegateIn, ILogger logger)
        {
            _ = Task.Run(() =>
            {
                _ = Console.ReadLine();
                _pluginInterface?.CancelAsync(in CancelToken);
            });

            IPlugin? pluginInterface = GetPlugin(delegateIn, out nint pluginInterfaceAddress);
            _pluginInterface = pluginInterface;

            if (pluginInterface == null)
            {
                throw new InvalidOperationException($"Cannot marshal pointer at: 0x{pluginInterfaceAddress:x8} into IPlugin interface");
            }
            logger.LogInformation("IPlugin marshalled at: 0x{PluginInterfaceAddress:x8}", pluginInterfaceAddress);

            DateTime pluginCreationDate = GetDateTime(pluginInterface);
            logger.LogInformation("IPlugin->GetPluginCreationDate(): {PluginCreationDate} UTC", pluginCreationDate.ToString("F"));

            string pluginName = pluginInterface.GetPluginName();
            logger.LogInformation("IPlugin->GetPluginName(): {PluginName}", pluginName);

            string pluginAuthor = pluginInterface.GetPluginAuthor();
            logger.LogInformation("IPlugin->GetPluginAuthor(): {PluginAuthor}", pluginAuthor);

            string pluginDescription = pluginInterface.GetPluginDescription();
            logger.LogInformation("IPlugin->GetPluginDescription(): {PluginDescription}", pluginDescription);

            int pluginPresetConfigCount = pluginInterface.GetPresetConfigCount();
            logger.LogInformation("IPlugin->GetPresetConfigCount(): Found {PluginPresetConfigCount} preset config!", pluginPresetConfigCount);
            for (int i = 0; i < pluginPresetConfigCount; i++)
            {
                IPluginPresetConfig presetConfig = GetPresetConfig(pluginInterface, i, out nint presetConfigAddress);
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
                long value = 0;
                await presetConfig.InitAsync(in CancelToken, result => value = result).WaitFromHandle();
                logger.LogInformation("Return value: {Value}", value);
            }
        }

        internal static async Task TestApiMedia(PluginGetPlugin delegateIn, ILogger logger)
        {
            IPlugin? pluginInterface = GetPlugin(delegateIn, out nint pluginInterfaceAddress);
            if (pluginInterface == null)
            {
                throw new InvalidOperationException($"Cannot marshal pointer at: 0x{pluginInterfaceAddress:x8} into IPlugin interface");
            }

            int pluginPresetConfigCount = pluginInterface.GetPresetConfigCount();
            for (int i = 0; i < pluginPresetConfigCount; i++)
            {
                IPluginPresetConfig presetConfig = GetPresetConfig(pluginInterface, i, out _);
                ILauncherApiMedia? apiMedia = presetConfig.get_LauncherApiMedia();

                if (apiMedia == null)
                    continue;

                long value = 0;
                logger.LogInformation("IPlugin->GetPresetConfig({PresetConfigIndex})->get_LauncherApiMedia()->InitAsync(): Invoking Asynchronously...", i);
                await apiMedia.InitAsync(in CancelToken, result => value = result).WaitFromHandle();
                logger.LogInformation("Return value: {ReturnValue}", value);

                using PluginDisposableMemory<LauncherPathEntry> backgroundPathMemory = PluginDisposableMemoryExtension.ToManagedSpan<LauncherPathEntry>(apiMedia.GetBackgroundEntries);
                int backgroundUrlCount = backgroundPathMemory.Length;

                logger.LogInformation("ILauncherApiMedia->GetBackgroundEntries(): Found {count} background handles at: 0x{address:x8}", backgroundUrlCount, backgroundPathMemory.AsSafePointer());

                string thisLocalPath = Path.Combine(Environment.CurrentDirectory, presetConfig.get_ProfileName());

                for (int j = 0; j < backgroundUrlCount; j++)
                {
                    using LauncherPathEntry entry = backgroundPathMemory[j];

                    string fileUrl = entry.GetPathString();
                    ArgumentException.ThrowIfNullOrEmpty(fileUrl);

                    logger.LogInformation("  LauncherPathEntry->GetStringFromHandle(): Downloading Background: {Url}", fileUrl);

                    string thisFileName = Path.GetFileName(fileUrl);
                    string thisFilePath = Path.Combine(thisLocalPath, thisFileName);

                    Directory.CreateDirectory(thisLocalPath);

                    await using FileStream fileStream = new(thisFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    await apiMedia.DownloadAssetAsync(entry, fileStream.SafeFileHandle.DangerousGetHandle(), (_, current, total) =>
                    {
                        Console.Write($"Downloaded: {current} / {total}...\r");
                    }, in CancelToken).WaitFromHandle();
                }

                using PluginDisposableMemory<LauncherPathEntry> logoPathMemory = PluginDisposableMemoryExtension.ToManagedSpan<LauncherPathEntry>(apiMedia.GetLogoOverlayEntries);
                int logoUrlCount = logoPathMemory.Length;

                logger.LogInformation("ILauncherApiMedia->GetLogoOverlayEntries(): Found {count} icon handles at: 0x{address:x8}", logoUrlCount, logoPathMemory.AsSafePointer());

                for (int j = 0; j < logoUrlCount; j++)
                {
                    using LauncherPathEntry entry = logoPathMemory[j];

                    string fileUrl = entry.GetPathString();
                    ArgumentException.ThrowIfNullOrEmpty(fileUrl);

                    logger.LogInformation("  LauncherPathEntry->GetStringFromHandle(): Downloading Icon: {Url}", fileUrl);

                    string thisFileName = Path.GetFileName(fileUrl);
                    string thisFilePath = Path.Combine(thisLocalPath, thisFileName);

                    Directory.CreateDirectory(thisLocalPath);

                    await using FileStream fileStream = new(thisFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    await apiMedia.DownloadAssetAsync(entry, fileStream.SafeFileHandle.DangerousGetHandle(), (_, current, total) =>
                    {
                        Console.Write($"Downloaded: {current} / {total}...\r");
                    }, in CancelToken).WaitFromHandle();
                }
            }
        }
    }
}
