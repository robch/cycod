using System;

/// <summary>
/// Builder for configuring and creating RunnableShellProcess instances.
/// </summary>
public class RunnableShellProcessBuilder
{
    /// <summary>
    /// Creates a new RunnableShellProcessBuilder.
    /// </summary>
    public RunnableShellProcessBuilder()
    {
    }
    
    /// <summary>
    /// Sets the shell type to use.
    /// </summary>
    /// <param name="shellType">The type of shell.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public RunnableShellProcessBuilder WithShellType(ShellType shellType)
    {
        _shellType = shellType;
        return this;
    }
    
    /// <summary>
    /// Sets the working directory for the shell.
    /// </summary>
    /// <param name="workingDirectory">The working directory.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public RunnableShellProcessBuilder WithWorkingDirectory(string? workingDirectory)
    {
        var ok = !string.IsNullOrEmpty(workingDirectory);
        if (ok) _processBuilder.WithWorkingDirectory(workingDirectory);
        return this;
    }
    
    /// <summary>
    /// Sets an environment variable for the shell.
    /// </summary>
    /// <param name="name">The name of the environment variable.</param>
    /// <param name="value">The value of the environment variable.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public RunnableShellProcessBuilder WithEnvironmentVariable(string name, string value)
    {
        _processBuilder.WithEnvironmentVariable(name, value);
        return this;
    }
    
    /// <summary>
    /// Sets environment variables for the shell.
    /// </summary>
    /// <param name="variables">Dictionary of environment variables.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public RunnableShellProcessBuilder WithEnvironmentVariables(Dictionary<string, string>? variables)
    {
        _processBuilder.WithEnvironmentVariables(variables);
        return this;
    }
    
    /// <summary>
    /// Sets a timeout for shell operations.
    /// </summary>
    /// <param name="timeoutMs">Timeout in milliseconds.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public RunnableShellProcessBuilder WithTimeout(int? timeoutMs)
    {
        var ok = timeoutMs.HasValue;
        if (ok) _processBuilder.WithTimeout(timeoutMs);
        return this;
    }
    
    /// <summary>
    /// Sets a cancellation token for shell operations.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public RunnableShellProcessBuilder WithCancellationToken(System.Threading.CancellationToken cancellationToken)
    {
        _processBuilder.WithCancellationToken(cancellationToken);
        return this;
    }
    
    /// <summary>
    /// Sets the timeout strategy for shell operations.
    /// </summary>
    /// <param name="strategy">The timeout strategy.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public RunnableShellProcessBuilder WithTimeoutStrategy(TimeoutStrategy strategy)
    {
        _processBuilder.WithTimeoutStrategy(strategy);
        return this;
    }
    
    /// <summary>
    /// Enables or disables verbose logging.
    /// </summary>
    /// <param name="verbose">Whether to enable verbose logging.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public RunnableShellProcessBuilder WithVerboseLogging(bool verbose = true)
    {
        _processBuilder.WithVerboseLogging(verbose);
        return this;
    }
    
    /// <summary>
    /// Sets a timeout for shell commands.
    /// </summary>
    /// <param name="timeoutMs">Timeout in milliseconds.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public RunnableShellProcessBuilder WithCommandTimeout(int timeoutMs)
    {
        _commandTimeoutMs = timeoutMs > 0 ? timeoutMs : throw new ArgumentException("Timeout must be greater than 0", nameof(timeoutMs));
        return this;
    }
    
    /// <summary>
    /// Sets a callback for standard output data.
    /// </summary>
    /// <param name="callback">The callback to invoke for each line of output.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public RunnableShellProcessBuilder OnOutput(Action<string> callback)
    {
        _processBuilder.OnOutput(callback);
        return this;
    }
    
    /// <summary>
    /// Sets a callback for standard error data.
    /// </summary>
    /// <param name="callback">The callback to invoke for each line of error output.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public RunnableShellProcessBuilder OnError(Action<string> callback)
    {
        _processBuilder.OnError(callback);
        return this;
    }
    
    /// <summary>
    /// Sets a callback for all output data (both stdout and stderr).
    /// </summary>
    /// <param name="callback">The callback to invoke for each line of output.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public RunnableShellProcessBuilder OnMergedOutput(Action<string> callback)
    {
        _processBuilder.OnMergedOutput(callback);
        return this;
    }
    
    /// <summary>
    /// Sets a callback for process events.
    /// </summary>
    /// <param name="callback">The callback to invoke for process events.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public RunnableShellProcessBuilder OnEvent(Action<ProcessEvent, string> callback)
    {
        _processBuilder.OnEvent(callback);
        return this;
    }
    
    /// <summary>
    /// Sets a callback for when the shell starts.
    /// </summary>
    /// <param name="callback">The callback to invoke when the shell starts.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public RunnableShellProcessBuilder OnStarted(Action<int> callback)
    {
        _processBuilder.OnStarted(callback);
        return this;
    }
    
    /// <summary>
    /// Sets a callback for when the shell times out.
    /// </summary>
    /// <param name="callback">The callback to invoke when the shell times out.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public RunnableShellProcessBuilder OnTimeout(Action callback)
    {
        _processBuilder.OnTimeout(callback);
        return this;
    }
    
    /// <summary>
    /// Sets a callback for when the shell exits.
    /// </summary>
    /// <param name="callback">The callback to invoke when the shell exits.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public RunnableShellProcessBuilder OnExit(Action<int> callback)
    {
        _processBuilder.OnExit(callback);
        return this;
    }
    
    /// <summary>
    /// Builds a configured RunnableShellProcess instance.
    /// </summary>
    /// <returns>A configured RunnableShellProcess.</returns>
    public RunnableShellProcess Build()
    {
        // Get the shell executable and arguments
        string shellExecutable = ProcessUtils.GetShellExecutable(_shellType);
        string shellArguments = ProcessUtils.GetShellStartupArguments(_shellType);
        
        // Configure the process builder with the shell executable and arguments
        _processBuilder.WithFileName(shellExecutable);
        _processBuilder.WithArguments(shellArguments);
        
        // Build the base process
        RunnableProcess baseProcess = _processBuilder.Build();
        
        // Create the appropriate shell process based on shell type
        RunnableShellProcess shellProcess;
        switch (_shellType)
        {
            case ShellType.Bash:
                shellProcess = new BashRunnableShellProcess(baseProcess);
                break;
                
            case ShellType.Cmd:
                shellProcess = new CmdRunnableShellProcess(baseProcess);
                break;
                
            case ShellType.PowerShell:
                shellProcess = new PowerShellRunnableShellProcess(baseProcess);
                break;
                
            default:
                throw new ArgumentException($"Unsupported shell type: {_shellType}");
        }
        
        // Configure the command timeout
        shellProcess.SetCommandTimeout(_commandTimeoutMs);
        
        return shellProcess;
    }

    private readonly RunnableProcessBuilder _processBuilder = new RunnableProcessBuilder();
    private ShellType _shellType = ProcessUtils.GetDefaultShellType();
    private int _commandTimeoutMs = 30000; // Default 30 seconds for commands
}