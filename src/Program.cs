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
        catch (EnvVarSettingException ex)
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
            var filesSaved = AliasFileHelpers.SaveAlias(
                commandLineOptions.SaveAliasName!,
                commandLineOptions.AllOptions,
                commandLineOptions.SaveAliasScope ?? ConfigFileScope.Local);

            DisplayBanner();
            AliasDisplayHelpers.DisplaySavedAliasFiles(filesSaved);

            return 0;
        }

        var threadCountMax = commandLineOptions.ThreadCount;
        var parallelism = threadCountMax > 0 ? threadCountMax : Environment.ProcessorCount;

        var isTruelyInteractive = commandLineOptions.Interactive && !Console.IsInputRedirected;
        if (isTruelyInteractive && parallelism > 1)
        {
            ConsoleHelpers.WriteDebugLine($"Max 1 thread in truly interactive mode");
            parallelism = 1;
        }

        var allTasks = new List<Task<int>>();
        var throttler = new SemaphoreSlim(parallelism);

        var commands = ForEachVarHelpers.ExpandForEachVars(commandLineOptions.Commands).ToList();
        foreach (var command in commands)
        {
            throttler.Wait();

            var needDelayForTemplateFileNameUniqueness = !isTruelyInteractive;
            if (needDelayForTemplateFileNameUniqueness)
            {
                var delay = TimeSpan.FromMilliseconds(10);
                await Task.Delay(delay);
            }

            var startedTask = command switch
            {
                ChatCommand chatCommand => chatCommand.ExecuteAsync(commandLineOptions.Interactive),
                VersionCommand versionCommand => versionCommand.ExecuteAsync(commandLineOptions.Interactive),
                GitHubLoginCommand loginCommand => loginCommand.ExecuteAsync(commandLineOptions.Interactive),
                ConfigListCommand configListCommand => configListCommand.Execute(commandLineOptions.Interactive),
                ConfigGetCommand configGetCommand => configGetCommand.Execute(commandLineOptions.Interactive),
                ConfigSetCommand configSetCommand => configSetCommand.Execute(commandLineOptions.Interactive),
                ConfigClearCommand configClearCommand => configClearCommand.Execute(commandLineOptions.Interactive),
                ConfigAddCommand configAddCommand => configAddCommand.Execute(commandLineOptions.Interactive),
                ConfigRemoveCommand configRemoveCommand => configRemoveCommand.Execute(commandLineOptions.Interactive),
                AliasListCommand aliasListCommand => aliasListCommand.Execute(commandLineOptions.Interactive),
                AliasGetCommand aliasGetCommand => aliasGetCommand.Execute(commandLineOptions.Interactive),
                AliasDeleteCommand aliasDeleteCommand => aliasDeleteCommand.Execute(commandLineOptions.Interactive),
                PromptListCommand promptListCommand => promptListCommand.Execute(commandLineOptions.Interactive),
                PromptGetCommand promptGetCommand => promptGetCommand.Execute(commandLineOptions.Interactive),
                PromptDeleteCommand promptDeleteCommand => promptDeleteCommand.Execute(commandLineOptions.Interactive),
                PromptCreateCommand promptCreateCommand => promptCreateCommand.Execute(commandLineOptions.Interactive),
                _ => throw new NotImplementedException($"Command type {command.GetType()} not implemented.")
            };

            allTasks.Add(WrapRunAndRelease(throttler, startedTask));
        }

        await Task.WhenAll(allTasks.ToArray());
        ConsoleHelpers.DisplayStatusErase();

        return 0;
    }

    private static Task<int> WrapRunAndRelease(SemaphoreSlim throttler, Task<int> startedTask)
    {
        return Task.Run(async () =>
        {
            try
            {
                return await startedTask;
            }
            finally
            {
                throttler.Release();
            }
        });
    }

    private static void DisplayBanner()
    {
        var programNameUppercase = Program.Name.ToUpper();
        ConsoleHelpers.WriteLine(
            $"{programNameUppercase} - AI-powered CLI, Version {VersionInfo.GetVersion()}\n" +
            "Copyright(c) 2025, Rob Chambers. All rights reserved.\n");
    }

    private static void DisplayException(CommandLineException ex)
    {
        var printMessage = !string.IsNullOrEmpty(ex.Message);
        if (printMessage)
        {
            ConsoleHelpers.WriteWarning($"{ex.Message}");
            ConsoleHelpers.Write("\n\n", overrideQuiet: true);
        }
    }

    private static void DisplaySavedAliasFiles(List<string> filesSaved)
    {
        ;
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
