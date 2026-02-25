# Utilities

This page covers the helper classes provided by `Hi3Helper.Plugin.Core` that you will use most frequently when writing a plugin.

## Table of contents

1. [PluginHttpClientBuilder](#1-pluginhttpclientbuilder)
2. [RetryableCopyToStreamTask](#2-retryablecopytostreamtask)
3. [SpeedLimiterService](#3-speedlimiterservice)
4. [PluginDisposableMemory\<T\>](#4-plugindisposablememoryt)

---

## 1. PluginHttpClientBuilder

`PluginHttpClientBuilder` is a fluent builder for `System.Net.Http.HttpClient`. Every `HttpClient` you create in a plugin **should** be created through this builder, because it:

- Automatically routes all connections through the launcher's **DNS resolver callback** (both synchronous and asynchronous) when one has been installed by the launcher via `SetDnsResolverCallback` / `SetDnsResolverCallbackAsync`.
- Applies the **proxy settings** pushed by the launcher via `IPlugin.SetPluginProxySettings`.
- Sets a plugin-specific default `User-Agent` header.
- Uses HTTP/3 by default (falls back to HTTP/2 in `MANUALCOM` mode).

### Basic usage

```csharp
using Hi3Helper.Plugin.Core.Utility;
using System.Net.Http;

// Minimal — all defaults
HttpClient client = new PluginHttpClientBuilder()
    .Create();

// Customised
HttpClient client = new PluginHttpClientBuilder()
    .SetBaseUrl("https://api.example.com/")
    .SetTimeout(30)                         // seconds
    .SetMaxConnection(16)
    .AllowRedirections()
    .AddHeader("X-My-Header", "value")
    .Create();
```

### Fluent API reference

| Method | Default | Description |
|--------|---------|-------------|
| `SetBaseUrl(string\|Uri)` | — | Sets `HttpClient.BaseAddress` |
| `SetTimeout(double seconds)` | 90 s | Request timeout |
| `SetMaxConnection(int)` | 32 | `SocketsHttpHandler.MaxConnectionsPerServer` |
| `SetHttpVersion(Version?, HttpVersionPolicy)` | HTTP/3 (HTTP/2 in MANUALCOM) | Negotiated HTTP protocol version |
| `SetAllowedDecompression(DecompressionMethods)` | `All` | Response decompression |
| `AllowRedirections(bool)` | `true` | Follow 3xx responses |
| `AllowCookies(bool)` | `true` | Cookie store |
| `AllowUntrustedCert(bool)` | `false` | Skip TLS certificate validation |
| `SetUserAgent(string?)` | Auto-generated | `User-Agent` header |
| `SetAuthHeader(string?)` | `null` | `Authorization` header |
| `AddHeader(string, string?)` | — | Arbitrary request header |
| **`Create()`** | — | Builds and returns the configured `HttpClient` |

> [!NOTE]
> The DNS resolver callback and proxy wiring are set up inside `Create()` automatically. You never need to configure them manually — just make sure the launcher has called `SetDnsResolverCallback` before the plugin starts making requests.

---

## 2. RetryableCopyToStreamTask

`RetryableCopyToStreamTask` runs a copy-to operation from a source `Stream` to a target `Stream` with automatic retry on network errors, per-read timeout, and optional speed throttling. It is ideal for all game download tasks.

### Creating a task

```csharp
using Hi3Helper.Plugin.Core.Utility;
using System.IO;
using System.Net.Http;

// Source factory — called again on each retry with the last successfully
// written byte position so the download can be resumed.
RetryableCopyToStreamTask.SourceStreamFactory factory = async (lastPosition, token) =>
{
    HttpRequestMessage request = new(HttpMethod.Get, "https://cdn.example.com/game.zip");
    if (lastPosition > 0)
        request.Headers.Range = new System.Net.Http.Headers.RangeHeaderValue(lastPosition, null);

    HttpResponseMessage response = await _httpClient.SendAsync(
        request, HttpCompletionOption.ResponseHeadersRead, token);
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadAsStreamAsync(token);
};

FileStream output = File.OpenWrite("game.zip");

RetryableCopyToStreamTask task = RetryableCopyToStreamTask.CreateTask(
    factory,
    output,
    new RetryableCopyToStreamTaskOptions
    {
        MaxBufferSize     = 65_536,  // 64 KiB read buffer
        MaxRetryCount     = 5,
        MaxTimeoutSeconds = 10,
        RetryDelaySeconds = 1,
        // Optionally wire in speed throttling (see section 3):
        SpeedLimiterServiceContext = SpeedLimiterService.CreateServiceContext()
    });

await task.StartTaskAsync(
    readDelegate: bytesRead => _bytesDownloaded += bytesRead,
    token: cancellationToken);
```

### `RetryableCopyToStreamTaskOptions` reference

| Property | Default | Description |
|----------|---------|-------------|
| `MaxBufferSize` | 4 096 bytes | Read-write buffer size (clamped: 1 KiB – 1 MiB) |
| `MaxRetryCount` | 5 | Maximum number of retry attempts |
| `MaxTimeoutSeconds` | 10 s | Per-read/write timeout (minimum 2 s) |
| `RetryDelaySeconds` | 1 s | Pause between retry attempts |
| `IsDisposeTargetStream` | `false` | Dispose the target stream when the task is disposed |
| `SpeedLimiterServiceContext` | `nint.Zero` | Handle from `SpeedLimiterService.CreateServiceContext()` |

---

## 3. SpeedLimiterService

`SpeedLimiterService` integrates with the launcher's `RegisterSpeedThrottlerService` export to enforce a per-session download speed cap. The launcher supplies the token-bucket parameters; the plugin calls `AddBytesOrWaitAsync` after each chunk to yield when the budget is exhausted.

### Usage

```csharp
using Hi3Helper.Plugin.Core.Utility;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

async Task DownloadAsync(Stream source, Stream dest, CancellationToken token)
{
    // Create one context per concurrent download stream.
    nint ctx = SpeedLimiterService.CreateServiceContext();

    byte[] buffer = new byte[65_536];
    int read;

    while ((read = await source.ReadAsync(buffer, token)) > 0)
    {
        await dest.WriteAsync(buffer.AsMemory(0, read), token);

        // Throttle: waits until the launcher's token bucket replenishes.
        await SpeedLimiterService.AddBytesOrWaitAsync(ctx, read, token);
    }
}
```

> [!NOTE]
> If the launcher has not called `RegisterSpeedThrottlerService`, `AddBytesOrWaitAsync` returns immediately without any delay, so the code works correctly in both throttled and unthrottled environments.
>
> `RetryableCopyToStreamTask` integrates `SpeedLimiterService` natively via `RetryableCopyToStreamTaskOptions.SpeedLimiterServiceContext`.

---

## 4. PluginDisposableMemory\<T\>

`PluginDisposableMemory<T>` is an unmanaged memory wrapper that can be passed across the COM boundary between the plugin and the launcher. It pairs a raw `T*` pointer with its length and a disposability flag, and exposes a managed `Span<T>` view.

### Allocating memory

```csharp
using Hi3Helper.Plugin.Core;
using Hi3Helper.Plugin.Core.Utility;

// Allocate 128 ints in unmanaged memory
using PluginDisposableMemory<int> memory = PluginDisposableMemory<int>.Allocate(128);

// Write via Span<T>
Span<int> span = memory.AsSpan();
for (int i = 0; i < span.Length; i++)
    span[i] = i * 2;

// Index access
ref int first = ref memory[0]; // ref-based, bounds-checked

// Stream view
using UnmanagedMemoryStream stream = memory.AsStream();
```

### API reference

| Member | Description |
|--------|-------------|
| `PluginDisposableMemory<T>.Allocate(int count)` | Allocates `count` elements of `T` in unmanaged memory |
| `PluginDisposableMemory<T>.Empty` | A zero-length non-disposable sentinel value |
| `Length` | Number of elements |
| `IsEmpty` | `true` when `Length == 0` |
| `this[int]` | Bounds-checked ref access to an element |
| `AsSpan(int offset, int length)` | Managed `Span<T>` view over a range |
| `AsPointer()` | Raw `T*` pointer |
| `AsSafePointer()` | `nint` representation of the pointer |
| `AsStream()` | `UnmanagedMemoryStream` over the entire buffer |
| `Dispose()` | Frees the unmanaged memory (only if `IsDisposable == true`) |
| `ForceDispose()` | Frees regardless of the disposability flag |

### Receiving memory from the launcher

The launcher may pass memory to the plugin via `PluginDisposableMemoryMarshal`. Use the extension method to convert it into a typed span:

```csharp
using Hi3Helper.Plugin.Core;

// 'marshal' comes from the launcher via COM
PluginDisposableMemory<byte> data = marshal.ToManagedSpan<byte>();
try
{
    // use data...
}
finally
{
    data.Dispose(); // frees the unmanaged pointer if the launcher marked it as disposable
}
```
