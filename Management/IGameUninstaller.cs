using Hi3Helper.Plugin.Core.Utility;
using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

#pragma warning disable CA1816
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
public partial interface IGameUninstaller : IFree, IDisposable
{
    /// <summary>
    /// Perform uninstallation routine asynchronously.
    /// </summary>
    /// <param name="cancelToken">Cancel token for the async operation.</param>
    /// <returns>
    /// A pointer to the <see cref="ComAsyncResult"/> instance.<br/>
    /// The pointer must be passed to <see cref="ComAsyncExtension.WaitFromHandle(nint)"/> in order to await the async function.<br/>
    /// The function, however is not-returnable.
    /// </returns>
    nint UninstallAsync(in Guid cancelToken);

    #region DynamicInterfaceCastable Explicit Calls
    /// <inheritdoc/>
    void IDisposable.Dispose() => Free();
    #endregion
}
