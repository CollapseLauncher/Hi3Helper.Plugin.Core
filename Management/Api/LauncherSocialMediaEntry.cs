using Hi3Helper.Plugin.Core.Utility;
using System;
using System.Runtime.CompilerServices;
// ReSharper disable ReplaceWithPrimaryConstructorParameter

namespace Hi3Helper.Plugin.Core.Management.Api;

/// <summary>
/// Entry of the launcher's social media data.
/// </summary>
public unsafe struct LauncherSocialMediaEntry
    : IDisposable, IInitializableStruct
{
    public const int ExDescriptionMaxLength = 128; // 256 bytes
    public const int ExUrlMaxLength         = 512; // 1024 bytes

    /// <summary>
    /// Entry of the launcher's social media data.
    /// </summary>
    /// <param name="iconDataHandle">
    /// The handle of the icon data handle.
    /// This can be a <see cref="PluginDisposableMemory{T}"/> of <see cref="byte"/> or <see cref="char"/>
    /// depending on what the <see cref="LauncherSocialMediaEntryFlag"/> is defined.
    /// </param>
    /// <param name="iconDataLengthInBytes">
    /// The length of data in bytes of the <paramref name="iconDataHandle"/>.
    /// </param>
    /// <param name="iconHoverDataHandle">
    /// The handle of the icon data handle.
    /// This can be a <see cref="PluginDisposableMemory{T}"/> of <see cref="byte"/> or <see cref="char"/>
    /// depending on what the <see cref="LauncherSocialMediaEntryFlag"/> is defined.<br/>
    /// This handle can be null-ed.
    /// </param>
    /// <param name="iconHoverDataLengthInBytes">
    /// The length of data in bytes of the <paramref name="iconDataHandle"/>.<br/>
    /// The size can be emptied-ed.
    /// </param>
    /// <param name="qrImageDataHandle">
    /// The handle of the qr image data handle.
    /// This can be a <see cref="PluginDisposableMemory{T}"/> of <see cref="byte"/> or <see cref="char"/>
    /// depending on what the <see cref="LauncherSocialMediaEntryFlag"/> is defined.<br/>
    /// This handle can be null-ed.
    /// </param>
    /// <param name="qrImageDataLengthInBytes">
    /// The length of data in bytes of the <paramref name="qrImageDataHandle"/>.<br/>
    /// The size can be emptied-ed.
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
                                    void* iconHoverDataHandle,
                                    int iconHoverDataLengthInBytes,
                                    void* qrImageDataHandle,
                                    int qrImageDataLengthInBytes,
                                    LauncherSocialMediaEntry* childEntryHandle,
                                    LauncherSocialMediaEntryFlag flags) =>
        InitInner(iconDataHandle,
                  iconDataLengthInBytes,
                  iconHoverDataHandle,
                  iconHoverDataLengthInBytes,
                  qrImageDataHandle,
                  qrImageDataLengthInBytes,
                  childEntryHandle,
                  flags);

    /// <summary>
    /// Initializes the inner-data handle of the <see cref="LauncherSocialMediaEntry"/> struct.
    /// </summary>
    /// <param name="iconDataHandle">
    /// The handle of the icon data handle.
    /// This can be a <see cref="PluginDisposableMemory{T}"/> of <see cref="byte"/> or <see cref="char"/>
    /// depending on what the <see cref="LauncherSocialMediaEntryFlag"/> is defined.
    /// </param>
    /// <param name="iconDataLengthInBytes">
    /// The length of data in bytes of the <paramref name="iconDataHandle"/>.
    /// </param>
    /// <param name="iconHoverDataHandle">
    /// The handle of the icon data handle.
    /// This can be a <see cref="PluginDisposableMemory{T}"/> of <see cref="byte"/> or <see cref="char"/>
    /// depending on what the <see cref="LauncherSocialMediaEntryFlag"/> is defined.<br/>
    /// This handle can be null-ed.
    /// </param>
    /// <param name="iconHoverDataLengthInBytes">
    /// The length of data in bytes of the <paramref name="iconDataHandle"/>.<br/>
    /// The size can be emptied-ed.
    /// </param>
    /// <param name="qrImageDataHandle">
    /// The handle of the qr image data handle.
    /// This can be a <see cref="PluginDisposableMemory{T}"/> of <see cref="byte"/> or <see cref="char"/>
    /// depending on what the <see cref="LauncherSocialMediaEntryFlag"/> is defined.<br/>
    /// This handle can be null-ed.
    /// </param>
    /// <param name="qrImageDataLengthInBytes">
    /// The length of data in bytes of the <paramref name="qrImageDataHandle"/>.<br/>
    /// The size can be emptied-ed.
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
                          void* iconHoverDataHandle,
                          int iconHoverDataLengthInBytes,
                          void* qrImageDataHandle,
                          int qrImageDataLengthInBytes,
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

