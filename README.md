# Collapse Launcher Plugin Core Library
This is the main repository of the ``Hi3Helper.Plugin.Core`` library (aka Collapse Launcher Standard Core Plugin Library), broadly used as a fundamental to develop Game Plugin Support system on Collapse Launcher, implements standard of API contracts and core functionality which is used by both the plugin and the launcher, including: Platform Invocation, COM Interop and Marshalling.

# How to Contribute
As per current state of the Plugin System on Collapse Launcher, you can contribute to this library by providing a proposal of the new API contract or by improving the existing API contract implementation. You might expect some changes in the near future as the existing APIs are still under development.

Keep in mind that the code included in this repository are mainly unsafe due to marshalling nature from Managed .NET code to Unmanaged code platform invocation.

# How to Start Developing Plugins
### TODO: Make an entire Wiki and documentation for How-to-use and stuff.

# Plugin Examples
To see the example of how the plugin implemented using this Core Library, check the link below:
* [Hi3Helper.Plugin.HBR](https://github.com/CollapseLauncher/Hi3Helper.Plugin.HBR) (A basic plugin implementation for Game: [Heaven Burns Red](https://heavenburnsred.yo-star.com/) by [Key](https://key.visualarts.gr.jp/))
* [Hi3Helper.Plugin.Template](https://github.com/CollapseLauncher/Hi3Helper.Plugin.Template) (A template for Plugin Development example, also a plugin implementation for [Wuthering Waves](https://wutheringwaves.kurogames.com/en/main) by [Kuro Games](https://kurogames.com))