using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Hi3Helper.Plugin.Core.Management;

[GeneratedComInterface]
[Guid("39ae72f3-2269-420a-727f-000000000002")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public partial interface IPluginPresetConfig
{
    #region Generic Read-only Properties
    [return: MarshalAs(UnmanagedType.LPWStr)]
    string get_GameName();
    [return: MarshalAs(UnmanagedType.LPWStr)]
    string get_ProfileName();
    [return: MarshalAs(UnmanagedType.LPWStr)]
    string get_ZoneDescription();
    [return: MarshalAs(UnmanagedType.LPWStr)]
    string get_ZoneName();
    [return: MarshalAs(UnmanagedType.LPWStr)]
    string get_ZoneFullName();
    [return: MarshalAs(UnmanagedType.LPWStr)]
    string get_ZoneLogoUrl();
    [return: MarshalAs(UnmanagedType.LPWStr)]
    string get_ZonePosterUrl();
    [return: MarshalAs(UnmanagedType.LPWStr)]
    string get_ZoneHomePageUrl();
    [PreserveSig]
    GameReleaseChannel get_ReleaseChannel();

    [return: MarshalAs(UnmanagedType.LPWStr)]
    string get_GameMainLanguage();
    [PreserveSig]
    int get_GameSupportedLanguagesCount();
    [return: MarshalAs(UnmanagedType.LPWStr)]
    string get_GameSupportedLanguages(int index);

    [return: MarshalAs(UnmanagedType.LPWStr)]
    string get_GameExecutableName();
    [return: MarshalAs(UnmanagedType.LPWStr)]
    string get_LauncherGameDirectoryName();
    #endregion
}
