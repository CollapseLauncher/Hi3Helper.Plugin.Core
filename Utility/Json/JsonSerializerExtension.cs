using System;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;

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

            not null => typeInfo != null
                ? JsonValue.Create(value, typeInfo)
                : throw new NotSupportedException(
                    $"Setting the value to JsonValue for Type \"{typeof(T).FullName}\" is not supported without providing a JsonTypeInfo<T> to the '{nameof(typeInfo)}' argument.")
        };
    }

    public static void SetConfigValueIfEmpty<T>(this JsonObject obj, string propertyName, T? value, JsonTypeInfo<T>? typeInfo = null)
    {
        if (!obj.ContainsKey(propertyName))
        {
            obj.SetConfigValue(propertyName, value, typeInfo);
        }
    }
}
