using Microsoft.Win32.SafeHandles;
using System.IO;
using System.Runtime.InteropServices;

namespace Hi3Helper.Plugin.Core.Utility;

/// <summary>
/// A struct contains the pointer of the <see cref="SafeFileHandle"/> or the safe handle of the <see cref="FileStream"/>.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public readonly struct FileHandle
{
    private FileHandle(nint unsafeHandle) => UnsafeHandle = unsafeHandle;

    internal readonly nint UnsafeHandle;

    public static implicit operator FileHandle(nint unsafeHandle) => new(unsafeHandle);

    public static implicit operator FileHandle(SafeFileHandle fileHandle) => fileHandle.DangerousGetHandle();

    public static implicit operator FileHandle(FileStream fileStream) => fileStream.SafeFileHandle;
}
