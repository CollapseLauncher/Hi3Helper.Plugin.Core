using Microsoft.Extensions.Logging;
using System;

#pragma warning disable CS8500
namespace Hi3Helper.Plugin.Core;

internal class SharedLogger : ILogger
{
    public unsafe void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        if (SharedStatic.InstanceLoggerCallback == null)
        {
            return;
        }

        if (exception != null)
        {
            string formattedWithExceptionString = formatter(state, exception) + "\r\n" + exception;
            fixed (char* formatterStrP = formattedWithExceptionString)
            {
                SharedStatic.InstanceLoggerCallback.Invoke(&logLevel, &eventId, formatterStrP, formattedWithExceptionString.Length);
            }
            return;
        }

        string formattedString = formatter(state, exception);
        fixed (char* formatterStrP = formattedString)
        {
            SharedStatic.InstanceLoggerCallback.Invoke(&logLevel, &eventId, formatterStrP, formattedString.Length);
        }
    }

    public bool IsEnabled(LogLevel logLevel) => true;

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
}
