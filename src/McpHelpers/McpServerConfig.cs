
/// <summary>
/// Root configuration class for MCP server JSON files (maintained for serialization compatibility)
/// </summary>
public class McpServerConfig
{
    /// <summary>
    /// Dictionary of server name to server configuration
    /// </summary>
    public Dictionary<string, IMcpServerConfigItem> McpServers { get; set; } = new Dictionary<string, IMcpServerConfigItem>();
}
