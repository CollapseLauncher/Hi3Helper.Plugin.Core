using Hi3Helper.Plugin.Core.Management;
using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Hi3Helper.Plugin.Core;

[GeneratedComInterface]
[Guid(ComInterfaceId.Plugin)]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public partial interface IPlugin
{
    /// <summary>
    /// Get the name of the plugin or game that it represents.
    /// </summary>
    /// <returns>Name of the plugin or game represented</returns>
    [return: MarshalAs(UnmanagedType.LPWStr)]
    string GetPluginName();

    /// <summary>
    /// Get the description of the plugin.
    /// </summary>
    /// <returns>The description of the plugin or game represented</returns>
    [return: MarshalAs(UnmanagedType.LPWStr)]
    string GetPluginDescription();

    /// <summary>
    /// Get the information about the author of the plugin.
    /// </summary>
    /// <returns>The author information of the plugin</returns>
    [return: MarshalAs(UnmanagedType.LPWStr)]
    string GetPluginAuthor();

    /// <summary>
    /// Get the information of the plugin creation date.
    /// </summary>
    /// <returns>A pointer to <see cref="DateTime"/> instance</returns>
    unsafe DateTime* GetPluginCreationDate();

    /// <summary>
    /// Get the count of <see cref="IPluginPresetConfig"/> instances.
    /// </summary>
    /// <returns>Count of the available <see cref="IPluginPresetConfig"/></returns>
    [PreserveSig]
    int GetPresetConfigCount();

    /// <summary>
    /// This function will return a pointer to the <see cref="IPluginPresetConfig"/> object. As the method returns a pointer,
    /// the main launcher requires to marshal the pointer into respective version of the <see cref="IPluginPresetConfig"/>.
    /// </summary>
    /// <param name="index">Get the index of the <see cref="IPluginPresetConfig"/> instance</param>
    /// <returns>The address of the <see cref="IPluginPresetConfig"/> instance</returns>
    [return: MarshalAs(UnmanagedType.Interface)]
    IPluginPresetConfig GetPresetConfig(int index);

    /// <summary>
    /// This function will cancel the async operation that is currently running.
    /// </summary>
    /// <param name="cancelToken">The token of the currently running task.</param>
    void CancelAsync(in Guid cancelToken);
}
