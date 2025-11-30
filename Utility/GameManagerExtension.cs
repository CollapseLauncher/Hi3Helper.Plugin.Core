using Hi3Helper.Plugin.Core.Management;
using Hi3Helper.Plugin.Core.Management.PresetConfig;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Threading;
using System.Threading.Tasks;

namespace Hi3Helper.Plugin.Core.Utility;

/// <summary>
/// This extension provides a method to launch the game using the plugin's game launch mechanism if available.
/// </summary>
/// <remarks>
/// This extension IS ONLY SUPPOSEDLY BE USED by the launcher, NOT by the plugin.
/// </remarks>
public static class GameManagerExtension
{
    public unsafe delegate void PrintGameLog(char* logString, int logStringLen, int isStringCanFree);

    /// <summary>
    /// A context used to manage the game launch routine using plugin's functionality.
    /// </summary>
    public class RunGameFromGameManagerContext
    {
        // Fields
        private bool? _isFeatureAvailable;

        /// <summary>
        /// The game manager instance which handles the game launch.
        /// </summary>
        public required IGameManager GameManager { get; init; }

        /// <summary>
        /// The instance of the plugin.
        /// </summary>
        public required IPlugin Plugin { get; init; }

        /// <summary>
        /// The preset config for the region of the game.
        /// </summary>
        public required IPluginPresetConfig PresetConfig { get; init; }

        /// <summary>
        /// The pointer to the plugin's library handle.
        /// </summary>
        public required nint PluginHandle { get; init; }

        /// <summary>
        /// A delegate which is pointed to a callback to print game log while the game is running.
        /// </summary>
        public required PrintGameLog? PrintGameLogCallback { get; set; }

        /// <summary>
        /// Indicates whether the Game Launch API is supported on the plugin.
        /// </summary>
        public bool IsFeatureAvailable => _isFeatureAvailable ??= this.IsGameRunning(out _, out _, out _);

        /// <summary>
        /// Indicates whether the game is currently running.
        /// </summary>
        // ReSharper disable once MemberHidesStaticFromOuterClass
        public bool IsGameRunning => IsFeatureAvailable && this.IsGameRunning(out bool running, out _, out _) && running;

        /// <summary>
        /// Indicates when the game launched. If the game isn't running, it will return a default value.
        /// </summary>
        public DateTime GameLaunchStartTime
        {
            get
            {
                if (!IsFeatureAvailable)
                {
                    return default;
                }

                if (!this.IsGameRunning(out bool running, out DateTime dateTime, out _) ||
                    !running)
                {
                    return default;
                }

                return dateTime;
            }
        }
    }

    /// <summary>
    /// Asynchronously launch the game using plugin's built-in game launch mechanism and wait until the game exit.
    /// </summary>
    /// <param name="context">The context to launch the game from <see cref="IGameManager"/>.</param>
    /// <param name="startArgument">The additional argument string to run the game executable (Default: null).</param>
    /// <param name="isRunBoosted">Based on <see cref="Process.PriorityBoostEnabled"/>, boost the process temporarily when the game window is focused (Default: false).</param>
    /// <param name="processPriority">Based on <see cref="Process.PriorityClass"/>, run the game process with specific priority (Default: <see cref="ProcessPriorityClass.Normal"/>).</param>
    /// <param name="token">
    /// Cancellation token to pass into the plugin's game launch mechanism.<br/>
    /// If cancellation is requested, it will cancel the awaiting but not killing the game process.
    /// </param>
    /// <returns>
    /// Returns <c>IsSuccess=false</c> if the plugin doesn't have game launch mechanism (or API Standard is equal or lower than v0.1.0), hence fallback to launcher's game launch mechanism.<br/>
    /// Otherwise, <c>IsSuccess=true</c> if the plugin does support game launch mechanism and the game ran successfully or await process is cancelled.
    /// </returns>
    public static async Task<(bool IsSuccess, Exception? Error)>
        RunGameFromGameManagerAsync(this RunGameFromGameManagerContext context,
                                    string?                            startArgument   = null,
                                    bool                               isRunBoosted    = false,
                                    ProcessPriorityClass               processPriority = ProcessPriorityClass.Normal,
                                    CancellationToken                  token           = default)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        if (!context.PluginHandle.TryGetExport("LaunchGameFromGameManagerAsync", out SharedStaticV1Ext.LaunchGameFromGameManagerAsyncDelegate launchGameFromGameManagerAsyncCallback))
        {
            return (false, new NotSupportedException("Plugin doesn't have LaunchGameFromGameManagerAsync export in its API definition!"));
        }

