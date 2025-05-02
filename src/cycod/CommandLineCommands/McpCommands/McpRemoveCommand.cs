using System;
using System.IO;
using System.Threading.Tasks;

/// <summary>
/// Command to remove an MCP (Model Context Protocol) Server.
/// </summary>
class McpRemoveCommand : McpBaseCommand
{
    /// <summary>
    /// The name of the MCP server to remove.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Constructor initializes the base command.
    /// </summary>
    public McpRemoveCommand() : base()
    {
        Scope = ConfigFileScope.Any; // Default to any scope for removal search
    }

    /// <summary>
    /// Checks if the command is empty (i.e., no server name provided).
    /// </summary>
    /// <returns>True if empty, false otherwise.</returns>
    public override bool IsEmpty()
    {
        return string.IsNullOrWhiteSpace(Name);
    }

    /// <summary>
    /// Gets the command name.
    /// </summary>
    /// <returns>The command name.</returns>
    public override string GetCommandName()
    {
        return "mcp remove";
    }

    /// <summary>
    /// Execute the remove command.
    /// </summary>
    /// <param name="interactive">Whether the command is running in interactive mode.</param>
    /// <returns>Exit code, 0 for success, non-zero for failure.</returns>
    public override async Task<object> ExecuteAsync(bool interactive)
    {
        return await Task.Run(() => 
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                ConsoleHelpers.WriteErrorLine("Error: MCP server name is required.");
                return 1;
            }

            return ExecuteRemove(Name, Scope ?? ConfigFileScope.Any);
        });
    }

    /// <summary>
    /// Execute the remove command for the specified MCP server name and scope.
    /// </summary>
    /// <param name="name">The name of the MCP server to remove.</param>
    /// <param name="scope">The scope to look in.</param>
    /// <returns>Exit code, 0 for success, non-zero for failure.</returns>
    private int ExecuteRemove(string name, ConfigFileScope scope)
    {
        ConsoleHelpers.WriteDebugLine($"ExecuteRemove; name: {name}; scope: {scope}");

        var isAnyScope = scope == ConfigFileScope.Any;
        var serverConfig = isAnyScope
            ? McpFileHelpers.GetFromAnyScope(name)
            : McpFileHelpers.GetFromScope(name, scope);
        
        if (serverConfig == null)
        {
            ConsoleHelpers.WriteErrorLine(isAnyScope
                ? $"Error: MCP server '{name}' not found in any scope."
                : $"Error: MCP server '{name}' not found in {scope} scope.");
            return 1;
        }

        var configFile = serverConfig.ConfigFile;
        if (configFile == null)
        {
            ConsoleHelpers.WriteErrorLine($"Error: MCP server '{name}' found but its config file is not available.");
            return 1;
        }

        var deleteFromScope = configFile.Scope;
        var deleted = McpFileHelpers.DeleteMcpServer(name, deleteFromScope);
        if (!deleted)
        {
            ConsoleHelpers.WriteErrorLine($"Error: Failed to delete MCP server '{name}'.");
            return 1;
        }

        Console.WriteLine($"Removed MCP server '{name}' from {deleteFromScope} scope.");
        return 0;
    }
}