using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public static class AliasFileHelpers
{
    /// <summary>
    /// Finds the alias directory in the specified scope.
    /// </summary>
    /// <param name="scope">The scope to search in</param>
    /// <param name="create">Whether to create the directory if it doesn't exist</param>
    /// <returns>The path to the alias directory, or null if not found</returns>
    public static string? FindAliasDirectoryInScope(ConfigFileScope scope, bool create = false)
    {
        return create
            ? ScopeFileHelpers.EnsureDirectoryInScope("aliases", scope)
            : ScopeFileHelpers.FindDirectoryInScope("aliases", scope);
    }

    /// <summary>
    /// Saves an alias with the provided options to a file in the specified scope.
    /// </summary>
    /// <param name="aliasName">Name of the alias</param>
    /// <param name="allOptions">Command-line options to save in the alias</param>
    /// <param name="scope">The scope to save the alias to</param>
    /// <returns>List of saved file paths</returns>
    public static List<string> SaveAlias(string aliasName, string[] allOptions, ConfigFileScope scope = ConfigFileScope.Local)
    {
        var filesSaved = new List<string>();
        
        // Filter options to exclude scope-related save options
        var optionsToFilter = new[] { 
            "--save-alias", 
            "--save-local-alias", 
            "--save-user-alias", 
            "--save-global-alias", 
            aliasName 
        };
        
        var aliasDirectory = FindAliasDirectoryInScope(scope, create: true)!;
        var fileName = Path.Combine(aliasDirectory, aliasName + ".alias");

        var options = allOptions
            .Where(x => !optionsToFilter.Contains(x))
            .Select(x => SingleLineOrNewAtFile(x, fileName, ref filesSaved));

        var asMultiLineString = string.Join('\n', options);
        FileHelpers.WriteAllText(fileName, asMultiLineString);

        filesSaved.Insert(0, fileName);
        return filesSaved;
    }

    /// <summary>
    /// Handles multiline text by saving it to a separate file if needed
    /// </summary>
    /// <param name="text">The text to process</param>
    /// <param name="baseFileName">Base file name for additional files</param>
    /// <param name="additionalFiles">Reference to list of additional files</param>
    /// <returns>Single line text or @filename reference</returns>
    private static string SingleLineOrNewAtFile(string text, string baseFileName, ref List<string> additionalFiles)
    {
        var isMultiLine = text.Contains('\n') || text.Contains('\r');
        if (!isMultiLine) return text;

        var additionalFileCount = additionalFiles.Count + 1;
        var additionalFileName = FileHelpers.GetFileNameFromTemplate(baseFileName, "{filepath}/{filebase}-" + additionalFileCount + "{fileext}")!;
        additionalFiles.Add(additionalFileName);

        FileHelpers.WriteAllText(additionalFileName, text);

        return "@" + additionalFileName;
    }
    /// <summary>
    /// Finds an alias file across all scopes (local, user, global) with optional parent directory search.
    /// </summary>
    /// <param name="aliasName">The name of the alias to find</param>
    /// <returns>The full path to the alias file if found, null otherwise</returns>
    public static string? FindAliasFile(string aliasName)
    {
        var aliasFileName = $"{aliasName}.alias";
        var aliasFilePath = ScopeFileHelpers.FindFileInAnyScope(aliasFileName, "aliases", searchParents: true);

        ConsoleHelpers.WriteDebugLine(aliasFilePath != null
            ? $"FindAliasFile; found alias in scope: {aliasFilePath}"
            : $"FindAliasFile; alias NOT FOUND in any scope: {aliasName}");
        return aliasFilePath;
    }

    public static string? FindAliasInScope(string aliasName, ConfigFileScope scope)
    {
        var aliasFileName = $"{aliasName}.alias";
        var aliasFilePath = ScopeFileHelpers.FindFileInScope(aliasFileName, "aliases", scope);

        ConsoleHelpers.WriteDebugLine(aliasFilePath != null
            ? $"FindAliasInScope; found alias in scope: {aliasFilePath}"
            : $"FindAliasInScope; alias NOT FOUND in scope: {aliasName}");
        return aliasFilePath;
    }

    /// <summary>
    /// Attempts to parse an alias from the command line
    /// </summary>
    /// <param name="commandLineOptions">Command line options object to update</param>
    /// <param name="command">Reference to the current command</param>
    /// <param name="args">The command line arguments</param>
    /// <param name="currentIndex">Current index in the args array</param>
    /// <param name="alias">The alias name</param>
    /// <returns>True if alias was found and parsed, false otherwise</returns>
    public static bool TryParseAliasOptions(CommandLineOptions commandLineOptions, ref Command? command, string[] args, ref int currentIndex, string alias)
    {
        var aliasFilePath = FindAliasFile(alias);
        if (aliasFilePath != null && File.Exists(aliasFilePath))
        {
            var aliasArgs = File.ReadAllLines(aliasFilePath);
            for (var j = 0; j < aliasArgs.Length; j++)
            {
                var parsed = CommandLineOptions.TryParseInputOptions(commandLineOptions, ref command, aliasArgs, ref j, aliasArgs[j]);
                if (!parsed)
                {
                    throw new CommandLineException($"Invalid argument in alias file: {aliasArgs[j]}");
                }
            }
            return true;
        }
        return false;
    }
}