class TestRunCommand : TestBaseCommand
{
    public TestRunCommand() : base()
    {
        OutputFormat = "trx"; // Default format
    }

    public string? OutputFile { get; set; }
    public string OutputFormat { get; set; }

    public override bool IsEmpty()
    {
        return false;
    }

    public override string GetCommandName()
    {
        return "run";
    }

    public override async Task<object> ExecuteAsync(bool interactive)
    {
        return await Task.Run(() => ExecuteTestRun());
    }

    private int ExecuteTestRun()
    {
        try
        {
            Logger.Log(new CycoDtTestFrameworkLogger());

            var tests = FindAndFilterTests();
            Console.WriteLine(tests.Count() == 1
                ? $"Found {tests.Count()} test...\n"
                : $"Found {tests.Count()} tests...\n");

            var consoleHost = new YamlTestFrameworkConsoleHost();
            var resultsByTestCaseId = YamlTestFramework.RunTests(tests, consoleHost);

            GetOutputFileAndFormat(out var file, out var format);
            consoleHost.Finish(resultsByTestCaseId, format, file);

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
    
    private void GetOutputFileAndFormat(out string file, out string format)
    {
        format = OutputFormat;
        var ext = format switch
        {
            "trx" => "trx",
            "junit" => "xml",
            _ => throw new Exception($"Unknown format: {format}")
        };

        file = OutputFile ?? $"test-results.{ext}";
        if (!file.EndsWith($".{ext}"))
        {
            file += $".{ext}";
        }
    }
}