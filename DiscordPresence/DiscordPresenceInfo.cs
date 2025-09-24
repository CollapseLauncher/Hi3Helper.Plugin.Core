using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Hi3Helper.Plugin.Core.Utility;

namespace Hi3Helper.Plugin.Core.DiscordPresence;

/// <summary>
/// Represents presence information for a Discord user, including logo URLs and tooltips for display in rich presence
/// integrations.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct DiscordPresenceInfo : IDisposable
{
    /// <summary>
    /// Represents the UID of the Discord Application ID.
    /// </summary>
    public ulong PresenceId; // 00 - 08

    /// <summary>
    /// Represents which logo to be used for large-icon display in Discord.<br/>
    /// The format of the URL can be just the name of the asset inside the Discord Application -> Rich Presence -> Art Assets menu or a full URL to an external logo (max: 256 chars).<br/>
    /// </summary>
    public ushort* LargeIconUrl; // 08 - 16

    /// <summary>
    /// The tooltip text to be displayed when hovering over the large-icon in Discord.
    /// </summary>
    public ushort* LargeIconTooltip; // 16 - 24

    /// <summary>
    /// Represents which logo to be used for small-icon display in Discord.<br/>
    /// The format of the URL can be just the name of the asset inside the Discord Application -> Rich Presence -> Art Assets menu or a full URL to an external logo (max: 256 chars).<br/>
    /// </summary>
    public ushort* SmallIconUrl; // 24 - 32

    /// <summary>
    /// The tooltip text to be displayed when hovering over the small-icon in Discord.
    /// </summary>
    public ushort* SmallIconTooltip; // 32 - 40

    /// <summary>
    /// Reserved for future use. Should be unused for now.
    /// </summary>
    public void* Reserved; // 40 - 48 (For future use)

    /// <inheritdoc cref="IDisposable.Dispose"/>
    public void Dispose()
    {
        Utf16StringMarshaller.Free(LargeIconUrl);
        Utf16StringMarshaller.Free(LargeIconTooltip);
        Utf16StringMarshaller.Free(SmallIconUrl);
        Utf16StringMarshaller.Free(SmallIconTooltip);

        if (Reserved != null)
        {
            Mem.Free(Reserved);
        }
    }
}