using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public static class AliasFileHelpers
{
    /// <summary>
    /// Finds the alias directory in the current or parent directories
    /// </summary>
    /// <param name="create">Whether to create the directory if it doesn't exist</param>
    /// <returns>The path to the alias directory, or null if not found</returns>
    public static string? FindAliasDirectory(bool create = false)
    {
        return create
            ? DirectoryHelpers.FindOrCreateDirectorySearchParents(".chatx", "aliases")
            : DirectoryHelpers.FindDirectorySearchParents(".chatx", "aliases");
    }

    /// <summary>
    /// Saves an alias with the provided options to a file
    /// </summary>
    /// <param name="aliasName">Name of the alias</param>
    /// <param name="allOptions">Command-line options to save in the alias</param>
    /// <returns>List of saved file paths</returns>
    public static List<string> SaveAlias(string aliasName, string[] allOptions)
    {
        var filesSaved = new List<string>();
        var aliasDirectory = FindAliasDirectory(create: true)!;
        var fileName = Path.Combine(aliasDirectory, aliasName + ".alias");

        var options = allOptions
            .Where(x => x != "--save-alias" && x != aliasName)
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
        var aliasDirectory = FindAliasDirectory(create: false) ?? ".";
        var aliasFilePath = Path.Combine(aliasDirectory, $"{alias}.alias");

        if (File.Exists(aliasFilePath))
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

    /// <summary>
    /// Displays information about saved alias files
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