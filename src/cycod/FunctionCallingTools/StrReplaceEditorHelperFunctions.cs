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
        path = PathHelpers.ExpandPath(path);

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
        path = PathHelpers.ExpandPath(path);
        
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

    [ReadOnly(false)]
    [Description("Replaces the entire content of an existing file. Requires knowing the current line count as verification that you've read the file.")]
    public string ReplaceFileContent(
        [Description("Absolute or relative path to file.")] string path,
        [Description("New content to replace the entire file.")] string newContent,
        [Description("Current line count of the file (for verification).")] int oldContentLineCount)
    {
        
        if (!File.Exists(path))
        {
            return $"File {path} does not exist. Use CreateFile to create a new file.";
        }

        // Read current content and count lines
        var currentContent = File.ReadAllText(path);
        var currentLines = currentContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        var actualLineCount = currentLines.Length;

        // Verify line count matches
        if (actualLineCount != oldContentLineCount)
        {
            return $"Line count mismatch: you specified {oldContentLineCount} but file has {actualLineCount} lines. Please read the file with ViewFile and verify the line count.";
        }

        // Save current content for undo
        if (!EditHistory.ContainsKey(path))
        {
            EditHistory[path] = currentContent;
        }

        // Replace entire content
        File.WriteAllText(path, newContent);
        var newLines = newContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        
        return $"Replaced content in {path}: {actualLineCount} lines → {newLines.Length} lines ({newContent.Length} characters).";
    }


    [ReadOnly(false)]
    [Description("Replaces the lines in the file at `path` from `startLine` to `endLine` with the new string `newStr`. If `endLine` is -1, all remaining lines are replaced.")]
    public string LinesReplace(
        [Description("Absolute or relative path to file.")] string path,
        [Description("Optional start line number (1-indexed) to view.")] int startLine,
        [Description("Optional end line number. Use -1 to view all remaining lines.")] int endLine,
        [Description("New string content that will replace the lines.")] string newStr)
    {
        if (!File.Exists(path))
        {
            return $"File {path} does not exist.";
        }

        var text = File.ReadAllText(path);
        var lines = text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();
        if (startLine < 0 || startLine > lines.Count)
        {
            return $"Invalid line number: {startLine}; file has {lines.Count} lines.";
        }

        // Save current text for undo.
        if (!EditHistory.ContainsKey(path))
        {
            EditHistory[path] = text;
        }

        // Replace lines with new string.
        var newLines = newStr.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();
        if (endLine == -1)
        {
            endLine = lines.Count;
        }
        else if (endLine < startLine || endLine > lines.Count)
        {
            return $"Invalid range: start line {startLine} and end line {endLine} exceed file line count of {lines.Count}";
        }
        lines.RemoveRange(startLine - 1, endLine - startLine + 1);
        lines.InsertRange(startLine - 1, newLines);
        var newText = string.Join(Environment.NewLine, lines);
        File.WriteAllText(path, newText);
        return $"Replaced lines {startLine} to {endLine} in {path} with {newStr.Length} characters.";
    }


    /// <summary>
    /// Finds all occurrences of a pattern and formats them with context for display
    /// </summary>
    private string FormatMultipleMatches(string filePath, string fileContent, string oldStr, int contextLines = 3)
    {
        var lines = fileContent.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        var sb = new StringBuilder();
        
        // Find all matches - try exact matching first
        var matches = new List<int>(); // List of character positions where matches start
        
        // Try exact matching
        int pos = 0;
        while ((pos = fileContent.IndexOf(oldStr, pos, StringComparison.Ordinal)) != -1)
        {
            matches.Add(pos);
            pos += oldStr.Length;
        }
        
        // If no exact matches, try fuzzy matching
        if (matches.Count == 0)
        {
            var escapedLines = oldStr
                .Split('\n', StringSplitOptions.None)
                .Select(line => line.TrimEnd('\r').TrimEnd())
                .Select(line => Regex.Escape(line))
                .ToArray();

            var linesWithTrailingWsPattern = escapedLines
                .Take(escapedLines.Length - 1)
                .Select(line => $"{line}[\\t\\f\\v\\r\\u00A0\\u2000-\\u200A\\u2028\\u2029\\u3000 ]*$")
                .Concat(new[] { escapedLines.Last() });

            var joined = string.Join('\n', linesWithTrailingWsPattern);
            var pattern = new Regex(joined, RegexOptions.Multiline | RegexOptions.CultureInvariant);
            var regexMatches = pattern.Matches(fileContent);
            
            foreach (Match match in regexMatches)
            {
                matches.Add(match.Index);
            }
        }
        
        if (matches.Count == 0)
        {
            return $"No occurrences found (this shouldn't happen - internal error)";
        }
        
        sb.AppendLine($"Found {matches.Count} matches in {filePath}:");
        sb.AppendLine();
        
        for (int matchIdx = 0; matchIdx < matches.Count; matchIdx++)
        {
            var matchPos = matches[matchIdx];
            
            // Find which line this match is on
            int currentPos = 0;
            int lineNum = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                int lineLength = lines[i].Length + Environment.NewLine.Length; // Account for newline
                if (currentPos + lineLength > matchPos)
                {
                    lineNum = i + 1; // 1-indexed
                    break;
                }
                currentPos += lineLength;
            }
            
            sb.AppendLine($"Match {matchIdx + 1} at line {lineNum}:");
            
            // Show context lines
            int startLine = Math.Max(0, lineNum - 1 - contextLines);
            int endLine = Math.Min(lines.Length - 1, lineNum - 1 + contextLines);
            
            for (int i = startLine; i <= endLine; i++)
            {
                var prefix = (i == lineNum - 1) ? "*" : " ";
                sb.AppendLine($"{prefix} {i + 1,4}: {lines[i]}");
            }
            
            if (matchIdx < matches.Count - 1)
            {
                sb.AppendLine();
            }
        }
        
        sb.AppendLine();
        sb.AppendLine("To replace a specific occurrence, you have two options:");
        sb.AppendLine();
        sb.AppendLine("Option 1 (Recommended): Use the 'which' parameter to specify which occurrence to replace:");
        sb.AppendLine($"  ReplaceOneInFile(path: \"{filePath}\", oldStr: \"...\", newStr: \"...\", which: 1)  // Replace 1st occurrence");
        sb.AppendLine($"  ReplaceOneInFile(path: \"{filePath}\", oldStr: \"...\", newStr: \"...\", which: 2)  // Replace 2nd occurrence");
        sb.AppendLine($"  ReplaceOneInFile(path: \"{filePath}\", oldStr: \"...\", newStr: \"...\", which: -1) // Replace last occurrence");
        sb.AppendLine();
        sb.AppendLine("Option 2: Add more surrounding context to your oldStr parameter to make it unique.");
        sb.AppendLine("  For example, include the lines before and after the target match.");
        
        return sb.ToString();
        
        return sb.ToString();
    }


    /// <summary>
    /// Replaces the Nth occurrence of a pattern in text
    /// </summary>
    private string? ReplaceNthOccurrence(string text, string oldStr, string newStr, int which, out int totalFound)
    {
        totalFound = 0;
        
        // Find all matches using exact matching first
        var matches = new List<int>();
        int pos = 0;
        while ((pos = text.IndexOf(oldStr, pos, StringComparison.Ordinal)) != -1)
        {
            matches.Add(pos);
            pos += oldStr.Length;
        }
        
        // If no exact matches, try fuzzy matching
        if (matches.Count == 0)
        {
            var escapedLines = oldStr
                .Split('\n', StringSplitOptions.None)
                .Select(line => line.TrimEnd('\r').TrimEnd())
                .Select(line => Regex.Escape(line))
                .ToArray();

            var linesWithTrailingWsPattern = escapedLines
                .Take(escapedLines.Length - 1)
                .Select(line => $"{line}[\\t\\f\\v\\r\\u00A0\\u2000-\\u200A\\u2028\\u2029\\u3000 ]*$")
                .Concat(new[] { escapedLines.Last() });

            var joined = string.Join('\n', linesWithTrailingWsPattern);
            var pattern = new Regex(joined, RegexOptions.Multiline | RegexOptions.CultureInvariant);
            var regexMatches = pattern.Matches(text);
            
            foreach (Match match in regexMatches)
            {
                matches.Add(match.Index);
            }
        }
        
        totalFound = matches.Count;
        
        if (totalFound == 0)
        {
            return null;
        }
        
        // Handle negative indices (e.g., -1 = last)
        int targetIndex;
        if (which < 0)
        {
            targetIndex = totalFound + which; // -1 becomes count-1 (last), -2 becomes count-2, etc.
        }
        else
        {
            targetIndex = which - 1; // Convert from 1-indexed to 0-indexed
        }
        
        // Validate index
        if (targetIndex < 0 || targetIndex >= totalFound)
        {
            return null; // Invalid index
        }
        
        // Get the position of the target match
        int matchPos = matches[targetIndex];
        
        // Determine the length of the match (need to handle fuzzy matches)
        int matchLength;
        if (text.Substring(matchPos).StartsWith(oldStr))
        {
            // Exact match
            matchLength = oldStr.Length;
        }
        else
        {
            // Fuzzy match - need to find the actual match length
            var escapedLines = oldStr
                .Split('\n', StringSplitOptions.None)
                .Select(line => line.TrimEnd('\r').TrimEnd())
                .Select(line => Regex.Escape(line))
                .ToArray();

            var linesWithTrailingWsPattern = escapedLines
                .Take(escapedLines.Length - 1)
                .Select(line => $"{line}[\\t\\f\\v\\r\\u00A0\\u2000-\\u200A\\u2028\\u2029\\u3000 ]*$")
                .Concat(new[] { escapedLines.Last() });

            var joined = string.Join('\n', linesWithTrailingWsPattern);
            var pattern = new Regex(joined, RegexOptions.Multiline | RegexOptions.CultureInvariant);
            var match = pattern.Match(text, matchPos);
            matchLength = match.Length;
        }
        
        // Perform the replacement
        var result = text.Substring(0, matchPos) + newStr + text.Substring(matchPos + matchLength);
        return result;
    }

    [ReadOnly(false)]
    [Description("Replaces the text specified by `oldStr` with `newStr` in the file at `path`. If the provided old string is not unique and `which` is not specified, no changes are made.")]
    public string ReplaceOneInFile(
        [Description("Absolute or relative path to file.")] string path,
        [Description("Existing text in the file that should be replaced.")] string oldStr,
        [Description("New string content that will replace the old string.")] string newStr,
        [Description("Which occurrence to replace (1 = first, 2 = second, -1 = last, null = must be unique). Default: null")] int? which = null)
    {
        if (!File.Exists(path))
        {
            return $"File {path} does not exist.";
        }
        
        var text = FileHelpers.ReadAllText(path);
        
        // If which is specified, use ReplaceNthOccurrence
        if (which.HasValue)
        {
            var replaced = ReplaceNthOccurrence(text, oldStr, newStr, which.Value, out var totalFound);
            
            if (totalFound == 0)
            {
                return $"No occurrences of '{oldStr}' found in {path}; no changes made.";
            }
            
            if (replaced == null)
            {
                // Invalid index
                var indexDesc = which.Value < 0 ? $"{which.Value} (counting from end)" : which.Value.ToString();
                return $"Invalid occurrence index {indexDesc}. Found {totalFound} total occurrence(s) in {path}.\n\n" +
                       FormatMultipleMatches(path, text, oldStr);
            }
            
            // Success - save undo and write file
            if (!EditHistory.ContainsKey(path))
            {
                EditHistory[path] = text;
            }
            
            File.WriteAllText(path, replaced);
            var occDesc = which.Value < 0 ? $"occurrence {totalFound + which.Value + 1} (index {which.Value})" : $"occurrence {which.Value}";
            return $"File {path} updated: replaced {occDesc} of {totalFound} total occurrences.";
        }
        
        // Original behavior when which is not specified - must be unique
        var replacedUnique = StringHelpers.ReplaceOnce(text, oldStr, newStr, out var countFound);
        if (countFound != 1)
        {
            if (countFound == 0)
            {
                return $"No occurrences of '{oldStr}' found in {path}; no changes made.";
            }
            else
            {
                // Multiple matches found - show them with context
                return FormatMultipleMatches(path, text, oldStr);
            }
        }

        if (!EditHistory.ContainsKey(path))
        {
            EditHistory[path] = text;
        }

        File.WriteAllText(path, replacedUnique);
        return $"File {path} updated: replaced 1 occurrence of specified text.";
    }

    [ReadOnly(false)]
    [Description("Replaces multiple text patterns in a single file atomically. All replacements must be unique occurrences. Inputs are a sequence of one-to-one")]
    public string ReplaceMultipleInFile(
        [Description("Absolute or relative path to file.")] string path,
        [Description("Array of old strings to be replaced. Each must match exactly one occurrence.")] string[] oldStrings,
        [Description("Array of new strings to replace with. Must be same length as oldStrings.")] string[] newStrings)
    {
        
        if (!File.Exists(path))
        {
            return $"File {path} does not exist.";
        }

        if (oldStrings.Length != newStrings.Length)
        {
            return $"Error: oldStrings array length ({oldStrings.Length}) must match newStrings array length ({newStrings.Length}).";
        }

        if (oldStrings.Length == 0)
        {
            return "Error: No replacements specified.";
        }

        var originalText = FileHelpers.ReadAllText(path);
        var currentText = originalText;

        // Validate all patterns exist and are unique before making any changes
        for (int i = 0; i < oldStrings.Length; i++)
        {
            var oldStr = oldStrings[i];
            var newStr = newStrings[i];
            
            var testReplacement = StringHelpers.ReplaceOnce(currentText, oldStr, newStr, out var countFound);
            if (countFound != 1)
            {
                var message = countFound == 0
                    ? $"Replacement {i + 1}: No occurrences of specified text found."
                    : $"Replacement {i + 1}: Multiple matches found for specified text; must be unique.";
                return $"{message}\nNo changes made to {path}.";
            }
        }

        // All validations passed, now perform the replacements
        for (int i = 0; i < oldStrings.Length; i++)
        {
            var oldStr = oldStrings[i];
            var newStr = newStrings[i];
            
            var replacedText = StringHelpers.ReplaceOnce(currentText, oldStr, newStr, out var countFound);
            if (replacedText != null && countFound == 1)
            {
                currentText = replacedText;
            }
            else
            {
                // This shouldn't happen since we validated, but handle gracefully
                return $"Unexpected error during replacement {i + 1}. File may be in inconsistent state.";
            }
        }

        // Save original content for undo before writing
        if (!EditHistory.ContainsKey(path))
        {
            EditHistory[path] = originalText;
        }

        File.WriteAllText(path, currentText);
        
        return $"File {path} updated: replaced {oldStrings.Length} occurrences.";
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
