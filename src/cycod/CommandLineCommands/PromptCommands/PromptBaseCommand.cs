using System;

/// <summary>
/// Base class for all prompt-related commands.
/// </summary>
abstract class PromptBaseCommand : Command
{
    /// <summary>
    /// The scope to use for the prompt operation.
    /// </summary>
    public ConfigFileScope? Scope { get; set; }

    /// <summary>
    /// Constructor initializes the base command.
    /// </summary>
    public PromptBaseCommand()
    {
    }

    /// <summary>
    /// Indicates if the command is empty.
    /// </summary>
    /// <returns>False, as prompt commands are never empty.</returns>
    public override bool IsEmpty()
    {
        return false;
    }
}