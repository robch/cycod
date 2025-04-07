using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace chatx.FunctionCalling
{
    /// <summary>
    /// Extension of FunctionFactory that adds support for Model Context Protocol (MCP) tools.
    /// </summary>
    public class McpFunctionFactory : FunctionFactory
    {
        private readonly Dictionary<string, IMcpClient> _mcpClients = new();
        private readonly Dictionary<string, McpClientTool> _mcpTools = new();

        /// <summary>
        /// Creates a new McpFunctionFactory.
        /// </summary>
        public McpFunctionFactory() : base()
        {
        }

        /// <summary>
        /// Adds all tools from an MCP client as functions to this factory.
        /// </summary>
        /// <param name="mcpClient">The MCP client containing tools to add.</param>
        /// <param name="clientName">The name of the client, used as a prefix for tool names.</param>
        /// <returns>The task representing the asynchronous operation.</returns>
        public async Task AddMcpClientToolsAsync(IMcpClient mcpClient, string clientName)
        {
            if (!_mcpClients.ContainsKey(clientName))
            {
                _mcpClients[clientName] = mcpClient;
            }

            // Get the list of tools from the MCP client
            var tools = await mcpClient.ListToolsAsync();
            ConsoleHelpers.WriteDebugLine($"Found {tools.Count} tools on MCP server '{mcpClient.ServerInfo.Name}'");

            // Add each tool individually
            foreach (var tool in tools)
            {
                AddMcpClientTool(tool, clientName);
            }
        }

        /// <summary>
        /// Adds a specific MCP client tool to this factory.
        /// </summary>
        /// <param name="tool">The tool to add.</param>
        /// <param name="clientName">The name of the client, used as a prefix for tool names.</param>
        public void AddMcpClientTool(McpClientTool tool, string clientName)
        {
            string toolKey = $"{clientName}_{tool.Name}";
            _mcpTools[toolKey] = tool;

            ConsoleHelpers.WriteDebugLine($"Adding tool '{tool.Name}' from MCP server '{clientName}' as {toolKey}");
            base.AddFunction(tool);
        }
        
        /// <summary>
        /// Gets the list of AITools, including MCP tools.
        /// </summary>
        /// <returns>A collection of AITool instances.</returns>
        public new IEnumerable<AITool> GetAITools()
        {
            // Get base tools
            var baseTools = base.GetAITools();
            
            // Add MCP tools
            return baseTools.Concat(_mcpTools.Values);
        }
        
        /// <summary>
        /// Method that acts as a proxy for calling MCP tools
        /// </summary>
        /// <param name="toolName">The name of the tool to call</param>
        /// <param name="argumentsJson">The arguments as JSON</param>
        public async Task<string> CallMcpToolMethodAsync(string toolName, string argumentsJson)
        {
            try
            {
                // Find the tool
                if (_mcpTools.TryGetValue(toolName, out var tool))
                {
                    var arguments = JsonSerializer.Deserialize<Dictionary<string, object?>>(argumentsJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    arguments ??= new Dictionary<string, object?>();
                    
                    var clientName = toolName.Split('_')[0];
                    if (_mcpClients.TryGetValue(clientName, out var client))
                    {
                        var result = await client.CallToolAsync(tool.Name, arguments);
                        var asText = string.Join('\n', result.Content
                            .Where(c => !string.IsNullOrEmpty(c.Text))
                            .Select(c => c.Text));
                        return asText ?? "(no text response)";
                    }
                    
                    return $"Error: MCP client not found for tool {toolName}";
                }
                
                return $"Error: MCP tool {toolName} not found";
            }
            catch (Exception ex)
            {
                return $"Error calling MCP tool: {ex.Message}";
            }
        }

        /// <summary>
        /// Disposes all MCP clients when the factory is disposed.
        /// </summary>
        public async Task DisposeAsync()
        {
            foreach (var client in _mcpClients.Values)
            {
                if (client is IAsyncDisposable asyncDisposable)
                {
                    await asyncDisposable.DisposeAsync();
                }
                else if (client is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            
            _mcpClients.Clear();
            _mcpTools.Clear();
        }
    }
}