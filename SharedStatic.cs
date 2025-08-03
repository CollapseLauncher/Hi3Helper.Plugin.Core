using Hi3Helper.Plugin.Core.Management;
using Hi3Helper.Plugin.Core.Utility;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Hi3Helper.Plugin.Core.Update;
// ReSharper disable CommentTypo

#if MANUALCOM
using Hi3Helper.Plugin.Core.ABI;
#endif

#pragma warning disable CS8500
namespace Hi3Helper.Plugin.Core;

/// <summary>
/// A delegate to a callback which prints the log from this plugin.
/// </summary>
/// <param name="logLevel">[In] The <see cref="LogLevel"/> provided by the plugin.</param>
/// <param name="eventId">[In] The information <see cref="EventId"/> provided by the plugin.</param>
/// <param name="messageBuffer">[In] The pointer of the message's UTF-16 unsigned string (without null terminator).</param>
/// <param name="messageLength">[In] The length of the message buffer.</param>
public unsafe delegate void SharedLoggerCallback(LogLevel* logLevel, EventId* eventId, char* messageBuffer, int messageLength);

/// <summary>
/// A delegate to a callback which returns a list of IP addresses resolved from the <paramref name="hostname"/>.
/// </summary>
/// <remarks>
/// DO NOT FREE THE POINTER GIVEN BY THIS DELEGATE! The pointers are borrowed and will be automatically cleared by the plugin.
/// </remarks>
/// <param name="hostname">[In] A hostname to resolve to.</param>
/// <param name="ipResolvedWriteBuffer">[Ref] A pointer to the buffer in which the main application will write the UTF-16 unsigned string (with null terminator) to.</param>
/// <param name="ipResolvedWriteBufferLength">[In] The length of the buffer that the main application able to write.</param>
/// <param name="ipResolvedWriteCount">[Out] How many IP address strings (with null terminator) are written into the buffer.</param>
public unsafe delegate void SharedDnsResolverCallback(char* hostname, char* ipResolvedWriteBuffer, int ipResolvedWriteBufferLength, int* ipResolvedWriteCount);

/// <summary>
/// A delegate to an asynchronous callback which returns an array pointer of <see cref="DnsARecordResult"/> struct for resolving DNS A/AAAA Record query.
/// </summary>
/// <param name="hostname">[In] A hostname to query the A/AAAA record to.</param>
/// <param name="cancelCallback">[Out] A callback in which invoked, will be cancelling the async operation of the resolver.</param>
/// <returns>A pointer to <see cref="ComAsyncResult"/> in which returns the pointer (<see cref="nint"/>) of the <see cref="DnsARecordResult"/> struct.</returns>
public unsafe delegate nint SharedDnsResolverCallbackAsync(char* hostname, int hostnameLength, void** cancelCallback);

/// <summary>
/// An action delegate. This is equivalent to <see cref="Action"/> delegate.
/// </summary>
public delegate void VoidCallback();

public class SharedStatic
{
    static unsafe SharedStatic()
    {
        TryRegisterApiExport<GetUnknownPointerDelegate>     ("GetPluginStandardVersion",    GetPluginStandardVersion);
        TryRegisterApiExport<GetUnknownPointerDelegate>     ("GetPluginVersion",            GetPluginVersion);
        TryRegisterApiExport<GetUnknownPointerDelegate>     ("GetPlugin",                   GetPlugin);
        TryRegisterApiExport<GetPluginUpdateCdnListDelegate>("GetPluginUpdateCdnList",      GetPluginUpdateCdnList);
        TryRegisterApiExport<SetCallbackPointerDelegate>    ("SetLoggerCallback",           SetLoggerCallback);
        TryRegisterApiExport<SetCallbackPointerDelegate>    ("SetDnsResolverCallback",      SetDnsResolverCallback);
        TryRegisterApiExport<SetCallbackPointerDelegate>    ("SetDnsResolverCallbackAsync", SetDnsResolverCallbackAsync);
        TryRegisterApiExport<VoidCallback>                  ("FreePlugin",                  DisposePlugin);
    }

    #region Private Static Fields
    private static GameVersion _currentDllVersion = GameVersion.Empty;
    private static IPlugin?    _thisPluginInstance;
    #endregion

    #region Public and Internal Static Fields
    internal static SharedLoggerCallback?           InstanceLoggerCallback;
    internal static SharedDnsResolverCallback?      InstanceDnsResolverCallback;
    internal static SharedDnsResolverCallbackAsync? InstanceDnsResolverCallbackAsync;
    internal static Uri?                            ProxyHost;
    internal static string?                         ProxyUsername;
    internal static string?                         ProxyPassword;
    internal static string                          PluginLocaleCode = "en-us";

