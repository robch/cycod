using System.Text.RegularExpressions;

public class StringHelpers
{
    public static string? ReplaceOnce(string fileContent, string oldStr, string newStr, out int countFound)
    {
        var check = ExactlyReplaceOnce(fileContent, oldStr, newStr, out countFound);
        if (check != null && countFound == 1) return check;

        return FuzzyReplaceOnce(fileContent, oldStr, newStr, out countFound);
    }

    public static string? ExactlyReplaceOnce(string fileContent, string oldStr, string newStr, out int countFound)
    {
        countFound = 0;

        var first = fileContent.IndexOf(oldStr, StringComparison.Ordinal);
        if (first == -1) return null;
        
        var second = fileContent.IndexOf(oldStr, first + 1, StringComparison.Ordinal);
        if (second != -1)
        {
            countFound = 2;
            return null;
        }

        countFound = 1;
        var newFileContent = fileContent.Substring(0, first) + newStr + fileContent.Substring(first + oldStr.Length);
        return newFileContent;
    }

    public static string? FuzzyReplaceOnce(string fileContent, string oldStr, string newStr, out int countFound)
    {
        countFound = 0;
        
        var escapedLines = oldStr
            .Split('\n', StringSplitOptions.None)
            .Select(line => line.TrimEnd('\r').TrimEnd())
            .Select(line => Regex.Escape(line))
            .ToArray();

        var linesWithTrailingWsPattern = escapedLines
            .Take(escapedLines.Length - 1)
            .Select(line => $"{line}[{WhitespaceChars}]*$")
            .Concat(new[] { escapedLines.Last() });

        var joined = string.Join('\n', linesWithTrailingWsPattern);
        var pattern = new Regex(joined, RegexOptions.Multiline | RegexOptions.CultureInvariant);

        var matches = pattern.Matches(fileContent);
        countFound = matches.Count;

        var foundOnlyOnce = countFound == 1;
        if (!foundOnlyOnce) return null;

        var match = matches[0];
        var start = match.Index;
        var end = match.Index + match.Length;

        return fileContent.Substring(0, start) + newStr + fileContent.Substring(end);
    }

    private const string WhitespaceChars = "\\t\\f\\v\\r\\u00A0\\u2000-\\u200A\\u2028\\u2029\\u3000 ";
}