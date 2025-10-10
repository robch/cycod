/// <summary>
/// Base class for slash commands that support subcommands.
/// Provides universal argument parsing and subcommand routing.
/// </summary>
public abstract class SlashCommandBase
{
    protected Dictionary<string, Func<string[], FunctionCallingChat, bool>> _subcommands = new();
    
    /// <summary>
    /// The name of the command (without the leading slash).
    /// </summary>
    public abstract string CommandName { get; }
    
    /// <summary>
    /// Attempts to handle a slash command with subcommand support.
    /// </summary>
    /// <param name="userPrompt">The user's input prompt</param>
    /// <param name="chat">The current chat instance</param>
    /// <returns>True if the command was handled, false if it should be passed to the assistant</returns>
    public bool TryHandle(string userPrompt, FunctionCallingChat chat)
    {
        if (!userPrompt.StartsWith($"/{CommandName}")) return false;
        
        var args = ParseArgs(userPrompt.Substring($"/{CommandName}".Length).Trim());
        
        if (args.Length == 0)
        {
            return HandleDefault(chat);
        }
        
        var subcommand = args[0].ToLowerInvariant();
        if (_subcommands.TryGetValue(subcommand, out var handler))
        {
            return handler(args.Skip(1).ToArray(), chat);
        }
        
        // Unknown subcommand - pass to assistant (existing pattern)
        return false;
    }
    
    /// <summary>
    /// Handles the case when no subcommand is provided.
    /// </summary>
    /// <param name="chat">The current chat instance</param>
    /// <returns>True if handled, false to pass to assistant</returns>
    protected abstract bool HandleDefault(FunctionCallingChat chat);
    
    /// <summary>
    /// Parses the argument string into an array of arguments.
    /// Can be overridden for custom parsing logic.
    /// </summary>
    /// <param name="input">The input string to parse</param>
    /// <returns>Array of parsed arguments</returns>
    protected virtual string[] ParseArgs(string input)
    {
        return input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
    }
    
    /// <summary>
    /// Shows help information for this command and its subcommands.
    /// </summary>
    /// <param name="metadata">Current conversation metadata for context</param>
    protected abstract void ShowHelp(ConversationMetadata? metadata);
}