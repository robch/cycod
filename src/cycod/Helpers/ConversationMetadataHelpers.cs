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
    /// Creates default empty metadata ready for future properties.
    /// </summary>
    /// <returns>New empty ConversationMetadata</returns>
    public static ConversationMetadata CreateDefault()
    {
        return new ConversationMetadata();
    }

    /// <summary>
    /// Sets a user-provided title and locks it from AI regeneration.
    /// </summary>
    /// <param name="metadata">Metadata to update</param>
    /// <param name="title">User-provided title</param>
    public static void SetUserTitle(ConversationMetadata metadata, string title)
    {
        metadata.Title = title?.Trim();
        metadata.TitleLocked = true;
    }

    /// <summary>
    /// Sets an AI-generated title without locking it.
    /// </summary>
    /// <param name="metadata">Metadata to update</param>
    /// <param name="title">AI-generated title</param>
    public static void SetGeneratedTitle(ConversationMetadata metadata, string title)
    {
        if (!metadata.TitleLocked) // Only set if not locked by user
        {
            metadata.Title = title?.Trim();
            // TitleLocked remains false
        }
    }

    /// <summary>
    /// Gets display-friendly title with fallback to filename.
    /// </summary>
    /// <param name="metadata">Conversation metadata</param>
    /// <param name="filePath">File path for fallback title generation</param>
    /// <returns>Display title</returns>
    public static string GetDisplayTitle(ConversationMetadata? metadata, string filePath)
    {
        // Use metadata title if available
        if (!string.IsNullOrEmpty(metadata?.Title))
        {
            return metadata.Title;
        }

        // Extract from filename: "chat-history-1234567890.jsonl" → "conversation-1234567890"
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        if (fileName.StartsWith("chat-history-"))
        {
            var timestamp = fileName.Substring("chat-history-".Length);
            return $"conversation-{timestamp}";
        }

        // Ultimate fallback
        return "Untitled Conversation";
    }

    /// <summary>
    /// Determines if a title should be generated for this conversation.
    /// </summary>
    /// <param name="metadata">Conversation metadata</param>
    /// <returns>True if title generation is needed</returns>
    public static bool ShouldGenerateTitle(ConversationMetadata? metadata)
    {
        return metadata != null && 
               string.IsNullOrEmpty(metadata.Title) && 
               !metadata.TitleLocked;
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