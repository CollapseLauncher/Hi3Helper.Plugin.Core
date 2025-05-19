using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hi3Helper.Plugin.Core.Management.Api;

public interface ILauncherApiNews : ILauncherApi
{
    /// <summary>
    /// Get the entries of the news data.
    /// </summary>
    /// <returns>
    /// A pointer to the first <see cref="LauncherNewsEntry"/> representing the news data entries.
    /// </returns>
    nint GetNewsEntries();
    nint GetCarouselEntries();
    nint GetSocialMediaEntries();
}
