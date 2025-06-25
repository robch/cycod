using System;
using System.Threading.Tasks;

public class CycodBenchProgramRunner : ProgramRunner
{
    public static async Task<int> RunAsync(string[] args)
    {
        try
        {
            var program = new CycodBenchProgramRunner();
            return await program.RunProgramAsync(args);
        }
        finally
        {
            ShellSession.ShutdownAll();
        }
    }

    override protected bool ParseCommandLine(string[] args, out CommandLineOptions? commandLineOptions, out CommandLineException? ex)
    {
        return CycodBenchCommandLineOptions.Parse(args, out commandLineOptions, out ex);
    }

    private CycodBenchProgramInfo _programInfo = new();
}