using System.Text.Json.Serialization;
/// <summary>
/// Configuration for SSE-based MCP servers
/// </summary>
public class SseServerConfig : IMcpServerConfigItem
{
    /// <summary>
    /// The type of the MCP server (always "sse")
    /// </summary>
    public string Type { get; set; } = "sse";
    
    /// <summary>
    /// The URL for the SSE endpoint
    /// </summary>
    public string? Url { get; set; }
    
    /// <summary>
    /// The config file containing this server configuration
    /// </summary>
    [JsonIgnore]
    public McpConfigFile? ConfigFile { get; set; }
}
