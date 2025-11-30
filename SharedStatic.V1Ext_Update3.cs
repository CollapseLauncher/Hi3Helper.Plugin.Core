using Hi3Helper.Plugin.Core.Management;
using Hi3Helper.Plugin.Core.Management.PresetConfig;
using Hi3Helper.Plugin.Core.Utility;
using Microsoft.Extensions.Logging;
using System;
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
    private static void InitExtension_Update3Exports()
    {
        /* ----------------------------------------------------------------------
         * Update 3 Feature Sets
         * ----------------------------------------------------------------------
         * This feature sets includes the following feature:
         *  - Game Launch
         *    - StartResizableWindowHook
         */

        // -> Plugin Async Resizable Window Hook Callback for Specific Game Region based on its IGameManager instance.
        TryRegisterApiExport<StartResizableWindowHookAsyncDelegate>("StartResizableWindowHookAsync", StartResizableWindowHookAsync);
    }

    #region ABI Proxies
    /// <summary>
    /// This method is an ABI proxy function between the PInvoke Export and the actual plugin's method.<br/>
    /// See the documentation for <see cref="SharedStaticV1Ext{T}.StartResizableWindowHookAsync(RunGameFromGameManagerContext, string?, int, int, string?, CancellationToken)"/> method for more information.
    /// </summary>
    private static unsafe HResult StartResizableWindowHookAsync(nint     gameManagerP,
                                                                nint     presetConfigP,
                                                                nint     exeName,
                                                                int      exeNameLen,
                                                                int      height,
                                                                int      width,
                                                                nint     exeDir,
                                                                int      exeDirLen,
                                                                ref Guid cancelToken,
                                                                out nint taskResult)
    {
        taskResult = nint.Zero;
        try
        {
#if MANUALCOM
            IGameManager? gameManager = ComWrappers.ComInterfaceDispatch.GetInstance<IGameManager>((ComWrappers.ComInterfaceDispatch*)gameManagerP);
            IPluginPresetConfig? presetConfig = ComWrappers.ComInterfaceDispatch.GetInstance<IPluginPresetConfig>((ComWrappers.ComInterfaceDispatch*)presetConfigP);
#else
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
                Plugin               = null!,
                PrintGameLogCallback = null!,
                PluginHandle         = nint.Zero
            };

            string? executableName = null;
            if (exeNameLen > 0)
            {
                char* exeNameP = (char*)exeName;
                ReadOnlySpan<char> executableNameSpan = Mem.CreateSpanFromNullTerminated<char>(exeNameP);
                if (executableNameSpan.Length > exeNameLen)
                {
                    executableNameSpan = executableNameSpan[..exeNameLen];
                }

                executableName = executableNameSpan.IsEmpty ? null : executableNameSpan.ToString();
            }

            string? executableDirectory = null;
            if (exeDirLen > 0)
            {
                char* exeDirP = (char*)exeDir;
                ReadOnlySpan<char> executableDirectorySpan = Mem.CreateSpanFromNullTerminated<char>(exeDirP);
                if (executableDirectorySpan.Length > exeNameLen)
                {
                    executableDirectorySpan = executableDirectorySpan[..exeDirLen];
                }

                executableDirectory = executableDirectorySpan.IsEmpty ? null : executableDirectorySpan.ToString();
            }

            (bool isSupported, Task<bool> task) = ThisExtensionExport
                .StartResizableWindowHookAsync(context,
                                               executableName,
                                               height == int.MinValue ? null : height,
                                               width == int.MinValue ? null : width,
                                               executableDirectory,
                                               cts?.Token ?? CancellationToken.None);

            taskResult = task.AsResult();
            return isSupported;
        }
        catch (Exception ex)
        {
            // ignored
            InstanceLogger.LogError(ex, "An error has occurred while trying to call StartResizableWindowHookAsync() from the plugin!");
            return Marshal.GetHRForException(ex);
        }
    }
    #endregion

    #region Core Methods
    /// <summary>
    /// Asynchronously hook to the game process making the window resizable and wait until the game exit.
    /// </summary>
    /// <param name="context">The context to launch the game from <see cref="IGameManager"/>.</param>
    /// <param name="executableName">The name of the game executable.</param>
    /// <param name="height">Height of the host screen.</param>
    /// <param name="width">Height of the host screen.</param>
    /// <param name="executableDirectory">The path where the game executable is located.</param>
    /// <param name="token">
    /// Cancellation token to pass into the plugin's game launch mechanism.<br/>
    /// If cancellation is requested, it will cancel the awaiting but not killing the game process.
    /// </param>
    /// <returns>
    /// Returns <c>IsSupported.false</c> if the plugin's API Standard is equal or lower than v0.1.3 or if this method isn't overriden.<br/>
    /// Otherwise, <c>IsSupported.true</c> if the plugin supports game launch mechanism and this method.
    /// </returns>
    protected virtual (bool IsSupported, Task<bool> Task) StartResizableWindowHookAsync(
        RunGameFromGameManagerContext context,
        string?                       executableName,
        int?                          height,
        int?                          width,
        string?                       executableDirectory,
        CancellationToken             token)
    {
        return (false, Task.FromResult(false));
    }
    #endregion
}
