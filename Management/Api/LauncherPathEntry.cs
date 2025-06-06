﻿using Hi3Helper.Plugin.Core.Utility;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
// ReSharper disable ConvertToAutoProperty

namespace Hi3Helper.Plugin.Core.Management.Api;

/// <summary>
/// Entry of the path used by the launcher assets.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 8)]
public unsafe struct LauncherPathEntry()
    : IDisposable, IInitializableStruct
{
    public const int ExPathMaxLength     = 1024;
    public const int ExFileHashMaxLength = 64;

    private byte  _isFreed        = 0;
    private int   _fileHashLength;
    private byte* _path           = null;
    private byte* _fileHash       = null;

    public void InitInner()
    {
        _path     = Mem.Alloc<byte>(ExPathMaxLength);
        _fileHash = Mem.Alloc<byte>(ExFileHashMaxLength);
    }

    /// <summary>
    /// The local path of where the asset is stored.
    /// </summary>
    public readonly PluginDisposableMemory<byte> Path => new(_path, ExPathMaxLength);

    /// <summary>
    /// The hash of the file. This is used to verify the integrity of the file.
    /// </summary>
    public readonly PluginDisposableMemory<byte> FileHash => new(_fileHash, ExFileHashMaxLength);

    /// <summary>
    /// The actual length of the <see cref="FileHash"/> buffer.
    /// </summary>
    public int FileHashLength { readonly get => _fileHashLength; set => _fileHashLength = value; }

    public void Dispose()
    {
        if (_isFreed == 1) return;

        Mem.Free(_path);
        Mem.Free(_fileHash);

        _isFreed = 1;
    }

    /// <summary>
    /// Get the string of <see cref="Path"/> field.
    /// </summary>
    /// <returns>The string of <see cref="Path"/> field.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public readonly string? GetPathString()
        => Path.CreateStringFromNullTerminated();
}
