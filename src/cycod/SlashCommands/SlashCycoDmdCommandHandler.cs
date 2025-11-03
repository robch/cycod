using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Handles slash commands by translating them to CYCODMD commands.
/// Implements the async interface since it executes external processes.
/// </summary>
public class SlashCycoDmdCommandHandler : IAsyncSlashCommandHandler
{
    public SlashCycoDmdCommandHandler(ChatCommand chatCommand)
    {
        _chatCommand = chatCommand;
        _cycoDmdWrapper = new CycoDmdCliWrapper();
        
        // Initialize command handlers
        _commandHandlers = new Dictionary<string, Func<string, Task<string>>>
        {
            // File searching commands - all map to basic CYCODMD
            { "/files", HandleCycoDmdPassthroughCommand },
            { "/file", HandleCycoDmdPassthroughCommand },
            { "/find", HandleCycoDmdPassthroughCommand },
            
            // Web commands
            { "/search", HandleWebSearchCommand },
            { "/get", HandleWebGetCommand },
            
            // Run command
            { "/run", HandleRunCommand },
            
            // Image command
            { "/image", HandleImageCommand }
        };
    }

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
    /// Handles the CycoDmd command asynchronously and adds the result to the conversation.
    /// </summary>
    public async Task<SlashCommandResult> HandleAsync(string userPrompt, FunctionCallingChat chat)
    {
        var command = ExtractCommandName(userPrompt);
        var arguments = ExtractArguments(userPrompt);
        
        // First check built-in commands
        if (_commandHandlers.TryGetValue(command, out var handler))
        {
            // Display function start (like original master branch)
            ConsoleHelpers.DisplayUserFunctionCall(command, null);
            
            try
            {
                var result = await handler(arguments);
                
                // Display function result (like original master branch)  
                ConsoleHelpers.DisplayUserFunctionCall(command, result ?? string.Empty);
                
                // Add result to conversation and skip assistant response
                chat.Conversation.AddUserMessage(result ?? string.Empty);
                return SlashCommandResult.Success();
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error executing slash command: {ex.Message}";
                
                // Display error result
                ConsoleHelpers.DisplayUserFunctionCall(command, errorMessage);
                
                chat.Conversation.AddUserMessage(errorMessage);
                return SlashCommandResult.Success();
            }
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

    /// <summary>
    /// Handles commands that are passed directly to CYCODMD without modification
    /// </summary>
    private async Task<string> HandleCycoDmdPassthroughCommand(string arguments)
    {
        return await _cycoDmdWrapper.ExecuteCycoDmdCommandAsync(arguments);
    }

    /// <summary>
    /// Handles the /search command by translating to CYCODMD web search
    /// </summary>
    private async Task<string> HandleWebSearchCommand(string arguments)
    {
        return await _cycoDmdWrapper.ExecuteCycoDmdCommandAsync($"web search {arguments}");
    }
    
    /// <summary>
    /// Handles the /get command by translating to CYCODMD web get
    /// </summary>
    private async Task<string> HandleWebGetCommand(string arguments)
    {
        return await _cycoDmdWrapper.ExecuteCycoDmdCommandAsync($"web get {arguments}");
    }
    
    /// <summary>
    /// Handles the /run command by translating to CYCODMD run
    /// </summary>
    private async Task<string> HandleRunCommand(string arguments)
    {
        var noInputProvided = string.IsNullOrWhiteSpace(arguments);
        if (noInputProvided) return "/run requires a command to run.";

        arguments = arguments.Trim();

        var shell = string.Empty;
        if (arguments.StartsWith("--bash"))
        {
            shell = "bash";
            arguments = arguments.Substring("--bash".Length).Trim();
        }
        else if (arguments.StartsWith("--cmd"))
        {
            shell = "cmd";
            arguments = arguments.Substring("--cmd".Length).Trim();
        }
        else if (arguments.StartsWith("--powershell"))
        {
            shell = "powershell";
            arguments = arguments.Substring("--powershell".Length).Trim();
        }

        var shellSpecified = !string.IsNullOrWhiteSpace(shell);
        var prefixArgsWithShell = shellSpecified ? $"--{shell} " : string.Empty;

        var shouldEscape = !arguments.StartsWith('"') || shellSpecified;
        arguments = shouldEscape
            ? _cycoDmdWrapper.EscapeArgument(arguments)
            : arguments;

        return await _cycoDmdWrapper.ExecuteCycoDmdCommandAsync($"run {prefixArgsWithShell}{arguments}");
    }
    
    /// <summary>
    /// Handles the /image command by adding the image pattern to the current chat context
    /// </summary>
    private Task<string> HandleImageCommand(string arguments)
    {
        if (string.IsNullOrWhiteSpace(arguments))
            return Task.FromResult("/image requires an image file path or pattern.");

        _chatCommand.ImagePatterns.Add(arguments.Trim());
        return Task.FromResult($"Added image pattern: {arguments.Trim()}");
    }
    
    private readonly CycoDmdCliWrapper _cycoDmdWrapper;
    private readonly Dictionary<string, Func<string, Task<string>>> _commandHandlers;
    private readonly ChatCommand _chatCommand;
}