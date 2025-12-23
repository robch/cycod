using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace CycoDj.Models;

public class ChatMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = "";
    
    [JsonPropertyName("content")]
    public string Content { get; set; } = "";
    
    [JsonPropertyName("tool_calls")]
    public List<ToolCall>? ToolCalls { get; set; }
    
    [JsonPropertyName("tool_call_id")]
    public string? ToolCallId { get; set; }
}
