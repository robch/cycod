using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Provides methods for working with files across different scope directories.
/// </summary>
public static class ScopeFileHelpers
{
    public static ConfigFileScope GetScopeFromPath(string path)
    {
        if (path.Contains(ConfigFileHelpers.GetGlobalScopeDirectory())) return ConfigFileScope.Global;
        if (path.Contains(ConfigFileHelpers.GetUserScopeDirectory())) return ConfigFileScope.User;
        if (path.Contains(ConfigFileHelpers.GetLocalScopeDirectory())) return ConfigFileScope.Local;
        return ConfigFileScope.FileName;
    }

    /// <summary>
    /// Finds a file in a specific scope's subdirectory.
    /// </summary>
    /// <param name="fileName">The name of the file to find</param>
    /// <param name="subDir">The subdirectory name within the scope directory</param>
    /// <param name="scope">The scope to search in</param>
    /// <returns>The full path to the file if found, null otherwise</returns>
    public static string? FindFileInScope(string fileName, string subDir, ConfigFileScope scope)
    {
        var scopeDir = ConfigFileHelpers.GetScopeDirectoryPath(scope);
        if (scopeDir == null)
        {
            ConsoleHelpers.WriteDebugLine($"FindFileInScope: Scope directory for {scope} not found.");
            return null;
        }

        var subDirPath = Path.Combine(scopeDir, subDir);
        if (!Directory.Exists(subDirPath))
        {
            ConsoleHelpers.WriteDebugLine($"FindFileInScope: Subdirectory {subDir} does not exist in scope {scope}.");
            return null;
        }

        var combined = Path.Combine(subDirPath, fileName);
        var existingFile = File.Exists(combined) ? combined : null;

        ConsoleHelpers.WriteDebugLine(existingFile != null
            ? $"FindFileInScope: Found file {fileName} in scope {scope} at {existingFile}"
            : $"FindFileInScope: File {fileName} not found in scope {scope}.");

        return existingFile;
    }

    /// <summary>
    /// Finds a file in any scope's subdirectory, with optional parent directory search.
    /// </summary>
    /// <param name="fileName">The name of the file to find</param>
    /// <param name="subDir">The subdirectory name within the scope directory</param>
    /// <param name="searchParents">Whether to search parent directories if the file is not found in any scope</param>
    /// <returns>The full path to the file if found, null otherwise</returns>
    public static string? FindFileInAnyScope(string fileName, string subDir, bool searchParents = false)
    {
        // First, check the current directory for the bare file
        var bareFilePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);
        if (File.Exists(bareFilePath))
        {
            ConsoleHelpers.WriteDebugLine($"FindFileInAnyScope: Found bare file in current directory: {bareFilePath}");
            return bareFilePath;
        }

        // Then check each scope in order: Local, User, Global
        foreach (var scope in new[] { ConfigFileScope.Local, ConfigFileScope.User, ConfigFileScope.Global })
        {
            var fileInScope = FindFileInScope(fileName, subDir, scope);
            if (fileInScope != null)
            {
                ConsoleHelpers.WriteDebugLine($"FindFileInAnyScope: Found file in scope {scope}: {fileInScope}");
                return fileInScope;
            }
        }

        // If not found in any scope and searchParents is true, search parent directories
        if (searchParents)
        {
            var configDirName = ProgramInfo.ConfigDirName;
            var existing = FileHelpers.FindFileSearchParents(configDirName, subDir, fileName);

            ConsoleHelpers.WriteDebugLine(existing != null
                ? $"FindFileInAnyScope: Found file in parent directory: {existing}"
                : $"FindFileInAnyScope: File {fileName} not found in any scope or parent directories.");
            return existing;
        }

