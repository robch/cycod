using System;
using System.Threading.Tasks;

/// <summary>
/// Command to get a specific tool.
/// </summary>
class ToolGetCommand : ToolBaseCommand
{
    /// <summary>
    /// The name of the tool to get.
    /// </summary>
    public string? ToolName { get; set; }

    /// <summary>
    /// Constructor initializes the base command.
    /// </summary>
    public ToolGetCommand() : base()
    {
        Scope = ConfigFileScope.Any;
    }

    /// <summary>
    /// Checks if the command is empty (i.e., no tool name provided).
    /// </summary>
    /// <returns>True if empty, false otherwise.</returns>
    public override bool IsEmpty()
    {
        return string.IsNullOrWhiteSpace(ToolName);
    }

    /// <summary>
    /// Gets the command name.
    /// </summary>
    /// <returns>The command name.</returns>
    public override string GetCommandName()
    {
        return "tool get";
    }

    /// <summary>
    /// Execute the get command.
    /// </summary>
    /// <param name="interactive">Whether the command is running in interactive mode.</param>
    /// <returns>Exit code, 0 for success, non-zero for failure.</returns>
    public override async Task<object> ExecuteAsync(bool interactive)
    {
        return await Task.Run(() =>
        {
            if (string.IsNullOrWhiteSpace(ToolName))
            {
                ConsoleHelpers.WriteErrorLine("Error: Tool name is required.");
                return 1;
            }

            ConsoleHelpers.WriteDebugLine($"Getting tool: {ToolName} in scope: {Scope}");
            
            var tool = ToolFileHelpers.GetToolDefinition(ToolName, Scope ?? ConfigFileScope.Any);
            
            if (tool == null)
            {
                ConsoleHelpers.WriteErrorLine($"Error: Tool '{ToolName}' not found");
                return 1;
            }
            
            ToolDisplayHelpers.DisplayToolDetails(tool);
            
            return 0;
        });
    }
}