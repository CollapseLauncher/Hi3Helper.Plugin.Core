using System;
using System.Runtime.InteropServices.Marshalling;
using Hi3Helper.Plugin.Core.Management.PresetConfig;
using Hi3Helper.Plugin.Core.Utility;
using Microsoft.Extensions.Logging;

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
    public abstract string GetPluginName();
    public abstract string GetPluginDescription();
    public abstract string GetPluginAuthor();
    public abstract unsafe DateTime* GetPluginCreationDate();
    public abstract int GetPresetConfigCount();
    public abstract IPluginPresetConfig GetPresetConfig(int index);

    public void CancelAsync(in Guid cancelToken)
    {
        // Cancel the async operation using the provided cancel token
        ComCancellationTokenVault.CancelToken(in cancelToken);
    }

    public bool SetPluginProxySettings(string? hostUri, string? username, string? password)
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
            return true;
        }

        // Try parse host URI and check if the username is not blank while the password isn't.
        if (!Uri.TryCreate(hostUri, UriKind.Absolute, out SharedStatic.ProxyHost) ||
            (string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password)))
        {
            SharedStatic.ProxyHost = null;
            return false;
        }

        SharedStatic.InstanceLogger.LogTrace("[IPlugin::SetPluginProxySettings] Proxy has been enabled! Hostname: {Hostname} as: {Username}", hostUri, username);

        // Set the username and password and return true.
        SharedStatic.ProxyUsername = username;
        SharedStatic.ProxyPassword = password;
        return true;
    }

    public void SetPluginLocaleId(string? localeId) => SharedStatic.SetPluginCurrentLocale(localeId);

    public virtual void Dispose()
    {
        // Cancel all the cancellable async operations first before disposing all plugin instance
        ComCancellationTokenVault.DangerousCancelAndUnregisterAllToken();

        // Then continue disposing all the plugin.
        int presetConfigCount = GetPresetConfigCount();
        for (int i = 0; i < presetConfigCount; i++)
        {
            IPluginPresetConfig presetConfig = GetPresetConfig(i);
            if (presetConfig is IDisposable disposablePresetConfig)
            {
                disposablePresetConfig.Dispose();
            }
        }

        GC.SuppressFinalize(this);
        SharedStatic.InstanceLogger.LogTrace("[PluginBase::Dispose] Plugin: {PluginName} has been disposed!", GetPluginName());
    }
}
