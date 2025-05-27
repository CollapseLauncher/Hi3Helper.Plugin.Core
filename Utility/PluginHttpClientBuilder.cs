/*
 * This code was copied from Collapse Launcher's HttpClientBuilder
 * implementation with some features trimmed.
 * 
 * Source:
 * https://github.com/CollapseLauncher/Collapse/blob/main/CollapseLauncher/Classes/Helper/HttpClientBuilder.cs
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Security;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Sockets;
// ReSharper disable UnusedMember.Global
// ReSharper disable StringLiteralTypo

// ReSharper disable StaticMemberInGenericType

namespace Hi3Helper.Plugin.Core.Utility;

public class PluginHttpClientBuilder : PluginHttpClientBuilder<SocketsHttpHandler>;

public class PluginHttpClientBuilder<THandler> where THandler : HttpMessageHandler, new()
{
    private const int ExMaxConnectionsDefault = 32;
    private const double ExHttpTimeoutDefault = 90; // in Seconds

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
    private Version HttpProtocolVersion { get; set; } = HttpVersion.Version30;
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
            + $"CollapsePlugin/{SharedStatic.LibraryStandardVersion}-{(SharedStatic.IsDebug ? "Debug" : "Release")}";
    }

    public PluginHttpClientBuilder<THandler> SetMaxConnection(int maxConnections = ExMaxConnectionsDefault)
    {
        if (maxConnections < 2)
            maxConnections = 2;

        MaxConnections = maxConnections;
        return this;
    }

    public PluginHttpClientBuilder<THandler> SetAllowedDecompression(DecompressionMethods decompressionMethods = DecompressionMethods.All)
    {
        DecompressionMethod = decompressionMethods;
        return this;
    }

    public PluginHttpClientBuilder<THandler> AllowRedirections(bool allowRedirections = true)
    {
        IsAllowHttpRedirections = allowRedirections;
        return this;
    }

    public PluginHttpClientBuilder<THandler> SetAuthHeader(string authHeader)
    {
        if (!string.IsNullOrEmpty(authHeader)) HttpAuthHeader = authHeader;
        return this;
    }

    public PluginHttpClientBuilder<THandler> AllowCookies(bool allowCookies = true)
    {
        IsAllowHttpCookies = allowCookies;
        return this;
    }

    public PluginHttpClientBuilder<THandler> AllowUntrustedCert(bool allowUntrustedCert = false)
    {
        IsAllowUntrustedCert = allowUntrustedCert;
        return this;
    }

    public PluginHttpClientBuilder<THandler> SetHttpVersion(Version? version = null, HttpVersionPolicy versionPolicy = HttpVersionPolicy.RequestVersionOrLower)
    {
        if (version != null)
            HttpProtocolVersion = version;

        HttpProtocolVersionPolicy = versionPolicy;
        return this;
    }

    public PluginHttpClientBuilder<THandler> SetTimeout(double fromSeconds = ExHttpTimeoutDefault)
    {
        if (double.IsNaN(fromSeconds) || double.IsInfinity(fromSeconds))
            fromSeconds = ExHttpTimeoutDefault;

        return SetTimeout(TimeSpan.FromSeconds(fromSeconds));
    }

    public PluginHttpClientBuilder<THandler> SetTimeout(TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(ExHttpTimeoutDefault);
        HttpTimeout = timeout.Value;
        return this;
    }

    public PluginHttpClientBuilder<THandler> SetUserAgent(string? userAgent = null)
    {
        HttpUserAgent = userAgent;
        return this;
    }

    public PluginHttpClientBuilder<THandler> SetBaseUrl(string baseUrl)
    {
        Uri baseUri = new(baseUrl);
        return SetBaseUrl(baseUri);
    }

    public PluginHttpClientBuilder<THandler> SetBaseUrl(Uri baseUrl)
    {
        HttpBaseUri = baseUrl;
        return this;
    }

    public PluginHttpClientBuilder<THandler> AddHeader(string key, string? value)
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

    public HttpClient Create()
    {
        // Create the instance of the handler
        THandler handler = new();

        // Set the features of each handler
        if (typeof(THandler) == typeof(HttpClientHandler))
        {
            // Cast as HttpClientHandler
            if (handler is not HttpClientHandler httpClientHandler)
                throw new InvalidCastException("Cannot cast handler as HttpClientHandler");

            // Set the properties
            httpClientHandler.UseProxy = IsUseProxy || IsUseSystemProxy;
            httpClientHandler.MaxConnectionsPerServer = MaxConnections;
            httpClientHandler.AllowAutoRedirect = IsAllowHttpRedirections;
            httpClientHandler.UseCookies = IsAllowHttpCookies;
            httpClientHandler.AutomaticDecompression = DecompressionMethod;
            httpClientHandler.ClientCertificateOptions = ClientCertificateOption.Manual;

            // Toggle for allowing untrusted cert
            if (IsAllowUntrustedCert)
                httpClientHandler.ServerCertificateCustomValidationCallback = delegate { return true; };

            // Set if the external proxy is set
            if (!IsUseSystemProxy && ExternalProxy != null)
                httpClientHandler.Proxy = ExternalProxy;
        }
        else if (typeof(THandler) == typeof(SocketsHttpHandler))
        {
            // Cast as SocketsHttpHandler
            if (handler is not SocketsHttpHandler socketsHttpHandler)
                throw new InvalidCastException("Cannot cast handler as SocketsHttpHandler");

            // Set the properties
            socketsHttpHandler.UseProxy = IsUseProxy || IsUseSystemProxy;
            socketsHttpHandler.MaxConnectionsPerServer = MaxConnections;
            socketsHttpHandler.AllowAutoRedirect = IsAllowHttpRedirections;
            socketsHttpHandler.UseCookies = IsAllowHttpCookies;
            socketsHttpHandler.AutomaticDecompression = DecompressionMethod;
            socketsHttpHandler.EnableMultipleHttp2Connections = true;
            socketsHttpHandler.EnableMultipleHttp3Connections = true;

            // Toggle for allowing untrusted cert
            if (IsAllowUntrustedCert)
            {
                SslClientAuthenticationOptions sslOptions = new()
                {
                    RemoteCertificateValidationCallback = delegate { return true; }
                };
                socketsHttpHandler.SslOptions = sslOptions;
            }

            // Set if the external proxy is set
            if (!IsUseSystemProxy && ExternalProxy != null)
                socketsHttpHandler.Proxy = ExternalProxy;

            socketsHttpHandler.ConnectCallback = ExternalDnsConnectCallback;
        }
        else
        {
            throw new InvalidOperationException("Generic must be a member of HttpMessageHandler!");
        }

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
                SharedStatic.InstanceDnsResolverCallback(context.DnsEndPoint.Host, out string[] ipAddresses);
                IPAddress[] addresses = new IPAddress[ipAddresses.Length];
                for (int i = 0; i < ipAddresses.Length; i++)
                {
                    addresses[i] = IPAddress.Parse(ipAddresses[i]);
                }

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
    }
}
