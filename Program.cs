using System.Diagnostics;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        try
        {
            SaveConsoleColor();
            return await DoProgram(ProcessDirectives(args));
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return 1;
        }
        finally
        {
            RestoreConsoleColor();
        }
    }

    private static async Task<int> DoProgram(string[] args)
    {
        if (!CommandLineOptions.Parse(args, out var commandLineOptions, out var ex))
        {
            PrintBanner();
            if (ex != null)
            {
                PrintException(ex);
                HelpHelpers.PrintUsage(ex.GetCommand());
                return 2;
            }
            else
            {
                HelpHelpers.PrintUsage(commandLineOptions!.HelpTopic);
                return 1;
            }
        }

        ConsoleHelpers.Configure(commandLineOptions!.Debug, commandLineOptions.Verbose);

        var helpCommand = commandLineOptions.Commands.OfType<HelpCommand>().FirstOrDefault();
        if (helpCommand != null)
        {
            PrintBanner();
            HelpHelpers.PrintHelpTopic(commandLineOptions.HelpTopic, commandLineOptions.ExpandHelpTopics);
            return 0;
        }

        var shouldSaveAlias = !string.IsNullOrEmpty(commandLineOptions.SaveAliasName);
        if (shouldSaveAlias)
        {
            var filesSaved = commandLineOptions.SaveAlias(commandLineOptions.SaveAliasName!);

            PrintBanner();
            PrintSavedAliasFiles(filesSaved);

            return 0;
        }

        var threadCountMax = commandLineOptions.Commands.Max(x => x.ThreadCount);
        var parallelism = threadCountMax > 0 ? threadCountMax : Environment.ProcessorCount;

        var allTasks = new List<Task<int>>();
        var throttler = new SemaphoreSlim(parallelism);

        foreach (var command in commandLineOptions.Commands)
        {
            var tasksThisCommand = command switch
            {
                ChatCommand chatCommand => await chatCommand.ExecuteAsync(),
                _ => new List<Task<int>>()
            };

            allTasks.AddRange(tasksThisCommand);
        }

        await Task.WhenAll(allTasks.ToArray());
        ConsoleHelpers.PrintStatusErase();

        return 0;
    }

    private static void PrintBanner()
    {
        var programNameUppercase = Program.Name.ToUpper();
        ConsoleHelpers.PrintLine(
            $"{programNameUppercase} - AI-powered CLI, Version 1.0.0\n" +
            "Copyright(c) 2025, Rob Chambers. All rights reserved.\n");
    }

    private static void PrintException(CommandLineException ex)
    {
        var printMessage = !string.IsNullOrEmpty(ex.Message);
        if (printMessage) ConsoleHelpers.PrintLine($"  {ex.Message}\n\n");
    }

    private static void PrintSavedAliasFiles(List<string> filesSaved)
    {
        var firstFileSaved = filesSaved.First();
        var additionalFiles = filesSaved.Skip(1).ToList();

        ConsoleHelpers.PrintLine($"Saved: {firstFileSaved}\n");

        var hasAdditionalFiles = additionalFiles.Any();
        if (hasAdditionalFiles)
        {
            foreach (var additionalFile in additionalFiles)
            {
                ConsoleHelpers.PrintLine($"  and: {additionalFile}");
            }
         
            ConsoleHelpers.PrintLine();
        }

        var aliasName = Path.GetFileNameWithoutExtension(firstFileSaved);
        ConsoleHelpers.PrintLine($"USAGE: {Program.Name} [...] --" + aliasName);
    }

    private static string[] ProcessDirectives(string[] args)
    {
        return CheckWaitForDebugger(args);
    }

    private static string[] CheckWaitForDebugger(string[] args)
    {
        var debug = args.Length >= 2 && args[0] == "debug" && args[1] == "wait";
        if (debug)
        {
            args = args.Skip(2).ToArray();
            Console.Write("Waiting for debugger...");
            for (; !Debugger.IsAttached; Thread.Sleep(200))
            {
                Console.Write('.');
            }
            Console.WriteLine();
        }

        return args;
    }

    private static void SaveConsoleColor()
    {
        _originalForegroundColor = Console.ForegroundColor;
        _originalBackgroundColor = Console.BackgroundColor;
    }
    
    private static void RestoreConsoleColor()
    {
        Console.ForegroundColor = _originalForegroundColor;
        Console.BackgroundColor = _originalBackgroundColor;
    }

    private static ConsoleColor _originalForegroundColor;
    private static ConsoleColor _originalBackgroundColor;

    public const string Name = "chatx";
}
