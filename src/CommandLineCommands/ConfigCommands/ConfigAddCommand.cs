using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Implements the 'config add' command for adding values to list settings.
/// </summary>
class ConfigAddCommand : ConfigBaseCommand
{
    public string? Key { get; set; }
    public string? Value { get; set; }

    /// <summary>
    /// Gets the name of the command.
    /// </summary>
    public override string GetCommandName()
    {
        return "config add";
    }

    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="interactive">Whether the command is run in interactive mode.</param>
    /// <returns>A list of tasks representing the execution of the command.</returns>
    public List<Task<int>> Execute(bool interactive)
    {
        var tasks = new List<Task<int>>();
        tasks.Add(Task.FromResult(ExecuteAdd(Key, Value, GetConfigScope())));
        return tasks;
    }

    private int ExecuteAdd(string? key, string? value, ConfigScope scope)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new CommandLineException($"Error: No key specified.");
        }

        if (value == null)
        {
            throw new CommandLineException($"Error: No value specified.");
        }

        bool added = _configStore.AddToList(key, value, scope, true);
        if (added)
        {
            Console.WriteLine($"Added '{value}' to '{key}' in {scope} configuration.");
        }
        else
        {
            Console.WriteLine($"Value '{value}' already exists in '{key}' in {scope} configuration.");
        }

        return 0;
    }
}