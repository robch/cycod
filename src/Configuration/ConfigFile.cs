using System;
using System.Collections.Generic;
using System.IO;

public abstract class ConfigFile
{
    protected ConfigFile(string filePath)
    {
        FilePath = filePath;
    }

    public abstract Dictionary<string, object> Read();

    public abstract void Write(Dictionary<string, object> data);

    public static ConfigFile FromFile(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
        {
            throw new ArgumentNullException(nameof(filePath));
        }

        return Path.GetExtension(filePath).ToLowerInvariant() switch
        {
            ".yaml" or ".yml" => new YamlConfigFile(filePath),
            _ => new IniConfigFile(filePath)
        };
    }

    protected readonly string FilePath;
}