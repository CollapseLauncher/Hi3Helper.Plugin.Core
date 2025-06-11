using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
// ReSharper disable UnusedMember.Global

namespace Hi3Helper.Plugin.Core.Management.Api;

/// <summary>
/// Represents an entry in the launcher carousel, which includes a description, image URL, and click URL.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 8)]
public unsafe struct LauncherCarouselEntry()
    : IDisposable
{
    public byte  _isFreed     = 0;
    public byte* _description = null;
    public byte* _imageUrl    = null;
    public byte* _clickUrl    = null;

    /// <summary>
    /// Gets a string to the description of the carousel entry.
    /// </summary>
    public readonly string? Description => Utf8StringMarshaller.ConvertToManaged(_description);

    /// <summary>
    /// Gets a span to the image URL of the carousel entry.
    /// </summary>
    public readonly string? ImageUrl => Utf8StringMarshaller.ConvertToManaged(_imageUrl);

    /// <summary>
    /// Gets a span to the click URL of the carousel entry.
    /// </summary>
    public readonly string? ClickUrl => Utf8StringMarshaller.ConvertToManaged(_clickUrl);

    /// <summary>
    /// Write the strings into the current struct.
    /// </summary>
    /// <param name="description">A string to the description of the carousel entry.</param>
    /// <param name="imageUrl">A span to the image URL of the carousel entry.</param>
    /// <param name="clickUrl">A span to the click URL of the carousel entry.</param>
    public void Write(string? description, string? imageUrl, string? clickUrl)
    {
        Utf8StringMarshaller.Free(_description);
        Utf8StringMarshaller.Free(_imageUrl);
        Utf8StringMarshaller.Free(_clickUrl);

        _description = Utf8StringMarshaller.ConvertToUnmanaged(description);
        _imageUrl    = Utf8StringMarshaller.ConvertToUnmanaged(imageUrl);
        _clickUrl    = Utf8StringMarshaller.ConvertToUnmanaged(clickUrl);
    }

    public void Dispose()
    {
        if (_isFreed == 1) return;

        Utf8StringMarshaller.Free(_description);
        Utf8StringMarshaller.Free(_imageUrl);
        Utf8StringMarshaller.Free(_clickUrl);

        _isFreed = 1;
    }
}
