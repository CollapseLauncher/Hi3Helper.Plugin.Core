#if MANUALCOM

using System;
using System.Collections;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Hi3Helper.Plugin.Core.ABI;

// Reference:
// https://github.com/dotnet/samples/blob/main/core/interop/comwrappers/Tutorial/Program.cs
public sealed unsafe class PluginWrappers : ComWrappers
{
    public static readonly ComInterfaceEntry* InterfaceDefinitions;
    private const int ExInterfaceDefinitionsLen = 2; // Only count IPlugin and IFree for now.

    static PluginWrappers()
    {
        ComInterfaceEntry* entries = (ComInterfaceEntry*)RuntimeHelpers.AllocateTypeAssociatedMemory(
            typeof(PluginWrappers),
            sizeof(ComInterfaceEntry) * ExInterfaceDefinitionsLen);

        // IPlugin
        entries[0].IID = new Guid(ComInterfaceId.ExPlugin);
        entries[0].Vtable = (nint)ABI_VTables.IPlugin;

        // IFree
        entries[1].IID = new Guid(ComInterfaceId.ExFree);
        entries[1].Vtable = (nint)ABI_VTables.IFree;

        InterfaceDefinitions = entries;
    }

    protected override ComInterfaceEntry* ComputeVtables(object obj, CreateComInterfaceFlags flags, out int count)
    {
        Debug.Assert(flags is CreateComInterfaceFlags.None);

        if (obj is IPlugin)
        {
            count = ExInterfaceDefinitionsLen;
            return InterfaceDefinitions;
        }

        // Note: this implementation does not handle cases where the passed in object implements
        // one or both of the supported interfaces but is not the expected .NET class.
        count = 0;
        return null;
    }

    protected override object CreateObject(nint externalComObject, CreateObjectFlags flags)
        => throw new NotSupportedException();

    protected override void ReleaseObjects(IEnumerable objects)
        => throw new NotSupportedException();
}

#endif