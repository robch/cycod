using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Builder for configuring and executing one-off shell commands.
/// </summary>
public class ShellCommandBuilder
{
    /// <summary>
    /// Creates a new ShellCommandBuilder.
    /// </summary>
    public ShellCommandBuilder()
    {
    }
    
    /// <summary>
    /// Sets the shell type to use.
    /// </summary>
    /// <param name="shellType">The type of shell.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public ShellCommandBuilder WithShellType(ShellType shellType)
    {
        _shellType = shellType;
        return this;
    }
    
    /// <summary>
    /// Sets the command to execute.
    /// </summary>
    /// <param name="command">The command string.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public ShellCommandBuilder WithCommand(string command)
    {
        _command = command;
        return this;
    }
    
    /// <summary>
    /// Sets the working directory for the command.
    /// </summary>
    /// <param name="workingDirectory">The working directory.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public ShellCommandBuilder WithWorkingDirectory(string? workingDirectory)
    {
        var ok = !string.IsNullOrEmpty(workingDirectory);
        if (ok) _workingDirectory = workingDirectory;
        return this;
    }
    
    /// <summary>
    /// Sets an environment variable for the shell.
    /// </summary>
    /// <param name="name">The name of the environment variable.</param>
    /// <param name="value">The value of the environment variable.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public ShellCommandBuilder WithEnvironmentVariable(string name, string value)
    {
        _environmentVariables[name] = value;
        return this;
    }
    
    /// <summary>
    /// Sets environment variables for the shell.
    /// </summary>
    /// <param name="variables">Dictionary of environment variables.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public ShellCommandBuilder WithEnvironmentVariables(Dictionary<string, string>? variables)
    {
        variables ??= new Dictionary<string, string>();
        foreach (var kvp in variables)
        {
            _environmentVariables[kvp.Key] = kvp.Value;
        }
        return this;
    }
    
    /// <summary>
    /// Sets a timeout for the command.
    /// </summary>
    /// <param name="timeoutMs">Timeout in milliseconds.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public ShellCommandBuilder WithTimeout(int? timeoutMs)
    {
        var ok = timeoutMs.HasValue;
        if (ok) _timeoutMs = timeoutMs!.Value;
        return this;
    }
    
    /// <summary>
    /// Sets a cancellation token for the command.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public ShellCommandBuilder WithCancellationToken(CancellationToken cancellationToken)
    {
        _cancellationToken = cancellationToken;
        return this;
    }
    
    /// <summary>
    /// Sets standard input text for the command.
    /// </summary>
    /// <param name="input">The input text.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public ShellCommandBuilder WithStandardInput(string? input)
    {
        _standardInput = input;
        return this;
    }
    
    /// <summary>
    /// Enables or disables verbose logging.
    /// </summary>
    /// <param name="verbose">Whether to enable verbose logging.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public ShellCommandBuilder WithVerboseLogging(bool verbose = true)
    {
        _verboseLogging = verbose;
        return this;
    }
    
    /// <summary>
    /// Sets a callback for standard output data.
    /// </summary>
    /// <param name="callback">The callback to invoke for each line of output.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public ShellCommandBuilder OnOutput(Action<string> callback)
    {
        _stdoutCallback = callback;
        return this;
    }
    
    /// <summary>
    /// Sets a callback for standard error data.
    /// </summary>
    /// <param name="callback">The callback to invoke for each line of error output.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public ShellCommandBuilder OnError(Action<string> callback)
    {
        _stderrCallback = callback;
        return this;
    }
    
    /// <summary>
    /// Sets a callback for all output data (both stdout and stderr).
    /// </summary>
    /// <param name="callback">The callback to invoke for each line of output.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public ShellCommandBuilder OnMergedOutput(Action<string> callback)
    {
        _mergedCallback = callback;
        return this;
    }
    
    /// <summary>
    /// Sets a callback for process events.
    /// </summary>
    /// <param name="callback">The callback to invoke for process events.</param>
    /// <returns>This builder instance for method chaining.</returns>
    public ShellCommandBuilder OnEvent(Action<ProcessEvent, string> callback)
    {
        _eventCallback = callback;
        return this;
    }
    
    /// <summary>
    /// Runs the command and returns the result.
    /// </summary>
    /// <returns>The result of the command execution.</returns>
    public ShellCommandResult Run()
    {
        return RunAsync().GetAwaiter().GetResult();
    }
    
    /// <summary>
    /// Runs the command asynchronously and returns the result.
    /// </summary>
    /// <returns>A task that completes with the command result.</returns>
    public async Task<ShellCommandResult> RunAsync()
    {
        if (string.IsNullOrEmpty(_command))
        {
            throw new InvalidOperationException("Command must be specified");
        }
        
        var shellBuilder = new RunnableShellProcessBuilder()
            .WithShellType(_shellType)
            .WithTimeout(_timeoutMs)
            .WithCancellationToken(_cancellationToken)
            .WithVerboseLogging(_verboseLogging);
            
        if (_environmentVariables.Count > 0)
        {
            shellBuilder.WithEnvironmentVariables(_environmentVariables);
        }
        
        shellBuilder.WithWorkingDirectory(_workingDirectory);
        
        if (_stdoutCallback != null)
        {
            shellBuilder.OnOutput(_stdoutCallback);
        }
        
        if (_stderrCallback != null)
        {
            shellBuilder.OnError(_stderrCallback);
        }
        
        if (_mergedCallback != null)
        {
            shellBuilder.OnMergedOutput(_mergedCallback);
        }
        
        if (_eventCallback != null)
        {
            shellBuilder.OnEvent(_eventCallback);
        }
        
        var shellProcess = shellBuilder.Build();
        try
        {
            return _standardInput != null
                ? await shellProcess.RunCommandAsync(_command, _standardInput)
                : await shellProcess.RunCommandAsync(_command, _cancellationToken);
        }
        finally
        {
            shellProcess.ForceShutdown();
        }
    }

    private ShellType _shellType = ProcessUtils.GetDefaultShellType();
    private string? _command;
    private string? _workingDirectory;
    private readonly Dictionary<string, string> _environmentVariables = new Dictionary<string, string>();
    private string? _standardInput;
    private int _timeoutMs = 30000; // Default 30 seconds
    private CancellationToken _cancellationToken = CancellationToken.None;
    private bool _verboseLogging = false;
    
    private Action<string>? _stdoutCallback;
    private Action<string>? _stderrCallback;
    private Action<string>? _mergedCallback;
    private Action<ProcessEvent, string>? _eventCallback;
}