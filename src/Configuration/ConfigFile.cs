using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Base class for configuration file handlers.
/// </summary>
public abstract class ConfigFile
{
    /// <summary>
    /// The path to the configuration file.
    /// </summary>
    protected readonly string FilePath;

    /// <summary>
    /// Initializes a new instance of the ConfigFile class.
    /// </summary>
    /// <param name="filePath">The path to the configuration file.</param>
    protected ConfigFile(string filePath)
    {
        FilePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
    }

    /// <summary>
    /// Reads the configuration from the file.
    /// </summary>
    /// <returns>A dictionary containing the configuration data.</returns>
    public abstract Dictionary<string, object> Read();

    /// <summary>
    /// Writes the configuration to the file.
    /// </summary>
    /// <param name="data">The configuration data to write.</param>
    public abstract void Write(Dictionary<string, object> data);

    /// <summary>
    /// Checks if the configuration file exists.
    /// </summary>
    /// <returns>True if the file exists, false otherwise.</returns>
    public bool Exists() => File.Exists(FilePath);

    /// <summary>
    /// Ensures the directory for the configuration file exists.
    /// </summary>
    protected void EnsureDirectoryExists()
    {
        string? directory = Path.GetDirectoryName(FilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    /// <summary>
    /// Creates an appropriate ConfigFile instance based on the file extension.
    /// </summary>
    /// <param name="filePath">The path to the configuration file.</param>
    /// <returns>A ConfigFile instance appropriate for the file.</returns>
    public static ConfigFile Create(string filePath)
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
}