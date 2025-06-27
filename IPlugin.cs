using Hi3Helper.Plugin.Core.Management.PresetConfig;
using Hi3Helper.Plugin.Core.Update;
using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
// ReSharper disable InconsistentNaming

namespace Hi3Helper.Plugin.Core;

/// <summary>
/// The plugin interface for accessing plugin metadata and preset configuration.
/// </summary>
/// <remarks>
/// This <see cref="IPlugin"/> interface provides information about the plugin and the <see cref="IPluginPresetConfig"/> interface to access the preset configuration.
/// </remarks>
[GeneratedComInterface]
[Guid(ComInterfaceId.ExPlugin)]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public partial interface IPlugin : IFree, IDisposable
{
    /// <summary>
    /// Gets the name of the plugin or the game it represents.
    /// </summary>
    /// <param name="result">The name of the plugin or game.</param>
    void GetPluginName([MarshalAs(UnmanagedType.LPWStr)] out string? result);

    /// <summary>
    /// Gets the description of the plugin.
    /// </summary>
    /// <param name="result">The description of the plugin or game.</param>
    void GetPluginDescription([MarshalAs(UnmanagedType.LPWStr)] out string? result);

    /// <summary>
    /// Gets information about the author of the plugin.
    /// </summary>
    /// <param name="result">The author information.</param>
    void GetPluginAuthor([MarshalAs(UnmanagedType.LPWStr)] out string? result);

    /// <summary>
    /// Gets the creation date of the plugin.
    /// </summary>
    /// <param name="result">A pointer to a <see cref="DateTime"/> instance representing the creation date (Equivalent to <c>DateTime** result</c>).</param>
    unsafe void GetPluginCreationDate(out DateTime* result);

    /// <summary>
    /// Gets the number of available <see cref="IPluginPresetConfig"/> instances.
    /// </summary>
    /// <param name="count">The count of how much Plugin Preset Configs are available.</param>
    void GetPresetConfigCount(out int count);

    /// <summary>
    /// Gets the app icon URL of the plugin.
    /// </summary>
    /// <param name="result">The icon URL of the plugin.</param>
    void GetPluginAppIconUrl([MarshalAs(UnmanagedType.LPWStr)] out string? result);

    /// <summary>
    /// Gets the notification poster URL of the plugin.
    /// </summary>
    /// <param name="result">The notification poster URL of the plugin.</param>
    void GetNotificationPosterUrl([MarshalAs(UnmanagedType.LPWStr)] out string? result);

    /// <summary>
    /// Gets a specific <see cref="IPluginPresetConfig"/> instance by index.
    /// </summary>
    /// <param name="index">The index of the preset configuration.</param>
    /// <param name="presetConfig">The <see cref="IPluginPresetConfig"/> instance at the specified index.</param>
    void GetPresetConfig(int index, [MarshalAs(UnmanagedType.Interface)] out IPluginPresetConfig presetConfig);

    /// <summary>
    /// Cancels an asynchronous operation associated with the specified cancellation token.
    /// </summary>
    /// <param name="cancelToken">The token identifying the operation to cancel.</param>
    void CancelAsync(in Guid cancelToken);

    /// <summary>
    /// Sets the proxy settings used by the plugin. To reset the configuration, set all arguments as <c>null</c>
    /// </summary>
    /// <param name="hostUri">
    /// The host URI of the proxy server.<br/>
    /// Only few Proxy protocol supported by the plugin, including: http://, https://, socks4:// and socks5://
    /// </param>
    /// <param name="username">The username for proxy authentication. Leave it as <c>null</c> if none is needed.</param>
    /// <param name="password">The password for proxy authentication. Leave it as <c>null</c> if none is needed.</param>
    /// <param name="isSuccess">Returns <c>true</c> if the settings are valid. Otherwise, returns <c>false</c>.</param>
    void SetPluginProxySettings([MarshalAs(UnmanagedType.LPWStr)] string? hostUri,
                                [MarshalAs(UnmanagedType.LPWStr)] string? username,
                                [MarshalAs(UnmanagedType.LPWStr)] string? password,
                                [MarshalAs(UnmanagedType.Bool)]   out bool isSuccess);

    /// <summary>
    /// Set the locale ID for the plugin.
    /// </summary>
    /// <param name="localeId">The locale ID to be set (for example: en-US)</param>
    void SetPluginLocaleId([MarshalAs(UnmanagedType.LPWStr)] string? localeId);

    /// <summary>
    /// Gets the plugin self-updater instance.
    /// </summary>
    /// <param name="selfUpdate">An instance to <see cref="IPluginSelfUpdate"/>.</param>
    void GetPluginSelfUpdater([MarshalAs(UnmanagedType.Interface)] out IPluginSelfUpdate? selfUpdate);
}
