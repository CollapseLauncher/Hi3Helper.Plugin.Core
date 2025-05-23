using System;

namespace Hi3Helper.Plugin.Core.Management.Api;

/// <summary>
/// Defines the kind of properties or flags that a social media entry can have.
/// </summary>
[Flags]
public enum LauncherSocialMediaEntryFlag : byte
{
    /// <summary>
    /// The icon is a <see cref="PluginDisposableMemory{T}"/> of <see cref="char"/> to the URL or Local path string.
    /// </summary>
    IconIsPath = 0b_00000001,

    /// <summary>
    /// The icon is embedded as a <see cref="PluginDisposableMemory{T}"/> of <see cref="byte"/>
    /// </summary>
    IconIsDataBuffer = 0b_00000010,

    /// <summary>
    /// The QR code image is a <see cref="PluginDisposableMemory{T}"/> of <see cref="char"/> to the URL or Local path string.
    /// </summary>
    QrImageIsPath = 0b_00000100,

    /// <summary>
    /// The QR code image is embedded as a <see cref="PluginDisposableMemory{T}"/> of <see cref="byte"/>
    /// </summary>
    QrImageIsDataBuffer = 0b_00001000,

    /// <summary>
    /// Whether the social media entry has a QR code image.
    /// </summary>
    HasQrImage = 0b_10000000,

    /// <summary>
    /// Whether the social media entry has a title or description of the social media.
    /// </summary>
    HasDescription = 0b_01000000,

    /// <summary>
    /// Whether the social media entry has a child entry.
    /// </summary>
    HasChild = 0b_00100000,

    /// <summary>
    /// Whether the social media entry has a HREF/Click URL.
    /// </summary>
    HasClickUrl = 0b_00010000
}
