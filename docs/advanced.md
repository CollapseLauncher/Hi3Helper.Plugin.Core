# Advanced Topics

## Table of contents

1. [DNS resolver callbacks](#1-dns-resolver-callbacks)
2. [Proxy settings](#2-proxy-settings)
3. [Discord Rich Presence](#3-discord-rich-presence)
4. [Multi-region plugins](#4-multi-region-plugins)

---

## 1. DNS resolver callbacks

The launcher can override how DNS resolution works in the plugin by installing a callback via one of two exports.

| Export | Callback type | Description |
|--------|--------------|-------------|
| `SetDnsResolverCallback` | `SharedDnsResolverCallback` | Synchronous resolver — the launcher writes resolved IP strings into a plugin-provided buffer |
| `SetDnsResolverCallbackAsync` | `SharedDnsResolverCallbackAsync` | Asynchronous resolver — returns a `ComAsyncResult` pointer; preferred when available |

**You do not need to wire these callbacks manually.** `PluginHttpClientBuilder.Create()` automatically hooks them into the `SocketsHttpHandler.ConnectCallback` of every `HttpClient` it produces. As long as you create all HTTP clients through `PluginHttpClientBuilder`, DNS override works transparently.

### How it works under the hood

When `PluginHttpClientBuilder.Create()` builds its `SocketsHttpHandler`, it sets `handler.ConnectCallback = ExternalDnsConnectCallback`. That callback:

1. Checks `SharedStatic.InstanceDnsResolverCallbackAsync` first (async preferred).
2. Falls back to `SharedStatic.InstanceDnsResolverCallback` (sync).
3. If neither is set, falls through to .NET's default DNS resolution.

The sync callback receives a flat UTF-16 write buffer (`char*`) and a count. The async callback returns a `nint` pointing to a `ComAsyncResult` whose result is a linked list of `DnsARecordResult` structs.

### Passing a custom resolver to non-HTTP sockets

If your plugin opens raw TCP sockets you can also call the resolver directly:

```csharp
using Hi3Helper.Plugin.Core;
using System.Net;

// Synchronous (only if InstanceDnsResolverCallback != null)
// PluginHttpClientBuilder exposes GetDnsResolverArrayFromCallback as private;
// roll your own by calling SharedStatic.InstanceDnsResolverCallback directly.

unsafe
{
    if (SharedStatic.InstanceDnsResolverCallback != null)
    {
        char[] buf = new char[512];
        int count  = 0;
        fixed (char* host = "api.example.com")
        fixed (char* bufP = buf)
            SharedStatic.InstanceDnsResolverCallback(host, bufP, buf.Length, &count);

        // buf now contains `count` null-terminated IP strings
    }
}
```

---

## 2. Proxy settings

The launcher calls `IPlugin.SetPluginProxySettings` to push proxy configuration (or to clear it). `PluginBase` already implements this method — it writes the values into the three static fields:

| Field | Type | Description |
|-------|------|-------------|
| `SharedStatic.ProxyHost` | `Uri?` | Parsed proxy URI, or `null` when disabled |
| `SharedStatic.ProxyUsername` | `string?` | Proxy username |
| `SharedStatic.ProxyPassword` | `string?` | Proxy password |

`PluginHttpClientBuilder` reads these fields inside `Create()` and sets them on the `SocketsHttpHandler`:

```csharp
handler.UseProxy = SharedStatic.ProxyHost != null || IsUseSystemProxy;
handler.Proxy    = SharedStatic.ProxyHost == null
    ? null
    : new WebProxy(SharedStatic.ProxyHost, true, null,
          new NetworkCredential(SharedStatic.ProxyUsername, SharedStatic.ProxyPassword));
```

**No action is required from the plugin author.** Every `HttpClient` you create through `PluginHttpClientBuilder` automatically honours the proxy that the launcher has configured, including live updates between plugin calls — because `PluginHttpClientBuilder.Create()` reads the fields at `HttpClient` creation time.

> [!WARNING]
> If you create `HttpClient` instances manually (e.g. `new HttpClient()`), they will **not** inherit the launcher's proxy or DNS settings. Always use `PluginHttpClientBuilder.Create()`.

---

## 3. Discord Rich Presence

The launcher can query a plugin for Discord Rich Presence details for the currently active game region. This is an **optional** feature gated behind the `v0.1-update2` extension export `GetCurrentDiscordPresenceInfo`.

### Enabling Discord RP in your plugin

Override `GetCurrentDiscordPresenceInfoCore` in your `SharedStaticV1Ext<T>` subclass:

```csharp
using Hi3Helper.Plugin.Core;
using Hi3Helper.Plugin.Core.DiscordPresence;
using Hi3Helper.Plugin.Core.Management.PresetConfig;
using System.Runtime.CompilerServices;

public partial class MyPluginExports : SharedStaticV1Ext<MyPluginExports>
{
    [ModuleInitializer]
    public static void Initialize() => Load<MyPlugin>();

    /// <summary>
    /// Called by the launcher to obtain Discord Rich Presence data for <paramref name="context"/>.
    /// Return false if the active region does not support Discord RP.
    /// </summary>
    protected override bool GetCurrentDiscordPresenceInfoCore(
        DiscordPresenceExtension.DiscordPresenceContext context,
        out ulong   presenceId,
        out string? largeIconUrl,
        out string? largeIconTooltip,
        out string? smallIconUrl,
        out string? smallIconTooltip)
    {
        // Use context.IsFeatureAvailable to check if the launcher already
        // supplied presence data from the preset config side.
        if (!context.IsFeatureAvailable)
        {
            presenceId       = 0;
            largeIconUrl     = null;
            largeIconTooltip = null;
            smallIconUrl     = null;
            smallIconTooltip = null;
            return false;
        }

        presenceId       = 1234567890123456789UL; // your Discord Application ID
        largeIconUrl     = "my_game_logo";  // asset name or full URL (max 256 chars)
        largeIconTooltip = "My Awesome Game";
        smallIconUrl     = "my_game_small"; // optional
        smallIconTooltip = "Playing now";
        return true;
    }
}
```

### How the launcher consumes it

The launcher calls the `GetCurrentDiscordPresenceInfo` export, which:

1. Unmarshals the `IPluginPresetConfig` pointer.
2. Calls `GetCurrentDiscordPresenceInfoCore` on your `SharedStaticV1Ext<T>` instance.
3. If `true` is returned, allocates a `DiscordPresenceInfo` struct in unmanaged memory and writes the pointer back to the launcher.
4. The launcher wraps it in a `DiscordPresenceContext`, reads the fields, and then disposes it.

The `DiscordPresenceInfo` struct layout:

```
Offset  Field             Type       Description
 00–08  PresenceId        ulong      Discord Application ID
 08–16  LargeIconUrl      ushort*    Asset name or URL
 16–24  LargeIconTooltip  ushort*    Hover text for large icon
 24–32  SmallIconUrl      ushort*    Asset name or URL (optional)
 32–40  SmallIconTooltip  ushort*    Hover text for small icon
 40–48  Reserved          void*      Reserved; always null
```

> [!NOTE]
> The default implementation in `SharedStaticV1Ext<T>` returns `false`, so if you do not override `GetCurrentDiscordPresenceInfoCore`, Discord RP is simply disabled.

---

## 4. Multi-region plugins

A single plugin DLL can expose multiple game regions (e.g. Global, SEA, China) by returning more than one `IPluginPresetConfig` from the `IPlugin` implementation. The launcher iterates from index `0` to `count - 1`.

### Example: two regions

```csharp
using Hi3Helper.Plugin.Core;
using Hi3Helper.Plugin.Core.Management.PresetConfig;
using System;
using System.Runtime.InteropServices.Marshalling;

[GeneratedComClass]
public partial class MyPlugin : PluginBase
{
    private static DateTime _creationDate =
        new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);

    public override void GetPluginName(out string? result)        => result = "My Awesome Game";
    public override void GetPluginDescription(out string? result) => result = "Multi-region plugin";
    public override void GetPluginAuthor(out string? result)      => result = "YourName";

    public override unsafe void GetPluginCreationDate(out DateTime* result)
    {
        fixed (DateTime* ptr = &_creationDate)
            result = ptr;
    }

    // Tell the launcher there are two regions
    public override void GetPresetConfigCount(out int count) => count = 2;

    public override void GetPresetConfig(int index, out IPluginPresetConfig result)
    {
        result = index switch
        {
            0 => new MyGlobalPresetConfig(),
            1 => new MySeaPresetConfig(),
            _ => throw new ArgumentOutOfRangeException(nameof(index))
        };
    }
}
```

Each preset config is a separate `PluginPresetConfigBase` subclass with its own metadata:

```csharp
[GeneratedComClass]
public partial class MyGlobalPresetConfig : PluginPresetConfigBase
{
    public override string? GameName    => "My Awesome Game";
    public override string  ProfileName => "Global";
    public override string  ZoneName    => "Global";
    public override string  ZoneFullName => "My Awesome Game - Global";
    // ... other required properties
    public override GameReleaseChannel ReleaseChannel => GameReleaseChannel.Stable;
    public override System.Collections.Generic.List<string> SupportedLanguages => ["en-US", "ja-JP"];
    public override IGameManager?      GameManager      { get; set; }
    public override IGameInstaller?    GameInstaller    { get; set; }
    public override ILauncherApiMedia? LauncherApiMedia { get; set; }
    public override ILauncherApiNews?  LauncherApiNews  { get; set; }
    // other abstract overrides...
}

[GeneratedComClass]
public partial class MySeaPresetConfig : PluginPresetConfigBase
{
    public override string? GameName    => "My Awesome Game";
    public override string  ProfileName => "SEA";
    public override string  ZoneName    => "SEA";
    public override string  ZoneFullName => "My Awesome Game - SEA";
    public override GameReleaseChannel ReleaseChannel => GameReleaseChannel.Stable;
    public override System.Collections.Generic.List<string> SupportedLanguages => ["en-US", "id-ID", "th-TH"];
    // ... other required properties pointing to SEA CDN endpoints
    public override IGameManager?      GameManager      { get; set; }
    public override IGameInstaller?    GameInstaller    { get; set; }
    public override ILauncherApiMedia? LauncherApiMedia { get; set; }
    public override ILauncherApiNews?  LauncherApiNews  { get; set; }
}
```

### Tips

- **Shared `HttpClient`** — if both regions share the same CDN, create a single static `HttpClient` (via `PluginHttpClientBuilder`) and reuse it in both preset config `InitializeAsync` implementations.
- **`ProfileName` vs `ZoneName`** — `ZoneName` is the short display label in the launcher UI. `ProfileName` is used as an internal grouping key; it can be the same as `ZoneName`.
- **Region ordering** — the launcher displays regions in the order the plugin returns them (index 0 first). Put the most common region first.
- **Locale handling** — `SharedStatic.PluginLocaleCode` is set by the launcher via the `SetPluginCurrentLocale` export. Both preset configs read the same `PluginLocaleCode`, so API calls for different regions will automatically use the user's selected language.
