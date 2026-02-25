# Introduction

`Hi3Helper.Plugin.Core` is a NativeAOT-compatible plugin framework for **Collapse Launcher**. It provides a set of COM-based interfaces and base classes that allow third-party developers to write game plugins — extending the launcher with support for additional games without modifying the launcher itself.

## Architecture overview

Plugins are compiled as native shared libraries (`.dll` on Windows) and loaded dynamically by the launcher at runtime. Communication between the launcher and the plugin uses [COM](https://learn.microsoft.com/en-us/windows/win32/com/the-component-object-model) interop via `System.Runtime.InteropServices.Marshalling`, which is fully compatible with .NET NativeAOT.

```
Collapse Launcher
│
│  GetApiExport("GetPlugin") ──────────────────────────────────┐
│                                                               │
▼                                                               ▼
Plugin.dll (NativeAOT)
├── SharedStatic / SharedStaticV1Ext<T>   (export registry)
├── IPlugin  ──► PluginBase               (plugin metadata)
│       └── IPluginPresetConfig[n] ──► PluginPresetConfigBase  (per-region config)
│               ├── IGameManager   ──► GameManagerBase         (runtime management)
│               ├── IGameInstaller ──► GameInstallerBase       (install / update)
│               ├── ILauncherApiMedia ──► LauncherApiMediaBase (launcher backgrounds)
│               └── ILauncherApiNews  ──► LauncherApiNewsBase  (launcher news feed)
└── IPluginSelfUpdate ──► PluginSelfUpdateBase                 (plugin self-update)
```

### Key concepts

| Concept | Description |
|---------|-------------|
| **`IPlugin` / `PluginBase`** | Root interface. Provides plugin metadata (name, author, version) and exposes one or more `IPluginPresetConfig` regions. |
| **`IPluginPresetConfig` / `PluginPresetConfigBase`** | Describes a single game region/zone (e.g. "Global", "SEA"). Holds all static metadata and references to the live management objects. |
| **`IGameManager` / `GameManagerBase`** | Handles in-game controls such as launching the game. |
| **`IGameInstaller` / `GameInstallerBase`** | Handles installation, repair, and game updates. |
| **`ILauncherApiMedia` / `LauncherApiMediaBase`** | Supplies launcher background images, icons, and other media. |
| **`ILauncherApiNews` / `LauncherApiNewsBase`** | Supplies the launcher news carousel and social media links. |
| **`IPluginSelfUpdate` / `PluginSelfUpdateBase`** | Allows the plugin DLL itself to be updated from a CDN. |
| **`SharedStatic`** | Static registry that maps export names to function pointers. The launcher calls `GetApiExport` to obtain any of these pointers at runtime. |

## API standard versioning

The current API standard version is **v0.1.4**. All plugins must implement at minimum the **v0.1 core** exports. Optional feature sets are versioned as update packages (`v0.1-update1`, `v0.1-update2`, etc.) and are handled automatically by `SharedStaticV1Ext<T>`.

## Next steps

Follow the [Getting Started](getting-started.md) guide to create your first plugin.
