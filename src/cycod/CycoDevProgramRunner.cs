using System;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;
using ProcessExecution;

public class CycoDevProgramRunner : ProgramRunner
{
    public static async Task<int> RunAsync(string[] args)
    {
        try
        {
            var program = new CycoDevProgramRunner();
            return await program.RunProgramAsync(args);
        }
        finally
        {
            NamedShellProcessManager.Instance.ShutdownAllShells();
        }
    }

    override protected bool ParseCommandLine(string[] args, out CommandLineOptions? commandLineOptions, out CommandLineException? ex)
    {
        return CycoDevCommandLineOptions.Parse(args, out commandLineOptions, out ex);
    }

    private CycoDevProgramInfo _programInfo = new();
}
