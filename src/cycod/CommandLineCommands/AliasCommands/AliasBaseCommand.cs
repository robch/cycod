using System;

/// <summary>
/// Base class for all alias-related commands.
/// </summary>
abstract class AliasBaseCommand : Command
{
    /// <summary>
    /// The scope to use for the alias operation.
    /// </summary>
    public ConfigFileScope? Scope { get; set; }

    /// <summary>
    /// Constructor initializes the base command.
    /// </summary>
    public AliasBaseCommand()
    {
    }

    /// <summary>
    /// Indicates if the command is empty.
    /// </summary>
    /// <returns>False, as alias commands are never empty.</returns>
    public override bool IsEmpty()
    {
        return false;
    }
}