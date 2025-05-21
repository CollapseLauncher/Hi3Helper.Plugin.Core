using System;

namespace Hi3Helper.Plugin.Core;

/// <summary>
/// A chained-able entry of the struct.
/// </summary>
public interface IChainedEntry : IDisposable
{
    /// <summary>
    /// The next entry of the struct. This should be non-null if multiple entries are available.
    /// </summary>
    nint NextEntry { get; set; }
}
