using Hi3Helper.Plugin.Core.Utility;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
// ReSharper disable ConvertToAutoProperty

namespace Hi3Helper.Plugin.Core.Management.Api;

/// <summary>
/// Entry of the path used by the launcher assets.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
public unsafe struct LauncherPathEntry()
    : IDisposable, IInitializableStruct
{
    public const int ExPathMaxLength     = 1024;
    public const int ExFileHashMaxLength = 64;

    private char* _path           = null;
    private byte* _fileHash       = null;
    private int   _fileHashLength;

    public void InitInner()
    {
        _path     = Mem.Alloc<char>(ExPathMaxLength);
        _fileHash = Mem.Alloc<byte>(ExFileHashMaxLength);
    }

    /// <summary>
    /// The local path of where the asset is stored.
    /// </summary>
    public readonly PluginDisposableMemory<char> Path => new(_path, ExPathMaxLength);

    /// <summary>
    /// The hash of the file. This is used to verify the integrity of the file.
    /// </summary>
    public readonly PluginDisposableMemory<byte> FileHash => new(_fileHash, ExFileHashMaxLength);

    /// <summary>
    /// The actual length of the <see cref="FileHash"/> buffer.
    /// </summary>
    public int FileHashLength { readonly get => _fileHashLength; set => _fileHashLength = value; }

    public readonly void Dispose()
    {
        Mem.Free(_path);
        Mem.Free(_fileHash);
    }

    /// <summary>
    /// Get the string of <see cref="Path"/> field.
    /// </summary>
    /// <returns>The string of <see cref="Path"/> field.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly string GetPathString()
        => Path.CreateStringFromNullTerminated();
}
