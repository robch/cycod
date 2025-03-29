using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class ConfigRemoveCommand : ConfigBaseCommand
{
    public string? Key { get; set; }
    public string? Value { get; set; }

    public override string GetCommandName()
    {
        return "config remove";
    }

    public List<Task<int>> Execute(bool interactive)
    {
        var tasks = new List<Task<int>>();
        tasks.Add(Task.FromResult(ExecuteRemove(Key, Value, Scope ?? ConfigFileScope.Local, ConfigFileName)));
        return tasks;
    }

    private int ExecuteRemove(string? key, string? value, ConfigFileScope scope, string? configFileName)
    {
        ConsoleHelpers.WriteDebugLine($"ExecuteRemove; key: {key}; value: {value}; scope: {scope}; configFileName: {configFileName}");
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new CommandLineException($"Error: No key specified.");
        }

        if (value == null)
        {
            throw new CommandLineException($"Error: No value specified.");
        }

        var isFileNameScope = scope == ConfigFileScope.FileName && !string.IsNullOrEmpty(configFileName);
        if (isFileNameScope) _configStore.RemoveFromList(key, value, configFileName!);
        if (!isFileNameScope) _configStore.RemoveFromList(key, value, scope, true);

        var listValue = isFileNameScope
            ? _configStore.GetFromFileName(key, configFileName!).AsList()
            : _configStore.GetFromScope(key, scope).AsList();

        ConsoleHelpers.WriteLine(listValue.Count > 0
            ? $"{key}:\n" + $"  - {string.Join("\n  - ", listValue)}"
            : $"{key}: (empty list)",
            overrideQuiet: true);

        return 0;
    }
}