using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

class ConfigGetCommand : ConfigBaseCommand
{
    public string? Key { get; set; }

    public override bool IsEmpty()
    {
        return string.IsNullOrWhiteSpace(Key);
    }

    public override string GetCommandName()
    {
        return "config get";
    }

    public Task<int> Execute(bool interactive)
    {
        var result = ExecuteGet(Key, Scope ?? ConfigFileScope.Any, ConfigFileName);
        return Task.FromResult(result);
    }

    private int ExecuteGet(string? key, ConfigFileScope scope, string? configFileName)
    {
        ConsoleHelpers.WriteDebugLine($"ExecuteGet; key: {key}; scope: {scope}; configFileName: {configFileName}");
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new CommandLineException("Error: No key specified.");
        }

        var isFileNameScope = scope == ConfigFileScope.FileName && !string.IsNullOrEmpty(configFileName);
        var value = isFileNameScope
            ? _configStore.GetFromFileName(key, configFileName!)
            : _configStore.GetFromScope(key, scope);

        ConfigDisplayHelpers.DisplayConfigValue(key, value, showLocation: true);
        return 0;
    }
}