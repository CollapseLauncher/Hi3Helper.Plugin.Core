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
    /// <summary>
    /// A Span/Array of the CDN URL to use for getting the update.
    /// </summary>
    protected abstract ReadOnlySpan<string> BaseCdnUrlSpan { get; }

    /// <summary>
    /// An <see cref="HttpClient"/> client to be used to perform download for update operations.
    /// </summary>
    protected abstract HttpClient UpdateHttpClient { get; }

    /// <summary>
    /// HACK: This is a workaround to allow the core plugin library to access the base CDN URL. This way, there will be no breaking changes on the plugin interface.<br/>
    /// The return is the same as <see cref="BaseCdnUrlSpan"/>.<br/>
    /// <br/>
    /// FOR PLUGIN DEVELOPERS:<br/>
    /// Please use <see cref="BaseCdnUrlSpan"/> instead of this property. This is for Core Plugin's purpose only.
    /// </summary>
    internal ReadOnlySpan<string> BaseCdnUrl => BaseCdnUrlSpan;

    /// <inheritdoc/>
    public void TryPerformUpdateAsync(string? outputDir, bool checkForUpdatesOnly, InstallProgressDelegate? progressDelegate, in Guid cancelToken, out nint result)
    {
        CancellationTokenSource tokenSource = ComCancellationTokenVault.RegisterToken(in cancelToken);
        CancellationToken token = tokenSource.Token;
        result = TryPerformUpdateAsync(outputDir, checkForUpdatesOnly, progressDelegate, token).AsResult();
    }

    /// <inheritdoc cref="IFree.Free"/>
    public void Free() => Dispose();

    public void Dispose()
    {
        UpdateHttpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
