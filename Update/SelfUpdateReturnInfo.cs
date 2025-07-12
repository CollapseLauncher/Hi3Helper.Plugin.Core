using Hi3Helper.Plugin.Core.Management;
using Hi3Helper.Plugin.Core.Utility;
using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

namespace Hi3Helper.Plugin.Core.Update;

/// <summary>
/// Returns a struct which contains the information about the update of a plugin.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 8, Size = 64)]
public unsafe struct SelfUpdateReturnInfo : IDisposable
{
    // ReSharper disable once ConvertToPrimaryConstructor
    public SelfUpdateReturnInfo(SelfUpdateReturnCode returnCode) => _retCode = returnCode;

    private SelfUpdateReturnCode _retCode; // Offset: 0
    private byte* _infoPluginName; // Offset: 8
    private byte* _infoPluginAuthor; // Offset: 16
    private byte* _infoPluginDescription; // Offset: 24
    private GameVersion* _infoPluginVersion; // Offset: 32
    private GameVersion* _infoPluginStandardVersion; // Offset: 40
    private DateTimeOffset* _infoPluginCreationDate; // Offset: 48
    private DateTimeOffset* _infoPluginCompiledDate; // Offset: 56

    // 8 bytes preserved for another struct later.

    /// <summary>
    /// Gets the return code of the self-update operation.
    /// </summary>
    // ReSharper disable once ConvertToAutoPropertyWithPrivateSetter
    public readonly SelfUpdateReturnCode ReturnCode => _retCode;

    /// <summary>
    /// Gets the name of the upcoming plugin release.
    /// </summary>
    public readonly string? Name => Utf8StringMarshaller.ConvertToManaged(_infoPluginName);

    /// <summary>
    /// Gets the author name of the upcoming plugin release.
    /// </summary>
    public readonly string? Author => Utf8StringMarshaller.ConvertToManaged(_infoPluginAuthor);

    /// <summary>
    /// Gets the description of the upcoming plugin release.
    /// </summary>
    public readonly string? Description => Utf8StringMarshaller.ConvertToManaged(_infoPluginDescription);

    /// <summary>
    /// Gets the version of the upcoming plugin release.
    /// </summary>
    public readonly GameVersion? PluginVersion => _infoPluginVersion == null ? null : *_infoPluginVersion;

    /// <summary>
    /// Gets the interface standard version of the upcoming plugin release.
    /// </summary>
    public readonly GameVersion? StandardVersion => _infoPluginStandardVersion == null ? null : *_infoPluginStandardVersion;

    /// <summary>
    /// Gets the creation date of the upcoming plugin release.
    /// </summary>
    public readonly DateTimeOffset? CreationDate => _infoPluginCreationDate == null ? null : *_infoPluginCreationDate;

    /// <summary>
    /// Gets the compilation date of the upcoming plugin release.
    /// </summary>
    public readonly DateTimeOffset? CompiledDate => _infoPluginCompiledDate == null ? null : *_infoPluginCompiledDate;

    /// <summary>
    /// Create a struct of this <see cref="SelfUpdateReturnInfo"/> instance in a native memory.
    /// </summary>
    /// <param name="returnCode">The return code of the self-update operation.</param>
    /// <param name="name">Name of the upcoming plugin release provided by <c>manifest.json</c></param>
    /// <param name="author">The author name of the upcoming plugin release provided by <c>manifest.json</c></param>
    /// <param name="description">The description of the upcoming plugin release provided by <c>manifest.json</c></param>
    /// <param name="pluginVersion">The version of the upcoming plugin release provided by <c>manifest.json</c></param>
    /// <param name="standardVersion">The interface standard version of the upcoming plugin release provided by <c>manifest.json</c></param>
    /// <param name="creationDate">The creation date of the upcoming plugin release provided by <c>manifest.json</c></param>
    /// <param name="compiledDate">The compilation date of the upcoming plugin release provided by <c>manifest.json</c></param>
    /// <returns>A native pointer to the <see cref="SelfUpdateReturnInfo"/> struct.</returns>
    public static nint CreateToNativeMemory(
        SelfUpdateReturnCode returnCode,
        string? name,
        string? author,
        string? description,
        GameVersion pluginVersion,
        GameVersion standardVersion,
        DateTimeOffset creationDate,
        DateTimeOffset compiledDate)
    {
        SelfUpdateReturnInfo* ptr = Mem.Alloc<SelfUpdateReturnInfo>(1, false);

        ptr->_retCode = returnCode;
        ptr->_infoPluginName = Utf8StringMarshaller.ConvertToUnmanaged(name);
        ptr->_infoPluginAuthor = Utf8StringMarshaller.ConvertToUnmanaged(author);
        ptr->_infoPluginDescription = Utf8StringMarshaller.ConvertToUnmanaged(description);
        ptr->_infoPluginVersion = pluginVersion.CopyStructToUnmanaged();
        ptr->_infoPluginStandardVersion = standardVersion.CopyStructToUnmanaged();
        ptr->_infoPluginCreationDate = creationDate.CopyStructToUnmanaged();
        ptr->_infoPluginCompiledDate = compiledDate.CopyStructToUnmanaged();

        return (nint)ptr;
    }

    /// <summary>
    /// Create a struct of this <see cref="SelfUpdateReturnInfo"/> instance in a native memory but only providing the return code of <see cref="SelfUpdateReturnCode"/>.
    /// </summary>
    /// <param name="returnCode">The return code of the self-update operation.</param>
    /// <returns>A native pointer to the <see cref="SelfUpdateReturnInfo"/> struct.</returns>
    public static nint CreateToNativeMemory(SelfUpdateReturnCode returnCode)
    {
        SelfUpdateReturnInfo* ptr = Mem.Alloc<SelfUpdateReturnInfo>();
        ptr->_retCode = returnCode;
        return (nint)ptr;
    }

    public void Dispose()
    {
        Utf8StringMarshaller.Free(_infoPluginName);
        Utf8StringMarshaller.Free(_infoPluginAuthor);
        Utf8StringMarshaller.Free(_infoPluginDescription);

        if (_infoPluginVersion != null)
        {
            Mem.Free(_infoPluginVersion);
            _infoPluginVersion = null;
        }

        if (_infoPluginStandardVersion != null)
        {
            Mem.Free(_infoPluginStandardVersion);
            _infoPluginStandardVersion = null;
        }

        if (_infoPluginCreationDate != null)
        {
            Mem.Free(_infoPluginCreationDate);
            _infoPluginCreationDate = null;
        }

        if (_infoPluginCompiledDate != null)
        {
            Mem.Free(_infoPluginCompiledDate);
            _infoPluginCompiledDate = null;
        }
    }
}