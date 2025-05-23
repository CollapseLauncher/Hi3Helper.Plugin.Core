using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
// ReSharper disable UnusedMember.Global

namespace Hi3Helper.Plugin.Core.Management.Api;

[GeneratedComClass]
public abstract partial class LauncherApiNewsBase : LauncherApiBase, ILauncherApiNews
{
    public abstract bool GetNewsEntries(out nint handle, out int count, out bool isDisposable);
    public abstract bool GetCarouselEntries(out nint handle, out int count, out bool isDisposable);
    public abstract bool GetSocialMediaEntries(out nint handle, out int count, out bool isDisposable);
}
