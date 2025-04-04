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
            return CommonDisplayHelpers.FormatLocationHeader(aliasPath, scope);
        }
        
        var scopeDir = ConfigFileHelpers.GetScopeDirectoryPath(scope);
        if (scopeDir == null) return null;

        return CommonDisplayHelpers.FormatLocationHeader(Path.Combine(scopeDir, "aliases"), scope);
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

        var aliasNames = aliasFiles
            .Select(file => Path.GetFileNameWithoutExtension(file))
            .ToList();

        // Display the aliases with location header
        CommonDisplayHelpers.DisplayItemList(locationDisplay, aliasNames);
    }

    /// <summary>
    /// Displays a single alias with content.
    /// </summary>
    /// <param name="name">Name of the alias</param>
    /// <param name="fileName">File path of the alias</param>
    /// <param name="scope">Scope of the alias</param>
    /// <param name="content">Content of the alias</param>
    public static void DisplayAlias(string name, string content, string fileName, ConfigFileScope scope)
    {
        var location = CommonDisplayHelpers.FormatLocationHeader(fileName, scope);
        CommonDisplayHelpers.DisplayItem(name, content, location);
    }

    /// <summary>
    /// Displays a list of alias files.
    /// </summary>
    /// <param name="aliasFiles">The list of alias files to display</param>
    /// <param name="indentLevel">The level of indentation for display</param>
    public static void DisplayAliasFiles(List<string> aliasFiles, int indentLevel = CommonDisplayHelpers.DefaultIndentLevel)
    {
        var indent = new string(' ', indentLevel);
        
        if (aliasFiles.Count == 0)
        {
            ConsoleHelpers.WriteLine($"{indent}No aliases found in this scope", overrideQuiet: true);
            return;
        }

        foreach (var aliasFile in aliasFiles)
        {
            var aliasName = Path.GetFileNameWithoutExtension(aliasFile);
            ConsoleHelpers.WriteLine($"{indent}{aliasName}", overrideQuiet: true);
        }
    }

    /// <summary>
    /// Displays information about saved alias files.
    /// </summary>
    /// <param name="filesSaved">List of saved files</param>
    public static void DisplaySavedAliasFiles(List<string> filesSaved)
    {
        CommonDisplayHelpers.DisplaySavedFiles(filesSaved, $"{Program.Name} [...] --{{name}}");
    }
}