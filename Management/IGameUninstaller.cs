using Hi3Helper.Plugin.Core.Utility;
using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Hi3Helper.Plugin.Core.Management;

/// <summary>
/// Defines a method which handles the game's uninstallation process.
/// </summary>
/// <remarks>
/// This interface is intended to perform uninstallation process.<br/>
/// All methods included within this interface are mostly asynchronous which requires awaiting via <see cref="ComAsyncResult"/>.
/// </remarks>
[GeneratedComInterface]
[Guid(ComInterfaceId.ExGameUninstaller)]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public partial interface IGameUninstaller : IInitializableTask
{
    /// <summary>
    /// Perform uninstallation routine asynchronously.
    /// </summary>
    /// <param name="cancelToken">Cancel token for the async operation.</param>
    /// <param name="result">A pointer to the <see cref="ComAsyncResult"/> instance.</param>
    /// <remarks>
    /// A pointer to the <see cref="ComAsyncResult"/> instance via <paramref name="result"/>.<br/>
    /// The pointer must be passed to <see cref="ComAsyncExtension.AsTask(nint)"/> in order to await the async function.<br/>
    /// The function, however is not-returnable.
    /// </remarks>
    void UninstallAsync(in Guid cancelToken, out nint result);
}
