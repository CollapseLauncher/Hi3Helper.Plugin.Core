#if USELIGHTWEIGHTJSONPARSER
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Hi3Helper.Plugin.Core.Utility.Json;

public interface IJsonStreamSerializable<in T>
{
    static abstract Task SerializeToStreamAsync(T obj, Stream stream, bool isDisposeStream = false, JsonWriterOptions options = default, CancellationToken token = default);
}
#endif