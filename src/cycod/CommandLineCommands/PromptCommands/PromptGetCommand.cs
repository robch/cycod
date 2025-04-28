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
    public override async Task<int> ExecuteAsync(bool interactive)
    {
        return await Task.Run(() => 
        {
            if (string.IsNullOrWhiteSpace(PromptName))
            {
                ConsoleHelpers.WriteErrorLine("Error: Prompt name is required.");
                return 1;
            }

            return ExecuteGet(PromptName, Scope ?? ConfigFileScope.Any);
        });
    }

    /// <summary>
    /// Execute the get command for the specified prompt name and scope.
    /// </summary>
    /// <param name="promptName">The name of the prompt to get.</param>
    /// <param name="scope">The scope to look in.</param>
    /// <returns>Exit code, 0 for success, non-zero for failure.</returns>
    private int ExecuteGet(string promptName, ConfigFileScope scope)
    {
        ConsoleHelpers.WriteDebugLine($"ExecuteGet; promptName: {promptName}; scope: {scope}");

        var promptFilePath = PromptFileHelpers.FindPromptFile(promptName);
        if (promptFilePath == null || !File.Exists(promptFilePath))
        {
            ConsoleHelpers.WriteErrorLine(scope == ConfigFileScope.Any
                ? $"Error: Prompt '{promptName}' not found in any scope."
                : $"Error: Prompt '{promptName}' not found in {scope} scope.");
            return 1;
        }

        // Read and display the prompt content
        var content = File.ReadAllText(promptFilePath);
        var foundInScope = ScopeFileHelpers.GetScopeFromPath(promptFilePath!);
        PromptDisplayHelpers.DisplayPrompt(promptName, promptFilePath, foundInScope, content);

        return 0;
    }
}