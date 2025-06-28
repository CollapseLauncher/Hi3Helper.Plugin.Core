#if MANUALCOM

using Hi3Helper.Plugin.Core.Management;
using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo
// ReSharper disable InconsistentNaming

namespace Hi3Helper.Plugin.Core.ABI;

// Reference:
// https://github.com/dotnet/samples/blob/main/core/interop/comwrappers/Tutorial/Program.cs
public sealed unsafe class GameInstallerWrappers : ComWrappers
{
    public static readonly ComInterfaceEntry* InterfaceDefinitions_IGameInstaller;
    public static readonly ComInterfaceEntry* InterfaceDefinitions_IGameUninstaller;
    private const int ExInterfaceDefinitionsLen_IGameInstaller = 4; // IGameInstaller, IGameUninstaller, IInitializable, IFree.
    private const int ExInterfaceDefinitionsLen_IGameUninstaller = 3; // IGameUninstaller, IInitializable, IFree.

    static GameInstallerWrappers()
    {
        {
            ComInterfaceEntry* p_IGameInstaller = (ComInterfaceEntry*)RuntimeHelpers.AllocateTypeAssociatedMemory(
                typeof(GameInstallerWrappers),
                sizeof(ComInterfaceEntry) * ExInterfaceDefinitionsLen_IGameInstaller);

            // IGameInstaller
            p_IGameInstaller[0].IID = new Guid(ComInterfaceId.ExGameInstaller);
            p_IGameInstaller[0].Vtable = (nint)ABI_VTables.IGameInstaller;
            p_IGameInstaller[1].IID = new Guid(ComInterfaceId.ExGameUninstaller);
            p_IGameInstaller[1].Vtable = (nint)ABI_VTables.IGameUninstaller;
            p_IGameInstaller[2].IID = new Guid(ComInterfaceId.ExInitializable);
            p_IGameInstaller[2].Vtable = (nint)ABI_VTables.IInitializableTask;
            p_IGameInstaller[3].IID = new Guid(ComInterfaceId.ExFree);
            p_IGameInstaller[3].Vtable = (nint)ABI_VTables.IFree;

            InterfaceDefinitions_IGameInstaller = p_IGameInstaller;
        }

        {
            ComInterfaceEntry* p_IGameUninstaller = (ComInterfaceEntry*)RuntimeHelpers.AllocateTypeAssociatedMemory(
                typeof(GameInstallerWrappers),
                sizeof(ComInterfaceEntry) * ExInterfaceDefinitionsLen_IGameUninstaller);

            // IGameUninstaller
            p_IGameUninstaller[0].IID = new Guid(ComInterfaceId.ExGameUninstaller);
            p_IGameUninstaller[0].Vtable = (nint)ABI_VTables.IGameUninstaller;
            p_IGameUninstaller[1].IID = new Guid(ComInterfaceId.ExInitializable);
            p_IGameUninstaller[1].Vtable = (nint)ABI_VTables.IInitializableTask;
            p_IGameUninstaller[2].IID = new Guid(ComInterfaceId.ExFree);
            p_IGameUninstaller[2].Vtable = (nint)ABI_VTables.IFree;

            InterfaceDefinitions_IGameUninstaller = p_IGameUninstaller;
        }
    }

    protected override ComInterfaceEntry* ComputeVtables(object obj, CreateComInterfaceFlags flags, out int count)
    {
        Debug.Assert(flags is CreateComInterfaceFlags.None);

        switch (obj)
        {
            case IGameInstaller:
                count = ExInterfaceDefinitionsLen_IGameInstaller;
                return InterfaceDefinitions_IGameInstaller;
            case IGameUninstaller:
                count = ExInterfaceDefinitionsLen_IGameUninstaller;
                return InterfaceDefinitions_IGameUninstaller;
            default:
                // Note: this implementation does not handle cases where the passed in object implements
                // one or both of the supported interfaces but is not the expected .NET class.
                count = 0;
                return null;
        }
    }

    protected override object CreateObject(IntPtr externalComObject, CreateObjectFlags flags)
        => throw new NotSupportedException();

    protected override void ReleaseObjects(IEnumerable objects)
        => throw new NotSupportedException();
}

#endif