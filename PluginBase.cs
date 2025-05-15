using System;
using System.Runtime.InteropServices.Marshalling;
using Hi3Helper.Plugin.Core.Management;
using Hi3Helper.Plugin.Core.Utility;

namespace Hi3Helper.Plugin.Core
{
    [GeneratedComClass]
    public abstract partial class PluginBase : IPlugin
    {
        public abstract string GetPluginName();
        public abstract string GetPluginDescription();
        public abstract string GetPluginAuthor();
        public abstract unsafe DateTime* GetPluginCreationDate();
        public abstract int GetPresetConfigCount();
        public abstract IPluginPresetConfig GetPresetConfig(int index);
        void IPlugin.CancelAsync(in Guid cancelToken)
        {
            // Cancel the async operation using the provided cancel token
            ComCancellationTokenVault.CancelToken(in cancelToken);
        }
    }
}
