using System;
using System.Linq;
using System.Threading.Tasks;
using CycoDj.CommandLine;

class Program
{
    static async Task<int> Main(string[] args)
    {
        CycoDjProgramInfo _programInfo = new();

        LoggingInitializer.InitializeMemoryLogger();
        LoggingInitializer.LogStartupDetails(args);
        Logger.Info($"Starting {ProgramInfo.Name}, version {VersionInfo.GetVersion()}");

        if (!CycoDjCommandLineOptions.Parse(args, out var commandLineOptions, out var ex))
        {
            DisplayBanner();
            if (ex != null)
            {
                Logger.Error($"Command line error: {ex.Message}");
                DisplayException(ex);
                HelpHelpers.DisplayUsage(ex.GetHelpTopic());
                return 2;
            }
            else
            {
                Logger.Warning("Displaying help due to command line parsing issue");
                HelpHelpers.DisplayUsage(commandLineOptions!.HelpTopic);
                return 1;
            }
        }

        var debug = ConsoleHelpers.IsDebug() || commandLineOptions!.Debug;
        var verbose = ConsoleHelpers.IsVerbose() || commandLineOptions!.Verbose;
        var quiet = ConsoleHelpers.IsQuiet() || commandLineOptions!.Quiet;
        ConsoleHelpers.Configure(debug, verbose, quiet);
        
        LoggingInitializer.InitializeLogging(commandLineOptions?.LogFile, debug);

        var helpCommand = commandLineOptions!.Commands.OfType<HelpCommand>().FirstOrDefault();
        if (helpCommand != null)
        {
            DisplayBanner();
            HelpHelpers.DisplayHelpTopic(commandLineOptions.HelpTopic, commandLineOptions.ExpandHelpTopics);
            return 0;
        }

        var versionCommand = commandLineOptions!.Commands.OfType<VersionCommand>().FirstOrDefault();
        if (versionCommand != null)
        {
            DisplayBanner();
            var version = await versionCommand.ExecuteAsync(false);
            ConsoleHelpers.WriteLine(version.ToString()!);
            return 0;
        }

        foreach (var command in commandLineOptions.Commands)
        {
            if (command is CycoDj.CommandLineCommands.ListCommand listCommand)
            {
                return await listCommand.ExecuteAsync();
            }
        }

        return 0;
    }

    private static void DisplayBanner()
    {
        ConsoleHelpers.WriteLine($"{ProgramInfo.Name} {VersionInfo.GetVersion()}", ConsoleColor.White);
    }

    private static void DisplayException(Exception ex)
    {
        ConsoleHelpers.WriteErrorLine(ex.Message);
        if (ConsoleHelpers.IsDebug())
        {
            ConsoleHelpers.WriteErrorLine(ex.StackTrace ?? string.Empty);
        }
    }
}

