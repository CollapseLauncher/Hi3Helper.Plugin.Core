using Hi3Helper.Plugin.Core.Management;
using Hi3Helper.Plugin.Core.Utility;
using Microsoft.Extensions.Logging;
using System;
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
    /// This method is an ABI proxy function between the PInvoke Export and the actual plugin's method.<br/>
    /// See the documentation for <see cref="SharedStatic.LaunchGameFromGameManagerCoreAsync(IGameManager, IPlugin, PrintGameLog, CancellationToken)"/> method for more information.
    /// </summary>
    public static unsafe int LaunchGameFromGameManagerAsync(nint gameManagerP, nint pluginP, nint printGameLogCallbackP, ref Guid cancelToken, out nint taskResult)
    {
        taskResult = nint.Zero;
        try
        {
        #if MANUALCOM
            IPlugin?      plugin      = ComWrappers.ComInterfaceDispatch.GetInstance<IPlugin>((ComWrappers.ComInterfaceDispatch*)gameManagerP);
            IGameManager? gameManager = ComWrappers.ComInterfaceDispatch.GetInstance<IGameManager>((ComWrappers.ComInterfaceDispatch*)gameManagerP);
        #else
            IPlugin?      plugin      = ComInterfaceMarshaller<IPlugin>.ConvertToManaged((void*)pluginP);
            IGameManager? gameManager = ComInterfaceMarshaller<IGameManager>.ConvertToManaged((void*)gameManagerP);
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

            if (printGameLogCallback == null)
            {
                throw new NullReferenceException("Cannot cast PrintGameLog callback from the pointer, hence it gives null!");
            }

            CancellationTokenSource? cts = null;
            if (Unsafe.IsNullRef(ref cancelToken))
            {
                cts = ComCancellationTokenVault.RegisterToken(in cancelToken);
            }

            taskResult = ThisPluginExport.LaunchGameFromGameManagerCoreAsync(gameManager,
                                                                             plugin,
                                                                             printGameLogCallback,
                                                                             cts?.Token ?? CancellationToken.None).AsResult();
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
    /// See the documentation for <see cref="SharedStatic.IsGameRunningCore(IGameManager, out bool)"/> method for more information.
    /// </summary>
    public static unsafe int IsGameRunning(nint gameManagerP, out int isGameRunningInt)
    {
        isGameRunningInt = 0;

        try
        {
        #if MANUALCOM
            IGameManager? gameManager = ComWrappers.ComInterfaceDispatch.GetInstance<IGameManager>((ComWrappers.ComInterfaceDispatch*)gameManagerP);
        #else
            IGameManager? gameManager = ComInterfaceMarshaller<IGameManager>.ConvertToManaged((void*)gameManagerP);
        #endif

            if (gameManager == null)
            {
                throw new NullReferenceException("Cannot cast IGameManager from the pointer, hence it gives null!");
            }

            bool isSupported = ThisPluginExport.IsGameRunningCore(gameManager, out bool isGameRunning);
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
    /// See the documentation for <see cref="SharedStatic.WaitRunningGameCoreAsync(IGameManager, IPlugin, CancellationToken)"/> method for more information.
    /// </summary>
    public static unsafe int WaitRunningGameAsync(nint gameManagerP, nint pluginP, ref Guid cancelToken, out nint taskResult)
    {
        taskResult = nint.Zero;
        try
        {
        #if MANUALCOM
            IPlugin?      plugin      = ComWrappers.ComInterfaceDispatch.GetInstance<IPlugin>((ComWrappers.ComInterfaceDispatch*)gameManagerP);
            IGameManager? gameManager = ComWrappers.ComInterfaceDispatch.GetInstance<IGameManager>((ComWrappers.ComInterfaceDispatch*)gameManagerP);
        #else
            IPlugin?      plugin      = ComInterfaceMarshaller<IPlugin>.ConvertToManaged((void*)pluginP);
            IGameManager? gameManager = ComInterfaceMarshaller<IGameManager>.ConvertToManaged((void*)gameManagerP);
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

            CancellationTokenSource? cts = null;
            if (Unsafe.IsNullRef(ref cancelToken))
            {
                cts = ComCancellationTokenVault.RegisterToken(in cancelToken);
            }

            taskResult = ThisPluginExport.WaitRunningGameCoreAsync(gameManager,
                                                                   plugin,
                                                                   cts?.Token ?? CancellationToken.None).AsResult();
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
    /// See the documentation for <see cref="SharedStatic.KillRunningGameCore(IGameManager, out bool)"/> method for more information.
    /// </summary>
    public static unsafe int KillRunningGame(nint gameManagerP, out int wasGameRunningInt)
    {
        wasGameRunningInt = 0;

        try
        {
        #if MANUALCOM
            IGameManager? gameManager = ComWrappers.ComInterfaceDispatch.GetInstance<IGameManager>((ComWrappers.ComInterfaceDispatch*)gameManagerP);
        #else
            IGameManager? gameManager = ComInterfaceMarshaller<IGameManager>.ConvertToManaged((void*)gameManagerP);
        #endif

            if (gameManager == null)
            {
                throw new NullReferenceException("Cannot cast IGameManager from the pointer, hence it gives null!");
            }

            bool isSupported = ThisPluginExport.KillRunningGameCore(gameManager, out bool wasGameRunning);
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
