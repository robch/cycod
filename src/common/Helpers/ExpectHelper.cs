using System.Text;
using System.Text.RegularExpressions;

public class ExpectHelper
{
    public static bool CheckOutput(string output, string? expected, string? unexpected, out string? failedReason)
    {
        var outputLines = output
            .Split(new char[] { '\n' }, StringSplitOptions.None)
            .Select(line => line.TrimEnd('\r'))
            .ToList() ?? new List<string>();
        var expectedLines = expected?.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.TrimEnd('\r'))
            .ToList() ?? new List<string>();
        var unexpectedLines = unexpected?.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(line => line.TrimEnd('\r'))
            .ToList() ?? new List<string>();

        return CheckLines(outputLines, expectedLines, unexpectedLines, out failedReason);
    }

    public static bool CheckLines(IEnumerable<string> lines, IEnumerable<string> expected, IEnumerable<string> unexpected, out string? details)
    {
        details = null;

        var helper = new ExpectHelper(lines, expected, unexpected);
        var result = helper.Expect();

        if (!result)
        {
            details = helper._details.ToString().TrimEnd('\r', '\n');
        }

        return result;
    }

    private ExpectHelper(IEnumerable<string> lines, IEnumerable<string> expected, IEnumerable<string> unexpected)
    {
        _allLines = lines;
        _expected = expected != null ? new Queue<string>(expected) : null;
        _unexpected = unexpected != null ? new List<string>(unexpected) : null;
    }

    private bool Expect()
    {
        foreach (string line in _allLines)
        {
            if (_expected != null) CheckExpected(line);
            if (_unexpected != null) CheckUnexpected(line);
        }

        var allExpectedFound = _expected == null || _expected.Count == 0;
        if (!allExpectedFound)
        {
            var codeBlock = MarkdownHelpers.GetCodeBlock(_unmatchedInput.ToString());
            var message = $"UNEXPECTED: Couldn't find '{_expected!.Peek()}' in:\n{codeBlock}";
            _details.AppendLine(message);
        }

        return !_foundUnexpected && allExpectedFound;
    }

    private void CheckExpected(string line)
    {
        ConsoleHelpers.WriteDebugHexDump(line, $"CheckExpected: Adding '{line}'");
        _unmatchedInput.AppendLine(line);
        ConsoleHelpers.WriteDebugHexDump(_unmatchedInput.ToString(), "CheckExpected: Unmatched is now:");
        while (_expected!.Count > 0)
        {
            var pattern = _expected.Peek();
            var check = _unmatchedInput.ToString();

            var match = Regex.Match(check, pattern);
            if (!match.Success)
            {
                ConsoleHelpers.WriteDebugLine($"CheckExpected: No match for '{pattern}' in unmatched!\nCheckExpected: ---"); 
                break; // continue reading input...
            }

            ConsoleHelpers.WriteDebugHexDump(check, $"CheckExpected: Matched '{pattern}' at {match.Index:x4} ({match.Length:x4} char(s)) in:");
            _unmatchedInput.Remove(0, match.Index + match.Length);
            ConsoleHelpers.WriteDebugHexDump(_unmatchedInput.ToString(), "CheckExpected: After removing, unmatched is now:");

            _expected.Dequeue();
        }
    }

    private void CheckUnexpected(string line)
    {
        foreach (var pattern in _unexpected!)
        {
            var match = Regex.Match(line, pattern);
            if (!match.Success) continue; // check more patterns

            _foundUnexpected = true;

            var message = $"UNEXPECTED: Found '{pattern}' in '{line}'";
            _details.AppendLine(message);
        }
    }

    StringBuilder _details = new StringBuilder();
    private StringBuilder _unmatchedInput = new StringBuilder();
    private IEnumerable<string> _allLines;

    private Queue<string>? _expected;
    private List<string>? _unexpected;
    bool _foundUnexpected = false;
}
