# Creating a Plugin — Full Guide

This guide covers the complete lifecycle of a Collapse Launcher plugin: project setup, implementing every interface, async patterns, self-update, and NativeAOT publishing.

## Table of contents

1. [Project setup](#1-project-setup)
2. [Implementing IPlugin](#2-implementing-iplugin)
3. [Implementing IPluginPresetConfig](#3-implementing-ipluginpresetconfig)
4. [Implementing IGameManager](#4-implementing-igamemanager)
5. [Implementing IGameInstaller](#5-implementing-igameinstaller)
6. [Implementing launcher API (media and news)](#6-implementing-the-launcher-api)
7. [Implementing plugin self-update](#7-implementing-plugin-self-update)
8. [Registering exports](#8-registering-exports)
9. [Async patterns with ComAsyncResult](#9-async-patterns-with-comasyncresult)
10. [NativeAOT publishing](#10-nativeaot-publishing)
11. [Build configurations](#11-build-configurations)

---

## 1. Project setup

### `.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Platforms>x64</Platforms>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
    <Nullable>enable</Nullable>
    <IsAotCompatible>true</IsAotCompatible>
    <InvariantGlobalization>true</InvariantGlobalization>
    <LangVersion>preview</LangVersion>
    <!-- Embed debug symbols so stack traces work in Release -->
    <DebugType>embedded</DebugType>
    <Configurations>Debug;Release;DebugNoReflection;ReleaseNoReflection</Configurations>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Hi3Helper.Plugin.Core" Version="*-*" />
  </ItemGroup>

  <!-- NoReflection build configurations used for full NativeAOT trim -->
  <PropertyGroup Condition="'$(Configuration)'=='DebugNoReflection'">
    <DefineConstants>$(DefineConstants);DEBUG;USELIGHTWEIGHTJSONPARSER;MANUALCOM</DefineConstants>
    <Optimize>false</Optimize>
  </PropertyGroup>

  <PropertyGroup Condition="'$(Configuration)'=='ReleaseNoReflection'">
    <DefineConstants>$(DefineConstants);USELIGHTWEIGHTJSONPARSER;MANUALCOM</DefineConstants>
    <Optimize>true</Optimize>
  </PropertyGroup>
</Project>
```

> [!WARNING]
> **NativeAOT and reflection**: Plugin code must be reflection-free wherever possible. Avoid `Type.GetType`, `Activator.CreateInstance`, or `Assembly.Load` calls. For JSON, use [`System.Text.Json` source generation](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/source-generation) (`JsonSerializerContext`) or low-level `Utf8JsonReader`/`Utf8JsonWriter`.

---

## 2. Implementing `IPlugin`

`IPlugin` is the root interface. Inherit `PluginBase`, which handles `CancelAsync` and proxy settings for you, and implement the abstract members.

```csharp
using Hi3Helper.Plugin.Core;
using Hi3Helper.Plugin.Core.Management.PresetConfig;
using Hi3Helper.Plugin.Core.Update;
using System;
using System.Runtime.InteropServices.Marshalling;

[GeneratedComClass]
public partial class MyPlugin : PluginBase
{
    // Keep a static field so we can return a stable pointer
    private static DateTime _creationDate = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);

    // --- Required abstract overrides ---

    public override void GetPluginName(out string? result)
        => result = "My Awesome Game";

    public override void GetPluginDescription(out string? result)
        => result = "A plugin for My Awesome Game";

    public override void GetPluginAuthor(out string? result)
        => result = "YourName";

    public override unsafe void GetPluginCreationDate(out DateTime* result)
    {
        // Return a pointer to a *static* field. Never return a pointer to a local variable.
        fixed (DateTime* ptr = &_creationDate)
            result = ptr;
    }

    public override void GetPresetConfigCount(out int count)
        => count = 1; // one region in this example

    public override void GetPresetConfig(int index, out IPluginPresetConfig result)
    {
        result = index switch
        {
            0 => new MyPresetConfig(),
            _ => throw new ArgumentOutOfRangeException(nameof(index))
        };
    }

    // --- Optional overrides ---

    // Provide an icon URL shown in the launcher
    public override void GetPluginAppIconUrl(out string result)
        => result = "https://example.com/icon.png";

    // Return a self-updater so the launcher can keep the plugin DLL up to date
    private readonly MyPluginSelfUpdater _selfUpdater = new();
    public override void GetPluginSelfUpdater(out IPluginSelfUpdate? selfUpdate)
        => selfUpdate = _selfUpdater;
}
```

> [!NOTE]
> `GetPluginAppIconUrl`, `GetNotificationPosterUrl`, and `GetPluginSelfUpdater` have default no-op implementations in `PluginBase`. Override only what you need.

---

## 3. Implementing `IPluginPresetConfig`

Each game region maps to one `PluginPresetConfigBase` instance. The base class implements all the COM glue; you just override properties.

```csharp
using Hi3Helper.Plugin.Core.Management;
using Hi3Helper.Plugin.Core.Management.Api;
using Hi3Helper.Plugin.Core.Management.PresetConfig;
using System.Collections.Generic;
using System.Runtime.InteropServices.Marshalling;

[GeneratedComClass]
public partial class MyPresetConfig : PluginPresetConfigBase
{
    // ---- Static metadata (all abstract — must be provided) ----

    public override string? GameName                  => "My Awesome Game";
    public override string  GameExecutableName        => "MyGame.exe";
    public override string  GameAppDataPath           => @"%AppData%\MyGame";
    public override string  GameLogFileName           => "output_log.txt";
    public override string  GameVendorName            => "MyStudio";
    public override string  GameRegistryKeyName       => @"SOFTWARE\MyStudio\MyGame";

    public override string ProfileName               => "Global";
    public override string ZoneName                  => "Global";
    public override string ZoneFullName              => "My Awesome Game - Global";
    public override string ZoneDescription           => "Official global server.";
    public override string ZoneLogoUrl               => "https://cdn.example.com/logo.png";
    public override string ZonePosterUrl             => "https://cdn.example.com/poster.jpg";
    public override string ZoneHomePageUrl           => "https://example.com/";
    public override string LauncherGameDirectoryName => "MyGame";

    public override string GameMainLanguage => "en-US";
    public override List<string> SupportedLanguages => ["en-US", "zh-CN", "ja-JP"];

    public override GameReleaseChannel ReleaseChannel => GameReleaseChannel.Stable;

    // ---- Live sub-systems (set during InitializeAsync) ----

    public override IGameManager?      GameManager      { get; set; }
    public override IGameInstaller?    GameInstaller    { get; set; }
    public override ILauncherApiMedia? LauncherApiMedia { get; set; }
    public override ILauncherApiNews?  LauncherApiNews  { get; set; }

    // ---- Async initialization (called by the launcher before using the config) ----

    protected override async Task InitializeAsync(CancellationToken token)
    {
        // This method is called exactly once before the launcher accesses any sub-system.
        // Instantiate and initialise your sub-systems here.
        var manager = new MyGameManager();
        await manager.InitializeAsync(token);
        GameManager = manager;

        var installer = new MyGameInstaller();
        GameInstaller = installer;

        LauncherApiMedia = new MyLauncherApiMedia();
        LauncherApiNews  = new MyLauncherApiNews();
    }
}
```

> [!TIP]
> The `InitializeAsync` method is the correct place to perform async setup (HTTP calls, file reads, etc.), because `PluginPresetConfigBase` inherits `InitializableTask` which ensures the method is invoked once before any property is accessed.

---

## 4. Implementing `IGameManager`

`GameManagerBase` (in `Management/GameManagerBase.cs`) provides the default implementation wiring. Override the members you need:

```csharp
using Hi3Helper.Plugin.Core.Management;
using System.Runtime.InteropServices.Marshalling;

[GeneratedComClass]
public partial class MyGameManager : GameManagerBase
{
    // Return a path to the game's executable (used by the launcher to launch the game)
    public override void GetGameExecutablePath(out string? result)
        => result = @"C:\Games\MyGame\MyGame.exe";

    // Called by the launcher when the user clicks "Play"
    public override void LaunchGame(string? additionalArgs, out bool isSuccess)
    {
        // Start the game process
        var psi = new System.Diagnostics.ProcessStartInfo(
            @"C:\Games\MyGame\MyGame.exe", additionalArgs ?? "")
        {
            UseShellExecute = true,
            WorkingDirectory = @"C:\Games\MyGame\"
        };
        System.Diagnostics.Process.Start(psi);
        isSuccess = true;
    }
}
```

---

## 5. Implementing `IGameInstaller`

`GameInstallerBase` supplies the COM plumbing. Async methods use `ComAsyncResult` (see [section 9](#9-async-patterns-with-comasyncresult)).

```csharp
using Hi3Helper.Plugin.Core.Management;
using Hi3Helper.Plugin.Core.Utility;
using System;
using System.Runtime.InteropServices.Marshalling;
using System.Threading;
using System.Threading.Tasks;

[GeneratedComClass]
public partial class MyGameInstaller : GameInstallerBase
{
    // ---- Size queries ----

    protected override async Task<long> GetGameSizeAsync(
        GameInstallerKind kind, CancellationToken token)
    {
        // Return the total size in bytes for the given installer kind
        return kind switch
        {
            GameInstallerKind.Install => 30_000_000_000L,
            GameInstallerKind.Update  => 1_500_000_000L,
            _ => 0L
        };
    }

    protected override async Task<long> GetGameDownloadedSizeAsync(
        GameInstallerKind kind, CancellationToken token)
    {
        return 0L; // query local disk for already-downloaded bytes
    }

    // ---- Installation ----

    protected override async Task StartInstallAsync(
        string installDir,
        InstallProgressDelegate? progressDelegate,
        CancellationToken token)
    {
        // Download and extract — report progress via progressDelegate
        var progress = new InstallProgress { TotalBytes = 30_000_000_000L };
        for (long downloaded = 0; downloaded < progress.TotalBytes; downloaded += 1024 * 1024)
        {
            token.ThrowIfCancellationRequested();
            progress.DownloadedBytes = downloaded;
            progressDelegate?.Invoke(in progress);
            await Task.Delay(10, token);
        }
    }

    // ---- Repair / Update ----

    protected override Task StartRepairAsync(
        string installDir, InstallProgressDelegate? progressDelegate, CancellationToken token)
        => StartInstallAsync(installDir, progressDelegate, token); // simplified

    protected override Task StartUpdateAsync(
        string installDir, InstallProgressDelegate? progressDelegate, CancellationToken token)
        => StartInstallAsync(installDir, progressDelegate, token); // simplified

    // ---- Uninstall (from IGameUninstaller) ----

    protected override Task StartUninstallAsync(string installDir, CancellationToken token)
    {
        if (System.IO.Directory.Exists(installDir))
            System.IO.Directory.Delete(installDir, recursive: true);
        return Task.CompletedTask;
    }
}
```

---

## 6. Implementing the Launcher API

### Media (`ILauncherApiMedia`)

Provides background images, icons, and other visual assets to the launcher.

```csharp
using Hi3Helper.Plugin.Core.Management.Api;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.InteropServices.Marshalling;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

[GeneratedComClass]
public partial class MyLauncherApiMedia : LauncherApiMediaBase
{
    private static readonly HttpClient _http = new();

    protected override async Task<LauncherBackgroundEntry?> GetBackgroundAsync(
        string locale, CancellationToken token)
    {
        // Fetch from your CDN and return a LauncherBackgroundEntry
        return new LauncherBackgroundEntry
        {
            BackgroundUrl  = "https://cdn.example.com/bg.jpg",
            ThumbnailUrl   = "https://cdn.example.com/bg_thumb.jpg",
            FallbackColor  = "#1A1A2E"
        };
    }

    protected override async Task<List<LauncherCarouselEntry>?> GetCarouselEntriesAsync(
        string locale, CancellationToken token)
    {
        return
        [
            new LauncherCarouselEntry
            {
                ImageUrl  = "https://cdn.example.com/banner1.jpg",
                TargetUrl = "https://example.com/news/1"
            }
        ];
    }

    protected override async Task<List<LauncherPathEntry>?> GetLauncherPathEntriesAsync(
        string locale, CancellationToken token)
        => null; // optional
}
```

### News (`ILauncherApiNews`)

Provides the news / events feed shown in the launcher.

```csharp
using Hi3Helper.Plugin.Core.Management.Api;
using System.Collections.Generic;
using System.Runtime.InteropServices.Marshalling;
using System.Threading;
using System.Threading.Tasks;

[GeneratedComClass]
public partial class MyLauncherApiNews : LauncherApiNewsBase
{
    protected override async Task<List<LauncherNewsEntry>?> GetNewsEntriesAsync(
        string locale, CancellationToken token)
    {
        return
        [
            new LauncherNewsEntry
            {
                Title      = "Version 2.0 is here!",
                Content    = "A brand new chapter begins.",
                Url        = "https://example.com/news/v2",
                ImageUrl   = "https://cdn.example.com/news1.jpg",
                EntryType  = LauncherNewsEntryType.News,
                PublishDate = new System.DateTime(2025, 6, 1)
            }
        ];
    }

    protected override async Task<List<LauncherSocialMediaEntry>?> GetSocialMediaEntriesAsync(
        string locale, CancellationToken token)
    {
        return
        [
            new LauncherSocialMediaEntry
            {
                Name    = "Twitter / X",
                Url     = "https://twitter.com/example",
                IconUrl = "https://cdn.example.com/twitter.png",
                Flag    = LauncherSocialMediaEntryFlag.Twitter
            }
        ];
    }
}
```

---

## 7. Implementing plugin self-update

Inherit `PluginSelfUpdateBase` and supply the CDN base URLs and an `HttpClient`. The core library constructs the URL by appending `manifest.json` to each CDN entry and tries them in order.

```csharp
using Hi3Helper.Plugin.Core.Update;
using System.Net.Http;

public class MyPluginSelfUpdater : PluginSelfUpdateBase
{
    // One or more CDN mirror URLs. Each must end with '/'.
    protected override ReadOnlySpan<string> BaseCdnUrlSpan =>
    [
        "https://cdn-primary.example.com/myplugin/",
        "https://cdn-mirror.example.com/myplugin/"
    ];

    // HttpClient used exclusively for downloading updates
    protected override HttpClient UpdateHttpClient { get; } = new HttpClient();
}
```

Wire it up in `MyPlugin`:

```csharp
private readonly MyPluginSelfUpdater _selfUpdater = new();

public override void GetPluginSelfUpdater(out IPluginSelfUpdate? selfUpdate)
    => selfUpdate = _selfUpdater;
```

The launcher will call `TryPerformUpdateAsync` with `checkForUpdatesOnly: true` to detect updates, and then again with `checkForUpdatesOnly: false` to download and apply them. Both cases are handled by `PluginSelfUpdateBase.TryPerformUpdateAsync` automatically.

---

## 8. Registering exports

Create a single class that inherits `SharedStaticV1Ext<T>` to auto-register all standard and optional exports. Then expose the `GetApiExport` entry point as an `UnmanagedCallersOnly` export.

```csharp
using Hi3Helper.Plugin.Core;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Must inherit SharedStaticV1Ext<T> (not SharedStatic directly) to include
// all optional v0.1 extension exports (locale, Discord RP, etc.)
public partial class MyPluginExports : SharedStaticV1Ext<MyPluginExports>
{
    [ModuleInitializer]
    public static void Initialize()
    {
        // Load<T> creates one instance of MyPlugin and stores it as the active IPlugin.
        // It must be called before the launcher calls GetApiExport.
        Load<MyPlugin>();
    }
}

public static partial class PluginEntryPoint
{
    /// <summary>
    /// Called by the launcher to obtain a function pointer for any named plugin export.
    /// Returns 0 on success, int.MinValue if the name is unknown.
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "GetApiExport")]
    public static unsafe int GetApiExport(char* apiName, void** outPtr)
        => SharedStatic.TryGetApiExportPointer(apiName, outPtr);
}
```

### Registered exports (v0.1 core)

| Export name | Description |
|-------------|-------------|
| `GetPluginStandardVersion` | Returns a pointer to the API standard version the plugin targets |
| `GetPluginVersion` | Returns a pointer to the plugin's own `GameVersion` |
| `GetPlugin` | Returns the COM interface pointer for `IPlugin` |
| `FreePlugin` | Releases the active `IPlugin` instance |
| `SetLoggerCallback` | Installs the launcher's log-sink callback |
| `SetDnsResolverCallback` | Installs a synchronous DNS override callback |

### Additional exports (v0.1-update1)

| Export name | Description |
|-------------|-------------|
| `GetPluginUpdateCdnList` | Returns the CDN URL array from `PluginSelfUpdateBase.BaseCdnUrlSpan` |
| `SetDnsResolverCallbackAsync` | Installs an asynchronous DNS override callback |

> [!NOTE]
> All values above are registered automatically by `SharedStatic` (core) and `SharedStaticV1Ext<T>` (extensions). You never have to call `TryRegisterApiExport` manually unless you are adding a custom export.

---

## 9. Async patterns with `ComAsyncResult`

All async operations in the plugin system return a `nint` that points to a `ComAsyncResult`. The launcher calls `ComAsyncExtension.AsTask<T>(nint)` to convert that pointer into an awaitable .NET task.

Inside your base class implementations the conversion is already handled — just write normal `async Task<T>` methods. If you ever need to marshal manually:

```csharp
// Inside an IGameInstaller method (already done by GameInstallerBase):
public void StartInstallAsync(string installDir, InstallProgressDelegate? progress,
    in Guid cancelToken, out nint result)
{
    CancellationTokenSource cts = ComCancellationTokenVault.RegisterToken(in cancelToken);
    result = StartInstallAsync(installDir, progress, cts.Token).AsResult();
    //                                                          ^^^^^^^^^
    //   Extension method from ComAsyncExtension that boxes the Task<T>
    //   into a ComAsyncResult and returns its pointer.
}
```

---

## 10. NativeAOT publishing

### Publish profile (`Properties/PublishProfiles/NativeAOT.pubxml`)

```xml
<Project>
  <PropertyGroup>
    <PublishAot>true</PublishAot>
    <RuntimeIdentifier>win-x64</RuntimeIdentifier>
    <!-- Set true only for DebugNoReflection / ReleaseNoReflection -->
    <IlcDisableReflection>false</IlcDisableReflection>
  </PropertyGroup>
</Project>
```

### Publish command

```sh
dotnet publish -p:PublishProfile=NativeAOT -c Release
```

The output directory contains the compiled plugin `.dll` ready for deployment.

---

## 11. Build configurations

| Configuration | `MANUALCOM` | `USELIGHTWEIGHTJSONPARSER` | Notes |
|---------------|:-----------:|:--------------------------:|-------|
| `Debug`               |             |             | Standard debug build with reflection |
| `Release`             |             |             | Standard release build with reflection |
| `DebugNoReflection`   | ✓           | ✓           | NativeAOT debug; manual COM wrappers, no `System.Text.Json` serializer |
| `ReleaseNoReflection` | ✓           | ✓           | NativeAOT release; full tree-shaking |

- `MANUALCOM` switches the COM interop layer from `ComInterfaceMarshaller<T>` to hand-rolled ABI wrappers in the `ABI/` folder.
- `USELIGHTWEIGHTJSONPARSER` disables the `JsonSerializer` API surface inside the library, which eliminates a large reflection-required rooted type set. Use `Utf8JsonReader`/`Utf8JsonWriter` or `JsonDocument` in your own code.

---

## Summary

1. Add `Hi3Helper.Plugin.Core` NuGet reference to a net10.0 class library.
2. Subclass `PluginBase` → override metadata and `GetPresetConfig`.
3. Subclass `PluginPresetConfigBase` → override all abstract properties; instantiate sub-systems in `InitializeAsync`.
4. Subclass `GameManagerBase`, `GameInstallerBase`, `LauncherApiMediaBase`, `LauncherApiNewsBase` as needed.
5. Subclass `PluginSelfUpdateBase` if the plugin should self-update.
6. Create a class that inherits `SharedStaticV1Ext<T>` and calls `Load<TPlugin>()` in a `[ModuleInitializer]`.
7. Expose `GetApiExport` as `[UnmanagedCallersOnly]`.
8. Publish with NativeAOT.
