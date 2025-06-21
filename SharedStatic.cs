using Hi3Helper.Plugin.Core.Management;
using Hi3Helper.Plugin.Core.Utility;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Hi3Helper.Plugin.Core;

public delegate void SharedLoggerCallback(LogLevel logLevel, EventId eventId, string message);
public delegate void SharedDnsResolverCallback(string hostname, out string[] ipAddresses);

public class SharedStatic
{
    static unsafe SharedStatic()
    {
        TryRegisterApiExport<GetVersionPointerDelegate> ("GetPluginStandardVersion", GetPluginStandardVersion);
        TryRegisterApiExport<GetVersionPointerDelegate> ("GetPluginVersion",         GetPluginVersion);
        TryRegisterApiExport<GetUnknownPointerDelegate> ("GetPlugin",                GetPlugin);
        TryRegisterApiExport<SetCallbackPointerDelegate>("SetLoggerCallback",        SetLoggerCallback);
        TryRegisterApiExport<SetCallbackPointerDelegate>("SetDnsResolverCallback",   SetDnsResolverCallback);
        TryRegisterApiExport<VoidDelegate>              ("FreePlugin",               DisposePlugin);
    }

    #region Private Static Fields
    private static GameVersion _currentDllVersion = GameVersion.Empty;
    private static GameVersion _libraryStandardVersion = new(0, 1, 0, 0);
    private static IPlugin?    _thisPluginInstance;
    #endregion

    #region Public and Internal Static Fields
    internal static SharedLoggerCallback?      InstanceLoggerCallback;
    internal static SharedDnsResolverCallback? InstanceDnsResolverCallback;
    internal static Uri?                       ProxyHost;
    internal static string?                    ProxyUsername;
    internal static string?                    ProxyPassword;
    internal static string                     PluginLocaleCode = "en-us";

    public static readonly ILogger InstanceLogger = new SharedLogger();

#if DEBUG
    internal static bool IsDebug = true;
#else
    internal static bool IsDebug = false;
#endif
    #endregion

    #region API Exports
    private static readonly Dictionary<string, nint>                                     RegisteredApiExports       = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, nint>.AlternateLookup<ReadOnlySpan<char>> RegisteredApiExportsLookup = RegisteredApiExports.GetAlternateLookup<ReadOnlySpan<char>>();

    protected unsafe delegate GameVersion* GetVersionPointerDelegate();
    protected unsafe delegate void*        GetUnknownPointerDelegate();
    protected        delegate void         SetCallbackPointerDelegate(nint callbackP);
    protected        delegate void         VoidDelegate();

    private static unsafe GameVersion* GetPluginStandardVersion() => _libraryStandardVersion.AsPointer();

    private static unsafe GameVersion* GetPluginVersion()
    {
        if (_currentDllVersion != GameVersion.Empty)
        {
            return _currentDllVersion.AsPointer();
        }

        try
        {
            Assembly? currentAssembly = Assembly.GetExecutingAssembly();
            if (currentAssembly == null)
            {
                return _currentDllVersion.AsPointer();
            }

            string? dllName = currentAssembly.FullName;
            string dllPath = AppContext.BaseDirectory;
            if (string.IsNullOrEmpty(dllName))
            {
                return _currentDllVersion.AsPointer();
            }

            string fullPath = Path.Combine(dllPath, dllName);
            if (!File.Exists(fullPath))
            {
                return _currentDllVersion.AsPointer();
            }

            FileVersionInfo fileVersion = FileVersionInfo.GetVersionInfo(fullPath);
            if (fileVersion == null)
            {
                return _currentDllVersion.AsPointer();
            }

            string? versionString = fileVersion.FileVersion;
            if (GameVersion.TryParse(versionString, out var version))
            {
                _currentDllVersion = version;
            }

            return _currentDllVersion.AsPointer();
        }
        catch
        {
            return _currentDllVersion.AsPointer();
        }
    }

