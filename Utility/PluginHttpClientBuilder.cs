/*
 * This code was copied from Collapse Launcher's HttpClientBuilder
 * implementation with some features trimmed.
 * 
 * Source:
 * https://github.com/CollapseLauncher/Collapse/blob/main/CollapseLauncher/Classes/Helper/HttpClientBuilder.cs
 */

using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable UnusedMember.Global
// ReSharper disable StringLiteralTypo
// ReSharper disable StaticMemberInGenericType

namespace Hi3Helper.Plugin.Core.Utility;

/// <summary>
/// Creates an <see cref="HttpClient"/> based on the customizable settings.
/// </summary>
public class PluginHttpClientBuilder
{
    private const int    ExMaxConnectionsDefault     = 32;
    private const double ExHttpTimeoutDefault        = 90; // in Seconds
    private const int    ExDnsResolverWriteBufferLen = 512;

    private static readonly ArrayPool<char> DnsResolverWriteBufferPool = ArrayPool<char>.Shared;

    private static bool IsUseProxy => SharedStatic.ProxyHost != null;
    private static bool IsUseSystemProxy => true;
    private static WebProxy? ExternalProxy => SharedStatic.ProxyHost == null ?
        null :
        SharedStatic.ProxyPassword == null ?
            new WebProxy(SharedStatic.ProxyHost, true) :
            new WebProxy(SharedStatic.ProxyHost, true, null, new NetworkCredential(SharedStatic.ProxyUsername, SharedStatic.ProxyPassword));

    private bool IsAllowHttpRedirections { get; set; }
    private bool IsAllowHttpCookies { get; set; }
    private bool IsAllowUntrustedCert { get; set; }

    private int MaxConnections { get; set; } = ExMaxConnectionsDefault;
    private DecompressionMethods DecompressionMethod { get; set; } = DecompressionMethods.All;
#if MANUALCOM
    private Version HttpProtocolVersion { get; set; } = HttpVersion.Version20;
#else
    private Version HttpProtocolVersion { get; set; } = HttpVersion.Version30;
#endif
    private string? HttpUserAgent { get; set; } = GetDefaultUserAgent();
    private string? HttpAuthHeader { get; set; }
    private HttpVersionPolicy HttpProtocolVersionPolicy { get; set; } = HttpVersionPolicy.RequestVersionOrLower;
    private TimeSpan HttpTimeout { get; set; } = TimeSpan.FromSeconds(ExHttpTimeoutDefault);
    private Uri? HttpBaseUri { get; set; }
    private Dictionary<string, string?> HttpHeaders { get; } = new();

    private static string GetDefaultUserAgent()
    {
        Version operatingSystemVer = Environment.OSVersion.Version;

        return $"Mozilla/5.0 (Windows NT {operatingSystemVer}; Win64; x64) "
            + $"{RuntimeInformation.FrameworkDescription.Replace(' ', '/')} (KHTML, like Gecko) "
            + $"CollapsePlugin/{SharedStatic.CurrentPluginVersion}/{SharedStatic.LibraryStandardVersion}-{(SharedStatic.IsDebug ? "Debug" : "Release")}";
    }

    /// <summary>
    /// Sets the maximum connection to be established by the <see cref="HttpClient"/>.
    /// </summary>
    /// <remarks>
    /// Minimum: 2<br/>
    /// Default: <see cref="ExMaxConnectionsDefault"/>
    /// </remarks>
    /// <param name="maxConnections">The amount of connections to be handled.</param>
    public PluginHttpClientBuilder SetMaxConnection(int maxConnections = ExMaxConnectionsDefault)
    {
        if (maxConnections < 2)
            maxConnections = 2;

        MaxConnections = maxConnections;
        return this;
    }

