using System;
using System.Threading.Tasks;

/// <summary>
/// Command to create a new prompt.
/// </summary>
class PromptCreateCommand : PromptBaseCommand
{
    /// <summary>
    /// The name of the prompt to create.
    /// </summary>
    public string? PromptName { get; set; }
    
    /// <summary>
    /// The text content of the prompt.
    /// </summary>
    public string? PromptText { get; set; }

    /// <summary>
    /// Constructor initializes the base command.
    /// </summary>
    public PromptCreateCommand() : base()
    {
        Scope = ConfigFileScope.Local; // Default to local scope
    }

    /// <summary>
    /// Checks if the command is empty (i.e., no prompt name or text provided).
    /// </summary>
    /// <returns>True if empty, false otherwise.</returns>
    public override bool IsEmpty()
    {
        return string.IsNullOrWhiteSpace(PromptName) || string.IsNullOrWhiteSpace(PromptText);
    }

    /// <summary>
    /// Gets the command name.
    /// </summary>
    /// <returns>The command name.</returns>
    public override string GetCommandName()
    {
        return "prompt create";
    }

    /// <summary>
    /// Execute the create command.
    /// </summary>
    /// <param name="interactive">Whether the command is running in interactive mode.</param>
    /// <returns>Exit code, 0 for success, non-zero for failure.</returns>
    public override async Task<object> ExecuteAsync(bool interactive)
    {
        return await Task.Run(() => 
        {
            if (string.IsNullOrWhiteSpace(PromptName))
            {
                ConsoleHelpers.WriteErrorLine("Error: Prompt name is required.");
                return 1;
            }
            
            if (string.IsNullOrWhiteSpace(PromptText))
            {
                ConsoleHelpers.WriteErrorLine("Error: Prompt text is required.");
                return 1;
            }

            return ExecuteCreate(PromptName, PromptText, Scope ?? ConfigFileScope.Local);
        });
    }

    /// <summary>
    /// Execute the create command for the specified prompt name, text, and scope.
    /// </summary>
    /// <param name="promptName">The name of the prompt to create.</param>
    /// <param name="promptText">The text content of the prompt.</param>
    /// <param name="scope">The scope to create the prompt in.</param>
    /// <returns>Exit code, 0 for success, non-zero for failure.</returns>
    private int ExecuteCreate(string promptName, string promptText, ConfigFileScope scope)
    {
        try
        {
            // Remove leading slash if someone added it (for consistency)
            if (promptName.StartsWith('/'))
            {
                promptName = promptName.Substring(1);
            }
            
            // Check if the prompt already exists
            var existingPromptFile = PromptFileHelpers.FindPromptFile(promptName);
            if (existingPromptFile != null)
            {
                ConsoleHelpers.WriteErrorLine($"Error: Prompt '{promptName}' already exists.");
                return 1;
            }
            
            // Save the prompt
            var fileName = PromptFileHelpers.SavePrompt(promptName, promptText, scope);
            PromptDisplayHelpers.DisplaySavedPromptFile(fileName);
            
            return 0;
        }
        catch (Exception ex)
        {
            ConsoleHelpers.LogException(ex, "Error creating prompt");
            return 1;
        }
    }
}