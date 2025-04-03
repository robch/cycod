using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Provides methods for displaying prompt information.
/// </summary>
public static class PromptDisplayHelpers
{
    /// <summary>
    /// Gets a display name for the prompt directory location in the specified scope.
    /// </summary>
    /// <param name="scope">The scope to get the location display name for</param>
    /// <returns>A string representing the location, or null if the scope is invalid</returns>
    public static string? GetLocationDisplayName(ConfigFileScope scope)
    {
        var promptPath = PromptFileHelpers.FindPromptDirectoryInScope(scope);
        if (promptPath != null)
        {
            return $"{promptPath} ({scope.ToString().ToLower()})";
        }
        
        var scopeDir = ConfigFileHelpers.GetScopeDirectoryPath(scope);
        if (scopeDir == null) return null;

        return $"{Path.Combine(scopeDir, "prompts")} ({scope.ToString().ToLower()})";
    }

    /// <summary>
    /// Displays prompts for a specific scope.
    /// </summary>
    /// <param name="scope">The scope to display prompts for</param>
    public static void DisplayPrompts(ConfigFileScope scope)
    {
        // Get the location display name, which works even if the directory doesn't exist yet
        var locationDisplay = GetLocationDisplayName(scope);
        if (locationDisplay == null) return;
        
        ConsoleHelpers.WriteLine($"LOCATION: {locationDisplay}");
        Console.WriteLine();

        // Find the directory to check for prompts
        var promptDir = PromptFileHelpers.FindPromptDirectoryInScope(scope);
        
        // Check for prompts only if the directory exists
        var promptFiles = new List<string>();
        if (promptDir != null && Directory.Exists(promptDir))
        {
            promptFiles = Directory.GetFiles(promptDir, "*.prompt")
                .OrderBy(file => Path.GetFileNameWithoutExtension(file))
                .ToList();
        }

        DisplayPromptFiles(promptFiles);
    }

    /// <summary>
    /// Displays a list of prompt files.
    /// </summary>
    /// <param name="promptFiles">The list of prompt files to display</param>
    /// <param name="indentLevel">The level of indentation for display</param>
    public static void DisplayPromptFiles(List<string> promptFiles, int indentLevel = 2)
    {
        var indent = new string(' ', indentLevel);
        
        if (promptFiles.Count == 0)
        {
            ConsoleHelpers.WriteLine($"{indent}No prompts found in this scope");
            return;
        }

        foreach (var promptFile in promptFiles)
        {
            var promptName = Path.GetFileNameWithoutExtension(promptFile);
            ConsoleHelpers.WriteLine($"{indent}/{promptName}");
        }
    }

    /// <summary>
    /// Displays information about saved prompt files.
    /// </summary>
    /// <param name="filesSaved">List of saved files</param>
    public static void DisplaySavedPromptFiles(List<string> filesSaved)
    {
        var firstFileSaved = filesSaved.First();
        var additionalFiles = filesSaved.Skip(1).ToList();

        ConsoleHelpers.WriteLine($"Saved: {firstFileSaved}\n");

        var hasAdditionalFiles = additionalFiles.Any();
        if (hasAdditionalFiles)
        {
            foreach (var additionalFile in additionalFiles)
            {
                ConsoleHelpers.WriteLine($"  and: {additionalFile}");
            }
         
            ConsoleHelpers.WriteLine();
        }

        var promptName = Path.GetFileNameWithoutExtension(firstFileSaved);
        ConsoleHelpers.WriteLine($"USAGE: /{promptName} (in chat)");
    }
}