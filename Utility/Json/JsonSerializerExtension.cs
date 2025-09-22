#if !USELIGHTWEIGHTJSONPARSER

using System;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Hi3Helper.Plugin.Core.Utility.Json;

public static class JsonSerializerExtension
{
    public static T? GetConfigValue<T>(this JsonObject? obj, string propertyName)
    {
        if (obj == null)
        {
            return default;
        }

        if (obj.TryGetPropertyValue(propertyName, out JsonNode? valueNode) &&
            valueNode != null)
        {
            return valueNode.GetValue<T>();
        }

        return default;
    }

    public static void SetConfigValue<T>(this JsonObject obj, string propertyName, T? value, JsonTypeInfo<T>? typeInfo = null)
    {
        obj[propertyName] = value switch
        {
            null => null,

            byte valueAsByte => JsonValue.Create(valueAsByte),
            sbyte valueAsSByte => JsonValue.Create(valueAsSByte),
            short valueAsShort => JsonValue.Create(valueAsShort),
            ushort valueAsUShort => JsonValue.Create(valueAsUShort),
            int valueAsInt => JsonValue.Create(valueAsInt),
            uint valueAsUInt => JsonValue.Create(valueAsUInt),
            long valueAsLong => JsonValue.Create(valueAsLong),
            ulong valueAsULong => JsonValue.Create(valueAsULong),

            float valueAsFloat => JsonValue.Create(valueAsFloat),
            double valueAsDouble => JsonValue.Create(valueAsDouble),
            decimal valueAsDecimal => JsonValue.Create(valueAsDecimal),

            bool valueAsBool => JsonValue.Create(valueAsBool),

            string valueAsString => JsonValue.Create(valueAsString),
            char valueAsChar => JsonValue.Create(valueAsChar),

            DateTime valueAsDateTime => JsonValue.Create(valueAsDateTime),
            DateTimeOffset valueAsDateTimeOffset => JsonValue.Create(valueAsDateTimeOffset),

            Guid valueAsGuid => JsonValue.Create(valueAsGuid),

#if !MANUALCOM
            not null => typeInfo != null
                ? JsonValue.Create(value, typeInfo)
                : throw new NotSupportedException(
                    $"Setting the value to JsonValue for Type \"{typeof(T).FullName}\" is not supported without providing a JsonTypeInfo<T> to the '{nameof(typeInfo)}' argument.")
#else
            not null => throw new NotSupportedException(
                    $"Setting the value to JsonValue for the type is not supported under Lightweight parser mode.")
#endif
        };
    }

    public static void SetConfigValueIfEmpty<T>(this JsonObject obj, string propertyName, T? value, JsonTypeInfo<T>? typeInfo = null)
    {
        if (!obj.ContainsKey(propertyName))
        {
            obj.SetConfigValue(propertyName, value, typeInfo);
        }
    }

    public static Task<T> GetApiResponseFromJsonAsync<T>(this HttpClient   client,
                                              string            url,
                                              JsonTypeInfo<T?>  typeInfo,
                                              CancellationToken token = default)
        => client.GetApiResponseFromJsonAsync(url, typeInfo, null, token);

    public static async Task<T> GetApiResponseFromJsonAsync<T>(this HttpClient   client,
                                                               string            url,
                                                               JsonTypeInfo<T?>  typeInfo,
                                                               HttpMethod?       httpMethod = null,
                                                               CancellationToken token      = default)
    {
        httpMethod ??= HttpMethod.Get;

        using HttpRequestMessage request = new(httpMethod, url);
        using HttpResponseMessage response =
                await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);

        response.EnsureSuccessStatusCode();

#if DEBUG
        string jsonResponse = await response.Content.ReadAsStringAsync(token);
        SharedStatic.InstanceLogger.LogTrace("API response for <T> ({TypeOf}): {JsonResponse}", typeof(T).Name, jsonResponse);
        return JsonSerializer.Deserialize(jsonResponse, typeInfo)
            ?? throw new NullReferenceException($"API response for <T> ({typeof(T).Name}) Returns null response!");
#else
        return await response.Content.ReadFromJsonAsync(typeInfo, token)
            ?? throw new NullReferenceException($"API response for <T> ({typeof(T).Name}) Returns null response!");
#endif
    }
}

#endif