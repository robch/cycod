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

        ConfigValue value;
        
        if (scope == ConfigFileScope.Any)
        {
            // When using Any scope, get from the highest priority scope
            value = _configStore.GetFromAnyScope(key);
        }
        else
        {
            // When using a specific scope, get only from that scope
            value = _configStore.GetFromScope(key, scope);
        }

        if (value.IsNullOrEmpty())
        {
            throw new CommandLineException($"Error: No value found for key '{key}'" + 
                (scope != ConfigFileScope.Any ? $" in {scope} scope." : "."));
        }

        ConfigDisplayHelpers.DisplayConfigValue(key, value);
        return 0;
    }
}