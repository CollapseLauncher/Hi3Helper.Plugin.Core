#if MANUALCOM

using Hi3Helper.Plugin.Core.Management.Api;
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
public sealed unsafe class LauncherApiWrappers : ComWrappers
{
    public static readonly ComInterfaceEntry* InterfaceDefinitions_ILauncherApiMedia;
    public static readonly ComInterfaceEntry* InterfaceDefinitions_ILauncherApiNews;
    public static readonly ComInterfaceEntry* InterfaceDefinitions_ILauncherApi;
    private const int ExInterfaceDefinitionsLen_ILauncherApiMedia = 4; // ILauncherApiMedia, ILauncherApi, IInitializableTask, IFree.
    private const int ExInterfaceDefinitionsLen_ILauncherApiNews = 4; // ILauncherApiNews, ILauncherApi, IInitializableTask, IFree.
    private const int ExInterfaceDefinitionsLen_ILauncherApi = 3; // ILauncherApi, IInitializableTask, IFree.

    static LauncherApiWrappers()
    {
        {
            ComInterfaceEntry* p_ILauncherApiMedia = (ComInterfaceEntry*)RuntimeHelpers.AllocateTypeAssociatedMemory(
                typeof(LauncherApiWrappers),
                sizeof(ComInterfaceEntry) * ExInterfaceDefinitionsLen_ILauncherApiMedia);

            // ILauncherApiMedia
            p_ILauncherApiMedia[0].IID = new Guid(ComInterfaceId.ExLauncherApiMedia);
            p_ILauncherApiMedia[0].Vtable = (nint)ABI_VTables.ILauncherApiMedia;
            p_ILauncherApiMedia[1].IID = new Guid(ComInterfaceId.ExLauncherApi);
            p_ILauncherApiMedia[1].Vtable = (nint)ABI_VTables.ILauncherApi;
            p_ILauncherApiMedia[2].IID = new Guid(ComInterfaceId.ExInitializable);
            p_ILauncherApiMedia[2].Vtable = (nint)ABI_VTables.IInitializableTask;
            p_ILauncherApiMedia[3].IID = new Guid(ComInterfaceId.ExFree);
            p_ILauncherApiMedia[3].Vtable = (nint)ABI_VTables.IFree;

            InterfaceDefinitions_ILauncherApiMedia = p_ILauncherApiMedia;
        }

        {
            ComInterfaceEntry* p_ILauncherApiNews = (ComInterfaceEntry*)RuntimeHelpers.AllocateTypeAssociatedMemory(
                typeof(LauncherApiWrappers),
                sizeof(ComInterfaceEntry) * ExInterfaceDefinitionsLen_ILauncherApiNews);

            // ILauncherApiNews
            p_ILauncherApiNews[0].IID = new Guid(ComInterfaceId.ExLauncherApiNewsFeed);
            p_ILauncherApiNews[0].Vtable = (nint)ABI_VTables.ILauncherApiNews;
            p_ILauncherApiNews[1].IID = new Guid(ComInterfaceId.ExLauncherApi);
            p_ILauncherApiNews[1].Vtable = (nint)ABI_VTables.ILauncherApi;
            p_ILauncherApiNews[2].IID = new Guid(ComInterfaceId.ExInitializable);
            p_ILauncherApiNews[2].Vtable = (nint)ABI_VTables.IInitializableTask;
            p_ILauncherApiNews[3].IID = new Guid(ComInterfaceId.ExFree);
            p_ILauncherApiNews[3].Vtable = (nint)ABI_VTables.IFree;

            InterfaceDefinitions_ILauncherApiNews = p_ILauncherApiNews;
        }

        {
            ComInterfaceEntry* p_ILauncherApi = (ComInterfaceEntry*)RuntimeHelpers.AllocateTypeAssociatedMemory(
                typeof(LauncherApiWrappers),
                sizeof(ComInterfaceEntry) * ExInterfaceDefinitionsLen_ILauncherApi);

            // ILauncherApiNews
            p_ILauncherApi[0].IID = new Guid(ComInterfaceId.ExLauncherApi);
            p_ILauncherApi[0].Vtable = (nint)ABI_VTables.ILauncherApi;
            p_ILauncherApi[1].IID = new Guid(ComInterfaceId.ExInitializable);
            p_ILauncherApi[1].Vtable = (nint)ABI_VTables.IInitializableTask;
            p_ILauncherApi[2].IID = new Guid(ComInterfaceId.ExFree);
            p_ILauncherApi[2].Vtable = (nint)ABI_VTables.IFree;

            InterfaceDefinitions_ILauncherApi = p_ILauncherApi;
        }
    }

    protected override ComInterfaceEntry* ComputeVtables(object obj, CreateComInterfaceFlags flags, out int count)
    {
        Debug.Assert(flags is CreateComInterfaceFlags.None);

        switch (obj)
        {
            case ILauncherApiMedia:
                count = ExInterfaceDefinitionsLen_ILauncherApiMedia;
                return InterfaceDefinitions_ILauncherApiMedia;
            case ILauncherApiNews:
                count = ExInterfaceDefinitionsLen_ILauncherApiNews;
                return InterfaceDefinitions_ILauncherApiNews;
            case ILauncherApi:
                count = ExInterfaceDefinitionsLen_ILauncherApi;
                return InterfaceDefinitions_ILauncherApi;
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