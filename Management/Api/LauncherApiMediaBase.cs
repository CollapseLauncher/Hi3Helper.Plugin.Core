using System.Runtime.InteropServices.Marshalling;
// ReSharper disable UnusedMember.Global

namespace Hi3Helper.Plugin.Core.Management.Api;

[GeneratedComClass]
public abstract partial class LauncherApiMediaBase : LauncherApiBase, ILauncherApiMedia
{
    /// <inheritdoc/>
    public abstract void GetBackgroundFlag(out LauncherBackgroundFlag result);

    /// <inheritdoc/>
    public abstract void GetLogoFlag(out LauncherBackgroundFlag result);

    /// <inheritdoc/>
    public abstract void GetBackgroundEntries(out nint handle, out int count, out bool isDisposable, out bool isAllocated);

    /// <inheritdoc/>
    public abstract void GetLogoOverlayEntries(out nint handle, out int count, out bool isDisposable, out bool isAllocated);

    /// <inheritdoc/>
    public virtual void GetBackgroundSpriteFps(out float result) => result = 0f;
}
