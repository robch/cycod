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
        tasks.Add(Task.FromResult(ExecuteClear(Key, GetConfigScope())));
        return tasks;
    }

    private int ExecuteClear(string? key, ConfigFileScope scope)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new CommandLineException($"Key cannot be null or empty.");
        }

        bool cleared = _configStore.Clear(key, scope, true);
        ConsoleHelpers.WriteLine(cleared
            ? $"{key}: (cleared)"
            : $"{key}: (not found)",
            overrideQuiet: true);

        return 0;
    }
}