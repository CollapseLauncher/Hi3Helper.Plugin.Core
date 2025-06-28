using System.Runtime.InteropServices.Marshalling;
// ReSharper disable UnusedMember.Global

namespace Hi3Helper.Plugin.Core.Management.Api;

[GeneratedComClass]
public abstract partial class LauncherApiMediaBase : LauncherApiBase, ILauncherApiMedia
{
    public abstract void GetBackgroundFlag(out LauncherBackgroundFlag result);

    public abstract void GetLogoFlag(out LauncherBackgroundFlag result);

    public abstract void GetBackgroundEntries(out nint handle, out int count, out bool isDisposable, out bool isAllocated);

    public abstract void GetLogoOverlayEntries(out nint handle, out int count, out bool isDisposable, out bool isAllocated);

    public virtual void GetBackgroundSpriteFps(out float result) => result = 0f;
}
