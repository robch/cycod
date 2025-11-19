using System.ComponentModel;
using System.Text.RegularExpressions;

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
    [Description("View the content of a specific file, optionally with line ranges and content filtering.")]
    public string ViewFile(
        [Description("Absolute or relative path to file.")] string path,
        [Description("Start line number (1-indexed). Negative numbers count from end (-1 = last line). Default: 1")] int startLine = 1,
        [Description("End line number. 0 or -1 = end of file. Negative numbers count from end. Default: 0")] int endLine = 0,
        [Description("Include line numbers in output.")] bool lineNumbers = false,
        
        // New filtering options
        [Description("Only show lines containing this regex pattern. Applied after removeAllLines filter.")] string lineContains = "",
        [Description("Remove lines containing this regex pattern. Applied first, before other filters.")] string removeAllLines = "",
        [Description("Number of lines to show before and after lineContains matches.")] int linesBeforeAndAfter = 0,
        
        [Description("Maximum number of characters to display per line.")] int maxCharsPerLine = 500,
        [Description("Maximum total number of characters to display.")] int maxTotalChars = 100000)
    {
        // Basic file validation
        var noFile = Directory.Exists(path) || !File.Exists(path);
        if (noFile) return $"Path {path} does not exist or is not a file.";

        // Read all lines from file
        var allLines = FileHelpers.ReadAllText(path).Split('\n', StringSplitOptions.None)
            .Select(line => line.TrimEnd('\r'))
            .ToArray();
        
        var fileLineCount = allLines.Length;
        
        // Enhanced line number handling with negative indexing
        if (endLine == 0) endLine = -1;
        if (startLine < 0) startLine = Math.Max(1, fileLineCount + startLine + 1);
        if (endLine < 0) endLine = Math.Max(1, fileLineCount + endLine + 1);
        
        // Validate and clamp line numbers
        if (startLine <= 0) startLine = 1;
        startLine = Math.Min(startLine, fileLineCount);
        endLine = Math.Min(endLine, fileLineCount);
        
        if (startLine > fileLineCount) 
            return $"Invalid range: start line {startLine} exceeds file line count of {fileLineCount}";
        if (startLine > endLine) 
            return $"Invalid range: startLine ({startLine}) cannot be after endLine ({endLine})";
        
        // Apply initial line range
        var rangeStartIdx = startLine - 1;
        var rangeEndIdx = endLine - 1;
        var rangeLines = allLines.Skip(rangeStartIdx).Take(rangeEndIdx - rangeStartIdx + 1).ToArray();
        
        // STEP 1: Apply removeAllLines filtering FIRST (completely eliminate unwanted lines)
        Regex? removeAllLinesRegex = null;
        int[] currentLineNumbers;
        
        if (!string.IsNullOrEmpty(removeAllLines))
        {
            try
            {
                removeAllLinesRegex = new Regex(removeAllLines, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            }
            catch (Exception ex)
            {
                return $"Invalid regular expression pattern for removeAllLines: {removeAllLines} - {ex.Message}";
            }
            
            // Filter out removed lines completely
            var lineNumberMapping = new List<int>();
            var filteredLines = new List<string>();
            
            for (int i = 0; i < rangeLines.Length; i++)
            {
                if (!removeAllLinesRegex.IsMatch(rangeLines[i]))
                {
                    filteredLines.Add(rangeLines[i]);
                    lineNumberMapping.Add(startLine + i); // Original line number
                }
            }
            
            rangeLines = filteredLines.ToArray();
            
            // If no lines left after removal, handle early
            if (rangeLines.Length == 0)
                return "No lines remain after applying removeAllLines filter.";
                
            // If no further filtering needed, return cleaned content
            if (string.IsNullOrEmpty(lineContains))
            {
                return FormatAndTruncateLines(rangeLines, lineNumbers, lineNumberMapping.ToArray(), false, null, maxCharsPerLine, maxTotalChars);
            }
            
            // Store the line number mapping for later use
            currentLineNumbers = lineNumberMapping.ToArray();
        }
        else
        {
            // No removal, use sequential line numbers
            currentLineNumbers = Enumerable.Range(startLine, rangeLines.Length).ToArray();
        }
        
        // If no filtering, use the range as-is
        if (string.IsNullOrEmpty(lineContains))
        {
            var lineNumbersArray = Enumerable.Range(startLine, rangeLines.Length).ToArray();
            return FormatAndTruncateLines(rangeLines, lineNumbers, lineNumbersArray, false, null, maxCharsPerLine, maxTotalChars);
        }
        
        // STEP 2: Apply lineContains filtering on the cleaned content
        Regex lineContainsRegex;
        try
        {
            lineContainsRegex = new Regex(lineContains, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }
        catch (Exception ex)
        {
            return $"Invalid regular expression pattern for lineContains: {lineContains} - {ex.Message}";
        }
        
        // Find matching line indices within the cleaned range  
        var matchedLineIndices = rangeLines.Select((line, index) => new { line, index })
            .Where(x => lineContainsRegex.IsMatch(x.line))
            .Select(x => x.index)
            .ToList();
            
        if (matchedLineIndices.Count == 0) 
            return "No lines matched the specified criteria.";
        
        // STEP 3: Expand with context lines if requested (on cleaned content)
        var linesToInclude = new HashSet<int>(matchedLineIndices);
        if (linesBeforeAndAfter > 0)
        {
            foreach (var index in matchedLineIndices)
            {
                for (int b = 1; b <= linesBeforeAndAfter; b++)
                {
                    var idxBefore = index - b;
                    if (idxBefore >= 0) linesToInclude.Add(idxBefore);
                }
                for (int a = 1; a <= linesBeforeAndAfter; a++)
                {
                    var idxAfter = index + a;
                    if (idxAfter < rangeLines.Length) linesToInclude.Add(idxAfter);
                }
            }
        }
        
        var expandedLineIndices = linesToInclude.OrderBy(i => i).ToList();
        var selectedLines = expandedLineIndices.Select(i => rangeLines[i]).ToArray();
        var selectedLineNumbers = expandedLineIndices.Select(i => currentLineNumbers[i]).ToArray();
        
        // Determine if we should highlight matches
        var shouldHighlight = linesBeforeAndAfter > 0;
        var matchedIndicesSet = matchedLineIndices.ToHashSet();
        
        return FormatAndTruncateLines(selectedLines, lineNumbers, selectedLineNumbers, shouldHighlight, 
            expandedLineIndices.Select(i => matchedIndicesSet.Contains(i)).ToArray(), maxCharsPerLine, maxTotalChars);
    }
    
    /// <summary>
    /// Helper method to format lines with optional line numbers and highlighting, then apply truncation
    /// </summary>
    private static string FormatAndTruncateLines(string[] lines, bool lineNumbers, int[] lineNumbersArray, 
        bool shouldHighlight, bool[]? isMatchingLine, int maxCharsPerLine, int maxTotalChars)
    {
        var formattedLines = lines.Select((line, idx) =>
        {
            if (lineNumbers)
            {
                var actualLineNum = lineNumbersArray[idx];
                var isMatch = isMatchingLine?[idx] == true;
                var prefix = shouldHighlight && isMatch ? "*" : " ";
                return $"{prefix} {actualLineNum}: {line}";
            }
            else
            {
                var isMatch = isMatchingLine?[idx] == true;
                var prefix = shouldHighlight && isMatch ? "* " : "";
                return $"{prefix}{line}";
            }
        }).ToArray();
        
        var output = string.Join("\n", formattedLines);
        return TextTruncationHelper.TruncateOutput(output, maxCharsPerLine, maxTotalChars);
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
        
        var text = FileHelpers.ReadAllText(path);
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
        [Description("Absolute or relative path to file.")] string path,
        [Description("Line number (1-indexed) after which to insert the new string.")] int insertLine,
        [Description("The string to insert into the file.")] string newStr)
    {
        if (!File.Exists(path))
        {
            return $"File {path} does not exist.";
        }
        var text = FileHelpers.ReadAllText(path);
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
        [Description("Absolute or relative path to file.")] string path)
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
