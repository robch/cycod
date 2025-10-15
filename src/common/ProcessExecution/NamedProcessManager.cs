using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ShellExecution.Results;

namespace ProcessExecution
{
    /// <summary>
    /// Enhanced manager for background processes that adds naming, input, output pattern matching, and resource monitoring.
    /// </summary>
    public static class NamedProcessManager
    {
        private static readonly ConcurrentDictionary<string, BackgroundProcessInfo> _processes = 
            new ConcurrentDictionary<string, BackgroundProcessInfo>();
        private static readonly ConcurrentDictionary<string, string> _nameToHandleMap = 
            new ConcurrentDictionary<string, string>();
        private static readonly Timer _cleanupTimer;
        private static readonly TimeSpan _defaultCleanupInterval = TimeSpan.FromHours(1);
        private static readonly TimeSpan _defaultMaxProcessAge = TimeSpan.FromHours(8);
        private static readonly ResourceMonitor _resourceMonitor = new ResourceMonitor();
        private static readonly object _lock = new object();
        
        // For generating unique names
        private static int _processCounter = 0;

        /// <summary>
        /// Static constructor to initialize the cleanup timer.
        /// </summary>
        static NamedProcessManager()
        {
            // Create a timer to periodically clean up old processes
            _cleanupTimer = new Timer(CleanupOldProcesses!, null, _defaultCleanupInterval, _defaultCleanupInterval);

            // Register for application exit to clean up all processes
            AppDomain.CurrentDomain.ProcessExit += (sender, e) => ShutdownAllProcesses();
            
            ConsoleHelpers.WriteDebugLine("Enhanced background process manager initialized");
        }

        /// <summary>
        /// Starts a new named background process.
        /// </summary>
        /// <param name="executablePath">Path to the executable file.</param>
        /// <param name="processName">Custom name for the process (optional).</param>
        /// <param name="processArguments">Arguments for the process (optional).</param>
        /// <param name="workingDirectory">Working directory (optional).</param>
        /// <param name="environmentVariables">Environment variables as JSON dictionary (optional).</param>
        /// <param name="limits">Resource limits for the process (optional).</param>
        /// <returns>Resource creation result with process name.</returns>
        public static ResourceCreationResult StartNamedProcess(
            string executablePath,
            string? processName = null,
            string processArguments = "",
            string? workingDirectory = null,
            Dictionary<string, string>? environmentVariables = null,
            ResourceLimits? limits = null)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                if (string.IsNullOrEmpty(executablePath))
                {
                    return new ResourceCreationResult(
                        string.Empty,
                        "Process",
                        false,
                        "Executable path cannot be empty",
                        stopwatch.Elapsed);
                }
                
                // Generate name if not provided
                if (string.IsNullOrEmpty(processName))
                {
                    string baseName = Path.GetFileNameWithoutExtension(executablePath).ToLowerInvariant();
                    processName = GenerateProcessName(baseName);
                }
                
                // Check if name already exists
                if (_nameToHandleMap.ContainsKey(processName))
                {
                    return new ResourceCreationResult(
                        processName,
                        "Process",
                        false,
                        $"Process with name '{processName}' already exists",
                        stopwatch.Elapsed);
                }
                
                // Normalize working directory
                if (!string.IsNullOrEmpty(workingDirectory))
                {
                    workingDirectory = Path.GetFullPath(workingDirectory);
                    if (!Directory.Exists(workingDirectory))
                    {
                        return new ResourceCreationResult(
                            processName,
                            "Process",
                            false,
                            $"Working directory '{workingDirectory}' does not exist",
                            stopwatch.Elapsed);
                    }
                }
                
                ConsoleHelpers.WriteDebugLine($"Starting named process: {processName} ({executablePath} {processArguments})");
                
                // Create process using RunnableProcessBuilder
                var processBuilder = new RunnableProcessBuilder()
                    .WithFileName(executablePath)
                    .WithTimeout(int.MaxValue); // Very long timeout since this is a background process
                
                if (!string.IsNullOrEmpty(processArguments))
                {
                    processBuilder.WithArguments(processArguments);
                }
                
