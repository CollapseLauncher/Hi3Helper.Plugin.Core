using Hi3Helper.Plugin.Core.Management;
using Hi3Helper.Plugin.Core.Management.PresetConfig;
using Hi3Helper.Plugin.Core.Utility;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

#if !MANUALCOM
using System.Runtime.InteropServices.Marshalling;
#endif

using static Hi3Helper.Plugin.Core.Utility.GameManagerExtension;

namespace Hi3Helper.Plugin.Core;

/// <summary>
/// Inherited <see cref="SharedStatic"/> with additional supports for APIs which require call or property access to derived exports.
/// </summary>
public class SharedStatic<T> : SharedStatic where T : SharedStatic, new()
{
    static SharedStatic()
    {
        ThisPluginExport = new T();

        // Plugin optional exports (based on v0.1-update1 (v0.1.1) API Standard)
        // ---------------------------------------------------------------
        // These exports are optional and can be removed if it's not
        // necessarily used. These optional exports are included under
        // additional functionalities used as a subset of v0.1, which is
        // called "update1" feature sets.

        // -> Plugin Async Game Launch Callback for Specific Game Region based on its IGameManager instance.
        TryRegisterApiExport<LaunchGameFromGameManagerAsyncDelegate>("LaunchGameFromGameManagerAsync", LaunchGameFromGameManagerAsync);
        // -> Plugin Game Run Check for Specific Game Region based on its IGameManager instance.
        TryRegisterApiExport<IsGameRunningDelegate>("IsGameRunning", IsGameRunning);
        // -> Plugin Async Game Run Awaiter for Specific Game Region based on its IGameManager instance.
        TryRegisterApiExport<WaitRunningGameAsyncDelegate>("WaitRunningGameAsync", WaitRunningGameAsync);
        // -> Plugin Game Process Killer for Specific Game Region based on its IGameManager instance.
        TryRegisterApiExport<IsGameRunningDelegate>("KillRunningGame", KillRunningGame);
    }

    private static readonly T ThisPluginExport;

    /// <summary>
    /// Specify which <see cref="IPlugin"/> instance to load and use in this plugin.
    /// </summary>
    /// <typeparam name="TPlugin">A member of COM Interface of <see cref="IPlugin"/>.</typeparam>
    protected new static void Load<TPlugin>(GameVersion interceptDllVersionTo = default)
        where TPlugin : class, IPlugin, new()
        => SharedStatic.Load<TPlugin>(interceptDllVersionTo);

    /// <summary>
    /// This method is an ABI proxy function between the PInvoke Export and the actual plugin's method.<br/>
    /// See the documentation for <see cref="SharedStatic.LaunchGameFromGameManagerCoreAsync(RunGameFromGameManagerContext, string?, bool, ProcessPriorityClass, CancellationToken)"/> method for more information.
    /// </summary>
    public static unsafe int LaunchGameFromGameManagerAsync(nint     gameManagerP,
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

            if (ThisPluginExport == null)
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

            RunGameFromGameManagerContext context = new RunGameFromGameManagerContext
            {
                GameManager          = gameManager,
                PresetConfig         = presetConfig,
                Plugin               = plugin,
                PrintGameLogCallback = printGameLogCallback,
                PluginHandle         = nint.Zero
            };

            bool                 isRunBoosted         = runBoostedInt == 1;
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

            taskResult = ThisPluginExport
                .LaunchGameFromGameManagerCoreAsync(context, startArguments, isRunBoosted, processPriorityClass, cts?.Token ?? CancellationToken.None)
                .AsResult();
            return 0;
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
    /// See the documentation for <see cref="SharedStatic.IsGameRunningCore(RunGameFromGameManagerContext, out bool, out DateTime)"/> method for more information.
    /// </summary>
    public static unsafe int IsGameRunning(nint gameManagerP, nint presetConfigP, out int isGameRunningInt, out DateTime gameStartTime)
    {
        isGameRunningInt = 0;
        gameStartTime    = default;

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

            RunGameFromGameManagerContext context = new RunGameFromGameManagerContext
            {
                GameManager          = gameManager,
                PresetConfig         = presetConfig,
                Plugin               = null!,
                PrintGameLogCallback = null!,
                PluginHandle         = nint.Zero
            };

            bool isSupported = ThisPluginExport.IsGameRunningCore(context, out bool isGameRunning, out gameStartTime);
            isGameRunningInt = isGameRunning ? 1 : 0;
            return isSupported ? 0 : throw new NotSupportedException("Method isn't overriden, yet");
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
    /// See the documentation for <see cref="SharedStatic.WaitRunningGameCoreAsync(RunGameFromGameManagerContext, CancellationToken)"/> method for more information.
    /// </summary>
    public static unsafe int WaitRunningGameAsync(nint gameManagerP, nint pluginP, nint presetConfigP, ref Guid cancelToken, out nint taskResult)
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

            if (ThisPluginExport == null)
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

            RunGameFromGameManagerContext context = new RunGameFromGameManagerContext
            {
                GameManager          = gameManager,
                PresetConfig         = presetConfig,
                Plugin               = plugin,
                PrintGameLogCallback = null!,
                PluginHandle         = nint.Zero
            };

            taskResult = ThisPluginExport
                .WaitRunningGameCoreAsync(context, cts?.Token ?? CancellationToken.None)
                .AsResult();
            return 0;
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
    /// See the documentation for <see cref="SharedStatic.KillRunningGameCore(RunGameFromGameManagerContext, out bool, out DateTime)"/> method for more information.
    /// </summary>
    public static unsafe int KillRunningGame(nint gameManagerP, nint presetConfigP, out int wasGameRunningInt, out DateTime gameStartTime)
    {
        wasGameRunningInt = 0;
        gameStartTime     = default;

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

            RunGameFromGameManagerContext context = new RunGameFromGameManagerContext
            {
                GameManager          = gameManager,
                PresetConfig         = presetConfig,
                Plugin               = null!,
                PrintGameLogCallback = null!,
                PluginHandle         = nint.Zero
            };

            bool isSupported = ThisPluginExport.KillRunningGameCore(context, out bool wasGameRunning, out gameStartTime);
            wasGameRunningInt = wasGameRunning ? 1 : 0;
            return isSupported ? 0 : throw new NotSupportedException("Method isn't overriden which mark this plugin doesn't support this feature!");
        }
        catch (Exception ex)
        {
            // ignored
            InstanceLogger.LogError(ex, "An error has occurred while trying to call KillRunningGameCore() from the plugin!");
            return Marshal.GetHRForException(ex);
        }
    }
}
