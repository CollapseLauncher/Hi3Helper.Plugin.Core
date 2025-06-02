using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Hi3Helper.Plugin.Core.Utility;

namespace Hi3Helper.Plugin.Core.Management.Api;

/// <summary>
/// Entry of the launcher's news data.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 8)]
public unsafe struct LauncherNewsEntry(LauncherNewsEntryType newsType)
    : IDisposable, IInitializableStruct
{
    public const int ExTitleMaxLength       = 128;
    public const int ExDescriptionMaxLength = 256;
    public const int ExUrlMaxLength         = 512;
    public const int ExPostDateLength       = 32;

    public void InitInner()
    {
        _title       = Mem.Alloc<byte>(ExTitleMaxLength);
        _description = Mem.Alloc<byte>(ExDescriptionMaxLength);
        _url         = Mem.Alloc<byte>(ExUrlMaxLength);
        _postDate    = Mem.Alloc<byte>(ExPostDateLength);
    }
    
    private byte _isFreed = 0;

    /// <summary>
    /// The type of the news entry. See <see cref="LauncherNewsEntryType"/> for the types.
    /// </summary>
    public readonly LauncherNewsEntryType Type = newsType;

    private byte* _title       = null;
    private byte* _description = null;
    private byte* _url         = null;
    private byte* _postDate    = null;

    /// <summary>
    /// The title of the news entry.
    /// </summary>
    public PluginDisposableMemory<byte> Title => new(_title, ExTitleMaxLength);

    /// <summary>
    /// The description of the news entry.
    /// </summary>
    public PluginDisposableMemory<byte> Description => new(_title, ExTitleMaxLength);

    /// <summary>
    /// The HREF/click URL of the news entry.
    /// </summary>
    public PluginDisposableMemory<byte> Url => new(_title, ExTitleMaxLength);

    /// <summary>
    /// The short format (DD/MM) of the date for the news entry.
    /// </summary>
    public PluginDisposableMemory<byte> PostDate => new(_title, ExTitleMaxLength);

    /// <summary>
    /// Get the string of <see cref="Title"/> field.
    /// </summary>
    /// <returns>The string of <see cref="Title"/> field.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetTitleString() => Title.CreateStringFromNullTerminated();

    /// <summary>
    /// Get the string of <see cref="Description"/> field.
    /// </summary>
    /// <returns>The string of <see cref="Description"/> field.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetDescriptionString() => Description.CreateStringFromNullTerminated();

    /// <summary>
    /// Get the string of <see cref="Url"/> field.
    /// </summary>
    /// <param name="handle">The handle to <see cref="LauncherNewsEntry"/> struct.</param>
    /// <returns>The string of <see cref="Url"/> field.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string GetUrlString(nint handle) => Url.CreateStringFromNullTerminated();

    /// <summary>
    /// Get the string of <see cref="PostDate"/> field.
    /// </summary>
    /// <returns>The string of <see cref="PostDate"/> field.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public string? GetPostDateString() => PostDate.CreateStringFromNullTerminated();

    public void Dispose()
    {
        if (_isFreed == 1) return;

        Mem.Free(_title);
        Mem.Free(_description);
        Mem.Free(_url);
        Mem.Free(_postDate);

        _isFreed = 1;
    }
}
