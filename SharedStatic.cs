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
/// <param name="ipResolvedWriteCount">[Out] How many IP address strings (with null terminator) are written into the buffer (<paramref name="ipResolvedWriteBuffer"/>).</param>
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
        // Plugin essential exports (based on v0.1 API Standard)
        // ---------------------------------------------------------------
        // These exports are required for the minimal plugin
        // functionalities in order run. Every plugin must have these
        // exports registered to comply the v0.1 API standard.

        // -> Plugin Versioning export
        TryRegisterApiExport<GetUnknownPointerDelegate>("GetPluginStandardVersion", GetPluginStandardVersion);
        TryRegisterApiExport<GetUnknownPointerDelegate>("GetPluginVersion", GetPluginVersion);
        // -> Plugin IPlugin Instance Getter export
        TryRegisterApiExport<GetUnknownPointerDelegate>("GetPlugin", GetPlugin);
        // -> Current's IPlugin Free export
        TryRegisterApiExport<VoidCallback>("FreePlugin", FreePlugin);
        // -> Plugin Debug Log Callback Setter export
        TryRegisterApiExport<SetCallbackPointerDelegate>("SetLoggerCallback", SetLoggerCallback);
        // -> Plugin DNS Resolver Callback Setter export
        TryRegisterApiExport<SetCallbackPointerDelegate>("SetDnsResolverCallback", SetDnsResolverCallback);

        // Plugin optional exports (based on v0.1-update1 API Standard)
        // ---------------------------------------------------------------
        // These exports are optional and can be removed if it's not
        // necesarilly used. These optional exports are included under
        // additional functionalities used as a subset of v0.1, which is
        // called "update1" feature sets.

        // -> Plugin Update CDN List Getter export
        TryRegisterApiExport<GetPluginUpdateCdnListDelegate>("GetPluginUpdateCdnList", GetPluginUpdateCdnList);
        // -> Plugin Async DNS Resolver Callback Setter export
        TryRegisterApiExport<SetCallbackPointerDelegate>("SetDnsResolverCallbackAsync", SetDnsResolverCallbackAsync);
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
    protected        delegate void  SetCallbackPointerDelegate(nint callbackP);

    /// <summary>
    /// Gets the array of CDN URLs used by the launcher to perform an update.
    /// </summary>
    /// <param name="count">[Out] The pointer to the count of the array.</param>
    /// <param name="ptr">[Out] The pointer to the array of the CDN URL strings.</param>
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

    /// <summary>
    /// Gets the pointer of the current API Standard version in which the plugin use.
    /// </summary>
    private static unsafe void* GetPluginStandardVersion()
    {
        fixed (void* ptr = &LibraryStandardVersion)
        {
            return ptr;
        }
    }

    /// <summary>
    /// Gets the pointer of the current plugin's version.<br/>
    /// If DebugNoReflection or ReleaseNoReflection build configuration is used, the pointer of <see cref="_currentDllVersion"/> will be returned.
    /// Otherwise, the plugin will auto-detect the version defined by the .csproj file (via <see cref="System.Reflection.Assembly"/>).
    /// </summary>
    private static unsafe void* GetPluginVersion()
    {
        try
        {
            Version? versionAssembly = _thisPluginInstance?.GetType().Assembly.GetName().Version;
            if (versionAssembly == null)
            {
                InstanceLogger.LogTrace("versionAssembly is null");
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

    /// <summary>
    /// Gets the COM interface pointer of the <see cref="IPlugin"/> instance.
    /// </summary>
    private static unsafe void* GetPlugin()
#if !MANUALCOM
        => ComInterfaceMarshaller<IPlugin>.ConvertToUnmanaged(_thisPluginInstance);
#else
        => ComWrappersExtension<PluginWrappers>.GetComInterfacePtrFromWrappers(_thisPluginInstance);
#endif

    /// <summary>
    /// Sets the pointer of the Debug Log callback/<see cref="SharedLoggerCallback"/> from the main application.
    /// </summary>
    /// <param name="loggerCallback">[In] A pointer to the Debug Log callback/<see cref="SharedLoggerCallback"/>.</param>
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

    /// <summary>
    /// Sets the pointer of the Asynchronous DNS Resolver callback/<see cref="SharedDnsResolverCallbackAsync"/>
    /// </summary>
    /// <param name="dnsResolverCallback">[In] A pointer to the Asynchronous DNS Resolver callback/<see cref="SharedDnsResolverCallbackAsync"/>.</param>
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

    /// <summary>
    /// Sets the pointer of the DNS Resolver callback/<see cref="SharedDnsResolverCallback"/>
    /// </summary>
    /// <param name="dnsResolverCallback">[In] A pointer to the DNS Resolver callback/<see cref="SharedDnsResolverCallback"/>.</param>
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

    /// <summary>
    /// Free the current instance of this plugin's <see cref="IPlugin"/>.
    /// </summary>
    private static void FreePlugin() => _thisPluginInstance?.Free();

    /// <summary>
    /// Sets the locale ID to use by the launcher. The format must be in IETF BCP-47 format with no region subtag.<br/>
    /// A valid format example as follows:<br/>
    ///   br-BR, id-ID, es-419, en-US
    /// </summary>
    /// <remarks>
    /// This method is used to set what language/locale ID is used to perform API request by the <see cref="Hi3Helper.Plugin.Core.Management.Api.ILauncherApi"/> members.<br/>
    /// The locale ID might be used to grab the data of the game news/launcher content based on user's current language.
    /// </remarks>
    /// <param name="currentLocale">The string represents the IETF BCP-47 non-region subtag locale ID.</param>
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
    /// <param name="apiExportName">[In] The name of the key for the delegate pointer.</param>
    /// <param name="delegateP">[Out] The pointer to the delegated function.</param>
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
