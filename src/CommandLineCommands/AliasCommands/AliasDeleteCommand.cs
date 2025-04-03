using System;
using System.IO;
using System.Threading.Tasks;

/// <summary>
/// Command to delete an alias.
/// </summary>
class AliasDeleteCommand : AliasBaseCommand
{
    /// <summary>
    /// The name of the alias to delete.
    /// </summary>
    public string? AliasName { get; set; }

    /// <summary>
    /// Constructor initializes the base command.
    /// </summary>
    public AliasDeleteCommand() : base()
    {
        Scope = ConfigFileScope.Any;
    }

    /// <summary>
    /// Checks if the command is empty (i.e., no alias name provided).
    /// </summary>
    /// <returns>True if empty, false otherwise.</returns>
    public override bool IsEmpty()
    {
        return string.IsNullOrWhiteSpace(AliasName);
    }

    /// <summary>
    /// Gets the command name.
    /// </summary>
    /// <returns>The command name.</returns>
    public override string GetCommandName()
    {
        return "alias delete";
    }

    /// <summary>
    /// Execute the delete command.
    /// </summary>
    /// <param name="interactive">Whether the command is running in interactive mode.</param>
    /// <returns>Exit code, 0 for success, non-zero for failure.</returns>
    public Task<int> Execute(bool interactive)
    {
        if (string.IsNullOrWhiteSpace(AliasName))
        {
            ConsoleHelpers.WriteErrorLine("Error: Alias name is required.");
            return Task.FromResult(1);
        }

        var result = ExecuteDelete(AliasName, Scope ?? ConfigFileScope.Any);
        return Task.FromResult(result);
    }

    /// <summary>
    /// Execute the delete command for the specified alias name and scope.
    /// </summary>
    /// <param name="aliasName">The name of the alias to delete.</param>
    /// <param name="scope">The scope to look in.</param>
    /// <returns>Exit code, 0 for success, non-zero for failure.</returns>
    private int ExecuteDelete(string aliasName, ConfigFileScope scope)
    {
        string? aliasFilePath = null;

        if (scope == ConfigFileScope.Any)
        {
            aliasFilePath = AliasFileHelpers.FindAliasFile(aliasName);
        }
        else
        {
            var aliasDir = AliasFileHelpers.FindAliasDirectoryInScope(scope);
            if (aliasDir != null)
            {
                var potentialFilePath = Path.Combine(aliasDir, $"{aliasName}.alias");
                if (File.Exists(potentialFilePath))
                {
                    aliasFilePath = potentialFilePath;
                }
            }
        }

        if (aliasFilePath == null || !File.Exists(aliasFilePath))
        {
            ConsoleHelpers.WriteErrorLine($"Error: Alias '{aliasName}' not found in specified scope.");
            return 1;
        }

        // Check for additional files that might be referenced (multiline content)
        var directory = Path.GetDirectoryName(aliasFilePath);
        var fileBase = Path.GetFileNameWithoutExtension(aliasFilePath);
        var fileExt = Path.GetExtension(aliasFilePath);
        var additionalFilePattern = $"{fileBase}-*{fileExt}";

        try
        {
            // Delete the main alias file
            File.Delete(aliasFilePath);
            ConsoleHelpers.WriteLine($"Deleted: {aliasFilePath}");

            // Delete any additional files if they exist
            if (directory != null)
            {
                var additionalFiles = Directory.GetFiles(directory, additionalFilePattern);
                foreach (var additionalFile in additionalFiles)
                {
                    File.Delete(additionalFile);
                    ConsoleHelpers.WriteLine($"Deleted: {additionalFile}");
                }
            }

            return 0;
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteErrorLine($"Error deleting alias: {ex.Message}");
            return 1;
        }
    }
}