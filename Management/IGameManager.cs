using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Hi3Helper.Plugin.Core.Management;

[GeneratedComInterface]
[Guid(ComInterfaceId.GameManager)]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
public partial interface IGameManager
{
    [return: MarshalAs(UnmanagedType.LPWStr)]
    string GetGamePath();

    void SaveGamePath([MarshalAs(UnmanagedType.LPWStr)] string gamePath);

    [return: MarshalAs(UnmanagedType.Interface)]
    IVersion GetCurrentGameVersion();

    void SetCurrentGameVersion([MarshalAs(UnmanagedType.Interface)] IVersion version);

    [return: MarshalAs(UnmanagedType.Bool)]
    bool IsGameInstalled();

    [return: MarshalAs(UnmanagedType.Bool)]
    bool IsGameHasUpdate();

    [return: MarshalAs(UnmanagedType.Bool)]
    bool IsGameHasPreload();
}
