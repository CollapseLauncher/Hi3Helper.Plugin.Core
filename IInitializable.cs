using Hi3Helper.Plugin.Core.Utility;
using System.Runtime.InteropServices.Marshalling;
using System.Runtime.InteropServices;
using System;
// ReSharper disable IdentifierTypo

namespace Hi3Helper.Plugin.Core;

/// <summary>
/// An interface that defines the asynchronous initialization of the class member.
/// </summary>
[GeneratedComInterface]
[Guid(ComInterfaceId.Initializable)]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public partial interface IInitializable
{
    #region Return Callbacks
    public delegate void InitAsyncIsSuccessCallback(int result);
    #endregion

    /// <summary>
    /// Asynchronously initializes the instance.
    /// </summary>
    /// <remarks>
    /// You must call <see cref="ComAsyncExtension.WaitFromHandle(nint)"/> in order to await the method
    /// </remarks>
    /// <param name="cancelToken"><see cref="Guid"/> instance for cancellation token</param>
    /// <param name="isSuccessReturnCallback">A callback which pass the return value</param>
    /// <returns>A pointer to <see cref="ComAsyncResult"/></returns>
    nint InitAsync(in Guid cancelToken, InitAsyncIsSuccessCallback isSuccessReturnCallback);
}
