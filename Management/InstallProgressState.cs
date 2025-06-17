using System;
// ReSharper disable UnusedMember.Global

namespace Hi3Helper.Plugin.Core.Management;

/// <summary>
/// Defines a state of the current installation progress.
/// </summary>
[Flags]
public enum InstallProgressState
{
    /// <summary>
    /// Installation process is currently on idle.
    /// </summary>
    Idle = 0b_00000000,

    /// <summary>
    /// Installation process is downloading assets.
    /// </summary>
    Download = 0b_00000001,

    /// <summary>
    /// Installation process is on installing state, which includes extraction of the archive or applying data on assets.
    /// </summary>
    Install = 0b_00000010,

    /// <summary>
    /// Installation process is verifying assets. This usually happens when the installation process required to verify the assets or archives before extracting/installing.
    /// </summary>
    Verify = 0b_00000100,

    /// <summary>
    /// Installation process is removing some assets. This usually happens when the installation process involves a clean-up process to unused assets.
    /// </summary>
    Removing = 0b_00001000,

    /// <summary>
    /// Installation process is on preparing state. This usually happens when the installation process required to prepare/fetch the API or References data before any operations can be run.
    /// </summary>
    Preparing = 0b_00010000,

    /// <summary>
    /// Installation process is on updating state, which includes extraction of the archive or applying data on assets.<br/>
    /// This state is the same as <see cref="InstallProgressState.Install"/>, but for updates.
    /// </summary>
    Updating = 0b_00100000,

    /// <summary>
    /// Installation process has been completed.
    /// </summary>
    Completed = 0b_01000000
}
