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
        Scope = ConfigFileScope.Any;
    }

    public List<Task<int>> Execute(bool interactive)
    {
        var tasks = new List<Task<int>>();
        tasks.Add(Task.FromResult(Scope == ConfigFileScope.FileName
            ? ExecuteList(this.ConfigFileName!)
            : ExecuteList(Scope ?? ConfigFileScope.Any)));
        return tasks;
    }

    private int ExecuteList(string configFileName)
    {
        DisplayConfigSettings(configFileName);
        return 0;
    }

    private int ExecuteList(ConfigFileScope scope)
    {
        var isAnyScope = scope == ConfigFileScope.Any;
        
        if (isAnyScope || scope == ConfigFileScope.Global)
        {
            DisplayConfigSettings(ConfigFileScope.Global);
            if (isAnyScope) Console.WriteLine();
        }

        if (isAnyScope || scope == ConfigFileScope.User)
        {
            DisplayConfigSettings(ConfigFileScope.User);
            if (isAnyScope) Console.WriteLine();
        }
        
        if (isAnyScope || scope == ConfigFileScope.User)
        {
            DisplayConfigSettings(ConfigFileScope.Local);
            if (isAnyScope) Console.WriteLine();
        }

        if (isAnyScope || scope == ConfigFileScope.FileName)
        {
            var fileNameToConfigValues = _configStore.ListFileNameScopeValues();
            foreach (var kvp in fileNameToConfigValues)
            {
                var location = $"{kvp.Key} (specified)";
                ConfigDisplayHelpers.DisplayConfigSettings(location, kvp.Value);
                Console.WriteLine();
            }
        }

        if (isAnyScope)
        {
            var commandLineValues = _configStore.ListFromCommandLineSettings();
            if (commandLineValues.Count > 0)
            {
                var location = "Command line (specified)";
                ConfigDisplayHelpers.DisplayConfigSettings(location, commandLineValues);
                Console.WriteLine();
            }
        }
        
        return 0;
    }

    private void DisplayConfigSettings(string fileName)
    {
        var location = $"{fileName} (specified)";
        ConfigDisplayHelpers.DisplayConfigSettings(location, _configStore.ListValuesFromFile(fileName));
    }

    private void DisplayConfigSettings(ConfigFileScope scope)
    {
        var location = ConfigFileHelpers.GetLocationDisplayName(scope);
        ConfigDisplayHelpers.DisplayConfigSettings(location, _configStore.ListValuesFromKnownScope(scope));
    }
}