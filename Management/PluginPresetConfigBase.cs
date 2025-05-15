using Hi3Helper.Plugin.Core.Utility;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.Marshalling;
using System.Threading;
using System.Threading.Tasks;

namespace Hi3Helper.Plugin.Core.Management
{
    /// <summary>
    /// This class is used to provide the abstract class of the plugin preset config.<br/>
    /// PLEASE Do NOT use this class or return this class directly. Instead, use this 
    /// class as a base class and override the properties to provide the preset config.
    /// </summary>
    [GeneratedComClass]
    public abstract partial class PluginPresetConfigBase : IPluginPresetConfig
    {
        public abstract string GameName { get; }
        public abstract string GameExecutableName { get; }
        public abstract string ProfileName { get; }
        public abstract string ZoneDescription { get; }
        public abstract string ZoneName { get; }
        public abstract string ZoneFullName { get; }
        public abstract string ZoneLogoUrl { get; }
        public abstract string ZonePosterUrl { get; }
        public abstract string ZoneHomePageUrl { get; }
        public abstract GameReleaseChannel ReleaseChannel { get; }
        public abstract string GameMainLanguage { get; }
        public abstract string LauncherGameDirectoryName { get; }
        public abstract List<string> SupportedLanguages { get; }

        #region Generic Read-only Properties Callbacks
        string IPluginPresetConfig.get_GameSupportedLanguages(int index)
        {
            if (index < 0 || index >= SupportedLanguages.Count)
            {
                return string.Empty;
            }

            return SupportedLanguages[index];
        }

        int IPluginPresetConfig.get_GameSupportedLanguagesCount() => SupportedLanguages.Count;
        string IPluginPresetConfig.get_GameExecutableName() => GameExecutableName;
        string IPluginPresetConfig.get_LauncherGameDirectoryName() => LauncherGameDirectoryName;
        string IPluginPresetConfig.get_GameName() => GameName;
        string IPluginPresetConfig.get_ProfileName() => ProfileName;
        string IPluginPresetConfig.get_ZoneDescription() => ZoneDescription;
        string IPluginPresetConfig.get_ZoneName() => ZoneName;
        string IPluginPresetConfig.get_ZoneFullName() => ZoneFullName;
        string IPluginPresetConfig.get_ZoneLogoUrl() => ZoneLogoUrl;
        string IPluginPresetConfig.get_ZonePosterUrl() => ZonePosterUrl;
        string IPluginPresetConfig.get_ZoneHomePageUrl() => ZoneHomePageUrl;
        GameReleaseChannel IPluginPresetConfig.get_ReleaseChannel() => ReleaseChannel;
        string IPluginPresetConfig.get_GameMainLanguage() => GameMainLanguage;
        #endregion

        public nint InitAsync(in Guid cancelToken, IPluginPresetConfig.InitAsyncIsSuccessCallback? isSuccessReturnCallback)
        {
            CancellationTokenSource tokenSource = ComCancellationTokenVault.RegisterToken(in cancelToken);
            CancellationToken token = tokenSource.Token;
            return InitAsync(token).AsHandle(result => isSuccessReturnCallback?.Invoke(result));
        }

        protected virtual async Task<int> InitAsync(CancellationToken token)
        {
            Console.WriteLine("Hello World from PluginPresetConfigBase->InitAsync()!");
            Console.WriteLine("If you see this message, that means you need to override this method in your own preset config class.");
            Console.WriteLine();
            Console.WriteLine("Moreover, this is a test method to ensure that the interop async is working as expected.");
            Console.WriteLine("Delaying for 10 seconds and you should've expected to get a return value of: 69420");
            Console.WriteLine("You can also try to cancel this method by passing the Guid Cancel token to IPlugin.CancelAsync() method.");
            try
            {
                await Task.Delay(10000, token);
                Console.WriteLine("Delay is done! Exiting method and returning: 69420...");
                return 69420;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine($"Delay is cancelled! Exiting method and returning: {int.MinValue}...");
                return int.MinValue;
            }
        }
    }
}
