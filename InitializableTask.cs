using Hi3Helper.Plugin.Core.Utility;
using Microsoft.Extensions.Logging;
using System;
using System.Runtime.InteropServices.Marshalling;
using System.Threading;
using System.Threading.Tasks;

using static Hi3Helper.Plugin.Core.SharedStatic;

// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo

namespace Hi3Helper.Plugin.Core;

/// <summary>
/// The base class where its derived class must require asynchronous initialization before use.
/// </summary>
[GeneratedComClass]
public partial class InitializableTask : IInitializableTask
{
    public virtual nint InitAsync(in Guid cancelToken)
    {
        CancellationTokenSource tokenSource = ComCancellationTokenVault.RegisterToken(in cancelToken);
        CancellationToken token = tokenSource.Token;
        return InitAsync(token).AsResult();
    }

    /// <summary>
    /// Perform asynchronous initialization of the instance.
    /// </summary>
    /// <param name="token">A <see cref="CancellationToken"/> that can be used to cancel the initialization process.</param>
    /// <returns>
    /// A <see cref="Task{Int32}"/> representing the asynchronous operation.
    /// <para>
    /// Returns <c>69420</c> if the initialization completes successfully after a 10-second delay.
    /// Returns <see cref="int.MinValue"/> if the operation is canceled via the provided token.
    /// </para>
    /// </returns>
    /// <remarks>
    /// Note for Plugin Developers:<br/>
    /// This base implementation is intended as a placeholder and should be overridden in derived classes.
    /// It demonstrates async interop and cancellation support for plugin initialization.
    /// </remarks>
    protected virtual async Task<int> InitAsync(CancellationToken token)
    {
        InstanceLogger?.LogDebug("Hello World! from Initializable->InitAsync()!");
        InstanceLogger?.LogDebug("If you see this message, that means you need to override this method in your own Initializable member class.");
        InstanceLogger?.LogDebug("");
        InstanceLogger?.LogDebug("Moreover, this is a test method to ensure that the interop async is working as expected.");
        InstanceLogger?.LogDebug("Delaying for 10 seconds and you should've expected to get a return value of: 69420");
        InstanceLogger?.LogDebug("You can also try to cancel this method by passing the Guid Cancel token to IPlugin.CancelAsync() method.");
        try
        {
            await Task.Delay(10000, token);
            InstanceLogger?.LogDebug("Delay is done! Exiting method and returning: 69420...");
            return 69420;
        }
        catch (OperationCanceledException)
        {
            InstanceLogger?.LogError("Delay is cancelled! Exiting method and returning: {Value}...", int.MinValue);
            return int.MinValue;
        }
    }
}
