using System.Runtime.InteropServices.Marshalling;
using System.Runtime.InteropServices;

namespace Hi3Helper.Plugin.Core.Management.Api;

/// <summary>
/// Defines the contract for managing launcher media assets such as background images, image sequences, videos, and logo overlays.
/// This interface is intended for use in plugin scenarios where the launcher UI requires dynamic or customizable media content.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="ILauncherApiMedia"/> interface provides methods to retrieve, count, and free resources related to launcher backgrounds and logo overlays.
/// It also exposes a method to determine the type and source of the current background media.
/// </para>
/// <para>
/// All methods that return pointers to entries (such as <see cref="GetBackgroundEntries"/> and <see cref="GetLogoOverlayEntries"/>) must be paired with their corresponding free methods
/// (<see cref="FreePathEntriesHandle(nint)"/> to avoid memory leaks.
/// </para>
/// <para>
/// This interface inherits from <see cref="IInitializable"/>, requiring implementers to provide asynchronous initialization logic.
/// </para>
/// </remarks>
[GeneratedComInterface]
[Guid(ComInterfaceId.LauncherApiMedia)]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
// ReSharper disable once RedundantUnsafeContext
public unsafe partial interface ILauncherApiMedia : ILauncherApi
{
    /// <summary>
    /// Get the background image's URL entries for the launcher.
    /// </summary>
    /// <returns>
    /// A pointer to the first <see cref="LauncherPathEntry"/> representing the background image URL entries.
    /// </returns>
    nint GetBackgroundEntries();

    /// <summary>
    /// Gets the logo overlay's URL entries for the launcher.
    /// </summary>
    /// <returns>
    /// A pointer to the first <see cref="LauncherPathEntry"/> representing the logo overlay sprites URL entries.
    /// </returns>
    nint GetLogoOverlayEntries();

    /// <summary>
    /// Frees the memory allocated for the handle of URL entries.
    /// This method should be called when the entries are no longer needed.
    /// </summary>
    /// <param name="handle">
    /// The handle pointer to the logo entries to be freed.
    /// </param>
    /// <returns>
    /// <c>true</c> if the entries were successfully freed; <c>false</c> if they were already freed or failed to be freed.
    /// </returns>
    [return: MarshalAs(UnmanagedType.Bool)]
    bool FreePathEntriesHandle(nint handle);

    /// <summary>
    /// Gets the current background flag, which indicates the type and source of the launcher background.
    /// </summary>
    /// <returns>
    /// A <see cref="LauncherBackgroundFlag"/> value representing the type (image, image sequence, video) and source (file, zip) of the background.
    /// </returns>
    [PreserveSig]
    LauncherBackgroundFlag GetBackgroundFlag();
}
