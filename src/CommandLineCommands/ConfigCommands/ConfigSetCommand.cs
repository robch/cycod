using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

/// <summary>
/// Implements the 'config set' command for setting configuration values.
/// </summary>
class ConfigSetCommand : ConfigBaseCommand
{
    public string? Key { get; set; }
    public string? Value { get; set; }

    /// <summary>
    /// Gets the name of the command.
    /// </summary>
    public override string GetCommandName()
    {
        return "config set";
    }

    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="interactive">Whether the command is run in interactive mode.</param>
    /// <returns>A list of tasks representing the execution of the command.</returns>
    public List<Task<int>> Execute(bool interactive)
    {
        var tasks = new List<Task<int>>();
        tasks.Add(Task.FromResult(ExecuteSet(Key, Value, GetConfigScope())));
        return tasks;
    }

    private int ExecuteSet(string? key, string? value, ConfigFileScope scope)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new CommandLineException($"Error: No key specified.");
        }

        if (value == null)
        {
            throw new CommandLineException($"Error: No value specified.");
        }

        // Try to parse as a list if the value is enclosed in brackets
        if (value.StartsWith("[") && value.EndsWith("]"))
        {
            var listContent = value.Substring(1, value.Length - 2);
            var listValue = new List<string>();
            
            if (!string.IsNullOrWhiteSpace(listContent))
            {
                var items = listContent.Split(',');
                foreach (var item in items)
                {
                    listValue.Add(item.Trim());
                }
            }
            
            _configStore.Set(key, listValue, scope, true);

            ConsoleHelpers.WriteLine(listValue.Count > 0
                ? $"{key}:\n" + $"  - {string.Join("\n  - ", listValue)}"
                : $"{key}: (empty list)",
                overrideQuiet: true);
        }
        else
        {
            _configStore.Set(key, value, scope, true);
            ConsoleHelpers.WriteLine($"{key}: {value}", overrideQuiet: true);
        }

        return 0;
    }
}