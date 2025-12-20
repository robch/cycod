using System;
using System.Threading.Tasks;

class Program
{
    static int Main(string[] args)
    {
        CycoDjProgramInfo _programInfo = new();

        LoggingInitializer.InitializeMemoryLogger();
        LoggingInitializer.LogStartupDetails(args);
        Logger.Info($"Starting {ProgramInfo.Name}, version {VersionInfo.GetVersion()}");

        DisplayBanner();
        ConsoleHelpers.WriteLine("cycodj - Chat History Journal Tool", ConsoleColor.Cyan);
        ConsoleHelpers.WriteLine("Phase 0 infrastructure complete!", ConsoleColor.Green);
        ConsoleHelpers.WriteLine();
        ConsoleHelpers.WriteLine("TODO: Implement command-line parsing and commands");
        
        return 0;
    }

    private static void DisplayBanner()
    {
        ConsoleHelpers.WriteLine($"{ProgramInfo.Name} {VersionInfo.GetVersion()}", ConsoleColor.White);
    }
}
