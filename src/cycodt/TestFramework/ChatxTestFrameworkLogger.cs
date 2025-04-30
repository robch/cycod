public class CycoDtTestFrameworkLogger : IYamlTestFrameworkLogger
{
    public void LogVerbose(string text)
    {
        // TODO: When cycod gets proper logging, replace this with actual logging
        if (IsDebugEnabled())
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"VERBOSE: {text}");
            Console.ResetColor();
        }
    }

    public void LogInfo(string text)
    {
        // TODO: When cycod gets proper logging, replace this with actual logging
        if (IsDebugEnabled())
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine($"INFO: {text}");
            Console.ResetColor();
        }
    }

    public void LogWarning(string text)
    {
        // TODO: When cycod gets proper logging, replace this with actual logging
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"WARNING: {text}");
        Console.ResetColor();
    }

    public void LogError(string text)
    {
        // TODO: When cycod gets proper logging, replace this with actual logging
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"ERROR: {text}");
        Console.ResetColor();
    }

    private bool IsDebugEnabled()
    {
        return ConsoleHelpers.IsDebug();
    }
}
