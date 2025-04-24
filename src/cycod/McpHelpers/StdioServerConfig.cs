using System.Text.Json.Serialization;
/// <summary>
/// Configuration for stdio-based MCP servers
/// </summary>
public class StdioServerConfig : IMcpServerConfigItem
{
    /// <summary>
    /// The type of the MCP server (always "stdio")
    /// </summary>
    public string Type { get; set; } = "stdio";
    
    /// <summary>
    /// The command to execute for the server
    /// </summary>
    public string Command { get; set; } = "";
    
    /// <summary>
    /// Arguments to pass to the command
    /// </summary>
    public List<string> Args { get; set; } = new List<string>();
    
    /// <summary>
    /// Environment variables for the server
    /// </summary>
    public Dictionary<string, string> Env { get; set; } = new Dictionary<string, string>();
    
    /// <summary>
    /// The config file containing this server configuration
    /// </summary>
    [JsonIgnore]
    public McpConfigFile? ConfigFile { get; set; }
}