        nint gameManagerP          = GetPointerFromInterface(context.GameManager);
        nint pluginP               = GetPointerFromInterface(context.Plugin);
        nint presetConfigP         = GetPointerFromInterface(context.PresetConfig);
        nint printGameLogCallbackP = context.PrintGameLogCallback == null ? nint.Zero : Marshal.GetFunctionPointerForDelegate(context.PrintGameLogCallback);

        if (gameManagerP == nint.Zero)
        {
            return (false, new COMException("Cannot cast IGameManager interface to pointer!"));
        }

        if (pluginP == nint.Zero)
        {
            return (false, new COMException("Cannot cast IPlugin interface to pointer!"));
        }

        if (presetConfigP == nint.Zero)
        {
            return (false, new COMException("Cannot cast IPluginPresetConfig interface to pointer!"));
        }

        nint argumentsP   = startArgument.GetPinnableStringPointerSafe();
        int  argumentsLen = startArgument?.Length ?? 0;

        Guid cancelTokenGuid = Guid.CreateVersion7();
        int hResult = launchGameFromGameManagerAsyncCallback(gameManagerP,
                                                             pluginP, 
                                                             presetConfigP,
                                                             printGameLogCallbackP,
                                                             argumentsP,
                                                             argumentsLen,
                                                             isRunBoosted ? 1 : 0,
                                                             (int)processPriority,
                                                             ref cancelTokenGuid,
                                                             out nint taskResult);

        if (taskResult == nint.Zero)
        {
            return (false, new NullReferenceException("ComAsyncResult pointer in taskReturn argument shouldn't return a null pointer!"));
        }

        if (hResult != 0)
        {
            return (false, Marshal.GetExceptionForHR(hResult));
        }

