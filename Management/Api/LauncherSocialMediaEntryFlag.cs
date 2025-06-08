using System;
using System.Buffers.Text;

namespace Hi3Helper.Plugin.Core.Management.Api;

/// <summary>
/// Defines kind of properties or flags that a social media entry can have.
/// </summary>
[Flags]
public enum LauncherSocialMediaEntryFlag
{
    /// <summary>
    /// The instance is untouched or has no data
    /// </summary>
    None,

    /// <summary>
    /// The icon is a UTF-8 path string.
    /// </summary>
    IconIsPath = 0b_00000001,

    /// <summary>
    /// The icon is an embedded data, encoded in <see cref="Base64Url"/> format.
    /// </summary>
    IconIsDataBuffer = 0b_00000010,

    /// <summary>
    /// The hover icon is a UTF-8 path string.
    /// </summary>
    IconHoverIsPath = 0b_00000100,

    /// <summary>
    /// The hover icon is an embedded data, encoded in <see cref="Base64Url"/> format.
    /// </summary>
    IconHoverIsDataBuffer = 0b_00001000,

    /// <summary>
    /// The QR code image is a UTF-8 path string.
    /// </summary>
    QrImageIsPath = 0b_00010000,

    /// <summary>
    /// The QR code image is an embedded data, encoded in <see cref="Base64Url"/> format.
    /// </summary>
    QrImageIsDataBuffer = 0b_00100000
}
