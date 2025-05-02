class TestListCommand : TestBaseCommand
{
    public override string GetCommandName()
    {
        return "list";
    }

    public override async Task<object> ExecuteAsync(bool interactive)
    {
        return await Task.Run(() => ExecuteList());
    }

    private int ExecuteList()
    {
        try
        {
            Logger.Log(new CycoDtTestFrameworkLogger());
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