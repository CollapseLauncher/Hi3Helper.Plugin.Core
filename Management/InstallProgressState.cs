﻿using System;
// ReSharper disable UnusedMember.Global

namespace Hi3Helper.Plugin.Core.Management;

[Flags]
public enum InstallProgressState
{
    Idle      = 0b_00000000,
    Download  = 0b_00000001,
    Install   = 0b_00000010,
    Verify    = 0b_00000100,
    Removing  = 0b_00001000,
    Preparing = 0b_00010000,
    Completed = 0b_00100000
}
