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
    /// <param name="isAllocated"><c>true</c> if it's not empty. Otherwise, <c>false</c></param>
    void GetBackgroundEntries(out nint handle, out int count, [MarshalAs(UnmanagedType.Bool)] out bool isDisposable, [MarshalAs(UnmanagedType.Bool)] out bool isAllocated);

    /// <summary>
    /// Gets the logo overlay's URL entries for the launcher.<br/>
    /// This method returns a handle to the <see cref="PluginDisposableMemory{T}"/> of <see cref="LauncherPathEntry"/>.<br/>
    /// Pass this method to <see cref="PluginDisposableMemoryExtension.ToManagedSpan{T}(PluginDisposableMemoryExtension.MarshalToMemorySelectorDelegate)"/> to get the span.
    /// </summary>
    /// <param name="handle">The handle to the <see cref="LauncherPathEntry"/> struct</param>
    /// <param name="count">How much data is available from the <paramref name="handle"/></param>
    /// <param name="isDisposable">Whether if the handle can be freed or not</param>
    /// <param name="isAllocated"><c>true</c> if it's not empty. Otherwise, <c>false</c></param>
    void GetLogoOverlayEntries(out nint handle, out int count, [MarshalAs(UnmanagedType.Bool)] out bool isDisposable, [MarshalAs(UnmanagedType.Bool)] out bool isAllocated);

    /// <summary>
    /// Gets the current background flag, which indicates the type and source of the launcher background.
    /// </summary>
    /// <param name="result">A <see cref="LauncherBackgroundFlag"/> value which representing the type (image, image sequence, video) and source (file, zip) of the background.</param>
    /// <remarks>
    /// This method returns a <see cref="LauncherBackgroundFlag"/> value via <paramref name="result"/> which representing the type (image, image sequence, video) and source (file, zip) of the background.
    /// </remarks>
    void GetBackgroundFlag(out LauncherBackgroundFlag result);

    /// <summary>
    /// Gets the current logo flag, which indicates the type and source of the logo.
    /// </summary>
    /// <param name="result">A <see cref="LauncherBackgroundFlag"/> value which representing the type (image, image sequence, video) and source (file, zip) of the logo.</param>
    /// <remarks>
    /// This method returns a <see cref="LauncherBackgroundFlag"/> value via <paramref name="result"/>which representing the type (image, image sequence, video) and source (file, zip) of the logo.
    /// </remarks>
    void GetLogoFlag(out LauncherBackgroundFlag result);

    /// <summary>
    /// Gets the background sprite FPS (frames per second) for the launcher background image sequence.
    /// </summary>
    /// <param name="result">Frames per second for the sprites to cycle. Returns 0 if the background image is static.</param>
    void GetBackgroundSpriteFps(out float result);
}
