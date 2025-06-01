using System.Runtime.InteropServices.Marshalling;
// ReSharper disable UnusedMember.Global

namespace Hi3Helper.Plugin.Core.Management.Api;

[GeneratedComClass]
public abstract partial class LauncherApiMediaBase : LauncherApiBase, ILauncherApiMedia
{
    public abstract LauncherBackgroundFlag GetBackgroundFlag();

    public abstract LauncherBackgroundFlag GetLogoFlag();

    public abstract bool GetBackgroundEntries(out nint handle, out int count, out bool isDisposable);

    public abstract bool GetLogoOverlayEntries(out nint handle, out int count, out bool isDisposable);

    public virtual float GetBackgroundSpriteFps() => 0f;
}
