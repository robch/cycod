using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using System.Text.Json;

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
        Logger.Info($"MCP: Adding tools from client '{clientName}' ({mcpClient.ServerInfo.Version})");
        
        if (!_mcpClients.ContainsKey(clientName))
        {
            _mcpClients[clientName] = mcpClient;
            Logger.Verbose($"MCP: Added client '{clientName}' to function factory");
        }

        // Get the list of tools from the MCP client
        var tools = await mcpClient.ListToolsAsync();
        var toolNames = string.Join(", ", tools.Select(t => t.Name));
        
        ConsoleHelpers.WriteDebugLine($"Found {tools.Count} tools on MCP server '{mcpClient.ServerInfo.Name}'");
        Logger.Info($"MCP: Found {tools.Count} tools on server '{clientName}': {toolNames}");

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
    public override bool TryCallFunction(string functionName, string functionArguments, out object? result)
    {
        result = null;

        if (!string.IsNullOrEmpty(functionName) && _mcpTools.TryGetValue(functionName, out var tool))
        {
            ConsoleHelpers.WriteDebugLine($"Found MCP tool '{functionName}'");
            Logger.Verbose($"MCP: Preparing to call tool '{functionName}'");
            
            try
            {
                var arguments = JsonSerializer.Deserialize<Dictionary<string, object?>>(functionArguments, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                arguments ??= new Dictionary<string, object?>();
                
                var clientName = functionName.Split('_')[0];
                var toolName = functionName.Substring(clientName.Length + 1);
                
                // Log the argument values (but mask potential sensitive data)
                var safeArgs = new Dictionary<string, object?>(arguments);
                foreach (var key in safeArgs.Keys.ToList())
                {
                    if (key.Contains("key", StringComparison.OrdinalIgnoreCase) || 
                        key.Contains("token", StringComparison.OrdinalIgnoreCase) ||
                        key.Contains("secret", StringComparison.OrdinalIgnoreCase) ||
                        key.Contains("password", StringComparison.OrdinalIgnoreCase))
                    {
                        safeArgs[key] = "********";
                    }
                }
                var argsJson = JsonSerializer.Serialize(safeArgs);
                
                if (_mcpClients.TryGetValue(clientName, out var client))
                {
                    // Log the tool call at Info level
                    Logger.Info($"MCP: Calling tool '{toolName}' on server '{clientName}' with args: {argsJson}");
                    ConsoleHelpers.WriteDebugLine($"Calling MCP tool '{toolName}' on client '{clientName}'");
                    
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    var response = Task.Run(async () => await client.CallToolAsync(toolName, arguments)).Result;
                    sw.Stop();
                    
                    var status = response.IsError ? "ERROR" : "SUCCESS";
                    ConsoleHelpers.WriteDebugLine($"MCP tool '{toolName}' on `{clientName}` resulted in {status}!");
                    
                    // Log at appropriate level based on success/failure
                    if (response.IsError)
                    {
                        Logger.Warning($"MCP: Tool '{toolName}' on server '{clientName}' failed after {sw.ElapsedMilliseconds}ms");
                    }
                    else
                    {
                        Logger.Info($"MCP: Tool '{toolName}' on server '{clientName}' completed successfully in {sw.ElapsedMilliseconds}ms");
                    }

                    result = string.Join('\n', response.Content
                        .Where(c => !string.IsNullOrEmpty(c.Text))
                        .Select(c => c.Text))
                        ?? "(no text response)";

                    // Log a short version of the result (truncate if too long)
                    var resultString = result.ToString() ?? "";
                    var truncatedResult = resultString.Length > 500 
                        ? resultString.Substring(0, 500) + "..." 
                        : resultString;
                    
                    Logger.Verbose($"MCP: Tool '{toolName}' returned: {truncatedResult}");
                    ConsoleHelpers.WriteDebugLine($"MCP tool '{functionName}' returned: {result}");
                    return true;
                }
                
                var errorMsg = $"Error: MCP client not found for tool {functionName}";
                Logger.Warning($"MCP: {errorMsg}");
                result = errorMsg;
                return false;
            }
            catch (Exception ex)
            {
                // Log detailed exception but return a simpler message as the function result
                ConsoleHelpers.LogException(ex, $"Error calling MCP tool '{functionName}'", showToUser: false);
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
        Logger.Info($"MCP: Disposing of {_mcpClients.Count} MCP clients");
        
        foreach (var entry in _mcpClients)
        {
            var clientName = entry.Key;
            var client = entry.Value;
            
            try
            {
                Logger.Verbose($"MCP: Disposing client '{clientName}'");
                
                if (client is IAsyncDisposable asyncDisposable)
                {
                    await asyncDisposable.DisposeAsync();
                    Logger.Verbose($"MCP: Async disposed client '{clientName}'");
                }
                else if (client is IDisposable disposable)
                {
                    disposable.Dispose();
                    Logger.Verbose($"MCP: Disposed client '{clientName}'");
                }
            }
            catch (Exception ex)
            {
                Logger.Warning($"MCP: Error disposing client '{clientName}': {ex.Message}");
            }
        }
        
        _mcpClients.Clear();
        _mcpTools.Clear();
        
        Logger.Info("MCP: All clients disposed and cleared");
    }

    private readonly Dictionary<string, IMcpClient> _mcpClients = new();
    private readonly Dictionary<string, McpClientTool> _mcpTools = new();

}