using System.Runtime.InteropServices.Marshalling;
using System.Runtime.InteropServices;

namespace Hi3Helper.Plugin.Core.Management.Api;

/// <summary>
/// Defines the contract for managing launcher media assets such as background images, image sequences, videos, and logo overlays.
/// This interface is intended for use in plugin scenarios where the launcher UI requires dynamic or customizable media content.
/// </summary>
/// <remarks>
/// <para>
/// This interface inherits from <see cref="IInitializableTask"/>, requiring implementers to provide asynchronous initialization logic.
/// </para>
/// </remarks>
[GeneratedComInterface]
[Guid(ComInterfaceId.ExLauncherApiMedia)]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
// ReSharper disable once RedundantUnsafeContext
public unsafe partial interface ILauncherApiMedia : ILauncherApi
{
    /// <summary>
    /// Get the background image's URL entries for the launcher.<br/>
    /// This method returns a handle to the <see cref="PluginDisposableMemory{T}"/> of <see cref="LauncherPathEntry"/>.<br/>
    /// Pass this method to <see cref="PluginDisposableMemoryExtension.ToManagedSpan{T}(PluginDisposableMemoryExtension.MarshalToMemorySelectorDelegate)"/> to get the span.
    /// </summary>
    /// <param name="handle">The handle to the <see cref="LauncherPathEntry"/> struct</param>
    /// <param name="count">How much data is available from the <paramref name="handle"/></param>
    /// <param name="isDisposable">Whether if the handle can be freed or not</param>
    /// <returns>
    /// Returns <c>true</c> if it's not empty. Otherwise, <c>false</c>.
    /// </returns>
    [return: MarshalAs(UnmanagedType.Bool)]
    bool GetBackgroundEntries(out nint handle, out int count, [MarshalAs(UnmanagedType.Bool)] out bool isDisposable);

    /// <summary>
    /// Gets the logo overlay's URL entries for the launcher.<br/>
    /// This method returns a handle to the <see cref="PluginDisposableMemory{T}"/> of <see cref="LauncherPathEntry"/>.<br/>
    /// Pass this method to <see cref="PluginDisposableMemoryExtension.ToManagedSpan{T}(PluginDisposableMemoryExtension.MarshalToMemorySelectorDelegate)"/> to get the span.
    /// </summary>
    /// <param name="handle">The handle to the <see cref="LauncherPathEntry"/> struct</param>
    /// <param name="count">How much data is available from the <paramref name="handle"/></param>
    /// <param name="isDisposable">Whether if the handle can be freed or not</param>
    /// <returns>
    /// Returns <c>true</c> if it's not empty. Otherwise, <c>false</c>.
    /// </returns>
    [return: MarshalAs(UnmanagedType.Bool)]
    bool GetLogoOverlayEntries(out nint handle, out int count, [MarshalAs(UnmanagedType.Bool)] out bool isDisposable);

    /// <summary>
    /// Gets the current background flag, which indicates the type and source of the launcher background.
    /// </summary>
    /// <returns>
    /// A <see cref="LauncherBackgroundFlag"/> value representing the type (image, image sequence, video) and source (file, zip) of the background.
    /// </returns>
    [PreserveSig]
    LauncherBackgroundFlag GetBackgroundFlag();

    /// <summary>
    /// Gets the current logo flag, which indicates the type and source of the launcher background.
    /// </summary>
    /// <returns>
    /// A <see cref="LauncherBackgroundFlag"/> value representing the type (image, image sequence, video) and source (file, zip) of the background.
    /// </returns>
    [PreserveSig]
    LauncherBackgroundFlag GetLogoFlag();

    /// <summary>
    /// Gets the background sprite FPS (frames per second) for the launcher background image sequence.
    /// </summary>
    /// <returns>Frames per second for the sprites to cycle. Returns 0 if the background image is static.</returns>
    [PreserveSig]
    float GetBackgroundSpriteFps();
}
