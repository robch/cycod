using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Command to run a specific tool.
/// </summary>
class ToolRunCommand : ToolBaseCommand
{
    /// <summary>
    /// The name of the tool to run.
    /// </summary>
    public string? ToolName { get; set; }

    /// <summary>
    /// Parameter values for the tool.
    /// </summary>
    public Dictionary<string, object> ParameterValues { get; set; } = new();

    /// <summary>
    /// Whether to show the command that would be executed without running it.
    /// </summary>
    public bool ShowCommand { get; set; } = false;

    /// <summary>
    /// Whether to parse and validate parameters without executing the command.
    /// </summary>
    public bool DryRun { get; set; } = false;

    /// <summary>
    /// Override the tool's default timeout.
    /// </summary>
    public int? Timeout { get; set; }

    /// <summary>
    /// Constructor initializes the base command.
    /// </summary>
    public ToolRunCommand() : base()
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
        return "tool run";
    }

    /// <summary>
    /// Execute the run command.
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

            ConsoleHelpers.WriteDebugLine($"Running tool: {ToolName} in scope: {Scope}");
            
            // Load the tool definition
            var tool = ToolFileHelpers.GetToolDefinition(ToolName, Scope ?? ConfigFileScope.Any);
            
            if (tool == null)
            {
                ConsoleHelpers.WriteErrorLine($"Error: Tool '{ToolName}' not found");
                return 1;
            }
            
            // Add detailed debugging information
            ToolErrorHelpers.WriteToolDebugInfo(tool, ParameterValues);
            
            // Validate parameters
            if (!ValidateParameters(tool, out var validationErrors))
            {
                ConsoleHelpers.WriteErrorLine(ToolErrorHelpers.GetParameterValidationError(ToolName, validationErrors));
                return 1;
            }
            
            // If dry run, just show parameter values and exit
            if (DryRun)
            {
                ConsoleHelpers.WriteLine($"Tool: {tool.Name}");
                ConsoleHelpers.WriteLine($"Description: {tool.Description}");
                
                if (ParameterValues.Count > 0)
                {
                    ConsoleHelpers.WriteLine("Parameters:");
                    foreach (var param in ParameterValues)
                    {
                        ConsoleHelpers.WriteLine($"  {param.Key}: {param.Value}");
                    }
                }
                
                return 0;
            }
            
            // Execute the tool
            return ExecuteTool(tool);
        });
    }

    /// <summary>
    /// Validates the parameters against the tool definition.
    /// </summary>
    private bool ValidateParameters(ToolDefinition tool, out List<string> errors)
    {
        errors = new List<string>();
        
        // Check required parameters
        foreach (var param in tool.Parameters)
        {
            var paramName = param.Key;
            var paramDef = param.Value;
            
            if (paramDef.Required && !ParameterValues.ContainsKey(paramName) && paramDef.Default == null)
            {
                errors.Add($"Required parameter '{paramName}' not provided.");
            }
            
            if (ParameterValues.TryGetValue(paramName, out var value))
            {
                if (!paramDef.ValidateValue(value, out var error))
                {
                    errors.Add($"Parameter '{paramName}': {error}");
                }
            }
        }
        
        // Check for unknown parameters
        foreach (var paramName in ParameterValues.Keys)
        {
            if (!tool.Parameters.ContainsKey(paramName))
            {
                errors.Add($"Unknown parameter '{paramName}'.");
            }
        }
        
        return errors.Count == 0;
    }

    /// <summary>
    /// Executes the tool.
    /// </summary>
    private int ExecuteTool(ToolDefinition tool)
    {
        // Create and use ToolExecutor to run the tool
        try
        {
            // If ShowCommand is true, handle it separately
            if (ShowCommand)
            {
                return ShowToolCommands(tool);
            }
            
            var executor = new ToolExecutor(tool, ParameterValues);
            var result = executor.Execute();
            
            // Display the output
            Console.Write(result.Output);
            
            // Check execution result
            if (result.ExitCode != 0)
            {
                ConsoleHelpers.WriteErrorLine($"Tool execution failed with exit code: {result.ExitCode}");
            }
            
            return result.ExitCode;
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteErrorLine($"Error executing tool '{ToolName ?? "unknown"}': {ex.Message}");
            ConsoleHelpers.WriteDebugLine(ex.ToString());
            return 1;
        }
    }

    /// <summary>
    /// Shows the commands that would be executed without running them.
    /// </summary>
    private int ShowToolCommands(ToolDefinition tool)
    {
        ConsoleHelpers.WriteLine($"Tool: {tool.Name}");
        ConsoleHelpers.WriteLine($"Description: {tool.Description}");
        ConsoleHelpers.WriteLine($"Source: {tool.SourceFilePath ?? "unknown"}");
        ConsoleHelpers.WriteLine($"Scope: {tool.Scope.ToString().ToLower()}");
        
        // Show parameter values
        if (ParameterValues.Count > 0)
        {
            ConsoleHelpers.WriteLine("\nParameters:");
            foreach (var param in ParameterValues)
            {
                ConsoleHelpers.WriteLine($"  {param.Key} = {param.Value}");
            }
        }
        
        if (tool.IsMultiStep())
        {
            ConsoleHelpers.WriteLine($"\nSteps: {tool.Steps.Count}");
            
            // Handle multi-step tool commands
            for (int i = 0; i < tool.Steps.Count; i++)
            {
                var step = tool.Steps[i];
                var stepName = string.IsNullOrEmpty(step.Name) ? $"step{i+1}" : step.Name;
                
                ConsoleHelpers.WriteLine($"\nStep {i+1}: {stepName}");
                
                var commandType = step.GetEffectiveCommandType();
                var commandContent = step.GetEffectiveCommandContent();
                
                // Create a ToolExecutor just for parameter substitution
                var executor = new ToolExecutor(tool, ParameterValues);
                var substitutedCommand = executor.SubstituteParametersForCommand(commandContent);
                
                ConsoleHelpers.WriteLine($"Type: {commandType}");
                ConsoleHelpers.WriteLine($"Command: {substitutedCommand}");
            }
        }
        else
        {
            // Handle single-step tool command
            var commandType = tool.GetEffectiveCommandType();
            var commandContent = tool.GetEffectiveCommandContent();
            
            // Create a ToolExecutor just for parameter substitution
            var executor = new ToolExecutor(tool, ParameterValues);
            var substitutedCommand = executor.SubstituteParametersForCommand(commandContent);
            
            ConsoleHelpers.WriteLine($"\nType: {commandType}");
            ConsoleHelpers.WriteLine($"Command: {substitutedCommand}");
        }
        
        // Show timeout information
        int timeoutValue = Timeout.HasValue ? Timeout.Value : tool.Timeout;
        ConsoleHelpers.WriteLine($"\nTimeout: {timeoutValue}ms");
        
        return 0;
    }

    /// <summary>
    /// Substitutes parameters in a string.
    /// </summary>
    private string SubstituteParameters(string text, Dictionary<string, ToolParameter> parameters)
    {
        var result = text;
        
        foreach (var param in parameters)
        {
            var paramName = param.Key;
            var paramDef = param.Value;
            
            if (ParameterValues.TryGetValue(paramName, out var value) || paramDef.Default != null)
            {
                var effectiveValue = paramDef.GetEffectiveValue(value);
                var valueStr = effectiveValue?.ToString() ?? string.Empty;
                
                // Regular parameter substitution (escaped)
                result = result.Replace($"{{{paramName}}}", ProcessHelpers.EscapeArgument(valueStr));
                
                // Raw parameter substitution (unescaped)
                result = result.Replace($"{{RAW:{paramName}}}", valueStr);
            }
        }
        
        return result;
    }

    /// <summary>
    /// Substitutes step outputs in a string.
    /// </summary>
    private string SubstituteStepOutputs(string text, Dictionary<string, Dictionary<string, string>> stepOutputs)
    {
        var result = text;
        
        foreach (var step in stepOutputs)
        {
            var stepName = step.Key;
            var outputs = step.Value;
            
            foreach (var output in outputs)
            {
                var outputName = output.Key;
                var outputValue = output.Value;
                
                // Regular output substitution (escaped)
                result = result.Replace($"{{{stepName}.{outputName}}}", ProcessHelpers.EscapeArgument(outputValue));
                
                // Raw output substitution (unescaped)
                result = result.Replace($"{{RAW:{stepName}.{outputName}}}", outputValue);
            }
        }
        
        return result;
    }
}