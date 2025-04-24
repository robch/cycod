using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

public class FileHelpers
{
    public static IEnumerable<string> FindFiles(string path, string pattern)
    {
        var combined = PathHelpers.Combine(path, pattern);
        return combined != null
            ? FindFiles(combined)
            : Enumerable.Empty<string>();
    }

    public static IEnumerable<string> FindFiles(string fileNames)
    {
        var currentDir = Directory.GetCurrentDirectory();
        foreach (var item in fileNames.Split(new char[] { ';', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            ConsoleHelpers.WriteDebugLine($"  Searching for files '{fileNames}'");

            var i1 = item.LastIndexOf(Path.DirectorySeparatorChar);
            var i2 = item.LastIndexOf(Path.AltDirectorySeparatorChar);
            var hasPath = i1 >= 0 || i2 >= 0;

            var pathLen = Math.Max(i1, i2);
            var path = !hasPath ? currentDir : item.Substring(0, pathLen);
            var pattern = !hasPath ? item : item.Substring(pathLen + 1);

            EnumerationOptions? recursiveOptions = null;
            if (path.EndsWith("**"))
            {
                path = path.Substring(0, path.Length - 2).TrimEnd('/', '\\');
                if (string.IsNullOrEmpty(path)) path = ".";
                recursiveOptions = new EnumerationOptions() { RecurseSubdirectories = true };
            }

            if (!Directory.Exists(path)) continue;

            var files = recursiveOptions != null 
                ? Directory.EnumerateFiles(path, pattern, recursiveOptions)
                : Directory.EnumerateFiles(path, pattern);
            foreach (var file in files)
            {
                yield return file;
            }
        }

        yield break;
    }

    public static IEnumerable<string> FindFilesInOsPath(string fileName)
    {
        var lookIn = Environment.GetEnvironmentVariable("PATH")!.Split(System.IO.Path.PathSeparator);
        var found = lookIn.SelectMany(x =>
        {
            try
            {
                return System.IO.Directory.GetFiles(x, fileName);
            }
            catch (Exception)
            {
                return Enumerable.Empty<string>();
            }
        });
        return found;
    }

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

    public static void WriteAllText(string fileName, string content)
    {
        DirectoryHelpers.EnsureDirectoryForFileExists(fileName);
        File.WriteAllText(fileName, content, Encoding.UTF8);
    }

    public static void AppendAllText(string fileName, string trajectoryContent)
    {
        DirectoryHelpers.EnsureDirectoryForFileExists(fileName);
        File.AppendAllText(fileName, trajectoryContent, Encoding.UTF8);
    }

    public static FileInfo GetAssemblyFileInfo(Type type)
    {
        // GetAssembly.Location always returns empty when the project is built as 
        // a single-file app (which we do when publishing the Dependency package),
        // warning IL3000
        var assembly = Assembly.GetAssembly(type);
        string assemblyPath = assembly?.Location ?? string.Empty;
        if (assemblyPath == string.Empty)
        {
            assemblyPath = AppContext.BaseDirectory;
            assemblyPath += assembly?.GetName().Name + ".dll";
        }
        return new FileInfo(assemblyPath);
    }

}
