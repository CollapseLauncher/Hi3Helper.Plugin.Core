using Hi3Helper.Plugin.Core.Management.PresetConfig;
using Hi3Helper.Plugin.Core.UI.Settings;
using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Hi3Helper.Plugin.Core.Utility;

/// <summary>
/// Provides launcher-side access to the optional v0.1.6 game settings exports.
/// </summary>
public static class GameSettingsExtension
{
    public sealed class GameSettingsContext
    {
        private readonly SharedStaticV1Ext.GetGameSettingsPageDelegate? _getPage;
        private readonly SharedStaticV1Ext.SetGameSettingValueDelegate? _setValue;
        private readonly SharedStaticV1Ext.ApplyGameSettingsDelegate? _apply;

        public IPluginPresetConfig PresetConfig { get; }
        public bool IsFeatureAvailable => _getPage != null && _setValue != null && _apply != null;
        public bool HasPage => TryGetPage(out _, out _);

        public GameSettingsContext(nint pluginHandle, IPluginPresetConfig presetConfig)
        {
            PresetConfig = presetConfig;
            pluginHandle.TryGetExport("GetGameSettingsPage", out SharedStaticV1Ext.GetGameSettingsPageDelegate getPage);
            pluginHandle.TryGetExport("SetGameSettingValue", out SharedStaticV1Ext.SetGameSettingValueDelegate setValue);
            pluginHandle.TryGetExport("ApplyGameSettings", out SharedStaticV1Ext.ApplyGameSettingsDelegate apply);
            _getPage = getPage;
            _setValue = setValue;
            _apply = apply;
        }

        public unsafe bool TryGetPage(out GameSettingsPage? page, out Exception? error)
        {
            page = null;
            error = null;
            if (!IsFeatureAvailable)
            {
                return false;
            }

            nint presetConfigP = (nint)ComInterfaceMarshaller<IPluginPresetConfig>.ConvertToUnmanaged(PresetConfig);
            try
            {
                int hResult = _getPage!(presetConfigP, out PluginDisposableMemoryMarshal pageJson);
                using var memory = pageJson.ToManagedSpan<byte>();
                if (hResult != 0)
                {
                    error = Marshal.GetExceptionForHR(hResult);
                    return false;
                }

                string? json = pageJson.Handle == 0 || pageJson.Length <= 0 ? null : memory;
                if (string.IsNullOrWhiteSpace(json))
                {
                    return false;
                }

                page = GameSettingsPageSerializer.Deserialize(json);
                return page != null;
            }
            catch (Exception ex)
            {
                error = ex;
                return false;
            }
            finally
            {
                Marshal.Release(presetConfigP);
            }
        }

        public unsafe void SetValue(string key, string value)
        {
            if (_setValue == null)
            {
                throw new NotSupportedException("The plugin does not expose game settings");
            }

            nint presetConfigP = (nint)ComInterfaceMarshaller<IPluginPresetConfig>.ConvertToUnmanaged(PresetConfig);
            try
            {
                fixed (char* keyP = key)
                fixed (char* valueP = value)
                {
                    int hResult = _setValue(presetConfigP, keyP, key.Length, valueP, value.Length);
                    Marshal.ThrowExceptionForHR(hResult);
                }
            }
            finally
            {
                Marshal.Release(presetConfigP);
            }
        }

        public unsafe void Apply()
        {
            if (_apply == null)
            {
                throw new NotSupportedException("The plugin does not expose game settings");
            }

            nint presetConfigP = (nint)ComInterfaceMarshaller<IPluginPresetConfig>.ConvertToUnmanaged(PresetConfig);
            try
            {
                Marshal.ThrowExceptionForHR(_apply(presetConfigP));
            }
            finally
            {
                Marshal.Release(presetConfigP);
            }
        }
    }
}
