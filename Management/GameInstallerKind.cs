using System;

namespace Hi3Helper.Plugin.Core.Management;

[Flags]
public enum GameInstallerKind
{
    None    = 0b_00000000,
    Install = 0b_00000001,
    Update  = 0b_00000010,
    Preload = 0b_00000100
}
