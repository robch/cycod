namespace CycodBench.DockerManager;

/// <summary>
/// Represents the result of executing a command.
/// </summary>
public class CommandResult
{
    /// <summary>
    /// Gets or sets the exit code of the command.
    /// </summary>
    public int ExitCode { get; set; }
    
    /// <summary>
    /// Gets or sets the standard output of the command.
    /// </summary>
    public string Output { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the standard error output of the command.
    /// </summary>
    public string Error { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets whether the command timed out.
    /// </summary>
    public bool TimedOut { get; set; }
    
    /// <summary>
    /// Gets or sets the execution time in milliseconds.
    /// </summary>
    public long ExecutionTimeMs { get; set; }
}