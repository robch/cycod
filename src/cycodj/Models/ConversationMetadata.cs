using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace CycoDj.Models;

/// <summary>
/// Represents metadata for a conversation file.
/// </summary>
public class ConversationMetadata
{
    /// <summary>
    /// Human-readable conversation title.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// If true, AI should never regenerate title (user has manually edited it).
    /// </summary>
    [JsonPropertyName("titleLocked")]
    public bool IsTitleLocked { get; set; }

    /// <summary>
    /// Preserves unknown properties for future extensibility.
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, JsonElement> AdditionalProperties { get; set; } = new();
}

/// <summary>
/// Wrapper class for metadata serialization in JSONL format.
/// </summary>
internal class MetadataWrapper
{
    [JsonPropertyName("_meta")]
    public ConversationMetadata? _meta { get; set; }
}
