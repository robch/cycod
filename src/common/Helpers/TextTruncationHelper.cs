using System;
using System.Linq;

/// <summary>
/// Helper class for truncating text output to control display size.
/// </summary>
public static class TextTruncationHelper
{
    /// <summary>
    /// Truncates text output according to line and total character limits.
    /// </summary>
    /// <param name="output">The text to truncate</param>
    /// <param name="maxCharsPerLine">Maximum characters per line</param>
    /// <param name="maxTotalChars">Maximum total characters</param>
    /// <returns>The truncated output with appropriate indicators</returns>
    public static string TruncateOutput(string output, int maxCharsPerLine, int maxTotalChars)
    {
        if (string.IsNullOrEmpty(output))
            return output;
            
        var outputLines = output.Split('\n', StringSplitOptions.None)
            .Select(line => line.TrimEnd('\r'))
            .ToArray();
            
        var needLineTruncs = outputLines.Any(line => line.Length > maxCharsPerLine);
        var linesAfterTrunc = outputLines.Select(line => 
            needLineTruncs && line.Length > maxCharsPerLine
                ? line.Substring(0, maxCharsPerLine - 1) + "…" 
                : line)
            .ToArray();
        
        var formatted = string.Join("\n", linesAfterTrunc);
                
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
}