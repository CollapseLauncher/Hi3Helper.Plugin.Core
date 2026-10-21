using Hi3Helper.Plugin.Core.Management.PresetConfig;
using Hi3Helper.Plugin.Core.UI.Settings;
using Hi3Helper.Plugin.Core.Utility;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Hi3Helper.Plugin.Core;

public partial class SharedStaticV1Ext
{
    internal unsafe delegate HResult GetGameSettingsPageDelegate(nint presetConfig,
                                                                  out PluginDisposableMemoryMarshal pageJson);
    internal unsafe delegate HResult SetGameSettingValueDelegate(nint presetConfig,
                                                                  char* key, int keyLength,
                                                                  char* value, int valueLength);
    internal delegate HResult ApplyGameSettingsDelegate(nint presetConfig);
}

public partial class SharedStaticV1Ext<T>
{
    private static unsafe void InitExtension_Update6Exports()
    {
        TryRegisterApiExport<GetGameSettingsPageDelegate>("GetGameSettingsPage", GetGameSettingsPage);
        TryRegisterApiExport<SetGameSettingValueDelegate>("SetGameSettingValue", SetGameSettingValue);
        TryRegisterApiExport<ApplyGameSettingsDelegate>("ApplyGameSettings", ApplyGameSettings);
    }

    private static unsafe HResult GetGameSettingsPage(nint presetConfigP,
                                                       out PluginDisposableMemoryMarshal pageJson)
    {
        pageJson = PluginDisposableMemoryMarshal.Empty;
        try
        {
            IPluginPresetConfig presetConfig = GetPresetConfig(presetConfigP);
            GameSettingsPage? page = ThisExtensionExport.GetGameSettingsPageCore(presetConfig);
            if (page == null)
            {
                return HResult.False;
            }

            pageJson = GameSettingsPageSerializer.Serialize(page);
            return HResult.Ok;
        }
        catch (Exception ex)
        {
            InstanceLogger.LogError(ex, "An error occurred while retrieving the plugin game settings page");
            return Marshal.GetHRForException(ex);
        }
    }

    private static unsafe HResult SetGameSettingValue(nint presetConfigP,
                                                       char* key, int keyLength,
                                                       char* value, int valueLength)
    {
        try
        {
            IPluginPresetConfig presetConfig = GetPresetConfig(presetConfigP);
            string keyString = new(key, 0, keyLength);
            string valueString = new(value, 0, valueLength);
            ThisExtensionExport.SetGameSettingValueCore(presetConfig, keyString, valueString);
            return HResult.Ok;
        }
        catch (Exception ex)
        {
            InstanceLogger.LogError(ex, "An error occurred while updating plugin game setting {Key}",
                                    key == null ? null : new string(key, 0, keyLength));
            return Marshal.GetHRForException(ex);
        }
    }

    private static HResult ApplyGameSettings(nint presetConfigP)
    {
        try
        {
            ThisExtensionExport.ApplyGameSettingsCore(GetPresetConfig(presetConfigP));
            return HResult.Ok;
        }
        catch (Exception ex)
        {
            InstanceLogger.LogError(ex, "An error occurred while applying plugin game settings");
            return Marshal.GetHRForException(ex);
        }
    }

    private static unsafe IPluginPresetConfig GetPresetConfig(nint presetConfigP)
    {
        if (presetConfigP == nint.Zero)
        {
            throw new ArgumentNullException(nameof(presetConfigP));
        }

#if MANUALCOM
        return ComWrappers.ComInterfaceDispatch.GetInstance<IPluginPresetConfig>(
            (ComWrappers.ComInterfaceDispatch*)presetConfigP);
#else
        return ComInterfaceMarshaller<IPluginPresetConfig>.ConvertToManaged((void*)presetConfigP)
            ?? throw new InvalidCastException("Cannot convert the preset config pointer to IPluginPresetConfig");
#endif
    }

    /// <summary>
    /// Returns the declarative settings page for a game preset, or <c>null</c> when the preset has no settings page.
    /// </summary>
    protected virtual GameSettingsPage? GetGameSettingsPageCore(IPluginPresetConfig presetConfig) => null;

    /// <summary>
    /// Receives a value edited by the user. Values use invariant strings; conversion and validation belong to the plugin.
    /// </summary>
    protected virtual void SetGameSettingValueCore(IPluginPresetConfig presetConfig, string key, string value) { }

    /// <summary>
    /// Persists the edited settings for the specified game preset.
    /// </summary>
    protected virtual void ApplyGameSettingsCore(IPluginPresetConfig presetConfig) { }
}
