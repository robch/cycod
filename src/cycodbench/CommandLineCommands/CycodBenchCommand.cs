using System;
using System.Threading.Tasks;

/// <summary>
/// Base class for all CycodBench commands
/// </summary>
public abstract class CycodBenchCommand : Command
{
    public CycodBenchCommand()
    {
    }
    
    override public Task<object> ExecuteAsync(bool interactive)
    {
        throw new NotImplementedException("ExecuteAsync is not implemented for CycodBenchCommand.");
    }
}