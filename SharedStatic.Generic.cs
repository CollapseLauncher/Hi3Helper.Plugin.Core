using Hi3Helper.Plugin.Core.Management;
using Hi3Helper.Plugin.Core.Utility;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Threading;

using static Hi3Helper.Plugin.Core.Utility.GameManagerExtension;

namespace Hi3Helper.Plugin.Core;

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
        TryRegisterApiExport<LaunchGameFromGameManagerDelegate>("LaunchGameFromGameManager", LaunchGameFromGameManager);
        // -> Plugin Game Run Check for Specific Game Region based on its IGameManager instance.
        TryRegisterApiExport<IsGameRunningDelegate>("IsGameRunning", IsGameRunning);
    }

    private static readonly T ThisPluginExport;

    public static unsafe int LaunchGameFromGameManager(nint gameManagerP, nint pluginP, nint printGameLogCallbackP, ref Guid cancelToken, out nint taskResult)
    {
        taskResult = nint.Zero;
        try
        {
        #if MANUALCOM
        #else
            IPlugin? plugin = ComInterfaceMarshaller<IPlugin>.ConvertToManaged((void*)pluginP);
            IGameManager? gameManager = ComInterfaceMarshaller<IGameManager>.ConvertToManaged((void*)gameManagerP);
            PrintGameLog printGameLogCallback = Marshal.GetDelegateForFunctionPointer<PrintGameLog>(printGameLogCallbackP);
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
            InstanceLogger.LogError(ex, "An error has occurred while trying to call LaunchGameFromGameManager() from the plugin!");
            return Marshal.GetHRForException(ex);
        }
    }

    public static unsafe int IsGameRunning(nint gameManagerP, out int isGameRunning)
    {
        isGameRunning = 0;

        try
        {
        #if MANUALCOM
        #else
            IGameManager? gameManager = ComInterfaceMarshaller<IGameManager>.ConvertToManaged((void*)gameManagerP);
        #endif

            if (gameManager == null)
            {
                throw new NullReferenceException("Cannot cast IGameManager from the pointer, hence it gives null!");
            }

            isGameRunning = ThisPluginExport.IsGameRunningCore(gameManager) ? 1 : 0;
            return 0;
        }
        catch (Exception ex)
        {
            // ignored
            InstanceLogger.LogError(ex, "An error has occurred while trying to call IsGameRunning() from the plugin!");
            return Marshal.GetHRForException(ex);
        }
    }
}
