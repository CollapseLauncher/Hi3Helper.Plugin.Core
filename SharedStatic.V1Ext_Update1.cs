using Hi3Helper.Plugin.Core.Management;
using Hi3Helper.Plugin.Core.Management.PresetConfig;
using Hi3Helper.Plugin.Core.Utility;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using static Hi3Helper.Plugin.Core.Utility.GameManagerExtension;

#if !MANUALCOM
using System.Runtime.InteropServices.Marshalling;
#endif

namespace Hi3Helper.Plugin.Core;

public partial class SharedStaticV1Ext<T>
{
    private static void InitExtension_Update1Exports()
    {
        /* ----------------------------------------------------------------------
         * Update 1 Feature Sets
         * ----------------------------------------------------------------------
         * This feature sets includes the following feature:
         *  - Game Launch
         *    - LaunchGameFromGameManagerAsync
         *    - WaitRunningGameAsync
         *  - Game Process Management
         *    - IsGameRunning
         *    - KillRunningGame
         */

        // -> Plugin Async Game Launch Callback for Specific Game Region based on its IGameManager instance.
        TryRegisterApiExport<LaunchGameFromGameManagerAsyncDelegate>("LaunchGameFromGameManagerAsync", LaunchGameFromGameManagerAsync);
        // -> Plugin Game Run Check for Specific Game Region based on its IGameManager instance.
        TryRegisterApiExport<IsGameRunningDelegate>("IsGameRunning", IsGameRunning);
        // -> Plugin Async Game Run Awaiter for Specific Game Region based on its IGameManager instance.
        TryRegisterApiExport<WaitRunningGameAsyncDelegate>("WaitRunningGameAsync", WaitRunningGameAsync);
        // -> Plugin Game Process Killer for Specific Game Region based on its IGameManager instance.
        TryRegisterApiExport<IsGameRunningDelegate>("KillRunningGame", KillRunningGame);
    }

    #region ABI Proxies
    /// <summary>
    /// This method is an ABI proxy function between the PInvoke Export and the actual plugin's method.<br/>
    /// See the documentation for <see cref="SharedStaticV1Ext{T}.LaunchGameFromGameManagerCoreAsync(RunGameFromGameManagerContext, string?, bool, ProcessPriorityClass, CancellationToken)"/> method for more information.
    /// </summary>
    private static unsafe HResult LaunchGameFromGameManagerAsync(nint     gameManagerP,
                                                                 nint     pluginP,
                                                                 nint     presetConfigP,
                                                                 nint     printGameLogCallbackP,
                                                                 nint     arguments,
                                                                 int      argumentsLen,
                                                                 int      runBoostedInt,
                                                                 int      processPriorityInt,
                                                                 ref Guid cancelToken,
                                                                 out nint taskResult)
    {
        taskResult = nint.Zero;
        try
        {
#if MANUALCOM
            IPlugin? plugin = ComWrappers.ComInterfaceDispatch.GetInstance<IPlugin>((ComWrappers.ComInterfaceDispatch*)pluginP);
            IGameManager? gameManager = ComWrappers.ComInterfaceDispatch.GetInstance<IGameManager>((ComWrappers.ComInterfaceDispatch*)gameManagerP);
            IPluginPresetConfig? presetConfig = ComWrappers.ComInterfaceDispatch.GetInstance<IPluginPresetConfig>((ComWrappers.ComInterfaceDispatch*)presetConfigP);
#else
            IPlugin? plugin = ComInterfaceMarshaller<IPlugin>.ConvertToManaged((void*)pluginP);
            IGameManager? gameManager = ComInterfaceMarshaller<IGameManager>.ConvertToManaged((void*)gameManagerP);
            IPluginPresetConfig? presetConfig = ComInterfaceMarshaller<IPluginPresetConfig>.ConvertToManaged((void*)presetConfigP);
#endif

            PrintGameLog printGameLogCallback = Marshal.GetDelegateForFunctionPointer<PrintGameLog>(printGameLogCallbackP);

            if (ThisExtensionExport == null)
            {
                throw new NullReferenceException("The ThisPluginExport field is null!");
            }

            if (gameManager == null)
            {
                throw new NullReferenceException("Cannot cast IGameManager from the pointer, hence it gives null!");
            }

            if (plugin == null)
            {
                throw new NullReferenceException("Cannot cast IPlugin from the pointer, hence it gives null!");
            }

            if (presetConfig == null)
            {
                throw new NullReferenceException("Cannot cast IPluginPresetConfig from the pointer, hence it gives null!");
            }

            if (printGameLogCallback == null)
            {
                throw new NullReferenceException("Cannot cast PrintGameLog callback from the pointer, hence it gives null!");
            }

            CancellationTokenSource? cts = null;
            if (Unsafe.IsNullRef(ref cancelToken))
            {
                cts = ComCancellationTokenVault.RegisterToken(in cancelToken);
            }

            RunGameFromGameManagerContext context = new()
            {
                GameManager          = gameManager,
                PresetConfig         = presetConfig,
                Plugin               = plugin,
                PrintGameLogCallback = printGameLogCallback,
                PluginHandle         = nint.Zero
            };

            bool isRunBoosted = runBoostedInt == 1;
            ProcessPriorityClass processPriorityClass = (ProcessPriorityClass)processPriorityInt;

            string? startArguments = null;
            if (argumentsLen > 0)
            {
                char* argumentsP = (char*)arguments;
                ReadOnlySpan<char> argumentsSpan = Mem.CreateSpanFromNullTerminated<char>(argumentsP);
                if (argumentsSpan.Length > argumentsLen)
                {
                    argumentsSpan = argumentsSpan[..argumentsLen];
                }

                startArguments = argumentsSpan.IsEmpty ? null : argumentsSpan.ToString();
            }

            (bool isSupported, Task<bool> task) = ThisExtensionExport
                .LaunchGameFromGameManagerCoreAsync(context,
                                                    startArguments,
                                                    isRunBoosted,
                                                    processPriorityClass,
                                                    cts?.Token ?? CancellationToken.None);

            taskResult = task.AsResult();
            return isSupported;
        }
        catch (Exception ex)
        {
            // ignored
            InstanceLogger.LogError(ex, "An error has occurred while trying to call LaunchGameFromGameManagerCoreAsync() from the plugin!");
            return Marshal.GetHRForException(ex);
        }
    }

