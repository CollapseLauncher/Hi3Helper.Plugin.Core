using Hi3Helper.Plugin.Core.Management.PresetConfig;
using Hi3Helper.Plugin.Core.Utility;
using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using static Hi3Helper.Plugin.Core.SharedStaticV1Ext;

namespace Hi3Helper.Plugin.Core.DiscordPresence;

/// <summary>
/// This extension provides a method to get the Discord Presence information for the current game region.
/// </summary>
/// <remarks>
/// This extension IS ONLY SUPPOSEDLY BE USED by the launcher, NOT by the plugin.
/// </remarks>
public static class DiscordPresenceExtension
{
    /// <summary>
    /// A context used to manage the context of Discord Presence information.
    /// </summary>
    public unsafe class DiscordPresenceContext : IDisposable
    {
        // Fields
        private DiscordPresenceInfo* _data;

        internal DiscordPresenceContext(IPluginPresetConfig presetConfig) : this(0, presetConfig)
        {
        }

        public DiscordPresenceContext(nint pluginHandle, IPluginPresetConfig presetConfig)
        {
            if (pluginHandle == nint.Zero)
            {
                return;
            }

            if (!pluginHandle.TryGetExport("GetCurrentDiscordPresenceInfo",
                                           out GetCurrentDiscordPresenceInfoDelegate method))
            {
                return;
            }

            DiscordPresenceInfo** info = null;

            void* ptr = ComInterfaceMarshaller<IPluginPresetConfig>.ConvertToUnmanaged(presetConfig);
            HResult result = method(ptr, info);

            if (result == HResult.Ok && info != null && *info != null)
            {
                _data = *info;
            }
        }

        /// <summary>
        /// Indicates whether the Discord Presence feature is available.
        /// </summary>
        public bool IsFeatureAvailable => _data != null;

        /// <inheritdoc cref="DiscordPresenceInfo.PresenceId"/>
        public ulong PresenceId => _data != null ? _data->PresenceId : 0;

        /// <inheritdoc cref="DiscordPresenceInfo.LargeIconUrl"/>
        public string? LargeIconUrl
        {
            get
            {
                if (_data == null || _data->LargeIconUrl == null)
                {
                    return null;
                }

                return field ??= Utf16StringMarshaller.ConvertToManaged(_data->LargeIconUrl);
            }
        }

        /// <inheritdoc cref="DiscordPresenceInfo.LargeIconTooltip"/>
        public string? LargeIconTooltip
        {
            get
            {
                if (_data == null || _data->LargeIconTooltip == null)
                {
                    return null;
                }

                return field ??= Utf16StringMarshaller.ConvertToManaged(_data->LargeIconTooltip);
            }
        }

        /// <inheritdoc cref="DiscordPresenceInfo.SmallIconUrl"/>
        public string? SmallIconUrl
        {
            get
            {
                if (_data == null || _data->SmallIconUrl == null)
                {
                    return null;
                }

                return field ??= Utf16StringMarshaller.ConvertToManaged(_data->SmallIconUrl);
            }
        }

        /// <inheritdoc cref="DiscordPresenceInfo.SmallIconTooltip"/>
        public string? SmallIconTooltip
        {
            get
            {
                if (_data == null || _data->SmallIconTooltip == null)
                {
                    return null;
                }

                return field ??= Utf16StringMarshaller.ConvertToManaged(_data->SmallIconTooltip);
            }
        }

        /// <inheritdoc cref="IDisposable.Dispose"/>
        public void Dispose()
        {
            if (_data == null)
            {
                return;
            }

            _data->Dispose();
            NativeMemory.Free(_data);

            _data = null;

            GC.SuppressFinalize(this);
        }
    }
}
