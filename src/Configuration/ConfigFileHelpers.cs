using System;
using System.IO;

/// <summary>
/// Provides methods for determining configuration file locations.
/// </summary>
public static class ConfigFileHelpers
{
    public static string GetConfigFileName(ConfigFileScope scope)
    {
        var path = FindConfigFile(scope);
        if (path != null) return path;

        return GetYamlConfigFileName(scope);
    }

    public static string? FindConfigFile(ConfigFileScope scope)
    {
        var yamlPath = GetYamlConfigFileName(scope);
        if (File.Exists(yamlPath))
        {
            ConsoleHelpers.WriteDebugLine($"Found YAML config file at: {yamlPath}");
            return yamlPath;
        }

        var iniPath = GetIniConfigFileName(scope);
        if (File.Exists(iniPath))
        {
            ConsoleHelpers.WriteDebugLine($"Found YAML config file at: {yamlPath}");
            return iniPath;
        }

        return null;
    }

    private static string GetYamlConfigFileName(ConfigFileScope scope)
    {
        return Path.Combine(GetScopeDirectory(scope), YAML_CONFIG_NAME);
    }

    private static string GetIniConfigFileName(ConfigFileScope scope)
    {
        return Path.Combine(GetScopeDirectory(scope), INI_CONFIG_NAME);
    }

    private static string GetScopeDirectory(ConfigFileScope scope)
    {
        var directory = scope switch
        {
            ConfigFileScope.Global => GetGlobalScopeDirectory(),
            ConfigFileScope.User => GetUserScopeDirectory(),
            ConfigFileScope.Local => GetLocalScopeDirectory(),
            _ => throw new ArgumentOutOfRangeException(nameof(scope))
        };

        ConsoleHelpers.WriteDebugLine($"Config directory for {scope} scope: {directory}");
        return directory;
    }

    private static string GetGlobalScopeDirectory()
    {
        var isLinuxOrMac = Environment.OSVersion.Platform == PlatformID.Unix || Environment.OSVersion.Platform == PlatformID.MacOSX;
        var parent = isLinuxOrMac ? $"/etc" : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData));
        return Path.Combine(parent, CONFIG_DIR_NAME);
    }

    private static string GetUserScopeDirectory()
    {
        var parent = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(parent, CONFIG_DIR_NAME);
    }

    private static string GetLocalScopeDirectory()
    {
        var existingYamlFile = FileHelpers.FindFileSearchParents(CONFIG_DIR_NAME, YAML_CONFIG_NAME);
        if (existingYamlFile != null) return Path.GetDirectoryName(existingYamlFile)!;

        var existingIniFile = FileHelpers.FindFileSearchParents(CONFIG_DIR_NAME, INI_CONFIG_NAME);
        if (existingIniFile != null) return Path.GetDirectoryName(existingIniFile)!;

        return Path.Combine(Directory.GetCurrentDirectory(), CONFIG_DIR_NAME);
    }

    private const string CONFIG_DIR_NAME = $".{Program.Name}";
    private const string YAML_CONFIG_NAME = "config.yaml";
    private const string INI_CONFIG_NAME = "config";
}