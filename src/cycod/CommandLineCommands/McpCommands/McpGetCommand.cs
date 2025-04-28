using System;
using System.IO;
using System.Threading.Tasks;

/// <summary>
/// Command to view the details of a specific MCP (Model Context Protocol) Server.
/// </summary>
class McpGetCommand : McpBaseCommand
{
    /// <summary>
    /// The name of the MCP server to get.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Constructor initializes the base command.
    /// </summary>
    public McpGetCommand() : base()
    {
        Scope = ConfigFileScope.Any;
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
        return "mcp get";
    }

    /// <summary>
    /// Execute the get command.
    /// </summary>
    /// <param name="interactive">Whether the command is running in interactive mode.</param>
    /// <returns>Exit code, 0 for success, non-zero for failure.</returns>
    public override async Task<int> ExecuteAsync(bool interactive)
    {
        return await Task.Run(() => 
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                ConsoleHelpers.WriteErrorLine("Error: MCP server name is required.");
                return 1;
            }

            return ExecuteGet(Name, Scope ?? ConfigFileScope.Any);
        });
    }

    /// <summary>
    /// Execute the get command for the specified MCP server name and scope.
    /// </summary>
    /// <param name="name">The name of the MCP server to get.</param>
    /// <param name="scope">The scope to look in.</param>
    /// <returns>Exit code, 0 for success, non-zero for failure.</returns>
    private int ExecuteGet(string name, ConfigFileScope scope)
    {
        ConsoleHelpers.WriteDebugLine($"ExecuteGet; name: {name}; scope: {scope}");

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

        // Display the found server
        McpDisplayHelpers.DisplayMcpServer(name, configFile.FileName, configFile.Scope, serverConfig);
        return 0;
    }
}