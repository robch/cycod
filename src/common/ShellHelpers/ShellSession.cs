using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public abstract class ShellSession
{
    public ShellSession()
    {
        lock (_sessions)
        {
            _sessions.Add(this);
        }
    }

    // Each derived class supplies its own shell type.
    protected abstract PersistentShellType GetShellType();

    // Makes sure the shell process is running.
    protected void EnsureProcess()
    {
        if (_shellProcess != null && !_shellProcess.HasExited)
            return;

        ConsoleHelpers.WriteDebugLine($"Starting {GetShellType()} shell...");
        var shellBuilder = new PersistentShellProcessBuilder()
            .WithShellType(GetShellType())
            .WithVerboseLogging(ConsoleHelpers.IsVerbose());

        if (ConsoleHelpers.IsVerbose())
        {
            shellBuilder.OnOutput(line => ConsoleHelpers.WriteLine(line, ConsoleColor.DarkMagenta));
            shellBuilder.OnError(line => ConsoleHelpers.WriteErrorLine(line));
        }

        _shellProcess = shellBuilder.Build();
        
        // Make sure the shell is started
        _shellProcess.EnsureStarted();
    }

    // Executes a command and waits for completion.
    public async Task<PersistentShellCommandResult> ExecuteCommandAsync(string command, int timeoutMs = 10000)
    {
        var isExit = command.Trim().ToLower() == "exit";
        if (isExit) return ResetShell(allShells: true);

        EnsureProcess();

        try
        {
            var commandBuilder = new PersistentShellCommandBuilder(_shellProcess!)
                .WithCommand(command)
                .WithTimeout(timeoutMs);

            ConsoleHelpers.WriteDebugLine($"Executing command: {command}");
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

    public void ForceShutdown()
    {
        try
        {
            _shellProcess?.ForceShutdown();
        }
        catch
        {
            // Ignore errors during cleanup
        }
        finally
        {
            _shellProcess = null;
        }
    }

    public static void ShutdownAll()
    {
        lock (_sessions)
        {
            foreach (var session in _sessions)
            {
                session.ForceShutdown();
            }
            _sessions.Clear();
        }
    }

    private static PersistentShellCommandResult ResetShell(bool allShells = false)
    {
        if (allShells)
        {
            ShutdownAll();
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

        var shutdownThisShellMessage = $"<Persistent shell has been closed... current working directory is now: {Environment.CurrentDirectory}>";
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

    protected PersistentShellProcess? _shellProcess;
    private static readonly List<ShellSession> _sessions = new List<ShellSession>();
}
