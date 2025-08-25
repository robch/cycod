using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

/// <summary>
/// Executes a tool definition with parameters.
/// </summary>
public class ToolExecutor
{
    private readonly ToolDefinition _tool;
    private readonly Dictionary<string, object?> _parameters;
    private readonly Dictionary<string, Dictionary<string, string>> _stepOutputs = new();
    private readonly Dictionary<string, ToolDefinition> _referencedTools;

    /// <summary>
    /// Result of a tool execution.
    /// </summary>
    public class ToolExecutionResult
    {
    /// <summary>
    /// Exit code of the tool execution.
    /// </summary>
    public int ExitCode { get; set; }

    /// <summary>
    /// Output of the tool execution.
    /// </summary>
    public string Output { get; set; } = string.Empty;
    }

    private readonly Dictionary<string, IMcpServerConfigItem> _referencedMcps;
    private readonly Dictionary<string, Dictionary<string, string>> _referencedConfigs;
    private readonly Dictionary<string, Dictionary<string, string>> _referencedProfiles;
    
    /// <summary>
    /// Creates a new ToolExecutor for the specified tool.
    /// </summary>
    public ToolExecutor(ToolDefinition tool, Dictionary<string, object?> parameters)
    {
        _tool = tool;
        _parameters = parameters;
        
        // Validate parameters
        ValidateParameters();
        
        // Resolve referenced resources
        _referencedTools = ToolReferenceResolver.ResolveToolReferences(tool);
        _referencedMcps = ToolReferenceResolver.ResolveMcpReferences(tool);
        _referencedConfigs = ToolReferenceResolver.ResolveConfigReferences(tool);
        _referencedProfiles = ToolReferenceResolver.ResolveProfileReferences(tool);
    }
    
    /// <summary>
    /// Validates the parameters against the tool definition.
    /// </summary>
    private void ValidateParameters()
    {
        foreach (var param in _tool.Parameters)
        {
            var paramName = param.Key;
            var paramDef = param.Value;
            
            if (paramDef.Required && !_parameters.ContainsKey(paramName) && paramDef.Default == null)
            {
                throw new ArgumentException($"Required parameter '{paramName}' not provided");
            }
            
            if (_parameters.TryGetValue(paramName, out var value) && value != null)
            {
                if (!paramDef.ValidateValue(value, out var error))
                {
                    throw new ArgumentException($"Parameter '{paramName}': {error}");
                }
            }
        }
    }
    
    /// <summary>
    /// Executes the tool with the provided parameters.
    /// </summary>
    public async Task<object?> ExecuteAsync()
    {
        try
        {
            if (_tool.IsMultiStep())
            {
                return await ExecuteMultiStepToolAsync();
            }
            else
            {
                return await ExecuteSingleStepToolAsync();
            }
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteDebugLine($"Error executing tool '{_tool.Name}': {ex.Message}");
            return $"Error executing tool '{_tool.Name}': {ex.Message}";
        }
    }
    
    /// <summary>
    /// Executes a single-step tool.
    /// </summary>
    /// <summary>
    /// Executes the tool with the provided parameters synchronously.
    /// </summary>
    public ToolExecutionResult Execute()
    {
        try
        {
            var result = ExecuteAsync().GetAwaiter().GetResult();
            return new ToolExecutionResult
            {
                ExitCode = 0,
                Output = result?.ToString() ?? string.Empty
            };
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteDebugLine($"Error executing tool '{_tool.Name}': {ex.Message}");
            return new ToolExecutionResult
            {
                ExitCode = 1,
                Output = $"Error executing tool '{_tool.Name}': {ex.Message}"
            };
        }
    }
    private async Task<object?> ExecuteSingleStepToolAsync()
    {
        var commandType = _tool.GetEffectiveCommandType();
        var commandContent = _tool.GetEffectiveCommandContent();
        
        // Apply parameter substitution
        commandContent = SubstituteParameters(commandContent);
        
        // Process environment variables with parameter substitution
        var environment = new Dictionary<string, string>();
        foreach (var env in _tool.Environment)
        {
            environment[env.Key] = SubstituteParameters(env.Value);
        }
        
        // Execute based on command type
        switch (commandType.ToLower())
        {
            case "run":
                return ExecuteProcessCommand(commandContent, _tool.WorkingDirectory, environment, null, _tool.Timeout);
            case "bash":
            case "cmd":
            case "pwsh":
            case "python":
            case "script":
                // Use RunShellScript for all script types - it handles the proper shell selection and script file creation
                var shell = commandType.ToLower() == "script" ? (_tool.Shell ?? "default") : commandType;
                var result = ProcessHelpers.RunShellScript(shell, commandContent, null, _tool.WorkingDirectory, environment, null, _tool.Timeout);
                return result.MergedOutput; // Use MergedOutput instead of StandardOutput to include stderr
            case "tool":
                return await ExecuteToolCommandAsync(commandContent);
            case "mcp":
                return await ExecuteMcpCommandAsync(commandContent);
            default:
                throw new NotSupportedException($"Command type '{commandType}' not supported");
        }
    }
    
