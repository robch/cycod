using System.Text.Json.Serialization;

namespace CycoDj.Models;

public class ToolFunction
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
    
    [JsonPropertyName("arguments")]
    public string Arguments { get; set; } = "";
}
