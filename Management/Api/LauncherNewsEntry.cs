using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Hi3Helper.Plugin.Core.Utility;

namespace Hi3Helper.Plugin.Core.Management.Api;

/// <summary>
/// Entry of the launcher's news data.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct LauncherNewsEntry : IChainedEntry
{
    public const int TitleMaxLength       = 128; // 256 bytes
    public const int DescriptionMaxLength = 238; // 476 bytes
    public const int UrlMaxLength         = 140; // 280 bytes

    /// <summary>
    /// The title of the news entry.
    /// </summary>
    public fixed char Title[TitleMaxLength];

    /// <summary>
    /// The description of the news entry.
    /// </summary>
    public fixed char Description[DescriptionMaxLength];

    /// <summary>
    /// The HREF/click URL of the news entry.
    /// </summary>
    public fixed char Url[UrlMaxLength];

    /// <summary>
    /// The type of the news entry. See <see cref="LauncherNewsEntryType"/> for the types.
    /// </summary>
    public LauncherNewsEntryType Type;

    private nint _nextEntry;
    /// <summary>
    /// The next entry of the news entry. This should be non-null if multiple entries are available.
    /// </summary>
    // ReSharper disable once ConvertToAutoProperty
    public nint NextEntry { get => _nextEntry; set => _nextEntry = value; }

    /// <summary>
    /// Get the string of <see cref="Title"/> field.
    /// </summary>
    /// <param name="handle">The handle to <see cref="LauncherNewsEntry"/> struct.</param>
    /// <returns>The string of <see cref="Title"/> field.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetTitleString(nint handle)
        => Mem.CreateStringFromNullTerminated(handle.AsPointer<LauncherNewsEntry>()->Title);

    /// <summary>
    /// Get the string of <see cref="Description"/> field.
    /// </summary>
    /// <param name="handle">The handle to <see cref="LauncherNewsEntry"/> struct.</param>
    /// <returns>The string of <see cref="Description"/> field.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetDescriptionString(nint handle)
        => Mem.CreateStringFromNullTerminated(handle.AsPointer<LauncherNewsEntry>()->Description);

    /// <summary>
    /// Get the string of <see cref="Url"/> field.
    /// </summary>
    /// <param name="handle">The handle to <see cref="LauncherNewsEntry"/> struct.</param>
    /// <returns>The string of <see cref="Url"/> field.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetUrlString(nint handle)
        => Mem.CreateStringFromNullTerminated(handle.AsPointer<LauncherNewsEntry>()->Url);

    public void Dispose() => Mem.Free(this.AsPointer());
}
