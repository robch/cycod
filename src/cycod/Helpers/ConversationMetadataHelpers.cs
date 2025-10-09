using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;

/// <summary>
/// Utilities for handling conversation metadata.
/// </summary>
public static class ConversationMetadataHelpers
{
    /// <summary>
    /// JSON serialization options for consistent metadata formatting.
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Creates default metadata with current timestamp for both created and updated.
    /// </summary>
    /// <returns>New ConversationMetadata with current UTC timestamp</returns>
    public static ConversationMetadata CreateDefault()
    {
        var now = DateTime.UtcNow;
        return new ConversationMetadata
        {
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    /// <summary>
    /// Updates the UpdatedAt timestamp to current UTC time.
    /// </summary>
    /// <param name="metadata">Metadata to update</param>
    public static void UpdateTimestamp(ConversationMetadata metadata)
    {
        metadata.UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Serializes metadata to JSON format for JSONL storage.
    /// </summary>
    /// <param name="metadata">Metadata to serialize</param>
    /// <returns>JSON string in format: {"_meta":{...}}</returns>
    public static string SerializeMetadata(ConversationMetadata metadata)
    {
        var wrapper = new MetadataWrapper { _meta = metadata };
        return JsonSerializer.Serialize(wrapper, JsonOptions);
    }

    /// <summary>
    /// Attempts to parse metadata from the first line of a conversation file.
    /// </summary>
    /// <param name="firstLine">First line of JSONL file</param>
    /// <returns>Tuple of (metadata, messageStartIndex). If no metadata found, returns (null, 0)</returns>
    public static (ConversationMetadata? metadata, int messageStartIndex) TryParseMetadata(string firstLine)
    {
        if (string.IsNullOrWhiteSpace(firstLine) || !firstLine.TrimStart().StartsWith("{\"_meta\":"))
        {
            return (null, 0);
        }

        try
        {
            var wrapper = JsonSerializer.Deserialize<MetadataWrapper>(firstLine, JsonOptions);
            return (wrapper?._meta, 1);
        }
        catch (JsonException)
        {
            // Malformed metadata - treat first line as regular message
            return (null, 0);
        }
    }

    /// <summary>
    /// Wrapper class for clean JSON serialization of _meta object.
    /// </summary>
    private class MetadataWrapper
    {
        [JsonPropertyName("_meta")]
        public ConversationMetadata? _meta { get; set; }
    }
}