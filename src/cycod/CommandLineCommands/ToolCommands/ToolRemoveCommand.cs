using System;
using System.Threading.Tasks;

/// <summary>
/// Command to remove a specific tool.
/// </summary>
class ToolRemoveCommand : ToolBaseCommand
{
    /// <summary>
    /// The name of the tool to remove.
    /// </summary>
    public string? ToolName { get; set; }

    /// <summary>
    /// Constructor initializes the base command.
    /// </summary>
    public ToolRemoveCommand() : base()
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
        return "tool remove";
    }

    /// <summary>
    /// Execute the remove command.
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

            ConsoleHelpers.WriteDebugLine($"Removing tool: {ToolName} in scope: {Scope}");
            
            // Find the tool first to confirm it exists
            var toolPath = ToolFileHelpers.FindToolFile(ToolName, Scope ?? ConfigFileScope.Any);
            
            if (toolPath == null)
            {
                ConsoleHelpers.WriteErrorLine(ToolErrorHelpers.GetToolNotFoundError(ToolName, Scope ?? ConfigFileScope.Any));
                return 1;
            }
            
            // Get the scope of the found tool
            var effectiveScope = ScopeFileHelpers.GetScopeFromPath(toolPath);
            
            // For now, we won't check if the user is an administrator for global scope
            // as we don't have access to the ProcessHelpers.IsAdministrator method
            /*if (effectiveScope == ConfigFileScope.Global && !ProcessHelpers.IsAdministrator())
            {
                ConsoleHelpers.WriteErrorLine("Error: Administrator privileges required to remove a global tool.");
                return 1;
            }*/
            
            var success = ToolFileHelpers.RemoveTool(ToolName, effectiveScope);
            
            if (success)
            {
                // Format output to be consistent with other CYCOD commands
                ConsoleHelpers.WriteLine($"Deleted: {toolPath}");
                return 0;
            }
            else
            {
                ConsoleHelpers.WriteErrorLine($"Failed to delete {toolPath}. You may not have permission to modify this file.");
                return 1;
            }
        });
    }
}