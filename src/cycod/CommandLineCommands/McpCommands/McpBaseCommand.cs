using System;

/// <summary>
/// Base class for all MCP (Model Context Protocol) related commands.
/// </summary>
abstract class McpBaseCommand : Command
{
    /// <summary>
    /// The scope to use for the MCP operation.
    /// </summary>
    public ConfigFileScope? Scope { get; set; }

    /// <summary>
    /// Constructor initializes the base command.
    /// </summary>
    public McpBaseCommand()
    {
    }

    /// <summary>
    /// Indicates if the command is empty.
    /// </summary>
    /// <returns>False, as MCP commands are never empty.</returns>
    public override bool IsEmpty()
    {
        return false;
    }
}