using Hi3Helper.Plugin.Core.Management;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hi3Helper.Plugin.Core.Update;

public partial class PluginSelfUpdateBase
{
    protected virtual async Task<SelfUpdateReturnCode> TryPerformUpdateAsync(string? outputTempDir, CancellationToken token)
    {
        if (BaseCdnUrlSpan.IsEmpty)
        {
            return SelfUpdateReturnCode.NoAvailableUpdate;
        }

        var updateInfo = await TryGetAvailableCdn(token);
        if (updateInfo is { Info: null, BaseUrl: null })
        {
            return SelfUpdateReturnCode.NoReachableCdn;
        }

        GameVersion currentPluginVersion = SharedStatic.CurrentPluginVersion;
        throw new NotImplementedException();
    }

    protected virtual Task<(SelfUpdateReferenceInfo? Info, string? BaseUrl)> TryGetAvailableCdn(CancellationToken token)
    {
        throw new NotImplementedException();
    }
}
