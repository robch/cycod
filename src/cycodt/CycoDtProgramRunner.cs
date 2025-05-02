




using System;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;

public class CycoDtProgramRunner : ProgramRunner
{
    public static async Task<int> RunAsync(string[] args)
    {
        var program = new CycoDtProgramRunner();
        return await program.RunProgramAsync(args);
    }

    override protected bool ParseCommandLine(string[] args, out CommandLineOptions? commandLineOptions, out CommandLineException? ex)
    {
        return CycoDtCommandLineOptions.Parse(args, out commandLineOptions, out ex);
    }

    private CycoDtProgramInfo _programInfo = new ();
}