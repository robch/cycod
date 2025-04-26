using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

public class FileHelpers
{
    public static string? FindFileSearchParents(params string[] paths)
    {
        return FindFileSearchParents(paths, createIfNotFound: false);
    }

    public static string? FindFileSearchParents(string[] paths, bool createIfNotFound)
    {
        var current = Directory.GetCurrentDirectory();
        while (current != null)
        {
            var combined = Path.Combine(paths.Prepend(current).ToArray());
            if (File.Exists(combined))
            {
                return combined;
            }

            current = Directory.GetParent(current)?.FullName;
        }

        if (createIfNotFound)
        {
            current = Directory.GetCurrentDirectory();
            var combined = Path.Combine(paths.Prepend(current).ToArray());
            DirectoryHelpers.EnsureDirectoryForFileExists(combined);
            WriteAllText(combined, string.Empty);
            return combined;
        }

        return null;
    }

    public static bool FileExists(string? fileName)
    {
        return !string.IsNullOrEmpty(fileName) && (File.Exists(fileName) || fileName == "-");
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
            .TrimEnd(' ', '/', '\\');
    }

    public static string ReadAllText(string fileName)
    {
        var content = ConsoleHelpers.IsStandardInputReference(fileName)
            ? string.Join("\n", ConsoleHelpers.GetAllLinesFromStdin())
            : File.ReadAllText(fileName, Encoding.UTF8);

        return content;
    }

    public static string WriteAllText(string fileName, string content, string? saveToFolderOnAccessDenied = null)
    {
        try
        {
            DirectoryHelpers.EnsureDirectoryForFileExists(fileName);
            File.WriteAllText(fileName, content, Encoding.UTF8);
        }
        catch (Exception)
        {
            var trySavingElsewhere = !string.IsNullOrEmpty(saveToFolderOnAccessDenied);
            if (trySavingElsewhere)
            {
                var userProfileFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
                var trySavingToFolder = Path.Combine(userProfileFolder, saveToFolderOnAccessDenied!);

                var fileNameWithoutFolder = Path.GetFileName(fileName);
                fileName = Path.Combine(trySavingToFolder, fileNameWithoutFolder);

                WriteAllText(fileName, content, null);
            }
        }
        return fileName;
    }

    public static void AppendAllText(string fileName, string trajectoryContent)
    {
        DirectoryHelpers.EnsureDirectoryForFileExists(fileName);
        File.AppendAllText(fileName, trajectoryContent, Encoding.UTF8);
    }
}
