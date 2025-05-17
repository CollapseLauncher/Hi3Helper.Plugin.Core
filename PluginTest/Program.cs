using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

#pragma warning disable CA2253
namespace PluginTest
{
    internal class Program
    {
        private static async Task<int> Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintHelp();
            }

            foreach (var arg in args)
            {
                var result = await TryPerformLibraryTest(arg);

                if (result.IsSuccess)
                    continue;

                Console.WriteLine($"An error has occurred with exit code: {result.ErrorCode}");
                return result.ErrorCode;
            }

            Console.WriteLine("All passed!");
            return 0;
        }

        private static async ValueTask<(bool IsSuccess, int ErrorCode)> TryPerformLibraryTest(string libraryPath)
        {
            Console.WriteLine($"Performing test for library: {libraryPath}");

            nint libraryHandle = nint.Zero;
            try
            {
                if (!PInvoke.TryLoadLibrary(libraryPath, out int errorCode, out libraryHandle))
                {
                    return (false, errorCode);
                }

                bool isSetLoggerCallback = LogInvokeTest<PluginSetLoggerCallback>(libraryHandle, "SetLoggerCallback", out errorCode, Test.TestSetLoggerCallback);
                bool isGetPluginStandardVersion = LogInvokeTest<PluginGetPluginVersion>(libraryHandle, "GetPluginStandardVersion", out errorCode, Test.TestGetPluginStandardVersion);
                bool isGetPluginVersion = errorCode == 0 && LogInvokeTest<PluginGetPluginVersion>(libraryHandle, "GetPluginVersion", out errorCode, Test.TestGetPluginVersion);
                (bool isGetPlugin, errorCode) = await LogInvokeTestAsync<PluginGetPlugin>(libraryHandle, "GetPlugin", Test.TestGetPlugin);
                (bool isTestApiMedia, errorCode) = await LogInvokeTestAsync<PluginGetPlugin>(libraryHandle, "GetPlugin", Test.TestApiMedia);

                return (isSetLoggerCallback && isGetPluginStandardVersion && isGetPluginVersion && isGetPlugin && isTestApiMedia, errorCode);
            }
            finally
            {
                if (nint.Zero != libraryHandle)
                {
                    PInvoke.FreeLibrary(libraryHandle);
                }
            }
        }

        internal static readonly InvokeLogger InvokeLogger = new();

        private static bool LogInvokeTest<T>(nint libraryHandle, string entryPointName, out int errorCode, Action<T, ILogger> resultDelegate)
        {
            errorCode = int.MinValue;
            Console.WriteLine($"  [InvokeTest] {entryPointName}()");
            try
            {
                if (!PInvoke.TryGetProcAddress(libraryHandle, entryPointName, out errorCode, out T delegateOut))
                {
                    return false;
                }
                resultDelegate(delegateOut, InvokeLogger);
                Console.WriteLine("    Result OK!");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    Result Failed! (Exception: {ex.Message})");
                return false;
            }
        }

        private static async ValueTask<(bool IsSuccess, int ErrorCode)> LogInvokeTestAsync<T>(nint libraryHandle, string entryPointName, Func<T, ILogger, Task> resultDelegate)
        {
            Console.WriteLine($"  [InvokeTest] {entryPointName}()");
            try
            {
                if (!PInvoke.TryGetProcAddress(libraryHandle, entryPointName, out int errorCode, out T delegateOut))
                {
                    return (false, errorCode);
                }
                await resultDelegate(delegateOut, InvokeLogger);
                Console.WriteLine("    Result OK!");

                return (true, 0);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"    Result Failed! (Exception: {ex.Message})");
                return (false, int.MinValue);
            }
        }

        private static void PrintHelp()
            => Console.WriteLine($"Usage:\r\n{Path.GetFileName(Environment.ProcessPath)} Path_to_dll_1 Path_to_dll_2 ...");
    }

    public class InvokeLogger : ILogger
    {
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            string logString = formatter(state, exception);
            string messages = logString.AsSpan().TrimStart(" \t").ToString();

            int spaceLen = logString.Length - messages.Length;
            string spaces = spaceLen > 0 ? logString.AsSpan(0, spaceLen).ToString() : string.Empty;
            Console.WriteLine($"    {spaces}Evaluate: {messages}");
        }

        public bool IsEnabled(LogLevel logLevel) => true;

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    }
}
