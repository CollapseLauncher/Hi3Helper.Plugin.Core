#if USELIGHTWEIGHTJSONPARSER
using System;
using System.Text.Json;

namespace Hi3Helper.Plugin.Core.Utility.Json;

public static class JsonElementExtension
{
    public static string? GetString(this JsonElement element, string propertyName)
    {
        if (!element.TryGetString(propertyName, out string? value))
        {
            throw new JsonException($"Cannot parse value of {propertyName} to string!");
        }

        return value;
    }

    public static string GetStringNonNullOrEmpty(this JsonElement element, string propertyName)
    {
        string? value = element.GetString(propertyName);
        if (string.IsNullOrEmpty(value))
        {
            throw new JsonException($"String value of {propertyName} is empty or null!");
        }

        return value;
    }

    public static byte[]? GetBytesFromBase64(this JsonElement element, string propertyName)
    {
        if (!element.TryGetBytesFromBase64(propertyName, out byte[]? value))
        {
            throw new JsonException($"Cannot parse value of {propertyName} to byte array!");
        }

        return value;
    }

    public static T GetValue<T>(this JsonElement element, string propertyName)
        where T : struct, ISpanParsable<T>
    {
        if (!element.TryGetValue(propertyName, out T value))
        {
            throw new JsonException($"Cannot parse value of {propertyName}!");
        }

        return value;
    }

    public static byte[] GetBytesFromBase64NonNull(this JsonElement element, string propertyName)
    {
        byte[]? value = element.GetBytesFromBase64(propertyName);
        if (value == null)
        {
            throw new JsonException($"Bytes array value of {propertyName} is null!");
        }

        return value;
    }

    public static bool TryGetString(this JsonElement element, string propertyName, out string? value)
    {
        if (!element.TryGetProperty(propertyName, out JsonElement valueElement))
        {
            value = null;
            return false;
        }

        value = valueElement.GetString();
        return true;
    }

    public static bool TryGetStringNonNullOrEmpty(this JsonElement element, string propertyName, out string value)
    {
        if (!element.TryGetString(propertyName, out string? valueOut) ||
            string.IsNullOrEmpty(valueOut))
        {
            value = "";
            return false;
        }

        value = valueOut;
        return true;
    }

    public static bool TryGetBytesFromBase64(this JsonElement element, string propertyName, out byte[]? bytes)
    {
        if (!element.TryGetProperty(propertyName, out JsonElement valueElement) ||
            !valueElement.TryGetBytesFromBase64(out byte[]? valueBytes))
        {
            bytes = null;
            return false;
        }

        bytes = valueBytes;
        return true;
    }

    public static bool TryGetBytesFromBase64NonNull(this JsonElement element, string propertyName, out byte[] bytes)
    {
        if (!element.TryGetBytesFromBase64(propertyName, out byte[]? valueBytes) ||
            valueBytes == null)
        {
            bytes = [];
            return false;
        }

        bytes = valueBytes;
        return true;
    }

    public static bool TryGetValue<T>(this JsonElement element, string propertyName, out T value)
        where T : struct, ISpanParsable<T>
    {
        if (element.TryGetProperty(propertyName, out JsonElement valueOutProp))
        {
            string? valueBacked = valueOutProp.ValueKind == JsonValueKind.Number ? valueOutProp.GetRawText() : valueOutProp.GetString();
            return T.TryParse(valueBacked, null, out value);
        }

        value = default;
        return false;
    }
}
#endif