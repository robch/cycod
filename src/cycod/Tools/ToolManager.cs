using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Manager for loading and managing custom tools.
/// </summary>
public static class ToolManager
{
    /// <summary>
    /// Loads a specific tool by name.
    /// </summary>
    /// <param name="toolName">The name of the tool to load</param>
    /// <param name="scope">The scope to search in</param>
    /// <returns>The loaded tool definition if found, null otherwise</returns>
    public static ToolDefinition? LoadTool(string toolName, ConfigFileScope scope = ConfigFileScope.Any)
    {
        var tool = ToolFileHelpers.GetToolDefinition(toolName, scope);
        if (tool == null)
        {
            ConsoleHelpers.WriteDebugLine($"Tool '{toolName}' not found in scope {scope}");
            return null;
        }
        
        ConsoleHelpers.WriteDebugLine($"Loaded tool '{toolName}' from {tool.SourceFilePath}");
        return tool;
    }
    
    /// <summary>
    /// Loads tools based on selection criteria.
    /// </summary>
    /// <param name="toolNames">List of tool names to load, or "*" for all</param>
    /// <param name="scope">The scope to search in</param>
    /// <returns>A list of loaded tool definitions</returns>
    public static List<ToolDefinition> LoadTools(List<string> toolNames, ConfigFileScope scope = ConfigFileScope.Any)
    {
        if (toolNames.Count == 0)
        {
            ConsoleHelpers.WriteDebugLine("No tools specified to load.");
            return new List<ToolDefinition>();
        }
        
        var start = DateTime.Now;
        ConsoleHelpers.WriteDebugLine($"Loading tools matching criteria: {string.Join(", ", toolNames)}");
        
        var allTools = ToolFileHelpers.ListTools(scope);
        var result = new List<ToolDefinition>();
        
        // Filter tools based on criteria
        foreach (var tool in allTools)
        {
            if (ShouldUseTool(tool.Name, toolNames))
            {
                result.Add(tool);
            }
        }
        
        if (result.Count == 0)
        {
            ConsoleHelpers.WriteDebugLine($"Found no tools matching criteria: {string.Join(", ", toolNames)}");
            return result;
        }
        
        var duration = TimeSpanFormatter.FormatMsOrSeconds(DateTime.Now - start);
        ConsoleHelpers.WriteDebugLine($"Loaded {result.Count} tools ({duration})");
        
        return result;
    }
    
    /// <summary>
    /// Determines if a tool should be used based on selection criteria.
    /// </summary>
    /// <param name="toolName">The name of the tool</param>
    /// <param name="toolSelections">List of selected tool names or patterns</param>
    /// <returns>True if the tool should be used, false otherwise</returns>
    public static bool ShouldUseTool(string toolName, List<string> toolSelections)
    {
        return toolSelections.Contains(toolName) || toolSelections.Contains("*");
    }
    
    /// <summary>
    /// Registers tools with a function factory.
    /// </summary>
    /// <param name="factory">The CustomToolFunctionFactory to register tools with</param>
    /// <param name="tools">The list of tools to register</param>
    /// <param name="displayActivation">Whether to display activation messages</param>
    /// <returns>The number of tools registered</returns>
    public static int RegisterTools(CustomToolFunctionFactory factory, List<ToolDefinition> tools, bool displayActivation = true)
    {
        if (tools.Count == 0)
        {
            return 0;
        }
        
        var count = 0;
        foreach (var tool in tools)
        {
            factory.AddCustomTool(tool);
            count++;
        }
        
        // Display activation message if requested
        if (displayActivation && !ConsoleHelpers.IsQuiet())
        {
            var toolNames = tools.Select(t => t.Name).ToList();
            var toolList = string.Join(", ", toolNames);
            ConsoleHelpers.WriteLine($"Enabled {count} tools: {toolList}", ConsoleColor.DarkGray);
        }
        
        return count;
    }
    
    /// <summary>
    /// Loads and registers tools in one operation.
    /// </summary>
    /// <param name="factory">The CustomToolFunctionFactory to register tools with</param>
    /// <param name="toolNames">List of tool names to load, or "*" for all</param>
    /// <param name="scope">The scope to search in</param>
    /// <param name="displayActivation">Whether to display activation messages</param>
    /// <returns>The number of tools registered</returns>
    public static int LoadAndRegisterTools(CustomToolFunctionFactory factory, List<string> toolNames, 
        ConfigFileScope scope = ConfigFileScope.Any, bool displayActivation = true)
    {
        if (toolNames.Count == 0)
        {
            ConsoleHelpers.WriteDebugLine("No tools specified to load.");
            return 0;
        }
        
        var tools = LoadTools(toolNames, scope);
        return RegisterTools(factory, tools, displayActivation);
    }
}