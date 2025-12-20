using System;
using System.Collections.Generic;

namespace CycoDj.Models;

public class Conversation
{
    public string Id { get; set; } = "";
    public string FilePath { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public ConversationMetadata? Metadata { get; set; }
    public List<ChatMessage> Messages { get; set; } = new();
    public List<string> ToolCallIds { get; set; } = new();
    
    // For branch detection
    public string? ParentId { get; set; }
    public List<string> BranchIds { get; set; } = new();
    
    /// <summary>
    /// Gets the display title, with fallback to ID if no title in metadata
    /// </summary>
    public string GetDisplayTitle()
    {
        if (!string.IsNullOrEmpty(Metadata?.Title))
        {
            return Metadata.Title;
        }
        return Id;
    }
}