    /// <summary>
    /// This method is an ABI proxy function between the PInvoke Export and the actual plugin's method.<br/>
    /// See the documentation for <see cref="SharedStaticV1Ext{T}.IsGameRunningCore(RunGameFromGameManagerContext, out bool, out DateTime)"/> method for more information.
    /// </summary>
    private static unsafe HResult IsGameRunning(nint gameManagerP, nint presetConfigP, out int isGameRunningInt, out DateTime gameStartTime)
    {
        isGameRunningInt = 0;
        gameStartTime = default;

        try
        {
#if MANUALCOM
            IGameManager? gameManager = ComWrappers.ComInterfaceDispatch.GetInstance<IGameManager>((ComWrappers.ComInterfaceDispatch*)gameManagerP);
            IPluginPresetConfig? presetConfig = ComWrappers.ComInterfaceDispatch.GetInstance<IPluginPresetConfig>((ComWrappers.ComInterfaceDispatch*)presetConfigP);
#else
            IGameManager? gameManager = ComInterfaceMarshaller<IGameManager>.ConvertToManaged((void*)gameManagerP);
            IPluginPresetConfig? presetConfig = ComInterfaceMarshaller<IPluginPresetConfig>.ConvertToManaged((void*)presetConfigP);
#endif

            if (gameManager == null)
            {
                throw new NullReferenceException("Cannot cast IGameManager from the pointer, hence it gives null!");
            }

            if (presetConfig == null)
            {
                throw new NullReferenceException("Cannot cast IPluginPresetConfig from the pointer, hence it gives null!");
            }

            RunGameFromGameManagerContext context = new()
            {
                GameManager          = gameManager,
                PresetConfig         = presetConfig,
                Plugin               = null!,
                PrintGameLogCallback = null!,
                PluginHandle         = nint.Zero
            };

            bool isSupported = ThisExtensionExport.IsGameRunningCore(context, out bool isGameRunning, out gameStartTime);
            isGameRunningInt = isGameRunning ? 1 : 0;

            return isSupported;
        }
        catch (Exception ex)
        {
            // ignored
            InstanceLogger.LogError(ex, "An error has occurred while trying to call IsGameRunningCore() from the plugin!");
            return Marshal.GetHRForException(ex);
        }
    }

