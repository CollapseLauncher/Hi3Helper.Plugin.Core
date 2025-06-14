using Hi3Helper.Plugin.Core;
using Hi3Helper.Plugin.Core.Management.Api;
using Hi3Helper.Plugin.Core.Management.PresetConfig;
using Hi3Helper.Plugin.Core.Utility;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace PluginTest;

internal static partial class Test
{
    internal static async Task TestApiMedia(PluginGetPlugin delegateIn, ILogger logger)
    {
        PluginInterface ??= GetPlugin(delegateIn, out _);
        ThrowIfPluginIsNull(PluginInterface);

        int pluginPresetConfigCount = PluginInterface.GetPresetConfigCount();
        for (int i = 0; i < pluginPresetConfigCount; i++)
        {
            IPluginPresetConfig presetConfig = GetPresetConfig(PluginInterface, i, out _);

            await InnerStartBackgroundImageEntryTest(logger, presetConfig, i);
            await InnerStartMediaSocMedEntryTest(logger, presetConfig, i, false);
        }
    }

    private static async Task InnerStartBackgroundImageEntryTest(ILogger logger, IPluginPresetConfig presetConfig, int presetConfigIndex)
    {
        ILauncherApiMedia? apiMedia = presetConfig.comGet_LauncherApiMedia();

        if (apiMedia == null)
            return;

        logger.LogInformation("IPlugin->GetPresetConfig({PresetConfigIndex})->comGet_LauncherApiMedia()->InitAsync(): Invoking Asynchronously...", presetConfigIndex);
        int value = await apiMedia.InitAsync(in CancelToken).WaitFromHandle<int>();
        logger.LogInformation("Return value: {ReturnValue}", value);

        using PluginDisposableMemory<LauncherPathEntry> backgroundPathMemory = PluginDisposableMemoryExtension.ToManagedSpan<LauncherPathEntry>(apiMedia.GetBackgroundEntries);
        int backgroundUrlCount = backgroundPathMemory.Length;

        logger.LogInformation("ILauncherApiMedia->GetBackgroundEntries(): Found {count} background handles at: 0x{address:x8}", backgroundUrlCount, backgroundPathMemory.AsSafePointer());

        string thisLocalPath = Path.Combine(Environment.CurrentDirectory, presetConfig.comGet_ProfileName());

        for (int j = 0; j < backgroundUrlCount; j++)
        {
            using LauncherPathEntry entry = backgroundPathMemory[j];

            string? fileUrl = entry.Path;
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

            string? fileUrl = entry.Path;
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

    private static async Task InnerStartMediaSocMedEntryTest(ILogger logger, IPluginPresetConfig presetConfig, int presetConfigIndex, bool isChild)
    {
        ILauncherApiNews? apiNews = presetConfig.comGet_LauncherApiNews();

        if (apiNews == null)
            return;

        logger.LogInformation("IPlugin->GetPresetConfig({PresetConfigIndex})->comGet_LauncherApiNews()->InitAsync(): Invoking Asynchronously...", presetConfigIndex);
        int value = await apiNews.InitAsync(in CancelToken).WaitFromHandle<int>();
        logger.LogInformation("Return value: {ReturnValue}", value);

        using PluginDisposableMemory<LauncherSocialMediaEntry> socialMediaSpan = PluginDisposableMemoryExtension.ToManagedSpan<LauncherSocialMediaEntry>(apiNews.GetSocialMediaEntries);

        int socialMediaCount = socialMediaSpan.Length;
        for (int j = 0; j < socialMediaCount; j++)
        {
            using LauncherSocialMediaEntry entry = socialMediaSpan[j];
            InnerStartMediaSocMedChildEntryTest(logger, isChild, entry);
        }
    }

    private static void InnerStartMediaSocMedChildEntryTest(ILogger logger, bool isChild, LauncherSocialMediaEntry entry)
    {
        string? socialMediaName = entry.Description;
        string? socialMediaClickUrl = entry.ClickUrl;
        LauncherSocialMediaEntryFlag socialMediaFlag = entry.Flags;

        logger.LogInformation("  LauncherSocialMediaEntry->SocialMediaDescription: {Str}", socialMediaName);
        logger.LogInformation("  LauncherSocialMediaEntry->SocialMediaClickUrl: {Str}", socialMediaClickUrl);
        logger.LogInformation("  LauncherSocialMediaEntry->Flags: {Str}", socialMediaFlag);

        string? iconDataContent = entry.IconPath;
        string? iconHoverDataContent = entry.IconHoverPath;

        if (string.IsNullOrEmpty(iconDataContent) && !isChild)
        {
            throw new InvalidOperationException($"Social media entry: {socialMediaName} ({socialMediaClickUrl}) doesn't have icon data!");
        }

        logger.LogInformation("  LauncherSocialMediaEntry->IconPath: Length: {Content}", iconDataContent);

        if (!string.IsNullOrEmpty(iconHoverDataContent))
        {
            logger.LogInformation("  LauncherSocialMediaEntry->IconHoverPath: {Content}", iconHoverDataContent);
        }

        string? qrDataContent = entry.QrPath;

        if (!string.IsNullOrEmpty(qrDataContent))
        {
            logger.LogInformation("  LauncherSocialMediaEntry->QrPath: {Content}", qrDataContent);
        }

        if (Unsafe.IsNullRef(ref entry.ChildEntryHandle))
        {
            return;
        }

        using LauncherSocialMediaEntry child = entry.ChildEntryHandle;
        InnerStartMediaSocMedChildEntryTest(logger, true, child);
    }
}
