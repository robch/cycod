using ModelContextProtocol.Client;

/// <summary>
/// Factory for creating MCP clients from configuration.
/// </summary>
public static class McpClientManager
{
    /// <summary>
    /// Creates an MCP client for the specified server name.
    /// </summary>
    /// <param name="serverName">The name of the configured MCP server.</param>
    /// <param name="scope">Optional scope to look in for the server.</param>
    /// <returns>An initialized MCP client or null if the server is not found.</returns>
    public static async Task<IMcpClient?> CreateClientAsync(string serverName, ConfigFileScope scope = ConfigFileScope.Any)
    {
        // Find the MCP server configuration
        var serverConfig = scope == ConfigFileScope.Any
            ? McpFileHelpers.GetFromAnyScope(serverName)
            : McpFileHelpers.GetFromScope(serverName, scope);

        if (serverConfig == null)
        {
            ConsoleHelpers.WriteErrorLine($"MCP server '{serverName}' not found");
            return null;
        }

        // Create the MCP client based on the transport type
        if (serverConfig is StdioServerConfig stdioConfig)
        {
            return await McpClientFactory.CreateAsync(new StdioClientTransport(new()
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

        return null;
    }

    /// <summary>
    /// Creates MCP clients for all configured servers.
    /// </summary>
    /// <param name="scope">Optional scope to look in for servers.</param>
    /// <returns>A dictionary of server names to MCP clients.</returns>
    public static async Task<Dictionary<string, IMcpClient>> CreateAllClientsAsync(ConfigFileScope scope = ConfigFileScope.Any)
    {
        var result = new Dictionary<string, IMcpClient>();
        var servers = new Dictionary<string, IMcpServerConfigItem>();

        // Get servers from all relevant scopes
        if (scope == ConfigFileScope.Any || scope == ConfigFileScope.Global)
        {
            foreach (var server in McpFileHelpers.ListMcpServers(ConfigFileScope.Global))
            {
                servers[server.Key] = server.Value;
            }
        }
        
        if (scope == ConfigFileScope.Any || scope == ConfigFileScope.User)
        {
            foreach (var server in McpFileHelpers.ListMcpServers(ConfigFileScope.User))
            {
                servers[server.Key] = server.Value;
            }
        }
        
        if (scope == ConfigFileScope.Any || scope == ConfigFileScope.Local)
        {
            foreach (var server in McpFileHelpers.ListMcpServers(ConfigFileScope.Local))
            {
                servers[server.Key] = server.Value;
            }
        }

        var start = DateTime.Now;
        ConsoleHelpers.Write($"Loading {servers.Count} MCP server(s) ...", ConsoleColor.DarkGray);

        // Create clients for each server
        var loaded = 0;
        foreach (var serverName in servers.Keys)
        {
            try
            {
                var client = await CreateClientAsync(serverName, scope);
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
        ConsoleHelpers.WriteLine($"\rLoaded {result.Count} MCP server(s) ({duration})", ConsoleColor.DarkGray);

        return result;
    }
}