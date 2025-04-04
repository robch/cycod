using System;
using System.IO;
using System.Threading.Tasks;

/// <summary>
/// Command to view the content of a specific prompt.
/// </summary>
class PromptGetCommand : PromptBaseCommand
{
    /// <summary>
    /// The name of the prompt to get.
    /// </summary>
    public string? PromptName { get; set; }

    /// <summary>
    /// Constructor initializes the base command.
    /// </summary>
    public PromptGetCommand() : base()
    {
        Scope = ConfigFileScope.Any;
    }

    /// <summary>
    /// Checks if the command is empty (i.e., no prompt name provided).
    /// </summary>
    /// <returns>True if empty, false otherwise.</returns>
    public override bool IsEmpty()
    {
        return string.IsNullOrWhiteSpace(PromptName);
    }

    /// <summary>
    /// Gets the command name.
    /// </summary>
    /// <returns>The command name.</returns>
    public override string GetCommandName()
    {
        return "prompt get";
    }

    /// <summary>
    /// Execute the get command.
    /// </summary>
    /// <param name="interactive">Whether the command is running in interactive mode.</param>
    /// <returns>Exit code, 0 for success, non-zero for failure.</returns>
    public Task<int> Execute(bool interactive)
    {
        if (string.IsNullOrWhiteSpace(PromptName))
        {
            ConsoleHelpers.WriteErrorLine("Error: Prompt name is required.");
            return Task.FromResult(1);
        }

        var result = ExecuteGet(PromptName, Scope ?? ConfigFileScope.Any);
        return Task.FromResult(result);
    }

    /// <summary>
    /// Execute the get command for the specified prompt name and scope.
    /// </summary>
    /// <param name="promptName">The name of the prompt to get.</param>
    /// <param name="scope">The scope to look in.</param>
    /// <returns>Exit code, 0 for success, non-zero for failure.</returns>
    private int ExecuteGet(string promptName, ConfigFileScope scope)
    {
        string? promptFilePath = null;

        if (scope == ConfigFileScope.Any)
        {
            promptFilePath = PromptFileHelpers.FindPromptFile(promptName);
        }
        else
        {
            var promptDir = PromptFileHelpers.FindPromptDirectoryInScope(scope);
            if (promptDir != null)
            {
                var potentialFilePath = Path.Combine(promptDir, $"{promptName}.prompt");
                if (File.Exists(potentialFilePath))
                {
                    promptFilePath = potentialFilePath;
                }
            }
        }

        if (promptFilePath == null || !File.Exists(promptFilePath))
        {
            ConsoleHelpers.WriteErrorLine($"Error: Prompt '{promptName}' not found in specified scope.");
            return 1;
        }

        // Determine the scope from the file path
        var fileScope = ConfigFileScope.Local; // Default
        if (promptFilePath.Contains(ConfigFileHelpers.GetScopeDirectoryPath(ConfigFileScope.Global) ?? ""))
        {
            fileScope = ConfigFileScope.Global;
        }
        else if (promptFilePath.Contains(ConfigFileHelpers.GetScopeDirectoryPath(ConfigFileScope.User) ?? ""))
        {
            fileScope = ConfigFileScope.User;
        }

        // Read and display the prompt content
        var content = File.ReadAllText(promptFilePath);
        
        // If content starts with @ symbol, it's a reference to another file
        if (content.StartsWith('@'))
        {
            var referencedFilePath = content.Substring(1);
            if (File.Exists(referencedFilePath))
            {
                content = File.ReadAllText(referencedFilePath);
            }
        }
        
        // Display the prompt using the standardized method
        PromptDisplayHelpers.DisplayPrompt(promptName, promptFilePath, fileScope, content);

        return 0;
    }
}