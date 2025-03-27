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

    protected ConfigFileScope GetConfigScope()
    {
        if (Scope.HasValue) 
        {
            return Scope.Value;
        }

        if (this is ConfigListCommand)
        {
            return ConfigFileScope.Any;
        }
        
        return ConfigFileScope.Local;
    }

    public override bool IsEmpty()
    {
        return false;
    }
}