# Collapse Launcher Plugin Core Library
This is the main repository of the ``Hi3Helper.Plugin.Core`` library (aka Collapse Launcher Standard Core Plugin Library), broadly used as a base to develop Game Plugin Support system for Collapse, implementing standards of API contracts and core functionalities which are used by both the plugin and the launcher, including: Platform Invocation, COM Interop and Marshalling.

# How to Contribute?
You can contribute to this library by providing a proposal of new API contracts, or by improving the existing API contract implementation. You can expect some changes in the near future as the existing APIs are still under development.

Keep in mind that the code included in this repository are mainly unsafe due to marshalling nature from Managed .NET code to Unmanaged code platform invocation.

Make sure that your code is **as reflection-free** as possible, as the code is entirely purposed to work with NativeAOT Compilation. Minimal reflection-based features (such as ``GetType``) are still supported, but ensure that you set the ``IlcDisableReflection`` on your ``.csproj`` project file (or ``.pubxml`` publish profile) to ``false``. If you need to perform JSON Serialization/Deserialization, please ensure that your code uses source-generated ``JsonSerializerContext`` or use Lightweight/No-Reflection (manually deserialize the JSON with some interface implementation ([See below](http://github.com/CollapseLauncher/Hi3Helper.Plugin.Core/blob/main/README.md#:~:text=Lightweight/No%2DReflection%20Supported%20JSON%20Serializer/Deserializer))) via ``JsonDocument``, ``Utf8JsonReader`` and ``Utf8JsonWriter``.

# What's Included?
This Core Library includes various APIs to make the plugin development faster, without needing to implement the entire functions from scratch. Here's a list of what's included currently:

### V1 (v0.1.3.0) Implementation Standard

This repository follows the V1 implementation standard (current library version: ``v0.1.3.0``). The standard collects base API contracts, COM interop helpers, marshallers and small utility primitives that plugin authors and the launcher can rely on.

> [!WARNING] 
> The API contracts and implementations are still under development, so expect some breaking changes in the future.
> This list is subject to change without prior notice.

> [!NOTE]
> This list is not exhaustive. Some utility classes and helper functions are not listed here.
> Please explore the source code for more details.

> [!TIP]
> If a new version of the API standard is released, you will need to make sure that your plugin is updated to the latest version to take advantage of the new features and improvements.

* Base/Abstract API Classes
  * [``InitializableTask``](https://github.com/CollapseLauncher/Hi3Helper.Plugin.Core/blob/main/InitializableTask.cs) (Implement: ``IInitializableTask``)
  * [``PluginBase``](https://github.com/CollapseLauncher/Hi3Helper.Plugin.Core/blob/main/PluginBase.cs) (Implement: ``IPlugin``)
  * [``PluginPresetConfigBase``](https://github.com/CollapseLauncher/Hi3Helper.Plugin.Core/blob/main/Management/PresetConfig/PluginPresetConfigBase.cs) (Inherit: ``InitializableTask``, Implement: ``IPluginPresetConfig``)
  * [``PluginSelfUpdateBase``](https://github.com/CollapseLauncher/Hi3Helper.Plugin.Core/blob/main/Update/PluginSelfUpdateBase.cs) (Implement: ``IPluginSelfUpdate``)
  * [``GameInstallerBase``](https://github.com/CollapseLauncher/Hi3Helper.Plugin.Core/blob/main/Management/GameInstallerBase.cs) (Inherit: ``LauncherApiBase``, Implement: ``IGameInstaller``)
  * [``GameManagerBase``](https://github.com/CollapseLauncher/Hi3Helper.Plugin.Core/blob/main/Management/GameManagerBase.cs) (Inherit: ``LauncherApiBase``, Implement: ``IGameManager``)
  * [``LauncherApiBase``](https://github.com/CollapseLauncher/Hi3Helper.Plugin.Core/blob/main/Management/Api/LauncherApiBase.cs) (Inherit: ``InitializableTask``, Implement: ``ILauncherApi``)
  * [``LauncherApiMediaBase``](https://github.com/CollapseLauncher/Hi3Helper.Plugin.Core/blob/main/Management/Api/LauncherApiMediaBase.cs) (Inherit: ``LauncherApiBase``, Implement: ``ILauncherApiMedia``)
  * [``LauncherApiNewsBase``](https://github.com/CollapseLauncher/Hi3Helper.Plugin.Core/blob/main/Management/Api/LauncherApiNewsBase.cs) (Inherit: ``LauncherApiBase``, Implement: ``ILauncherApiNews``)

* COM-API Interfaces/Contracts
  * [``IFree``](https://github.com/CollapseLauncher/Hi3Helper.Plugin.Core/blob/main/IFree.cs)
  * [``IPlugin``](https://github.com/CollapseLauncher/Hi3Helper.Plugin.Core/blob/main/IPlugin.cs) (Inherit: ``IFree``)
  * [``IInitializableTask``](https://github.com/CollapseLauncher/Hi3Helper.Plugin.Core/blob/main/IInitializableTask.cs) (Inherit: ``IFree``)
  * [``IPluginPresetConfig``](https://github.com/CollapseLauncher/Hi3Helper.Plugin.Core/blob/main/Management/PresetConfig/IPluginPresetConfig.cs) (Inherit: ``IInitializableTask``)
  * [``IPluginSelfUpdate``](https://github.com/CollapseLauncher/Hi3Helper.Plugin.Core/blob/main/Update/IPluginSelfUpdate.cs) (Inherit: ``IFree``)
  * [``IGameManager``](https://github.com/CollapseLauncher/Hi3Helper.Plugin.Core/blob/main/Management/IGameManager.cs) (Inherit: ``IInitializableTask``)
  * [``IGameUninstaller``](https://github.com/CollapseLauncher/Hi3Helper.Plugin.Core/blob/main/Management/IGameUninstaller.cs) (Inherit: ``IInitializableTask``)
  * [``IGameInstaller``](https://github.com/CollapseLauncher/Hi3Helper.Plugin.Core/blob/main/Management/IGameInstaller.cs) (Inherit: ``IGameUninstaller``)
  * [``ILauncherApi``](https://github.com/CollapseLauncher/Hi3Helper.Plugin.Core/blob/main/Management/Api/ILauncherApi.cs) (Inherit: ``IInitializableTask``)
  * [``ILauncherApiMedia``](https://github.com/CollapseLauncher/Hi3Helper.Plugin.Core/blob/main/Management/Api/ILauncherApiMedia.cs) (Inherit: ``ILauncherApi``)
  * [``ILauncherApiNews``](https://github.com/CollapseLauncher/Hi3Helper.Plugin.Core/blob/main/Management/Api/ILauncherApiNews.cs) (Inherit: ``ILauncherApi``)

* COM Interop, Marshallers and Extensions
  * Asynchronous Task Marshaller with ``Exception`` throw and ``CancellationToken`` support (via: [``ComAsyncResult``](https://github.com/CollapseLauncher/Hi3Helper.Plugin.Core/blob/main/ComAsyncResult.cs) and [``ComAsyncExtension``](https://github.com/CollapseLauncher/Hi3Helper.Plugin.Core/blob/main/Utility/ComAsyncExtension.cs))
  * COM-Interop ABI For Manual Marshalling to Support ``No-Reflection`` mode if ``MANUALCOM`` "constant define" included inside the ``.csproj`` file (via: [``ABI\ABI_I***Wrappers``](https://github.com/CollapseLauncher/Hi3Helper.Plugin.Core/tree/main/ABI))
    > Source-generated COM ABI and Wrapper is used by default if ``MANUALCOM`` isn't defined.
  * Disposable Plugin Memory for managing data in unmanaged memory between the plugin and the main application (via: [``PluginDisposableMemory``](https://github.com/CollapseLauncher/Hi3Helper.Plugin.Core/blob/main/PluginDisposableMemory.cs))
  * Memory and String Tools (via: [``Mem``](https://github.com/CollapseLauncher/Hi3Helper.Plugin.Core/blob/main/Utility/Mem.cs) and [``Mem.String``](https://github.com/CollapseLauncher/Hi3Helper.Plugin.Core/blob/main/Utility/Mem.String.cs))
  * Lightweight/No-Reflection Supported JSON Serializer/Deserializer Contracts (using: ``JsonDocument``, ``Utf8JsonWriter`` and ``Utf8JsonReader``) via:
    * [``IJsonElementParsable<out T>``](https://github.com/CollapseLauncher/Hi3Helper.Plugin.Core/blob/main/Utility/Json/IJsonElementParsable.cs)
    * [``IJsonStreamParsable<T>``](https://github.com/CollapseLauncher/Hi3Helper.Plugin.Core/blob/main/Utility/Json/IJsonStreamParsable.cs)
    * [``IJsonStreamSerializable<in T>``](https://github.com/CollapseLauncher/Hi3Helper.Plugin.Core/blob/main/Utility/Json/IJsonStreamSerializable.cs)
    * [``IJsonStringSerializable<in T>``](https://github.com/CollapseLauncher/Hi3Helper.Plugin.Core/blob/main/Utility/Json/IJsonStringSerializable.cs)
    * [``IJsonWriterSerializable<in T>``](https://github.com/CollapseLauncher/Hi3Helper.Plugin.Core/blob/main/Utility/Json/IJsonWriterSerializable.cs)

# How to Start Developing Plugins
### TODO: Make an entire Wiki and documentation for How-to-use and stuff.

# Plugin Examples
To see the example of how the plugin implemented using this Core Library, check the link below:
* [Hi3Helper.Plugin.HBR](https://github.com/CollapseLauncher/Hi3Helper.Plugin.HBR) (A basic plugin implementation for Game: [Heaven Burns Red](https://heavenburnsred.yo-star.com/) by [Key](https://key.visualarts.gr.jp/))
* [Hi3Helper.Plugin.Wuwa](https://github.com/CollapseLauncher/Hi3Helper.Plugin.Wuwa) (A plugin implementation for [Wuthering Waves](https://wutheringwaves.kurogames.com/en/main) by [Kuro Games](https://kurogames.com))
* [Hi3Helper.Plugin.DNA](https://github.com/CollapseLauncher/Hi3Helper.Plugin.DNA) (A plugin implementation for [Duet Night Abyss](https://duetnightabyss.dna-panstudio.com//) by [Hero Games](https://herogame.com/))