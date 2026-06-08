#if MANUALCOM

using Hi3Helper.Plugin.Core.Management;
using Hi3Helper.Plugin.Core.Update;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using static System.Runtime.InteropServices.ComWrappers;

namespace Hi3Helper.Plugin.Core.ABI;

// ReSharper disable once InconsistentNaming
internal sealed unsafe class ABI_IPluginSelfUpdateWrapper
{
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_TryPerformUpdateAsync(ComInterfaceDispatch* thisNative, ushort* outputDirNative, int checkForUpdatesOnlyNative, nint progressDelegateNative, Guid* cancelTokenNativeParam, nint* resultNativeParam)
    {
        ref Guid cancelTokenNative = ref *cancelTokenNativeParam;
        ref nint resultNative = ref *resultNativeParam;
        int retVal;
        try
        {
            // Unmarshal - Convert native data to managed data.
            Guid cancelToken = cancelTokenNative;
            InstallProgressDelegate? progressDelegate = progressDelegateNative != 0 ? Marshal.GetDelegateForFunctionPointer<InstallProgressDelegate>(progressDelegateNative) : null;
            bool checkForUpdatesOnly = checkForUpdatesOnlyNative != 0;
            string? outputDir = Utf16StringMarshaller.ConvertToManaged(outputDirNative);
            IPluginSelfUpdate @this = ComInterfaceDispatch.GetInstance<IPluginSelfUpdate>(thisNative);
            @this.TryPerformUpdateAsync(outputDir, checkForUpdatesOnly, progressDelegate, in cancelToken, out nint result);
            // NotifyForSuccessfulInvoke - Keep alive any managed objects that need to stay alive across the call.
            retVal = 0; // S_OK
            // Marshal - Convert managed data to native data.
            resultNative = result;
        }
        catch (Exception exception)
        {
            retVal = Marshal.GetHRForException(exception);
        }

        return retVal;
    }
}

#endif