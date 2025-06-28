#if MANUALCOM

using Hi3Helper.Plugin.Core.Management;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static System.Runtime.InteropServices.ComWrappers;

namespace Hi3Helper.Plugin.Core.ABI;

// ReSharper disable once InconsistentNaming
internal sealed unsafe class ABI_IGameInstallerWrapper
{
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_GetGameSizeAsync(ComInterfaceDispatch* thisNative, GameInstallerKind gameInstallerKind, Guid* cancelTokenNativeParam, nint* resultNativeParam)
    {
        ref Guid cancelTokenNative = ref *cancelTokenNativeParam;
        ref nint resultNative = ref *resultNativeParam;
        int retVal;
        try
        {
            // Unmarshal - Convert native data to managed data.
            var cancelToken = cancelTokenNative;
            var @this = ComInterfaceDispatch.GetInstance<IGameInstaller>(thisNative);
            @this.GetGameSizeAsync(gameInstallerKind, in cancelToken, out var result);
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

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_GetGameDownloadedSizeAsync(ComInterfaceDispatch* thisNative, GameInstallerKind gameInstallerKind, Guid* cancelTokenNativeParam, nint* resultNativeParam)
    {
        ref Guid cancelTokenNative = ref *cancelTokenNativeParam;
        ref nint resultNative = ref *resultNativeParam;
        int retVal;
        try
        {
            // Unmarshal - Convert native data to managed data.
            var cancelToken = cancelTokenNative;
            var @this = ComInterfaceDispatch.GetInstance<IGameInstaller>(thisNative);
            @this.GetGameDownloadedSizeAsync(gameInstallerKind, in cancelToken, out var result);
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

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_StartInstallAsync(ComInterfaceDispatch* thisNative, nint progressDelegateNative, nint progressStateDelegateNative, Guid* cancelTokenNativeParam, nint* resultNativeParam)
    {
        ref Guid cancelTokenNative = ref *cancelTokenNativeParam;
        ref nint resultNative = ref *resultNativeParam;
        int retVal;
        try
        {
            // Unmarshal - Convert native data to managed data.
            var cancelToken = cancelTokenNative;
            var progressStateDelegate = progressStateDelegateNative != 0 ? Marshal.GetDelegateForFunctionPointer<InstallProgressStateDelegate>(progressStateDelegateNative) : null;
            var progressDelegate = progressDelegateNative != 0 ? Marshal.GetDelegateForFunctionPointer<InstallProgressDelegate>(progressDelegateNative) : null;
            var @this = ComInterfaceDispatch.GetInstance<IGameInstaller>(thisNative);
            @this.StartInstallAsync(progressDelegate, progressStateDelegate, in cancelToken, out var result);
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

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_StartUpdateAsync(ComInterfaceDispatch* thisNative, nint progressDelegateNative, nint progressStateDelegateNative, Guid* cancelTokenNativeParam, nint* resultNativeParam)
    {
        ref Guid cancelTokenNative = ref *cancelTokenNativeParam;
        ref nint resultNative = ref *resultNativeParam;
        int retVal;
        try
        {
            // Unmarshal - Convert native data to managed data.
            var cancelToken = cancelTokenNative;
            var progressStateDelegate = progressStateDelegateNative != 0 ? Marshal.GetDelegateForFunctionPointer<InstallProgressStateDelegate>(progressStateDelegateNative) : null;
            var progressDelegate = progressDelegateNative != 0 ? Marshal.GetDelegateForFunctionPointer<InstallProgressDelegate>(progressDelegateNative) : null;
            var @this = ComInterfaceDispatch.GetInstance<IGameInstaller>(thisNative);
            @this.StartUpdateAsync(progressDelegate, progressStateDelegate, in cancelToken, out var result);
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

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_StartPreloadAsync(ComInterfaceDispatch* thisNative, nint progressDelegateNative, nint progressStateDelegateNative, Guid* cancelTokenNativeParam, nint* resultNativeParam)
    {
        ref Guid cancelTokenNative = ref *cancelTokenNativeParam;
        ref nint resultNative = ref *resultNativeParam;
        int retVal;
        try
        {
            // Unmarshal - Convert native data to managed data.
            var cancelToken = cancelTokenNative;
            var progressStateDelegate = progressStateDelegateNative != 0 ? Marshal.GetDelegateForFunctionPointer<InstallProgressStateDelegate>(progressStateDelegateNative) : null;
            var progressDelegate = progressDelegateNative != 0 ? Marshal.GetDelegateForFunctionPointer<InstallProgressDelegate>(progressDelegateNative) : null;
            var @this = ComInterfaceDispatch.GetInstance<IGameInstaller>(thisNative);
            @this.StartPreloadAsync(progressDelegate, progressStateDelegate, in cancelToken, out var result);
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