    /// <summary>
    /// This method is an ABI proxy function between the PInvoke Export and the actual plugin's method.<br/>
    /// See the documentation for <see cref="SharedStaticV1Ext{T}.WaitRunningGameCoreAsync(RunGameFromGameManagerContext, CancellationToken)"/> method for more information.
    /// </summary>
    private static unsafe HResult WaitRunningGameAsync(nint gameManagerP, nint pluginP, nint presetConfigP, ref Guid cancelToken, out nint taskResult)
    {
        taskResult = nint.Zero;
        try
        {
#if MANUALCOM
            IPlugin? plugin = ComWrappers.ComInterfaceDispatch.GetInstance<IPlugin>((ComWrappers.ComInterfaceDispatch*)pluginP);
            IGameManager? gameManager = ComWrappers.ComInterfaceDispatch.GetInstance<IGameManager>((ComWrappers.ComInterfaceDispatch*)gameManagerP);
            IPluginPresetConfig? presetConfig = ComWrappers.ComInterfaceDispatch.GetInstance<IPluginPresetConfig>((ComWrappers.ComInterfaceDispatch*)presetConfigP);
#else
            IPlugin? plugin = ComInterfaceMarshaller<IPlugin>.ConvertToManaged((void*)pluginP);
            IGameManager? gameManager = ComInterfaceMarshaller<IGameManager>.ConvertToManaged((void*)gameManagerP);
            IPluginPresetConfig? presetConfig = ComInterfaceMarshaller<IPluginPresetConfig>.ConvertToManaged((void*)presetConfigP);
#endif

            if (ThisExtensionExport == null)
            {
                throw new NullReferenceException("The ThisPluginExport field is null!");
            }

            if (gameManager == null)
            {
                throw new NullReferenceException("Cannot cast IGameManager from the pointer, hence it gives null!");
            }

            if (plugin == null)
            {
                throw new NullReferenceException("Cannot cast IPlugin from the pointer, hence it gives null!");
            }

            if (presetConfig == null)
            {
                throw new NullReferenceException("Cannot cast IPluginPresetConfig from the pointer, hence it gives null!");
            }

            CancellationTokenSource? cts = null;
            if (Unsafe.IsNullRef(ref cancelToken))
            {
                cts = ComCancellationTokenVault.RegisterToken(in cancelToken);
            }

            RunGameFromGameManagerContext context = new()
            {
                GameManager          = gameManager,
                PresetConfig         = presetConfig,
                Plugin               = plugin,
                PrintGameLogCallback = null!,
                PluginHandle         = nint.Zero
            };

            (bool isSupported, Task<bool> task) = ThisExtensionExport.WaitRunningGameCoreAsync(context, cts?.Token ?? CancellationToken.None);
            taskResult = task.AsResult();

            return isSupported;
        }
        catch (Exception ex)
        {
            // ignored
            InstanceLogger.LogError(ex, "An error has occurred while trying to call WaitRunningGameCoreAsync() from the plugin!");
            return Marshal.GetHRForException(ex);
        }
    }

    /// <summary>
    /// This method is an ABI proxy function between the PInvoke Export and the actual plugin's method.<br/>
    /// See the documentation for <see cref="SharedStaticV1Ext{T}.KillRunningGameCore(RunGameFromGameManagerContext, out bool, out DateTime)"/> method for more information.
    /// </summary>
    private static unsafe HResult KillRunningGame(nint gameManagerP, nint presetConfigP, out int wasGameRunningInt, out DateTime gameStartTime)
    {
        wasGameRunningInt = 0;
        gameStartTime = default;

        try
        {
#if MANUALCOM
            IGameManager? gameManager = ComWrappers.ComInterfaceDispatch.GetInstance<IGameManager>((ComWrappers.ComInterfaceDispatch*)gameManagerP);
            IPluginPresetConfig? presetConfig = ComWrappers.ComInterfaceDispatch.GetInstance<IPluginPresetConfig>((ComWrappers.ComInterfaceDispatch*)presetConfigP);
#else
            IGameManager? gameManager = ComInterfaceMarshaller<IGameManager>.ConvertToManaged((void*)gameManagerP);
            IPluginPresetConfig? presetConfig = ComInterfaceMarshaller<IPluginPresetConfig>.ConvertToManaged((void*)presetConfigP);
#endif

            if (gameManager == null)
            {
                throw new NullReferenceException("Cannot cast IGameManager from the pointer, hence it gives null!");
            }

            if (presetConfig == null)
            {
                throw new NullReferenceException("Cannot cast IPluginPresetConfig from the pointer, hence it gives null!");
            }

            RunGameFromGameManagerContext context = new()
            {
                GameManager          = gameManager,
                PresetConfig         = presetConfig,
                Plugin               = null!,
                PrintGameLogCallback = null!,
                PluginHandle         = nint.Zero
            };

            bool isSupported = ThisExtensionExport.KillRunningGameCore(context, out bool wasGameRunning, out gameStartTime);
            wasGameRunningInt = wasGameRunning ? 1 : 0;
            return isSupported;
        }
        catch (Exception ex)
        {
            // ignored
            InstanceLogger.LogError(ex, "An error has occurred while trying to call KillRunningGameCore() from the plugin!");
            return Marshal.GetHRForException(ex);
        }
    }
    #endregion

