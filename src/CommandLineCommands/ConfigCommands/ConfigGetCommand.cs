using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

class ConfigGetCommand : ConfigBaseCommand
{
    public string? Key { get; set; }

    public override string GetCommandName()
    {
        return "config get";
    }

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

        var value = scope == ConfigFileScope.Any
            ? _configStore.GetFromAnyScope(key)
            : _configStore.GetFromScope(key, scope);

        ConfigDisplayHelpers.DisplayConfigValue(key, value);
        return 0;
    }
}