using Hi3Helper.Plugin.Core.Utility;
using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
// ReSharper disable IdentifierTypo

namespace Hi3Helper.Plugin.Core;

/// <summary>
/// An interface that defines the asynchronous initialization of the class member.
/// </summary>
[GeneratedComInterface]
[Guid(ComInterfaceId.ExInitializable)]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public partial interface IInitializableTask
{
    /// <summary>
    /// Asynchronously initializes the instance.
    /// </summary>
    /// <param name="cancelToken"><see cref="Guid"/> instance for cancellation token</param>
    /// <returns>
    /// A pointer to <see cref="ComAsyncResult"/>. This method has Return value of <see cref="int"/>.<br/>
    /// Please use <see cref="ComAsyncExtension.WaitFromHandle{T}(nint)"/> to get the return value.
    /// </returns>
    nint InitAsync(in Guid cancelToken);
}
