using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

class ConfigListCommand : ConfigBaseCommand
{
    public override string GetCommandName()
    {
        return "config list";
    }

    public ConfigListCommand() : base()
    {
        // Default to Any scope, which shows settings from all scopes
        Scope = ConfigFileScope.Any;
    }

    public List<Task<int>> Execute(bool interactive)
    {
        var tasks = new List<Task<int>>();
        tasks.Add(Task.FromResult(ExecuteList(GetConfigScope())));
        return tasks;
    }

    private int ExecuteList(ConfigFileScope scope)
    {
        if (scope != ConfigFileScope.Any)
        {
            Console.WriteLine($"LOCATION: {ConfigFileHelpers.GetLocationDisplayName(scope)}\n");
            var config = _configStore.ListScope(scope);
            ConfigDisplayHelpers.DisplayConfigSettings(config);
            return 0;
        }
        
        // Global scope
        Console.WriteLine($"LOCATION: {ConfigFileHelpers.GetLocationDisplayName(ConfigFileScope.Global)}\n");
        var globalConfig = _configStore.ListScope(ConfigFileScope.Global);
        ConfigDisplayHelpers.DisplayConfigSettings(globalConfig);
        Console.WriteLine();
        
        // User scope
        Console.WriteLine($"LOCATION: {ConfigFileHelpers.GetLocationDisplayName(ConfigFileScope.User)}\n");
        var userConfig = _configStore.ListScope(ConfigFileScope.User);
        ConfigDisplayHelpers.DisplayConfigSettings(userConfig);
        Console.WriteLine();
        
        // Local scope
        Console.WriteLine($"LOCATION: {ConfigFileHelpers.GetLocationDisplayName(ConfigFileScope.Local)}\n");
        var localConfig = _configStore.ListScope(ConfigFileScope.Local);
        ConfigDisplayHelpers.DisplayConfigSettings(localConfig);
        
        return 0;
    }
}