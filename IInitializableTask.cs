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
public partial interface IInitializableTask : IFree
{
    /// <summary>
    /// Asynchronously initializes the instance.
    /// </summary>
    /// <param name="cancelToken"><see cref="Guid"/> instance for cancellation token.</param>
    /// <param name="result">A pointer to <see cref="ComAsyncResult"/>.</param>
    /// <remarks>
    /// This method returns a pointer to <see cref="ComAsyncResult"/> via <c>out</c> <paramref name="result"/>. This method has Return value of <see cref="int"/>.<br/>
    /// Please use <see cref="ComAsyncExtension.AsTask{T}(nint)"/> to get the return value.
    /// </remarks>
    void InitAsync(in Guid cancelToken, out nint result);
}
