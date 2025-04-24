using System;
using System.IO;

public static class ProfileFileHelpers
{
    /// <summary>
    /// Loads a profile by name into the configuration store.
    /// </summary>
    /// <param name="profileName">The name of the profile to load</param>
    /// <exception cref="CommandLineException">Thrown when profile name is empty or profile not found</exception>
    public static void LoadProfile(string profileName)
    {
        if (string.IsNullOrEmpty(profileName))
        {
            throw new CommandLineException("Profile name cannot be empty.");
        }
        
        var profilePath = FindProfileFile(profileName);
        if (profilePath == null)
        {
            throw new CommandLineException($"Profile '{profileName}' not found in any scope or parent directories.");
        }
        
        ConsoleHelpers.WriteDebugLine($"Loading profile from {profilePath}");
        ConfigStore.Instance.LoadConfigFile(profilePath);
    }
    
    /// <summary>
    /// Finds a profile file by name across all scopes and optionally parent directories.
    /// </summary>
    /// <param name="profileName">The name of the profile to find</param>
    /// <returns>The path to the profile file if found, null otherwise</returns>
    public static string? FindProfileFile(string profileName)
    {
        var yamlFileName = $"{profileName}.yaml";
        var yamlProfilePath = ScopeFileHelpers.FindFileInAnyScope(yamlFileName, "profiles", searchParents: true);
        if (yamlProfilePath != null)
        {
            return yamlProfilePath;
        }

        var bareFileNameNoExt = $"{profileName}";
        return ScopeFileHelpers.FindFileInAnyScope(bareFileNameNoExt, "profiles", searchParents: true);
    }
    
    /// <summary>
    /// Finds the profiles directory in the specified scope.
    /// </summary>
    /// <param name="scope">The scope to search in</param>
    /// <param name="create">Whether to create the directory if it doesn't exist</param>
    /// <returns>The path to the profiles directory, or null if not found</returns>
    public static string? FindProfilesDirectoryInScope(ConfigFileScope scope, bool create = false)
    {
        return create
            ? ScopeFileHelpers.EnsureDirectoryInScope("profiles", scope)
            : ScopeFileHelpers.FindDirectoryInScope("profiles", scope);
    }
}