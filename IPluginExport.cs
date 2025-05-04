using Hi3Helper.Plugin.Core.Management;

namespace Hi3Helper.Plugin.Core;

public unsafe interface IPlugin
{
    /// <summary>
    /// Get the name of the plugin or game that it represents.
    /// </summary>
    /// <returns>Name of the plugin or game represented</returns>
    public string GetPluginName();

    /// <summary>
    /// Get the description of the plugin.
    /// </summary>
    /// <returns>The description of the plugin or game represented</returns>
    public string GetPluginDescription();

    /// <summary>
    /// Get the count of <see cref="IPluginPresetConfig"/> instances.
    /// </summary>
    /// <returns>Count of the available <see cref="IPluginPresetConfig"/></returns>
    public int GetPresetConfigCount();

    /// <summary>
    /// This function will return a pointer to the <see cref="IPluginPresetConfig"/> object. As the method returns a pointer,
    /// the main launcher requires to marshal the pointer into respective version of the <see cref="IPluginPresetConfig"/>.
    /// </summary>
    /// <param name="index">Get the index of the <see cref="IPluginPresetConfig"/> instance</param>
    /// <returns>The address of the <see cref="IPluginPresetConfig"/> instance</returns>
    public void* GetPresetConfig(int index);
}
