using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Command to list all available aliases.
/// </summary>
class AliasListCommand : AliasBaseCommand
{
    /// <summary>
    /// Constructor initializes with default scope of Any.
    /// </summary>
    public AliasListCommand() : base()
    {
        Scope = ConfigFileScope.Any;
    }

    /// <summary>
    /// Gets the command name.
    /// </summary>
    /// <returns>The command name.</returns>
    public override string GetCommandName()
    {
        return "alias list";
    }

    /// <summary>
    /// Execute the list command.
    /// </summary>
    /// <param name="interactive">Whether the command is running in interactive mode.</param>
    /// <returns>Exit code, 0 for success.</returns>
    public Task<int> Execute(bool interactive)
    {
        var result = ExecuteList(Scope ?? ConfigFileScope.Any);
        return Task.FromResult(result);
    }

    /// <summary>
    /// Execute the list command for the specified scope.
    /// </summary>
    /// <param name="scope">The scope to list aliases for.</param>
    /// <returns>Exit code, 0 for success.</returns>
    private int ExecuteList(ConfigFileScope scope)
    {
        var isAnyScope = scope == ConfigFileScope.Any;

        if (isAnyScope || scope == ConfigFileScope.Global)
        {
            AliasDisplayHelpers.DisplayAliases(ConfigFileScope.Global);
            if (isAnyScope) Console.WriteLine();
        }

        if (isAnyScope || scope == ConfigFileScope.User)
        {
            AliasDisplayHelpers.DisplayAliases(ConfigFileScope.User);
            if (isAnyScope) Console.WriteLine();
        }

        if (isAnyScope || scope == ConfigFileScope.Local)
        {
            AliasDisplayHelpers.DisplayAliases(ConfigFileScope.Local);
        }

        return 0;
    }
}