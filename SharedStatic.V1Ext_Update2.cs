using Hi3Helper.Plugin.Core.DiscordPresence;
using Hi3Helper.Plugin.Core.Management.PresetConfig;
using Hi3Helper.Plugin.Core.Utility;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Hi3Helper.Plugin.Core;

public partial class SharedStaticV1Ext<T>
{
    private static unsafe void InitExtension_Update2Exports()
    {
        /* ----------------------------------------------------------------------
         * Update 2 Feature Sets
         * ----------------------------------------------------------------------
         * This feature sets includes the following feature:
         *  - Discord Rich Presence
         *    - GetCurrentDiscordPresenceInfo
         */

        // -> Plugin Discord Presence Info based on Specific Game Region.
        TryRegisterApiExport<GetCurrentDiscordPresenceInfoDelegate>("GetCurrentDiscordPresenceInfo", GetCurrentDiscordPresenceInfo);
    }

    #region ABI Proxies
    /// <summary>
    /// This method is an ABI proxy function between the PInvoke Export and the actual plugin's method.<br/>
    /// See the documentation for <see cref="SharedStaticV1Ext{T}.GetCurrentDiscordPresenceInfoCore"/> method for more information.
    /// </summary>
    private static unsafe HResult GetCurrentDiscordPresenceInfo(void* presetConfigP, DiscordPresenceInfo** presenceInfoP)
    {
        try
        {
            if (presetConfigP == null)
            {
                throw new NullReferenceException("Cannot cast IPluginPresetConfig from the pointer, hence it gives null!");
            }

#if MANUALCOM
            IPluginPresetConfig? presetConfig = ComWrappers.ComInterfaceDispatch.GetInstance<IPluginPresetConfig>((ComWrappers.ComInterfaceDispatch*)presetConfigP);
#else
            IPluginPresetConfig? presetConfig = ComInterfaceMarshaller<IPluginPresetConfig>.ConvertToManaged((void*)presetConfigP);
#endif

            DiscordPresenceExtension.DiscordPresenceContext context = new(presetConfig);

            if (presenceInfoP == null)
            {
                return false;
            }

            bool isSupported = ThisExtensionExport
                .GetCurrentDiscordPresenceInfoCore(context,
                                                   out ulong presenceId,
                                                   out string? largeIconUrl,
                                                   out string? largeIconTooltip,
                                                   out string? smallIconUrl,
                                                   out string? smallIconTooltip);

            if (!isSupported)
            {
                return isSupported;
            }

            DiscordPresenceInfo* info = Mem.Alloc<DiscordPresenceInfo>();
            info->PresenceId       = presenceId;
            info->LargeIconUrl     = Utf16StringMarshaller.ConvertToUnmanaged(largeIconUrl);
            info->LargeIconTooltip = Utf16StringMarshaller.ConvertToUnmanaged(largeIconTooltip);
            info->SmallIconUrl     = Utf16StringMarshaller.ConvertToUnmanaged(smallIconUrl);
            info->SmallIconTooltip = Utf16StringMarshaller.ConvertToUnmanaged(smallIconTooltip);

            *presenceInfoP = info;

            return isSupported;
        }
        catch (Exception ex)
        {
            // ignored
            InstanceLogger.LogError(ex, "An error has occurred while trying to call GetCurrentDiscordPresenceInfo() from the plugin!");
            return Marshal.GetHRForException(ex);
        }
    }
    #endregion

    #region Core Methods
    /// <summary>
    /// Retrieves the current Discord Rich Presence information for the specified game region from its <paramref name="context"/>.
    /// </summary>
    /// <param name="context">The context containing details about the Discord presence request.</param>
    /// <param name="presenceId">Unique identifier for the Discord presence, if available.</param>
    /// <param name="largeIconUrl">URL of the large icon to display in the presence, or null if not set.</param>
    /// <param name="largeIconTooltip">Tooltip text for the large icon, or null if not set.</param>
    /// <param name="smallIconUrl">URL of the small icon to display in the presence, or null if not set.</param>
    /// <param name="smallIconTooltip">Tooltip text for the small icon, or null if not set.</param>
    /// <returns>
    /// Returns <c>false</c> if the plugin doesn't have game launch mechanism (or API Standard is equal or lower than v0.1.2) or if this method isn't overriden.<br/>
    /// Otherwise, <c>true</c> if the plugin supports game launch mechanism.
    /// </returns>
    protected virtual bool GetCurrentDiscordPresenceInfoCore(
        DiscordPresenceExtension.DiscordPresenceContext context,
        out ulong                                       presenceId,
        out string?                                     largeIconUrl,
        out string?                                     largeIconTooltip,
        out string?                                     smallIconUrl,
        out string?                                     smallIconTooltip)
    {
        presenceId = 0;
        Unsafe.SkipInit(out largeIconUrl);
        Unsafe.SkipInit(out largeIconTooltip);
        Unsafe.SkipInit(out smallIconUrl);
        Unsafe.SkipInit(out smallIconTooltip);
        return false;
    }
    #endregion
}
