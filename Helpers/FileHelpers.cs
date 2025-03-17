using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

class FileHelpers
{
    public static string EnsureDirectoryExists(string folder)
    {
        var validFolderName = !string.IsNullOrEmpty(folder);
        if (validFolderName && !Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder!);
        }

        return folder;
    }

    public static string FindOrCreateDirectory(params string[] paths)
    {
        return FindDirectory(paths, createIfNotFound: true)!;
    }

    public static string? FindDirectory(params string[] paths)
    {
        return FindDirectory(paths, createIfNotFound: false);
    }

    public static string? FindDirectory(string[] paths, bool createIfNotFound)
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

    public static void EnsureDirectoryForFileExists(string fileName)
    {
        EnsureDirectoryExists(Path.GetDirectoryName(fileName) ?? ".");
    }

    public static bool IsFileMatch(string fileName, List<Regex> includeFileContainsPatternList, List<Regex> excludeFileContainsPatternList)
    {
        var checkContent = includeFileContainsPatternList.Any() || excludeFileContainsPatternList.Any();
        if (!checkContent) return true;

        try
        {
            ConsoleHelpers.DisplayStatus($"Processing: {fileName} ...");

            var content = ReadAllText(fileName);
            var includeFile = includeFileContainsPatternList.All(regex => regex.IsMatch(content));
            var excludeFile = excludeFileContainsPatternList.Count > 0 && excludeFileContainsPatternList.Any(regex => regex.IsMatch(content));

            return includeFile && !excludeFile;
        }
        catch (Exception)
        {
            return false;
        }
        finally
        {
            ConsoleHelpers.DisplayStatusErase();
        }
    }

    public static string? GetFileNameFromTemplate(string fileName, string? template)
    {
        if (string.IsNullOrEmpty(template)) return template;

        var filePath = Path.GetDirectoryName(fileName);
        var fileBase = Path.GetFileNameWithoutExtension(fileName);
        var fileExt = Path.GetExtension(fileName).TrimStart('.');
        var timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        var time = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();

        ConsoleHelpers.WriteDebugLine($"filePath: {filePath}");
        ConsoleHelpers.WriteDebugLine($"fileBase: {fileBase}");
        ConsoleHelpers.WriteDebugLine($"fileExt: {fileExt}");
        ConsoleHelpers.WriteDebugLine($"timeStamp: {timeStamp}");

        return template
            .Replace("{fileName}", fileName)
            .Replace("{filename}", fileName)
            .Replace("{filePath}", filePath)
            .Replace("{filepath}", filePath)
            .Replace("{fileBase}", fileBase)
            .Replace("{filebase}", fileBase)
            .Replace("{fileExt}", fileExt)
            .Replace("{fileext}", fileExt)
            .Replace("{timeStamp}", timeStamp)
            .Replace("{timestamp}", timeStamp)
            .Replace("{time}", time)
            .Trim(' ', '/', '\\');
    }

    public static string MakeRelativePath(string fullPath)
    {
        var currentDirectory = Directory.GetCurrentDirectory().TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
        fullPath = Path.GetFullPath(fullPath);

        if (fullPath.StartsWith(currentDirectory, StringComparison.OrdinalIgnoreCase))
        {
            return fullPath.Substring(currentDirectory.Length);
        }

        Uri fullPathUri = new Uri(fullPath);
        Uri currentDirectoryUri = new Uri(currentDirectory);

        string relativePath = Uri.UnescapeDataString(currentDirectoryUri.MakeRelativeUri(fullPathUri).ToString().Replace('/', Path.DirectorySeparatorChar));

        if (Path.DirectorySeparatorChar == '\\')
        {
            relativePath = relativePath.Replace('/', Path.DirectorySeparatorChar);
        }

        return relativePath;
    }

    public static bool FileExists(string? fileName)
    {
        return !string.IsNullOrEmpty(fileName) && (File.Exists(fileName) || fileName == "-");
    }

    public static string ReadAllText(string fileName)
    {
        var content = fileName == "-"
            ? string.Join("\n", ConsoleHelpers.GetAllLinesFromStdin())
            : File.ReadAllText(fileName, Encoding.UTF8);

        return content;
    }

    public static void WriteAllText(string fileName, string content)
    {
        EnsureDirectoryForFileExists(fileName);
        File.WriteAllText(fileName, content, Encoding.UTF8);
    }

    public static IEnumerable<string> GetEmbeddedStreamFileNames()
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        return assembly.GetManifestResourceNames();
    }

    public static bool EmbeddedStreamExists(string fileName)
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .Where(name => name.EndsWith(fileName, StringComparison.OrdinalIgnoreCase))
            .OrderBy(name => name.Length)
            .FirstOrDefault();

        var found = resourceName != null;
        if (found) return true;

        var allResourceNames = string.Join("\n  ", assembly.GetManifestResourceNames());
        ConsoleHelpers.WriteDebugLine($"DEBUG: Embedded resources ({assembly.GetManifestResourceNames().Count()}):\n\n  {allResourceNames}\n");

        return false;
    }

    public static string? ReadEmbeddedStream(string fileName)
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        var resourceName = assembly.GetManifestResourceNames()
            .Where(name => name.EndsWith(fileName, StringComparison.OrdinalIgnoreCase))
            .OrderBy(name => name.Length)
            .FirstOrDefault();

        if (resourceName == null) return null;

        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null) return null;

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