    public static readonly GameVersion LibraryStandardVersion = new(0, 1, 1, 0);
    public static readonly ILogger     InstanceLogger         = new SharedLogger();

#if DEBUG
    internal static bool IsDebug = true;
#else
    internal static bool IsDebug = false;
#endif
    internal static unsafe GameVersion CurrentPluginVersion => *(GameVersion*)GetPluginVersion();
    #endregion

    #region API Exports
    private static readonly Dictionary<string, nint>                                     RegisteredApiExports       = new(StringComparer.OrdinalIgnoreCase);
    private static readonly Dictionary<string, nint>.AlternateLookup<ReadOnlySpan<char>> RegisteredApiExportsLookup = RegisteredApiExports.GetAlternateLookup<ReadOnlySpan<char>>();

    protected unsafe delegate void* GetUnknownPointerDelegate();
    protected unsafe delegate void  GetPluginUpdateCdnListDelegate(int* count, ushort*** ptr);
    protected delegate        void  SetCallbackPointerDelegate(nint     callbackP);

    private static unsafe void GetPluginUpdateCdnList(int* count, ushort*** ptr)
    {
        try
        {
            IPluginSelfUpdate? selfUpdateManager = null;
            _thisPluginInstance?.GetPluginSelfUpdater(out selfUpdateManager);

            if (selfUpdateManager is not PluginSelfUpdateBase asManagerBase)
            {
                *count = 0;
                *ptr = null;
                return;
            }

            ReadOnlySpan<string> urlSpan = asManagerBase.BaseCdnUrl;
            if (urlSpan.IsEmpty)
            {
                *count = 0;
                *ptr = null;
                return;
            }

            *count = urlSpan.Length;
            ushort** alloc = (ushort**)Mem.Alloc<nint>(urlSpan.Length);

            for (int i = 0; i < urlSpan.Length; i++)
            {
                string url = urlSpan[i];
                alloc[i] = Utf16StringMarshaller.ConvertToUnmanaged(url);
            }

            *ptr = alloc;
        }
        catch (Exception e)
        {
            *count = 0;
            *ptr = null;
            InstanceLogger.LogError(e, "Cannot obtain CDN List due to error: {ex}", e);
        }
    }

    private static unsafe void* GetPluginStandardVersion()
    {
        fixed (void* ptr = &LibraryStandardVersion)
        {
            return ptr;
        }
    }

    private static unsafe void* GetPluginVersion()
    {
        if (_currentDllVersion != GameVersion.Empty)
        {
            return _currentDllVersion.AsPointer();
        }

        try
        {
            Version? versionAssembly = _thisPluginInstance?.GetType().Assembly.GetName().Version;
            if (versionAssembly == null)
            {
                InstanceLogger.LogTrace("versionFromIPluginAssembly is null");
                return _currentDllVersion.AsPointer();
            }

            _currentDllVersion = new GameVersion(versionAssembly.Major, versionAssembly.Minor, versionAssembly.Build, versionAssembly.Revision);
            return _currentDllVersion.AsPointer();
        }
        catch
        {
            return _currentDllVersion.AsPointer();
        }
    }

    private static unsafe void* GetPlugin()
#if !MANUALCOM
        => ComInterfaceMarshaller<IPlugin>.ConvertToUnmanaged(_thisPluginInstance);
#else
        => ComWrappersExtension<PluginWrappers>.GetComInterfacePtrFromWrappers(_thisPluginInstance);
#endif

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

    private static void SetDnsResolverCallbackAsync(nint dnsResolverCallback)
    {
        if (dnsResolverCallback == nint.Zero)
        {
            InstanceLogger.LogTrace("[Exports::SetDnsResolverCallbackAsync] Asynchronous DNS Resolver callback has been detached!");
            InstanceDnsResolverCallbackAsync = null;
            return;
        }

        if (InstanceDnsResolverCallback != null)
        {
            InstanceLogger.LogWarning("[Exports::SetDnsResolverCallbackAsync] You already have synchronous DNS Resolver set! This async callback will be used and synchronous one will be ignored.");
        }

        InstanceLogger.LogTrace("[Exports::SetDnsResolverCallbackAsync] Asynchronous DNS Resolver callback has been attached to address: 0x{Ptr:x8}", dnsResolverCallback);
        InstanceDnsResolverCallbackAsync = Marshal.GetDelegateForFunctionPointer<SharedDnsResolverCallbackAsync>(dnsResolverCallback);
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

    private static void DisposePlugin() => _thisPluginInstance?.Free();

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
    protected static void Load<TPlugin>(GameVersion interceptDllVersionTo = default)
        where TPlugin : class, IPlugin, new()
    {
        if (interceptDllVersionTo != GameVersion.Empty)
        {
            _currentDllVersion = interceptDllVersionTo;
        }

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
    /// <returns><c>0</c> if it's been registered. Otherwise, <see cref="int.MinValue"/> if not registered or if <paramref name="apiExportName"/> is undefined/null.</returns>
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
