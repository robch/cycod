/// <summary>
/// Routes slash commands to appropriate handlers using a clean fan-out pattern.
/// Eliminates the need for parallel handler tracking in ChatCommand.
/// </summary>
public class SlashCommandRouter
{
    private readonly List<ISlashCommandHandler> _handlers = new();
    
    /// <summary>
    /// Registers a slash command handler with the router.
    /// </summary>
    /// <typeparam name="T">The type of handler to register</typeparam>
    /// <param name="handler">The handler instance</param>
    public void Register<T>(T handler) where T : ISlashCommandHandler
    {
        _handlers.Add(handler);
    }
    
    /// <summary>
    /// Attempts to handle a slash command by trying each registered handler in order.
    /// </summary>
    /// <param name="userPrompt">The user's input prompt</param>
    /// <param name="chat">The current chat instance</param>
    /// <returns>The result of the first handler that can process the command, or NotHandled if none can</returns>
    public async Task<SlashCommandResult> HandleAsync(string userPrompt, FunctionCallingChat chat)
    {
        foreach (var handler in _handlers)
        {
            if (handler.CanHandle(userPrompt))
            {
                return await handler.HandleAsync(userPrompt, chat);
            }
        }
        
        return SlashCommandResult.NotHandled();
    }
}