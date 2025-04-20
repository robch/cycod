class TestListCommand : TestBaseCommand
{
    public override string GetCommandName()
    {
        return "test list";
    }

    public Task<int> Execute(bool interactive)
    {
        var result = ExecuteList();
        return Task.FromResult(result);
    }

    private int ExecuteList()
    {
        try
        {
            Logger.Log(new ChatxTestFrameworkLogger());
            var tests = FindAndFilterTests();
            
            Console.ForegroundColor = ConsoleColor.DarkGray;
            foreach (var test in tests)
            {
                Console.WriteLine(test.FullyQualifiedName);
            }
            Console.ResetColor();

            Console.WriteLine(tests.Count() == 1
                ? $"\nFound {tests.Count()} test..."
                : $"\nFound {tests.Count()} tests...");

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
}