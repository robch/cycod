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
        
        var profilesDirectory = FindProfilesDirectory(create: false);
        var yamlProfile = profilesDirectory != null
            ? Path.Combine(profilesDirectory, $"{profileName}.yaml")
            : Path.Combine(Directory.GetCurrentDirectory(), $"{profileName}.yaml");
        var iniProfile = profilesDirectory != null
            ? Path.Combine(profilesDirectory, profileName)
            : Path.Combine(Directory.GetCurrentDirectory(), profileName);

        var yamlProfileOk = FileHelpers.FileExists(yamlProfile);
        var iniProfileOk = FileHelpers.FileExists(iniProfile);
        var profileOk = yamlProfileOk || iniProfileOk;

        if (!profileOk)
        {
            throw new CommandLineException($"Profile '{profileName}' not found at path; checked:\n- {yamlProfile}\n- {iniProfile}");
        }
        
        var profile = yamlProfileOk ? yamlProfile : iniProfile;
        ConsoleHelpers.WriteDebugLine($"Loading profile from {profile}");
        ConfigStore.Instance.LoadConfigFile(profile);
    }
    
    /// <summary>
    /// Finds the profiles directory in the parent directories.
    /// </summary>
    /// <param name="create">If true, creates the directory if it doesn't exist</param>
    /// <returns>The path to the profiles directory, or null if not found</returns>
    public static string? FindProfilesDirectory(bool create = false)
    {
        return create
            ? DirectoryHelpers.FindOrCreateDirectorySearchParents(".chatx", "profiles")
            : DirectoryHelpers.FindDirectorySearchParents(".chatx", "profiles");
    }
}