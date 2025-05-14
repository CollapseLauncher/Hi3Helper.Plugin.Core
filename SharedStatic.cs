using Hi3Helper.Plugin.Core.Management;
using Microsoft.Extensions.Logging;
#pragma warning disable CA2211

namespace Hi3Helper.Plugin.Core
{
    public static class SharedStatic
    {
        public static GameVersion LibraryStandardVersion = new(0, 1, 0, 0);
        public static ILogger? InstanceLogger = null;
    }
}
