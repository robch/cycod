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
}