    /// <summary>
    /// Sets the allowed decompression methods on the HTTP response.
    /// </summary>
    /// <remarks>
    /// Default: <see cref="DecompressionMethods.All"/>
    /// </remarks>
    /// <param name="decompressionMethods">Kind of decompressors to be used.</param>
    public PluginHttpClientBuilder SetAllowedDecompression(DecompressionMethods decompressionMethods = DecompressionMethods.All)
    {
        DecompressionMethod = decompressionMethods;
        return this;
    }

    /// <summary>
    /// Allows the redirection (30x) HTTP status code or not.
    /// </summary>
    /// <remarks>
    /// Default: <c>true</c>
    /// </remarks>
    /// <param name="allowRedirections">Either allow the redirection or not.</param>
    public PluginHttpClientBuilder AllowRedirections(bool allowRedirections = true)
    {
        IsAllowHttpRedirections = allowRedirections;
        return this;
    }

    /// <summary>
    /// Sets the value of <c>Authorization</c> HTTP header.
    /// </summary>
    /// <remarks>
    /// Default: <c>null</c><br/>
    /// If the <paramref name="authHeader"/> is set to <c>null</c>, the <c>Authorization</c> HTTP header will be unset.
    /// </remarks>
    /// <param name="authHeader"></param>
    /// <returns></returns>
    public PluginHttpClientBuilder SetAuthHeader(string? authHeader = null)
    {
        HttpAuthHeader = authHeader;
        return this;
    }

    /// <summary>
    /// Allows cookies to be used on the <see cref="HttpClient"/> or not.
    /// </summary>
    /// <remarks>
    /// Default: <c>true</c>
    /// </remarks>
    /// <param name="allowCookies">Either allow cookies or not.</param>
    public PluginHttpClientBuilder AllowCookies(bool allowCookies = true)
    {
        IsAllowHttpCookies = allowCookies;
        return this;
    }

    /// <summary>
    /// Allows <see cref="HttpClient"/> to skip untrusted/unvalidated certificate validation on each of the request.
    /// </summary>
    /// <remarks>
    /// Default: <c>false</c>
    /// </remarks>
    /// <param name="allowUntrustedCert">Either to allow <see cref="HttpClient"/> to skip certificate validation or not.</param>
    public PluginHttpClientBuilder AllowUntrustedCert(bool allowUntrustedCert = true)
    {
        IsAllowUntrustedCert = allowUntrustedCert;
        return this;
    }

    // ReSharper disable once CommentTypo
    /// <summary>
    /// Sets the policy of which version of HTTP to be used.
    /// </summary>
    /// <remarks>
    /// Default <paramref name="version"/>: <c>null (or <see cref="HttpVersion.Version11"/>)</c><br/>
    /// Default <paramref name="versionPolicy"/>: <see cref="HttpVersionPolicy.RequestVersionOrLower"/><br/><br/>
    /// If the <paramref name="version"/> is set to <c>null</c>, the <see cref="HttpVersion.Version11"/> will be used instead.<br/>
    /// If <c>MANUALCOM</c> constant is defined and <paramref name="version"/> set to <see cref="HttpVersion.Version30"/>, it will be reverted to <see cref="HttpVersion.Version20"/> (due to unsupported features).
    /// </remarks>
    /// <param name="version">What HTTP version to be requested.</param>
    /// <param name="versionPolicy">The version request policy used.</param>
    public PluginHttpClientBuilder SetHttpVersion(Version? version = null, HttpVersionPolicy versionPolicy = HttpVersionPolicy.RequestVersionOrLower)
    {
        version ??= HttpVersion.Version11;

#if MANUALCOM
        if (version.Major == 3)
        {
            version = HttpVersion.Version20;
        }
#endif

        HttpProtocolVersion       = version;
        HttpProtocolVersionPolicy = versionPolicy;
        return this;
    }

