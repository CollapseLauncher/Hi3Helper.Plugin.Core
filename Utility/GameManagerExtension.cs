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
        private bool? _canUseGameLaunchApi;

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
        public required PrintGameLog PrintGameLogCallback { get; init; }

        /// <summary>
        /// Indicates whether the Game Launch API is supported on the plugin.
        /// </summary>
        public bool CanUseGameLaunchApi => _canUseGameLaunchApi ??= this.IsGameRunning(out _, out _);

        /// <summary>
        /// Indicates whether the game is currently running.
        /// </summary>
        public bool IsGameRunning => CanUseGameLaunchApi && this.IsGameRunning(out bool running, out _) && running;
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
        if (!context.PluginHandle.TryGetExport("LaunchGameFromGameManagerAsync", out SharedStatic.LaunchGameFromGameManagerAsyncDelegate launchGameFromGameManagerAsyncCallback))
        {
            return (false, new NotSupportedException("Plugin doesn't have LaunchGameFromGameManagerAsync export in its API definition!"));
        }

        nint gameManagerP          = GetPointerFromInterface(context.GameManager);
        nint pluginP               = GetPointerFromInterface(context.Plugin);
        nint presetConfigP         = GetPointerFromInterface(context.PresetConfig);
        nint printGameLogCallbackP = Marshal.GetFunctionPointerForDelegate(context.PrintGameLogCallback);

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

        if (printGameLogCallbackP == nint.Zero)
        {
            return (false, new COMException("Cannot cast PrintGameLog delegate/callback to pointer!"));
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
    /// <returns>
    /// To find the actual return value, please use <paramref name="isGameRunning"/> out-argument.<br/><br/>
    /// 
    /// Returns <c>false</c> if the plugin doesn't have game launch mechanism (or API Standard is equal or lower than v0.1.0).<br/>
    /// Otherwise, <c>true</c> if the plugin supports game launch mechanism.
    /// </returns>
    public static bool IsGameRunning(this                     RunGameFromGameManagerContext context,
                                     out                      bool                          isGameRunning,
                                     [NotNullWhen(false)] out Exception?                    errorException)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        isGameRunning  = false;
        errorException = null;

        if (!context.PluginHandle.TryGetExport("IsGameRunning", out SharedStatic.IsGameRunningDelegate isGameRunningCallback))
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

        int hResult = isGameRunningCallback(gameManagerP, presetConfigP, out int isGameRunningInt);

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
        if (!context.PluginHandle.TryGetExport("WaitRunningGameAsync", out SharedStatic.WaitRunningGameAsyncDelegate waitRunningGameAsyncCallback))
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
    /// <param name="errorException">Represents an exception from HRESULT of the plugin's function.</param>
    /// <returns>
    /// To find the actual return value, please use <paramref name="wasGameRunning"/> out-argument.<br/><br/>
    /// 
    /// Returns <c>false</c> if the plugin doesn't have game launch mechanism (or API Standard is equal or lower than v0.1.0).<br/>
    /// Otherwise, <c>true</c> if the plugin supports game launch mechanism.
    /// </returns>
    public static bool KillRunningGame(this                     RunGameFromGameManagerContext context,
                                       out                      bool                          wasGameRunning,
                                       [NotNullWhen(false)] out Exception?                    errorException)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        errorException = null;
        wasGameRunning = false;

        if (!context.PluginHandle.TryGetExport("KillRunningGame", out SharedStatic.IsGameRunningDelegate killRunningGameCallback))
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

        int hResult = killRunningGameCallback(gameManagerP, presetConfigP, out int wasGameRunningInt);

        errorException = Marshal.GetExceptionForHR(hResult);
        if (errorException != null)
        {
            return false;
        }

        wasGameRunning = wasGameRunningInt == 1;
        return true;
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