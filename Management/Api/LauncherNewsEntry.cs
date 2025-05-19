using System.Runtime.InteropServices;

namespace Hi3Helper.Plugin.Core.Management.Api;

/// <summary>
/// Entry of the launcher's news data.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct LauncherNewsEntry
{
    public const int TitleMaxLength       = 256;
    public const int DescriptionMaxLength = 512;
    public const int ClickUrlMaxLength    = 254;

    /// <summary>
    /// The title of the news entry.
    /// </summary>
    public fixed char Title[TitleMaxLength]; // Size: 256 chars -> 512 bytes -> Offset: 0 byte

    /// <summary>
    /// The description of the news entry.
    /// </summary>
    public fixed char Description[DescriptionMaxLength]; // Size: 512 chars -> 1024 bytes -> Offset: 1536 bytes

    /// <summary>
    /// The HREF/click URL of the news entry.
    /// </summary>
    public fixed char ClickUrl[ClickUrlMaxLength]; // Size: 254 chars -> 508 bytes -> Offset: 2044 bytes

    /// <summary>
    /// The type of the news entry. See <see cref="LauncherNewsEntryType"/> for the types.
    /// </summary>
    public LauncherNewsEntryType Type; // Size: 4 bytes -> Offset: 2048 bytes
}
