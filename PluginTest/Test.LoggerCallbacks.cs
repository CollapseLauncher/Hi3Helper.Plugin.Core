using Hi3Helper.Plugin.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.InteropServices;

#pragma warning disable CS8500
namespace PluginTest;

internal static partial class Test
{
    internal static unsafe void TestSetLoggerCallback(PluginSetLoggerCallback delegateIn, ILogger logger)
    {
        nint pointerToCallback = Marshal.GetFunctionPointerForDelegate<SharedLoggerCallback>(PluginLoggerCallback);
        delegateIn(pointerToCallback);
        logger.LogInformation("Logger attached at address: 0x{DelegateSetLoggerCallback:x8}", pointerToCallback);
    }

    private static unsafe void PluginLoggerCallback(LogLevel* logLevel, EventId* eventId, char* messageBuffer, int messageLength)
    {
        LogLevel level = *logLevel;
        if (level == LogLevel.None)
        {
            return;
        }

        string message = new ReadOnlySpan<char>(messageBuffer, messageLength).ToString();

        switch (level)
        {
            case LogLevel.Trace:
                Program.InvokeLogger.LogTrace(*eventId, "[Plugin Log: TRACE] {message}", message);
                break;
            case LogLevel.Information:
                Program.InvokeLogger.LogInformation(*eventId, "[Plugin Log: INFO] {message}", message);
                break;
            case LogLevel.Debug:
                Program.InvokeLogger.LogDebug(*eventId, "[Plugin Log: DEBUG] {message}", message);
                break;
            case LogLevel.Warning:
                Program.InvokeLogger.LogWarning(*eventId, "[Plugin Log: WARNING] {message}", message);
                break;
            case LogLevel.Error:
                Program.InvokeLogger.LogError(*eventId, "[Plugin Log: ERROR] {message}", message);
                break;
            case LogLevel.Critical:
                Program.InvokeLogger.LogCritical(*eventId, "[Plugin Log: CRITICAL] {message}", message);
                break;
            case LogLevel.None:
            default:
                throw new ArgumentOutOfRangeException(nameof(logLevel), *logLevel, null);
        }
    }
}
