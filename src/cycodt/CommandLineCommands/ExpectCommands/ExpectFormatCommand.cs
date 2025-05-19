using System.Text;
using System.Text.RegularExpressions;

class ExpectFormatCommand : ExpectBaseCommand
{
    public ExpectFormatCommand() : base()
    {
        Strict = true; // Default to true
    }

    public bool Strict { get; set; }

    public override string GetCommandName()
    {
        return "expect format";
    }

    public override async Task<object> ExecuteAsync(bool interactive)
    {
        return await Task.Run(() => ExecuteFormat());
    }

    private int ExecuteFormat()
    {
        try
        {
            var input = FileHelpers.ReadAllText(Input!);
            var formattedText = FormatInput(input);
            WriteOutput(formattedText);
            return 0;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"ERROR: {ex.Message}\n{ex.StackTrace}");
            Console.ResetColor();
            return 1;
        }
    }

    private string FormatInput(string input)
    {
        var c = input.Count(c => c == '\n');
        ConsoleHelpers.WriteDebugHexDump(input, $"ExpectFormatCommand.FormatInput: Input contains {c} lines.");

        var lines = input.Split('\n', StringSplitOptions.None);
        var formattedLines = new List<string>();

        foreach (var line in lines)
        {
            var formatted = FormatLine(line, Strict);
            formattedLines.Add(formatted);
        }

        return string.Join("\n", formattedLines);
    }

    private static string EscapeSpecialRegExChars(string line)
    {
        return Regex.Replace(line, @"([\\()\[\]{}.*+?|^$])", @"\$1");
    }

    private string FormatLine(string line, bool strict)
    {
        ConsoleHelpers.WriteDebugHexDump(line, "ExpectFormatCommand.FormatLine:");

        var escaped = EscapeSpecialRegExChars(line);
        ConsoleHelpers.WriteDebugHexDump(escaped, "ExpectFormatCommand.FormatLine; post-escape:");

        var escapedCR = strict
            ? escaped.Trim('\r').Replace("\r", "\\r")
            : escaped.Replace("\r", "\\r");
        ConsoleHelpers.WriteDebugHexDump(escapedCR, "ExpectFormatCommand.FormatLine; post-cr-escape:");

        var result = strict ? $"^{escapedCR}\\r?$\\n" : escapedCR;
        ConsoleHelpers.WriteDebugHexDump(result, "ExpectFormatCommand.FormatLine; result:");

        return result;
    }
}