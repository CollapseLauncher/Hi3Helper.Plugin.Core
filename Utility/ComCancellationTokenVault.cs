using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Extensions.Logging;

namespace Hi3Helper.Plugin.Core.Utility;

/// <summary>
/// An internal static class which stores the Cancellation Token of each respective Guid token.<br/>
/// This class also handles token registration for <see cref="CancellationTokenSource"/> used by plugin's async method.
/// </summary>
internal static class ComCancellationTokenVault
{
    private static readonly Dictionary<Guid, CancellationTokenSource> TokenVault = new();
    private static readonly Lock ThreadLock = new();

    /// <summary>
    /// Registers a <see cref="CancellationTokenSource"/> in the vault using the specified <paramref name="tokenHandle"/> as a unique key.
    /// </summary>
    /// <param name="tokenHandle">The unique <see cref="Guid"/> handle to associate with the token.</param>
    /// <returns>
    /// The <see cref="CancellationTokenSource"/> associated with the specified <paramref name="tokenHandle"/>.
    /// If a token source with the given handle already exists, it is returned; otherwise, a new one is created, registered, and returned.
    /// </returns>
    /// <remarks>
    /// This method is thread-safe. If the token handle is already registered, the existing token source is returned.
    /// </remarks>
    internal static CancellationTokenSource RegisterToken(in Guid tokenHandle)
    {
        using (ThreadLock.EnterScope())
        {
            // Try to get the existing Token Source. If it's already exist, then return it.
            if (TokenVault.TryGetValue(tokenHandle, out CancellationTokenSource? tokenSource))
            {
                return tokenSource;
            }

            // Otherwise, create a new one and return it.
            TokenVault.Add(tokenHandle, tokenSource = new CancellationTokenSource());
            return tokenSource;
        }
    }

    /// <summary>
    /// Unregisters and disposes the <see cref="CancellationTokenSource"/> associated with the specified <paramref name="tokenHandle"/>.
    /// </summary>
    /// <param name="tokenHandle">The unique <see cref="Guid"/> handle of the token to unregister.</param>
    /// <param name="tokenSource">
    /// When this method returns, contains the <see cref="CancellationTokenSource"/> that was removed from the vault, or <c>null</c> if the token was not found.
    /// </param>
    /// <returns>
    /// <c>true</c> if the token was found, removed, and disposed; <c>false</c> if the token was not found in the vault.
    /// </returns>
    /// <remarks>
    /// This method is thread-safe. The removed <see cref="CancellationTokenSource"/> is disposed after removal.
    /// </remarks>
    internal static bool UnregisterToken(in Guid tokenHandle, out CancellationTokenSource? tokenSource)
    {
        using (ThreadLock.EnterScope())
        {
            // Try to remove the Token Source from vault and dispose it, then return the status whether it's removed or not.
            bool isRemoved = TokenVault.Remove(tokenHandle, out tokenSource);
            tokenSource?.Dispose();
            return isRemoved;
        }
    }


    /// <summary>
    /// Cancels the <see cref="CancellationTokenSource"/> associated with the specified <paramref name="tokenHandle"/>.
    /// </summary>
    /// <param name="tokenHandle">The unique <see cref="Guid"/> handle of the token to cancel.</param>
    /// <param name="isUnregisterToken">
    /// If <c>true</c>, the token will also be unregistered and disposed after cancellation. 
    /// If <c>false</c>, the token remains in the vault after cancellation.
    /// </param>
    /// <returns>
    /// <c>true</c> if the token was found and cancelled; <c>false</c> if the token was not found in the vault.
    /// </returns>
    /// <remarks>
    /// This method is thread-safe. If <paramref name="isUnregisterToken"/> is <c>true</c>, the token will be removed from the vault and disposed after cancellation.
    /// </remarks>
    internal static bool CancelToken(in Guid tokenHandle, bool isUnregisterToken = true)
    {
        using (ThreadLock.EnterScope())
        {
            // Get token source ref from the vault. If it's already null/removed, return false.
            ref CancellationTokenSource tokenSource = ref CollectionsMarshal.GetValueRefOrNullRef(TokenVault, tokenHandle);
            if (Unsafe.IsNullRef(in tokenSource))
            {
                return false;
            }

            // Cancel the token source and if isUnregisterToken == true, then remove the token from the vault.
            tokenSource.Cancel();
            if (isUnregisterToken)
            {
                _ = UnregisterToken(in tokenHandle, out _);
            }
            return true;
        }
    }

    /// <summary>
    /// Cancel, Dispose and Unregister all the Guid Token of the respective cancel tokens.
    /// </summary>
    /// <remarks>
    /// USE THIS ONLY IF YOU'RE DONE OR ABOUT TO FREE THE PLUGIN.
    /// </remarks>
    internal static void DangerousCancelAndUnregisterAllToken()
    {
        using (ThreadLock.EnterScope())
        {
            // Cancel all the token sources in the vault and clear the vault.
            foreach (KeyValuePair<Guid, CancellationTokenSource> tokenSource in TokenVault)
            {
                SharedStatic.InstanceLogger?.LogTrace("[ComCancellationTokenVault::DangerousCancelAndUnregisterAllToken()] Disposing all async operations from cancel token: {Guid}", tokenSource.Key);

                tokenSource.Value.Cancel();
                tokenSource.Value.Dispose();
            }

            // Clear the vault
            TokenVault.Clear();
        }
    }
}
