using System;
using System.IO;
using System.Threading.Tasks;

/// <summary>
/// Command to view the content of a specific alias.
/// </summary>
class AliasGetCommand : AliasBaseCommand
{
    /// <summary>
    /// The name of the alias to get.
    /// </summary>
    public string? AliasName { get; set; }

    /// <summary>
    /// Constructor initializes the base command.
    /// </summary>
    public AliasGetCommand() : base()
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
        return "alias get";
    }

    /// <summary>
    /// Execute the get command.
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

        var result = ExecuteGet(AliasName, Scope ?? ConfigFileScope.Any);
        return Task.FromResult(result);
    }

    /// <summary>
    /// Execute the get command for the specified alias name and scope.
    /// </summary>
    /// <param name="aliasName">The name of the alias to get.</param>
    /// <param name="scope">The scope to look in.</param>
    /// <returns>Exit code, 0 for success, non-zero for failure.</returns>
    private int ExecuteGet(string aliasName, ConfigFileScope scope)
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

        // Determine the scope from the file path
        var fileScope = ConfigFileScope.Local; // Default
        if (aliasFilePath.Contains(ConfigFileHelpers.GetScopeDirectoryPath(ConfigFileScope.Global) ?? ""))
        {
            fileScope = ConfigFileScope.Global;
        }
        else if (aliasFilePath.Contains(ConfigFileHelpers.GetScopeDirectoryPath(ConfigFileScope.User) ?? ""))
        {
            fileScope = ConfigFileScope.User;
        }

        ConsoleHelpers.WriteLine($"ALIAS: {aliasName}");
        ConsoleHelpers.WriteLine($"LOCATION: {aliasFilePath} ({fileScope.ToString().ToLowerInvariant()})");
        Console.WriteLine();

        // Read and display the alias content
        var content = File.ReadAllText(aliasFilePath);
        ConsoleHelpers.WriteLine(content);
        Console.WriteLine();
        
        // Show usage example
        ConsoleHelpers.WriteLine($"USAGE: {Program.Name} [...] --{aliasName}");

        return 0;
    }
}