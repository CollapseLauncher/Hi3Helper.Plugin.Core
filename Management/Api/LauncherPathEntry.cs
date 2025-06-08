using Hi3Helper.Plugin.Core.Utility;
using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
// ReSharper disable ConvertToAutoProperty

namespace Hi3Helper.Plugin.Core.Management.Api;

/// <summary>
/// Entry of the path used by the launcher assets.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 8)]
public unsafe struct LauncherPathEntry()
    : IDisposable
{
    private byte  _isFreed        = 0;
    private int   _fileHashLength;
    private byte* _path           = null;
    private byte* _fileHash       = null;

    /// <summary>
    /// The local path of where the asset is stored.
    /// </summary>
    public string? Path => Utf8StringMarshaller.ConvertToManaged(_path);

    /// <summary>
    /// The hash of the file. This is used to verify the integrity of the file.
    /// </summary>
    public readonly PluginDisposableMemory<byte> FileHash => new(_fileHash, _fileHashLength);

    /// <summary>
    /// Write the strings into the current struct.
    /// </summary>
    /// <param name="path">The local path of where the asset is stored.</param>
    /// <param name="fileHash">The hash of the file. This is used to verify the integrity of the file.</param>
    public void Write(ReadOnlySpan<char> path, Span<byte> fileHash)
    {
        if (fileHash.IsEmpty)
        {
            _fileHash = null;
        }
        else
        {
            _fileHash = Mem.Alloc<byte>(fileHash.Length, false);
            _fileHashLength = fileHash.Length;

            fileHash.CopyTo(new Span<byte>(_fileHash, _fileHashLength));
        }

        _path = path.Utf16SpanToUtf8Unmanaged();
    }

    public void Dispose()
    {
        if (_isFreed == 1) return;

        Utf8StringMarshaller.Free(_path);
        Mem.Free(_fileHash);

        _isFreed = 1;
    }
}
