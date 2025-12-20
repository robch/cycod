using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

public class LineHelpers
{
    public static bool IsLineMatch(string line, List<Regex> includeLineContainsPatternList, List<Regex> removeAllLineContainsPatternList)
    {
        var includeMatch = includeLineContainsPatternList.All(regex => regex.IsMatch(line));
        var excludeMatch = removeAllLineContainsPatternList.Count > 0 && removeAllLineContainsPatternList.Any(regex => regex.IsMatch(line));
        
        // Log detailed information at verbose level
        if (ConsoleHelpers.IsVerbose())
        {
            if (!includeMatch && includeLineContainsPatternList.Count > 0)
            {
                var failedPatterns = includeLineContainsPatternList
                    .Where(regex => !regex.IsMatch(line))
                    .Select(regex => regex.ToString())
                    .ToList();
                
                Logger.Verbose($"Line excluded because it doesn't match include patterns: [{string.Join(", ", failedPatterns)}]");
                Logger.Verbose($"Line: {line.Substring(0, Math.Min(50, line.Length))}...");
            }
            
            if (excludeMatch)
            {
                var matchedPatterns = removeAllLineContainsPatternList
                    .Where(regex => regex.IsMatch(line))
                    .Select(regex => regex.ToString())
                    .ToList();
                    
                Logger.Verbose($"Line excluded because it matches exclude patterns: [{string.Join(", ", matchedPatterns)}]");
                Logger.Verbose($"Line: {line.Substring(0, Math.Min(50, line.Length))}...");
            }
        }

        return includeMatch && !excludeMatch;
    }

    public static string AddLineNumbers(string content)
    {
        var lines = content.Split('\n');
        return string.Join('\n', lines.Select((line, index) => $"{index + 1}: {line}"));
    }

    public static string? FilterAndExpandContext(
        string content, 
        List<Regex> includeLineContainsPatternList, 
        int includeLineCountBefore, 
        int includeLineCountAfter, 
        bool includeLineNumbers, 
        List<Regex> removeAllLineContainsPatternList, 
        string backticks, 
        bool highlightMatches = false)
    {
        // Find the matching lines/indices (line numbers are 1-based, indices are 0-based)
        var allLines = content.Split('\n');
        var matchedLineIndices = allLines.Select((line, index) => new { line, index })
            .Where(x => IsLineMatch(x.line, includeLineContainsPatternList, removeAllLineContainsPatternList))
            .Select(x => x.index)
            .ToList();
        if (matchedLineIndices.Count == 0) return null;

        // Expand the range of lines, based on before and after counts
        var linesToInclude = new HashSet<int>(matchedLineIndices);
        foreach (var index in matchedLineIndices)
        {
            for (int b = 1; b <= includeLineCountBefore; b++)
            {
                var idxBefore = index - b;
                if (idxBefore >= 0)
                {
                    // Only add context lines that wouldn't be removed
                    var contextLine = allLines[idxBefore];
                    var shouldRemoveContextLine = removeAllLineContainsPatternList.Any(regex => regex.IsMatch(contextLine));
                    if (!shouldRemoveContextLine)
                    {
                        linesToInclude.Add(idxBefore);
                    }
                }
            }

            for (int a = 1; a <= includeLineCountAfter; a++)
            {
                var idxAfter = index + a;
                if (idxAfter < allLines.Length)
                {
                    // Only add context lines that wouldn't be removed  
                    var contextLine = allLines[idxAfter];
                    var shouldRemoveContextLine = removeAllLineContainsPatternList.Any(regex => regex.IsMatch(contextLine));
                    if (!shouldRemoveContextLine)
                    {
                        linesToInclude.Add(idxAfter);
                    }
                }
            }
        }
        var expandedLineIndices = linesToInclude.OrderBy(i => i).ToList();

        var checkForLineNumberBreak = (includeLineCountBefore + includeLineCountAfter) > 0;
        int? previousLineIndex = null;

        // Loop through the lines to include and accumulate the output
        var output = new List<string>();
        foreach (var index in expandedLineIndices)
        {
            var addSeparatorForLineNumberBreak = checkForLineNumberBreak && previousLineIndex != null && index > previousLineIndex + 1;
            if (addSeparatorForLineNumberBreak)
            {
                output.Add($"{backticks}\n\n{backticks}");
            }

            var line = allLines[index];
            var isMatchingLine = matchedLineIndices.Contains(index); // Track if this line was an actual match

            if (includeLineNumbers)
            {
                var lineNumber = index + 1;
                // Add * prefix for matching lines when highlighting is enabled
                var prefix = highlightMatches && isMatchingLine ? "*" : " ";
                
                output.Add($"{prefix} {lineNumber}: {line}");
            }
            else
            {
                // Add * prefix for matching lines when highlighting is enabled (no line numbers)
                var prefix = highlightMatches && isMatchingLine ? "* " : "";
                output.Add($"{prefix}{line}");
            }

            previousLineIndex = index;
        }

        return string.Join("\n", output);
    }
}
