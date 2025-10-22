using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.IO;

/// <summary>
/// Manages long-running background processes.
/// </summary>
public static class BackgroundProcessManager
{
    private static readonly ConcurrentDictionary<string, BackgroundProcessInfo> _processes = new ConcurrentDictionary<string, BackgroundProcessInfo>();
    private static readonly Timer _cleanupTimer;
    private static readonly TimeSpan _defaultCleanupInterval = TimeSpan.FromHours(1);
    private static readonly TimeSpan _defaultMaxProcessAge = TimeSpan.FromHours(8);

    /// <summary>
    /// Static constructor to initialize the cleanup timer.
    /// </summary>
    static BackgroundProcessManager()
    {
        // Create a timer to periodically clean up old processes
        _cleanupTimer = new Timer(CleanupOldProcesses, null, _defaultCleanupInterval, _defaultCleanupInterval);

        // Register for application exit to clean up all processes
        AppDomain.CurrentDomain.ProcessExit += (sender, e) => 
        {
            Logger.Info("Application exiting - shutting down all background processes");
            ShutdownAllProcesses();
        };
        
        Logger.Info("Background process manager initialized");
    }

    /// <summary>
    /// Starts a new background process.
    /// </summary>
    /// <param name="processName">The name of the executable to run.</param>
    /// <param name="processArguments">Optional arguments to pass to the process.</param>
    /// <param name="workingDirectory">Optional working directory.</param>
    /// <returns>A handle to reference the process.</returns>
    public static string StartLongRunningProcess(string processName, string? processArguments = null, string? workingDirectory = null)
    {
        if (string.IsNullOrEmpty(processName))
        {
            throw new ArgumentException("Process name cannot be null or empty", nameof(processName));
        }

        var handle = Guid.NewGuid().ToString("N");
        Logger.Info($"Starting background process: {processName} {processArguments ?? ""} (handle: {handle})");
        if (!string.IsNullOrEmpty(workingDirectory))
        {
            ConsoleHelpers.WriteDebugLine($"Background process working directory: {workingDirectory}");
        }

        // Create process using RunnableProcessBuilder
        var processBuilder = new RunnableProcessBuilder()
            .WithFileName(processName)
            .WithTimeout(int.MaxValue); // Very long timeout since this is a background process
        
        if (!string.IsNullOrEmpty(processArguments))
        {
            processBuilder.WithArguments(processArguments);
        }
        
        if (!string.IsNullOrEmpty(workingDirectory))
        {
            processBuilder.WithWorkingDirectory(workingDirectory);
        }

        var process = processBuilder.Build();

        // Create the process info and store it
        var processInfo = new BackgroundProcessInfo(handle, process);
        _processes[handle] = processInfo;

        // Start the process asynchronously
        _ = Task.Run(async () =>
        {
            try
            {
                // Start the process but don't await it - let it run in the background
                ConsoleHelpers.WriteDebugLine($"Executing background process {handle} asynchronously");
                await process.StartAsync();
                Logger.Info($"Background process {handle} started successfully");
            }
            catch (Exception ex)
            {
                ConsoleHelpers.LogException(ex, $"Error starting background process {handle}");
            }
        });

        // Return the handle to the caller
        return handle;
    }

    /// <summary>
    /// Checks if a process with the specified handle is running.
    /// </summary>
    /// <param name="handle">The process handle.</param>
    /// <returns>True if the process is running, false otherwise.</returns>
    public static bool IsLongRunningProcessRunning(string handle)
    {
        if (string.IsNullOrEmpty(handle) || !_processes.TryGetValue(handle, out var processInfo))
        {
            return false;
        }

        return processInfo.IsRunning;
    }

