using System;
using System.IO;

/// <summary>
/// Provides methods for determining configuration file locations.
/// </summary>
public static class ConfigLocation
{
    private const string CONFIG_DIR_NAME = $".{Program.Name}";
    private const string YAML_CONFIG_NAME = "config.yaml";
    private const string INI_CONFIG_NAME = "config";

    /// <summary>
    /// Gets the directory path for the specified configuration scope.
    /// </summary>
    /// <param name="scope">The configuration scope.</param>
    /// <returns>The directory path for the specified scope.</returns>
    public static string GetScopeDirectory(ConfigScope scope)
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

    /// <summary>
    /// Gets the path to the YAML configuration file for the specified scope.
    /// </summary>
    /// <param name="scope">The configuration scope.</param>
    /// <returns>The path to the YAML configuration file.</returns>
    public static string GetYamlConfigPath(ConfigScope scope)
    {
        return Path.Combine(GetScopeDirectory(scope), YAML_CONFIG_NAME);
    }

    /// <summary>
    /// Gets the path to the INI configuration file for the specified scope.
    /// </summary>
    /// <param name="scope">The configuration scope.</param>
    /// <returns>The path to the INI configuration file.</returns>
    public static string GetIniConfigPath(ConfigScope scope)
    {
        return Path.Combine(GetScopeDirectory(scope), INI_CONFIG_NAME);
    }

    /// <summary>
    /// Gets the best available configuration file path for the specified scope.
    /// Prefers YAML file if it exists, otherwise falls back to the INI file.
    /// </summary>
    /// <param name="scope">The configuration scope.</param>
    /// <returns>The path to the best available configuration file, or null if none exists.</returns>
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

    /// <summary>
    /// Ensures the configuration directory exists for the specified scope.
    /// </summary>
    /// <param name="scope">The configuration scope.</param>
    public static void EnsureConfigDirectoryExists(ConfigScope scope)
    {
        string directory = GetScopeDirectory(scope);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}