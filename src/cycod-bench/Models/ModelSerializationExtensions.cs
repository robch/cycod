using Newtonsoft.Json;

namespace CycodBench.Models;

/// <summary>
/// Extension methods for model serialization and deserialization.
/// </summary>
public static class ModelSerializationExtensions
{
    private static readonly JsonSerializerSettings SerializerSettings = new()
    {
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Ignore,
    };

    /// <summary>
    /// Serializes an object to a JSON string.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <returns>A JSON string representation of the object.</returns>
    public static string ToJson<T>(this T obj) where T : class
    {
        return JsonConvert.SerializeObject(obj, SerializerSettings);
    }

    /// <summary>
    /// Deserializes a JSON string to an object of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of object to deserialize to.</typeparam>
    /// <param name="json">The JSON string to deserialize.</param>
    /// <returns>An object of the specified type.</returns>
    public static T? FromJson<T>(this string json) where T : class
    {
        return JsonConvert.DeserializeObject<T>(json, SerializerSettings);
    }

    /// <summary>
    /// Serializes an object to a JSON file.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <param name="filePath">The path to the file to write to.</param>
    public static void ToJsonFile<T>(this T obj, string filePath) where T : class
    {
        FileHelpers.WriteAllText(filePath, obj.ToJson());
    }

    /// <summary>
    /// Deserializes a JSON file to an object of the specified type.
    /// </summary>
    /// <typeparam name="T">The type of object to deserialize to.</typeparam>
    /// <param name="filePath">The path to the file to read from.</param>
    /// <returns>An object of the specified type.</returns>
    public static T? FromJsonFile<T>(this string filePath) where T : class
    {
        if (!File.Exists(filePath))
        {
            return null;
        }
        
        string json = File.ReadAllText(filePath);
        return json.FromJson<T>();
    }

    /// <summary>
    /// Serializes an object to a line in a JSONL file (appending).
    /// </summary>
    /// <typeparam name="T">The type of object to serialize.</typeparam>
    /// <param name="obj">The object to serialize.</param>
    /// <param name="filePath">The path to the file to append to.</param>
    public static void AppendToJsonlFile<T>(this T obj, string filePath) where T : class
    {
        var dir = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
        
        // Use compact formatting for JSONL
        var line = JsonConvert.SerializeObject(obj, new JsonSerializerSettings
        {
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Ignore,
        });
        
        File.AppendAllLines(filePath, new[] { line });
    }
    
    /// <summary>
    /// Reads a JSONL file and returns a list of deserialized objects.
    /// </summary>
    /// <typeparam name="T">The type of objects to deserialize to.</typeparam>
    /// <param name="filePath">The path to the JSONL file.</param>
    /// <returns>A list of objects of the specified type.</returns>
    public static List<T> ReadJsonlFile<T>(this string filePath) where T : class
    {
        if (!File.Exists(filePath))
        {
            return new List<T>();
        }
        
        var lines = File.ReadAllLines(filePath);
        var result = new List<T>();
        
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line)) continue;
            try
            {
                var obj = line.FromJson<T>();
                if (obj != null)
                {
                    result.Add(obj);
                }
            }
            catch (JsonException)
            {
                // Skip invalid lines
            }
        }
        
        return result;
    }
}