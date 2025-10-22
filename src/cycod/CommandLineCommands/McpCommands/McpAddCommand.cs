using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Command to add a new MCP (Model Context Protocol) Server.
/// </summary>
class McpAddCommand : McpBaseCommand
{
    /// <summary>
    /// The name of the MCP server to add.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The command to execute for the MCP server (for stdio transport).
    /// </summary>
    public string? Command { get; set; }
    
    /// <summary>
    /// The URL for the SSE endpoint (for sse transport).
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// The arguments to pass to the command.
    /// </summary>
    public List<string> Args { get; set; } = new List<string>();

    /// <summary>
    /// The transport type for the MCP server (stdio or sse).
    /// Defaults to "stdio" initially, but will be set to "sse" automatically if a URL is provided.
    /// </summary>
    public string? Transport { get; set; } = "stdio";

    /// <summary>
    /// Environment variables for the MCP server (in key=value format).
    /// </summary>
    public List<string> EnvironmentVars { get; set; } = new List<string>();

    /// <summary>
    /// Constructor initializes the base command.
    /// </summary>
    public McpAddCommand() : base()
    {
        Scope = ConfigFileScope.Local; // Default to local scope for adding
    }

    /// <summary>
    /// Checks if the command is empty.
    /// </summary>
    /// <returns>True if empty, false otherwise.</returns>
    public override bool IsEmpty()
    {
        var hasName = !string.IsNullOrWhiteSpace(Name);
        if (!hasName) return true;

        var isStdio = string.IsNullOrWhiteSpace(Transport) || Transport?.ToLower() == "stdio";
        if (isStdio && string.IsNullOrWhiteSpace(Command)) return true;
        
        var isSse = Transport?.ToLower() == "sse";
        if (isSse && string.IsNullOrWhiteSpace(Url)) return true;
        
        return false;
    }

    /// <summary>
    /// Gets the command name.
    /// </summary>
    /// <returns>The command name.</returns>
    public override string GetCommandName()
    {
        return "mcp add";
    }

    /// <summary>
    /// Execute the add command.
    /// Auto-sets transport to "sse" if URL is provided and transport isn't explicitly set.
    /// </summary>
    /// <param name="interactive">Whether the command is running in interactive mode.</param>
    /// <returns>Exit code, 0 for success, non-zero for failure.</returns>
    public override async Task<object> ExecuteAsync(bool interactive)
    {
        return await Task.Run(() => 
        {
            if (IsEmpty())
            {
                throw new InvalidOperationException("mcp add requires name and either command or url");
            }

            return ExecuteAdd(Name!, Command, Args, Transport, EnvironmentVars, Url, Scope ?? ConfigFileScope.Local);
        });
    }

    /// <summary>
    /// Execute the add command with the specified parameters.
    /// </summary>
    /// <param name="name">The name of the MCP server to add.</param>
    /// <param name="command">The command to execute for the server (for stdio).</param>
    /// <param name="args">Arguments to pass to the command.</param>
    /// <param name="transport">The transport type (stdio or sse).</param>
    /// <param name="envVars">Environment variables for the server.</param>
    /// <param name="url">The URL for the SSE endpoint (for sse).</param>
    /// <param name="scope">The scope to add the MCP server to.</param>
    /// <returns>Exit code, 0 for success, non-zero for failure.</returns>
    private int ExecuteAdd(string name, string? command, List<string> args, string? transport, List<string> envVars, string? url, ConfigFileScope scope)
    {
        try
        {
            var isStdio = string.IsNullOrWhiteSpace(transport) || transport?.ToLower() == "stdio";
            var savedFilePath = isStdio
                ? McpFileHelpers.SaveMcpServer(name, command, args, envVars, "stdio", scope: scope)
                : McpFileHelpers.SaveMcpServer(name, null, null, null, transport!, url: url, scope: scope);
            
            ConsoleHelpers.WriteLine($"Created MCP server '{name}' at {savedFilePath}.", overrideQuiet: true);
            return 0;
        }
        catch (Exception ex)
        {
            ConsoleHelpers.LogException(ex, "Error creating MCP server");
            return 1;
        }
    }
}
