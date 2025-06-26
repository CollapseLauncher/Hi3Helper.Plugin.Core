#if USELIGHTWEIGHTJSONPARSER
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Hi3Helper.Plugin.Core.Utility.Json;

public interface IJsonWriterSerializable<in T>
{
    static abstract Task SerializeToWriterAsync(T obj, Utf8JsonWriter writer, CancellationToken token = default);
}
#endif