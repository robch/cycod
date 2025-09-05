using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Helper class for improved error handling and debugging information for tools.
/// </summary>
public static class ToolErrorHelpers
{
    /// <summary>
    /// Returns a formatted error message for tool not found errors.
    /// </summary>
    /// <param name="toolName">The name of the tool that was not found</param>
    /// <param name="scope">The scope that was searched</param>
    /// <returns>Formatted error message</returns>
    public static string GetToolNotFoundError(string toolName, ConfigFileScope scope)
    {
        var scopeStr = GetScopeDescription(scope);
        return $"Tool '{toolName}' not found in {scopeStr}. Run 'cycod tool list' to see available tools.";
    }

    /// <summary>
    /// Returns a formatted error message for invalid tool definition errors.
    /// </summary>
    /// <param name="toolName">The name of the tool</param>
    /// <param name="errors">The list of validation errors</param>
    /// <returns>Formatted error message</returns>
    public static string GetToolValidationError(string toolName, IEnumerable<string> errors)
    {
        var errorList = string.Join(Environment.NewLine, errors.Select(e => $"  - {e}"));
        return $"Invalid tool definition for '{toolName}':{Environment.NewLine}{errorList}";
    }

    /// <summary>
    /// Returns a formatted error message for tool parameter validation errors.
    /// </summary>
    /// <param name="toolName">The name of the tool</param>
    /// <param name="errors">The list of parameter validation errors</param>
    /// <returns>Formatted error message</returns>
    public static string GetParameterValidationError(string toolName, IEnumerable<string> errors)
    {
        var errorList = string.Join(Environment.NewLine, errors.Select(e => $"  - {e}"));
        return $"Invalid parameters for tool '{toolName}':{Environment.NewLine}{errorList}{Environment.NewLine}Run 'cycod tool get {toolName}' to see valid parameters.";
    }

    /// <summary>
    /// Returns a formatted error message for file operation errors.
    /// </summary>
    /// <param name="operation">The operation that failed (e.g., "save", "load", "remove")</param>
    /// <param name="toolName">The name of the tool</param>
    /// <param name="filePath">The file path (if available)</param>
    /// <param name="errorMessage">The original error message</param>
    /// <returns>Formatted error message</returns>
    public static string GetFileOperationError(string operation, string toolName, string? filePath, string errorMessage)
    {
        var pathInfo = filePath != null ? $" at {filePath}" : "";
        return $"Failed to {operation} tool '{toolName}'{pathInfo}: {errorMessage}";
    }

    /// <summary>
    /// Returns a formatted error message for tool execution errors.
    /// </summary>
    /// <param name="toolName">The name of the tool</param>
    /// <param name="step">The step that failed (if applicable)</param>
    /// <param name="errorMessage">The original error message</param>
    /// <returns>Formatted error message</returns>
    public static string GetExecutionError(string toolName, string? step, string errorMessage)
    {
        var stepInfo = !string.IsNullOrEmpty(step) ? $" (step: {step})" : "";
        return $"Error executing tool '{toolName}'{stepInfo}: {errorMessage}";
    }

    /// <summary>
    /// Returns a formatted error message for command execution timeout.
    /// </summary>
    /// <param name="toolName">The name of the tool</param>
    /// <param name="step">The step that timed out (if applicable)</param>
    /// <param name="timeout">The timeout value in milliseconds</param>
    /// <returns>Formatted error message</returns>
    public static string GetTimeoutError(string toolName, string? step, int timeout)
    {
        var stepInfo = !string.IsNullOrEmpty(step) ? $" (step: {step})" : "";
        return $"Tool '{toolName}'{stepInfo} timed out after {timeout}ms. Use --timeout to increase the timeout.";
    }

    /// <summary>
    /// Gets a user-friendly description of a scope.
    /// </summary>
    /// <param name="scope">The scope</param>
    /// <returns>A user-friendly description</returns>
    private static string GetScopeDescription(ConfigFileScope scope)
    {
        return scope switch
        {
            ConfigFileScope.Global => "global scope (all users)",
            ConfigFileScope.User => "user scope (current user)",
            ConfigFileScope.Local => "local scope (current project)",
            ConfigFileScope.Any => "any scope",
            _ => scope.ToString().ToLower()
        };
    }

    /// <summary>
    /// Writes detailed debugging information for a tool execution.
    /// </summary>
    /// <param name="tool">The tool being executed</param>
    /// <param name="parameters">The parameters being used</param>
    /// <param name="commands">The resolved commands (if available)</param>
    public static void WriteToolDebugInfo(ToolDefinition tool, Dictionary<string, object> parameters, IEnumerable<string>? commands = null)
    {
        ConsoleHelpers.WriteDebugLine($"Tool: {tool.Name} ({tool.Scope})");
        ConsoleHelpers.WriteDebugLine($"Description: {tool.Description}");
        ConsoleHelpers.WriteDebugLine($"Source: {tool.SourceFilePath ?? "unknown"}");
        
        if (tool.Parameters != null && tool.Parameters.Count > 0)
        {
            ConsoleHelpers.WriteDebugLine("Parameters:");
            foreach (var param in tool.Parameters)
            {
                var paramName = param.Key;
                var paramValue = parameters.TryGetValue(paramName, out var value) ? value?.ToString() : "[NOT SET]";
                var paramInfo = param.Value;
                var required = paramInfo.Required ? "required" : "optional";
                var defaultValue = paramInfo.Default != null ? $", default: {paramInfo.Default}" : "";
                
                ConsoleHelpers.WriteDebugLine($"  {paramName} = {paramValue} ({paramInfo.Type}, {required}{defaultValue})");
            }
        }
        
        if (commands != null)
        {
            ConsoleHelpers.WriteDebugLine("Commands:");
            foreach (var command in commands)
            {
                ConsoleHelpers.WriteDebugLine($"  {command}");
            }
        }
    }
}