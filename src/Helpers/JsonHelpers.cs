using System.Text;
using System.Text.Json;

public static class JsonHelpers
{
    public static string? GetJsonPropertyValue(string json, string propertyName, string? valueIfNotFound = null)
    {
        if (string.IsNullOrEmpty(json)) return valueIfNotFound;

        try
        {
            var jsonElement = JsonSerializer.Deserialize<JsonElement>(json);
            
            // Check if the JSON is an object and has the specified property
            if (jsonElement.ValueKind == JsonValueKind.Object && 
                jsonElement.TryGetProperty(propertyName, out JsonElement propertyElement))
            {
                // Return the string value or default if not a string
                return propertyElement.ValueKind == JsonValueKind.String 
                    ? propertyElement.GetString() 
                    : valueIfNotFound;
            }
        }
        catch
        {
            // Silently catch any exceptions (invalid JSON, etc.)
        }

        return valueIfNotFound;
    }

    public static List<Dictionary<string, string>> FromJsonArrayText(string json)
    {
        var list = new List<Dictionary<string, string>>();
        using var doc = JsonDocument.Parse(json);
        foreach (var item in doc.RootElement.EnumerateArray())
        {
            var properties = new Dictionary<string, string>();
            foreach (var property in item.EnumerateObject())
            {
                properties.Add(property.Name, property.Value.GetString() ?? property.Value.GetRawText());
            }
            list.Add(properties);
        }
        return list;
    }

    public static string GetJsonArrayText(List<Dictionary<string, string>> list)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions{ Indented = false });

        WriteJsonArray(writer, list);

        writer.Flush();
        return Encoding.UTF8.GetString(stream.ToArray());
    }

    private static void WriteJsonArray(Utf8JsonWriter writer, List<Dictionary<string, string>> items)
    {
        writer.WriteStartArray();
        foreach (var item in items.Where(x => x != null).ToList())
        {
            WriteJsonObject(writer, item);
        }
        writer.WriteEndArray();
    }

    private static void WriteJsonObject(Utf8JsonWriter writer, Dictionary<string, string> properties)
    {
        writer.WriteStartObject();
        foreach (var key in properties.Keys)
        {
            WritePropertyJsonOrString(writer, key, properties[key]);
        }
        writer.WriteEndObject();
    }

    private static void WritePropertyJsonOrString(Utf8JsonWriter writer, string key, string value)
    {
        if (key.EndsWith(".json"))
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                writer.WritePropertyName(key);
                writer.WriteRawValue(value);
            }
        }
        else
        {
            writer.WriteString(key, value);
        }
    }

    private static void WriteJsonOrStringValue(Utf8JsonWriter writer, string key, string value)
    {
        if (key.EndsWith(".json"))
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                writer.WriteRawValue(value);
            }
        }
        else
        {
            writer.WriteStringValue(value);
        }
    }

    private static void WriteJsonObject(Utf8JsonWriter writer, Dictionary<string, List<string>> properties)
    {
        writer.WriteStartObject();
        foreach (var key in properties.Keys)
        {
            var values = properties[key].Where(x => !string.IsNullOrEmpty(x));
            if (values.Count() == 1)
            {
                WritePropertyJsonOrString(writer, key, values.First());
                continue;
            }

            writer.WriteStartArray(key);
            foreach (var item in values)
            {
                WriteJsonOrStringValue(writer, key, item);
            }
            writer.WriteEndArray();
        }
        writer.WriteEndObject();
    }
}