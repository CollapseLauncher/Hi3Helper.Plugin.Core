using System;
using System.Runtime.InteropServices.Marshalling;
using Hi3Helper.Plugin.Core.Management.PresetConfig;
using Hi3Helper.Plugin.Core.Utility;

namespace Hi3Helper.Plugin.Core;

/// <summary>
/// The base class for accessing plugin metadata and preset configuration.
/// </summary>
/// <remarks>
/// This <see cref="PluginBase"/> base class requires the abstract methods to be implemented, except for the <see cref="IPlugin.CancelAsync(in Guid)"/> method.
/// </remarks>
[GeneratedComClass]
public abstract partial class PluginBase : IPlugin
{
    public abstract string GetPluginName();
    public abstract string GetPluginDescription();
    public abstract string GetPluginAuthor();
    public abstract unsafe DateTime* GetPluginCreationDate();
    public abstract int GetPresetConfigCount();
    public abstract IPluginPresetConfig GetPresetConfig(int index);
    void IPlugin.CancelAsync(in Guid cancelToken)
    {
        // Cancel the async operation using the provided cancel token
        ComCancellationTokenVault.CancelToken(in cancelToken);
    }
}