                if (!string.IsNullOrEmpty(workingDirectory))
                {
                    processBuilder.WithWorkingDirectory(workingDirectory);
                }
                
                if (environmentVariables != null && environmentVariables.Count > 0)
                {
                    foreach (var kvp in environmentVariables)
                    {
                        processBuilder.WithEnvironmentVariable(kvp.Key, kvp.Value);
                    }
                }
                
                var process = processBuilder.Build();
                
                // Generate a handle for internal tracking
                string handle = Guid.NewGuid().ToString("N");
                
                // Create and store the process info
                var processInfo = new BackgroundProcessInfo(handle, process)
                {
                    Name = processName,
                    HasCustomName = true
                };
                
                _processes[handle] = processInfo;
                _nameToHandleMap[processName] = handle;
                
                // Start the process asynchronously
                _ = Task.Run(async () =>
                {
                    try
                    {
                        // Start the process but don't await it - let it run in the background
                        ConsoleHelpers.WriteDebugLine($"[PROCESS] Starting async process {processName}");
                        await process.StartAsync();
                        ConsoleHelpers.WriteDebugLine($"[PROCESS] Process {processName} actually started (PID: {process.Process?.Id})");
                        
                        // Register with resource monitor
                        if (process.Process != null)
                        {
                            _resourceMonitor.RegisterResource(
                                process.Process, 
                                processName, 
                                "Process",
                                limits);
                        }
                    }
                    catch (Exception ex)
                    {
                        ConsoleHelpers.WriteDebugLine($"Error starting named process {processName}: {ex.Message}");
                    }
                });
                
                return new ResourceCreationResult(
                    processName,
                    "Process",
                    true,
                    string.Empty,
                    stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                return new ResourceCreationResult(
                    processName ?? "unknown",
                    "Process",
                    false,
                    $"Error starting process: {ex.Message}",
                    stopwatch.Elapsed);
            }
        }
        