        return null;
    }

    /// <summary>
    /// Finds a directory in a specific scope.
    /// </summary>
    /// <param name="subDir">The subdirectory name within the scope directory</param>
    /// <param name="scope">The scope to search in</param>
    /// <returns>The full path to the directory if found, null otherwise</returns>
    public static string? FindDirectoryInScope(string subDir, ConfigFileScope scope)
    {
        var scopeDir = ConfigFileHelpers.GetScopeDirectoryPath(scope);
        if (scopeDir == null)
        {
            ConsoleHelpers.WriteDebugLine($"FindDirectoryInScope: Scope directory for {scope} not found.");
            return null;
        }

        var dirPath = Path.Combine(scopeDir, subDir);
        var existing = Directory.Exists(dirPath) ? dirPath : null;

        ConsoleHelpers.WriteDebugLine(existing != null
            ? $"FindDirectoryInScope: Found directory {subDir} in scope {scope} at {existing}"
            : $"FindDirectoryInScope: Directory {subDir} not found in scope {scope}.");
        return existing;
    }

    /// <summary>
    /// Finds a directory in any scope, with optional parent directory search.
    /// </summary>
    /// <param name="subDir">The subdirectory name within the scope directory</param>
    /// <param name="searchParents">Whether to search parent directories if the directory is not found in any scope</param>
    /// <returns>The full path to the directory if found, null otherwise</returns>
    public static string? FindDirectoryInAnyScope(string subDir, bool searchParents = false)
    {
        // Check each scope in order: Local, User, Global
        foreach (var scope in new[] { ConfigFileScope.Local, ConfigFileScope.User, ConfigFileScope.Global })
        {
            var dirInScope = FindDirectoryInScope(subDir, scope);
            if (dirInScope != null)
            {
                ConsoleHelpers.WriteDebugLine($"FindDirectoryInAnyScope: Found directory in scope {scope}: {dirInScope}");
                return dirInScope;
            }
        }

        // If not found in any scope and searchParents is true, search parent directories
        if (searchParents)
        {
            var configDirName = ProgramInfo.ConfigDirName;
            var existing = DirectoryHelpers.FindDirectorySearchParents(configDirName, subDir);

            ConsoleHelpers.WriteDebugLine(existing != null
                ? $"FindDirectoryInAnyScope: Found directory in parent directory: {existing}"
                : $"FindDirectoryInAnyScope: Directory {subDir} not found in any scope or parent directories.");
            return existing;
        }

        ConsoleHelpers.WriteDebugLine($"FindDirectoryInAnyScope: Directory {subDir} not found in any scope.");
        return null;
    }

    /// <summary>
    /// Ensures a directory exists in the specified scope.
    /// </summary>
    /// <param name="subDir">The subdirectory name within the scope directory</param>
    /// <param name="scope">The scope to create the directory in</param>
    /// <returns>The full path to the directory</returns>
    public static string EnsureDirectoryInScope(string subDir, ConfigFileScope scope)
    {
        var scopeDir = ConfigFileHelpers.GetScopeDirectoryPath(scope);
        if (scopeDir == null)
        {
            throw new InvalidOperationException($"Could not get scope directory path for {scope}");
        }

        var dirPath = Path.Combine(scopeDir, subDir);
        var existing = DirectoryHelpers.EnsureDirectoryExists(dirPath);

        ConsoleHelpers.WriteDebugLine($"EnsureDirectoryInScope: Ensured directory {subDir} in scope {scope} at {existing}");
        return existing;
    }

    /// <summary>
    /// Finds all files matching a pattern in a specific scope.
    /// </summary>
    /// <param name="pattern">The search pattern</param>
    /// <param name="subDir">The subdirectory name within the scope directory</param>
    /// <param name="scope">The scope to search in</param>
    /// <returns>An array of file paths matching the pattern</returns>
    public static string[] FindFilesInScope(string pattern, string subDir, ConfigFileScope scope)
    {
        var dirPath = FindDirectoryInScope(subDir, scope);
        if (dirPath == null || !Directory.Exists(dirPath))
        {
            return Array.Empty<string>();
        }

        return Directory.GetFiles(dirPath, pattern);
    }
}