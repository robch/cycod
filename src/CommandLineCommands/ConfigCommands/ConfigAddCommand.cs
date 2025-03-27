using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class ConfigAddCommand : ConfigBaseCommand
{
    public string? Key { get; set; }
    public string? Value { get; set; }

    public override string GetCommandName()
    {
        return "config add";
    }

    public List<Task<int>> Execute(bool interactive)
    {
        var tasks = new List<Task<int>>();
        tasks.Add(Task.FromResult(ExecuteAdd(Key, Value, GetConfigScope())));
        return tasks;
    }

    private int ExecuteAdd(string? key, string? value, ConfigFileScope scope)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new CommandLineException($"Error: No key specified.");
        }

        if (value == null)
        {
            throw new CommandLineException($"Error: No value specified.");
        }

        _configStore.AddToList(key, value, scope, true);

        var listValue = _configStore.GetFromScope(key, scope).AsList();
        ConsoleHelpers.WriteLine(listValue.Count > 0
            ? $"{key}:\n" + $"- {string.Join("\n- ", listValue)}"
            : $"{key}: (empty list)",
            overrideQuiet: true);

        return 0;
    }
}