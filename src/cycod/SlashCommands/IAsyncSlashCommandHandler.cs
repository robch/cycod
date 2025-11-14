/// <summary>
/// Interface for asynchronous slash command handlers.
/// Use this for commands that perform async operations (network calls, process execution).
/// </summary>
public interface IAsyncSlashCommandHandler
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