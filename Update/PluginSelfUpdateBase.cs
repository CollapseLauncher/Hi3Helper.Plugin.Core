using Hi3Helper.Plugin.Core.Management;
using Hi3Helper.Plugin.Core.Utility;
using System;
using System.Net.Http;
using System.Runtime.InteropServices.Marshalling;
using System.Threading;

namespace Hi3Helper.Plugin.Core.Update;

/// <summary>
/// A plugin self-updater instance which is used to perform update check and update download.
/// </summary>
[GeneratedComClass]
public abstract partial class PluginSelfUpdateBase : IPluginSelfUpdate
{
    protected abstract ReadOnlySpan<string> BaseCdnUrlSpan { get; }
    protected abstract HttpClient UpdateHttpClient { get; }

    public void TryPerformUpdateAsync(string? outputDir, bool checkForUpdatesOnly, InstallProgressDelegate? progressDelegate, in Guid cancelToken, out nint result)
    {
        CancellationTokenSource tokenSource = ComCancellationTokenVault.RegisterToken(in cancelToken);
        CancellationToken token = tokenSource.Token;
        result = TryPerformUpdateAsync(outputDir, checkForUpdatesOnly, progressDelegate, token).AsResult();
    }

    public void Free() => Dispose();

    public void Dispose()
    {
        UpdateHttpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
