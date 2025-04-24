using System;

abstract class ConfigBaseCommand : Command
{
    protected readonly ConfigStore _configStore;
    
    public ConfigFileScope? Scope { get; set; }
    public string? ConfigFileName { get; internal set; }

    public ConfigBaseCommand()
    {
        _configStore = ConfigStore.Instance;
    }

    public override bool IsEmpty()
    {
        return false;
    }
}