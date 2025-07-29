using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using Hi3Helper.Plugin.Core.Management.PresetConfig;
using Hi3Helper.Plugin.Core.Update;
using Hi3Helper.Plugin.Core.Utility;
using Microsoft.Extensions.Logging;

#if MANUALCOM
using Hi3Helper.Plugin.Core.ABI;
#endif

namespace Hi3Helper.Plugin.Core;

/// <summary>
/// The base class for accessing plugin metadata and preset configuration.
/// </summary>
/// <remarks>
/// This <see cref="PluginBase"/> base class requires the abstract methods to be implemented, except for the <see cref="IPlugin.CancelAsync(in Guid)"/> method.
/// </remarks>
[GeneratedComClass]
public abstract partial class PluginBase : IPlugin
{
    /// <inheritdoc/>
    public abstract void GetPluginName(out string? result);

    /// <inheritdoc/>
    public abstract void GetPluginDescription(out string? result);

    /// <inheritdoc/>
    public abstract void GetPluginAuthor(out string? result);

    /// <inheritdoc/>
    public abstract unsafe void GetPluginCreationDate(out DateTime* result);

    /// <inheritdoc/>
    public abstract void GetPresetConfigCount(out int count);

    /// <inheritdoc/>
    public abstract void GetPresetConfig(int index, out IPluginPresetConfig result);

    /// <inheritdoc/>
    public virtual void GetPluginSelfUpdater(out IPluginSelfUpdate? selfUpdate) => Unsafe.SkipInit(out selfUpdate);

    /// <inheritdoc/>
    public virtual void GetPluginAppIconUrl(out string result) => Unsafe.SkipInit(out result);

    /// <inheritdoc/>
    public virtual void GetNotificationPosterUrl(out string result) => Unsafe.SkipInit(out result);

    /// <inheritdoc/>
    public void CancelAsync(in Guid cancelToken)
    {
        // Cancel the async operation using the provided cancel token
        ComCancellationTokenVault.CancelToken(in cancelToken);
    }

    /// <inheritdoc/>
    public void SetPluginProxySettings(string? hostUri, string? username, string? password, out bool isSuccess)
    {
        // If all nulls or empty, assume as it resets the configuration, then return true.
        if (string.IsNullOrEmpty(hostUri) &&
            string.IsNullOrEmpty(username) &&
            string.IsNullOrEmpty(password))
        {
            SharedStatic.InstanceLogger.LogTrace("[IPlugin::SetPluginProxySettings] Proxy has been disabled!");

            SharedStatic.ProxyHost     = null;
            SharedStatic.ProxyUsername = null;
            SharedStatic.ProxyPassword = null;
            isSuccess = true;
            return;
        }

        // Try parse host URI and check if the username is not blank while the password isn't.
        if (!Uri.TryCreate(hostUri, UriKind.Absolute, out SharedStatic.ProxyHost) ||
            (string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password)))
        {
            SharedStatic.ProxyHost = null;
            isSuccess = false;
            return;
        }

        SharedStatic.InstanceLogger.LogTrace("[IPlugin::SetPluginProxySettings] Proxy has been enabled! Hostname: {Hostname} as: {Username}", hostUri, username);

        // Set the username and password and return true.
        SharedStatic.ProxyUsername = username;
        SharedStatic.ProxyPassword = password;
        isSuccess = true;
    }

    /// <inheritdoc/>
    public void SetPluginLocaleId(string? localeId) => SharedStatic.SetPluginCurrentLocale(localeId);

    /// <inheritdoc cref="IFree.Free"/>
    public void Free() => Dispose();

    public virtual unsafe void Dispose()
    {
        // Cancel all the cancellable async operations first before disposing all plugin instance
        ComCancellationTokenVault.DangerousCancelAndUnregisterAllToken();

        // Then continue disposing all the plugin.
        GetPresetConfigCount(out int presetConfigCount);
        for (int i = 0; i < presetConfigCount; i++)
        {
            GetPresetConfig(i, out IPluginPresetConfig presetConfig);
#if MANUALCOM
            nint ptr = (nint)ComWrappersExtension<PluginPresetConfigWrappers>.GetComInterfacePtrFromWrappers(presetConfig);

            Guid freeGuid = new(ComInterfaceId.ExFree);
            Marshal.QueryInterface(ptr, in freeGuid, out nint ppv);

            object? objFree = ComWrappersExtension<PluginPresetConfigWrappers>.GetComInterfaceObjFromWrappers(ppv);
            if (objFree is IFree disposablePresetConfig)
            {
                disposablePresetConfig.Free();
            }
#else
            nint ptr = (nint)ComInterfaceMarshaller<IPluginPresetConfig>.ConvertToUnmanaged(presetConfig);

            Guid freeGuid = new(ComInterfaceId.ExFree);
            Marshal.QueryInterface(ptr, in freeGuid, out nint ppv);

            IFree? objFree = ComInterfaceMarshaller<IFree>.ConvertToManaged((void*)ppv);
            objFree?.Free();
#endif
        }

        GC.SuppressFinalize(this);
        GetPluginName(out string? pluginName);
        SharedStatic.InstanceLogger.LogTrace("[PluginBase::Dispose] Plugin: {PluginName} has been disposed!", pluginName);
    }
}