#pragma warning disable CS0618
        InitInner();
#pragma warning restore CS0618
    }

    [Obsolete("This might cause an error while being used as the struct requires some fields to be initialized. Please use InitInner() instead!.")]
    public void InitInner()
    {
        if (Flags.HasFlag(LauncherSocialMediaEntryFlag.HasDescription))
        {
            _socialMediaDescription = Mem.Alloc<char>(ExDescriptionMaxLength);
        }

        if (Flags.HasFlag(LauncherSocialMediaEntryFlag.HasClickUrl))
        {
            _socialMediaClickUrl = Mem.Alloc<char>(ExUrlMaxLength);
        }

        if (!Flags.HasFlag(LauncherSocialMediaEntryFlag.HasQrImage)) return;

        if (Flags.HasFlag(LauncherSocialMediaEntryFlag.QrImageIsPath))
        {
            _qrImageDescription = Mem.Alloc<char>(ExDescriptionMaxLength);
        }
    }

    private void* _iconPathHandle;
    private int _iconPathLengthInBytes;

    private void* _iconHoverPathHandle;
    private int _iconHoverPathLengthInBytes;

    private void* _qrPathHandle;
    private int _qrPathLengthInBytes;

    private char* _socialMediaDescription;
    private char* _socialMediaClickUrl;
    private char* _qrImageDescription;

    private LauncherSocialMediaEntry* _childEntryHandle;

    /// <summary>
    /// Defines the kind of properties or flags that a social media entry can have.
    /// </summary>
    public LauncherSocialMediaEntryFlag Flags;

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
            new PluginDisposableMemory<byte>((byte*)_iconPathHandle, _iconPathLengthInBytes);

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
            new PluginDisposableMemory<byte>((byte*)_iconHoverPathHandle, _iconHoverPathLengthInBytes);

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
            new PluginDisposableMemory<byte>((byte*)_qrPathHandle, _qrPathLengthInBytes);

    /// <summary>
    /// A span of social media description as string. If the flag doesn't have flag for the description, it will return an empty span.
    /// </summary>
    public readonly PluginDisposableMemory<char> SocialMediaDescription
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => !Flags.HasFlag(LauncherSocialMediaEntryFlag.HasDescription) ?
                PluginDisposableMemory<char>.Empty :
            new PluginDisposableMemory<char>(_socialMediaDescription, ExDescriptionMaxLength);
    }

    /// <summary>
    /// A span of QR Image description as string. If the flag doesn't have flag for the description, it will return an empty span.
    /// </summary>
    public readonly PluginDisposableMemory<char> QrImageDescription
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => !Flags.HasFlag(LauncherSocialMediaEntryFlag.HasQrImage) ?
                PluginDisposableMemory<char>.Empty :
            new PluginDisposableMemory<char>(_qrImageDescription, ExDescriptionMaxLength);
    }

    /// <summary>
    /// A span of social media Click URL as string. If the flag doesn't have flag for the click URL, it will return an empty span.
    /// </summary>
    public readonly PluginDisposableMemory<char> SocialMediaClickUrl
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => !Flags.HasFlag(LauncherSocialMediaEntryFlag.HasClickUrl) ?
                PluginDisposableMemory<char>.Empty :
            new PluginDisposableMemory<char>(_socialMediaClickUrl, ExUrlMaxLength);
    }

    /// <summary>
    /// Dispose the handles of the icon, QR image, description, Click URL and its child handles.
    /// </summary>
    public void Dispose()
    {
        if (Flags.HasFlag(LauncherSocialMediaEntryFlag.IconIsPath))
        {
            GetIconAsPath().Dispose();
            GetIconHoverAsPath().Dispose();
        }
        else
        {
            Mem.Free(_iconPathHandle);
            Mem.Free(_iconHoverPathHandle);
        }

        if (Flags.HasFlag(LauncherSocialMediaEntryFlag.HasQrImage))
        {
            if (Flags.HasFlag(LauncherSocialMediaEntryFlag.QrImageIsPath))
            {
                GetQrImageAsPath().Dispose();
            }
            else
            {
                Mem.Free(_qrPathHandle);
            }
        }

        Mem.Free(_qrImageDescription);
        Mem.Free(_socialMediaDescription);
        Mem.Free(_socialMediaClickUrl);

        if (_childEntryHandle != null)
        {
            _childEntryHandle->Dispose();
        }
    }
}
