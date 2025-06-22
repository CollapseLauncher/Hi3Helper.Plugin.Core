using Hi3Helper.Plugin.Core.Utility;
using System;
using System.Net.Http;
using System.Runtime.InteropServices.Marshalling;
using System.Threading;

namespace Hi3Helper.Plugin.Core.Update;

[GeneratedComClass]
public abstract partial class PluginSelfUpdateBase : IPluginSelfUpdate
{
    protected virtual ReadOnlySpan<string> BaseCdnUrlSpan => ReadOnlySpan<string>.Empty;
    protected abstract HttpClient UpdateHttpClient { get; }

    nint IPluginSelfUpdate.TryPerformUpdateAsync(string? outputTempDir, in Guid cancelToken)
    {
        CancellationTokenSource tokenSource = ComCancellationTokenVault.RegisterToken(in cancelToken);
        CancellationToken token = tokenSource.Token;
        return TryPerformUpdateAsync(outputTempDir, token).AsResult();
    }

    public void Free() => Dispose();

    public void Dispose()
    {
        UpdateHttpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
