using Hi3Helper.Plugin.Core;
using System;

namespace PluginTest;

internal static partial class Test
{
    internal static IPlugin? PluginInterface;
    internal static Guid     CancelToken = Guid.CreateVersion7();
}
