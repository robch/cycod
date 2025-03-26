using System;
using System.IO;

/// <summary>
/// Provides methods for determining configuration file locations.
/// </summary>
public static class ConfigLocation
{
    public static string GetConfigPath(ConfigScope scope)
    {
        var path = GetExistingConfigPath(scope);
        if (path != null) return path;

        return GetYamlConfigPath(scope);
    }

    public static string? GetExistingConfigPath(ConfigScope scope)
    {
        string yamlPath = GetYamlConfigPath(scope);
        if (File.Exists(yamlPath))
        {
            ConsoleHelpers.WriteDebugLine($"Found YAML config file at: {yamlPath}");
            return yamlPath;
        }

        string iniPath = GetIniConfigPath(scope);
        if (File.Exists(iniPath))
        {
            ConsoleHelpers.WriteDebugLine($"Found YAML config file at: {yamlPath}");
            return iniPath;
        }

        return null;
    }

    public static void EnsureConfigDirectoryExists(ConfigScope scope)
    {
        string directory = GetScopeDirectory(scope);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }

    private static string GetYamlConfigPath(ConfigScope scope)
    {
        return Path.Combine(GetScopeDirectory(scope), YAML_CONFIG_NAME);
    }

    private static string GetIniConfigPath(ConfigScope scope)
    {
        return Path.Combine(GetScopeDirectory(scope), INI_CONFIG_NAME);
    }

    private static string GetScopeDirectory(ConfigScope scope)
    {
        var directory = scope switch
        {
            ConfigScope.Global => GetGlobalScopeDirectory(),
            ConfigScope.User => GetUserScopeDirectory(),
            ConfigScope.Project => GetProjectScopeDirectory(),
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

    private static string GetProjectScopeDirectory()
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