using Hi3Helper.Plugin.Core;
using System;
using System.IO;
using System.Runtime.InteropServices.Marshalling;
using Microsoft.Extensions.Logging;
using Hi3Helper.Plugin.Core.Management;

#pragma warning disable CA2253
namespace PluginTest
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintHelp();
            }

            foreach (var arg in args)
            {
                if (TryPerformLibraryTest(arg, out int errorCode)) continue;
                Console.WriteLine($"An error has occurred with exit code: {errorCode}");
                return errorCode;
            }

            Console.WriteLine("All passed!");
            return 0;
        }

        private static bool TryPerformLibraryTest(string libraryPath, out int errorCode)
        {
            Console.WriteLine($"Performing test for library: {libraryPath}");

            nint libraryHandle = nint.Zero;
            try
            {
                if (!PInvoke.TryLoadLibrary(libraryPath, out errorCode, out libraryHandle))
                {
                    return false;
                }

                return LogInvokeTest<PluginGetPluginVersion>(libraryHandle, "GetPluginStandardVersion", out errorCode, Test.TestGetPluginStandardVersion) && 
                       LogInvokeTest<PluginGetPluginVersion>(libraryHandle, "GetPluginVersion", out errorCode, Test.TestGetPluginVersion) &&
                       LogInvokeTest<PluginGetPlugin>(libraryHandle, "GetPlugin", out errorCode, Test.TestGetPlugin);
            }
            finally
            {
                if (nint.Zero != libraryHandle)
                {
                    PInvoke.FreeLibrary(libraryHandle);
                }
            }
        }

        private static readonly InvokeLogger InvokeLogger = new();

        private static bool LogInvokeTest<T>(nint libraryHandle, string entryPointName, out int errorCode, Action<T, ILogger> resultDelegate)
        {
            errorCode = 0;
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

        static void PrintHelp()
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
