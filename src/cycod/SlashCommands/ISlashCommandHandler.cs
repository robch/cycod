/// <summary>
/// Standard interface for all slash command handlers.
/// Provides a clean, consistent way to handle slash commands.
/// </summary>
public interface ISlashCommandHandler
{
    /// <summary>
    /// Checks if this handler can process the given command.
    /// </summary>
    /// <param name="userPrompt">The user's input prompt</param>
    /// <returns>True if this handler can process the command</returns>
    bool CanHandle(string userPrompt);
    
    /// <summary>
    /// Handles the command asynchronously and returns the result.
    /// </summary>
    /// <param name="userPrompt">The user's input prompt</param>
    /// <param name="chat">The current chat instance</param>
    /// <returns>The result of executing the command</returns>
    Task<SlashCommandResult> HandleAsync(string userPrompt, FunctionCallingChat chat);
}