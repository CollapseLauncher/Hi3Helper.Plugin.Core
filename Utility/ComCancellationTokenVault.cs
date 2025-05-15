using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace Hi3Helper.Plugin.Core.Utility
{
    public static class ComCancellationTokenVault
    {
        private static readonly Dictionary<Guid, CancellationTokenSource> TokenVault = new();
        private static readonly Lock ThreadLock = new();

        internal static CancellationTokenSource RegisterToken(in Guid tokenHandle)
        {
            using (ThreadLock.EnterScope())
            {
                if (TokenVault.TryGetValue(tokenHandle, out CancellationTokenSource? tokenSource))
                {
                    return tokenSource;
                }

                TokenVault.Add(tokenHandle, tokenSource = new CancellationTokenSource());
                return tokenSource;
            }
        }

        internal static bool CancelToken(in Guid tokenHandle)
        {
            using (ThreadLock.EnterScope())
            {
                ref CancellationTokenSource tokenSource = ref CollectionsMarshal.GetValueRefOrNullRef(TokenVault, tokenHandle);
                if (Unsafe.IsNullRef(in tokenSource))
                {
                    return false;
                }

                tokenSource.Cancel();
                tokenSource.Dispose();
                TokenVault.Remove(tokenHandle);
                return true;
            }
        }
    }
}
