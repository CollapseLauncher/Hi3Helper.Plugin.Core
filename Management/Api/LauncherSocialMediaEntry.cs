using Hi3Helper.Plugin.Core.Utility;
using System;
using System.Buffers;
using System.Buffers.Text;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
// ReSharper disable ReplaceWithPrimaryConstructorParameter

namespace Hi3Helper.Plugin.Core.Management.Api;

/// <summary>
/// Entry of the launcher's social media data.
/// </summary>
[StructLayout(LayoutKind.Explicit)] // Fit to 64 bytes.
public unsafe struct LauncherSocialMediaEntry() : IDisposable
{
    [FieldOffset(0)]
    private byte _isFreed = 0;

    /// <summary>
    /// Defines the kind of properties or flags that a social media entry can have.
    /// </summary>
    [FieldOffset(4)]
    public LauncherSocialMediaEntryFlag Flags = LauncherSocialMediaEntryFlag.None;

    [FieldOffset(8)]
    private byte* _iconPath = null;

    [FieldOffset(16)]
    private byte* _iconHoverPath = null;

    [FieldOffset(24)]
    private byte* _qrPath = null;

    [FieldOffset(32)]
    private byte* _qrImageDescription = null;

    [FieldOffset(40)]
    private byte* _socialMediaDescription = null;

    [FieldOffset(48)]
    private byte* _socialMediaClickUrl = null;

    [FieldOffset(56)]
    private LauncherSocialMediaEntry* _childEntryHandle = null;