    /// <summary>
    /// Gets the output from a background process.
    /// </summary>
    /// <param name="handle">The process handle.</param>
    /// <param name="clearBuffer">Whether to clear the output buffer after retrieving.</param>
    /// <returns>A dictionary containing stdout, stderr, and merged output.</returns>
    public static Dictionary<string, string> GetLongRunningProcessOutput(string handle, bool clearBuffer = false)
    {
        if (string.IsNullOrEmpty(handle) || !_processes.TryGetValue(handle, out var processInfo))
        {
            return new Dictionary<string, string>
            {
                { "stdout", string.Empty },
                { "stderr", $"Process with handle {handle} not found." },
                { "merged", $"Process with handle {handle} not found." }
            };
        }

        return processInfo.GetOutput(clearBuffer);
    }

    /// <summary>
    /// Terminates a background process.
    /// </summary>
    /// <param name="handle">The process handle.</param>
    /// <param name="force">Whether to force kill the process.</param>
    /// <returns>True if the process was terminated, false otherwise.</returns>
    public static bool KillLongRunningProcess(string handle, bool force = false)
    {
        if (string.IsNullOrEmpty(handle) || !_processes.TryGetValue(handle, out var processInfo))
        {
            Logger.Warning($"Attempt to kill non-existent background process with handle: {handle}");
            return false;
        }

        try
        {
            var processName = processInfo.Process.FileName;
            var runTime = DateTime.Now - processInfo.StartTime;
            
            if (processInfo.IsRunning)
            {
                Logger.Info($"Terminating background process: {processName} (handle: {handle}, running for {runTime.TotalSeconds:F1}s, force: {force})");
                
                // Send Ctrl+C if not forcing
                if (!force)
                {
                    ConsoleHelpers.WriteDebugLine($"Sending Ctrl+C to background process {handle}");
                    processInfo.Process.SendCtrlCAsync().Wait(500);
                }

                // If process is still running or force is true, kill it
                if (force || processInfo.IsRunning)
                {
                    ConsoleHelpers.WriteDebugLine($"Force killing background process {handle}");
                    processInfo.Process.ForceShutdown();
                }
            }
            else
            {
                Logger.Info($"Background process already exited: {processName} (handle: {handle}, ran for {runTime.TotalSeconds:F1}s)");
            }

            // Remove from our collection
            _processes.TryRemove(handle, out _);
            return true;
        }
        catch (Exception ex)
        {
            ConsoleHelpers.LogException(ex, $"Error killing background process {handle}");
            return false;
        }
    }

    /// <summary>
    /// Gets a list of all running background processes.
    /// </summary>
    /// <returns>A list of process information.</returns>
    public static IList<BackgroundProcessInfo> GetAllProcesses()
    {
        return _processes.Values.ToList();
    }

    /// <summary>
    /// Terminates all background processes.
    /// </summary>
    public static void ShutdownAllProcesses()
    {
        var processCount = _processes.Count;
        if (processCount > 0)
        {
            Logger.Info($"Shutting down all {processCount} background processes");
            
            foreach (var handle in _processes.Keys.ToList())
            {
                KillLongRunningProcess(handle, true);
            }

            _processes.Clear();
            Logger.Info("All background processes terminated");
        }
    }

    /// <summary>
    /// Cleans up old processes that have been running for too long.
    /// </summary>
    /// <param name="state">Timer state (not used).</param>
    private static void CleanupOldProcesses(object? state)
    {
        var now = DateTime.Now;
        var oldProcesses = _processes.Values
            .Where(p => (now - p.StartTime) > _defaultMaxProcessAge)
            .ToList();

        if (oldProcesses.Count > 0)
        {
            Logger.Info($"Cleaning up {oldProcesses.Count} old background processes that exceeded maximum age of {_defaultMaxProcessAge.TotalHours:F1} hours");
            
            foreach (var process in oldProcesses)
            {
                var runTime = now - process.StartTime;
                Logger.Info($"Cleaning up old process: {process.Handle} running for {runTime.TotalHours:F1} hours");
                KillLongRunningProcess(process.Handle, true);
            }
        }
    }
}