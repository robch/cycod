using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class ConfigAddCommand : ConfigBaseCommand
{
    public string? Key { get; set; }
    public string? Value { get; set; }

    public override bool IsEmpty()
    {
        return string.IsNullOrWhiteSpace(Key) || Value == null;
    }

    public override string GetCommandName()
    {
        return "config add";
    }

    public override async Task<int> ExecuteAsync(bool interactive)
    {
        return await Task.Run(() => ExecuteAdd(Key, Value, Scope ?? ConfigFileScope.Local, ConfigFileName));
    }

    private int ExecuteAdd(string? key, string? value, ConfigFileScope scope, string? configFileName)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new CommandLineException($"Error: No key specified.");
        if (value == null) throw new CommandLineException($"Error: No value specified.");

        var isFileNameScope = scope == ConfigFileScope.FileName && !string.IsNullOrEmpty(configFileName);
        if (isFileNameScope) _configStore.AddToList(key, value, configFileName!);
        if (!isFileNameScope) _configStore.AddToList(key, value, scope, true);

        var listValue = isFileNameScope
            ? _configStore.GetFromFileName(key, configFileName!).AsList()
            : _configStore.GetFromScope(key, scope).AsList();

        ConsoleHelpers.WriteLine(listValue.Count > 0
            ? $"{key}:\n" + $"- {string.Join("\n- ", listValue)}"
            : $"{key}: (empty list)",
            overrideQuiet: true);

        return 0;
    }
}