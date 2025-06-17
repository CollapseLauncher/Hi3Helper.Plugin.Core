using System;
// ReSharper disable UnusedMember.Global

namespace Hi3Helper.Plugin.Core.Management;

/// <summary>
/// Gets or Sets which method is used to perform game installation routine.
/// </summary>
[Flags]
public enum GameInstallerKind
{
    /// <summary>
    /// Do nothing.
    /// </summary>
    None = 0b_00000000,

    /// <summary>
    /// Performs installation routine.
    /// </summary>
    Install = 0b_00000001,

    /// <summary>
    /// Performs update routine.
    /// </summary>
    Update = 0b_00000010,

    /// <summary>
    /// Performs preload routine.
    /// </summary>
    Preload = 0b_00000100
}
