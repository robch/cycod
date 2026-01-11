using System;
using System.Collections.Generic;
using System.IO;

public class PathHelpers
{
    public static string? Combine(string path1, string path2)
    {
        try
        {
            return Path.Combine(path1, path2);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    public static string? Combine(string path1, string path2, string path3)
    {
        try
        {
            return Path.Combine(path1, path2, path3);
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    public static IEnumerable<string> Combine(string path1, IEnumerable<string> path2s)
    {
        var list = new List<string>();
        foreach (var path2 in path2s)
        {
            var combined = PathHelpers.Combine(path1, path2);
            if (combined == null) continue;

            list.Add(!string.IsNullOrEmpty(path2)
                ? combined
                : path1);
        }
        return list;
    }

    public static string NormalizePath(string outputDirectory)
    {
        var normalized = new DirectoryInfo(outputDirectory).FullName;
        var cwd = Directory.GetCurrentDirectory();
        return normalized.StartsWith(cwd) && normalized.Length > cwd.Length + 1
            ? normalized.Substring(cwd.Length + 1)
            : normalized;
    }

    /// <summary>
    /// Expands tilde (~) paths to full paths using the user's home directory.
    /// Handles both "~" (home directory) and "~/path" (home directory + path).
    /// </summary>
    /// <param name="path">The path that may contain a tilde</param>
    /// <returns>The expanded path with tilde replaced by the home directory</returns>
    public static string ExpandPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return path;

        // Handle exact "~" case
        if (path == "~")
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        }

        // Handle "~/..." case
        if (path.StartsWith("~/"))
        {
            var homeDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(homeDir, path.Substring(2));
        }

        // Return unchanged if no tilde expansion needed
        return path;
    }
}
