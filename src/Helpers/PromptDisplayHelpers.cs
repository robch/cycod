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
            return CommonDisplayHelpers.FormatLocationHeader(promptPath, scope);
        }
        
        var scopeDir = ConfigFileHelpers.GetScopeDirectoryPath(scope);
        if (scopeDir == null) return null;

        return CommonDisplayHelpers.FormatLocationHeader(Path.Combine(scopeDir, "prompts"), scope);
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

        var promptNames = promptFiles
            .Select(file => Path.GetFileNameWithoutExtension(file))
            .ToList();

        // Display the prompts with location header
        CommonDisplayHelpers.DisplayItemList(locationDisplay, promptNames);
    }

    /// <summary>
    /// Displays a single prompt with content preview.
    /// </summary>
    /// <param name="name">Name of the prompt</param>
    /// <param name="promptFilePath">File path of the prompt</param>
    /// <param name="scope">Scope of the prompt</param>
    /// <param name="content">Content of the prompt</param>
    public static void DisplayPrompt(string name, string promptFilePath, ConfigFileScope scope, string content)
    {
        var location = CommonDisplayHelpers.FormatLocationHeader(promptFilePath, scope);
        CommonDisplayHelpers.DisplayItem(name, content, location, limitValue: true);
    }

    /// <summary>
    /// Displays information about saved prompt files.
    /// </summary>
    /// <param name="filesSaved">List of saved files</param>
    public static void DisplaySavedPromptFile(string fileName)
    {
        var wrappedList = new List<string> { fileName };
        CommonDisplayHelpers.DisplaySavedFiles(wrappedList, "/{name} (in chat)");
    }
}