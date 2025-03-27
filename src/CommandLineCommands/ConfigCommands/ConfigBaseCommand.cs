using System;

abstract class ConfigBaseCommand : Command
{
    protected readonly ConfigStore _configStore;
    
    public ConfigFileScope? Scope { get; set; }

    public ConfigBaseCommand()
    {
        _configStore = ConfigStore.Instance;
    }

    protected ConfigFileScope GetConfigScope()
    {
        // If a scope was explicitly set via command line, use it
        if (Scope.HasValue)
        {
            return Scope.Value;
        }
        
        // Default to Any for config list, Local for other commands
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