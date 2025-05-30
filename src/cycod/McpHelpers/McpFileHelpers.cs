using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Provides methods for working with MCP (Model Context Protocol) server configuration files across different scopes.
/// </summary>
public static class McpFileHelpers
{
    /// <summary>
    /// Finds the MCP directory in the specified scope.
    /// </summary>
    /// <param name="scope">The scope to search in</param>
    /// <param name="create">Whether to create the directory if it doesn't exist</param>
    /// <returns>The path to the MCP directory, or null if not found</returns>
    public static string? FindMcpDirectoryInScope(ConfigFileScope scope, bool create = false)
    {
        return create
            ? ScopeFileHelpers.EnsureDirectoryInScope(McpConfigFile.DefaultFolderName, scope)
            : ScopeFileHelpers.FindDirectoryInScope(McpConfigFile.DefaultFolderName, scope);
    }

    /// <summary>
    /// Saves an MCP server configuration to a file in the specified scope.
    /// </summary>
    /// <param name="serverName">Name of the MCP server</param>
    /// <param name="command">Command to execute for the server (null for SSE servers)</param>
    /// <param name="args">Arguments to pass to the command</param>
    /// <param name="envVars">Environment variables for the server (key=value format)</param>
    /// <param name="transport">Transport type (stdio or sse)</param>
    /// <param name="url">URL for SSE servers (null for stdio servers)</param>
    /// <param name="scope">The scope to save the configuration to</param>
    /// <returns>Path to the saved configuration file</returns>
    public static string SaveMcpServer(
        string serverName,
        string? command,
        IEnumerable<string>? args,
        IEnumerable<string>? envVars,
        string transport = "stdio",
        string? url = null,
        ConfigFileScope scope = ConfigFileScope.Local)
    {
        // Get or create the config file for this scope
        var configFile = McpConfigFile.FromScope(scope) ?? McpConfigFile.Create(scope);

        // Create the server configuration
        if (transport.ToLower() == "sse")
        {
            configFile.Servers[serverName] = new SseServerConfig
            {
                Type = "sse",
                Url = url,
                ConfigFile = configFile
            };
        }
        else // stdio is default
        {
            var environmentDict = new Dictionary<string, string?>();
            if (envVars != null)
            {
                foreach (var env in envVars)
                {
                    var parts = env.Split('=', 2);
                    if (parts.Length == 2)
                    {
                        environmentDict[parts[0]] = parts[1];
                    }
                }
            }

            configFile.Servers[serverName] = new StdioServerConfig
            {
                Type = "stdio",
                Command = command ?? "",
                Args = args?.ToList() ?? new List<string>(),
                Env = environmentDict,
                ConfigFile = configFile
            };
        }

        // Save the config file
        configFile.Save();
        return configFile.FileName;
    }

    /// <summary>
    /// Gets a server configuration from any scope based on server name.
    /// </summary>
    /// <param name="serverName">The name of the MCP server to find</param>
    /// <returns>The server configuration if found, null otherwise</returns>
    public static IMcpServerConfigItem? GetFromAnyScope(string serverName)
    {
        // Search in priority order: Local, User, Global
        foreach (var scope in new[] { ConfigFileScope.Local, ConfigFileScope.User, ConfigFileScope.Global })
        {
            var serverConfig = GetFromScope(serverName, scope);
            if (serverConfig != null)
            {
                return serverConfig;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets a server configuration from a specific scope based on server name.
    /// </summary>
    /// <param name="serverName">The name of the MCP server to find</param>
    /// <param name="scope">The scope to search in</param>
    /// <returns>The server configuration if found, null otherwise</returns>
    public static IMcpServerConfigItem? GetFromScope(string serverName, ConfigFileScope scope)
    {
        var configFile = McpConfigFile.FromScope(scope);
        if (configFile == null || !configFile.Servers.TryGetValue(serverName, out var serverConfig))
        {
            return null;
        }

        return serverConfig;
    }

    /// <summary>
    /// Deletes an MCP server configuration.
    /// </summary>
    /// <param name="serverName">The name of the MCP server to delete</param>
    /// <param name="scope">The scope to delete from (Any to search all scopes)</param>
    /// <returns>True if the server was deleted, false otherwise</returns>
    public static bool DeleteMcpServer(string serverName, ConfigFileScope scope = ConfigFileScope.Any)
    {
        // Find the server configuration
        IMcpServerConfigItem? serverConfig = null;
        McpConfigFile? configFile = null;

        if (scope == ConfigFileScope.Any)
        {
            // Search in priority order
            foreach (var searchScope in new[] { ConfigFileScope.Local, ConfigFileScope.User, ConfigFileScope.Global })
            {
                configFile = McpConfigFile.FromScope(searchScope);
                if (configFile != null && configFile.Servers.TryGetValue(serverName, out serverConfig))
                {
                    break;
                }
            }
        }
        else
        {
            configFile = McpConfigFile.FromScope(scope);
            if (configFile != null)
            {
                configFile.Servers.TryGetValue(serverName, out serverConfig);
            }
        }

        if (configFile == null || serverConfig == null)
        {
            ConsoleHelpers.WriteDebugLine($"DeleteMcpServer: Server not found: {serverName}");
            return false;
        }

        // Remove the server from the config file
        if (configFile.Servers.Remove(serverName))
        {
            // Save the updated config file
            configFile.Save();
            return true;
        }

        return false;
    }

    /// <summary>
    /// Lists all MCP servers in a specified scope.
    /// </summary>
    /// <param name="scope">The scope to list servers from</param>
    /// <returns>A dictionary of server names to their configurations</returns>
    public static Dictionary<string, IMcpServerConfigItem> ListMcpServers(ConfigFileScope scope)
    {
        var servers = new Dictionary<string, IMcpServerConfigItem>();

        var addGlobal = scope == ConfigFileScope.Any || scope == ConfigFileScope.Global;
        if (addGlobal) AddServersFromScope(servers, ConfigFileScope.Global);

        var addUser = scope == ConfigFileScope.Any || scope == ConfigFileScope.User;
        if (addUser) AddServersFromScope(servers, ConfigFileScope.User);

        var addLocal = scope == ConfigFileScope.Any || scope == ConfigFileScope.Local;
        if (addLocal) AddServersFromScope(servers, ConfigFileScope.Local);

        return servers;
    }

    /// <summary>
    /// Adds servers from a specific scope to the provided dictionary.
    /// </summary>
    /// <param name="servers"></param>
    /// <param name="scope"></param>
    private static void AddServersFromScope(Dictionary<string, IMcpServerConfigItem> servers, ConfigFileScope scope)
    {
        var configFile = McpConfigFile.FromScope(scope);
        var serversFromScope = configFile?.Servers ?? new Dictionary<string, IMcpServerConfigItem>();

        foreach (var server in serversFromScope)
        {
            servers.TryAdd(server.Key, server.Value);
        }
    }
}
