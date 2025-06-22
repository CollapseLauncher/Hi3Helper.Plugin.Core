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
    /// <param name="outputTempDir">An output directory which the new version of library will be downloaded to.</param>
    /// <param name="cancelToken"><see cref="Guid"/> instance for cancellation token.</param>
    /// <returns>
    /// A pointer to <see cref="ComAsyncResult"/>. This method has Return value of <see cref="SelfUpdateReturnCode"/>.<br/>
    /// Please use <see cref="ComAsyncExtension.WaitFromHandle{T}(nint)"/> to get the return value.
    /// </returns>
    nint TryPerformUpdateAsync([MarshalAs(UnmanagedType.LPWStr)] string? outputTempDir, in Guid cancelToken);

    #region DynamicInterfaceCastable Explicit Calls
    /// <inheritdoc/>
    void IDisposable.Dispose() => Free();
    #endregion
}
