using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Implements the 'config list' command for listing configuration settings.
/// </summary>
class ConfigListCommand : ConfigBaseCommand
{
    /// <summary>
    /// Gets the name of the command.
    /// </summary>
    public override string GetCommandName()
    {
        return "config list";
    }

    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="interactive">Whether the command is run in interactive mode.</param>
    /// <returns>A list of tasks representing the execution of the command.</returns>
    public List<Task<int>> Execute(bool interactive)
    {
        var tasks = new List<Task<int>>();
        tasks.Add(Task.FromResult(ExecuteList(GetConfigScope())));;
        return tasks;
    }

    private int ExecuteList(ConfigFileScope scope)
    {
        Dictionary<string, ConfigValue> config;
        
        if (scope == ConfigFileScope.Local)
        {
            // When listing with no scope flag, show merged configuration
            Console.WriteLine("Listing all configuration settings (merged from all scopes):");
            config = _configStore.ListAll();
        }
        else
        {
            // When listing with a scope flag, show only that scope
            Console.WriteLine($"Listing {scope} configuration settings:");
            config = _configStore.ListScope(scope);
        }

        if (config.Count == 0)
        {
            Console.WriteLine("No configuration settings found.");
            return 0;
        }

        // Sort the keys for consistent output
        var sortedKeys = new List<string>(config.Keys);
        sortedKeys.Sort();

        foreach (var key in sortedKeys)
        {
            var value = config[key];
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
        }

        return 0;
    }
}