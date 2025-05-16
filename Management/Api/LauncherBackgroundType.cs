using System;
// ReSharper disable CommentTypo

namespace Hi3Helper.Plugin.Core.Management.Api;

/// <summary>
/// Flags representing the type and source of a launcher background.
/// </summary>
[Flags]
public enum LauncherBackgroundFlag
{
    /// <summary>
    /// No background type or source specified.
    /// </summary>
    None = 0b_0000_0000,

    /// <summary>
    /// The background is a single image.
    /// </summary>
    TypeIsImage = 0b_0000_0001,

    /// <summary>
    /// The background is an image sequence (used by some games, e.g., Wuthering Waves).
    /// </summary>
    TypeIsImageSequence = 0b_0000_0010,

    /// <summary>
    /// The background is a video.
    /// </summary>
    TypeIsVideo = 0b_0000_0100,

    /// <summary>
    /// The background source is a file.
    /// </summary>
    IsSourceFile = 0b_0001_0000,

    /// <summary>
    /// The background source is a zip archive (e.g., image sequences stored in .zip for Wuthering Waves).
    /// </summary>
    IsSourceZip = 0b_0010_0000
}
