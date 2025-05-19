using System.Runtime.InteropServices.Marshalling;
// ReSharper disable UnusedMember.Global

namespace Hi3Helper.Plugin.Core.Management.Api;

[GeneratedComClass]
public abstract partial class LauncherApiNewsBase : LauncherApiBase, ILauncherApiNews
{
    public abstract nint GetNewsEntries();
    public abstract nint GetCarouselEntries();
    public abstract nint GetSocialMediaEntries();
}
