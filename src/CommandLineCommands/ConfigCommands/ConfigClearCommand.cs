using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class ConfigClearCommand : ConfigBaseCommand
{
    public string? Key { get; set; }

    public override string GetCommandName()
    {
        return "config clear";
    }

    public List<Task<int>> Execute(bool interactive)
    {
        var tasks = new List<Task<int>>();
        tasks.Add(Task.FromResult(ExecuteClear(Key, Scope ?? ConfigFileScope.Local, ConfigFileName)));
        return tasks;
    }

    private int ExecuteClear(string? key, ConfigFileScope scope, string? configFileName)
    {
        ConsoleHelpers.WriteDebugLine($"ExecuteClear; key: {key}; scope: {scope}; configFileName: {configFileName}");
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new CommandLineException($"Key cannot be null or empty.");
        }

        var isFileNameScope = scope == ConfigFileScope.FileName && !string.IsNullOrEmpty(configFileName);
        bool cleared = isFileNameScope
            ? _configStore.Clear(key, configFileName!)
            : _configStore.Clear(key, scope, true);

        ConsoleHelpers.WriteLine(cleared
            ? $"{key}: (cleared)"
            : $"{key}: (not found)",
            overrideQuiet: true);

        return 0;
    }
}