using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;

/// <summary>
/// Provides access to in-memory logs for AI debugging and troubleshooting.
/// </summary>
public class MemoryLogHelperFunctions
{
    [ReadOnly(true)]
    [Description("Get current in-memory logs with filtering and range selection for debugging AI function calls")]
    public string GetMemoryLogs(
        [Description("Start log line number (1-indexed). Negative numbers count from end (-1 = last line). Default: 1")] int startLine = 1,
        [Description("End log line number. 0 or -1 = end of logs. Negative numbers count from end. Default: 0")] int endLine = 0,
        
        [Description("Only show lines containing this regex pattern (if useRegex=true) or literal text (if useRegex=false). Applied after removeAllLines filter.")] string lineContains = "",
        [Description("Remove lines containing this regex pattern (if useRegex=true) or literal text (if useRegex=false). Applied first, before other filters. Default filters out GetMemoryLogs calls.")] string removeAllLines = "GetMemoryLogs",
        
        [Description("Number of lines to show before and after lineContains matches.")] int linesBeforeAndAfter = 0,
        [Description("Include line numbers in output.")] bool lineNumbers = true,
        
        [Description("Use regex for lineContains and removeAllLines patterns. If false, uses literal text matching.")] bool useRegex = true,
        
        [Description("Maximum number of characters to display per line.")] int maxCharsPerLine = 500,
        [Description("Maximum total number of characters to display.")] int maxTotalChars = 100000)
    {
        // Check if file logging is enabled and get the location
        var fileLogPath = FileLogger.Instance.GetCurrentLogFilePath();
        var fileLoggingInfo = "";
        if (!string.IsNullOrEmpty(fileLogPath))
        {
            fileLoggingInfo = $"\n\n💾 Full logs are being saved to: {fileLogPath}\n   (These logs persist across rebuilds - use ViewFile to access them)";
        }
        
        // Get all memory logs thread-safely
        var allLines = MemoryLogger.Instance.GetAllLogs()
            .Select(line => line.TrimEnd('\r', '\n'))
            .ToArray();
        var logLineCount = allLines.Length;
        
        if (logLineCount == 0)
            return $"No logs available in memory.{fileLoggingInfo}";
        
        // Enhanced line number handling with negative indexing
        if (endLine == 0) endLine = -1;
        if (startLine < 0) startLine = Math.Max(1, logLineCount + startLine + 1);
        if (endLine < 0) endLine = Math.Max(1, logLineCount + endLine + 1);
        
        // Validate and clamp line numbers
        if (startLine <= 0) startLine = 1;
        startLine = Math.Min(startLine, logLineCount);
        endLine = Math.Min(endLine, logLineCount);
        
        if (startLine > logLineCount) 
            return $"Invalid range: start line {startLine} exceeds log line count of {logLineCount}";
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
                var removePattern = useRegex ? removeAllLines : Regex.Escape(removeAllLines);
                removeAllLinesRegex = new Regex(removePattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
            }
            catch (Exception ex)
            {
                return $"Invalid {(useRegex ? "regular expression" : "text")} pattern for removeAllLines: {removeAllLines} - {ex.Message}";
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
            
            currentLineNumbers = lineNumberMapping.ToArray();
        }
        else
        {
            // No removal filtering, just use sequential line numbers
            currentLineNumbers = Enumerable.Range(startLine, rangeLines.Length).ToArray();
        }
        
        // STEP 2: Apply lineContains filtering (only keep matching lines)
        if (string.IsNullOrEmpty(lineContains))
        {
            // No filtering - return all lines in range (after removeAllLines was applied)
            return FormatAndTruncateLines(rangeLines, lineNumbers, currentLineNumbers, false, null, 
                maxCharsPerLine, maxTotalChars, logLineCount);
        }
        
        Regex lineContainsRegex;
        try
        {
            var linePattern = useRegex ? lineContains : Regex.Escape(lineContains);
            lineContainsRegex = new Regex(linePattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }
        catch (Exception ex)
        {
            return $"Invalid {(useRegex ? "regular expression" : "text")} pattern for lineContains: {lineContains} - {ex.Message}";
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
            expandedLineIndices.Select(i => matchedIndicesSet.Contains(i)).ToArray(), maxCharsPerLine, maxTotalChars, logLineCount, matchedLineIndices.Count, expandedLineIndices.Count);
    }
    
    /// <summary>
    /// Helper method to format lines with optional line numbers and highlighting, then apply truncation
    /// </summary>
    private static string FormatAndTruncateLines(string[] lines, bool lineNumbers, int[] lineNumbersArray, 
        bool shouldHighlight, bool[]? isMatchingLine, int maxCharsPerLine, int maxTotalChars, int logLineCount, int totalMatches = 0, int totalIncluded = 0)
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
        
        if (totalMatches > 0)
        {
            sb.Append($"[Matched {totalMatches} line{(totalMatches != 1 ? "s" : "")}");
            if (totalIncluded > totalMatches)
            {
                sb.Append($", showing {totalIncluded} lines with context");
            }
            sb.Append(", ");
        }
        else
        {
            sb.Append("[Showing ");
        }
        
        sb.Append($"lines {firstLine}-{lastLine}");
        
        if (wasTruncated)
        {
            var linesNotShown = lines.Length - linesShown;
            sb.Append($" (truncated: {linesNotShown} more matched lines not shown)");
        }
        
        sb.Append($" of {logLineCount} total]");
        
        if (lastLine >= logLineCount)
        {
            sb.Append(" [End of logs]");
        }
        else
        {
            var remaining = logLineCount - lastLine;
            sb.Append($" [{remaining} lines remaining]");
        }
        
        if (anyLineTruncated)
        {
            sb.AppendLine();
            sb.Append($"[Note: Some lines were truncated to {maxCharsPerLine} chars]");
        }
        
        // Add file logging info at the end
        var fileLogPath = FileLogger.Instance.GetCurrentLogFilePath();
        if (!string.IsNullOrEmpty(fileLogPath))
        {
            sb.AppendLine();
            sb.AppendLine();
            sb.Append($"💾 Full logs saved to: {fileLogPath}");
            sb.AppendLine();
            sb.Append("   (Persists across rebuilds - use ViewFile to access)");
        }
        
        return sb.ToString();
    }
}
