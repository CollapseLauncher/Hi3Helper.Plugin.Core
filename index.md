---
_layout: landing
---

# Hi3Helper.Plugin.Core

`Hi3Helper.Plugin.Core` is the official plugin framework for **Collapse Launcher**. It provides a set of COM-based interfaces, abstract base classes, and utility helpers that let you build a game plugin as a native Windows DLL — no launcher source code changes required.

## Highlights

- **Full NativeAOT support** — designed to be compiled with .NET NativeAOT; reflection-free when targeting `MANUALCOM`/`USELIGHTWEIGHTJSONPARSER` configurations
- **COM interop via `System.Runtime.InteropServices.Marshalling`** — no custom COM registration; the launcher discovers everything through a single `GetApiExport` entry point
- **Batteries-included utilities** — pre-configured `HttpClient` builder with automatic proxy and DNS resolver integration, retry-able download streams, speed limiting, and unmanaged memory helpers
- **Versioned extension API** — optional feature sets (`v0.1-update1` through `v0.1-update4`) for game launch, Discord Rich Presence, resizable-window hook, and download throttling are all opt-in

## Getting started

| | |
|--|--|
| [Introduction](docs/introduction.md) | Architecture overview, key concepts, and API standard versioning |
| [Getting Started](docs/getting-started.md) | Create and publish a minimal working plugin in 5 steps |
| [Creating a Plugin](docs/create-plugin.md) | Full guide — every interface, async patterns, self-update, and NativeAOT publishing |
| [Utilities](docs/utilities.md) | `PluginHttpClientBuilder`, retry-able downloads, speed limiting, and unmanaged memory |
| [Advanced Topics](docs/advanced.md) | DNS resolver callbacks, proxy settings, Discord Rich Presence, multi-region plugins |
| [API Reference](api/index.md) | Auto-generated reference for every public type |

## Plugin examples

- [Hi3Helper.Plugin.HBR](https://github.com/CollapseLauncher/Hi3Helper.Plugin.HBR) — Heaven Burns Red
- [Hi3Helper.Plugin.Wuwa](https://github.com/CollapseLauncher/Hi3Helper.Plugin.Wuwa) — Wuthering Waves
- [Hi3Helper.Plugin.DNA](https://github.com/CollapseLauncher/Hi3Helper.Plugin.DNA) — Duet Night Abyss