using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Hi3Helper.Plugin.Core.Utility;

namespace Hi3Helper.Plugin.Core.Management.Api;

/// <summary>
/// Entry of the launcher's news data.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct LauncherNewsEntry(LauncherNewsEntryType newsType)
    : IDisposable, IInitializableStruct
{
    public const int ExTitleMaxLength       = 128; // 256 bytes
    public const int ExDescriptionMaxLength = 256; // 512 bytes
    public const int ExUrlMaxLength         = 512; // 1024 bytes

    public void InitInner()
    {
        _title       = Mem.Alloc<char>(ExTitleMaxLength);
        _description = Mem.Alloc<char>(ExDescriptionMaxLength);
        _url         = Mem.Alloc<char>(ExUrlMaxLength);
    }

    private char* _title       = null;
    private char* _description = null;
    private char* _url         = null;
    private int   _isFreed     = 0;

    /// <summary>
    /// The type of the news entry. See <see cref="LauncherNewsEntryType"/> for the types.
    /// </summary>
    public readonly LauncherNewsEntryType Type = newsType;

    /// <summary>
    /// The title of the news entry.
    /// </summary>
    public PluginDisposableMemory<char> Title => new(_title, ExTitleMaxLength);

    /// <summary>
    /// The description of the news entry.
    /// </summary>
    public PluginDisposableMemory<char> Description => new(_title, ExTitleMaxLength);

    /// <summary>
    /// The HREF/click URL of the news entry.
    /// </summary>
    public PluginDisposableMemory<char> Url => new(_title, ExTitleMaxLength);

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

    public void Dispose()
    {
        if (_isFreed == 1) return;

        Mem.Free(_title);
        Mem.Free(_description);
        Mem.Free(_url);

        _isFreed = 1;
    }
}
