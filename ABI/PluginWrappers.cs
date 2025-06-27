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
        IntPtr sIPluginVTable;
        IntPtr sIFreeVTable;

        // Get methods for IUnknown interface query on runtime.
        GetIUnknownImpl(
            out IntPtr fpQueryInterface,
            out IntPtr fpAddRef,
            out IntPtr fpRelease);

        // Assign VTables for IPlugin
        {
            void** vtable = (void**)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(IPlugin), sizeof(void*) * 16);

            vtable[0] = (void*)fpQueryInterface;
            vtable[1] = (void*)fpAddRef;
            vtable[2] = (void*)fpRelease;

            /* =======================================================================================================================
             * NOTE FOR PLUGIN DEVELOPERS
             * =======================================================================================================================
             * To find which type being used for the delegated arguments, please refer to the source-generated code from GeneratedComInterface
             */
            vtable[3] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, int>)&ABI_IFreeWrapper.ABI_Free;
            vtable[4] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort**, int>)&ABI_IPluginWrapper.ABI_GetPluginName;
            vtable[5] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort**, int>)&ABI_IPluginWrapper.ABI_GetPluginDescription;
            vtable[6] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort**, int>)&ABI_IPluginWrapper.ABI_GetPluginAuthor;
            vtable[7] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, DateTime**, int>)&ABI_IPluginWrapper.ABI_GetPluginCreationDate;
            vtable[8] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, int*, int>)&ABI_IPluginWrapper.ABI_GetPresetConfigCount;
            vtable[9] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort**, int>)&ABI_IPluginWrapper.ABI_GetPluginAppIconUrl;
            vtable[10] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort**, int>)&ABI_IPluginWrapper.ABI_GetNotificationPosterUrl;
            vtable[11] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, int, void**, int>)&ABI_IPluginWrapper.ABI_GetPresetConfig;
            vtable[12] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, Guid*, int>)&ABI_IPluginWrapper.ABI_CancelAsync;
            vtable[13] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort*, ushort*, ushort*, int*, int>)&ABI_IPluginWrapper.ABI_SetPluginProxySettings;
            vtable[14] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort*, int>)&ABI_IPluginWrapper.ABI_SetPluginLocaleId;
            vtable[15] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, void**, int>)&ABI_IPluginWrapper.ABI_GetPluginSelfUpdater;

            sIPluginVTable = (IntPtr)vtable;
        }

        // Assign VTables for IFree
        {
            void** vtable = (void**)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(IPlugin), sizeof(void*) * 4);

            vtable[0] = (void*)fpQueryInterface;
            vtable[1] = (void*)fpAddRef;
            vtable[2] = (void*)fpRelease;

            /* =======================================================================================================================
             * NOTE FOR PLUGIN DEVELOPERS
             * =======================================================================================================================
             * To find which type being used for the delegated arguments, please refer to the source-generated code from GeneratedComInterface
             */
            vtable[3] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, int>)&ABI_IFreeWrapper.ABI_Free;

            sIFreeVTable = (IntPtr)vtable;
        }

        {
            ComInterfaceEntry* entries = (ComInterfaceEntry*)RuntimeHelpers.AllocateTypeAssociatedMemory(
                typeof(PluginWrappers),
                sizeof(ComInterfaceEntry) * ExInterfaceDefinitionsLen);

            // IPlugin
            entries[0].IID = new Guid(ComInterfaceId.ExPlugin);
            entries[0].Vtable = sIPluginVTable;

            // IFree
            entries[1].IID = new Guid(ComInterfaceId.ExFree);
            entries[1].Vtable = sIFreeVTable;

            InterfaceDefinitions = entries;
        }
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

    protected override object CreateObject(IntPtr externalComObject, CreateObjectFlags flags)
        => throw new NotSupportedException();

    protected override void ReleaseObjects(IEnumerable objects)
        => throw new NotSupportedException();
}

#endif