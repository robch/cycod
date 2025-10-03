using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;

class ExpectCheckCommand : ExpectBaseCommand
{
    public ExpectCheckCommand() : base()
    {
        RegexPatterns = new List<string>();
        NotRegexPatterns = new List<string>();
    }

    public List<string> RegexPatterns { get; set; }
    public List<string> NotRegexPatterns { get; set; }
    public string? Instructions { get; set; }

    public override string GetCommandName()
    {
        return "expect check";
    }

    public override async Task<object> ExecuteAsync(bool interactive)
    {
        return await Task.Run(() => ExecuteCheck());
    }

    private int ExecuteCheck()
    {
        try
        {
            var message = "Checking expectations...";
            ConsoleHelpers.Write($"{message}");

            var lines = FileHelpers.ReadAllLines(Input!);
            var text = string.Join("\n", lines);

            var linesOk = ExpectHelper.CheckLines(lines, RegexPatterns, NotRegexPatterns, out var expectFailedReason);
            if (!linesOk)
            {
                ConsoleHelpers.WriteLine($"\r{message} FAILED!\n\n{expectFailedReason}");
                return 1;
            }

            var instructionsOk = CheckExpectInstructionsHelper.CheckExpectations(text, Instructions, null, out _, out _, out var instructionsFailedReason);
            if (!instructionsOk)
            {
                ConsoleHelpers.WriteLine($"\r{message} FAILED!\n\n{instructionsFailedReason}");
                return 1;
            }

            ConsoleHelpers.WriteLine($"\r{message} PASS!");
            return 0;
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteErrorLine($"ERROR: {ex.Message}\n{ex.StackTrace}");
            return 1;
        }
    }
}
