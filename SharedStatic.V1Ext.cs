using Hi3Helper.Plugin.Core.DiscordPresence;
using Hi3Helper.Plugin.Core.Management;
using Hi3Helper.Plugin.Core.Utility;
using System;

namespace Hi3Helper.Plugin.Core;

public class SharedStaticV1Ext : SharedStatic
{
    // Update1
    internal delegate HResult LaunchGameFromGameManagerAsyncDelegate(nint gameManagerP, nint pluginP, nint presetConfigP, nint printGameLogCallbackP, nint arguments, int argumentsLen, int runBoostedInt, int processPriorityInt, ref Guid cancelToken, out nint taskResult);
    internal delegate HResult WaitRunningGameAsyncDelegate(nint gameManagerP, nint pluginP, nint presetConfigP, ref Guid cancelToken, out nint taskResult);
    internal delegate HResult IsGameRunningDelegate(nint gameManagerP, nint presetConfigP, out int isGameRunning, out DateTime processStartTime);

    // Update2
    internal unsafe delegate HResult GetCurrentDiscordPresenceInfoDelegate(void*                 presetConfigP,
                                                                           DiscordPresenceInfo** presenceInfoP);

    // Update3
    internal delegate HResult StartResizableWindowHookAsyncDelegate(nint gameManagerP, nint presetConfigP, nint executableName, int executableNameLen, int height, int width, nint executableDirectory, int executableDirectoryLen, ref Guid cancelToken, out nint taskResult);
}

/// <summary>
/// Inherited <see cref="SharedStatic"/> with additional supports for API extensions which require call or property access to derived exports.
/// </summary>
public partial class SharedStaticV1Ext<T> : SharedStaticV1Ext
    where T : SharedStaticV1Ext<T>, new()
{
    private static T ThisExtensionExport { get; }

    static SharedStaticV1Ext()
    {
        ThisExtensionExport = new T();

        /* ----------------------------------------------------------------------
         * Plugin extension exports
         * ----------------------------------------------------------------------
         * These exports are optional and can be removed if it's not necessarily
         * used. These optional exports are included under additional
         * functionalities used as a subset of v0.1 API standard.
         */
        InitExtension_Update1Exports();
        InitExtension_Update2Exports();
        InitExtension_Update3Exports();
    }

    /// <summary>
    /// Specify which <see cref="IPlugin"/> instance to load and use in this plugin.
    /// </summary>
    /// <typeparam name="TPlugin">A member of COM Interface of <see cref="IPlugin"/>.</typeparam>
    protected new static void Load<TPlugin>(GameVersion interceptDllVersionTo = default)
        where TPlugin : class, IPlugin, new()
        => SharedStatic.Load<TPlugin>(interceptDllVersionTo);
}
