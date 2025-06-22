using System;
using System.IO;
// ReSharper disable IdentifierTypo

namespace Hi3Helper.Plugin.Core.Utility;

/// <summary>
/// Sets the parameters used for running a task from <see cref="RetryableCopyToStreamTask"/> instance.
/// </summary>
public class RetryableCopyToStreamTaskOptions
{
    /// <summary>
    /// The amount of max. buffer size to set for copy-to routine.
    /// </summary>
    /// <remarks>
    /// Default: 4096 bytes.<br/>
    /// This is important to set the max. buffer size NOT LESS THAN 1024 bytes and not too big than 1048576 bytes (1 MiB)
    /// </remarks>
    public int MaxBufferSize { get; init; } = 4 << 10;

    /// <summary>
    /// How many retry attempts for the copy-to routine.
    /// </summary>
    /// <remarks>
    /// Default: 5 attempts
    /// </remarks>
    public int MaxRetryCount { get; init; } = 5;

    /// <summary>
    /// How many seconds before the time-out happen on each read-write operations on copy-to routine.
    /// </summary>
    /// <remarks>
    /// Default: 10 seconds<br/>
    /// The value CANNOT BE LOWER than 2 seconds.
    /// </remarks>
    public double MaxTimeoutSeconds { get; init; } = 10d;

    /// <summary>
    /// How many seconds of the delay before resuming copy-to routine on each retry attempt.
    /// </summary>
    /// <remarks>
    /// Default: 1 second
    /// </remarks>
    public double RetryDelaySeconds { get; init; } = 1d;

    /// <summary>
    /// Whether to dispose the target <see cref="Stream"/> once <see cref="IDisposable.Dispose()"/> is being called.
    /// </summary>
    /// <remarks>
    /// Default: false
    /// </remarks>
    public bool IsDisposeTargetStream { get; init; }
}
