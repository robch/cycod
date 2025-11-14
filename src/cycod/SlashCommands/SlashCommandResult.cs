/// <summary>
/// Result of executing a slash command.
/// Provides rich information about how the command was handled.
/// </summary>
public class SlashCommandResult
{
    /// <summary>
    /// Whether the command was handled by this handler.
    /// </summary>
    public bool Handled { get; init; }
    
    /// <summary>
    /// Text to pass to the assistant (for prompt expansion commands).
    /// </summary>
    public string? ResponseText { get; init; }
    
    /// <summary>
    /// Whether to skip the assistant response after handling this command.
    /// </summary>
    public bool SkipAssistant { get; init; }
    
    /// <summary>
    /// Whether the conversation needs to be saved after this command.
    /// </summary>
    public bool NeedsSave { get; init; }
    
    // Factory methods for common scenarios
    
    /// <summary>
    /// Command was not handled - should try next handler or pass to assistant.
    /// </summary>
    public static SlashCommandResult NotHandled() => new() { Handled = false };
    
    /// <summary>
    /// Command was handled and should pass response text to assistant.
    /// </summary>
    public static SlashCommandResult WithResponse(string responseText) => new() 
    { 
        Handled = true, 
        ResponseText = responseText, 
        SkipAssistant = false 
    };
    
    /// <summary>
    /// Command was handled successfully, skip assistant response.
    /// </summary>
    public static SlashCommandResult Success() => new() 
    { 
        Handled = true, 
        SkipAssistant = true 
    };
    
    /// <summary>
    /// Command was handled and modified conversation - save needed.
    /// </summary>
    public static SlashCommandResult HandledWithSave() => new() 
    { 
        Handled = true, 
        SkipAssistant = true, 
        NeedsSave = true 
    };
}