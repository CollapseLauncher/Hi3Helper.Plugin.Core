// ReSharper disable UnusedMember.Global
namespace Hi3Helper.Plugin.Core.Management;

/// <summary>
/// Defines kind of game release. This is just for cosmetic purposes which is used to display necessary messages on the launcher.
/// </summary>
public enum GameReleaseChannel
{
    /// <summary>
    /// The game is currently Globally released.
    /// </summary>
    Public,

    /// <summary>
    /// The game is currently in Open-beta release. 
    /// </summary>
    OpenBeta,

    /// <summary>
    /// The game is currently in Closed-beta/Non-disclosure release/Development release.
    /// </summary>
    ClosedBeta
}
