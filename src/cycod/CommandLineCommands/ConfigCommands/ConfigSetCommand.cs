using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

class ConfigSetCommand : ConfigBaseCommand
{
    public string? Key { get; set; }
    public string? Value { get; set; }

    public override bool IsEmpty()
    {
        return string.IsNullOrWhiteSpace(Key) || Value == null;
    }

    public override string GetCommandName()
    {
        return "config set";
    }

    public override async Task<int> ExecuteAsync(bool interactive)
    {
        return await Task.Run(() => ExecuteSet(Key, Value, Scope ?? ConfigFileScope.Local, ConfigFileName));
    }

    private int ExecuteSet(string? key, string? value, ConfigFileScope scope, string? configFileName)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new CommandLineException($"Error: No key specified.");
        if (value == null) throw new CommandLineException($"Error: No value specified.");

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
            
            var isFileNameScope = scope == ConfigFileScope.FileName && !string.IsNullOrEmpty(configFileName);
            if (isFileNameScope) _configStore.Set(key, listValue, configFileName!);
            if (!isFileNameScope) _configStore.Set(key, listValue, scope, true);

            ConfigDisplayHelpers.DisplayList(key, listValue);
        }
        else
        {
            var isFileNameScope = scope == ConfigFileScope.FileName && !string.IsNullOrEmpty(configFileName);
            if (isFileNameScope) _configStore.Set(key, value, configFileName!);
            if (!isFileNameScope) _configStore.Set(key, value, scope, true);

            var configValue = isFileNameScope
                ? _configStore.GetFromFileName(key, configFileName!)
                : _configStore.GetFromScope(key, scope);

            ConfigDisplayHelpers.DisplayConfigValue(key, configValue, showLocation: true);
        }

        return 0;
    }
}