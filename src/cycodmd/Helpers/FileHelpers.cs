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
            Directory.CreateDirectory(folder);
        }

        return folder;
    }

    public static string FindOrCreateDirectory(params string[] paths)
    {
        return FindDirectory(paths, createIfNotFound: true);
    }

    public static string FindDirectory(params string[] paths)
    {
        return FindDirectory(paths, createIfNotFound: false);
    }

    public static string FindDirectory(string[] paths, bool createIfNotFound)
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

    public static string ParentFindFile(string fileName)
    {
        return ParentFindFile(new[] { "./" }, fileName);
    }

    public static string ParentFindFile(string[] paths, string fileName)
    {
        var current = Directory.GetCurrentDirectory();
        while (current != null)
        {
            var combined = Path.Combine(paths.Prepend(current).ToArray());
            var file = Path.Combine(combined, fileName);
            if (File.Exists(file))
            {
                return file;
            }

            current = Directory.GetParent(current)?.FullName;
        }

        return null;
    }

    public static void EnsureDirectoryForFileExists(string fileName)
    {
        EnsureDirectoryExists(Path.GetDirectoryName(fileName));
    }

    public static bool IsFileMatch(string fileName, List<Regex> includeFileContainsPatternList, List<Regex> excludeFileContainsPatternList)
    {
        var checkContent = includeFileContainsPatternList.Any() || excludeFileContainsPatternList.Any();
        if (!checkContent) return true;

        try
        {
            ConsoleHelpers.PrintStatus($"Processing: {fileName} ...");

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
            ConsoleHelpers.PrintStatusErase();
        }
    }

    public static string GetFileNameFromTemplate(string fileName, string template)
    {
        string filePath = Path.GetDirectoryName(fileName);
        string fileBase = Path.GetFileNameWithoutExtension(fileName);
        string fileExt = Path.GetExtension(fileName).TrimStart('.');
        string timeStamp = DateTime.Now.ToString("yyyyMMddHHmmss");
        string time = DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString();

        ConsoleHelpers.PrintDebugLine($"filePath: {filePath}");
        ConsoleHelpers.PrintDebugLine($"fileBase: {fileBase}");
        ConsoleHelpers.PrintDebugLine($"fileExt: {fileExt}");
        ConsoleHelpers.PrintDebugLine($"timeStamp: {timeStamp}");

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
    
    public static IEnumerable<string> FilesFromGlobs(List<string> globs)
    {
        foreach (var glob in globs)
        {
            foreach (var file in FilesFromGlob(glob))
            {
                yield return file;
            }
        }
    }

    public static IEnumerable<string> FilesFromGlob(string glob)
    {
        ConsoleHelpers.PrintStatus($"Finding files: {glob} ...");
        try
        {
            if (glob == "-") return [ glob ]; // special case for stdin

            var matcher = new Microsoft.Extensions.FileSystemGlobbing.Matcher();
            matcher.AddInclude(MakeRelativePath(glob));

            var directoryInfo = new Microsoft.Extensions.FileSystemGlobbing.Abstractions.DirectoryInfoWrapper(new DirectoryInfo(Directory.GetCurrentDirectory()));
            var matchResult = matcher.Execute(directoryInfo);

            return matchResult.Files.Select(file => MakeRelativePath(Path.Combine(Directory.GetCurrentDirectory(), file.Path)));
        }
        catch (Exception)
        {
            return Enumerable.Empty<string>();
        }
        finally
        {
            ConsoleHelpers.PrintStatusErase();
        }
    }

    public static IEnumerable<string> FindMatchingFiles(
        List<string> globs,
        List<string> excludeGlobs,
        List<Regex> excludeFileNamePatternList,
        List<Regex> includeFileContainsPatternList,
        List<Regex> excludeFileContainsPatternList)
    {
        var excludeFiles = new HashSet<string>(FilesFromGlobs(excludeGlobs));
        var files = FilesFromGlobs(globs)
            .Where(file => !excludeFiles.Contains(file))
            .Where(file => !excludeFileNamePatternList.Any(regex => regex.IsMatch(Path.GetFileName(file))))
            .ToList();

        ConsoleHelpers.PrintDebugLine($"DEBUG: 1: Found files ({files.Count()}): ");
        files.ForEach(x => ConsoleHelpers.PrintDebugLine($"DEBUG: 1: - {x}"));
        ConsoleHelpers.PrintDebugLine("");

        if (files.Count == 0)
        {
            ConsoleHelpers.PrintLine($"## Pattern: {string.Join(" ", globs)}\n\n - No files found\n");
            return Enumerable.Empty<string>();
        }

        var filtered = files.Where(file => IsFileMatch(file, includeFileContainsPatternList, excludeFileContainsPatternList)).ToList();
        if (filtered.Count == 0)
        {
            ConsoleHelpers.PrintLine($"## Pattern: {string.Join(" ", globs)}\n\n - No files matched criteria\n");
            return Enumerable.Empty<string>();
        }

        var distinct = filtered.Distinct().ToList();
        ConsoleHelpers.PrintDebugLine($"DEBUG: 2: Found files ({distinct.Count()} distinct/filtered): ");
        distinct.ForEach(x => ConsoleHelpers.PrintDebugLine($"DEBUG: 2: - {x}"));

        return distinct;
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

    public static string ReadAllText(string fileName)
    {
        var content = fileName == "-"
            ? string.Join("\n", ConsoleHelpers.GetAllLinesFromStdin())
            : File.ReadAllText(fileName, Encoding.UTF8);

        return content;
    }

    public static string ReadAllText(string fileName, out bool isStdin, out bool isMarkdown, out bool isBinary)
    {
        isStdin = fileName == "-";
        isBinary = !isStdin && File.ReadAllBytes(fileName).Any(x => x == 0);
        isMarkdown = isBinary || fileName.EndsWith(".md", StringComparison.OrdinalIgnoreCase);

        return !isBinary
            ? FileHelpers.ReadAllText(fileName)
            : FileConverters.ConvertToMarkdown(fileName);
    }

    public static void WriteAllText(string fileName, string content)
    {
        EnsureDirectoryForFileExists(fileName);
        File.WriteAllText(fileName, content, Encoding.UTF8);
    }

    public static string GetFriendlyLastModified(FileInfo fileInfo)
    {
        var modified = fileInfo.LastWriteTime;
        var modifiedSeconds = (int)((DateTime.Now - modified).TotalSeconds);
        var modifiedMinutes = modifiedSeconds / 60;
        var modifiedHours = modifiedSeconds / 3600;
        var modifiedDays = modifiedSeconds / 86400;

        var formatted =
            modifiedMinutes < 1 ? "just now" :
            modifiedMinutes == 1 ? "1 minute ago" :
            modifiedMinutes < 60 ? $"{modifiedMinutes} minutes ago" :
            modifiedHours == 1 ? "1 hour ago" :
            modifiedHours < 24 ? $"{modifiedHours} hours ago" :
            modifiedDays == 1 ? "1 day ago" :
            modifiedDays < 7 ? $"{modifiedDays} days ago" :
            modified.ToString();

        return formatted;
    }

    public static string GetFriendlySize(FileInfo fileInfo)
    {
        var size = fileInfo.Length;
        var sizeFormatted = size >= 1024 * 1024 * 1024
            ? $"{size / (1024 * 1024 * 1024)} GB"
            : size >= 1024 * 1024
                ? $"{size / (1024 * 1024)} MB"
                : size >= 1024
                    ? $"{size / 1024} KB"
                    : $"{size} bytes";
        return sizeFormatted;
    }

    public static string GetMarkdownLanguage(string extension)
    {
        return extension switch
        {
            ".bat" => "batch",
            ".bmp" => "markdown",
            ".cpp" => "cpp",
            ".cs" => "csharp",
            ".csproj" => "xml",
            ".css" => "css",
            ".docx" => "markdown",
            ".gif" => "markdown",
            ".go" => "go",
            ".html" => "html",
            ".java" => "java",
            ".jpeg" => "markdown",
            ".jpg" => "markdown",
            ".js" => "javascript",
            ".json" => "json",
            ".kt" => "kotlin",
            ".m" => "objective-c",
            ".md" => "markdown",
            ".pdf" => "markdown",
            ".php" => "php",
            ".pl" => "perl",
            ".png" => "markdown",
            ".pptx" => "markdown",
            ".py" => "python",
            ".r" => "r",
            ".rb" => "ruby",
            ".rs" => "rust",
            ".scala" => "scala",
            ".sh" => "bash",
            ".sln" => "xml",
            ".sql" => "sql",
            ".swift" => "swift",
            ".ts" => "typescript",
            ".xml" => "xml",
            ".yaml" => "yaml",
            ".yml" => "yaml",
            _ => "plaintext"
        };
    }

    public static string GenerateUniqueFileNameFromUrl(string url, string saveToFolder)
    {
        FileHelpers.EnsureDirectoryExists(saveToFolder);

        var uri = new Uri(url);
        var path = uri.Host + uri.AbsolutePath + uri.Query;

        var parts = path.Split(_invalidFileNameCharsForWeb, StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .ToArray();

        var check = Path.Combine(saveToFolder, string.Join("-", parts));
        if (!File.Exists(check)) return check;

        while (true)
        {
            var guidPart = Guid.NewGuid().ToString().Substring(0, 8);
            var fileTimePart = DateTime.Now.ToFileTimeUtc().ToString();
            var tryThis = check + "-" + fileTimePart + "-" + guidPart;
            if (!File.Exists(tryThis)) return tryThis;
        }
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
        ConsoleHelpers.PrintDebugLine($"DEBUG: Embedded resources ({assembly.GetManifestResourceNames().Count()}):\n\n  {allResourceNames}\n");

        return false;
    }

    public static string ReadEmbeddedStream(string fileName)
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

    private static char[] GetInvalidFileNameCharsForWeb()
    {
        var invalidCharList = Path.GetInvalidFileNameChars().ToList();
        for (char c = (char)0; c < 128; c++)
        {
            if (!char.IsLetterOrDigit(c)) invalidCharList.Add(c);
        }
        return invalidCharList.Distinct().ToArray();
    }

    private static char[] _invalidFileNameCharsForWeb = GetInvalidFileNameCharsForWeb();
}