    /// <summary>
    /// Sets how long the request will be allowed to be performed until certain timeout duration (in seconds).
    /// </summary>
    /// <remarks>
    /// Default: <see cref="ExHttpTimeoutDefault"/>
    /// </remarks>
    /// <param name="fromSeconds">How many seconds before timeout is happening.</param>
    public PluginHttpClientBuilder SetTimeout(double fromSeconds = ExHttpTimeoutDefault)
    {
        if (double.IsNaN(fromSeconds) || double.IsInfinity(fromSeconds))
            fromSeconds = ExHttpTimeoutDefault;

        return SetTimeout(TimeSpan.FromSeconds(fromSeconds));
    }

    /// <summary>
    /// Sets how long the request will be allowed to be performed until certain timeout duration (in <see cref="TimeSpan"/>).
    /// </summary>
    /// <remarks>
    /// Default: <see cref="TimeSpan"/>.FromSeconds(<see cref="ExHttpTimeoutDefault"/>)
    /// </remarks>
    /// <param name="timeout">The amount of time span used before timeout is happening.</param>
    public PluginHttpClientBuilder SetTimeout(TimeSpan? timeout = null)
    {
        HttpTimeout = timeout ?? TimeSpan.FromSeconds(ExHttpTimeoutDefault);
        return this;
    }

    /// <summary>
    /// Sets the user-agent used by the <see cref="HttpClient"/> on each of its requests.
    /// </summary>
    /// <remarks>
    /// Default: <c>null</c><br/>
    /// If the <paramref name="userAgent"/> is set to <c>null</c>, the default plugin user-agent will be used instead.
    /// </remarks>
    /// <param name="userAgent">The user-agent to be used.</param>
    public PluginHttpClientBuilder SetUserAgent(string? userAgent = null)
    {
        HttpUserAgent = userAgent;
        return this;
    }

    /// <summary>
    /// Sets the base URL used by the <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="baseUrl">The base URL used by the <see cref="HttpClient"/>.</param>
    public PluginHttpClientBuilder SetBaseUrl(string baseUrl) => SetBaseUrl(new Uri(baseUrl));

    /// <summary>
    /// Sets the base URL used by the <see cref="HttpClient"/>.
    /// </summary>
    /// <param name="baseUrl">The base URL used by the <see cref="HttpClient"/>.</param>
    public PluginHttpClientBuilder SetBaseUrl(Uri baseUrl)
    {
        HttpBaseUri = baseUrl;
        return this;
    }

    /// <summary>
    /// Adds or updates an HTTP header in the builder's configuration.
    /// </summary>
    /// <remarks>If the header key is "User-Agent" (case-insensitive), the value of the <see cref="HttpUserAgent"/> property will be overridden.<br/>
    /// If the specified header key already exists, its value will be updated; otherwise, the header will be added.
    /// </remarks>
    /// <param name="key">The name of the HTTP header to add or update. Cannot be null or empty.</param>
    /// <param name="value">The value of the HTTP header. Can be null to indicate the header should have no value.</param>
    public PluginHttpClientBuilder AddHeader(string key, string? value)
    {
        // Throw if the key is null or empty
        ArgumentException.ThrowIfNullOrEmpty(key, nameof(key));

        // Try check if the key is user-agent. If the user-agent has already
        // been set, then override the value from HttpUserAgent property
        if (key.Equals("User-Agent", StringComparison.OrdinalIgnoreCase))
        {
            HttpUserAgent = null;
        }

        // If the key already exist, then override the previous one.
        // Otherwise, add the new key-value pair
        // ReSharper disable once RedundantDictionaryContainsKeyBeforeAdding
        if (!HttpHeaders.TryAdd(key, value))
        {
            HttpHeaders[key] = value;
        }

        // Return the instance of the builder
        return this;
    }

