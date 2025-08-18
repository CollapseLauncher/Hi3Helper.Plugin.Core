using Hi3Helper.Plugin.Core.Management;
using System;
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
    public unsafe delegate void PrintGameLog(char* logString, int logStringLen, bool isStringCanFree);

    public class RunGameFromGameManagerContext
    {
        public required IGameManager GameManager { get; init; }
        public required IPlugin Plugin { get; init; }
        public required nint PluginHandle { get; init; }
        public required PrintGameLog PrintGameLogCallback { get; init; }
    }

    /// <summary>
    /// Asynchronously launch the game using plugin's built-in game launch mechanism and wait until the game exit.
    /// </summary>
    /// <param name="context">The context to launch the game from <see cref="IGameManager"/>.</param>
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
                                    CancellationToken                  token)
    {
        ArgumentNullException.ThrowIfNull(context, nameof(context));
        if (!context.PluginHandle.TryGetExport("LaunchGameFromGameManagerAsync", out SharedStatic.LaunchGameFromGameManagerAsyncDelegate launchGameFromGameManagerAsyncCallback))
        {
            return (false, new NotSupportedException("Plugin doesn't have LaunchGameFromGameManagerAsync export in its API definition!"));
        }

        nint gameManagerP          = GetPointerFromInterface(context.GameManager);
        nint pluginP               = GetPointerFromInterface(context.Plugin);
        nint printGameLogCallbackP = Marshal.GetFunctionPointerForDelegate(context.PrintGameLogCallback);

        if (gameManagerP == nint.Zero)
        {
            return (false, new COMException("Cannot cast IGameManager interface to pointer!"));
        }

        if (pluginP == nint.Zero)
        {
            return (false, new COMException("Cannot cast IPlugin interface to pointer!"));
        }

        if (printGameLogCallbackP == nint.Zero)
        {
            return (false, new COMException("Cannot cast PrintGameLog delegate/callback to pointer!"));
        }

        Guid cancelTokenGuid = Guid.CreateVersion7();
        int hResult = launchGameFromGameManagerAsyncCallback(gameManagerP, pluginP, printGameLogCallbackP, ref cancelTokenGuid, out nint taskResult);

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
    /// <param name="manager">The game manager instance which handles the game launch.</param>
    /// <param name="pluginHandle">The pointer to the plugin's library handle.</param>
    /// <param name="isGameRunning">Whether the game is currently running or not.</param>
    /// <param name="errorException">Represents an exception from HRESULT of the plugin's function.</param>
    /// <returns>
    /// To find the actual return value, please use <paramref name="isGameRunning"/> out-argument.<br/><br/>
    /// 
    /// Returns <c>false</c> if the plugin doesn't have game launch mechanism (or API Standard is equal or lower than v0.1.0).<br/>
    /// Otherwise, <c>true</c> if the plugin supports game launch mechanism.
    /// </returns>
    public static bool IsGameRunning(this IGameManager                   manager,
                                     nint                                pluginHandle,
                                     out                      bool       isGameRunning,
                                     [NotNullWhen(false)] out Exception? errorException)
    {
        ArgumentNullException.ThrowIfNull(manager, nameof(manager));
        isGameRunning  = false;
        errorException = null;

        if (!pluginHandle.TryGetExport("IsGameRunning", out SharedStatic.IsGameRunningDelegate isGameRunningCallback))
        {
            errorException = new NotSupportedException("Plugin doesn't have IsGameRunning export in its API definition!");
            return false;
        }

        nint gameManagerP = GetPointerFromInterface(manager);
        if (gameManagerP == nint.Zero)
        {
            errorException = new COMException("Cannot cast IGameManager interface to pointer!");
            return false;
        }

        int hResult = isGameRunningCallback(gameManagerP, out int isGameRunningInt);

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
    /// <param name="manager">The game manager instance which handles the game launch.</param>
    /// <param name="pluginInstance">The instance of the plugin.</param>
    /// <param name="pluginHandle">The pointer to the plugin's library handle.</param>
    /// <param name="token">
    /// Cancellation token to pass into the plugin's game launch mechanism.<br/>
    /// If cancellation is requested, it will cancel the awaiting but not killing the game process.
    /// </param>
    /// <returns>
    /// Returns <c>IsSuccess=false</c> if the plugin doesn't have game launch mechanism (or API Standard is equal or lower than v0.1.0), hence fallback to launcher's game launch mechanism.<br/>
    /// Otherwise, <c>IsSuccess=true</c> if the plugin does support game launch mechanism and the game ran successfully.
    /// </returns>
    public static async Task<(bool IsSuccess, Exception? Error)>
        WaitRunningGameAsync(this IGameManager manager,
                             IPlugin           pluginInstance,
                             nint              pluginHandle,
                             CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(manager, nameof(manager));
        if (!pluginHandle.TryGetExport("WaitRunningGameAsync", out SharedStatic.WaitRunningGameAsyncDelegate waitRunningGameAsyncCallback))
        {
            return (false, new NotSupportedException("Plugin doesn't have WaitRunningGameAsync export in its API definition!"));
        }

        nint gameManagerP = GetPointerFromInterface(manager);
        nint pluginP      = GetPointerFromInterface(pluginInstance);

        if (gameManagerP == nint.Zero)
        {
            return (false, new COMException("Cannot cast IGameManager interface to pointer!"));
        }

        if (pluginP == nint.Zero)
        {
            return (false, new COMException("Cannot cast IPlugin interface to pointer!"));
        }

        Guid cancelTokenGuid = Guid.CreateVersion7();
        int  hResult         = waitRunningGameAsyncCallback(gameManagerP, pluginP, ref cancelTokenGuid, out nint taskResult);

        if (taskResult == nint.Zero)
        {
            return (false, new NullReferenceException("ComAsyncResult pointer in taskReturn argument shouldn't return a null pointer!"));
        }

        if (hResult != 0)
        {
            return (false, Marshal.GetExceptionForHR(hResult));
        }

        return await ExecuteSuccessAsyncTask(pluginInstance, taskResult, cancelTokenGuid, token);
    }

    /// <summary>
    /// Kill the process of the currently running game.
    /// </summary>
    /// <param name="manager">The game manager instance which handles the game launch.</param>
    /// <param name="pluginHandle">The pointer to the plugin's library handle.</param>
    /// <param name="wasGameRunning">Whether to indicate that the game was running or not.</param>
    /// <param name="errorException">Represents an exception from HRESULT of the plugin's function.</param>
    /// <returns>
    /// To find the actual return value, please use <paramref name="wasGameRunning"/> out-argument.<br/><br/>
    /// 
    /// Returns <c>false</c> if the plugin doesn't have game launch mechanism (or API Standard is equal or lower than v0.1.0).<br/>
    /// Otherwise, <c>true</c> if the plugin supports game launch mechanism.
    /// </returns>
    public static bool KillRunningGame(this IGameManager                   manager,
                                       nint                                pluginHandle,
                                       out                      bool       wasGameRunning,
                                       [NotNullWhen(false)] out Exception? errorException)
    {
        ArgumentNullException.ThrowIfNull(manager, nameof(manager));
        errorException = null;
        wasGameRunning = false;

        if (!pluginHandle.TryGetExport("KillRunningGame", out SharedStatic.IsGameRunningDelegate killRunningGameCallback))
        {
            errorException = new NotSupportedException("Plugin doesn't have KillRunningGame export in its API definition!");
            return false;
        }

        nint gameManagerP = GetPointerFromInterface(manager);

        if (gameManagerP == nint.Zero)
        {
            errorException = new COMException("Cannot cast IGameManager interface to pointer!");
            return false;
        }

        int hResult = killRunningGameCallback(gameManagerP, out int wasGameRunningInt);

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