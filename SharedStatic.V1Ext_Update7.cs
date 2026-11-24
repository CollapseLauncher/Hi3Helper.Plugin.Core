using System.IO;
using Hi3Helper.Plugin.Core.Utility;
using Hi3Helper.Plugin.Core.Utility.Windows;

namespace Hi3Helper.Plugin.Core;

public partial class SharedStaticV1Ext
{
    /// <summary>
    /// Gets the full path of the plugin .dll file.
    /// </summary>
    public static unsafe string? CurrentPluginPath => field ??= PInvoke.GetModulePathFromAddr(PInvoke.ModulePathThisPluginAddr);

    /// <summary>
    /// Gets the full current working directory path of the plugin .dll file.
    /// </summary>
    public static unsafe string? CurrentPluginDirectory => field ??= Path.GetDirectoryName(CurrentPluginPath);

    /// <summary>
    /// Gets the full path of the program which loads this plugin instance.
    /// </summary>
    public static unsafe string? CurrentLoaderPath => field ??= PInvoke.GetModulePathFromAddr(PInvoke.ModulePathLoaderAddr);

    /// <summary>
    /// Gets the full directory of the program which loads this plugin instance.
    /// </summary>
    public static unsafe string? CurrentLoaderDirectory => field ??= Path.GetDirectoryName(CurrentLoaderPath);

    internal delegate void GetStringDelegate(out nint pathP, out int length);
}

public partial class SharedStaticV1Ext<T>
{
    private static unsafe void InitExtension_Update7Exports()
    {
        TryRegisterApiExport<GetStringDelegate>("GetCurrentPluginPath", GetCurrentPluginPath);
        TryRegisterApiExport<GetStringDelegate>("GetCurrentPluginDirectory", GetCurrentPluginDirectory);
        TryRegisterApiExport<GetStringDelegate>("GetCurrentLoaderPath", GetCurrentLoaderPath);
        TryRegisterApiExport<GetStringDelegate>("GetCurrentLoaderDirectory", GetCurrentLoaderDirectory);
    }

    /// <inheritdoc cref="SharedStaticV1Ext.CurrentPluginPath"/>
    private static void GetCurrentPluginPath(out nint pathP, out int length)
        => CurrentPluginPath.GetPinnableStringPointerAndLength(out pathP, out length);

    /// <inheritdoc cref="SharedStaticV1Ext.CurrentPluginDirectory"/>
    private static void GetCurrentPluginDirectory(out nint pathP, out int length)
        => CurrentPluginDirectory.GetPinnableStringPointerAndLength(out pathP, out length);

    /// <inheritdoc cref="SharedStaticV1Ext.CurrentLoaderPath"/>
    private static void GetCurrentLoaderPath(out nint pathP, out int length)
        => CurrentLoaderPath.GetPinnableStringPointerAndLength(out pathP, out length);

    /// <inheritdoc cref="SharedStaticV1Ext.CurrentLoaderDirectory"/>
    private static void GetCurrentLoaderDirectory(out nint pathP, out int length)
        => CurrentLoaderDirectory.GetPinnableStringPointerAndLength(out pathP, out length);
}