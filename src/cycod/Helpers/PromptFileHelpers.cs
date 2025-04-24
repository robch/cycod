using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Provides methods for working with prompt files across different scopes.
/// </summary>
public static class PromptFileHelpers
{
    /// <summary>
    /// Finds the prompt directory in the specified scope.
    /// </summary>
    /// <param name="scope">The scope to search in</param>
    /// <param name="create">Whether to create the directory if it doesn't exist</param>
    /// <returns>The path to the prompt directory, or null if not found</returns>
    public static string? FindPromptDirectoryInScope(ConfigFileScope scope, bool create = false)
    {
        return create
            ? ScopeFileHelpers.EnsureDirectoryInScope("prompts", scope)
            : ScopeFileHelpers.FindDirectoryInScope("prompts", scope);
    }

    /// <summary>
    /// Saves a prompt with the provided text to a file in the specified scope.
    /// </summary>
    /// <param name="promptName">Name of the prompt</param>
    /// <param name="promptText">Text to save in the prompt</param>
    /// <param name="scope">The scope to save the prompt to</param>
    /// <returns>List of saved file paths</returns>
    public static string SavePrompt(string promptName, string promptText, ConfigFileScope scope = ConfigFileScope.Local)
    {
        var promptDirectory = FindPromptDirectoryInScope(scope, create: true)!;
        var fileName = Path.Combine(promptDirectory, promptName + ".prompt");
        FileHelpers.WriteAllText(fileName, promptText);

        return fileName;
    }

    /// <summary>
    /// Finds a prompt file across all scopes (local, user, global) with optional parent directory search.
    /// </summary>
    /// <param name="promptName">The name of the prompt to find</param>
    /// <returns>The full path to the prompt file if found, null otherwise</returns>
    public static string? FindPromptFile(string promptName, ConfigFileScope scope = ConfigFileScope.Any)
    {
        var promptFileName = $"{promptName}.prompt";
        var promptFilePath = scope == ConfigFileScope.Any
            ? ScopeFileHelpers.FindFileInAnyScope(promptFileName, "prompts", searchParents: true)
            : ScopeFileHelpers.FindFileInScope(promptFileName, "prompts", scope);

        ConsoleHelpers.WriteDebugLine(promptFilePath != null
            ? $"FindPromptFile; found prompt in scope: {promptFilePath}"
            : $"FindPromptFile; prompt NOT FOUND in any scope: {promptName}");
        return promptFilePath;
    }

    /// <summary>
    /// Gets the text content of a prompt file.
    /// </summary>
    /// <param name="promptName">The name of the prompt to get</param>
    /// <returns>The prompt text if found, null otherwise</returns>
    public static string? GetPromptText(string promptName, ConfigFileScope scope = ConfigFileScope.Any)
    {
        var promptFilePath = FindPromptFile(promptName, scope);
        if (promptFilePath == null || !File.Exists(promptFilePath))
        {
            return null;
        }

        var content = File.ReadAllText(promptFilePath).Trim();
        
        // If content starts with @ symbol, it's a reference to another file
        if (content.StartsWith('@'))
        {
            var referencedFilePath = content.Substring(1);
            if (File.Exists(referencedFilePath))
            {
                return File.ReadAllText(referencedFilePath);
            }
        }
        
        return content;
    }

    // Display methods moved to PromptDisplayHelpers.cs
}