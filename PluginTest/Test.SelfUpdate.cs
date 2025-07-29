using System;
using System.IO;
using System.Runtime.InteropServices;
using Hi3Helper.Plugin.Core.Management.PresetConfig;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Hi3Helper.Plugin.Core.Update;
using Hi3Helper.Plugin.Core.Utility;

namespace PluginTest;

internal static partial class Test
{
    internal static async Task TestSelfUpdate(PluginGetPlugin delegateIn, ILogger logger)
    {
        PluginInterface ??= GetPlugin(delegateIn, out _);
        ThrowIfPluginIsNull(PluginInterface);

        logger.LogInformation("IPlugin->GetPluginSelfUpdater(): Getting plugin self-updater");
        PluginInterface.GetPluginSelfUpdater(out IPluginSelfUpdate? updater);
        if (updater == null)
        {
            logger.LogInformation("IPlugin->GetPluginSelfUpdater(): IPluginSelfUpdate returns null! Ignoring...");
            return;
        }

        string curDir = Path.Combine(Environment.CurrentDirectory, "UpdateTest");

        logger.LogInformation("IPluginSelfUpdate->TryPerformUpdateAsync(): Checking for update...");
        updater.TryPerformUpdateAsync(curDir, true, null, in CancelToken, out nint result);
        nint updateInfo = await result.AsTask<nint>();

        using SelfUpdateReturnInfo updateInfoP = updateInfo.AsRef<SelfUpdateReturnInfo>();
        logger.LogInformation("IPluginSelfUpdate->TryPerformUpdateAsync(): Successfully invoking update test with return code: {code}", updateInfoP.ReturnCode);
        updateInfo.Free();
    }
}
