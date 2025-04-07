using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using System.Text.Json;

namespace chatx.FunctionCalling
{
    /// <summary>
    /// Extension of FunctionFactory that adds support for Model Context Protocol (MCP) tools.
    /// </summary>
    public class McpFunctionFactory : FunctionFactory
    {
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
            var newName = $"{clientName}_{tool.Name}";
            var withNewName = tool.WithName(newName);

            _mcpTools[newName] = withNewName;

            ConsoleHelpers.WriteDebugLine($"Adding tool '{tool.Name}' from MCP server '{clientName}' as {newName}");
            base.AddFunction(withNewName);
        }
        
        /// <summary>
        /// Overrides the TryCallFunction method to handle MCP client tools.
        /// </summary>
        /// <param name="functionName">The name of the function to call.</param>
        /// <param name="functionArguments">The arguments for the function call as a JSON string.</param>
        /// <param name="result">The result of the function call, if successful.</param>
        /// <returns>True if the function was found and called successfully, false otherwise.</returns>
        public override bool TryCallFunction(string functionName, string functionArguments, out string? result)
        {
            result = null;

            if (!string.IsNullOrEmpty(functionName) && _mcpTools.TryGetValue(functionName, out var tool))
            {
                ConsoleHelpers.WriteDebugLine($"Found MCP tool '{functionName}'");
                try
                {
                    var arguments = JsonSerializer.Deserialize<Dictionary<string, object?>>(functionArguments, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    arguments ??= new Dictionary<string, object?>();
                    
                    var clientName = functionName.Split('_')[0];
                    var toolName = functionName.Substring(clientName.Length + 1);
                    if (_mcpClients.TryGetValue(clientName, out var client))
                    {
                        ConsoleHelpers.WriteDebugLine($"Calling MCP tool '{toolName}' on client '{clientName}'");
                        var response = Task.Run(async () => await client.CallToolAsync(toolName, arguments)).Result;
                        ConsoleHelpers.WriteDebugLine(response.IsError
                            ? $"MCP tool '{toolName}' on `{clientName}` resulted in ERROR!"
                            : $"MCP tool '{toolName}' on `{clientName}` resulted in SUCCESS!");

                        result = string.Join('\n', response.Content
                            .Where(c => !string.IsNullOrEmpty(c.Text))
                            .Select(c => c.Text))
                            ?? "(no text response)";

                        ConsoleHelpers.WriteDebugLine($"MCP tool '{functionName}' returned: {result}");
                        return true;
                    }
                    
                    result = $"Error: MCP client not found for tool {functionName}";
                    return false;
                }
                catch (Exception ex)
                {
                    result = $"Error calling MCP tool: {ex.Message}";
                    return false;
                }
            }

            // If not an MCP tool, use the base implementation
            return base.TryCallFunction(functionName, functionArguments, out result);
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
 
        private readonly Dictionary<string, IMcpClient> _mcpClients = new();
        private readonly Dictionary<string, McpClientTool> _mcpTools = new();

   }
}