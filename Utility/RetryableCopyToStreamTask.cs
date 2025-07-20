using System;
using System.Buffers;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
// ReSharper disable IdentifierTypo

namespace Hi3Helper.Plugin.Core.Utility;

/// <summary>
/// Runs a retry-able task of Copy-to routine from Source to Target <see cref="Stream"/>.<br/>
/// This is suitable for Copy-to operation which involves a <see cref="Stream"/> from network related instances (for example: <see cref="HttpClient"/>) whether as source or target or both.
/// </summary>
public class RetryableCopyToStreamTask : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// A delegate which creates a source <see cref="Stream"/> based on the last position of the download.
    /// </summary>
    /// <param name="lastBytesPosition">The last position of the bytes downloaded.</param>
    /// <param name="token">The cancellation token for creating <see cref="Stream"/> instance.</param>
    /// <returns>A source <see cref="Stream"/> to copy from.</returns>
    public delegate Task<Stream> SourceStreamFactory(long lastBytesPosition, CancellationToken token = default);

    /// <summary>
    /// Gets how many bytes being read per cycle from source <see cref="Stream"/>.
    /// </summary>
    /// <param name="read">Length of bytes being read from source <see cref="Stream"/>.</param>
    public delegate void ReadDelegate(int read);

    private readonly RetryableCopyToStreamTaskOptions _options;
    private readonly SourceStreamFactory              _sourceStreamFactory;
    private          Stream?                          _sourceStream;
    private readonly Stream                           _targetStream;

    private int _disposed;

    public void Dispose()
    {
        // Ensure the _disposed state atomically to avoid invalid state due to race-condition.
        if (_disposed == 1 || Interlocked.Exchange(ref _disposed, 1) == 1)
        {
            return;
        }

        if (_options.IsDisposeTargetStream)
        {
            _targetStream.Dispose();
        }
        _sourceStream?.Dispose();

        GC.SuppressFinalize(this);
    }

    public async ValueTask DisposeAsync()
    {
        // Ensure the _disposed state atomically to avoid invalid state due to race-condition.
        if (_disposed == 1 || Interlocked.Exchange(ref _disposed, 1) == 1)
        {
            return;
        }

        if (_options.IsDisposeTargetStream)
        {
            await _targetStream.DisposeAsync();
        }

        if (_sourceStream != null)
        {
            await _sourceStream.DisposeAsync();
        }

        GC.SuppressFinalize(this);
    }

    private RetryableCopyToStreamTask(SourceStreamFactory sourceStreamFactory, Stream targetStream, RetryableCopyToStreamTaskOptions options)
    {
        _sourceStreamFactory = sourceStreamFactory;
        _targetStream        = targetStream;
        _options             = options;
    }

    /// <summary>
    /// Creates an instance of <see cref="RetryableCopyToStreamTask"/>.
    /// </summary>
    /// <param name="sourceStreamFactory">An asynchronous callback to create a source <see cref="Stream"/> to copy from.</param>
    /// <param name="targetStream">The stream in which the data will be copied to.</param>
    /// <param name="options">Sets of parameters given to run the task.</param>
    /// <returns>An instance of <see cref="RetryableCopyToStreamTask"/>.</returns>
    public static RetryableCopyToStreamTask CreateTask(
        SourceStreamFactory               sourceStreamFactory,
        Stream                            targetStream,
        RetryableCopyToStreamTaskOptions? options = null)
    {
        options ??= new RetryableCopyToStreamTaskOptions();
        return new RetryableCopyToStreamTask(sourceStreamFactory, targetStream, options);
    }

    /// <summary>
    /// Starts the Copy-to Task asynchronously.
    /// </summary>
    /// <param name="readDelegate">A delegate which gets how many bytes being read from the source <see cref="Stream"/>.</param>
    /// <param name="token">Cancellation token for the task.</param>
    /// <returns>An asynchronous task of the Copy-to routine.</returns>
    public ValueTask StartTaskAsync(ReadDelegate? readDelegate = null, CancellationToken token = default)
    {
        // Throw if the task instance is already disposed
        ObjectDisposedException.ThrowIf(_disposed == 1, this);

        // Create task
        return StartTaskAsyncCore(readDelegate, token);
    }

    private async ValueTask StartTaskAsyncCore(ReadDelegate? readDelegate, CancellationToken token)
    {
        int bufferLen = Math.Clamp(_options.MaxBufferSize, 1 << 10, 1 << 20);
        byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferLen);

        try
        {
            await WriteTaskCore(readDelegate, buffer, _options.MaxRetryCount, token);
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }

    private async ValueTask WriteTaskCore(ReadDelegate? readDelegate, byte[] buffer, int retryAttemptLeft, CancellationToken token)
    {
        long     lastBytesPosition = 0;
        TimeSpan timeoutSpan       = TimeSpan.FromSeconds(_options.MaxTimeoutSeconds < 2d ? 2d : _options.MaxTimeoutSeconds);

        while (retryAttemptLeft > 0)
        {
            var (timedOutCts, coopCts) = RenewTimeOutCancelToken(in timeoutSpan, in token);
            _sourceStream = await _sourceStreamFactory(lastBytesPosition, coopCts.Token);
            if (_sourceStream == null)
            {
                throw new NullReferenceException("Source stream cannot be null!");
            }

            try
            {
                int read;

                while ((read = await _sourceStream
                    .ReadAsync(buffer, coopCts.Token)
                    .ConfigureAwait(false)) > 0)
                {
                    lastBytesPosition += read;
                    readDelegate?.Invoke(read);
                    await _targetStream
                        .WriteAsync(buffer.AsMemory(0, read), coopCts.Token)
                        .ConfigureAwait(false);

                    // Both timedOutCts and coopCts required to be disposed before renewal to avoid memory leaks.
                    timedOutCts.Dispose();
                    coopCts.Dispose();

                    (timedOutCts, coopCts) = RenewTimeOutCancelToken(in timeoutSpan, in token);
                }

                return;
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                int lastAttemptLeft = Interlocked.Decrement(ref retryAttemptLeft);
                if (lastAttemptLeft == 0 ||
                    (ex is HttpRequestException asNetworkException &&
                     !IsRetryableHttpStatusCode(asNetworkException.StatusCode)))
                {
                    throw;
                }

                SharedStatic.InstanceLogger.LogError(ex, "An error has occurred while running copy-to task to target stream! (Retry attempt left: {LastAttempt}) Exception: {Exception}", lastAttemptLeft, ex);
                if (_options.RetryDelaySeconds > 0)
                {
                    await Task.Delay(TimeSpan.FromSeconds(_options.RetryDelaySeconds), token);
                }
            }
            finally
            {
                await _sourceStream.DisposeAsync();
                timedOutCts.Dispose();
                coopCts.Dispose();
            }
        }
    }

    private static bool IsRetryableHttpStatusCode(HttpStatusCode? statusCode)
    {
        if (statusCode == null)
        {
            return false;
        }

        bool isSuccessStatusCode = (int)statusCode is >= 200 and <= 299; // This replicates HttpResponseMessage.IsSuccessStatusCode behaviour
        bool isTimedOut = statusCode == HttpStatusCode.RequestTimeout;

        // Returns true if it's due to timeout.
        return isTimedOut ||
               // Otherwise, decide based on isSuccessStatusCode
               isSuccessStatusCode;
    }

    private static (CancellationTokenSource TimedOutCts, CancellationTokenSource CoopCts)
        RenewTimeOutCancelToken(in TimeSpan timeoutSpan, in CancellationToken innerToken)
    {
        CancellationTokenSource timedOutCts = new CancellationTokenSource(timeoutSpan);
        CancellationTokenSource coopCts     = CancellationTokenSource.CreateLinkedTokenSource(timedOutCts.Token, innerToken);

        return (timedOutCts, coopCts);
    }
}
