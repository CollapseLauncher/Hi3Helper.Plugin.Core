using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Hi3Helper.Plugin.Core.Management.Api;

/// <summary>
/// Entry of the launcher's news data.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 8)]
public unsafe struct LauncherNewsEntry(LauncherNewsEntryType newsType)
    : IDisposable
{
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
    public string? Title => Utf8StringMarshaller.ConvertToManaged(_title);

    /// <summary>
    /// The description of the news entry.
    /// </summary>
    public string? Description => Utf8StringMarshaller.ConvertToManaged(_description);

    /// <summary>
    /// The HREF/click URL of the news entry.
    /// </summary>
    public string? Url => Utf8StringMarshaller.ConvertToManaged(_url);

    /// <summary>
    /// The short format (DD/MM) of the date for the news entry.
    /// </summary>
    public string? PostDate => Utf8StringMarshaller.ConvertToManaged(_postDate);

    /// <summary>
    /// Write the strings into the current struct.
    /// </summary>
    /// <param name="title">The title of the news entry.</param>
    /// <param name="description">The description of the news entry.</param>
    /// <param name="url">The HREF/click URL of the news entry.</param>
    /// <param name="postDate">The short format (DD/MM) of the date for the news entry.</param>
    public void Write(string? title, string? description, string? url, string? postDate)
    {
        Utf8StringMarshaller.Free(_title);
        Utf8StringMarshaller.Free(_description);
        Utf8StringMarshaller.Free(_url);
        Utf8StringMarshaller.Free(_postDate);

        _title       = Utf8StringMarshaller.ConvertToUnmanaged(title);
        _description = Utf8StringMarshaller.ConvertToUnmanaged(description);
        _url         = Utf8StringMarshaller.ConvertToUnmanaged(url);
        _postDate    = Utf8StringMarshaller.ConvertToUnmanaged(postDate);
    }

    public void Dispose()
    {
        if (_isFreed == 1) return;

        Utf8StringMarshaller.Free(_title);
        Utf8StringMarshaller.Free(_description);
        Utf8StringMarshaller.Free(_url);
        Utf8StringMarshaller.Free(_postDate);

        _isFreed = 1;
    }
}
