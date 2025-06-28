#if MANUALCOM

using Hi3Helper.Plugin.Core.Update;
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
public sealed unsafe class PluginSelfUpdateWrappers : ComWrappers
{
    public static readonly ComInterfaceEntry* InterfaceDefinitions;
    private const int ExInterfaceDefinitionsLen = 2; // IPluginSelfUpdate, IFree

    static PluginSelfUpdateWrappers()
    {
        ComInterfaceEntry* entries = (ComInterfaceEntry*)RuntimeHelpers.AllocateTypeAssociatedMemory(
            typeof(PluginSelfUpdateWrappers),
            sizeof(ComInterfaceEntry) * ExInterfaceDefinitionsLen);

        // IPluginSelfUpdate
        entries[0].IID = new Guid(ComInterfaceId.ExPluginSelfUpdate);
        entries[0].Vtable = (nint)ABI_VTables.IPluginSelfUpdate;

        // IFree
        entries[1].IID = new Guid(ComInterfaceId.ExFree);
        entries[1].Vtable = (nint)ABI_VTables.IFree;

        InterfaceDefinitions = entries;
    }

    protected override ComInterfaceEntry* ComputeVtables(object obj, CreateComInterfaceFlags flags, out int count)
    {
        Debug.Assert(flags is CreateComInterfaceFlags.None);

        if (obj is IPluginSelfUpdate)
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