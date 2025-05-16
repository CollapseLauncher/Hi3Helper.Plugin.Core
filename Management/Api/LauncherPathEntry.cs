using System;
using System.Runtime.InteropServices;

namespace Hi3Helper.Plugin.Core.Management.Api;

/// <summary>
/// Entry of the path used by the launcher assets.
/// </summary>
public unsafe struct LauncherPathEntry
{
    /// <summary>
    /// The local path of where the asset is stored.
    /// </summary>
    public fixed char Path[1024];

    /// <summary>
    /// The next entry of the path. This should be non-null if multiple entries are available.
    /// </summary>
    public LauncherPathEntry* NextEntry;

    /// <summary>
    /// Gets the path as a <see cref="ReadOnlySpan{Char}"/>.
    /// </summary>
    /// <returns>The read-only span of <see cref="char"/></returns>
    public ReadOnlySpan<char> GetPathSpan()
    {
        fixed (char* pathPtr = Path)
        {
            return MemoryMarshal.CreateReadOnlySpanFromNullTerminated(pathPtr);
        }
    }
}