    /// <summary>
    /// The handle of the child entry. The type is <see cref="LauncherSocialMediaEntry"/>
    /// </summary>
    public readonly ref LauncherSocialMediaEntry ChildEntryHandle
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => ref Mem.AsRef<LauncherSocialMediaEntry>(_childEntryHandle);
    }

    /// <summary>
    /// Represent a path of the Icon.<br/>
    /// The format can be a <see cref="Base64Url"/> if <see cref="LauncherSocialMediaEntryFlag.IconIsDataBuffer"/> is defined<br/>
    /// or a path/URL <see cref="string"/> if <see cref="LauncherSocialMediaEntryFlag.IconIsPath"/> is defined.
    /// </summary>
    public string? IconPath
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Utf8StringMarshaller.ConvertToManaged(_iconPath);
    }

    /// <summary>
    /// Represent a path of the Hover Icon.<br/>
    /// The format can be a <see cref="Base64Url"/> if <see cref="LauncherSocialMediaEntryFlag.IconHoverIsDataBuffer"/> is defined<br/>
    /// or a path/URL <see cref="string"/> if <see cref="LauncherSocialMediaEntryFlag.IconHoverIsPath"/> is defined.
    /// </summary>
    public string? IconHoverPath
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Utf8StringMarshaller.ConvertToManaged(_iconHoverPath);
    }

    /// <summary>
    /// Represent a path of the QR Image.<br/>
    /// The format can be a <see cref="Base64Url"/> if <see cref="LauncherSocialMediaEntryFlag.QrImageIsDataBuffer"/> is defined<br/>
    /// or a path/URL <see cref="string"/> if <see cref="LauncherSocialMediaEntryFlag.QrImageIsPath"/> is defined.
    /// </summary>
    public string? QrPath
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Utf8StringMarshaller.ConvertToManaged(_qrPath);
    }

    /// <summary>
    /// Represent the description string of the QR Image description.
    /// </summary>
    public string? QrDescription
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Utf8StringMarshaller.ConvertToManaged(_qrImageDescription);
    }

    /// <summary>
    /// Represent the description string of this <see cref="LauncherSocialMediaEntry"/> instance.
    /// </summary>
    public string? Description
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Utf8StringMarshaller.ConvertToManaged(_socialMediaDescription);
    }

    /// <summary>
    /// Represent the click link/HREF string of this <see cref="LauncherSocialMediaEntry"/> instance.
    /// </summary>
    public string? ClickUrl
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Utf8StringMarshaller.ConvertToManaged(_socialMediaClickUrl);
    }

    /// <summary>
    /// Write <see cref="IconPath"/> as a path or URL.
    /// </summary>
    /// <param name="path">The path/URL to write.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteIcon(string? path)
    {
        _iconPath = FreeAndWriteFromString(_iconPath, path);
        Flags |= ~LauncherSocialMediaEntryFlag.IconIsDataBuffer;
        Flags |= LauncherSocialMediaEntryFlag.IconIsPath;
    }

    /// <summary>
    /// Write <see cref="IconPath"/> as an embedded data in <see cref="Base64Url"/> format.
    /// </summary>
    /// <param name="buffer">The buffer of the data to be written from.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteIcon(ReadOnlySpan<byte> buffer)
    {
        _iconPath = FreeAndWriteFromByte(_iconPath, buffer);
        Flags |= ~LauncherSocialMediaEntryFlag.IconIsPath;
        Flags |= LauncherSocialMediaEntryFlag.IconIsDataBuffer;
    }

    /// <summary>
    /// Write <see cref="IconHoverPath"/> as a path or URL.
    /// </summary>
    /// <param name="path">The path/URL to write.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteIconHover(string? path)
    {
        _iconHoverPath = FreeAndWriteFromString(_iconHoverPath, path);
        Flags |= ~LauncherSocialMediaEntryFlag.IconHoverIsDataBuffer;
        Flags |= LauncherSocialMediaEntryFlag.IconHoverIsPath;
    }

    /// <summary>
    /// Write <see cref="IconHoverPath"/> as an embedded data in <see cref="Base64Url"/> format.
    /// </summary>
    /// <param name="buffer">The buffer of the data to be written from.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteIconHover(ReadOnlySpan<byte> buffer)
    {
        _iconHoverPath = FreeAndWriteFromByte(_iconHoverPath, buffer);
        Flags |= ~LauncherSocialMediaEntryFlag.IconHoverIsPath;
        Flags |= LauncherSocialMediaEntryFlag.IconHoverIsDataBuffer;
    }

    /// <summary>
    /// Write <see cref="QrPath"/> as a path or URL.
    /// </summary>
    /// <param name="path">The path/URL to write.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteQrImage(string? path)
    {
        _qrPath = FreeAndWriteFromString(_qrPath, path);
        Flags |= ~LauncherSocialMediaEntryFlag.QrImageIsDataBuffer;
        Flags |= LauncherSocialMediaEntryFlag.QrImageIsPath;
    }

    /// <summary>
    /// Write <see cref="QrPath"/> as an embedded data in <see cref="Base64Url"/> format.
    /// </summary>
    /// <param name="buffer">The buffer of the data to be written from.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteQrImage(ReadOnlySpan<byte> buffer)
    {
        _qrPath = FreeAndWriteFromByte(_qrPath, buffer);
        Flags |= ~LauncherSocialMediaEntryFlag.QrImageIsPath;
        Flags |= LauncherSocialMediaEntryFlag.QrImageIsDataBuffer;
    }

    /// <summary>
    /// Write the description/name of this entry
    /// </summary>
    /// <param name="description">The description/name of this entry.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteDescription(string? description)
    {
        Utf8StringMarshaller.Free(_socialMediaDescription);
        _socialMediaDescription = Utf8StringMarshaller.ConvertToUnmanaged(description);
    }

    /// <summary>
    /// Write the description/name of the QR Image
    /// </summary>
    /// <param name="description">The description/name of the QR Image.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteQrImageDescription(string? description)
    {
        Utf8StringMarshaller.Free(_qrImageDescription);
        _qrImageDescription = Utf8StringMarshaller.ConvertToUnmanaged(description);
    }

    /// <summary>
    /// Write the click link/HREF of this entry.
    /// </summary>
    /// <param name="url">The click link/HREF of this entry.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteClickUrl(string? url)
    {
        Utf8StringMarshaller.Free(_socialMediaClickUrl);
        _socialMediaClickUrl = Utf8StringMarshaller.ConvertToUnmanaged(url);
    }

    /// <summary>
    /// Write the click link/HREF of this entry.
    /// </summary>
    /// <param name="url">The click link/HREF of this entry.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteClickUrl(Uri? url) => WriteClickUrl(url?.ToString());

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte* FreeAndWriteFromString(byte* ptr, string? inputStr)
    {
        Utf8StringMarshaller.Free(ptr);
        return Utf8StringMarshaller.ConvertToUnmanaged(inputStr);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte* FreeAndWriteFromByte(byte* ptr, ReadOnlySpan<byte> buffer)
    {
        Utf8StringMarshaller.Free(ptr);

        int maxEncoded = Base64Url.GetEncodedLength(buffer.Length);
        byte[] writeBuffer = ArrayPool<byte>.Shared.Rent(maxEncoded);

        try
        {
            if (!Base64Url.TryEncodeToUtf8(buffer, writeBuffer, out int written))
            {
                throw new InvalidOperationException("Cannot write data to buffer in Base64 format!");
            }

            int outLen = written + 1; // + \0 terminator
            byte* outPtr = (byte*)Marshal.AllocCoTaskMem(outLen);
            Span<byte> outSpan = new(outPtr, outLen);

            writeBuffer.AsSpan(0, written).CopyTo(outSpan); // Copy to buffer
            outSpan[outLen] = 0; // Ensure to write the \0 terminator as the memory might not be always cleared

            // Return CoTaskMemory
            return outPtr;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(writeBuffer);
        }
    }

    /// <summary>
    /// Dispose the handles of the icon, QR image, description, Click URL and its child handles.
    /// </summary>
    public void Dispose()
    {
        if (_isFreed == 1) return;

        Utf8StringMarshaller.Free(_iconPath);
        Utf8StringMarshaller.Free(_iconHoverPath);
        Utf8StringMarshaller.Free(_qrPath);
        Utf8StringMarshaller.Free(_qrImageDescription);
        Utf8StringMarshaller.Free(_socialMediaDescription);
        Utf8StringMarshaller.Free(_socialMediaClickUrl);

        if (_childEntryHandle != null)
        {
            _childEntryHandle->Dispose();
        }

        _isFreed = 1;
    }
}