        /// <summary>
        /// Gets the internal handle for a named process.
        /// </summary>
        /// <param name="name">The process name.</param>
        /// <returns>The internal handle, or null if not found.</returns>
        private static string GetHandleForName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return null!;
            }
            
            // If the input is already a handle, verify it exists
            if (_processes.ContainsKey(name))
            {
                return name;
            }
            
            // Otherwise, look up by name
            if (_nameToHandleMap.TryGetValue(name, out var handle))
            {
                return handle;
            }
            
            return null!;
        }
        
        /// <summary>
        /// Gets information about a named process.
        /// </summary>
        /// <param name="name">Name of the process.</param>
        /// <returns>The process info, or null if not found.</returns>
        private static BackgroundProcessInfo GetProcessInfo(string name)
        {
            var handle = GetHandleForName(name);
            if (handle == null || !_processes.TryGetValue(handle, out var processInfo))
            {
                return null!;
            }
            
            return processInfo;
        }

        /// <summary>
        /// Checks if a named process exists (running or completed but still tracked).
        /// </summary>
        /// <param name="name">Name or handle of the process.</param>
        /// <returns>True if the process exists in tracking, false otherwise.</returns>
        public static bool ProcessExists(string name)
        {
            var processInfo = GetProcessInfo(name);
            return processInfo != null;
        }

        /// <summary>
        /// Checks if a named process is running.
        /// </summary>
        /// <param name="name">Name or handle of the process.</param>
        /// <returns>True if the process is running, false otherwise.</returns>
        public static bool IsProcessRunning(string name)
        {
            var processInfo = GetProcessInfo(name);
            return processInfo?.IsRunning ?? false;
        }

        /// <summary>
        /// Gets output from a named process.
        /// </summary>
        /// <param name="name">Name or handle of the process.</param>
        /// <param name="outputType">Type of output to retrieve: "stdout", "stderr", or "all".</param>
        /// <param name="clearBuffer">Whether to clear the output buffer after retrieval.</param>
        /// <returns>The process output.</returns>
        public static OutputResult GetProcessOutput(
            string name,
            string outputType = "all",
            bool clearBuffer = false)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                var processInfo = GetProcessInfo(name);
                if (processInfo == null)
                {
                    return new OutputResult(
                        string.Empty,
                        string.Empty,
                        false,
                        false,
                        string.Empty,
                        false,
                        $"Process '{name}' not found",
                        stopwatch.Elapsed);
                }
                
                var output = processInfo.GetOutput(clearBuffer);
                string stdout = output.ContainsKey("stdout") ? output["stdout"] : string.Empty;
                string stderr = output.ContainsKey("stderr") ? output["stderr"] : string.Empty;
                string combined = output.ContainsKey("merged") ? output["merged"] : string.Empty;
                
                return new OutputResult(
                    stdout,
                    stderr,
                    clearBuffer,
                    false,
                    string.Empty,
                    true,
                    string.Empty,
                    stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                return new OutputResult(
                    string.Empty,
                    string.Empty,
                    false,
                    false,
                    string.Empty,
                    false,
                    $"Error getting process output: {ex.Message}",
                    stopwatch.Elapsed);
            }
        }

        /// <summary>
        /// Sends input to a named process.
        /// </summary>
        /// <param name="name">Name or handle of the process.</param>
        /// <param name="input">Text to send as input.</param>
        /// <returns>True if input was sent successfully, false otherwise.</returns>
        public static bool SendInputToProcess(string name, string input)
        {
            try
            {
                var processInfo = GetProcessInfo(name);
                if (processInfo == null || !processInfo.IsRunning)
                {
                    return false;
                }
                
                // Add a newline if not already present
                if (!input.EndsWith("\n"))
                {
                    input += "\n";
                }
                
                processInfo.Process.SendInputAsync(input).Wait();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Waits for output matching a pattern from a named process.
        /// </summary>
        /// <param name="name">Name or handle of the process.</param>
        /// <param name="pattern">Regular expression pattern to wait for.</param>
        /// <param name="timeoutMs">Timeout in milliseconds, -1 for indefinite.</param>
        /// <param name="matchedOutput">The matched output.</param>
        /// <returns>True if pattern was matched within timeout, false otherwise.</returns>
        public static bool WaitForProcessOutput(
            string name, 
            string pattern, 
            int timeoutMs, 
            out string matchedOutput)
        {
            matchedOutput = null!;
            
            try
            {
                var processInfo = GetProcessInfo(name);
                if (processInfo == null || !processInfo.IsRunning)
                {
                    return false;
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
                    if (!processInfo.IsRunning)
                    {
                        return false;
                    }
                    
                    // Get current output
                    var output = processInfo.GetOutput(false);
                    currentOutput = output.ContainsKey("merged") ? output["merged"] : string.Empty;
                    
                    // Check for pattern match
                    var match = regex.Match(currentOutput);
                    if (match.Success)
                    {
                        matchedOutput = match.Value;
                        return true;
                    }
                    
                    // Wait a bit before checking again
                    Thread.Sleep(100);
                }
                
                // Timeout occurred
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Terminates a named process.
        /// </summary>
        /// <param name="name">Name or handle of the process.</param>
        /// <param name="force">Whether to force kill if graceful termination fails.</param>
        /// <returns>Result of the termination operation.</returns>
        public static TerminationResult TerminateProcess(string name, bool force = false)
        {
            var stopwatch = Stopwatch.StartNew();
            bool wasRunning = false;
            
            try
            {
                // Get the process handle
                var handle = GetHandleForName(name);
                if (handle == null || !_processes.TryGetValue(handle, out var processInfo))
                {
                    return new TerminationResult(
                        false,
                        false,
                        null,
                        false,
                        $"Process '{name}' not found",
                        stopwatch.Elapsed);
                }
                
                // Check if it's running
                wasRunning = processInfo.IsRunning;
                
                if (wasRunning)
                {
                    ConsoleHelpers.WriteDebugLine($"Terminating process {name} (force: {force})");
                    
                    try
                    {
                        // Send Ctrl+C if not forcing
                        if (!force)
                        {
                            processInfo.Process.SendCtrlCAsync().Wait(1000);
                            
                            // Check if process exited gracefully
                            if (!processInfo.IsRunning)
                            {
                                // Successfully terminated gracefully
                                _resourceMonitor.UnregisterResource(name);
                                _processes.TryRemove(handle, out _);
                                _nameToHandleMap.TryRemove(processInfo.Name, out _);
                                
                                return new TerminationResult(
                                    true,
                                    false,
                                    0,
                                    true,
                                    string.Empty,
                                    stopwatch.Elapsed);
                            }
                        }
                        
                        // If still running or force=true, forcefully kill it
                        if (processInfo.IsRunning || force)
                        {
                            processInfo.Process.ForceShutdown();
                        }
                    }
                    catch (Exception ex)
                    {
                        return new TerminationResult(
                            wasRunning,
                            force,
                            null,
                            false,
                            $"Error terminating process: {ex.Message}",
                            stopwatch.Elapsed);
                    }
                }
                
                // Clean up resources
                _resourceMonitor.UnregisterResource(name);
                _processes.TryRemove(handle, out _);
                _nameToHandleMap.TryRemove(processInfo.Name, out _);
                
                return new TerminationResult(
                    wasRunning,
                    force,
                    0,
                    true,
                    string.Empty,
                    stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                return new TerminationResult(
                    wasRunning,
                    force,
                    null,
                    false,
                    $"Error terminating process: {ex.Message}",
                    stopwatch.Elapsed);
            }
        }

        /// <summary>
        /// Renames a process.
        /// </summary>
        /// <param name="oldName">Current name of the process.</param>
        /// <param name="newName">New name for the process.</param>
        /// <returns>True if rename was successful, false otherwise.</returns>
        public static bool RenameProcess(string oldName, string newName)
        {
            if (string.IsNullOrEmpty(oldName) || string.IsNullOrEmpty(newName))
            {
                return false;
            }
            
            if (oldName == newName)
            {
                return true; // Already has the desired name
            }
            
            // Check if new name already exists
            if (_nameToHandleMap.ContainsKey(newName))
            {
                return false;
            }
            
            lock (_lock)
            {
                // Get the process handle
                var handle = GetHandleForName(oldName);
                if (handle == null || !_processes.TryGetValue(handle, out var processInfo))
                {
                    return false;
                }
                
                // Update the name in the process info
                string oldNameForMapping = processInfo.Name;
                processInfo.Name = newName;
                
                // Update the name mapping
                _nameToHandleMap.TryRemove(oldNameForMapping, out _);
                _nameToHandleMap[newName] = handle;
                
                // Update resource monitor registration
                _resourceMonitor.UnregisterResource(oldNameForMapping);
                if (processInfo.IsRunning)
                {
                    _resourceMonitor.RegisterResource(
                        processInfo.Process.Process!, 
                        newName, 
                        "Process");
                }
                
                return true;
            }
        }

        /// <summary>
        /// Gets resource usage for a process.
        /// </summary>
        /// <param name="name">Name or handle of the process.</param>
        /// <returns>Resource usage information, or null if not available.</returns>
        public static ResourceUsage GetProcessResourceUsage(string name)
        {
            return _resourceMonitor.GetResourceUsage(name);
        }

        /// <summary>
        /// Sets resource limits for a process.
        /// </summary>
        /// <param name="name">Name or handle of the process.</param>
        /// <param name="limits">Resource limits to set.</param>
        /// <returns>True if limits were set, false otherwise.</returns>
        public static bool SetProcessResourceLimits(string name, ResourceLimits limits)
        {
            return _resourceMonitor.SetResourceLimits(name, limits);
        }

        /// <summary>
        /// Gets a list of all process names.
        /// </summary>
        /// <returns>List of process names.</returns>
        public static List<string> GetAllProcessNames()
        {
            return _nameToHandleMap.Keys.ToList();
        }

        /// <summary>
        /// Gets information about all processes.
        /// </summary>
        /// <returns>List of dictionaries with process information.</returns>
        public static List<Dictionary<string, string>> GetAllProcessInfo()
        {
            var result = new List<Dictionary<string, string>>();
            
            foreach (var name in GetAllProcessNames())
            {
                var processInfo = GetProcessInfo(name);
                if (processInfo != null)
                {
                    var info = new Dictionary<string, string>
                    {
                        ["Name"] = processInfo.Name,
                        ["Handle"] = processInfo.Handle,
                        ["IsRunning"] = processInfo.IsRunning.ToString(),
                        ["StartTime"] = processInfo.StartTime.ToString("yyyy-MM-dd HH:mm:ss"),
                        ["RunningTime"] = (DateTime.Now - processInfo.StartTime).ToString(@"hh\:mm\:ss")
                    };
                    
                    var resourceUsage = _resourceMonitor.GetResourceUsage(name);
                    if (resourceUsage != null)
                    {
                        info["MemoryUsage"] = $"{resourceUsage.MemoryBytes / 1024 / 1024} MB";
                        info["CpuUsage"] = $"{resourceUsage.CpuPercent:F1}%";
                    }
                    
                    result.Add(info);
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// Generates a unique process name based on a base name.
        /// </summary>
        /// <param name="baseName">Base name for the process.</param>
        /// <returns>A unique process name.</returns>
        private static string GenerateProcessName(string baseName)
        {
            // Clean up the base name
            baseName = string.IsNullOrEmpty(baseName) ? "process" : baseName;
            baseName = Regex.Replace(baseName, @"[^\w\-\.]", "").ToLower();
            baseName = baseName.Length > 20 ? baseName.Substring(0, 20) : baseName;
            
            int counter = Interlocked.Increment(ref _processCounter);
            string name;
            
            do
            {
                name = $"{baseName}-{counter}";
                counter++;
            } while (_nameToHandleMap.ContainsKey(name));
            
            return name;
        }

        /// <summary>
        /// Terminates all processes.
        /// </summary>
        public static void ShutdownAllProcesses()
        {
            var processNames = GetAllProcessNames();
            if (processNames.Count > 0)
            {
                ConsoleHelpers.WriteDebugLine($"Shutting down all {processNames.Count} background processes");
                
                foreach (var name in processNames)
                {
                    TerminateProcess(name, true);
                }
            }
        }
        
        /// <summary>
        /// Cleans up old processes that have been running for too long.
        /// </summary>
        /// <param name="state">Timer state (not used).</param>
        private static void CleanupOldProcesses(object state)
        {
            try
            {
                var now = DateTime.Now;
                var handles = _processes.Keys.ToList();
                
                foreach (var handle in handles)
                {
                    if (_processes.TryGetValue(handle, out var processInfo))
                    {
                        var age = now - processInfo.StartTime;
                        
                        // Check if process has been running too long
                        if (age > _defaultMaxProcessAge)
                        {
                            ConsoleHelpers.WriteDebugLine($"Cleaning up old process {processInfo.Name} running for {age.TotalHours:F1} hours");
                            TerminateProcess(processInfo.Name, true);
                        }
                        
                        // Check if process has exited but wasn't cleaned up
                        if (!processInfo.IsRunning)
                        {
                            ConsoleHelpers.WriteDebugLine($"Cleaning up terminated process {processInfo.Name}");
                            _nameToHandleMap.TryRemove(processInfo.Name, out _);
                            _processes.TryRemove(handle, out _);
                            _resourceMonitor.UnregisterResource(processInfo.Name);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleHelpers.WriteDebugLine($"Error in background process cleanup: {ex.Message}");
            }
        }

        /// <summary>
        /// Add backward compatibility methods that delegate to existing BackgroundProcessManager
        /// </summary>
        public static string StartLongRunningProcess(string processName, string processArguments = "", string? workingDirectory = null)
        {
            return BackgroundProcessManager.StartLongRunningProcess(processName, processArguments, workingDirectory);
        }

        public static bool IsLongRunningProcessRunning(string handle)
        {
            return BackgroundProcessManager.IsLongRunningProcessRunning(handle);
        }

        public static Dictionary<string, string> GetLongRunningProcessOutput(string handle, bool clearBuffer = false)
        {
            return BackgroundProcessManager.GetLongRunningProcessOutput(handle, clearBuffer);
        }

        public static bool KillLongRunningProcess(string handle, bool force = false)
        {
            return BackgroundProcessManager.KillLongRunningProcess(handle, force);
        }

        public static IList<BackgroundProcessInfo> GetAllProcesses()
        {
            return BackgroundProcessManager.GetAllProcesses();
        }
    }
}