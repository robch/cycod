using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Implements the 'config clear' command for clearing configuration settings.
/// </summary>
class ConfigClearCommand : ConfigBaseCommand
{
    public string? Key { get; set; }

    /// <summary>
    /// Gets the name of the command.
    /// </summary>
    public override string GetCommandName()
    {
        return "config clear";
    }

    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="interactive">Whether the command is run in interactive mode.</param>
    /// <returns>A list of tasks representing the execution of the command.</returns>
    public List<Task<int>> Execute(bool interactive)
    {
        var tasks = new List<Task<int>>();
        tasks.Add(Task.FromResult(ExecuteClear(Key, GetConfigScope())));
        return tasks;
    }

    private int ExecuteClear(string? key, ConfigFileScope scope)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new CommandLineException($"Key cannot be null or empty.");
        }

        bool cleared = _configStore.Clear(key, scope, true);
        ConsoleHelpers.WriteLine(cleared
            ? $"{key}: (cleared)"
            : $"{key}: (not found)",
            overrideQuiet: true);

        return 0;
    }
}