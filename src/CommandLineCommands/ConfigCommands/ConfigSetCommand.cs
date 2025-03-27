using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

class ConfigSetCommand : ConfigBaseCommand
{
    public string? Key { get; set; }
    public string? Value { get; set; }

    public override string GetCommandName()
    {
        return "config set";
    }

    public List<Task<int>> Execute(bool interactive)
    {
        var tasks = new List<Task<int>>();
        var scope = GetConfigScope();
        
        // Cannot set to 'Any' scope
        if (scope == ConfigFileScope.Any)
        {
            throw new CommandLineException("Error: Cannot set configuration to 'Any' scope. Please specify --global, --user, or use local scope (default).");
        }
        
        tasks.Add(Task.FromResult(ExecuteSet(Key, Value, scope)));
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
            ConfigDisplayHelpers.DisplayList(key, listValue);
        }
        else
        {
            _configStore.Set(key, value, scope, true);
            ConsoleHelpers.WriteLine($"{key}: {value}", overrideQuiet: true);
        }

        return 0;
    }
}