#if USELIGHTWEIGHTJSONPARSER
using System.Text.Json;

namespace Hi3Helper.Plugin.Core.Utility.Json;

public interface IJsonElementParsable<out T>
{
    static abstract T? ParseFrom(JsonElement element);
}
#endif
