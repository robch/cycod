using System;
using System.Collections.Generic;

namespace CycoDj.Models;

public class Conversation
{
    public string Id { get; set; } = "";
    public string FilePath { get; set; } = "";
    public DateTime Timestamp { get; set; }
    public string? Title { get; set; }
    public List<ChatMessage> Messages { get; set; } = new();
    public List<string> ToolCallIds { get; set; } = new();
    
    // For branch detection
    public string? ParentId { get; set; }
    public List<string> BranchIds { get; set; } = new();
}
