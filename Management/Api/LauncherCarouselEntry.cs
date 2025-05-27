using System;
using System.Runtime.InteropServices;
using Hi3Helper.Plugin.Core.Utility;

namespace Hi3Helper.Plugin.Core.Management.Api;

/// <summary>
/// Represents an entry in the launcher carousel, which includes a description, image URL, and click URL.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 8)]
public unsafe struct LauncherCarouselEntry()
    : IDisposable, IInitializableStruct
{
    public const int ExDescriptionMaxLength = 256;
    public const int ExImageUrlMaxLength    = 512;
    public const int ExClickUrlMaxLength    = 512;
    
    public byte  _isFreed     = 0;
    public byte* _description = null;
    public byte* _imageUrl    = null;
    public byte* _clickUrl    = null;

    /// <summary>
    /// Gets a span to the description of the carousel entry.
    /// </summary>
    public PluginDisposableMemory<byte> Description => new(_description, ExDescriptionMaxLength);

    /// <summary>
    /// Gets a span to the image URL of the carousel entry.
    /// </summary>
    public PluginDisposableMemory<byte> ImageUrl => new(_imageUrl, ExImageUrlMaxLength);

    /// <summary>
    /// Gets a span to the click URL of the carousel entry.
    /// </summary>
    public PluginDisposableMemory<byte> ClickUrl => new(_clickUrl, ExClickUrlMaxLength);

    public void InitInner()
    {
        _description = Mem.Alloc<byte>(ExDescriptionMaxLength);
        _imageUrl    = Mem.Alloc<byte>(ExImageUrlMaxLength);
        _clickUrl    = Mem.Alloc<byte>(ExClickUrlMaxLength);
    }

    public void Dispose()
    {
        if (_isFreed == 1) return;

        Mem.Free(_description);
        Mem.Free(_imageUrl);
        Mem.Free(_clickUrl);

        _isFreed = 1;
    }
}
