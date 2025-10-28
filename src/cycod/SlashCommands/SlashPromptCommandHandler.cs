/// <summary>
/// Handles slash commands by expanding them to prompt file contents.
/// Implements the clean ISlashCommandHandler interface.
/// </summary>
public class SlashPromptCommandHandler : ISlashCommandHandler
{
    /// <summary>
    /// Checks if this handler can process the given command.
    /// </summary>
    public bool CanHandle(string userPrompt)
    {
        if (string.IsNullOrWhiteSpace(userPrompt) || !userPrompt.StartsWith("/"))
        {
            return false;
        }

        var commandName = ExtractCommandName(userPrompt);
        var promptName = commandName.TrimStart('/');
        var promptFile = PromptFileHelpers.FindPromptFile(promptName);

        return promptFile != null;
    }
    
    /// <summary>
    /// Handles the prompt command synchronously and returns the expanded prompt text.
    /// </summary>
    public SlashCommandResult Handle(string userPrompt, FunctionCallingChat chat)
    {
        var command = ExtractCommandName(userPrompt);
        var arguments = ExtractArguments(userPrompt);
        // TODO: Use the arguments...
        
        var promptName = command.TrimStart('/');
        var promptText = PromptFileHelpers.GetPromptText(promptName);

        if (!string.IsNullOrWhiteSpace(promptText))
        {
            return SlashCommandResult.WithResponse(promptText);
        }

        return SlashCommandResult.NotHandled();
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
