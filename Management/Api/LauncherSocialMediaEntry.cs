﻿using Hi3Helper.Plugin.Core.Utility;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
// ReSharper disable ReplaceWithPrimaryConstructorParameter

namespace Hi3Helper.Plugin.Core.Management.Api;

/// <summary>
/// Entry of the launcher's social media data.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 8)]
public unsafe struct LauncherSocialMediaEntry
    : IDisposable, IInitializableStruct
{
    public const int ExDescriptionMaxLength = 128;
    public const int ExUrlMaxLength         = 512;

    /// <summary>
    /// Entry of the launcher's social media data.
    /// </summary>
    /// <param name="iconDataHandle">
    /// The handle of the icon data handle.
    /// This can be a <see cref="PluginDisposableMemory{T}"/> of data buffer or UTF-8 (null terminated) string.
    /// depending on what the <see cref="LauncherSocialMediaEntryFlag"/> is defined.
    /// </param>
    /// <param name="iconDataLengthInBytes">
    /// The length of data in bytes of the <paramref name="iconDataHandle"/>.
    /// </param>
    /// <param name="isIconDataDisposable">
    /// Whether to determine if handle is disposable or not.
    /// </param>
    /// <param name="iconHoverDataHandle">
    /// The handle of the icon data handle.
    /// This can be a <see cref="PluginDisposableMemory{T}"/> of data buffer or UTF-8 (null terminated) string.
    /// depending on what the <see cref="LauncherSocialMediaEntryFlag"/> is defined.<br/>
    /// This handle can be null-ed.
    /// </param>
    /// <param name="iconHoverDataLengthInBytes">
    /// The length of data in bytes of the <paramref name="iconDataHandle"/>.<br/>
    /// The size can be emptied-ed.
    /// </param>
    /// <param name="isIconHoverDataDisposable">
    /// Whether to determine if handle is disposable or not.
    /// </param>
    /// <param name="qrImageDataHandle">
    /// The handle of the qr image data handle.
    /// This can be a <see cref="PluginDisposableMemory{T}"/> of data buffer or UTF-8 (null terminated) string.
    /// depending on what the <see cref="LauncherSocialMediaEntryFlag"/> is defined.<br/>
    /// This handle can be null-ed.
    /// </param>
    /// <param name="qrImageDataLengthInBytes">
    /// The length of data in bytes of the <paramref name="qrImageDataHandle"/>.<br/>
    /// The size can be emptied-ed.
    /// </param>
    /// <param name="isQrImageDataDisposable">
    /// Whether to determine if handle is disposable or not.
    /// </param>
    /// <param name="childEntryHandle">
    /// The handle of the child entry.<br/>
    /// This handle can be null-ed.
    /// </param>
    /// <param name="flags">
    /// The definition of what kind of properties that the entry could have.
    /// </param>
    public LauncherSocialMediaEntry(void* iconDataHandle,
                                    int iconDataLengthInBytes,
                                    bool isIconDataDisposable,
                                    void* iconHoverDataHandle,
                                    int iconHoverDataLengthInBytes,
                                    bool isIconHoverDataDisposable,
                                    void* qrImageDataHandle,
                                    int qrImageDataLengthInBytes,
                                    bool isQrImageDataDisposable,
                                    LauncherSocialMediaEntry* childEntryHandle,
                                    LauncherSocialMediaEntryFlag flags) =>
        InitInner(iconDataHandle,
                  iconDataLengthInBytes,
                  isIconDataDisposable,
                  iconHoverDataHandle,
                  iconHoverDataLengthInBytes,
                  isIconHoverDataDisposable,
                  qrImageDataHandle,
                  qrImageDataLengthInBytes,
                  isQrImageDataDisposable,
                  childEntryHandle,
                  flags);

    /// <summary>
    /// Initializes the inner-data handle of the <see cref="LauncherSocialMediaEntry"/> struct.
    /// </summary>
    /// <param name="iconDataHandle">
    /// The handle of the icon data handle.
    /// This can be a <see cref="PluginDisposableMemory{T}"/> of data buffer or UTF-8 (null terminated) string.
    /// depending on what the <see cref="LauncherSocialMediaEntryFlag"/> is defined.
    /// </param>
    /// <param name="iconDataLengthInBytes">
    /// The length of data in bytes of the <paramref name="iconDataHandle"/>.
    /// </param>
    /// <param name="isIconDataDisposable">
    /// Whether to determine if handle is disposable or not.
    /// </param>
    /// <param name="iconHoverDataHandle">
    /// The handle of the icon data handle.
    /// This can be a <see cref="PluginDisposableMemory{T}"/> of data buffer or UTF-8 (null terminated) string.
    /// depending on what the <see cref="LauncherSocialMediaEntryFlag"/> is defined.<br/>
    /// This handle can be null-ed.
    /// </param>
    /// <param name="iconHoverDataLengthInBytes">
    /// The length of data in bytes of the <paramref name="iconDataHandle"/>.<br/>
    /// The size can be emptied-ed.
    /// </param>
    /// <param name="isIconHoverDataDisposable">
    /// Whether to determine if handle is disposable or not.
    /// </param>
    /// <param name="qrImageDataHandle">
    /// The handle of the qr image data handle.
    /// This can be a <see cref="PluginDisposableMemory{T}"/> of data buffer or UTF-8 (null terminated) string.
    /// depending on what the <see cref="LauncherSocialMediaEntryFlag"/> is defined.<br/>
    /// This handle can be null-ed.
    /// </param>
    /// <param name="qrImageDataLengthInBytes">
    /// The length of data in bytes of the <paramref name="qrImageDataHandle"/>.<br/>
    /// The size can be emptied-ed.
    /// </param>
    /// <param name="isQrImageDataDisposable">
    /// Whether to determine if handle is disposable or not.
    /// </param>
    /// <param name="childEntryHandle">
    /// The handle of the child entry.<br/>
    /// This handle can be null-ed.
    /// </param>
    /// <param name="flags">
    /// The definition of what kind of properties that the entry could have.
    /// </param>
    public void InitInner(void* iconDataHandle,
                          int iconDataLengthInBytes,
                          bool isIconDataDisposable,
                          void* iconHoverDataHandle,
                          int iconHoverDataLengthInBytes,
                          bool isIconHoverDataDisposable,
                          void* qrImageDataHandle,
                          int qrImageDataLengthInBytes,
                          bool isQrImageDataDisposable,
                          LauncherSocialMediaEntry* childEntryHandle,
                          LauncherSocialMediaEntryFlag flags)
    {
        _iconPathHandle = iconDataHandle;
        _iconPathLengthInBytes = iconDataLengthInBytes;
        _iconHoverPathHandle = iconHoverDataHandle;
        _iconHoverPathLengthInBytes = iconHoverDataLengthInBytes;
        _qrPathHandle = qrImageDataHandle;
        _qrPathLengthInBytes = qrImageDataLengthInBytes;
        _childEntryHandle = childEntryHandle;
        Flags = flags;

        _isIconHandleDisposable = (byte)(isIconDataDisposable ? 1 : 0);
        _isIconHoverHandleDisposable = (byte)(isIconHoverDataDisposable ? 1 : 0);
        _isQrImageHandleDisposable = (byte)(isQrImageDataDisposable ? 1 : 0);

#pragma warning disable CS0618
        InitInner();
#pragma warning restore CS0618
    }

    [Obsolete("This might cause an error while being used as the struct requires some fields to be initialized. Please use InitInner() instead!.")]
    public void InitInner()
    {
        if (Flags.HasFlag(LauncherSocialMediaEntryFlag.HasDescription))
        {
            _socialMediaDescription = Mem.Alloc<byte>(ExDescriptionMaxLength);
        }

        if (Flags.HasFlag(LauncherSocialMediaEntryFlag.HasClickUrl))
        {
            _socialMediaClickUrl = Mem.Alloc<byte>(ExUrlMaxLength);
        }

        if (!Flags.HasFlag(LauncherSocialMediaEntryFlag.HasQrImage)) return;

        if (Flags.HasFlag(LauncherSocialMediaEntryFlag.QrImageIsPath))
        {
            _qrImageDescription = Mem.Alloc<byte>(ExDescriptionMaxLength);
        }
    }

    private byte _isFreed = 0;

    private void* _iconPathHandle;
    private int _iconPathLengthInBytes;

    private void* _iconHoverPathHandle;
    private int _iconHoverPathLengthInBytes;

    private void* _qrPathHandle;
    private int _qrPathLengthInBytes;

    private byte* _socialMediaDescription;
    private byte* _socialMediaClickUrl;
    private byte* _qrImageDescription;

    private LauncherSocialMediaEntry* _childEntryHandle;

    /// <summary>
    /// Defines the kind of properties or flags that a social media entry can have.
    /// </summary>
    public LauncherSocialMediaEntryFlag Flags;
    private byte _isIconHandleDisposable = 0;
    private byte _isIconHoverHandleDisposable = 0;
    private byte _isQrImageHandleDisposable = 0;

    /// <summary>
    /// The handle of the child entry. The type is <see cref="LauncherSocialMediaEntry"/>
    /// </summary>
    public nint ChildEntryHandle => (nint)_childEntryHandle;

    /// <summary>
    /// Gets the icon handle as path.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ref LauncherPathEntry GetIconAsPath()
    {
        if (!Flags.HasFlag(LauncherSocialMediaEntryFlag.IconIsPath))
        {
            return ref Unsafe.NullRef<LauncherPathEntry>();
        }
        
        return ref Mem.AsRef<LauncherPathEntry>(_iconPathHandle);
    }

    /// <summary>
    /// Gets the icon handle as a data buffer.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly PluginDisposableMemory<byte> GetIconAsDataBuffer() =>
        !Flags.HasFlag(LauncherSocialMediaEntryFlag.IconIsDataBuffer) ?
                PluginDisposableMemory<byte>.Empty :
            new PluginDisposableMemory<byte>((byte*)_iconPathHandle, _iconPathLengthInBytes, _isIconHandleDisposable == 1);

    /// <summary>
    /// Gets the hovered icon handle as path.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ref LauncherPathEntry GetIconHoverAsPath()
    {
        if (!Flags.HasFlag(LauncherSocialMediaEntryFlag.IconIsPath) || _iconHoverPathHandle == null)
        {
            return ref Unsafe.NullRef<LauncherPathEntry>();
        }

        return ref Mem.AsRef<LauncherPathEntry>(_iconHoverPathHandle);
    }

    /// <summary>
    /// Gets the hovered icon handle as a data buffer.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly PluginDisposableMemory<byte> GetIconHoverAsDataBuffer() =>
        !Flags.HasFlag(LauncherSocialMediaEntryFlag.IconIsDataBuffer) || _iconHoverPathHandle == null ?
            PluginDisposableMemory<byte>.Empty :
            new PluginDisposableMemory<byte>((byte*)_iconHoverPathHandle, _iconHoverPathLengthInBytes, _isIconHoverHandleDisposable == 1);

    /// <summary>
    /// Gets the QR Image handle as path.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly ref LauncherPathEntry GetQrImageAsPath()
    {
        if (!Flags.HasFlag(LauncherSocialMediaEntryFlag.QrImageIsPath))
        {
            return ref Unsafe.NullRef<LauncherPathEntry>();
        }

        return ref Mem.AsRef<LauncherPathEntry>(_qrPathHandle);
    }

    /// <summary>
    /// Gets the QR Image handle as a data buffer.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly PluginDisposableMemory<byte> GetQrImageAsDataBuffer() =>
        !Flags.HasFlag(LauncherSocialMediaEntryFlag.HasQrImage) ||
        !Flags.HasFlag(LauncherSocialMediaEntryFlag.QrImageIsDataBuffer) ?
            PluginDisposableMemory<byte>.Empty :
            new PluginDisposableMemory<byte>((byte*)_qrPathHandle, _qrPathLengthInBytes, _isQrImageHandleDisposable == 1);

    /// <summary>
    /// A span of social media description as string. If the flag doesn't have flag for the description, it will return an empty span.
    /// </summary>
    public readonly PluginDisposableMemory<byte> SocialMediaDescription
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => !Flags.HasFlag(LauncherSocialMediaEntryFlag.HasDescription) ?
                PluginDisposableMemory<byte>.Empty :
            new PluginDisposableMemory<byte>(_socialMediaDescription, ExDescriptionMaxLength);
    }

    /// <summary>
    /// A span of QR Image description as string. If the flag doesn't have flag for the description, it will return an empty span.
    /// </summary>
    public readonly PluginDisposableMemory<byte> QrImageDescription
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => !Flags.HasFlag(LauncherSocialMediaEntryFlag.HasQrImage) ?
                PluginDisposableMemory<byte>.Empty :
            new PluginDisposableMemory<byte>(_qrImageDescription, ExDescriptionMaxLength);
    }

    /// <summary>
    /// A span of social media Click URL as string. If the flag doesn't have flag for the click URL, it will return an empty span.
    /// </summary>
    public readonly PluginDisposableMemory<byte> SocialMediaClickUrl
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => !Flags.HasFlag(LauncherSocialMediaEntryFlag.HasClickUrl) ?
                PluginDisposableMemory<byte>.Empty :
            new PluginDisposableMemory<byte>(_socialMediaClickUrl, ExUrlMaxLength);
    }

    /// <summary>
    /// Dispose the handles of the icon, QR image, description, Click URL and its child handles.
    /// </summary>
    public void Dispose()
    {
        if (_isFreed == 1) return;

        if (Flags.HasFlag(LauncherSocialMediaEntryFlag.IconIsPath))
        {
            if (_isIconHandleDisposable == 1) GetIconAsPath().Dispose();
            if (_isIconHoverHandleDisposable == 1) GetIconHoverAsPath().Dispose();
        }
        else
        {
            if (_isIconHandleDisposable == 1) Mem.Free(_iconPathHandle);
            if (_isIconHoverHandleDisposable == 1) Mem.Free(_iconHoverPathHandle);
        }

        if (Flags.HasFlag(LauncherSocialMediaEntryFlag.HasQrImage))
        {
            if (Flags.HasFlag(LauncherSocialMediaEntryFlag.QrImageIsPath))
            {
                if (_isQrImageHandleDisposable == 1) GetQrImageAsPath().Dispose();
            }
            else
            {
                if (_isQrImageHandleDisposable == 1) Mem.Free(_qrPathHandle);
            }
        }

        Mem.Free(_qrImageDescription);
        Mem.Free(_socialMediaDescription);
        Mem.Free(_socialMediaClickUrl);

        if (_childEntryHandle != null)
        {
            _childEntryHandle->Dispose();
        }

        _isFreed = 1;
    }
}
