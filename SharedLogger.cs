using Microsoft.Extensions.Logging;
using System;

namespace Hi3Helper.Plugin.Core;

internal class SharedLogger : ILogger
{
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (exception != null)
        {
            SharedStatic.InstanceLoggerCallback?.Invoke(logLevel, eventId, formatter(state, exception) + "\r\n" + exception);
            return;
        }

        SharedStatic.InstanceLoggerCallback?.Invoke(logLevel, eventId, formatter(state, exception));
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
}
