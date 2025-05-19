using System.Runtime.InteropServices.Marshalling;
// ReSharper disable UnusedMember.Global

namespace Hi3Helper.Plugin.Core.Management.Api;

[GeneratedComClass]
public abstract partial class LauncherApiMediaBase : LauncherApiBase, ILauncherApiMedia
{
    public abstract LauncherBackgroundFlag GetBackgroundFlag();

    public abstract LauncherBackgroundFlag GetLogoFlag();

    public abstract nint GetBackgroundEntries();

    public abstract nint GetLogoOverlayEntries();
}
