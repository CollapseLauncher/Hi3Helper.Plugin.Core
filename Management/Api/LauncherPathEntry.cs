using System;
using System.Runtime.InteropServices;

namespace Hi3Helper.Plugin.Core.Management.Api;

/// <summary>
/// Entry of the path used by the launcher assets.
/// </summary>
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

    public static string GetStringFromHandle(nint handle)
    {
        if (handle == nint.Zero)
        {
            return string.Empty;
        }

        LauncherPathEntry* entry = (LauncherPathEntry*)handle;
        char* path = entry->Path;
        return MemoryMarshal.CreateReadOnlySpanFromNullTerminated(path).ToString(); 
    }

    public static nint GetNextHandleAndFreed(nint handle)
    {
        nint nextHandle = GetNextHandle(handle);
        NativeMemory.Free((void*)handle);

        return nextHandle;
    }

    public static nint GetNextHandle(nint handle)
    {
        LauncherPathEntry* entry = (LauncherPathEntry*)handle;
        return entry->NextEntry;
    }

    public static int GetCountFromHandle(nint handle)
    {
        int count = 0;
        LauncherPathEntry* entry = (LauncherPathEntry*)handle;
        while (entry != null)
        {
            count++;
            entry = (LauncherPathEntry*)entry->NextEntry;
        }

        return count;
    }
}
