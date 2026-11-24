using Microsoft.Extensions.Logging;
using System;

namespace PluginTest;

internal static partial class Test
{
    internal static unsafe void TestGetWorkingPathCallback(PluginGetStringCallback delegateIn, ILogger logger)
    {
        delegateIn(out nint ptr, out int length);
        if (length == 0)
        {
            logger.LogWarning("Working path cannot be obtained or is not implemented");
            return;
        }

        ReadOnlySpan<char> str = new((void*)ptr, length);
        logger.LogInformation("Getting Working path: {}", str.ToString());
    }
}
