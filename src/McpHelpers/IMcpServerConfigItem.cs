using System.Text.Json.Serialization;
/// <summary>
/// Interface for MCP server configuration items
/// </summary>
public interface IMcpServerConfigItem
{
    /// <summary>
    /// The type of the MCP server (stdio or sse)
    /// </summary>
    string Type { get; }
    
    /// <summary>
    /// The config file containing this server configuration
    /// </summary>
    [JsonIgnore]
    McpConfigFile? ConfigFile { get; set; }
}
