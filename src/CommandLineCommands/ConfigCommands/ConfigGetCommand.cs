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

    private int ExecuteGet(string? key, ConfigScope scope)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new CommandLineException("Error: No key specified.");
        }

        var value = scope == ConfigScope.Project
            ? _configStore.Get(key) // Get from the highest priority scope
            : _configStore.GetFromScope(key, scope); // Get from the specified scope

        if (value.IsNullOrEmpty())
        {
            throw new CommandLineException($"Error: No value found for key '{key}' in {scope} scope.");
        }

        if (value.RawValue is List<string> listValue)
        {
            Console.WriteLine($"[{string.Join(", ", listValue)}]");
        }
        else
        {
            Console.WriteLine(value);
        }

        return 0;
    }
}