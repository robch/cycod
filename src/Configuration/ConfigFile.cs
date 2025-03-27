using System;
using System.Collections.Generic;
using System.IO;

public abstract class ConfigFile
{
    public static ConfigFile FromFile(string filePath, ConfigFileScope scope)
    {
        return Path.GetExtension(filePath).ToLowerInvariant() switch
        {
            ".yaml" or ".yml" => new YamlConfigFile(filePath, scope),
            _ => new IniConfigFile(filePath, scope)
        };
    }

    public string FileName { get => _fileName; }
    public ConfigFileScope Scope { get => _scope; }

    public Dictionary<string, object> Settings
    {
        get
        {
            EnsureLoaded();
            return _settings!;
        }

        protected set
        {
            _settings = value;
        }
    }

    public void Save()
    {
        EnsureLoaded();
        WriteSettings(_fileName);
    }

    protected ConfigFile(string fileName, ConfigFileScope scope = ConfigFileScope.FileName)
    {
        _fileName = fileName;
        _scope = scope;
    }

    protected abstract Dictionary<string, object> ReadSettings(string fileName);

    protected abstract void WriteSettings(string fileName, Dictionary<string, object>? settings = null);

    protected void EnsureLoaded()
    {
        if (_settings == null)
        {
            _settings = ReadSettings(_fileName);
        }
    }

    private readonly string _fileName;
    private readonly ConfigFileScope _scope;
    private Dictionary<string, object>? _settings;
}