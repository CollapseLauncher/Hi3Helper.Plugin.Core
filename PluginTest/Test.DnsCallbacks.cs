using Hi3Helper.Plugin.Core;
using Microsoft.Extensions.Logging;
using System.Runtime.InteropServices;

namespace PluginTest;

internal static partial class Test
{
    internal static void TestSetDnsResolverCallback(PluginSetDnsResolverCallback delegateIn, ILogger logger)
    {
        nint pointerToCallback = Marshal.GetFunctionPointerForDelegate<SharedDnsResolverCallback>(PluginDnsResolverCallback);
        delegateIn(pointerToCallback);
        logger.LogInformation("DNS Resolver attached at address: 0x{DelegateSetDnsResolverCallback:x8}", pointerToCallback);
    }

    internal static void TestResetSetDnsResolverCallback(PluginSetDnsResolverCallback delegateIn, ILogger logger)
    {
        delegateIn(nint.Zero);
        logger.LogInformation("DNS Resolver has been detached!");
    }

    private static void PluginDnsResolverCallback(string host, out string[] ipAddresses)
    {
        ipAddresses = ["127.0.0.1"];
    }
}
