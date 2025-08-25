using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Provides methods for working with tool files across different scopes.
/// </summary>
public static class ToolFileHelpers
{
    private const string TOOLS_DIRECTORY = "tools";

    /// <summary>
    /// Finds the tool directory in the specified scope.
    /// </summary>
    /// <param name="scope">The scope to search in</param>
    /// <param name="create">Whether to create the directory if it doesn't exist</param>
    /// <returns>The path to the tool directory, or null if not found</returns>
    public static string? FindToolDirectoryInScope(ConfigFileScope scope, bool create = false)
    {
        return create
            ? ScopeFileHelpers.EnsureDirectoryInScope(TOOLS_DIRECTORY, scope)
            : ScopeFileHelpers.FindDirectoryInScope(TOOLS_DIRECTORY, scope);
    }

    /// <summary>
    /// Saves a tool definition to a file in the specified scope.
    /// </summary>
    /// <param name="tool">The tool definition to save</param>
    /// <param name="scope">The scope to save the tool to</param>
    /// <returns>The path to the saved file</returns>
    public static string SaveToolDefinition(ToolDefinition tool, ConfigFileScope scope = ConfigFileScope.Local)
    {
        var toolDirectory = FindToolDirectoryInScope(scope, create: true)!;
        var fileName = Path.Combine(toolDirectory, $"{tool.Name}.yaml");

        try
        {
            var yaml = tool.ToYaml();
            FileHelpers.WriteAllText(fileName, yaml);
            
            tool.SourceFilePath = fileName;
            tool.Scope = scope;
            
            return fileName;
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteErrorLine(ToolErrorHelpers.GetFileOperationError("save", tool.Name, fileName, ex.Message));
            throw;
        }
    }

    /// <summary>
    /// Finds a tool file across all scopes (local, user, global) with optional parent directory search.
    /// </summary>
    /// <param name="toolName">The name of the tool to find</param>
    /// <param name="scope">The scope to search in</param>
    /// <returns>The full path to the tool file if found, null otherwise</returns>
    public static string? FindToolFile(string toolName, ConfigFileScope scope = ConfigFileScope.Any)
    {
        var toolFileName = $"{toolName}.yaml";
        var toolFilePath = scope == ConfigFileScope.Any
            ? ScopeFileHelpers.FindFileInAnyScope(toolFileName, TOOLS_DIRECTORY, searchParents: true)
            : ScopeFileHelpers.FindFileInScope(toolFileName, TOOLS_DIRECTORY, scope);

        ConsoleHelpers.WriteDebugLine(toolFilePath != null
            ? $"FindToolFile; found tool in scope: {toolFilePath}"
            : $"FindToolFile; tool NOT FOUND in any scope: {toolName}");
            
        return toolFilePath;
    }

    /// <summary>
    /// Loads a tool definition from a file.
    /// </summary>
    /// <param name="filePath">Path to the tool definition file</param>
    /// <param name="scope">The scope the file was found in</param>
    /// <returns>The loaded tool definition, or null if loading failed</returns>
    public static ToolDefinition? LoadToolDefinition(string filePath, ConfigFileScope scope = ConfigFileScope.Any)
    {
        try
        {
            var yaml = FileHelpers.ReadAllText(filePath);
            var tool = ToolDefinition.FromYaml(yaml, filePath, scope);
            
            if (tool != null)
            {
                // Validate the tool definition
                if (tool.Validate(out var errors))
                {
                    return tool;
                }
                else
                {
                    ConsoleHelpers.WriteErrorLine(ToolErrorHelpers.GetToolValidationError(Path.GetFileNameWithoutExtension(filePath), errors));
                    return null;
                }
            }
            
            return null;
        }
        catch (Exception ex)
        {
            var toolName = Path.GetFileNameWithoutExtension(filePath);
            ConsoleHelpers.WriteErrorLine(ToolErrorHelpers.GetFileOperationError("load", toolName, filePath, ex.Message));
            return null;
        }
    }