    private static unsafe void* GetPlugin() =>
        ComInterfaceMarshaller<IPlugin>.ConvertToUnmanaged(_thisPluginInstance);

    private static void SetLoggerCallback(nint loggerCallback)
    {
        if (loggerCallback == nint.Zero)
        {
            InstanceLogger.LogTrace("[Exports::SetLoggerCallback] Logger callback has been detached!");
            InstanceLoggerCallback = null;
            return;
        }

        InstanceLogger.LogTrace("[Exports::SetLoggerCallback] Logger callback has been attached to address: 0x{Ptr:x8}", loggerCallback);
        InstanceLoggerCallback = Marshal.GetDelegateForFunctionPointer<SharedLoggerCallback>(loggerCallback);
    }

    private static void SetDnsResolverCallback(nint dnsResolverCallback)
    {
        if (dnsResolverCallback == nint.Zero)
        {
            InstanceLogger.LogTrace("[Exports::SetDnsResolverCallback] DNS Resolver callback has been detached!");
            InstanceDnsResolverCallback = null;
            return;
        }

        InstanceLogger.LogTrace("[Exports::SetDnsResolverCallback] DNS Resolver callback has been attached to address: 0x{Ptr:x8}", dnsResolverCallback);
        InstanceDnsResolverCallback = Marshal.GetDelegateForFunctionPointer<SharedDnsResolverCallback>(dnsResolverCallback);
    }

    private static void DisposePlugin() => _thisPluginInstance?.Dispose();

    internal static void SetPluginCurrentLocale(ReadOnlySpan<char> currentLocale)
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
            InstanceLogger.LogWarning("Locale string: {Locale} is not a valid Locale ID format!", currentLocale.ToString());
            return;
        }

        PluginLocaleCode = currentLocale.ToString();
        InstanceLogger.LogTrace("Locale ID has been set to: {Locale}", PluginLocaleCode);
    }

    /// <summary>
    /// Specify which <see cref="IPlugin"/> instance to load and use in this plugin.
    /// </summary>
    /// <typeparam name="TPlugin">A member of COM Interface of <see cref="IPlugin"/>.</typeparam>
    protected static void Load<TPlugin>()
        where TPlugin : class, IPlugin, new()
    {
        if (_thisPluginInstance != null)
        {
            return;
        }

        TPlugin instance = new();
        _thisPluginInstance = instance;
    }

    /// <summary>
    /// Registers the API exports to the lookup table.
    /// </summary>
    /// <typeparam name="T">The type of the delegate of the function.</typeparam>
    /// <param name="apiExportName">The name of the key for the delegate pointer.</param>
    /// <param name="callback">The callback of the function.</param>
    /// <returns><c>True</c> if it's added. <c>False</c> if the same export name is already registered.</returns>
    public static bool TryRegisterApiExport<T>(string apiExportName, T callback)
        where T : notnull => RegisteredApiExports.TryAdd(apiExportName, Marshal.GetFunctionPointerForDelegate(callback));

    /// <summary>
    /// Retrieve the pointer of the API exports from the lookup table.
    /// </summary>
    /// <param name="apiExportName">The name of the key for the delegate pointer.</param>
    /// <param name="delegateP">The pointer to the delegated function.</param>
    /// <returns><c>0</c> if it's been registered. Otherwise, <see cref="int.MinValue"/> if not registered or if <see cref="apiExportName"/> is undefined/null.</returns>
    public static unsafe int TryGetApiExportPointer(char* apiExportName, void** delegateP)
    {
        *delegateP = null;

        if (apiExportName == null)
        {
            return int.MinValue;
        }

        ReadOnlySpan<char> exportKeyName = MemoryMarshal.CreateReadOnlySpanFromNullTerminated(apiExportName);
        if (!RegisteredApiExportsLookup.TryGetValue(exportKeyName, out nint delegatePSafe))
        {
            return int.MinValue;
        }

        *delegateP = (void*)delegatePSafe;
        return 0;
    }
    #endregion
}
