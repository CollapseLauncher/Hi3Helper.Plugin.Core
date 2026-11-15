#if USELIGHTWEIGHTJSONPARSER
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
// ReSharper disable UnusedMemberInSuper.Global

namespace Hi3Helper.Plugin.Core.Utility.Json;

public interface IJsonStreamParsable<T>
{
    static abstract T ParseFrom(Stream stream, bool isDisposeStream = false, JsonDocumentOptions options = default);
    static abstract Task<T> ParseFromAsync(Stream stream, bool isDisposeStream = false, JsonDocumentOptions options = default, CancellationToken token = default);
}
#endif