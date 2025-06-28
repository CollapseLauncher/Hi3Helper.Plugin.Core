#if MANUALCOM

using Hi3Helper.Plugin.Core.Management;
using Hi3Helper.Plugin.Core.Management.PresetConfig;
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
public sealed unsafe class PluginPresetConfigWrappers : ComWrappers
{
    public static readonly ComInterfaceEntry* InterfaceDefinitions;
    private const int ExInterfaceDefinitionsLen = 3; // Only count IPlugin and IFree for now.

    static PluginPresetConfigWrappers()
    {
        ComInterfaceEntry* entries = (ComInterfaceEntry*)RuntimeHelpers.AllocateTypeAssociatedMemory(
            typeof(PluginPresetConfigWrappers),
            sizeof(ComInterfaceEntry) * ExInterfaceDefinitionsLen);

        // IPluginPresetConfig
        entries[0].IID = new Guid(ComInterfaceId.ExPluginPresetConfig);
        entries[0].Vtable = (nint)ABI_VTables.IPluginPresetConfig;

        // IInitializableTask
        entries[1].IID = new Guid(ComInterfaceId.ExInitializable);
        entries[1].Vtable = (nint)ABI_VTables.IInitializableTask;

        // IFree
        entries[2].IID = new Guid(ComInterfaceId.ExFree);
        entries[2].Vtable = (nint)ABI_VTables.IFree;

        InterfaceDefinitions = entries;
    }

    protected override ComInterfaceEntry* ComputeVtables(object obj, CreateComInterfaceFlags flags, out int count)
    {
        Debug.Assert(flags is CreateComInterfaceFlags.None);

        if (obj is IPluginPresetConfig)
        {
            count = ExInterfaceDefinitionsLen;
            return InterfaceDefinitions;
        }

        // Note: this implementation does not handle cases where the passed in object implements
        // one or both of the supported interfaces but is not the expected .NET class.
        count = 0;
        return null;
    }

    protected override object CreateObject(IntPtr externalComObject, CreateObjectFlags flags)
        => throw new NotSupportedException();

    protected override void ReleaseObjects(IEnumerable objects)
        => throw new NotSupportedException();
}

#endif