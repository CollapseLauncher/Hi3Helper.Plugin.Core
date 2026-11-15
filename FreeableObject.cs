using System.Runtime.InteropServices.Marshalling;

namespace Hi3Helper.Plugin.Core;

/// <summary>
/// A base class for free-able objects.
/// </summary>
[GeneratedComClass]
public abstract partial class FreeableObject : IFree
{
    /// <summary>
    /// Whether the object has been disposed/freed.
    /// </summary>
    protected bool IsFreed;

    /// <summary>
    /// Free the resources associated by an instance.
    /// </summary>
    public abstract void Free();
}
