using ModelContextProtocol.Client;
using System.Diagnostics;

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
        Logger.Info($"MCP: Creating client for server '{serverName}' of type '{serverConfig.Type}'");
        
        IMcpClient? created = null;
        var sw = Stopwatch.StartNew();
        try
        {
            if (serverConfig is StdioServerConfig stdioConfig)
            {
                ConsoleHelpers.WriteDebugLine($"MCP: Creating stdio client for '{serverName}' with command: {stdioConfig.Command}");
                
                created = await McpClientFactory.CreateAsync(new StdioClientTransport(new()
                {
                    Name = serverName,
                    Command = stdioConfig.Command,
                    Arguments = stdioConfig.Args,
                    EnvironmentVariables = stdioConfig.Env,
                }));
                
                ConsoleHelpers.WriteDebugLine($"MCP: Successfully created stdio client for '{serverName}'");
            }
            else if (serverConfig is SseServerConfig sseConfig)
            {
                ConsoleHelpers.WriteDebugLine($"MCP: Creating SSE client for '{serverName}' with endpoint: {sseConfig.Url}");
                
                created = await McpClientFactory.CreateAsync(new SseClientTransport(new()
                {
                    Name = serverName,
                    Endpoint = new Uri(sseConfig.Url!),
                    UseStreamableHttp = true
                }));
                
                ConsoleHelpers.WriteDebugLine($"MCP: Successfully created SSE client for '{serverName}'");
            }
            else
            {
                var errorMsg = $"Unsupported transport type '{serverConfig.Type}' for MCP server '{serverName}'";
                ConsoleHelpers.WriteErrorLine(errorMsg);
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"MCP: Failed to create client for '{serverName}': {ex.Message}\n{ex.StackTrace}");
            throw; // Rethrow to be handled by caller
        }
        finally
        {
            sw.Stop();
            Logger.Verbose($"MCP: Client creation for '{serverName}' took {sw.ElapsedMilliseconds}ms");
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
        Logger.Info($"MCP: Loading {servers.Count} registered MCP server(s)");

        var loaded = 0;
        var skipped = 0;
        var result = new Dictionary<string, IMcpClient>();
        foreach (var serverName in servers.Keys)
        {
            try
            {
                ConsoleHelpers.WriteDebugLine($"MCP: Attempting to create client for '{serverName}'");
                var client = await CreateClientAsync(serverName, servers[serverName]);
                if (client != null)
                {
                    result[serverName] = client;
                    loaded++;
                    Logger.Info($"MCP: Successfully loaded MCP server '{serverName}' ({client.ServerInfo.Version})");
                }
            }
            catch (Exception ex)
            {
                skipped++;
                ConsoleHelpers.LogException(ex, $"Skipping MCP server '{serverName}'; failed to load", showToUser: true);
            }
        }

        var duration = TimeSpanFormatter.FormatMsOrSeconds(DateTime.Now - start);
        var statusMsg = $"Loaded {result.Count} registered MCP server(s) ({duration})";
        ConsoleHelpers.WriteLine($"\r{statusMsg}", ConsoleColor.DarkGray);
        Logger.Info($"MCP: {statusMsg}, skipped {skipped} servers");

        return result;
    }
}