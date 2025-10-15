using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ProcessExecution;
using ShellExecution.Results;

public abstract class ShellSession
{
    private static readonly TimeSpan DefaultIdleTimeout = TimeSpan.FromHours(1);
    private readonly object _lockObject = new object();
    protected PersistentShellProcess? _shellProcess;
    private string _name;
    private readonly TaskCompletionSource<bool> _initializationTcs = new TaskCompletionSource<bool>();
    
    // To maintain backwards compatibility, we keep the static sessions list
    // but mark it as obsolete
    [Obsolete("Use NamedShellProcessManager instead")]
    private static readonly List<ShellSession> _sessions = new List<ShellSession>();

    /// <summary>
    /// Gets the name of the shell.
    /// </summary>
    public string Name => _name;

    /// <summary>
    /// Gets the creation time of the shell.
    /// </summary>
    public DateTime CreationTime { get; }

    /// <summary>
    /// Gets the last time the shell was used.
    /// </summary>
    public DateTime LastActivityTime { get; private set; }

    /// <summary>
    /// Gets or sets the idle timeout for the shell.
    /// </summary>
    public TimeSpan IdleTimeout { get; set; }

    /// <summary>
    /// Gets the current state of the shell.
    /// </summary>
    public ShellState State 
    { 
        get
        {
            if (_shellProcess == null)
            {
                return ShellState.Terminated;
            }
            
            if (_shellProcess.HasExited)
            {
                return ShellState.Terminated;
            }
            
            if (!_initializationTcs.Task.IsCompleted)
            {
                return ShellState.Initializing;
            }
            
            return ShellState.Idle;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ShellSession"/> class.
    /// </summary>
    /// <param name="name">The name of the shell.</param>
    public ShellSession(string name)
    {
        _name = name ?? throw new ArgumentNullException(nameof(name));
        CreationTime = DateTime.Now;
        LastActivityTime = DateTime.Now;
        IdleTimeout = DefaultIdleTimeout;
        
        // For backwards compatibility, add to static sessions list
#pragma warning disable CS0618 // Type or member is obsolete
        lock (_sessions)
        {
            _sessions.Add(this);
        }
#pragma warning restore CS0618 // Type or member is obsolete
    }

    /// <summary>
    /// Each derived class supplies its own shell type.
    /// </summary>
    /// <returns>The shell type.</returns>
    public abstract PersistentShellType GetShellType();

    /// <summary>
    /// Initializes the shell.
    /// </summary>
    public void Initialize()
    {
        try
        {
            EnsureProcess();
            _initializationTcs.SetResult(true);
        }
        catch (Exception ex)
        {
            _initializationTcs.SetException(ex);
            throw;
        }
    }

    /// <summary>
    /// Makes sure the shell process is running.
    /// </summary>
    protected void EnsureProcess()
    {
        lock (_lockObject)
        {
            if (_shellProcess != null && !_shellProcess.HasExited)
                return;

            ConsoleHelpers.WriteDebugLine($"Starting {GetShellType()} shell ({Name})...");
            var shellBuilder = new PersistentShellProcessBuilder()
                .WithShellType(GetShellType())
                .WithVerboseLogging(ConsoleHelpers.IsVerbose());

            if (ConsoleHelpers.IsVerbose())
            {
                shellBuilder.OnOutput(line => ConsoleHelpers.WriteLine($"[{Name}] {line}", ConsoleColor.DarkMagenta));
                shellBuilder.OnError(line => ConsoleHelpers.WriteErrorLine($"[{Name}] {line}"));
            }

            _shellProcess = shellBuilder.Build();
            
            // Make sure the shell is started
            _shellProcess.EnsureStarted();
        }
    }

    /// <summary>
    /// Executes a command and waits for completion.
    /// </summary>
    /// <param name="command">The command to execute.</param>
    /// <param name="timeoutMs">Timeout in milliseconds.</param>
    /// <returns>The result of the command execution.</returns>
    public async Task<PersistentShellCommandResult> ExecuteCommandAsync(string command, int timeoutMs = 10000)
    {
        if (string.IsNullOrEmpty(command))
        {
            return PersistentShellCommandResult.FromProcessResult(
                new RunnableProcessResult(
                    "",
                    "Command was empty",
                    "Command was empty",
                    -1,
                    ProcessCompletionState.Error
                ),
                command
            );
        }
        
        var isExit = command.Trim().ToLower() == "exit";
        if (isExit) return ResetShell(allShells: false);

        await _initializationTcs.Task; // Wait for initialization
        EnsureProcess();
        UpdateLastActivityTime();

        try
        {
            var commandBuilder = new PersistentShellCommandBuilder(_shellProcess!)
                .WithCommand(command)
                .WithTimeout(timeoutMs);

            ConsoleHelpers.WriteDebugLine($"Executing command in {Name}: {command}");
            return await commandBuilder.RunAsync();
        }
        catch (TimeoutException)
        {
            ForceShutdown();
            var errorMsg = $"<Command timed out after {timeoutMs}ms>";
            return PersistentShellCommandResult.FromProcessResult(
                new RunnableProcessResult(
                    "",
                    errorMsg,
                    errorMsg,
                    -1,
                    ProcessCompletionState.TimedOut,
                    TimeSpan.FromMilliseconds(timeoutMs),
                    ProcessErrorType.Timeout,
                    errorMsg
                ),
                command
            );
        }
        catch (Exception ex)
        {
            var errorMsg = $"<Error executing command: {ex.Message}>";
            return PersistentShellCommandResult.FromProcessResult(
                new RunnableProcessResult(
                    "",
                    errorMsg,
                    errorMsg,
                    -1,
                    ProcessCompletionState.Error,
                    TimeSpan.Zero,
                    ProcessErrorType.Other,
                    errorMsg,
                    ex
                ),
                command
            );
        }
    }

    /// <summary>
    /// Sends input to the shell process.
    /// </summary>
    /// <param name="input">The input to send.</param>
    /// <returns>True if input was successfully sent, false otherwise.</returns>
    public async Task<bool> SendInputAsync(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return false;
        }
        
        await _initializationTcs.Task; // Wait for initialization
        EnsureProcess();
        UpdateLastActivityTime();
        
        try
        {
            if (_shellProcess == null || _shellProcess.HasExited)
            {
                return false;
            }
            
            return _shellProcess.SendInput(input);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Waits for output matching a specified pattern.
    /// </summary>
    /// <param name="pattern">Regular expression pattern to match.</param>
    /// <param name="timeoutMs">Timeout in milliseconds, -1 for indefinite.</param>
    /// <returns>The matched output, or null if timeout or error.</returns>
    public async Task<string> WaitForOutputPatternAsync(string pattern, int timeoutMs)
    {
        if (string.IsNullOrEmpty(pattern))
        {
            return null!;
        }
        
        await _initializationTcs.Task; // Wait for initialization
        EnsureProcess();
        UpdateLastActivityTime();
        
        try
        {
            if (_shellProcess == null || _shellProcess.HasExited)
            {
                return null!;
            }
            
            Regex regex;
            try
            {
                regex = new Regex(pattern);
            }
            catch
            {
                // If regex is invalid, treat it as a literal string
                regex = new Regex(Regex.Escape(pattern));
            }
            
            var timeout = timeoutMs < 0 ? Timeout.InfiniteTimeSpan : TimeSpan.FromMilliseconds(timeoutMs);
            var endTime = DateTime.Now.Add(timeout);
            string currentOutput = string.Empty;
            
            while (DateTime.Now < endTime || timeout == Timeout.InfiniteTimeSpan)
            {
                // Get output accumulated since last check
                var result = _shellProcess.GetOutput();
                currentOutput += result.Stdout;
                
                // Check for pattern match
                var match = regex.Match(currentOutput);
                if (match.Success)
                {
                    return match.Value;
                }
                
                // Wait a bit before checking again
                await Task.Delay(100);
                
                // Update activity time occasionally
                if (DateTime.Now - LastActivityTime > TimeSpan.FromMinutes(1))
                {
                    UpdateLastActivityTime();
                }
            }
            
            // Timeout occurred
            return null!;
        }
        catch
        {
            return null!;
        }
    }

    /// <summary>
    /// Forcefully shuts down the shell process.
    /// </summary>
    public void ForceShutdown()
    {
        try
        {
            lock (_lockObject)
            {
                _shellProcess?.ForceShutdown();
                _shellProcess = null;
            }
        }
        catch
        {
            // Ignore errors during cleanup
        }
    }

    /// <summary>
    /// Gracefully shuts down the shell process.
    /// </summary>
    public void Shutdown()
    {
        try
        {
            lock (_lockObject)
            {
                if (_shellProcess != null && !_shellProcess.HasExited)
                {
                    // Try to execute exit command first
                    try
                    {
                        var exitCommand = GetShellType() switch
                        {
                            PersistentShellType.PowerShell => "exit",
                            PersistentShellType.Cmd => "exit",
                            _ => "exit" // bash
                        };
                        
                        _shellProcess.SendInput(exitCommand + Environment.NewLine);
                        
                        // Give it a short time to exit gracefully
                        var exited = SpinWait.SpinUntil(() => _shellProcess.HasExited, 2000);
                        
                        if (!exited)
                        {
                            _shellProcess.ForceShutdown();
                        }
                    }
                    catch
                    {
                        _shellProcess.ForceShutdown();
                    }
                }
                
                _shellProcess = null;
            }
        }
        catch
        {
            // Ignore errors during cleanup
        }
    }

    /// <summary>
    /// Gets the underlying process.
    /// </summary>
    /// <returns>The process, or null if not available.</returns>
    public Process GetUnderlyingProcess()
    {
        if (_shellProcess == null || _shellProcess.HasExited)
        {
            return null!;
        }
        
        try
        {
            return _shellProcess.Process;
        }
        catch
        {
            return null!;
        }
    }

    /// <summary>
    /// Updates the last activity time for the shell.
    /// </summary>
    public void UpdateLastActivityTime()
    {
        LastActivityTime = DateTime.Now;
    }


    /// <summary>
    /// Gets the current standard output captured from the shell.
    /// </summary>
    public string GetCurrentOutput()
    {
        if (_shellProcess == null || _shellProcess.HasExited)
        {
            return string.Empty;
        }
        
        var output = _shellProcess.GetCurrentOutput();
        return output ?? string.Empty;
    }

    /// <summary>
    /// Gets the current standard error captured from the shell.
    /// </summary>
    public string GetCurrentError()
    {
        if (_shellProcess == null || _shellProcess.HasExited)
        {
            return string.Empty;
        }
        
        var error = _shellProcess.GetCurrentError();
        return error ?? string.Empty;
    }

    /// <summary>
    /// Gets the current merged output (stdout + stderr) captured from the shell.
    /// </summary>
    public string GetCurrentMergedOutput()
    {
        if (_shellProcess == null || _shellProcess.HasExited)
        {
            return string.Empty;
        }
        
        var merged = _shellProcess.GetCurrentMergedOutput();
        return merged ?? string.Empty;
    }

    /// <summary>
    /// Renames the shell.
    /// </summary>
    /// <param name="newName">The new name for the shell.</param>
    public void Rename(string newName)
    {
        if (string.IsNullOrEmpty(newName))
        {
            throw new ArgumentNullException(nameof(newName));
        }
        
        _name = newName;
    }

    /// <summary>
    /// Shuts down all shells (for backward compatibility).
    /// </summary>
    [Obsolete("Use NamedShellProcessManager.ShutdownAllShells instead")]
    public static void ShutdownAll()
    {
#pragma warning disable CS0618 // Type or member is obsolete
        lock (_sessions)
        {
            foreach (var session in _sessions)
            {
                session.ForceShutdown();
            }
            _sessions.Clear();
        }
#pragma warning restore CS0618 // Type or member is obsolete
    }

    private PersistentShellCommandResult ResetShell(bool allShells = false)
    {
        if (allShells)
        {
#pragma warning disable CS0618 // Type or member is obsolete
            ShutdownAll();
#pragma warning restore CS0618 // Type or member is obsolete
            
            var shutdownAllShellsMessage = $"<All persistent shells have been closed... current working directory is now: {Environment.CurrentDirectory}>";
            return PersistentShellCommandResult.FromProcessResult(
                new RunnableProcessResult(
                    shutdownAllShellsMessage,
                    "",
                    shutdownAllShellsMessage,
                    0,
                    ProcessCompletionState.Completed
                ),
                "exit"
            );
        }

        Shutdown();
        
        var shutdownThisShellMessage = $"<Shell '{Name}' has been closed... current working directory is now: {Environment.CurrentDirectory}>";
        return PersistentShellCommandResult.FromProcessResult(
            new RunnableProcessResult(
                shutdownThisShellMessage,
                "",
                shutdownThisShellMessage,
                0,
                ProcessCompletionState.Completed
            ),
            "exit"
        );
    }
}
