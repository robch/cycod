using System.Text.Json;
using System.Text.Json.Serialization;

namespace CycoDj.Models;

public class ToolCall
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = "";
    
    [JsonPropertyName("function")]
    public ToolFunction? Function { get; set; }
}
