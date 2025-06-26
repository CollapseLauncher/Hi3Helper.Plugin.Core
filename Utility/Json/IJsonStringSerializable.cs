#if USELIGHTWEIGHTJSONPARSER
using System.Text.Json;

namespace Hi3Helper.Plugin.Core.Utility.Json;

public interface IJsonStringSerializable<in T>
{
    static abstract string SerializeToString(T obj, JsonWriterOptions options = default);
}
#endif