    /// <summary>
    /// Creates and configures an instance of <see cref="HttpClient"/> with the specified settings.
    /// </summary>
    /// <remarks>This method initializes a new <see cref="HttpClient"/> using a <see
    /// cref="SocketsHttpHandler"/>  configured with various properties such as proxy settings, connection limits,
    /// decompression methods,  and HTTP protocol versions. It also applies custom headers, authentication settings, and
    /// other  options based on the provided configuration.  The returned <see cref="HttpClient"/> is ready for use and
    /// includes settings such as timeout,  base address, and user-agent headers. If the configuration allows untrusted
    /// certificates,  the handler is set to bypass certificate validation.</remarks>
    /// <returns>A fully configured <see cref="HttpClient"/> instance ready for making HTTP requests.</returns>
    public HttpClient Create()
    {
        // Create the instance of the handler
        SocketsHttpHandler handler = new();

        // Set the properties
        handler.UseProxy = IsUseProxy || IsUseSystemProxy;
        handler.MaxConnectionsPerServer = MaxConnections;
        handler.AllowAutoRedirect = IsAllowHttpRedirections;
        handler.UseCookies = IsAllowHttpCookies;
        handler.AutomaticDecompression = DecompressionMethod;
        handler.EnableMultipleHttp2Connections = true;
        handler.EnableMultipleHttp3Connections = true;

        // Toggle for allowing untrusted cert
        if (IsAllowUntrustedCert)
        {
            SslClientAuthenticationOptions sslOptions = new()
            {
                RemoteCertificateValidationCallback = delegate { return true; }
            };
            handler.SslOptions = sslOptions;
        }

        // Set if the external proxy is set
        if (!IsUseSystemProxy && ExternalProxy != null)
            handler.Proxy = ExternalProxy;

        handler.ConnectCallback = ExternalDnsConnectCallback;

        // Create the HttpClient instance
        HttpClient client = new(handler, false)
        {
            Timeout = HttpTimeout,
            DefaultRequestVersion = HttpProtocolVersion,
            DefaultVersionPolicy = HttpProtocolVersionPolicy,
            BaseAddress = HttpBaseUri,
            MaxResponseContentBufferSize = int.MaxValue
        };

        // Set User-agent
        if (!string.IsNullOrEmpty(HttpUserAgent))
            client.DefaultRequestHeaders.Add("User-Agent", HttpUserAgent);

        // Add Http Auth Header
        if (!string.IsNullOrEmpty(HttpAuthHeader))
            client.DefaultRequestHeaders.Add("Authorization", HttpAuthHeader);

        // Add other headers
        foreach (KeyValuePair<string, string?> header in HttpHeaders)
        {
            _ = client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
        }

        return client;
    }