    #region Core Methods
    /// <summary>
    /// Asynchronously launch the game using plugin's built-in game launch mechanism and wait until the game exit.
    /// </summary>
    /// <param name="context">The context to launch the game from <see cref="IGameManager"/>.</param>
    /// <param name="startArgument">The additional argument to run the game executable.</param>
    /// <param name="isRunBoosted">Based on <see cref="Process.PriorityBoostEnabled"/>, boost the process temporarily when the game window is focused (Default: false).</param>
    /// <param name="processPriority">Based on <see cref="Process.PriorityClass"/>, run the game process with specific priority (Default: <see cref="ProcessPriorityClass.Normal"/>).</param>
    /// <param name="token">
    /// Cancellation token to pass into the plugin's game launch mechanism.<br/>
    /// If cancellation is requested, it will cancel the awaiting but not killing the game process.
    /// </param>
    /// <returns>
    /// Returns <c>IsSupported.false</c> if the plugin doesn't have game launch mechanism (or API Standard is equal or lower than v0.1.1) or if this method isn't overriden.<br/>
    /// Otherwise, <c>IsSupported.true</c> if the plugin supports game launch mechanism.
    /// </returns>
    protected virtual (bool IsSupported, Task<bool> Task) LaunchGameFromGameManagerCoreAsync(
        RunGameFromGameManagerContext context,
        string?                       startArgument,
        bool                          isRunBoosted,
        ProcessPriorityClass          processPriority,
        CancellationToken             token)
    {
        return (false, Task.FromResult(false));
    }

    /// <summary>
    /// Check if the game from the current <see cref="IGameManager"/> is running or not.
    /// </summary>
    /// <param name="context">The context to launch the game from <see cref="IGameManager"/>.</param>
    /// <param name="isGameRunning">Whether the game is currently running or not.</param>
    /// <param name="gameStartTime">The date time stamp of when the process was started.</param>
    /// <returns>
    /// To find the actual return value, please use <paramref name="isGameRunning"/> out-argument.<br/><br/>
    /// 
    /// Returns <c>false</c> if the plugin doesn't have game launch mechanism (or API Standard is equal or lower than v0.1.1) or if this method isn't overriden.<br/>
    /// Otherwise, <c>true</c> if the plugin supports game launch mechanism.
    /// </returns>
    protected virtual bool IsGameRunningCore(
        RunGameFromGameManagerContext context,
        out bool                      isGameRunning,
        out DateTime                  gameStartTime)
    {
        isGameRunning = false;
        gameStartTime = default;
        return false;
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
    /// Returns <c>IsSupported.false</c> if the plugin doesn't have game launch mechanism (or API Standard is equal or lower than v0.1.1) or if this method isn't overriden.<br/>
    /// Otherwise, <c>IsSupported.true</c> if the plugin does support game launch mechanism and the game ran successfully.
    /// </returns>
    protected virtual (bool IsSupported, Task<bool> Task) WaitRunningGameCoreAsync(
        RunGameFromGameManagerContext context,
        CancellationToken             token)
    {
        return (false, Task.FromResult(false));
    }

    /// <summary>
    /// Kill the process of the currently running game.
    /// </summary>
    /// <param name="context">The context to launch the game from <see cref="IGameManager"/>.</param>
    /// <param name="wasGameRunning">Whether to indicate that the game was running or not.</param>
    /// <param name="gameStartTime">The date time stamp of when the process was started.</param>
    /// <returns>
    /// To find the actual return value, please use <paramref name="wasGameRunning"/> out-argument.<br/><br/>
    /// 
    /// Returns <c>false</c> if the plugin doesn't have game launch mechanism (or API Standard is equal or lower than v0.1.1) or if this method isn't overriden.<br/>
    /// Otherwise, <c>true</c> if the plugin supports game launch mechanism.
    /// </returns>
    protected virtual bool KillRunningGameCore(
        RunGameFromGameManagerContext context,
        out bool                      wasGameRunning,
        out DateTime                  gameStartTime)
    {
        wasGameRunning = false;
        gameStartTime = default;
        return false;
    }
    #endregion
}
