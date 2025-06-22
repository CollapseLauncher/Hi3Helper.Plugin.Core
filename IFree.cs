using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Hi3Helper.Plugin.Core;

/// <summary>
/// Implements a method which indicates the instance is free-able/can be disposed.
/// </summary>
[GeneratedComInterface]
[Guid(ComInterfaceId.ExFree)]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public partial interface IFree
{
    /// <summary>
    /// Free the resources associated by an instance.
    /// </summary>
    void Free();
}
