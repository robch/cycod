using ModelContextProtocol.Client;

/// <summary>
/// Factory for creating MCP clients from configuration.
/// </summary>
public static class McpClientManager
{
    /// <summary>
    /// Creates an MCP client based on the provided server configuration.
    /// </summary>
    /// <param name="serverName">The name of the MCP server</param>
    /// <param name="serverConfig">The configuration item for the MCP server</param>
    /// <returns>An instance of IMcpClient if created successfully, null otherwise</returns>
    public static async Task<IMcpClient?> CreateClientAsync(string serverName, IMcpServerConfigItem serverConfig)
    {
        IMcpClient? created = null;
        if (serverConfig is StdioServerConfig stdioConfig)
        {
            created = await McpClientFactory.CreateAsync(new StdioClientTransport(new()
            {
                Name = serverName,
                Command = stdioConfig.Command,
                Arguments = stdioConfig.Args,
                EnvironmentVariables = stdioConfig.Env,
            }));
        }
        else if (serverConfig is SseServerConfig sseConfig)
        {
            // TODO: Implement SSE transport
            throw new NotImplementedException("SSE transport is not yet implemented");
        }
        else
        {
            ConsoleHelpers.WriteErrorLine($"Unsupported transport type '{serverConfig.Type}' for MCP server '{serverName}'");
        }

        return created;
    }

    /// <summary>
    /// Creates MCP clients for all servers in the provided configuration.
    /// </summary>
    /// <param name="servers">Dictionary of server names to their configuration items</param>
    /// <returns>A dictionary mapping server names to their created MCP clients</returns>
    public static async Task<Dictionary<string, IMcpClient>> CreateClientsAsync(Dictionary<string, IMcpServerConfigItem> servers)
    {
        var start = DateTime.Now;
        ConsoleHelpers.Write($"Loading {servers.Count} registered MCP server(s) ...", ConsoleColor.DarkGray);

        var loaded = 0;
        var result = new Dictionary<string, IMcpClient>();
        foreach (var serverName in servers.Keys)
        {
            try
            {
                var client = await CreateClientAsync(serverName, servers[serverName]);
                if (client != null)
                {
                    result[serverName] = client;
                    loaded++;
                }
            }
            catch (Exception ex)
            {
                var message = "  " + ex.Message.Replace("\n", "\n  ").TrimEnd();
                ConsoleHelpers.WriteErrorLine($"\rSkipping MCP server '{serverName}'; failed to load\n\n{message}\n");
            }
        }

        var duration = TimeSpanFormatter.FormatMsOrSeconds(DateTime.Now - start);
        ConsoleHelpers.WriteLine($"\rLoaded {result.Count} registered MCP server(s) ({duration})", ConsoleColor.DarkGray);

        return result;
    }
}