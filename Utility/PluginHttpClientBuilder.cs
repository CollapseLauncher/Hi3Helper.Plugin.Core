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

public class PluginHttpClientBuilder
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

    public PluginHttpClientBuilder SetMaxConnection(int maxConnections = ExMaxConnectionsDefault)
    {
        if (maxConnections < 2)
            maxConnections = 2;

        MaxConnections = maxConnections;
        return this;
    }

    public PluginHttpClientBuilder SetAllowedDecompression(DecompressionMethods decompressionMethods = DecompressionMethods.All)
    {
        DecompressionMethod = decompressionMethods;
        return this;
    }

    public PluginHttpClientBuilder AllowRedirections(bool allowRedirections = true)
    {
        IsAllowHttpRedirections = allowRedirections;
        return this;
    }

    public PluginHttpClientBuilder SetAuthHeader(string authHeader)
    {
        if (!string.IsNullOrEmpty(authHeader)) HttpAuthHeader = authHeader;
        return this;
    }

    public PluginHttpClientBuilder AllowCookies(bool allowCookies = true)
    {
        IsAllowHttpCookies = allowCookies;
        return this;
    }

    public PluginHttpClientBuilder AllowUntrustedCert(bool allowUntrustedCert = false)
    {
        IsAllowUntrustedCert = allowUntrustedCert;
        return this;
    }

    public PluginHttpClientBuilder SetHttpVersion(Version? version = null, HttpVersionPolicy versionPolicy = HttpVersionPolicy.RequestVersionOrLower)
    {
        if (version != null)
            HttpProtocolVersion = version;

        HttpProtocolVersionPolicy = versionPolicy;
        return this;
    }

    public PluginHttpClientBuilder SetTimeout(double fromSeconds = ExHttpTimeoutDefault)
    {
        if (double.IsNaN(fromSeconds) || double.IsInfinity(fromSeconds))
            fromSeconds = ExHttpTimeoutDefault;

        return SetTimeout(TimeSpan.FromSeconds(fromSeconds));
    }

    public PluginHttpClientBuilder SetTimeout(TimeSpan? timeout = null)
    {
        timeout ??= TimeSpan.FromSeconds(ExHttpTimeoutDefault);
        HttpTimeout = timeout.Value;
        return this;
    }

    public PluginHttpClientBuilder SetUserAgent(string? userAgent = null)
    {
        HttpUserAgent = userAgent;
        return this;
    }

    public PluginHttpClientBuilder SetBaseUrl(string baseUrl)
    {
        Uri baseUri = new(baseUrl);
        return SetBaseUrl(baseUri);
    }

    public PluginHttpClientBuilder SetBaseUrl(Uri baseUrl)
    {
        HttpBaseUri = baseUrl;
        return this;
    }

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
