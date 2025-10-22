using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class ConfigClearCommand : ConfigBaseCommand
{
    public string? Key { get; set; }

    public override bool IsEmpty()
    {
        return string.IsNullOrWhiteSpace(Key);
    }

    public override string GetCommandName()
    {
        return "config clear";
    }

    public override async Task<object> ExecuteAsync(bool interactive)
    {
        return await Task.Run(() => ExecuteClear(Key, Scope ?? ConfigFileScope.Local, ConfigFileName));
    }

    private int ExecuteClear(string? key, ConfigFileScope scope, string? configFileName)
    {
        ConsoleHelpers.WriteDebugLine($"ExecuteClear; key: {key}; scope: {scope}; configFileName: {configFileName}");
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new CommandLineException($"Key cannot be null or empty.");
        }

        // Normalize the key if it's a known setting
        if (KnownSettings.IsKnown(key))
        {
            key = KnownSettings.GetCanonicalForm(key);
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