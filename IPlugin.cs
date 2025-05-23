using Hi3Helper.Plugin.Core.Management.PresetConfig;
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
public partial interface IPlugin
{
    /// <summary>
    /// Gets the name of the plugin or the game it represents.
    /// </summary>
    /// <returns>The name of the plugin or game.</returns>
    [return: MarshalAs(UnmanagedType.LPWStr)]
    string GetPluginName();

    /// <summary>
    /// Gets the description of the plugin.
    /// </summary>
    /// <returns>The description of the plugin or game.</returns>
    [return: MarshalAs(UnmanagedType.LPWStr)]
    string GetPluginDescription();

    /// <summary>
    /// Gets information about the author of the plugin.
    /// </summary>
    /// <returns>The author information.</returns>
    [return: MarshalAs(UnmanagedType.LPWStr)]
    string GetPluginAuthor();

    /// <summary>
    /// Gets the creation date of the plugin.
    /// </summary>
    /// <returns>A pointer to a <see cref="DateTime"/> instance representing the creation date.</returns>
    unsafe DateTime* GetPluginCreationDate();

    /// <summary>
    /// Gets the number of available <see cref="IPluginPresetConfig"/> instances.
    /// </summary>
    /// <returns>The count of preset configurations.</returns>
    [PreserveSig]
    int GetPresetConfigCount();

    /// <summary>
    /// Gets a specific <see cref="IPluginPresetConfig"/> instance by index.
    /// </summary>
    /// <param name="index">The index of the preset configuration.</param>
    /// <returns>The <see cref="IPluginPresetConfig"/> instance at the specified index.</returns>
    [return: MarshalAs(UnmanagedType.Interface)]
    IPluginPresetConfig GetPresetConfig(int index);

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
    /// <returns>Returns <c>true</c> if the settings are valid. Otherwise, returns <c>false</c></returns>
    [return: MarshalAs(UnmanagedType.Bool)]
    bool SetPluginProxySettings([MarshalAs(UnmanagedType.LPWStr)] string? hostUri = null,
                                [MarshalAs(UnmanagedType.LPWStr)] string? username = null,
                                [MarshalAs(UnmanagedType.LPWStr)] string? password = null);
}
