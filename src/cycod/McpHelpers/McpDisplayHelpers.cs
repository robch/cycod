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
            ConsoleHelpers.WriteLine($"  No MCP servers found in this scope.", overrideQuiet: true);
            return;
        }

        // Display each server
        foreach (var server in servers)
        {
            if (server.Value is StdioServerConfig stdioConfig)
            {
                ConsoleHelpers.WriteLine($"  {server.Key} (stdio)", overrideQuiet: true);
                ConsoleHelpers.WriteLine($"    Command: {stdioConfig.Command}", overrideQuiet: true);
                if (stdioConfig.Args.Count > 0)
                {
                    ConsoleHelpers.WriteLine($"    Arguments: {string.Join(" ", stdioConfig.Args.Select(a => $"\"{a}\""))}", overrideQuiet: true);
                }
                if (stdioConfig.Env.Count > 0)
                {
                    ConsoleHelpers.WriteLine($"    Environment variables: {stdioConfig.Env.Count}", overrideQuiet: true);
                }
            }
            else if (server.Value is SseServerConfig sseConfig)
            {
                ConsoleHelpers.WriteLine($"  {server.Key} (sse)", overrideQuiet: true);
                ConsoleHelpers.WriteLine($"    URL: {sseConfig.Url}", overrideQuiet: true);
            }
            ConsoleHelpers.WriteLine(overrideQuiet: true);
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

        ConsoleHelpers.WriteLine($"  {serverName} ({serverConfig.Type})", overrideQuiet: true);
        ConsoleHelpers.WriteLine(overrideQuiet: true);

        if (serverConfig is StdioServerConfig stdioConfig)
        {
            ConsoleHelpers.WriteLine($"    Command: {stdioConfig.Command}", overrideQuiet: true);
            
            if (stdioConfig.Args.Count > 0)
            {
                ConsoleHelpers.WriteLine(overrideQuiet: true);
                ConsoleHelpers.WriteLine("    Arguments:", overrideQuiet: true);
                foreach (var arg in stdioConfig.Args)
                {
                    ConsoleHelpers.WriteLine($"      {arg}", overrideQuiet: true);
                }
            }
            
            if (stdioConfig.Env.Count > 0)
            {
                ConsoleHelpers.WriteLine(overrideQuiet: true);
                ConsoleHelpers.WriteLine("    Environment Variables:", overrideQuiet: true);
                foreach (var env in stdioConfig.Env)
                {
                    // Mask sensitive values like API keys
                    var value = env.Key.Contains("key", StringComparison.OrdinalIgnoreCase) || 
                               env.Key.Contains("token", StringComparison.OrdinalIgnoreCase) || 
                               env.Key.Contains("secret", StringComparison.OrdinalIgnoreCase) 
                        ? "****" 
                        : env.Value;
                    
                    ConsoleHelpers.WriteLine($"      {env.Key}={value}", overrideQuiet: true);
                }
            }
        }
        else if (serverConfig is SseServerConfig sseConfig)
        {
            ConsoleHelpers.WriteLine($"    URL: {sseConfig.Url}", overrideQuiet: true);
        }
    }
}