    /// <summary>
    /// Resolves the IP addresses for the specified host using a callback function.
    /// </summary>
    /// <remarks>This method uses a callback function to perform DNS resolution. The callback must be set in 
    /// <see cref="SharedStatic.InstanceDnsResolverCallback"/> prior to calling this method. If the callback does not
    /// return any IP addresses, an <see cref="InvalidOperationException"/> is thrown.</remarks>
    /// <param name="host">The host name for which to resolve IP addresses. Cannot be <see langword="null"/> or empty.</param>
    /// <param name="ipAddresses">When the method returns, contains an array of IP addresses resolved for the specified host. If no IP addresses
    /// are resolved, an exception is thrown.</param>
    /// <exception cref="InvalidOperationException">Thrown if the callback does not return any IP addresses for the specified host.</exception>
    private static unsafe void GetDnsResolverArrayFromCallback(string host, out string[] ipAddresses)
    {
        Unsafe.SkipInit(out ipAddresses);

        // Throw if the callback is null
        ArgumentNullException.ThrowIfNull(SharedStatic.InstanceDnsResolverCallback, nameof(SharedStatic.InstanceDnsResolverCallback));

        char[] dnsResolverWriteBuffer = DnsResolverWriteBufferPool.Rent(ExDnsResolverWriteBufferLen);
        char* dnsResolverWriteBufferP = (char*)Marshal.UnsafeAddrOfPinnedArrayElement(dnsResolverWriteBuffer, 0);
        char* hostPAlloc = (char*)Utf16StringMarshaller.ConvertToUnmanaged(host);
        try
        {
            // Call the callback from main application to write the IP address into the buffer.
            int ipAddressWrittenCount = 0;
            SharedStatic.InstanceDnsResolverCallback(hostPAlloc, dnsResolverWriteBufferP, ExDnsResolverWriteBufferLen, &ipAddressWrittenCount);

            // SANITY
            if (ipAddressWrittenCount == 0)
            {
                throw new InvalidOperationException("DnsResolverCallback doesn't return any IP addresses!");
            }

            // Now we write the goods >:)
            ipAddresses = new string[ipAddressWrittenCount];
            int offset = 0;
            int index = 0;
            while (offset < ExDnsResolverWriteBufferLen && index < ipAddressWrittenCount && *(dnsResolverWriteBufferP + offset) != '\0')
            {
                // Use SIMD to get the index of null char as its length.
                char* currentOffset = dnsResolverWriteBufferP + offset;
                int len = SpanHelpers.IndexOfNullCharacter(currentOffset);
                if (len < 0)
                {
                    break;
                }

                ReadOnlySpan<char> currentCharEntry = new(currentOffset, len);
                ipAddresses[index++] = new string(currentCharEntry);

                // Advance to the next entry
                offset += len + 1;
            }

            // SAFETY CHECK:
            // If the processed string is less than what it reports by the main application, resize the ipAddresses array.
            if (index < ipAddressWrittenCount)
            {
                Array.Resize(ref ipAddresses, index);
            }
        }
        finally
        {
            Utf16StringMarshaller.Free((ushort*)hostPAlloc);
            DnsResolverWriteBufferPool.Return(dnsResolverWriteBuffer);
        }
    }

    /// <summary>
    /// Establishes a TCP connection to the specified DNS endpoint and returns a network stream.
    /// </summary>
    /// <remarks>This method attempts to connect to the DNS endpoint specified in the <paramref
    /// name="context"/>. If a custom DNS resolver callback is configured, it resolves the DNS host to a set of IP
    /// addresses and connects to one of them. Otherwise, it connects directly to the DNS endpoint.  The returned <see
    /// cref="Stream"/> is a <see cref="NetworkStream"/> that wraps the underlying socket. The socket is owned by the
    /// stream and will be disposed when the stream is disposed.</remarks>
    /// <param name="context">The <see cref="SocketsHttpConnectionContext"/> containing the DNS endpoint and other connection-related
    /// information.</param>
    /// <param name="token">A <see cref="CancellationToken"/> that can be used to cancel the connection attempt.</param>
    /// <returns>A <see cref="Stream"/> representing the network connection to the DNS endpoint.</returns>
    private static async ValueTask<Stream> ExternalDnsConnectCallback(SocketsHttpConnectionContext context, CancellationToken token)
    {
        Socket socket = new(SocketType.Stream, ProtocolType.Tcp)
        {
            NoDelay = true
        };

        try
        {
            if (SharedStatic.InstanceDnsResolverCallback != null)
            {
                IPAddress[] addresses = await GetCallbackResult();
                await socket.ConnectAsync(addresses, context.DnsEndPoint.Port, token);
                return new NetworkStream(socket, ownsSocket: true);
            }

            await socket.ConnectAsync(context.DnsEndPoint, token);
            return new NetworkStream(socket, ownsSocket: true);
        }
        catch
        {
            socket.Dispose();
            throw;
        }

        Task<IPAddress[]> GetCallbackResult()
            => Task.Factory.StartNew(() =>
                                     {
                                         GetDnsResolverArrayFromCallback(context.DnsEndPoint.Host, out string[] ipAddresses);
                                         IPAddress[] addressReturn = new IPAddress[ipAddresses.Length];
                                         for (int i = 0; i < ipAddresses.Length; i++)
                                         {
                                             addressReturn[i] = IPAddress.Parse(ipAddresses[i]);
                                         }

                                         return addressReturn;
                                     }, token);
    }
}
