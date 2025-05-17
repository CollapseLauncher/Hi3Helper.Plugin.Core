using System;
using Hi3Helper.Plugin.Core.Management;
using Microsoft.Extensions.Logging;
#pragma warning disable CA2211

namespace Hi3Helper.Plugin.Core;

public delegate void SharedLoggerCallback(LogLevel logLevel, EventId eventId, string message);

public class SharedStatic
{
    public static GameVersion           LibraryStandardVersion = new(0, 1, 0, 0);
    public static ILogger?              InstanceLogger         = new SharedLogger();
    public static SharedLoggerCallback? InstanceLoggerCallback;
    public static Uri?                  ProxyHost;
    public static string?               ProxyUsername;
    public static string?               ProxyPassword;
    public static bool                  IsDebug =
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
}
