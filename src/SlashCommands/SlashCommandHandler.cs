using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Handles slash commands by translating them to MDX commands
/// </summary>
public class SlashCommandHandler
{
    public SlashCommandHandler()
    {
        _mdxWrapper = new MdxCliWrapper();
        
        // Initialize command handlers
        _commandHandlers = new Dictionary<string, Func<string, Task<string>>>
        {
            // File searching commands - all map to basic MDX
            { "/files", HandleMdxPassthroughCommand },
            { "/file", HandleMdxPassthroughCommand },
            { "/find", HandleMdxPassthroughCommand },
            
            // Web commands
            { "/search", HandleWebSearchCommand },
            { "/get", HandleWebGetCommand },
            
            // Run command
            { "/run", HandleRunCommand }
        };
    }

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
        return _commandHandlers.ContainsKey(commandName);
    }
    
    /// <summary>
    /// Extracts the command name from the user prompt
    /// </summary>
    /// <param name="userPrompt">The user's input</param>
    /// <returns>The command name</returns>
    public string GetCommandName(string userPrompt)
    {
        return ExtractCommandName(userPrompt);
    }

    /// <summary>
    /// Executes a slash command and returns the result
    /// </summary>
    /// <param name="userPrompt">The user's input</param>
    /// <returns>The result of executing the command</returns>
    public async Task<string> HandleCommand(string userPrompt)
    {
        var command = ExtractCommandName(userPrompt);
        var arguments = ExtractArguments(userPrompt);
        
        if (_commandHandlers.TryGetValue(command, out var handler))
        {
            try
            {
                return await handler(arguments);
            }
            catch (Exception ex)
            {
                return $"Error executing slash command: {ex.Message}";
            }
        }
        
        return $"Unknown command: {command}";
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

    /// <summary>
    /// Handles commands that are passed directly to MDX without modification
    /// </summary>
    private async Task<string> HandleMdxPassthroughCommand(string arguments)
    {
        return await _mdxWrapper.ExecuteMdxCommandAsync(arguments);
    }

    /// <summary>
    /// Handles the /search command by translating to MDX web search
    /// </summary>
    private async Task<string> HandleWebSearchCommand(string arguments)
    {
        return await _mdxWrapper.ExecuteMdxCommandAsync($"web search {arguments}");
    }
    
    /// <summary>
    /// Handles the /get command by translating to MDX web get
    /// </summary>
    private async Task<string> HandleWebGetCommand(string arguments)
    {
        return await _mdxWrapper.ExecuteMdxCommandAsync($"web get {arguments}");
    }
    
    /// <summary>
    /// Handles the /run command by translating to MDX run
    /// </summary>
    private async Task<string> HandleRunCommand(string arguments)
    {
        return await _mdxWrapper.ExecuteMdxCommandAsync($"run {arguments}");
    }

    private readonly MdxCliWrapper _mdxWrapper;
    private readonly Dictionary<string, Func<string, Task<string>>> _commandHandlers;
}