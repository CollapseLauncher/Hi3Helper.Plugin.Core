using Hi3Helper.Plugin.Core.Utility;
using System.Runtime.InteropServices;

namespace Hi3Helper.Plugin.Core.Management.Api;

/// <summary>
/// Entry of the path used by the launcher assets.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct LauncherPathEntry
{
    public const int FileHashMaxLength = 64;
    public const int PathMaxLength     = 956;

    /// <summary>
    /// The local path of where the asset is stored.
    /// </summary>
    public fixed char Path[PathMaxLength];

    /// <summary>
    /// The hash of the file. This is used to verify the integrity of the file.
    /// </summary>
    public fixed byte FileHash[FileHashMaxLength];

    /// <summary>
    /// The actual length of the <see cref="FileHash"/> buffer.
    /// </summary>
    public int FileHashLength;

    /// <summary>
    /// The next entry of the path. This should be non-null if multiple entries are available.
    /// </summary>
    public nint NextEntry;

    /// <summary>
    /// Get the string of the Path from the handle.
    /// </summary>
    /// <param name="handle">The pointer to the handle</param>
    /// <returns>A string represents the value of Path field.</returns>
    public static string GetStringFromHandle(nint handle)
    {
        if (handle == nint.Zero)
        {
            return string.Empty;
        }

        LauncherPathEntry* entry = handle.AsPointer<LauncherPathEntry>();
        char* path = entry->Path;
        return Mem.CreateSpanFromNullTerminated<char>(path).ToString(); 
    }

    /// <summary>
    /// Get the handle of the next entry and freed the current handle passed into the <paramref name="handle"/> argument.
    /// </summary>
    /// <param name="handle">The handle of the current entry.</param>
    /// <returns>The handle to the next entry. It will return <see cref="nint.Zero"/> if no entries left.</returns>
    public static nint GetNextHandleAndFreed(nint handle)
    {
        nint nextHandle = GetNextHandle(handle);
        Mem.Free(handle);

        return nextHandle;
    }

    /// <summary>
    ///  Get the handle of the next entry from the current handle passed into the <paramref name="handle"/> argument.
    /// </summary>
    /// <param name="handle">The handle of the current entry.</param>
    /// <returns>The handle to the next entry. It will return <see cref="nint.Zero"/> if no entries left.</returns>
    public static nint GetNextHandle(nint handle)
    {
        LauncherPathEntry* entry = handle.AsPointer<LauncherPathEntry>();
        return entry->NextEntry;
    }

    /// <summary>
    /// Get the count of the entries from the handle.
    /// </summary>
    /// <param name="handle">The handle of the current entry.</param>
    /// <returns>Count of entries available from the current handle.</returns>
    public static int GetCountFromHandle(nint handle)
    {
        int count = 0;
        LauncherPathEntry* entry = handle.AsPointer<LauncherPathEntry>();
        while (entry != null)
        {
            count++;
            entry = entry->NextEntry.AsPointer<LauncherPathEntry>();
        }

        return count;
    }
}