    /// <summary>
    /// Executes a multi-step tool.
    /// </summary>
    private async Task<object?> ExecuteMultiStepToolAsync()
    {
        object? finalResult = null;
        StringBuilder combinedOutput = new StringBuilder();
        
        for (int i = 0; i < _tool.Steps.Count; i++)
        {
            var step = _tool.Steps[i];
            var stepName = string.IsNullOrEmpty(step.Name) ? $"step{i+1}" : step.Name;
            
            ConsoleHelpers.WriteDebugLine($"Executing step: {stepName}");
            
            var commandType = step.GetEffectiveCommandType();
            var commandContent = step.GetEffectiveCommandContent();
            
            // Apply parameter substitution
            commandContent = SubstituteParameters(commandContent);
            
            // Apply step output substitution
            commandContent = SubstituteStepOutputs(commandContent);
            
            // Merge environment variables
            var environment = new Dictionary<string, string>();
            
            // Process tool-level environment variables with parameter substitution
            foreach (var env in _tool.Environment)
            {
                environment[env.Key] = SubstituteParameters(env.Value);
            }
            
            // Process step-level environment variables with parameter substitution
            foreach (var env in step.Environment)
            {
                environment[env.Key] = SubstituteParameters(env.Value);
            }
            
            // Get step timeout and working directory
            var timeout = step.Timeout ?? _tool.Timeout;
            var workingDirectory = step.WorkingDirectory ?? _tool.WorkingDirectory;
            
            // Execute based on command type
            object? stepResult;
            string output = "";
            string stdout = "";
            string stderr = "";
            int exitCode = 0;
            
            switch (commandType.ToLower())
            {
                case "run":
                    var runResult = ProcessHelpers.RunProcess(commandContent, workingDirectory, environment, null, timeout);
                    stdout = runResult.StandardOutput;
                    stderr = runResult.StandardError;
                    exitCode = runResult.ExitCode;
                    stepResult = stdout;
                    break;
                case "bash":
                case "cmd":
                case "pwsh":
                case "python":
                case "script":
                    // Use RunShellScript for all script types - it handles the proper shell selection and script file creation
                    var shell = commandType.ToLower() == "script" ? (step.Shell ?? _tool.Shell ?? "default") : commandType;
                    var scriptResult = ProcessHelpers.RunShellScript(shell, commandContent, null, workingDirectory, environment, null, timeout);
                    stdout = scriptResult.StandardOutput;
                    stderr = scriptResult.StandardError;
                    exitCode = scriptResult.ExitCode;
                    stepResult = scriptResult.MergedOutput; // Use MergedOutput instead of just stdout
                    break;
                case "tool":
                    stepResult = await ExecuteToolCommandAsync(commandContent);
                    stdout = stepResult?.ToString() ?? "";
                    stderr = "";
                    exitCode = 0;
                    break;
                case "mcp":
                    stepResult = await ExecuteMcpCommandAsync(commandContent);
                    stdout = stepResult?.ToString() ?? "";
                    stderr = "";
                    exitCode = 0;
                    break;
                default:
                    throw new NotSupportedException($"Command type '{commandType}' not supported in step '{stepName}'");
            }
            
            output = stdout + stderr;
            
            // Store step outputs
            _stepOutputs[stepName] = new Dictionary<string, string>
            {
                ["output"] = output,
                ["stdout"] = stdout,
                ["stderr"] = stderr,
                ["exit-code"] = exitCode.ToString()
            };
            
            // Add to combined output
            combinedOutput.AppendLine($"--- Step {i+1}: {stepName} ---");
            combinedOutput.AppendLine(output);
            combinedOutput.AppendLine();
            
            // Update final result
            finalResult = stepResult;
            
            if (exitCode != 0)
            {
                combinedOutput.AppendLine($"Error: Step '{stepName}' failed with exit code {exitCode}");
                return combinedOutput.ToString();
            }
        }
        
        // For multi-step tools, return combined output by default
        return combinedOutput.ToString();
    }
    
