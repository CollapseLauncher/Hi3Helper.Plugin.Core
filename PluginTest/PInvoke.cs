using Hi3Helper.Plugin.Core.Management;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace PluginTest
{
    internal        delegate void         PluginSetLoggerCallback(nint callback);
    internal        delegate void         PluginSetDnsResolverCallback(nint callback);
    internal unsafe delegate GameVersion* PluginGetPluginVersion();
    internal unsafe delegate void*        PluginGetPlugin();

    internal static partial class PInvoke
    {
        [LibraryImport("kernel32.dll", EntryPoint = "LoadLibraryW", StringMarshalling = StringMarshalling.Utf16, SetLastError = true)]
        public static partial nint LoadLibrary(string libName);

        [LibraryImport("kernel32.dll", EntryPoint = "GetProcAddress", StringMarshalling = StringMarshalling.Utf8, SetLastError = true)]
        public static partial nint GetProcAddress(nint handle, string entryPoint);

        [LibraryImport("kernel32.dll", EntryPoint = "FreeLibrary")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static partial bool FreeLibrary(nint handle);

        public static bool TryLoadLibrary(string libName, out int err, out nint handle)
        {
            err = 0;
            handle = LoadLibrary(libName);
            if (handle != nint.Zero)
            {
                return true;
            }

            err = Marshal.GetLastWin32Error();
            Console.Error.WriteLine("Error while loading library: {0}\r\nWin32 Error Code: {1} ({2})",
                libName,
                err,
                Marshal.GetPInvokeErrorMessage(err));
            return false;
        }

        public static bool TryGetProcAddress<T>(nint handle, string exportName, out int err, out T delegateOut)
        {
            Unsafe.SkipInit(out delegateOut);

            err = 0;
            nint procAddress = GetProcAddress(handle, exportName);
            if (procAddress != nint.Zero)
            {
                delegateOut = Marshal.GetDelegateForFunctionPointer<T>(procAddress);
                return true;
            }

            err = Marshal.GetLastWin32Error();
            Console.Error.WriteLine("    Error while trying to get export: {0}\r\nWin32 Error Code: {1} ({2})",
                exportName,
                err,
                Marshal.GetPInvokeErrorMessage(err));
            return false;
        }
    }
}
