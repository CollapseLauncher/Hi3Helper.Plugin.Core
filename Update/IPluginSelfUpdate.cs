using Hi3Helper.Plugin.Core.Management;
using Hi3Helper.Plugin.Core.Utility;
using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

#pragma warning disable CA1816
namespace Hi3Helper.Plugin.Core.Update;

/// <summary>
/// An interface which provides a method to perform plugin self-update.
/// </summary>
[GeneratedComInterface]
[Guid(ComInterfaceId.ExPluginSelfUpdate)]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public partial interface IPluginSelfUpdate : IFree, IDisposable
{
    /// <summary>
    /// Asynchronously perform update on the plugin.
    /// </summary>
    /// <param name="outputDir">An output directory which the new version of library will be downloaded to.</param>
    /// <param name="checkForUpdatesOnly">Whether to perform the check only or also to perform the update at the same time.</param>
    /// <param name="progressDelegate">A delegate which pass <see cref="InstallProgress"/> to indicate update progress into the callback.</param>
    /// <param name="cancelToken"><see cref="Guid"/> instance for cancellation token.</param>
    /// <param name="result">A pointer to <see cref="ComAsyncResult"/>.</param>
    /// <remarks>
    /// This method returns a pointer to <see cref="ComAsyncResult"/> via <paramref name="result"/>. This method has a return value of the pointer of <see cref="SelfUpdateReturnInfo"/>.<br/>
    /// Please use <see cref="ComAsyncExtension.AsTask{T}(nint)"/> to get the return value.<br/><br/>
    /// 
    /// While <paramref name="checkForUpdatesOnly"/> is set to <c>true</c>, the status will only contain <see cref="SelfUpdateReturnCode.UpdateIsAvailable"/> or <see cref="SelfUpdateReturnCode.NoAvailableUpdate"/> inside of <see cref="SelfUpdateReturnInfo.ReturnCode"/> and the update won't be performed.
    /// </remarks>
    void TryPerformUpdateAsync([MarshalAs(UnmanagedType.LPWStr)] string? outputDir, [MarshalAs(UnmanagedType.Bool)] bool checkForUpdatesOnly, InstallProgressDelegate? progressDelegate, in Guid cancelToken, out nint result);
}
