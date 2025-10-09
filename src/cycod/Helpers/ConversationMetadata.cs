using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary>
/// Represents metadata for a conversation file.
/// </summary>
public class ConversationMetadata
{
    /// <summary>
    /// Preserves unknown properties for future extensibility.
    /// When we add title, description, capabilities, etc., they'll go here as proper properties.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement> AdditionalProperties { get; set; } = new();
}