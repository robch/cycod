using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Provides methods for displaying tool information following consistent CYCOD formatting patterns.
/// </summary>
public static class ToolDisplayHelpers
{
    /// <summary>
    /// Displays tools for a specific scope.
    /// </summary>
    /// <param name="scope">The scope to display tools for</param>
    public static void DisplayTools(ConfigFileScope scope)
    {
        // Get the location display name
        var toolsDir = ToolFileHelpers.FindToolDirectoryInScope(scope);
        if (toolsDir == null)
        {
            // If the directory doesn't exist, create a default path for display purposes
            toolsDir = GetScopeDirectory(scope);
        }
        
        // Format the location header
        var locationHeader = $"{toolsDir} ({scope.ToString().ToLower()})";
        
        // Write the location header
        ConsoleHelpers.WriteLine($"LOCATION: {locationHeader}", overrideQuiet: true);
        ConsoleHelpers.WriteLine("", overrideQuiet: true);
        
        // Get the tools for this scope
        var tools = ToolFileHelpers.ListTools(scope);
        
        // Display the tools with proper indentation
        if (tools.Count == 0)
        {
            ConsoleHelpers.WriteLine("  No tools found in this scope.", overrideQuiet: true);
            return;
        }
        
        foreach (var tool in tools.OrderBy(t => t.Name))
        {
            var commandType = tool.IsMultiStep() 
                ? $"multi ({tool.Steps.Count})" 
                : tool.GetEffectiveCommandType();
            
            ConsoleHelpers.WriteLine($"  {tool.Name} ({commandType}) - {tool.Description}", overrideQuiet: true);
        }
    }

    /// <summary>
    /// Lists tools in a scope-grouped, indented format consistent with other CYCOD commands.
    /// </summary>
    public static void ListTools(List<ToolDefinition> tools)
    {
        // Group tools by scope for organized display
        var groupedTools = tools.GroupBy(t => t.Scope)
                               .OrderBy(g => g.Key); // Order by scope priority (Global, User, Local)
        
        foreach (var group in groupedTools)
        {
            var scope = group.Key;
            var scopeDir = GetScopeDirectory(scope);
            
            // Display location header
            ConsoleHelpers.WriteLine($"LOCATION: {scopeDir} ({scope.ToString().ToLower()})");
            ConsoleHelpers.WriteLine();
            
            if (!group.Any())
            {
                ConsoleHelpers.WriteLine("  No tools found in this scope.");
                ConsoleHelpers.WriteLine();
                continue;
            }
            
            // Display tools in this scope with indentation
            foreach (var tool in group.OrderBy(t => t.Name))
            {
                var commandType = tool.IsMultiStep() 
                    ? $"multi ({tool.Steps.Count})" 
                    : tool.GetEffectiveCommandType();
                
                ConsoleHelpers.WriteLine($"  {tool.Name} ({commandType}) - {tool.Description}");
            }
            
            ConsoleHelpers.WriteLine();
        }
    }

    /// <summary>
    /// Displays the full details of a tool in a format consistent with other CYCOD get commands.
    /// </summary>
    public static void DisplayToolDetails(ToolDefinition tool)
    {
        // Display location header consistent with other get commands
        ConsoleHelpers.WriteLine($"LOCATION: {tool.SourceFilePath} ({tool.Scope.ToString().ToLower()})");
        ConsoleHelpers.WriteLine();
        
        // Tool name with type in parentheses
        var commandType = tool.IsMultiStep() 
            ? $"multi ({tool.Steps.Count})" 
            : tool.GetEffectiveCommandType();
        
        ConsoleHelpers.WriteLine($"  {tool.Name} ({commandType})");
        ConsoleHelpers.WriteLine();
        
        // Indented details
        ConsoleHelpers.WriteLine($"    Description: {tool.Description}");
        
        // Display command content or steps based on tool type
        if (tool.IsMultiStep())
        {
            ConsoleHelpers.WriteLine();
            ConsoleHelpers.WriteLine($"    Steps:");
            
            for (int i = 0; i < tool.Steps.Count; i++)
            {
                var step = tool.Steps[i];
                ConsoleHelpers.WriteLine($"      {i+1}. {step.Name} ({step.GetEffectiveCommandType()})");
                ConsoleHelpers.WriteLine($"         Command: {step.GetEffectiveCommandContent()}");
            }
        }
        else
        {
            var commandContent = tool.GetEffectiveCommandContent();
            ConsoleHelpers.WriteLine();
            ConsoleHelpers.WriteLine($"    Command: {commandContent}");
        }
        
        // Parameters section if any exist
        if (tool.Parameters.Count > 0)
        {
            ConsoleHelpers.WriteLine();
            ConsoleHelpers.WriteLine($"    Parameters:");
            
            foreach (var param in tool.Parameters)
            {
                var required = param.Value.Required ? " (required)" : "";
                var defaultValue = param.Value.Default != null ? $" [default: {param.Value.Default}]" : "";
                ConsoleHelpers.WriteLine($"      {param.Key}: {param.Value.Type}{required}{defaultValue}");
                ConsoleHelpers.WriteLine($"        {param.Value.Description}");
            }
        }
        
        // Environment variables if any exist
        if (tool.Environment.Count > 0)
        {
            ConsoleHelpers.WriteLine();
            ConsoleHelpers.WriteLine($"    Environment Variables:");
            
            foreach (var env in tool.Environment)
            {
                ConsoleHelpers.WriteLine($"      {env.Key}={env.Value}");
            }
        }
        
        // Tags if any exist
        if (tool.Tags.Count > 0)
        {
            ConsoleHelpers.WriteLine();
            ConsoleHelpers.WriteLine($"    Tags: {string.Join(", ", tool.Tags)}");
        }
        
        // Technical details at the end
        ConsoleHelpers.WriteLine();
        ConsoleHelpers.WriteLine($"    Timeout: {tool.Timeout} ms");
        
        if (!string.IsNullOrEmpty(tool.WorkingDirectory))
        {
            ConsoleHelpers.WriteLine($"    Working Directory: {tool.WorkingDirectory}");
        }
    }
    
    /// <summary>
    /// Gets the directory path for a scope.
    /// </summary>
    private static string GetScopeDirectory(ConfigFileScope scope)
    {
        // This is a simplified method to get the scope directory path
        // In a real implementation, this would use ScopeFileHelpers
        switch (scope)
        {
            case ConfigFileScope.Global:
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), ".cycod", "tools");
            case ConfigFileScope.User:
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cycod", "tools");
            case ConfigFileScope.Local:
            default:
                return Path.Combine(Directory.GetCurrentDirectory(), ".cycod", "tools");
        }
    }
}