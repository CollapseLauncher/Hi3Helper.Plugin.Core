using Hi3Helper.Plugin.Core;
using Hi3Helper.Plugin.Core.Management.PresetConfig;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.InteropServices.Marshalling;

namespace PluginTest;

internal static partial class Test
{
    internal static unsafe IPlugin? GetPlugin(PluginGetPlugin delegateIn, out nint address)
    {
        void* pluginInterfaceAddress = delegateIn();
        address = (nint)pluginInterfaceAddress;
        return ComInterfaceMarshaller<IPlugin>.ConvertToManaged(pluginInterfaceAddress);
    }

    private static unsafe DateTime GetDateTime(IPlugin pluginInterface)
    {
        pluginInterface.GetPluginCreationDate(out DateTime* pluginCreationDate);
        return *pluginCreationDate;
    }

    private static unsafe IPluginPresetConfig GetPresetConfig(IPlugin pluginInterface, int index, out nint presetConfigAddress)
    {
        pluginInterface.GetPresetConfig(index, out IPluginPresetConfig presetConfig);
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
}
