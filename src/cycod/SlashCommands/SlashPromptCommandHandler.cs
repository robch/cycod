/// <summary>
/// Handles slash commands by translating them to MDX commands
/// </summary>
public class SlashPromptCommandHandler
{
    /// <summary>
    /// Checks if the handler can process a given command
    /// </summary>
    /// <param name="commandWithArgs">The user's input</param>
    /// <returns>True if the command can be handled</returns>
    public bool IsCommand(string commandWithArgs)
    {
        if (string.IsNullOrWhiteSpace(commandWithArgs) || !commandWithArgs.StartsWith("/"))
        {
            return false;
        }

        var commandName = ExtractCommandName(commandWithArgs);
        var promptName = commandName.TrimStart('/');
        var promptFile = PromptFileHelpers.FindPromptFile(promptName);

        var found = promptFile != null;
        return found;
    }
    
    /// <summary>
    /// Executes a slash command and returns the result
    /// </summary>
    /// <param name="userPrompt">The user's input</param>
    /// <returns>The result of executing the command</returns>
    public string? HandleCommand(string userPrompt)
    {
        var command = ExtractCommandName(userPrompt);
        var arguments = ExtractArguments(userPrompt);
        // TODO: Use the arguments...
        
        var promptName = command.TrimStart('/');
        var promptText = PromptFileHelpers.GetPromptText(promptName);

        var found = !string.IsNullOrWhiteSpace(promptText);
        if (found) return promptText!;

        return null;
    }

    /// <summary>
    /// Extracts the command name from a user prompt
    /// </summary>
    private string ExtractCommandName(string userPrompt)
    {
        var parts = userPrompt.Split(new[] { ' ' }, 2);
        return parts[0].ToLowerInvariant();
    }
    
    /// <summary>
    /// Extracts the arguments from a user prompt
    /// </summary>
    private string ExtractArguments(string userPrompt)
    {
        var parts = userPrompt.Split(new[] { ' ' }, 2);
        return parts.Length > 1 ? parts[1] : string.Empty;
    }
}
