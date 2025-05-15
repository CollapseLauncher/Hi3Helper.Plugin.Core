using Hi3Helper.Plugin.Core.Utility;
using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Hi3Helper.Plugin.Core.Management;

[GeneratedComInterface]
[Guid(ComInterfaceId.PluginPresetConfig)]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public partial interface IPluginPresetConfig
{
    #region Return Callbacks
    public delegate void InitAsyncIsSuccessCallback(int result);
    #endregion

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

    /// <summary>
    /// Initialize the Preset config instance asynchronously.
    /// </summary>
    /// <param name="cancelToken"><see cref="Guid"/> instance for cancellation token</param>
    /// <param name="isSuccessReturnCallback"></param>
    /// <returns>A pointer to <see cref="ComAsyncResult"/>. You must call <see cref="ComAsyncExtension.WaitFromHandle(nint)"/> in order to await the method.</returns>
    nint InitAsync(in Guid cancelToken, InitAsyncIsSuccessCallback isSuccessReturnCallback);
    #endregion
}