        return await ExecuteSuccessAsyncTask(context.Plugin, taskResult, cancelTokenGuid, token);
    }

    /// <summary>
    /// Check if the game from the current <see cref="IGameManager"/> is running or not.
    /// </summary>
    /// <param name="context">The context to launch the game from <see cref="IGameManager"/>.</param>
    /// <param name="isGameRunning">Whether the game is currently running or not.</param>
    /// <param name="errorException">Represents an exception from HRESULT of the plugin's function.</param>
    /// <param name="gameStartTime">The date time stamp of when the process was started.</param>
    /// <returns>
    /// To find the actual return value, please use <paramref name="isGameRunning"/> out-argument.<br/><br/>
    /// 
    /// Returns <c>false</c> if the plugin doesn't have game launch mechanism (or API Standard is equal or lower than v0.1.0).<br/>
    /// Otherwise, <c>true</c> if the plugin supports game launch mechanism.
    /// </returns>
    public static bool IsGameRunning(this                     RunGameFromGameManagerContext context,
                                     out                      bool                          isGameRunning,
                                     out                      DateTime                      gameStartTime,
                                     [NotNullWhen(false)] out Exception?                    errorException)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        isGameRunning  = false;
        errorException = null;
        gameStartTime  = default;

        if (!context.PluginHandle.TryGetExport("IsGameRunning", out SharedStaticV1Ext.IsGameRunningDelegate isGameRunningCallback))
        {
            errorException = new NotSupportedException("Plugin doesn't have IsGameRunning export in its API definition!");
            return false;
        }

        nint gameManagerP  = GetPointerFromInterface(context.GameManager);
        nint presetConfigP = GetPointerFromInterface(context.PresetConfig);

        if (gameManagerP == nint.Zero)
        {
            errorException = new COMException("Cannot cast IGameManager interface to pointer!");
            return false;
        }

        if (presetConfigP == nint.Zero)
        {
            errorException = new COMException("Cannot cast IPluginPresetConfig interface to pointer!");
            return false;
        }

        int hResult = isGameRunningCallback(gameManagerP, presetConfigP, out int isGameRunningInt, out gameStartTime);

        errorException = Marshal.GetExceptionForHR(hResult);
        if (errorException != null)
        {
            return false;
        }

        isGameRunning = isGameRunningInt == 1;
        return true;
    }

    /// <summary>
    /// Asynchronously wait currently running game until it exit.
    /// </summary>
    /// <param name="context">The context to launch the game from <see cref="IGameManager"/>.</param>
    /// <param name="token">
    /// Cancellation token to pass into the plugin's game launch mechanism.<br/>
    /// If cancellation is requested, it will cancel the awaiting but not killing the game process.
    /// </param>
    /// <returns>
    /// Returns <c>IsSuccess=false</c> if the plugin doesn't have game launch mechanism (or API Standard is equal or lower than v0.1.0), hence fallback to launcher's game launch mechanism.<br/>
    /// Otherwise, <c>IsSuccess=true</c> if the plugin does support game launch mechanism and the game ran successfully.
    /// </returns>
    public static async Task<(bool IsSuccess, Exception? Error)>
        WaitRunningGameAsync(this RunGameFromGameManagerContext context,
                             CancellationToken                  token)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        if (!context.PluginHandle.TryGetExport("WaitRunningGameAsync", out SharedStaticV1Ext.WaitRunningGameAsyncDelegate waitRunningGameAsyncCallback))
        {
            return (false, new NotSupportedException("Plugin doesn't have WaitRunningGameAsync export in its API definition!"));
        }

        nint gameManagerP  = GetPointerFromInterface(context.GameManager);
        nint pluginP       = GetPointerFromInterface(context.Plugin);
        nint presetConfigP = GetPointerFromInterface(context.PresetConfig);

        if (gameManagerP == nint.Zero)
        {
            return (false, new COMException("Cannot cast IGameManager interface to pointer!"));
        }

        if (pluginP == nint.Zero)
        {
            return (false, new COMException("Cannot cast IPlugin interface to pointer!"));
        }

        if (presetConfigP == nint.Zero)
        {
            return (false, new COMException("Cannot cast IPluginPresetConfig interface to pointer!"));
        }

        Guid cancelTokenGuid = Guid.CreateVersion7();
        int  hResult         = waitRunningGameAsyncCallback(gameManagerP, pluginP, presetConfigP, ref cancelTokenGuid, out nint taskResult);

        if (taskResult == nint.Zero)
        {
            return (false, new NullReferenceException("ComAsyncResult pointer in taskReturn argument shouldn't return a null pointer!"));
        }

        if (hResult != 0)
        {
            return (false, Marshal.GetExceptionForHR(hResult));
        }

        return await ExecuteSuccessAsyncTask(context.Plugin, taskResult, cancelTokenGuid, token);
    }

    /// <summary>
    /// Kill the process of the currently running game.
    /// </summary>
    /// <param name="context">The context to launch the game from <see cref="IGameManager"/>.</param>
    /// <param name="wasGameRunning">Whether to indicate that the game was running or not.</param>
    /// <param name="gameStartTime">The date time stamp of when the process was started.</param>
    /// <param name="errorException">Represents an exception from HRESULT of the plugin's function.</param>
    /// <returns>
    /// To find the actual return value, please use <paramref name="wasGameRunning"/> out-argument.<br/><br/>
    /// 
    /// Returns <c>false</c> if the plugin doesn't have game launch mechanism (or API Standard is equal or lower than v0.1.0).<br/>
    /// Otherwise, <c>true</c> if the plugin supports game launch mechanism.
    /// </returns>
    public static bool KillRunningGame(this                     RunGameFromGameManagerContext context,
                                       out                      bool                          wasGameRunning,
                                       out                      DateTime                      gameStartTime,
                                       [NotNullWhen(false)] out Exception?                    errorException)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        errorException = null;
        wasGameRunning = false;
        gameStartTime  = default;

        if (!context.PluginHandle.TryGetExport("KillRunningGame", out SharedStaticV1Ext.IsGameRunningDelegate killRunningGameCallback))
        {
            errorException = new NotSupportedException("Plugin doesn't have KillRunningGame export in its API definition!");
            return false;
        }

        nint gameManagerP  = GetPointerFromInterface(context.GameManager);
        nint presetConfigP = GetPointerFromInterface(context.PresetConfig);

        if (gameManagerP == nint.Zero)
        {
            errorException = new COMException("Cannot cast IGameManager interface to pointer!");
            return false;
        }

        if (presetConfigP == nint.Zero)
        {
            errorException = new COMException("Cannot cast IPluginPresetConfig interface to pointer!");
            return false;
        }

        int hResult = killRunningGameCallback(gameManagerP, presetConfigP, out int wasGameRunningInt, out gameStartTime);

        errorException = Marshal.GetExceptionForHR(hResult);
        if (errorException != null)
        {
            return false;
        }

        wasGameRunning = wasGameRunningInt == 1;
        return true;
    }

    /// <summary>
    /// Asynchronously hook to the game process making the window resizable and wait until the game exit.
    /// </summary>
    /// <param name="context">The context to launch the game from <see cref="IGameManager"/>.</param>
    /// <param name="executableName">The name of the game executable.</param>
    /// <param name="height">Height of the host screen.</param>
    /// <param name="width">Width of the host screen.</param>
    /// <param name="executableDirectory">The path to the directory where the game executable is located.</param>
    /// <param name="token">
    /// Cancellation token to pass into the plugin's game launch mechanism.<br/>
    /// If cancellation is requested, it will cancel the awaiting but not killing the game process.
    /// </param>
    /// <returns>
    /// Returns <c>IsSupported.false</c> if the plugin's API Standard is equal or lower than v0.1.3 or if this method isn't overriden.<br/>
    /// Otherwise, <c>IsSupported.true</c> if the plugin supports game launch mechanism and this method.
    /// </returns>
    public static async Task<(bool IsSuccess, Exception? Error)>
        StartResizableWindowHookAsync(this RunGameFromGameManagerContext context,
                                      string? executableName = null,
                                      int? height = null,
                                      int? width = null,
                                      string? executableDirectory = null,
                                      CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        if (!context.PluginHandle.TryGetExport("StartResizableWindowHookAsync", out SharedStaticV1Ext.StartResizableWindowHookAsyncDelegate startResizableWindowHookAsyncCallback))
        {
            return (false, new NotSupportedException("Plugin doesn't have StartResizableWindowHookAsync export in its API definition!"));
        }

        nint gameManagerP = GetPointerFromInterface(context.GameManager);
        nint presetConfigP = GetPointerFromInterface(context.PresetConfig);

        if (gameManagerP == nint.Zero)
        {
            return (false, new COMException("Cannot cast IGameManager interface to pointer!"));
        }

        if (presetConfigP == nint.Zero)
        {
            return (false, new COMException("Cannot cast IPluginPresetConfig interface to pointer!"));
        }

        nint exeNameP = executableName.GetPinnableStringPointerSafe();
        int exeNameLen = executableName?.Length ?? 0;

        nint exeDirP = executableDirectory.GetPinnableStringPointerSafe();
        int exeDirLen = executableDirectory?.Length ?? 0;

        Guid cancelTokenGuid = Guid.CreateVersion7();
        int hResult = startResizableWindowHookAsyncCallback(gameManagerP,
                                                            presetConfigP,
                                                            exeNameP,
                                                            exeNameLen,
                                                            height ?? int.MinValue,
                                                            width ?? int.MinValue,
                                                            exeDirP,
                                                            exeDirLen,
                                                            ref cancelTokenGuid,
                                                            out nint taskResult);

        if (taskResult == nint.Zero)
        {
            return (false, new NullReferenceException("ComAsyncResult pointer in taskReturn argument shouldn't return a null pointer!"));
        }

        if (hResult != 0)
        {
            return (false, Marshal.GetExceptionForHR(hResult));
        }

        return await ExecuteSuccessAsyncTask(context.Plugin, taskResult, cancelTokenGuid, token);
    }

    private static unsafe nint GetPointerFromInterface<T>(this T interfaceSource)
        where T : class
        => (nint)ComInterfaceMarshaller<T>.ConvertToUnmanaged(interfaceSource);

    private static async Task<(bool IsSuccess, Exception? Error)>
        ExecuteSuccessAsyncTask(IPlugin           pluginInstance,
                                nint              taskResult,
                                Guid              cancelTokenGuid,
                                CancellationToken token)
    {

        try
        {
            token.Register(() => pluginInstance.CancelAsync(in cancelTokenGuid));
            bool isSuccess = await taskResult.AsTask<bool>();
            if (isSuccess)
            {
                return (true, null);
            }

            throw new Exception("Failed to await ComAsyncResult as Task<bool>!");
        }
        catch (OperationCanceledException ex)
        {
            return (true, ex);
        }
        catch (Exception ex)
        {
            return (false, ex);
        }
    }
}