using System;

/// <summary>
/// Base class for all tool-related commands.
/// </summary>
abstract class ToolBaseCommand : Command
{
    /// <summary>
    /// The scope to use for the tool operation.
    /// </summary>
    public ConfigFileScope? Scope { get; set; }

    /// <summary>
    /// Constructor initializes the base command.
    /// </summary>
    public ToolBaseCommand()
    {
        Scope = ConfigFileScope.Local;
    }

    /// <summary>
    /// Indicates if the command is empty.
    /// </summary>
    /// <returns>False, as tool commands are never empty.</returns>
    public override bool IsEmpty()
    {
        return false;
    }
}