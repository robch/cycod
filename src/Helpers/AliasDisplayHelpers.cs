using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Provides methods for displaying alias information.
/// </summary>
public static class AliasDisplayHelpers
{
    /// <summary>
    /// Gets a display name for the alias directory location in the specified scope.
    /// </summary>
    /// <param name="scope">The scope to get the location display name for</param>
    /// <returns>A string representing the location, or null if the scope is invalid</returns>
    public static string? GetLocationDisplayName(ConfigFileScope scope)
    {
        var aliasPath = AliasFileHelpers.FindAliasDirectoryInScope(scope);
        if (aliasPath != null)
        {
            return $"{aliasPath} ({scope.ToString().ToLower()})";
        }
        
        var scopeDir = ConfigFileHelpers.GetScopeDirectoryPath(scope);
        if (scopeDir == null) return null;

        return $"{Path.Combine(scopeDir, "aliases")} ({scope.ToString().ToLower()})";
    }

    /// <summary>
    /// Displays aliases for a specific scope.
    /// </summary>
    /// <param name="scope">The scope to display aliases for</param>
    public static void DisplayAliases(ConfigFileScope scope)
    {
        // Get the location display name
        var locationDisplay = GetLocationDisplayName(scope);
        if (locationDisplay == null) return;
        
        ConsoleHelpers.WriteLine($"LOCATION: {locationDisplay}");
        Console.WriteLine();

        // Find the directory to check for aliases
        var aliasDir = AliasFileHelpers.FindAliasDirectoryInScope(scope);
        
        // Check for aliases only if the directory exists
        var aliasFiles = new List<string>();
        if (aliasDir != null && Directory.Exists(aliasDir))
        {
            aliasFiles = Directory.GetFiles(aliasDir, "*.alias")
                .OrderBy(file => Path.GetFileNameWithoutExtension(file))
                .ToList();
        }

        DisplayAliasFiles(aliasFiles);
    }

    /// <summary>
    /// Displays a list of alias files.
    /// </summary>
    /// <param name="aliasFiles">The list of alias files to display</param>
    /// <param name="indentLevel">The level of indentation for display</param>
    public static void DisplayAliasFiles(List<string> aliasFiles, int indentLevel = 2)
    {
        var indent = new string(' ', indentLevel);
        
        if (aliasFiles.Count == 0)
        {
            ConsoleHelpers.WriteLine($"{indent}No aliases found in this scope");
            return;
        }

        foreach (var aliasFile in aliasFiles)
        {
            var aliasName = Path.GetFileNameWithoutExtension(aliasFile);
            ConsoleHelpers.WriteLine($"{indent}{aliasName}");
        }
    }

    /// <summary>
    /// Displays information about saved alias files.
    /// </summary>
    /// <param name="filesSaved">List of saved files</param>
    public static void DisplaySavedAliasFiles(List<string> filesSaved)
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

        var aliasName = Path.GetFileNameWithoutExtension(firstFileSaved);
        ConsoleHelpers.WriteLine($"USAGE: {Program.Name} [...] --" + aliasName);
    }
}