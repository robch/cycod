using System.ComponentModel;
using System.Text;
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
                return FormatAndTruncateLines(rangeLines, lineNumbers, lineNumberMapping.ToArray(), false, null, maxCharsPerLine, maxTotalChars, fileLineCount);
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
            return FormatAndTruncateLines(rangeLines, lineNumbers, lineNumbersArray, false, null, maxCharsPerLine, maxTotalChars, fileLineCount);
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
            expandedLineIndices.Select(i => matchedIndicesSet.Contains(i)).ToArray(), maxCharsPerLine, maxTotalChars, fileLineCount);
    }
    
    /// <summary>
    /// Helper method to format lines with optional line numbers and highlighting, then apply truncation
    /// </summary>
    private static string FormatAndTruncateLines(string[] lines, bool lineNumbers, int[] lineNumbersArray, 
        bool shouldHighlight, bool[]? isMatchingLine, int maxCharsPerLine, int maxTotalChars, int fileLineCount)
    {
        var sb = new StringBuilder();
        int firstLine = lineNumbersArray[0];
        int lastLine = firstLine;
        int linesShown = 0;
        bool wasTruncated = false;
        bool anyLineTruncated = false;
        
        for (int i = 0; i < lines.Length; i++)
        {
            // Format this line
            string formattedLine;
            if (lineNumbers)
            {
                var actualLineNum = lineNumbersArray[i];
                var isMatch = isMatchingLine?[i] == true;
                var prefix = shouldHighlight && isMatch ? "*" : " ";
                
                var content = lines[i];
                if (content.Length > maxCharsPerLine)
                {
                    content = content.Substring(0, maxCharsPerLine) + "…";
                    anyLineTruncated = true;
                }
                
                formattedLine = $"{prefix} {actualLineNum}: {content}";
            }
            else
            {
                var isMatch = isMatchingLine?[i] == true;
                var prefix = shouldHighlight && isMatch ? "* " : "";
                
                var content = lines[i];
                if (content.Length > maxCharsPerLine)
                {
                    content = content.Substring(0, maxCharsPerLine) + "…";
                    anyLineTruncated = true;
                }
                
                formattedLine = $"{prefix}{content}";
            }
            
            // Check if adding this line would exceed total limit
            var potentialLength = sb.Length + formattedLine.Length + (sb.Length > 0 ? 1 : 0); // +1 for newline if not first
            if (potentialLength > maxTotalChars && linesShown > 0)
            {
                wasTruncated = true;
                break;
            }
            
            // Add the line
            if (sb.Length > 0)
            {
                sb.AppendLine();
            }
            sb.Append(formattedLine);
            
            lastLine = lineNumbersArray[i];
            linesShown++;
        }
        
        // Add metadata
        sb.AppendLine();
        sb.AppendLine();
        sb.Append($"[Showing lines {firstLine}-{lastLine}");
        
        if (wasTruncated)
        {
            var linesNotShown = lines.Length - linesShown;
            sb.Append($" (truncated: {linesNotShown} more matched lines not shown)");
        }
        
        sb.Append($" of {fileLineCount} total]");
        
        if (lastLine >= fileLineCount)
        {
            sb.Append(" [End of file]");
        }
        else
        {
            var remaining = fileLineCount - lastLine;
            sb.Append($" [{remaining} lines remaining]");
        }
        
        if (anyLineTruncated)
        {
            sb.AppendLine();
            sb.Append($"[Note: Some lines were truncated to {maxCharsPerLine} chars]");
        }
        
        return sb.ToString();
    }

    [ReadOnly(false)]
    [Description("Creates a new file at the specified path with the given content. The `create` command cannot be used if the file already exists.")]
    public string CreateFile(
        [Description("Absolute or relative path to file.")] string path,
        [Description("Content to be written to the file.")] string fileText)
    {
        if (File.Exists(path))
        {
            return $"Path {path} already exists; cannot create file. Use ViewFile and then ReplaceOneInFile to edit the file.";
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
    public string ReplaceOneInFile(
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
    
    [ReadOnly(false)]
    [Description("Replace text across multiple files with preview and bulk operation capabilities. Supports both literal text and regex patterns.")]
    public async Task<string> ReplaceAllInFiles(
        [Description("File glob patterns to search (e.g., **/*.cs, src/*.md)")] string[] filePatterns,
        [Description("File glob patterns to exclude")] string[]? excludePatterns = null,
        [Description("Only include files containing this regex pattern")] string fileContains = "",
        [Description("Exclude files containing this regex pattern")] string fileNotContains = "",
        [Description("Only files modified after this time (e.g., '3d', '2023-01-01')")] string modifiedAfter = "",
        [Description("Only files modified before this time")] string modifiedBefore = "",
        [Description("Maximum number of files to process.")] int maxFiles = 50,
        [Description("Text or regex pattern to find")] string old = "",
        [Description("Replacement text")] string @new = "",
        [Description("Use regex patterns instead of literal text")] bool useRegex = false,
        [Description("Preview mode - show what would be replaced without making changes")] bool preview = true)
    {
        Logger.Info($"ReplaceAllInFiles called with filePatterns: [{string.Join(", ", filePatterns)}]");
        Logger.Info($"Replacing '{old}' with '{@new}' (useRegex: {useRegex}, preview: {preview})");

        if (string.IsNullOrEmpty(old))
        {
            return "Error: 'old' parameter cannot be empty.";
        }
        
        if (string.IsNullOrEmpty(@new))
        {
            return "Error: 'new' parameter cannot be empty.";
        }

        try
        {
            // For preview mode, just call cycodmd to show diff
            if (preview)
            {
                return await CallCycoDmdForPreview(filePatterns, excludePatterns, fileContains, 
                    fileNotContains, modifiedAfter, modifiedBefore, old, @new, useRegex);
            }
            else
            {
                // For execute mode, get file list first, store undo history, then execute
                return await ExecuteReplacementWithUndo(filePatterns, excludePatterns, fileContains, 
                    fileNotContains, modifiedAfter, modifiedBefore, old, @new, useRegex);
            }
        }
        catch (Exception ex)
        {
            Logger.Error($"Error in ReplaceAllInFiles: {ex.Message}");
            return $"Error performing replacement: {ex.Message}";
        }
    }
    
    /// <summary>
    /// Calls cycodmd for preview mode (shows diff without making changes)
    /// </summary>
    private async Task<string> CallCycoDmdForPreview(
        string[] filePatterns, string[]? excludePatterns, string fileContains, 
        string fileNotContains, string modifiedAfter, string modifiedBefore,
        string old, string @new, bool useRegex)
    {
        var searchPattern = useRegex ? old : System.Text.RegularExpressions.Regex.Escape(old);
        
        var arguments = BuildCycoDmdArguments(filePatterns, excludePatterns, fileContains,
            fileNotContains, modifiedAfter, modifiedBefore, searchPattern, @new, executeMode: false);
        
        Logger.Info($"Calling cycodmd for preview with arguments: {arguments}");
        return await _cycoDmdWrapper.ExecuteRawCycoDmdCommandAsync(arguments);
    }
    
    /// <summary>
    /// Executes replacement with undo history integration
    /// </summary>
    private async Task<string> ExecuteReplacementWithUndo(
        string[] filePatterns, string[]? excludePatterns, string fileContains, 
        string fileNotContains, string modifiedAfter, string modifiedBefore,
        string old, string @new, bool useRegex)
    {
        // First, get list of files that will be affected
        var findFilesArgs = $"find-files {string.Join(" ", filePatterns.Select(p => $"\"{p}\""))}";
        
        if (excludePatterns?.Length > 0)
            findFilesArgs += $" --exclude {string.Join(" ", excludePatterns.Select(p => $"\"{p}\""))}";
        if (!string.IsNullOrEmpty(fileContains))
            findFilesArgs += $" --file-contains \"{fileContains}\"";
        if (!string.IsNullOrEmpty(fileNotContains))
            findFilesArgs += $" --file-not-contains \"{fileNotContains}\"";
        if (!string.IsNullOrEmpty(modifiedAfter))
            findFilesArgs += $" --modified-after \"{modifiedAfter}\"";
        if (!string.IsNullOrEmpty(modifiedBefore))
            findFilesArgs += $" --modified-before \"{modifiedBefore}\"";
        
        findFilesArgs += " --files-only";
        
        var fileListOutput = await _cycoDmdWrapper.ExecuteRawCycoDmdCommandAsync(findFilesArgs);
        var filesToProcess = fileListOutput
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Where(f => !string.IsNullOrWhiteSpace(f))
            .ToList();
        
        if (filesToProcess.Count == 0)
        {
            return "No files found matching the specified criteria.";
        }
        
        // Store original content for undo (only for files that will actually change)
        var searchPattern = useRegex ? old : System.Text.RegularExpressions.Regex.Escape(old);
        var regex = new System.Text.RegularExpressions.Regex(searchPattern);
        var filesWithChanges = new List<string>();
        
        foreach (var file in filesToProcess)
        {
            if (File.Exists(file))
            {
                var content = await File.ReadAllTextAsync(file);
                if (regex.IsMatch(content))
                {
                    // Store original content for undo
                    EditHistory[file] = content;
                    filesWithChanges.Add(file);
                    Logger.Info($"Stored undo history for: {file}");
                }
            }
        }
        
        if (filesWithChanges.Count == 0)
        {
            return "No files contain the specified search text.";
        }
        
        // Now execute the replacement via cycodmd
        var executeArgs = BuildCycoDmdArguments(filePatterns, excludePatterns, fileContains,
            fileNotContains, modifiedAfter, modifiedBefore, searchPattern, @new, executeMode: true);
        
        Logger.Info($"Executing replacement with undo history stored for {filesWithChanges.Count} files");
        var result = await _cycoDmdWrapper.ExecuteRawCycoDmdCommandAsync(executeArgs);
        
        return result + $"\n\nUndo history stored for {filesWithChanges.Count} file(s). Use UndoEdit to revert individual files.";
    }
    
    /// <summary>
    /// Builds cycodmd command arguments for replacement operations
    /// </summary>
    private string BuildCycoDmdArguments(
        string[] filePatterns, string[]? excludePatterns, string fileContains,
        string fileNotContains, string modifiedAfter, string modifiedBefore,
        string searchPattern, string replacementText, bool executeMode)
    {
        var arguments = $"find-files {string.Join(" ", filePatterns.Select(p => $"\"{p}\""))}";
        
        if (excludePatterns?.Length > 0)
            arguments += $" --exclude {string.Join(" ", excludePatterns.Select(p => $"\"{p}\""))}";
        
        if (!string.IsNullOrEmpty(fileContains))
            arguments += $" --file-contains \"{fileContains}\"";
        if (!string.IsNullOrEmpty(fileNotContains))
            arguments += $" --file-not-contains \"{fileNotContains}\"";
        if (!string.IsNullOrEmpty(modifiedAfter))
            arguments += $" --modified-after \"{modifiedAfter}\"";
        if (!string.IsNullOrEmpty(modifiedBefore))
            arguments += $" --modified-before \"{modifiedBefore}\"";
        
        arguments += $" --contains \"{searchPattern}\"";
        arguments += $" --replace-with \"{replacementText}\"";
        
        if (executeMode)
            arguments += " --execute";
        
        return arguments;
    }

    private readonly CycoDmdCliWrapper _cycoDmdWrapper = new CycoDmdCliWrapper();
    private readonly Dictionary<string, string> EditHistory = new Dictionary<string, string>();
}
