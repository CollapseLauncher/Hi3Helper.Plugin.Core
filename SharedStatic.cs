using System;
using Hi3Helper.Plugin.Core.Management;
using Microsoft.Extensions.Logging;
#pragma warning disable CA2211

namespace Hi3Helper.Plugin.Core;

public delegate void SharedLoggerCallback(LogLevel logLevel, EventId eventId, string message);
public delegate void SharedDnsResolverCallback(string hostname, out string[] ipAddresses);

public class SharedStatic
{
    public static IPlugin?                   ThisPluginInstance;
    public static GameVersion                LibraryStandardVersion = new(0, 1, 0, 0);
    public static ILogger?                   InstanceLogger         = new SharedLogger();
    public static SharedLoggerCallback?      InstanceLoggerCallback;
    public static SharedDnsResolverCallback? InstanceDnsResolverCallback;
    public static Uri?                       ProxyHost;
    public static string?                    ProxyUsername;
    public static string?                    ProxyPassword;
    public static string                     PluginLocaleCode = "en-us";
    public static bool                       IsDebug =
#if DEBUG
        true;
#else
        false;
#endif

    private class SharedLogger : ILogger
    {
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => InstanceLoggerCallback?.Invoke(logLevel, eventId, formatter(state, exception));

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    }

    public static void DisposePlugin()
    {
        ThisPluginInstance?.Dispose();
    }

    public static void SetPluginCurrentLocale(ReadOnlySpan<char> currentLocale)
    {
        if (currentLocale.IsEmpty)
        {
            return;
        }

        Span<Range> range = stackalloc Range[2];
        int len = currentLocale.SplitAny(range, "-_", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        if (len is < 1 or > 2 ||
            currentLocale[range[0]].Length is < 2 or > 3 ||
            currentLocale[range[1]].Length is < 2 or > 3)
        {
            InstanceLogger?.LogWarning("Locale string: {Locale} is not a valid Locale ID format!", currentLocale.ToString());
            return;
        }

        PluginLocaleCode = currentLocale.ToString();
        InstanceLogger?.LogTrace("Locale ID has been set to: {Locale}", PluginLocaleCode);
    }
}
