using System.Runtime.InteropServices.Marshalling;
// ReSharper disable UnusedMember.Global

namespace Hi3Helper.Plugin.Core.Management.Api;

[GeneratedComClass]
public abstract partial class LauncherApiNewsBase : LauncherApiBase, ILauncherApiNews
{
    /// <inheritdoc/>
    public abstract void GetNewsEntries(out nint handle, out int count, out bool isDisposable, out bool isAllocated);

    /// <inheritdoc/>
    public abstract void GetCarouselEntries(out nint handle, out int count, out bool isDisposable, out bool isAllocated);

    /// <inheritdoc/>
    public abstract void GetSocialMediaEntries(out nint handle, out int count, out bool isDisposable, out bool isAllocated);
}
