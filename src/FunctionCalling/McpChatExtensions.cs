using chatx.McpHelpers;
using ModelContextProtocol.Client;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace chatx.FunctionCalling
{
    /// <summary>
    /// Extension methods for working with MCP in the ChatCommand
    /// </summary>
    public static class McpChatExtensions
    {
        /// <summary>
        /// Adds an MCP server to the chat by creating an MCP client and adding its tools to the function factory
        /// </summary>
        /// <param name="factory">The function factory to extend with MCP tools</param>
        /// <param name="serverName">The name of the MCP server</param>
        /// <param name="scope">The scope to look for the MCP server</param>
        /// <returns>True if the MCP server was successfully added, false otherwise</returns>
        public static async Task<bool> AddMcpServerAsync(this McpFunctionFactory factory, string serverName, ConfigFileScope scope = ConfigFileScope.Any)
        {
            // Create an MCP client for the server
            var client = await McpClientManager.CreateClientAsync(serverName, scope);
            if (client == null)
            {
                return false;
            }

            // Add all tools from the client to the factory
            await factory.AddMcpClientToolsAsync(client, serverName);
            return true;
        }

        /// <summary>
        /// Adds all configured MCP servers to the chat by creating MCP clients and adding their tools to the function factory
        /// </summary>
        /// <param name="factory">The function factory to extend with MCP tools</param>
        /// <param name="scope">The scope to look for MCP servers</param>
        /// <returns>A dictionary of server names to boolean values indicating if the server was successfully added</returns>
        public static async Task<Dictionary<string, bool>> AddAllMcpServersAsync(this McpFunctionFactory factory, ConfigFileScope scope = ConfigFileScope.Any)
        {
            var results = new Dictionary<string, bool>();
            
            // Get all MCP servers from all scopes
            var servers = new Dictionary<string, IMcpServerConfigItem>();
            
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
            
            // Add each server to the factory
            foreach (var serverName in servers.Keys)
            {
                results[serverName] = await AddMcpServerAsync(factory, serverName, scope);
            }
            
            return results;
        }
    }
}