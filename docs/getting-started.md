# Getting Started

This guide walks you through creating a minimal but fully functional Collapse Launcher plugin from scratch. At the end you will have a NativeAOT-compiled `.dll` that the launcher can load.

For a deeper look at each piece, see the [full plugin creation guide](create-plugin.md).

## Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) or later
- A C++ build toolchain (required by NativeAOT linker): [Visual Studio Build Tools](https://visualstudio.microsoft.com/downloads/#build-tools-for-visual-studio-2022) with the **Desktop development with C++** workload
- x64 Windows target (current supported runtime)

## 1. Create the project

```sh
dotnet new classlib -n MyGamePlugin -f net10.0
cd MyGamePlugin
```

Edit the `.csproj` so it looks like this:

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
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Hi3Helper.Plugin.Core" Version="*-*" />
  </ItemGroup>
</Project>
```

## 2. Create your `IPlugin` implementation

Create `MyPlugin.cs`:

```csharp
using Hi3Helper.Plugin.Core;
using Hi3Helper.Plugin.Core.Management.PresetConfig;
using Hi3Helper.Plugin.Core.Update;
using System;
using System.Runtime.InteropServices.Marshalling;

[GeneratedComClass]
public partial class MyPlugin : PluginBase
{
    private static DateTime _creationDate = new DateTime(2025, 6, 1, 0, 0, 0, DateTimeKind.Utc);

    public override void GetPluginName(out string? result)        => result = "My Game Plugin";
    public override void GetPluginDescription(out string? result) => result = "Plugin for My Awesome Game";
    public override void GetPluginAuthor(out string? result)      => result = "YourName";

    public override unsafe void GetPluginCreationDate(out DateTime* result)
    {
        fixed (DateTime* ptr = &_creationDate)
            result = ptr;
    }

    public override void GetPresetConfigCount(out int count)  => count = 1;

    public override void GetPresetConfig(int index, out IPluginPresetConfig result)
    {
        result = index switch
        {
            0 => new MyPresetConfig(),
            _ => throw new ArgumentOutOfRangeException(nameof(index))
        };
    }
}
```

## 3. Create your `IPluginPresetConfig` implementation

Create `MyPresetConfig.cs`:

```csharp
using Hi3Helper.Plugin.Core.Management;
using Hi3Helper.Plugin.Core.Management.Api;
using Hi3Helper.Plugin.Core.Management.PresetConfig;
using System.Collections.Generic;
using System.Runtime.InteropServices.Marshalling;

[GeneratedComClass]
public partial class MyPresetConfig : PluginPresetConfigBase
{
    public override string? GameName                  => "My Awesome Game";
    public override string  GameExecutableName        => "MyGame.exe";
    public override string  GameAppDataPath           => @"%AppData%\MyGame";
    public override string  GameLogFileName           => "output_log.txt";
    public override string  GameVendorName            => "MyStudio";
    public override string  GameRegistryKeyName       => @"SOFTWARE\MyStudio\MyGame";
    public override string  ProfileName               => "Global";
    public override string  ZoneName                  => "Global";
    public override string  ZoneFullName              => "My Awesome Game - Global";
    public override string  ZoneDescription           => "Global server.";
    public override string  ZoneLogoUrl               => "https://example.com/logo.png";
    public override string  ZonePosterUrl             => "https://example.com/poster.jpg";
    public override string  ZoneHomePageUrl           => "https://example.com/";
    public override string  GameMainLanguage          => "en-US";
    public override string  LauncherGameDirectoryName => "MyGame";

    public override GameReleaseChannel  ReleaseChannel    => GameReleaseChannel.Stable;
    public override List<string>        SupportedLanguages => ["en-US", "zh-CN", "ja-JP"];

    // Wire up optional sub-systems after they are instantiated in InitializeAsync
    public override IGameManager?      GameManager      { get; set; }
    public override IGameInstaller?    GameInstaller    { get; set; }
    public override ILauncherApiMedia? LauncherApiMedia { get; set; }
    public override ILauncherApiNews?  LauncherApiNews  { get; set; }
}
```

## 4. Register exports and expose the entry point

Create `PluginExports.cs`. This wires up all required function-pointer exports and exposes the single `GetApiExport` entry point that the launcher calls:

```csharp
using Hi3Helper.Plugin.Core;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Inheriting SharedStaticV1Ext<T> registers all standard + optional v0.1 exports.
public partial class MyPluginExports : SharedStaticV1Ext<MyPluginExports>
{
    [ModuleInitializer]
    public static void Initialize() => Load<MyPlugin>();
}

public static partial class PluginEntryPoint
{
    /// <summary>
    /// Single entry point used by the launcher to resolve any plugin function by name.
    /// </summary>
    [UnmanagedCallersOnly(EntryPoint = "GetApiExport")]
    public static unsafe int GetApiExport(char* apiName, void** outPtr)
        => SharedStatic.TryGetApiExportPointer(apiName, outPtr);
}
```

> [!NOTE]
> `SharedStaticV1Ext<T>` is a subclass of `SharedStatic` that also registers the optional v0.1 extension exports (Discord Rich Presence, locale, etc.). Use plain `SharedStatic` only if you need the absolute minimum export surface.

## 5. Publish with NativeAOT

Add a publish profile at `Properties/PublishProfiles/NativeAOT.pubxml`:

```xml
<Project>
  <PropertyGroup>
    <PublishAot>true</PublishAot>
    <RuntimeIdentifier>win-x64</RuntimeIdentifier>
    <!-- Keep false unless you are targeting DebugNoReflection / ReleaseNoReflection -->
    <IlcDisableReflection>false</IlcDisableReflection>
  </PropertyGroup>
</Project>
```

Publish:

```sh
dotnet publish -p:PublishProfile=NativeAOT -c Release
```

The resulting `.dll` in the publish output directory is ready to be loaded by Collapse Launcher.

---

## What's next?

| Topic | Guide |
|-------|-------|
| Full walkthrough with game management and self-update | [Creating a Plugin — full guide](create-plugin.md) |
| API reference | [API docs](../api/Hi3Helper.Plugin.Core.html) |
