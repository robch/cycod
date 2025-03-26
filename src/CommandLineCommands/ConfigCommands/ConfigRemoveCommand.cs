using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Implements the 'config remove' command for removing values from list settings.
/// </summary>
class ConfigRemoveCommand : ConfigBaseCommand
{
    public string? Key { get; set; }
    public string? Value { get; set; }

    /// <summary>
    /// Gets the name of the command.
    /// </summary>
    public override string GetCommandName()
    {
        return "config remove";
    }

    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="interactive">Whether the command is run in interactive mode.</param>
    /// <returns>A list of tasks representing the execution of the command.</returns>
    public List<Task<int>> Execute(bool interactive)
    {
        var tasks = new List<Task<int>>();
        tasks.Add(Task.FromResult(ExecuteRemove(Key, Value, GetConfigScope())));
        return tasks;
    }

    private int ExecuteRemove(string? key, string? value, ConfigScope scope)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new CommandLineException($"Error: No key specified.");
        }

        if (value == null)
        {
            throw new CommandLineException($"Error: No value specified.");
        }

        bool removed = _configStore.RemoveFromList(key, value, scope, true);
        if (removed)
        {
            Console.WriteLine($"Removed '{value}' from '{key}' in {scope} configuration.");
        }
        else
        {
            Console.Error.WriteLine($"Value '{value}' not found in '{key}' in {scope} configuration.");
        }

        return 0;
    }
}