using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Implements the 'config get' command for retrieving configuration settings.
/// </summary>
class ConfigGetCommand : ConfigBaseCommand
{
    public string? Key { get; set; }

    /// <summary>
    /// Gets the name of the command.
    /// </summary>
    public override string GetCommandName()
    {
        return "config get";
    }

    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="interactive">Whether the command is run in interactive mode.</param>
    /// <returns>A list of tasks representing the execution of the command.</returns>
    public List<Task<int>> Execute(bool interactive)
    {
        var tasks = new List<Task<int>>();
        tasks.Add(Task.FromResult(ExecuteGet(Key, GetConfigScope())));
        return tasks;
    }

    private int ExecuteGet(string? key, ConfigFileScope scope)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new CommandLineException("Error: No key specified.");
        }

        var value = scope == ConfigFileScope.Local
            ? _configStore.GetFromAnyScope(key) // Get from the highest priority scope
            : _configStore.GetFromScope(key, scope); // Get from the specified scope

        if (value.IsNullOrEmpty())
        {
            throw new CommandLineException($"Error: No value found for key '{key}' in {scope} scope.");
        }

        if (value.Value is List<string> listValue)
        {
            ConsoleHelpers.WriteLine(listValue.Count > 0
                ? $"{key}:\n" + $"  - {string.Join("\n  - ", listValue)}"
                : $"{key}: (empty list)",
                overrideQuiet: true);
        }
        else
        {
            ConsoleHelpers.WriteLine($"{key}: {value.Value}", overrideQuiet: true);
        }

        return 0;
    }
}