    /// <summary>
    /// Gets a tool definition by name.
    /// </summary>
    /// <param name="toolName">The name of the tool to get</param>
    /// <param name="scope">The scope to search in</param>
    /// <returns>The tool definition if found, null otherwise</returns>
    public static ToolDefinition? GetToolDefinition(string toolName, ConfigFileScope scope = ConfigFileScope.Any)
    {
        var toolFilePath = FindToolFile(toolName, scope);
        if (toolFilePath == null)
        {
            return null;
        }
        
        var effectiveScope = DetermineScope(toolFilePath);
        var tool = LoadToolDefinition(toolFilePath, effectiveScope);
        
        return tool;
    }

    /// <summary>
    /// Lists all tools in the specified scope.
    /// </summary>
    /// <param name="scope">The scope to list tools from</param>
    /// <returns>A list of tool definitions</returns>
    public static List<ToolDefinition> ListTools(ConfigFileScope scope = ConfigFileScope.Any)
    {
        var tools = new List<ToolDefinition>();
        
        if (scope == ConfigFileScope.Any)
        {
            // Search in all scopes
            foreach (var s in new[] { ConfigFileScope.Local, ConfigFileScope.User, ConfigFileScope.Global })
            {
                tools.AddRange(GetToolsInScope(s));
            }
        }
        else
        {
            // Search in the specified scope
            tools.AddRange(GetToolsInScope(scope));
        }
        
        return tools;
    }

    /// <summary>
    /// Gets all tools in a specific scope.
    /// </summary>
    /// <param name="scope">The scope to get tools from</param>
    /// <returns>A list of tool definitions</returns>
    private static List<ToolDefinition> GetToolsInScope(ConfigFileScope scope)
    {
        var tools = new List<ToolDefinition>();
        var directory = FindToolDirectoryInScope(scope);
        
        if (directory == null || !Directory.Exists(directory))
        {
            return tools;
        }
        
        foreach (var file in Directory.GetFiles(directory, "*.yaml"))
        {
            var tool = LoadToolDefinition(file, scope);
            if (tool != null)
            {
                tools.Add(tool);
            }
        }
        
        return tools;
    }

    /// <summary>
    /// Removes a tool from the specified scope.
    /// </summary>
    /// <param name="toolName">The name of the tool to remove</param>
    /// <param name="scope">The scope to remove the tool from</param>
    /// <returns>True if the tool was removed, false otherwise</returns>
    public static bool RemoveTool(string toolName, ConfigFileScope scope = ConfigFileScope.Any)
    {
        var toolFile = FindToolFile(toolName, scope);
        if (toolFile == null || !FileHelpers.FileExists(toolFile))
        {
            return false;
        }
        
        try
        {
            File.Delete(toolFile);
           
            return true;
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteErrorLine(ToolErrorHelpers.GetFileOperationError("remove", toolName, toolFile, ex.Message));
            return false;
        }
    }

    /// <summary>
    /// Determines the scope of a file based on its path.
    /// </summary>
    /// <param name="filePath">The file path to check</param>
    /// <returns>The scope of the file</returns>
    private static ConfigFileScope DetermineScope(string filePath)
    {
        var globalDir = ScopeFileHelpers.FindDirectoryInScope(TOOLS_DIRECTORY, ConfigFileScope.Global);
        var userDir = ScopeFileHelpers.FindDirectoryInScope(TOOLS_DIRECTORY, ConfigFileScope.User);
        var localDir = ScopeFileHelpers.FindDirectoryInScope(TOOLS_DIRECTORY, ConfigFileScope.Local);

        if (globalDir != null && filePath.StartsWith(globalDir))
        {
            return ConfigFileScope.Global;
        }
        else if (userDir != null && filePath.StartsWith(userDir))
        {
            return ConfigFileScope.User;
        }
        else if (localDir != null && filePath.StartsWith(localDir))
        {
            return ConfigFileScope.Local;
        }
        
        return ConfigFileScope.Any;
    }
}