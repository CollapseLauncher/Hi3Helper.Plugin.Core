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
/// (<see cref="FreeBackgroundEntries"/> and <see cref="FreeLogoOverlayEntries"/>) to avoid memory leaks.
/// </para>
/// <para>
/// This interface inherits from <see cref="IInitializable"/>, requiring implementers to provide asynchronous initialization logic.
/// </para>
/// </remarks>
[GeneratedComInterface]
[Guid(ComInterfaceId.LauncherApiMedia)]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public partial interface ILauncherApiMedia : IInitializable
{
    /// <summary>
    /// Get the background image's local path entries for the launcher.
    /// </summary>
    /// <returns>
    /// The handle pointer or the current entry of the background sprite path.
    /// </returns>
    unsafe LauncherPathEntry* GetBackgroundEntries();

    /*
    /// <summary>
    /// Get the count of the background image entries.
    /// </summary>
    /// <returns>
    /// The count of the entry. It should return > 1 if multiple background sprites are available.
    /// </returns>
    [PreserveSig]
    int GetBackgroundEntriesCount();
    */

    /// <summary>
    /// Free the background's local path entries. This function should be called if the entries are no longer needed.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the entries were successfully freed; <c>false</c> if they were already freed or failed to be freed.
    /// </returns>
    [return: MarshalAs(UnmanagedType.Bool)]
    bool FreeBackgroundEntries();

    /// <summary>
    /// Gets the current background flag, which indicates the type and source of the launcher background.
    /// </summary>
    /// <returns>
    /// A <see cref="LauncherBackgroundFlag"/> value representing the type (image, image sequence, video) and source (file, zip) of the background.
    /// </returns>
    [PreserveSig]
    LauncherBackgroundFlag GetBackgroundFlag();

    /// <summary>
    /// Gets the logo overlay's local path entries for the launcher.
    /// </summary>
    /// <returns>
    /// A pointer to the first <see cref="LauncherPathEntry"/> representing the logo overlay sprites path.
    /// </returns>
    unsafe LauncherPathEntry* GetLogoOverlayEntries();

    /*
    /// <summary>
    /// Gets the count of logo overlay entries.
    /// </summary>
    /// <returns>
    /// The number of logo overlay entries available.
    /// </returns>
    [PreserveSig]
    int GetLogoOverlayEntriesCount();
    */

    /// <summary>
    /// Frees the memory allocated for the logo overlay's local path entries.
    /// This method should be called when the entries are no longer needed.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the entries were successfully freed; <c>false</c> if they were already freed or failed to be freed.
    /// </returns>
    [return: MarshalAs(UnmanagedType.Bool)]
    bool FreeLogoOverlayEntries();
}
