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
    public override async Task<object> ExecuteAsync(bool interactive)
    {
        return await Task.Run(() => 
        {
            if (string.IsNullOrWhiteSpace(AliasName))
            {
                ConsoleHelpers.WriteErrorLine("Error: Alias name is required.");
                return 1;
            }

            return ExecuteGet(AliasName, Scope ?? ConfigFileScope.Any);
        });
    }

    /// <summary>
    /// Execute the get command for the specified alias name and scope.
    /// </summary>
    /// <param name="aliasName">The name of the alias to get.</param>
    /// <param name="scope">The scope to look in.</param>
    /// <returns>Exit code, 0 for success, non-zero for failure.</returns>
    private int ExecuteGet(string aliasName, ConfigFileScope scope)
    {
        ConsoleHelpers.WriteDebugLine($"ExecuteGet; aliasName: {aliasName}; scope: {scope}");

        var isAnyScope = scope == ConfigFileScope.Any;
        var aliasFilePath = isAnyScope
            ? AliasFileHelpers.FindAliasFile(aliasName)
            : AliasFileHelpers.FindAliasInScope(aliasName, scope);

        var fileNotFound = aliasFilePath == null || !File.Exists(aliasFilePath);
        if (fileNotFound)
        {
            ConsoleHelpers.WriteErrorLine(isAnyScope
                ? $"Error: Alias '{aliasName}' not found in any scope."
                : $"Error: Alias '{aliasName}' not found in specified scope.");
            return 1;
        }
        
        // Display the alias using the standardized method
        var content = FileHelpers.ReadAllText(aliasFilePath!);
        var foundInScope = ScopeFileHelpers.GetScopeFromPath(aliasFilePath!);
        AliasDisplayHelpers.DisplayAlias(aliasName, content, aliasFilePath!, foundInScope);

        return 0;
    }
}