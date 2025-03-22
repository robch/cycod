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
            ConsoleHelpers.WriteLine($"Error: {ex.Message}");
            return 2;
        }
        catch (CalcException ex)
        {
            ExceptionHelpers.SaveAndDisplayException(ex);
            return 1;
        }
        catch (Exception ex)
        {
            ExceptionHelpers.SaveAndDisplayException(ex);
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
            DisplayBanner();
            if (ex != null)
            {
                DisplayException(ex);
                HelpHelpers.DisplayUsage(ex.GetCommand());
                return 2;
            }
            else
            {
                HelpHelpers.DisplayUsage(commandLineOptions!.HelpTopic);
                return 1;
            }
        }

        ConsoleHelpers.Configure(commandLineOptions!.Debug, commandLineOptions.Verbose, commandLineOptions.Quiet);

        var helpCommand = commandLineOptions.Commands.OfType<HelpCommand>().FirstOrDefault();
        if (helpCommand != null)
        {
            DisplayBanner();
            HelpHelpers.DisplayHelpTopic(commandLineOptions.HelpTopic, commandLineOptions.ExpandHelpTopics);
            return 0;
        }

        var shouldSaveAlias = !string.IsNullOrEmpty(commandLineOptions.SaveAliasName);
        if (shouldSaveAlias)
        {
            var filesSaved = commandLineOptions.SaveAlias(commandLineOptions.SaveAliasName!);

            DisplayBanner();
            DisplaySavedAliasFiles(filesSaved);

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
                ChatCommand chatCommand => await chatCommand.ExecuteAsync(commandLineOptions.Interactive),
                _ => new List<Task<int>>()
            };

            allTasks.AddRange(tasksThisCommand);
        }

        await Task.WhenAll(allTasks.ToArray());
        ConsoleHelpers.DisplayStatusErase();

        return 0;
    }

    private static void DisplayBanner()
    {
        var programNameUppercase = Program.Name.ToUpper();
        ConsoleHelpers.WriteLine(
            $"{programNameUppercase} - AI-powered CLI, Version 1.0.0\n" +
            "Copyright(c) 2025, Rob Chambers. All rights reserved.\n");
    }

    private static void DisplayException(CommandLineException ex)
    {
        var printMessage = !string.IsNullOrEmpty(ex.Message);
        if (printMessage) ConsoleHelpers.WriteLine($"  {ex.Message}\n\n", overrideQuiet: true);
    }

    private static void DisplaySavedAliasFiles(List<string> filesSaved)
    {
        var firstFileSaved = filesSaved.First();
        var additionalFiles = filesSaved.Skip(1).ToList();

        ConsoleHelpers.WriteLine($"Saved: {firstFileSaved}\n");

        var hasAdditionalFiles = additionalFiles.Any();
        if (hasAdditionalFiles)
        {
            foreach (var additionalFile in additionalFiles)
            {
                ConsoleHelpers.WriteLine($"  and: {additionalFile}");
            }
         
            ConsoleHelpers.WriteLine();
        }

        var aliasName = Path.GetFileNameWithoutExtension(firstFileSaved);
        ConsoleHelpers.WriteLine($"USAGE: {Program.Name} [...] --" + aliasName);
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
            ConsoleHelpers.Write("Waiting for debugger...", overrideQuiet: true);
            for (; !Debugger.IsAttached; Thread.Sleep(200))
            {
                ConsoleHelpers.Write(".");
            }
            ConsoleHelpers.WriteLine();
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
