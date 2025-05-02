using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Provides methods for displaying MCP (Model Context Protocol) server information.
/// </summary>
public static class McpDisplayHelpers
{
    /// <summary>
    /// Gets a display name for the location of the MCP servers in a scope.
    /// </summary>
    /// <param name="scope">The scope to get a display name for</param>
    /// <returns>A display name for the scope location</returns>
    public static string? GetLocationDisplayName(ConfigFileScope scope)
    {
        var mcpDir = McpFileHelpers.FindMcpDirectoryInScope(scope);
        if (mcpDir != null)
        {
            string mcpFilePath = Path.Combine(mcpDir, McpConfigFile.DefaultFileName);
            return CommonDisplayHelpers.FormatLocationHeader(mcpFilePath, scope);
        }
        
        var scopeDir = ConfigFileHelpers.GetScopeDirectoryPath(scope);
        if (scopeDir == null) return null;

        return CommonDisplayHelpers.FormatLocationHeader(Path.Combine(scopeDir, McpConfigFile.DefaultFolderName, McpConfigFile.DefaultFileName), scope);
    }

    /// <summary>
    /// Displays all MCP servers in a specific scope.
    /// </summary>
    /// <param name="scope">The scope to display servers from</param>
    public static void DisplayMcpServers(ConfigFileScope scope)
    {
        // Get the location display name
        var locationDisplay = GetLocationDisplayName(scope);
        if (locationDisplay == null) return;
        
        CommonDisplayHelpers.WriteLocationHeader(locationDisplay);

        // Find the MCP directory in the scope, and get the servers
        var mcpDir = McpFileHelpers.FindMcpDirectoryInScope(scope);
        var servers = mcpDir != null
            ? McpFileHelpers.ListMcpServers(scope)
            : new();

        if (servers.Count == 0)
        {
            Console.WriteLine($"  No MCP servers found in this scope.");
            return;
        }

        // Display each server
        foreach (var server in servers)
        {
            if (server.Value is StdioServerConfig stdioConfig)
            {
                Console.WriteLine($"  {server.Key} (stdio)");
                Console.WriteLine($"    Command: {stdioConfig.Command}");
                if (stdioConfig.Args.Count > 0)
                {
                    Console.WriteLine($"    Arguments: {string.Join(" ", stdioConfig.Args.Select(a => $"\"{a}\""))}");
                }
                if (stdioConfig.Env.Count > 0)
                {
                    Console.WriteLine($"    Environment variables: {stdioConfig.Env.Count}");
                }
            }
            else if (server.Value is SseServerConfig sseConfig)
            {
                Console.WriteLine($"  {server.Key} (sse)");
                Console.WriteLine($"    URL: {sseConfig.Url}");
            }
            Console.WriteLine();
        }
    }

    /// <summary>
    /// Displays detailed information about a specific MCP server.
    /// </summary>
    /// <param name="serverName">The name of the server to display</param>
    /// <param name="configPath">Path to the server configuration file</param>
    /// <param name="scope">The scope where the server was found</param>
    /// <param name="serverConfig">The server configuration</param>
    public static void DisplayMcpServer(string serverName, string configPath, ConfigFileScope scope, IMcpServerConfigItem serverConfig)
    {
        var location = CommonDisplayHelpers.FormatLocationHeader(configPath, scope);
        CommonDisplayHelpers.WriteLocationHeader(location);

        Console.WriteLine($"  {serverName} ({serverConfig.Type})");
        Console.WriteLine();

        if (serverConfig is StdioServerConfig stdioConfig)
        {
            Console.WriteLine($"    Command: {stdioConfig.Command}");
            
            if (stdioConfig.Args.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("    Arguments:");
                foreach (var arg in stdioConfig.Args)
                {
                    Console.WriteLine($"      {arg}");
                }
            }
            
            if (stdioConfig.Env.Count > 0)
            {
                Console.WriteLine();
                Console.WriteLine("    Environment Variables:");
                foreach (var env in stdioConfig.Env)
                {
                    // Mask sensitive values like API keys
                    var value = env.Key.Contains("key", StringComparison.OrdinalIgnoreCase) || 
                               env.Key.Contains("token", StringComparison.OrdinalIgnoreCase) || 
                               env.Key.Contains("secret", StringComparison.OrdinalIgnoreCase) 
                        ? "****" 
                        : env.Value;
                    
                    Console.WriteLine($"      {env.Key}={value}");
                }
            }
        }
        else if (serverConfig is SseServerConfig sseConfig)
        {
            Console.WriteLine($"    URL: {sseConfig.Url}");
        }
    }
}