    /// <summary>
    /// Substitutes parameters in a string.
    /// </summary>
    private string SubstituteParameters(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return text;
        }
        
        // Use StringBuilder for more efficient string manipulation
        var result = new StringBuilder(text);
        
        // Process conditional parameter patterns first (e.g., {PARAM:-default})
        var conditionalParamRegex = new Regex(@"\{([^:}]+):-([^}]+)\}");
        var matches = conditionalParamRegex.Matches(result.ToString());
        
        foreach (Match match in matches)
        {
            var paramName = match.Groups[1].Value;
            var defaultValue = match.Groups[2].Value;
            string replacementValue;
            
            // Check if parameter exists and has a value
            if (_parameters.TryGetValue(paramName, out var paramValue) && paramValue != null && !string.IsNullOrEmpty(paramValue.ToString()))
            {
                replacementValue = paramValue.ToString() ?? string.Empty;
            }
            else if (_tool.Parameters.TryGetValue(paramName, out var paramDef) && 
                    paramDef.Default != null && !string.IsNullOrEmpty(paramDef.Default.ToString()))
            {
                replacementValue = paramDef.Default.ToString() ?? string.Empty;
            }
            else
            {
                replacementValue = defaultValue;
            }
            
            result.Replace(match.Value, replacementValue);
        }
        
        // 1. Process tool parameters
        foreach (var param in _tool.Parameters)
        {
            var paramName = param.Key;
            var paramDef = param.Value;
            
            if (_parameters.TryGetValue(paramName, out var value) || paramDef.Default != null)
            {
                var effectiveValue = value ?? paramDef.Default;
                
                // Special handling for boolean parameters with flag mapping
                if (paramDef.Type == "boolean" && paramDef.FlagMapping != null)
                {
                    bool boolValue = false;
                    
                    if (effectiveValue != null)
                    {
                        if (effectiveValue is bool b)
                        {
                            boolValue = b;
                        }
                        else if (bool.TryParse(effectiveValue.ToString(), out var parsedBool))
                        {
                            boolValue = parsedBool;
                        }
                        else if (effectiveValue.ToString() == "1" || 
                                effectiveValue.ToString()?.ToLower() == "yes" ||
                                effectiveValue.ToString()?.ToLower() == "y")
                        {
                            boolValue = true;
                        }
                    }
                    
                    var flagValue = boolValue ? paramDef.FlagMapping : string.Empty;
                    result.Replace($"{{{paramName}}}", flagValue);
                }
                else
                {
                    var valueStr = effectiveValue?.ToString() ?? string.Empty;
                    var escapedValue = ProcessHelpers.EscapeArgument(valueStr);
                    
                    // Replace both patterns in one pass
                    result.Replace($"{{{paramName}}}", escapedValue);
                    result.Replace($"{{RAW:{paramName}}}", valueStr);
                }
            }
        }
        
        // 2. Process referenced configs
        foreach (var config in _referencedConfigs)
        {
            var configName = config.Key;
            var configValues = config.Value;
            
            foreach (var entry in configValues)
            {
                var key = entry.Key;
                var value = entry.Value;
                var escapedValue = ProcessHelpers.EscapeArgument(value);
                
                // Replace both patterns in one pass
                result.Replace($"{{config.{configName}.{key}}}", escapedValue);
                result.Replace($"{{RAW:config.{configName}.{key}}}", value);
            }
        }
        
        // 3. Process referenced profiles
        foreach (var profile in _referencedProfiles)
        {
            var profileName = profile.Key;
            var profileValues = profile.Value;
            
            foreach (var entry in profileValues)
            {
                var key = entry.Key;
                var value = entry.Value;
                var escapedValue = ProcessHelpers.EscapeArgument(value);
                
                // Replace both patterns in one pass
                result.Replace($"{{profile.{profileName}.{key}}}", escapedValue);
                result.Replace($"{{RAW:profile.{profileName}.{key}}}", value);
            }
        }
        
