#if MANUALCOM

using Hi3Helper.Plugin.Core.Management;
using Hi3Helper.Plugin.Core.Utility;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using static System.Runtime.InteropServices.ComWrappers;

namespace Hi3Helper.Plugin.Core.ABI;

// ReSharper disable once InconsistentNaming
internal sealed unsafe class ABI_IGameManagerWrapper
{
    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_GetGamePath(ComInterfaceDispatch* thisNative, ushort** resultNativeParam)
        => ABIExtension.AllocUtf16StringFromFunc(ComInterfaceDispatch.GetInstance<IGameManager>(thisNative).GetGamePath, resultNativeParam);

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_SetGamePath(ComInterfaceDispatch* thisNative, ushort* gamePathNative)
    {
        int retVal;
        try
        {
            // Unmarshal - Convert native data to managed data.
            var gamePath = Utf16StringMarshaller.ConvertToManaged(gamePathNative);
            var @this = ComInterfaceDispatch.GetInstance<IGameManager>(thisNative);
            @this.SetGamePath(gamePath ?? "");
            // NotifyForSuccessfulInvoke - Keep alive any managed objects that need to stay alive across the call.
            retVal = 0; // S_OK
        }
        catch (Exception exception)
        {
            retVal = Marshal.GetHRForException(exception);
        }

        return retVal;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_GetCurrentGameVersion(ComInterfaceDispatch* thisNative, GameVersion* gameVersionNativeParam)
    {
        ref GameVersion gameVersionNative = ref *gameVersionNativeParam;
        int retVal;
        try
        {
            // Unmarshal - Convert native data to managed data.
            var @this = ComInterfaceDispatch.GetInstance<IGameManager>(thisNative);
            @this.GetCurrentGameVersion(out var gameVersion);
            // NotifyForSuccessfulInvoke - Keep alive any managed objects that need to stay alive across the call.
            retVal = 0; // S_OK
            // Marshal - Convert managed data to native data.
            gameVersionNative = gameVersion;
        }
        catch (Exception exception)
        {
            retVal = Marshal.GetHRForException(exception);
        }

        return retVal;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_SetCurrentGameVersion(ComInterfaceDispatch* thisNative, GameVersion* versionNativeParam)
    {
        ref GameVersion versionNative = ref *versionNativeParam;
        int retVal;
        try
        {
            // Unmarshal - Convert native data to managed data.
            var version = versionNative;
            var @this = ComInterfaceDispatch.GetInstance<IGameManager>(thisNative);
            @this.SetCurrentGameVersion(in version);
            // NotifyForSuccessfulInvoke - Keep alive any managed objects that need to stay alive across the call.
            retVal = 0; // S_OK
        }
        catch (Exception exception)
        {
            retVal = Marshal.GetHRForException(exception);
        }

        return retVal;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_GetApiGameVersion(ComInterfaceDispatch* thisNative, GameVersion* gameVersionNativeParam)
    {
        ref GameVersion gameVersionNative = ref *gameVersionNativeParam;
        int retVal;
        try
        {
            // Unmarshal - Convert native data to managed data.
            var @this = ComInterfaceDispatch.GetInstance<IGameManager>(thisNative);
            @this.GetApiGameVersion(out var gameVersion);
            // NotifyForSuccessfulInvoke - Keep alive any managed objects that need to stay alive across the call.
            retVal = 0; // S_OK
            // Marshal - Convert managed data to native data.
            gameVersionNative = gameVersion;
        }
        catch (Exception exception)
        {
            retVal = Marshal.GetHRForException(exception);
        }

        return retVal;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_GetApiPreloadGameVersion(ComInterfaceDispatch* thisNative, GameVersion* gameVersionNativeParam)
    {
        ref GameVersion gameVersionNative = ref *gameVersionNativeParam;
        int retVal;
        try
        {
            // Unmarshal - Convert native data to managed data.
            var @this = ComInterfaceDispatch.GetInstance<IGameManager>(thisNative);
            @this.GetApiPreloadGameVersion(out var gameVersion);
            // NotifyForSuccessfulInvoke - Keep alive any managed objects that need to stay alive across the call.
            retVal = 0; // S_OK
            // Marshal - Convert managed data to native data.
            gameVersionNative = gameVersion;
        }
        catch (Exception exception)
        {
            retVal = Marshal.GetHRForException(exception);
        }

        return retVal;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_IsGameInstalled(ComInterfaceDispatch* thisNative, int* resultNativeParam)
    {
        ref int resultNative = ref *resultNativeParam;
        int retVal;
        try
        {
            // Unmarshal - Convert native data to managed data.
            var @this = ComInterfaceDispatch.GetInstance<IGameManager>(thisNative);
            @this.IsGameInstalled(out var result);
            // NotifyForSuccessfulInvoke - Keep alive any managed objects that need to stay alive across the call.
            retVal = 0; // S_OK
            // Marshal - Convert managed data to native data.
            resultNative = result ? 1 : 0;
        }
        catch (Exception exception)
        {
            retVal = Marshal.GetHRForException(exception);
        }

        return retVal;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_IsGameHasUpdate(ComInterfaceDispatch* thisNative, int* resultNativeParam)
    {
        ref int resultNative = ref *resultNativeParam;
        int retVal;
        try
        {
            // Unmarshal - Convert native data to managed data.
            var @this = ComInterfaceDispatch.GetInstance<IGameManager>(thisNative);
            @this.IsGameHasUpdate(out var result);
            // NotifyForSuccessfulInvoke - Keep alive any managed objects that need to stay alive across the call.
            retVal = 0; // S_OK
            // Marshal - Convert managed data to native data.
            resultNative = result ? 1 : 0;
        }
        catch (Exception exception)
        {
            retVal = Marshal.GetHRForException(exception);
        }

        return retVal;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_IsGameHasPreload(ComInterfaceDispatch* thisNative, int* resultNativeParam)
    {
        ref int resultNative = ref *resultNativeParam;
        int retVal;
        try
        {
            // Unmarshal - Convert native data to managed data.
            var @this = ComInterfaceDispatch.GetInstance<IGameManager>(thisNative);
            @this.IsGameHasPreload(out var result);
            // NotifyForSuccessfulInvoke - Keep alive any managed objects that need to stay alive across the call.
            retVal = 0; // S_OK
            // Marshal - Convert managed data to native data.
            resultNative = result ? 1 : 0;
        }
        catch (Exception exception)
        {
            retVal = Marshal.GetHRForException(exception);
        }

        return retVal;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_LoadConfig(ComInterfaceDispatch* thisNative)
    {
        int retVal;
        try
        {
            // Unmarshal - Convert native data to managed data.
            var @this = ComInterfaceDispatch.GetInstance<IGameManager>(thisNative);
            @this.LoadConfig();
            // NotifyForSuccessfulInvoke - Keep alive any managed objects that need to stay alive across the call.
            retVal = 0; // S_OK
        }
        catch (Exception exception)
        {
            retVal = Marshal.GetHRForException(exception);
        }

        return retVal;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_SaveConfig(ComInterfaceDispatch* thisNative)
    {
        int retVal;
        try
        {
            // Unmarshal - Convert native data to managed data.
            var @this = ComInterfaceDispatch.GetInstance<IGameManager>(thisNative);
            @this.SaveConfig();
            // NotifyForSuccessfulInvoke - Keep alive any managed objects that need to stay alive across the call.
            retVal = 0; // S_OK
        }
        catch (Exception exception)
        {
            retVal = Marshal.GetHRForException(exception);
        }

        return retVal;
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvMemberFunction)])]
    internal static int ABI_FindExistingInstallPathAsync(ComInterfaceDispatch* thisNative, Guid* cancelTokenNativeParam, nint* resultNativeParam)
    {
        ref Guid cancelTokenNative = ref *cancelTokenNativeParam;
        ref nint resultNative = ref *resultNativeParam;
        int retVal;
        try
        {
            // Unmarshal - Convert native data to managed data.
            var cancelToken = cancelTokenNative;
            var @this = ComInterfaceDispatch.GetInstance<IGameManager>(thisNative);
            @this.FindExistingInstallPathAsync(in cancelToken, out var result);
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