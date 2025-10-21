using static ConsoleHelpers;

/// <summary>
/// Result of processing a slash command.
/// </summary>
public enum SlashCommandResult
{
    /// <summary>
    /// Command was handled successfully, no further action needed.
    /// </summary>
    Handled,

    /// <summary>
    /// Command was not recognized, should be passed to the assistant.
    /// </summary>
    PassToAssistant,

    /// <summary>
    /// Command was handled and modified conversation metadata - save is needed.
    /// </summary>
    NeedsSave
}

/// <summary>
/// Base class for slash commands that support subcommands.
/// Provides universal argument parsing and subcommand routing.
/// </summary>
public abstract class SlashCommandBase
{
    protected Dictionary<string, Func<string[], FunctionCallingChat, SlashCommandResult>> _subcommands = new();
    
    /// <summary>
    /// The name of the command (without the leading slash).
    /// </summary>
    public abstract string CommandName { get; }
    
    /// <summary>
    /// Attempts to handle a slash command with subcommand support.
    /// </summary>
    /// <param name="userPrompt">The user's input prompt</param>
    /// <param name="chat">The current chat instance</param>
    /// <param name="result">The result of processing the command</param>
    /// <returns>True if the command was handled, false if it should be passed to the assistant</returns>
    public bool TryHandle(string userPrompt, FunctionCallingChat chat, out SlashCommandResult result)
    {
        if (!userPrompt.StartsWith($"/{CommandName}"))
        {
            result = SlashCommandResult.PassToAssistant;
            return false;
        }
        
        var args = ParseArgs(userPrompt.Substring($"/{CommandName}".Length).Trim());
        
        if (args.Length == 0)
        {
            result = HandleDefault(chat);
            return true;
        }
        
        var subcommand = args[0].ToLowerInvariant();
        if (_subcommands.TryGetValue(subcommand, out var handler))
        {
            result = handler(args.Skip(1).ToArray(), chat);
            return true;
        }
        
        // Unknown subcommand - pass to assistant (existing pattern)
        ConsoleHelpers.WriteLine($"Unknown subcommand '{subcommand}' for /{CommandName}.\n", ConsoleColor.Yellow);
        result = SlashCommandResult.Handled;
        return true;
    }
    
    /// <summary>
    /// Handles the case when no subcommand is provided.
    /// </summary>
    /// <param name="chat">The current chat instance</param>
    /// <returns>Result of handling the default command</returns>
    protected abstract SlashCommandResult HandleDefault(FunctionCallingChat chat);
    
    /// <summary>
    /// Parses the argument string into an array of arguments.
    /// Handles quoted strings as single arguments and removes outer quotes.
    /// </summary>
    /// <param name="input">The input string to parse</param>
    /// <returns>Array of parsed arguments</returns>
    protected virtual string[] ParseArgs(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return Array.Empty<string>();
            
        var args = new List<string>();
        var i = 0;
        
        while (i < input.Length)
        {
            // Skip whitespace
            while (i < input.Length && char.IsWhiteSpace(input[i]))
                i++;
                
            if (i >= input.Length)
                break;
                
            // Check if this argument starts with a quote
            if (input[i] == '"')
            {
                // Find the closing quote
                i++; // Skip opening quote
                var start = i;
                while (i < input.Length && input[i] != '"')
                    i++;
                    
                if (i < input.Length)
                {
                    // Found closing quote - extract the content between quotes
                    args.Add(input.Substring(start, i - start));
                    i++; // Skip closing quote
                }
                else
                {
                    // No closing quote found - treat as regular text from the opening quote
                    args.Add(input.Substring(start - 1));
                    break;
                }
            }
            else
            {
                // Regular argument - collect until whitespace
                var start = i;
                while (i < input.Length && !char.IsWhiteSpace(input[i]))
                    i++;
                args.Add(input.Substring(start, i - start));
            }
        }
        
        return args.ToArray();
    }
    
    /// <summary>
    /// Shows help information for this command and its subcommands.
    /// </summary>
    /// <param name="metadata">Current conversation metadata for context</param>
    protected abstract void ShowHelp(ConversationMetadata? metadata);
}