        return result.ToString();
    }
    
    /// <summary>
    /// Substitutes parameters in a command string.
    /// This method is public to support external needs like showing commands without execution.
    /// </summary>
    /// <param name="commandText">The command text to substitute parameters in.</param>
    /// <returns>The command text with parameters substituted.</returns>
    public string SubstituteParametersForCommand(string commandText)
    {
        var result = SubstituteParameters(commandText);
        return SubstituteStepOutputs(result);
    }
    
    /// <summary>
    /// Substitutes step outputs in a string.
    /// </summary>
    private string SubstituteStepOutputs(string text)
    {
        var result = text;
        
        foreach (var step in _stepOutputs)
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
    
    #region Command Execution Methods
    
    /// <summary>
    /// Executes a process directly and returns the standard output.
    /// </summary>
    private string ExecuteProcessCommand(string command, string? workingDirectory, Dictionary<string, string> environment, string? input, int timeout)
    {
        var result = ProcessHelpers.RunProcess(command, workingDirectory, environment, input, timeout);
        return result.StandardOutput;
    }
    
    private async Task<string> ExecuteToolCommandAsync(string toolName)
    {
        // First check if the tool is a referenced tool
        if (_referencedTools.TryGetValue(toolName, out var referencedTool))
        {
            // Execute the referenced tool
            var executor = new ToolExecutor(referencedTool, _parameters);
            var result = await executor.ExecuteAsync();
            return result?.ToString() ?? string.Empty;
        }
        
        // Next, check if the tool is available as a dependency in the factory
        var factory = GetFunctionFactory();
        if (factory != null && factory.HasDependencyTool(toolName))
        {
            var dependencyTool = factory.GetDependencyTool(toolName);
            if (dependencyTool != null)
            {
                ConsoleHelpers.WriteDebugLine($"Executing dependency tool '{toolName}' from another tool");
                var executor = new ToolExecutor(dependencyTool, _parameters);
                var result = await executor.ExecuteAsync();
                return result?.ToString() ?? string.Empty;
            }
        }
        
        // Otherwise, try to load the tool from the file system
        var otherTool = ToolFileHelpers.GetToolDefinition(toolName);
        if (otherTool == null)
        {
            throw new ArgumentException($"Tool '{toolName}' not found");
        }
        
        // For now, just pass the same parameters
        var toolExecutor = new ToolExecutor(otherTool, _parameters);
        var toolResult = await toolExecutor.ExecuteAsync();
        
        return toolResult?.ToString() ?? string.Empty;
    }
    
    /// <summary>
    /// Gets the current function factory from the static provider if available.
    /// </summary>
    private CustomToolFunctionFactory? GetFunctionFactory()
    {
        // Access the static reference in CustomToolFunctionFactory
        return CustomToolFunctionFactory._currentFactory;
    }
    
    private async Task<string> ExecuteMcpCommandAsync(string mcpName)
    {
        // Check if the MCP is in the referenced MCPs
        if (_referencedMcps.TryGetValue(mcpName, out var mcpConfig))
        {
            // Create an MCP client and execute the command
            try
            {
                var client = await McpClientManager.CreateClientAsync(mcpName, mcpConfig);
                if (client != null)
                {
                    try
                    {
                        // Just return basic info about the MCP
                        return $"MCP '{mcpName}' connected successfully.\nServer Name: {client.ServerInfo.Name}\nVersion: {client.ServerInfo.Version}";
                    }
                    finally
                    {
                        await client.DisposeAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error connecting to MCP '{mcpName}': {ex.Message}";
            }
        }
        
        // Otherwise, try to load the MCP from the file system
        var mcps = McpFileHelpers.ListMcpServers(ConfigFileScope.Any);
        if (mcps.TryGetValue(mcpName, out var mcp))
        {
            try
            {
                var client = await McpClientManager.CreateClientAsync(mcpName, mcp);
                if (client != null)
                {
                    try
                    {
                        // Just return basic info about the MCP
                        return $"MCP '{mcpName}' connected successfully.\nServer Name: {client.ServerInfo.Name}\nVersion: {client.ServerInfo.Version}";
                    }
                    finally
                    {
                        await client.DisposeAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                return $"Error connecting to MCP '{mcpName}': {ex.Message}";
            }
        }
        
        return $"MCP '{mcpName}' not found";
    }
    
    #endregion
}