using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Hi3Helper.Plugin.Core.Management.Api;

/// <summary>
/// Defines the contract for managing launcher news feed, such as social media details, carousel images, and news entries.
/// </summary>
/// <remarks>
/// <para>
/// This interface inherits from <see cref="IInitializableTask"/>, requiring implementers to provide asynchronous initialization logic.
/// </para>
/// </remarks>
[GeneratedComInterface]
[Guid(ComInterfaceId.ExLauncherApiNewsFeed)]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
// ReSharper disable once RedundantUnsafeContext
public unsafe partial interface ILauncherApiNews : ILauncherApi
{
    /// <summary>
    /// Get the news entries fot the launcher.<br/>
    /// This method returns a handle to the <see cref="PluginDisposableMemory{T}"/> of <see cref="LauncherNewsEntry"/>.<br/>
    /// Pass this method to <see cref="PluginDisposableMemoryExtension.ToManagedSpan{T}(PluginDisposableMemoryExtension.MarshalToMemorySelectorDelegate)"/> to get the span.
    /// </summary>
    /// <param name="handle">The handle to the pointer of the <see cref="LauncherNewsEntry"/> data</param>
    /// <param name="count">How much data of <see cref="LauncherNewsEntry"/> inside of the handle</param>
    /// <param name="isDisposable">Whether the handle is disposable</param>
    /// <param name="isAllocated"><c>true</c> if it's not empty. Otherwise, <c>false</c></param>
    void GetNewsEntries(out nint handle, out int count, [MarshalAs(UnmanagedType.Bool)] out bool isDisposable, [MarshalAs(UnmanagedType.Bool)] out bool isAllocated);

    /// <summary>
    /// Get the carousel image entries for the launcher.<br/>
    /// This method returns a handle to the <see cref="PluginDisposableMemory{T}"/> of <see cref="LauncherCarouselEntry"/>.<br/>
    /// Pass this method to <see cref="PluginDisposableMemoryExtension.ToManagedSpan{T}(PluginDisposableMemoryExtension.MarshalToMemorySelectorDelegate)"/> to get the span.
    /// </summary>
    /// <param name="handle">The handle to the pointer of the <see cref="LauncherCarouselEntry"/> data</param>
    /// <param name="count">How much data of <see cref="LauncherCarouselEntry"/> inside of the handle</param>
    /// <param name="isDisposable">Whether the handle is disposable</param>
    /// <param name="isAllocated"><c>true</c> if it's not empty. Otherwise, <c>false</c></param>
    void GetCarouselEntries(out nint handle, out int count, [MarshalAs(UnmanagedType.Bool)] out bool isDisposable, [MarshalAs(UnmanagedType.Bool)] out bool isAllocated);

    /// <summary>
    /// Get the social media info entries for the launcher.<br/>
    /// This method returns a handle to the <see cref="PluginDisposableMemory{T}"/> of <see cref="LauncherSocialMediaEntry"/>.<br/>
    /// Pass this method to <see cref="PluginDisposableMemoryExtension.ToManagedSpan{T}(PluginDisposableMemoryExtension.MarshalToMemorySelectorDelegate)"/> to get the span.
    /// </summary>
    /// <param name="handle">The handle to the pointer of the <see cref="LauncherSocialMediaEntry"/> data</param>
    /// <param name="count">How much data of <see cref="LauncherSocialMediaEntry"/> inside of the handle</param>
    /// <param name="isDisposable">Whether the handle is disposable</param>
    /// <param name="isAllocated"><c>true</c> if it's not empty. Otherwise, <c>false</c></param>
    void GetSocialMediaEntries(out nint handle, out int count, [MarshalAs(UnmanagedType.Bool)] out bool isDisposable, [MarshalAs(UnmanagedType.Bool)] out bool isAllocated);
}
