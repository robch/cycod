using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public class DirectoryHelpers
{
    public static string FindOrCreateDirectorySearchParents(params string[] paths)
    {
        return FindDirectorySearchParents(paths, createIfNotFound: true)!;
    }

    public static string? FindDirectorySearchParents(params string[] paths)
    {
        return FindDirectorySearchParents(paths, createIfNotFound: false);
    }

    public static string? FindDirectorySearchParents(string[] paths, bool createIfNotFound)
    {
        var current = Directory.GetCurrentDirectory();
        while (current != null)
        {
            var combined = Path.Combine(paths.Prepend(current).ToArray());
            if (Directory.Exists(combined))
            {
                return combined;
            }

            current = Directory.GetParent(current)?.FullName;
        }

        if (createIfNotFound)
        {
            current = Directory.GetCurrentDirectory();
            var combined = Path.Combine(paths.Prepend(current).ToArray());
            return EnsureDirectoryExists(combined);
        }

        return null;
    }
    
    public static string EnsureDirectoryExists(string folder)
    {
        try
        {
            var validFolderName = !string.IsNullOrEmpty(folder);
            if (validFolderName && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder!);
            }
        }
        catch (Exception ex)
        {
            if (!Directory.Exists(folder))
            {
                Console.WriteLine($"Error creating directory: {ex.Message}");
                throw;
            }
        }
        return folder;
    }

    public static void EnsureDirectoryForFileExists(string fileName)
    {
        EnsureDirectoryExists(Path.GetDirectoryName(fileName) ?? ".");
    }
}
