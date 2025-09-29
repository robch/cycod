using System.ComponentModel;

/// <summary>
/// Provides file and directory editing functionality similar to the TypeScript str_replace_editor tool.
/// State is maintained across calls to allow undo of the previous file edit.
/// </summary>
public class StrReplaceEditorHelperFunctions
{
    [ReadOnly(true)]
    [Description("Returns a list of non-hidden files and directories up to 2 levels deep.")]
    public string ListFiles([Description("Absolute or relative path to directory.")] string path)
    {
        if (Directory.Exists(path))
        {
            path = Path.GetFullPath(path);
            var entries = Directory.GetFileSystemEntries(path)
                .Select(entry => Path.GetFullPath(Path.Combine(path, entry)))
                .SelectMany(entry => new [] { entry }
                    .Concat(!Directory.Exists(entry)
                        ? new string[] { }
                        : Directory.GetFileSystemEntries(entry)
                            .Select(subEntry => Path.GetFullPath(Path.Combine(entry, subEntry)))))
                .Select(entry => Directory.Exists(entry) ? $"{entry} (directory)" : $"{entry}")
                .Select(entry => entry.StartsWith(path)
                    ? entry.Substring(path.Length + (path.EndsWith(Path.DirectorySeparatorChar.ToString()) ? 0 : 1))
                    : entry);

            var joined = string.Join(Environment.NewLine, entries);
            ConsoleHelpers.WriteDebugLine($"Listing of {path}:\n{joined}");

            return joined;
        }
        else
        {
            return $"Path {path} does not exist or is not a directory.";
        }
    }

    [ReadOnly(true)]
    [Description("If `path` is a file, returns the file content (optionally in a specified line range) with line numbers.")]
    public string ViewFile(
        [Description("Absolute or relative path to file or directory.")] string path,
        [Description("Start line number (1-indexed) to view. Default: 1")] int startLine = 1,
        [Description("End line number. Use 0 or -1 to view all remaining lines. Default: 0")] int endLine = 0,
        [Description("Optional flag to view the file with line numbers.")] bool lineNumbers = false,
        [Description("Maximum number of characters to display per line.")] int maxCharsPerLine = 500,
        [Description("Maximum total number of characters to display.")] int maxTotalChars = 100000)
    {
        var noFile = Directory.Exists(path) || !File.Exists(path);
        if (noFile) return $"Path {path} does not exist or is not a file.";

        var linesFromFile = File.ReadAllText(path).Split('\n', StringSplitOptions.None)
            .Select(line => line.TrimEnd('\r'))
            .ToArray();
            
        startLine = Math.Max(1, startLine);
        var invalidStart = startLine > linesFromFile.Length;
        if (invalidStart) return $"Invalid range: start line {startLine} exceeds file line count of {linesFromFile.Length}";
        
        endLine = (endLine <= 0) ? linesFromFile.Length : Math.Min(endLine, linesFromFile.Length);
        var requestedLines = linesFromFile.Skip(startLine - 1)
            .Take(endLine - startLine + 1)
            .ToArray();
            
        var needLineTruncs = requestedLines.Any(line => line.Length > maxCharsPerLine);
        var linesAfterTrunc = requestedLines.Select(line => 
            needLineTruncs && line.Length > maxCharsPerLine
                ? line.Substring(0, maxCharsPerLine - 1) + "…" 
                : line)
            .ToArray();
        
        var formatted = string.Join("\n", lineNumbers
            ? linesAfterTrunc.Select((line, idx) => $"{startLine + idx}: {line}")
            : linesAfterTrunc);
            
        var needTotalTrunc = formatted.Length > maxTotalChars;
        var formattedAfterTrunc = needTotalTrunc
            ? formatted.Substring(0, maxTotalChars - 1) + "…"
            : formatted;
        
        var noTruncations = !needLineTruncs && !needTotalTrunc;
        if (noTruncations) return formattedAfterTrunc;
        
        var onlyLinesTrunc = needLineTruncs && !needTotalTrunc;
        if (onlyLinesTrunc) return formattedAfterTrunc + "\n" + $"[Note: Lines with … were truncated ({maxCharsPerLine} char limit)]";
        
        var formattedLineCount = linesAfterTrunc.Count();
        var truncatedCount = formattedLineCount - formattedAfterTrunc.Split('\n').Length;

        var warning = needLineTruncs
            ? $"[{truncatedCount} more lines truncated; lines with … exceeded {maxCharsPerLine} char limit]"
            : $"[{truncatedCount} more lines truncated ({maxTotalChars} char total limit)]";

        return formattedAfterTrunc + "\n" + warning;
    }

