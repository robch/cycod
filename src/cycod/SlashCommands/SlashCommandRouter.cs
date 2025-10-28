/// <summary>
/// Routes slash commands to appropriate handlers using a clean fan-out pattern.
/// Supports both synchronous and asynchronous handlers naturally.
/// Eliminates the need for parallel handler tracking in ChatCommand.
/// </summary>
public class SlashCommandRouter
{
    private readonly List<ISlashCommandHandler> _syncHandlers = new();
    private readonly List<IAsyncSlashCommandHandler> _asyncHandlers = new();
    
    /// <summary>
    /// Registers a synchronous slash command handler with the router.
    /// </summary>
    /// <param name="handler">The sync handler instance</param>
    public void Register(ISlashCommandHandler handler)
    {
        _syncHandlers.Add(handler);
    }
    
    /// <summary>
    /// Registers an asynchronous slash command handler with the router.
    /// </summary>
    /// <param name="handler">The async handler instance</param>
    public void Register(IAsyncSlashCommandHandler handler)
    {
        _asyncHandlers.Add(handler);
    }
    
    /// <summary>
    /// Attempts to handle a slash command by trying each registered handler in order.
    /// Tries sync handlers first (faster), then async handlers.
    /// </summary>
    /// <param name="userPrompt">The user's input prompt</param>
    /// <param name="chat">The current chat instance</param>
    /// <returns>The result of the first handler that can process the command, or NotHandled if none can</returns>
    public async Task<SlashCommandResult> HandleAsync(string userPrompt, FunctionCallingChat chat)
    {
        // Try sync handlers first - faster for operations that don't need async
        foreach (var handler in _syncHandlers)
        {
            if (handler.CanHandle(userPrompt))
            {
                return handler.Handle(userPrompt, chat); // ← Direct sync call, no Task overhead
            }
        }
        
        // Try async handlers
        foreach (var handler in _asyncHandlers)
        {
            if (handler.CanHandle(userPrompt))
            {
                return await handler.HandleAsync(userPrompt, chat); // ← Natural async call
            }
        }
        
        return SlashCommandResult.NotHandled();
    }
    
    /// <summary>
    /// Gets the total number of registered handlers (for debugging/testing).
    /// </summary>
    public int HandlerCount => _syncHandlers.Count + _asyncHandlers.Count;
}