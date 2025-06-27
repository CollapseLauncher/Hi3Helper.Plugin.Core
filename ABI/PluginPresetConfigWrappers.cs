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

namespace Hi3Helper.Plugin.Core.ABI;

// Reference:
// https://github.com/dotnet/samples/blob/main/core/interop/comwrappers/Tutorial/Program.cs
public sealed unsafe class PluginPresetConfigWrappers : ComWrappers
{
    public static readonly ComInterfaceEntry* InterfaceDefinitions;
    private const int ExInterfaceDefinitionsLen = 3; // Only count IPlugin and IFree for now.

    static PluginPresetConfigWrappers()
    {
        IntPtr sIPluginPresetConfigVTable;
        IntPtr sIInitializableTaskVTable;
        IntPtr sIFreeVTable;

        // Get methods for IUnknown interface query on runtime.
        GetIUnknownImpl(
            out IntPtr fpQueryInterface,
            out IntPtr fpAddRef,
            out IntPtr fpRelease);

        // Assign VTables for IPluginPresetConfig
        {
            void** vtable = (void**)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(IPluginPresetConfig), sizeof(void*) * 27);
            vtable[0] = (void*)fpQueryInterface;
            vtable[1] = (void*)fpAddRef;
            vtable[2] = (void*)fpRelease;

            /* =======================================================================================================================
             * NOTE FOR PLUGIN DEVELOPERS
             * =======================================================================================================================
             * To find which type being used for the delegated arguments, please refer to the source-generated code from GeneratedComInterface
             */
            vtable[3] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, int>)&ABI_IFreeWrapper.ABI_Free;
            vtable[4] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, Guid*, nint*, int>)&ABI_IInitializableTask.ABI_InitAsync;
            vtable[5] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort**, int>)&ABI_IPluginPresetConfigWrapper.ABI_comGet_GameName;
            vtable[6] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort**, int>)&ABI_IPluginPresetConfigWrapper.ABI_comGet_ProfileName;
            vtable[7] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort**, int>)&ABI_IPluginPresetConfigWrapper.ABI_comGet_ZoneDescription;
            vtable[8] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort**, int>)&ABI_IPluginPresetConfigWrapper.ABI_comGet_ZoneName;
            vtable[9] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort**, int>)&ABI_IPluginPresetConfigWrapper.ABI_comGet_ZoneFullName;
            vtable[10] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort**, int>)&ABI_IPluginPresetConfigWrapper.ABI_comGet_ZoneLogoUrl;
            vtable[11] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort**, int>)&ABI_IPluginPresetConfigWrapper.ABI_comGet_ZonePosterUrl;
            vtable[12] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort**, int>)&ABI_IPluginPresetConfigWrapper.ABI_comGet_ZoneHomePageUrl;
            vtable[13] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, GameReleaseChannel*, int>)&ABI_IPluginPresetConfigWrapper.ABI_comGet_ReleaseChannel;
            vtable[14] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort**, int>)&ABI_IPluginPresetConfigWrapper.ABI_comGet_GameMainLanguage;
            vtable[15] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, int*, int>)&ABI_IPluginPresetConfigWrapper.ABI_comGet_GameSupportedLanguagesCount;
            vtable[16] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, int, ushort**, int>)&ABI_IPluginPresetConfigWrapper.ABI_comGet_GameSupportedLanguages;
            vtable[17] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort**, int>)&ABI_IPluginPresetConfigWrapper.ABI_comGet_GameExecutableName;
            vtable[18] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort**, int>)&ABI_IPluginPresetConfigWrapper.ABI_comGet_GameLogFileName;
            vtable[19] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort**, int>)&ABI_IPluginPresetConfigWrapper.ABI_comGet_GameAppDataPath;
            vtable[20] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort**, int>)&ABI_IPluginPresetConfigWrapper.ABI_comGet_LauncherGameDirectoryName;
            vtable[21] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort**, int>)&ABI_IPluginPresetConfigWrapper.ABI_comGet_GameVendorName;
            vtable[22] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, ushort**, int>)&ABI_IPluginPresetConfigWrapper.ABI_comGet_GameRegistryKeyName;
            vtable[23] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, void**, int>)&ABI_IPluginPresetConfigWrapper.ABI_comGet_LauncherApiMedia;
            vtable[24] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, void**, int>)&ABI_IPluginPresetConfigWrapper.ABI_comGet_LauncherApiNews;
            vtable[25] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, void**, int>)&ABI_IPluginPresetConfigWrapper.ABI_comGet_GameManager;
            vtable[26] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, void**, int>)&ABI_IPluginPresetConfigWrapper.ABI_comGet_GameInstaller;

            sIPluginPresetConfigVTable = (IntPtr)vtable;
        }

        // ReSharper disable once CommentTypo
        // Assign VTables for IInitializableTask
        {
            void** vtable = (void**)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(IInitializableTask), sizeof(void*) * 5);

            vtable[0] = (void*)fpQueryInterface;
            vtable[1] = (void*)fpAddRef;
            vtable[2] = (void*)fpRelease;

            /* =======================================================================================================================
             * NOTE FOR PLUGIN DEVELOPERS
             * =======================================================================================================================
             * To find which type being used for the delegated arguments, please refer to the source-generated code from GeneratedComInterface
             */
            vtable[3] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, int>)&ABI_IFreeWrapper.ABI_Free;
            vtable[4] = (delegate* unmanaged[MemberFunction]<ComInterfaceDispatch*, Guid*, nint*, int>)&ABI_IInitializableTask.ABI_InitAsync;

            sIInitializableTaskVTable = (IntPtr)vtable;
        }

        // Assign VTables for IFree
        {
            void** vtable = (void**)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(IFree), sizeof(void*) * 4);

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

            // IPluginPresetConfig
            entries[0].IID = new Guid(ComInterfaceId.ExPluginPresetConfig);
            entries[0].Vtable = sIPluginPresetConfigVTable;

            // IInitializableTask
            entries[1].IID = new Guid(ComInterfaceId.ExInitializable);
            entries[1].Vtable = sIInitializableTaskVTable;

            // IFree
            entries[2].IID = new Guid(ComInterfaceId.ExFree);
            entries[2].Vtable = sIFreeVTable;

            InterfaceDefinitions = entries;
        }
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