    [ReadOnly(false)]
    [Description("Creates a new file at the specified path with the given content. The `create` command cannot be used if the file already exists.")]
    public string CreateFile(
        [Description("Absolute or relative path to file.")] string path,
        [Description("Content to be written to the file.")] string fileText)
    {
        if (File.Exists(path))
        {
            return $"Path {path} already exists; cannot create file. Use ViewFile and then StrReplace to edit the file.";
        }
        DirectoryHelpers.EnsureDirectoryForFileExists(path);
        File.WriteAllText(path, fileText);
        return $"Created file {path} with {fileText.Length} characters.";
    }

    // [ReadOnly(false)]
    // [Description("Replaces the lines in the file at `path` from `startLine` to `endLine` with the new string `newStr`. If `endLine` is -1, all remaining lines are replaced.")]
    // public string LinesReplace(
    //     [Description("Absolute or relative path to file.")] string path,
    //     [Description("Optional start line number (1-indexed) to view.")] int startLine,
    //     [Description("Optional end line number. Use -1 to view all remaining lines.")] int endLine,
    //     [Description("New string content that will replace the lines.")] string newStr)
    // {
    //     if (!File.Exists(path))
    //     {
    //         return $"File {path} does not exist.";
    //     }

    //     var text = File.ReadAllText(path);
    //     var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();
    //     if (startLine < 0 || startLine > lines.Count)
    //     {
    //         return $"Invalid line number: {startLine}; file has {lines.Count} lines.";
    //     }

    //     // Save current text for undo.
    //     if (!EditHistory.ContainsKey(path))
    //     {
    //         EditHistory[path] = text;
    //     }

    //     // Replace lines with new string.
    //     var newLines = newStr.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();
    //     if (endLine == -1)
    //     {
    //         endLine = lines.Count;
    //     }
    //     else if (endLine < startLine || endLine > lines.Count)
    //     {
    //         return $"Invalid range: start line {startLine} and end line {endLine} exceed file line count of {lines.Count}";
    //     }
    //     lines.RemoveRange(startLine - 1, endLine - startLine + 1);
    //     lines.InsertRange(startLine - 1, newLines);
    //     var newText = string.Join(Environment.NewLine, lines);
    //     File.WriteAllText(path, newText);
    //     return $"Replaced lines {startLine} to {endLine} in {path} with {newStr.Length} characters.";
    // }

    [ReadOnly(false)]
    [Description("Replaces the text specified by `oldStr` with `newStr` in the file at `path`. If the provided old string is not unique, no changes are made.")]
    public string StrReplace(
        [Description("Absolute or relative path to file.")] string path,
        [Description("Existing text in the file that should be replaced. Must match exactly one occurrence.")] string oldStr,
        [Description("New string content that will replace the old string.")] string newStr)
    {
        if (!File.Exists(path))
        {
            return $"File {path} does not exist.";
        }
        
        var text = File.ReadAllText(path);
        var replaced = StringHelpers.ReplaceOnce(text, oldStr, newStr, out var countFound);
        if (countFound != 1)
        {
            return countFound == 0
                ? $"No occurrences of '{oldStr}' found in {path}; no changes made."
                : $"Multiple matches found for '{oldStr}' in {path}; no changes made. Please be more specific.";
        }

        if (!EditHistory.ContainsKey(path))
        {
            EditHistory[path] = text;
        }

        File.WriteAllText(path, replaced);
        return $"File {path} updated: replaced 1 occurrence of specified text.";
    }

    [ReadOnly(false)]
    [Description("Inserts the specified string `newStr` into the file at `path` after the specified line number (`insertLine`). Use 0 to insert at the beginning of the file.")]
    public string Insert(
        [Description("Absolute path to file.")] string path,
        [Description("Line number (1-indexed) after which to insert the new string.")] int insertLine,
        [Description("The string to insert into the file.")] string newStr)
    {
        if (!File.Exists(path))
        {
            return $"File {path} does not exist.";
        }
        var text = File.ReadAllText(path);
        var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();
        if (insertLine < 0 || insertLine > lines.Count)
        {
            return $"Invalid line number: {insertLine}; file has {lines.Count} lines.";
        }
        // Save current text for undo.
        if (!EditHistory.ContainsKey(path))
        {
            EditHistory[path] = text;
        }
        // Insert new string after specified line (if insertLine==lines.Count, it appends at the end).
        lines.Insert(insertLine, newStr);
        var newText = string.Join(Environment.NewLine, lines);
        File.WriteAllText(path, newText);
        return $"Inserted {newStr.Length} characters at line {insertLine} in {path}.";
    }

    [ReadOnly(false)]
    [Description("Reverts the last edit made to the file at `path`, undoing the last change if available.")]
    public string UndoEdit(
        [Description("Absolute path to file.")] string path)
    {
        if (!EditHistory.ContainsKey(path))
        {
            return $"No previous edit found for {path}.";
        }
        var previousText = EditHistory[path];
        File.WriteAllText(path, previousText);
        EditHistory.Remove(path);
        return $"Reverted last edit made to {path}.";
    }

    private readonly Dictionary<string, string> EditHistory = new Dictionary<string, string>();
}
