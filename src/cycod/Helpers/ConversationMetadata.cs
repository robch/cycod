using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Represents metadata for a conversation file.
/// </summary>
public class ConversationMetadata
{
    /// <summary>
    /// UTC timestamp when the conversation was created.
    /// </summary>
    [JsonPropertyName("createdAt")]
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// UTC timestamp when the conversation was last modified.
    /// </summary>
    [JsonPropertyName("updatedAt")]
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Preserves unknown properties for future extensibility.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement> AdditionalProperties { get; set; } = new();
}