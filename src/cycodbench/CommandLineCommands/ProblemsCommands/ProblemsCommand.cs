using System;
using System.Threading.Tasks;

/// <summary>
/// Base class for problems commands
/// </summary>
public abstract class ProblemsCommand : CycodBenchCommand
{
    /// <summary>
    /// Filter by problem ID
    /// </summary>
    public string? ProblemId { get; set; }
    
    /// <summary>
    /// Filter by repository name
    /// </summary>
    public string? Repository { get; set; }
    
    /// <summary>
    /// Filter by text in problem
    /// </summary>
    public string? ContainsPattern { get; set; }
    
    /// <summary>
    /// Maximum number of items to process
    /// </summary>
    public int MaxItems { get; set; } = 0;
    
    /// <summary>
    /// Path to save the output
    /// </summary>
    public string? OutputPath { get; set; }

    public override string GetCommandName()
    {
        return "problems";
    }
}