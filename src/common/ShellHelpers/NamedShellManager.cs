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
    /// Information about a command currently executing in a shell.
    /// </summary>
    public class ShellCommandInfo
    {
        /// <summary>
        /// Gets the command being executed.
        /// </summary>
        public string Command { get; }
        
        /// <summary>
        /// Gets the time when the command started.
        /// </summary>
        public DateTime StartTime { get; }
        
        /// <summary>
        /// Gets the timeout for the command.
        /// </summary>
        public int TimeoutMs { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ShellCommandInfo"/> class.
        /// </summary>
        public ShellCommandInfo(string command, DateTime startTime, int timeoutMs)
        {
            Command = command;
            StartTime = startTime;
            TimeoutMs = timeoutMs;
        }

        /// <summary>
        /// Gets the running time of the command.
        /// </summary>
        public TimeSpan RunningTime => DateTime.Now - StartTime;
    }

    /// <summary>
    /// Manages named shell instances.
    /// </summary>
    public class NamedShellManager
    {
        private readonly ConcurrentDictionary<string, ShellSession> _shells = new ConcurrentDictionary<string, ShellSession>();
        private readonly ConcurrentDictionary<string, ShellCommandInfo> _runningCommands = new ConcurrentDictionary<string, ShellCommandInfo>();
        private readonly Timer _cleanupTimer;
        private readonly TimeSpan _defaultCleanupInterval = TimeSpan.FromMinutes(15);
        private readonly TimeSpan _autoPromotedShellTimeout = TimeSpan.FromMinutes(30);
        private readonly object _syncLock = new object();
        private readonly ResourceMonitor _resourceMonitor;
        
        // This is used for ensuring unique auto-promoted shell names
        private static int _autoShellCounter = 0;

        /// <summary>
        /// Singleton instance of the NamedShellManager.
        /// </summary>
        public static NamedShellManager Instance { get; } = new NamedShellManager();

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedShellManager"/> class.
        /// </summary>
        private NamedShellManager()
        {
            _cleanupTimer = new Timer(CleanupIdleShells!, null, _defaultCleanupInterval, _defaultCleanupInterval);
            _resourceMonitor = new ResourceMonitor();

            // Register for process exit to clean up all shells
            AppDomain.CurrentDomain.ProcessExit += (s, e) => ShutdownAllShells();
        }

        /// <summary>
        /// Creates a new named shell.
        /// </summary>
        /// <param name="shellType">Type of shell to create.</param>
        /// <param name="shellName">Optional name for the shell (auto-generated if null).</param>
        /// <param name="workingDirectory">Optional working directory.</param>
        /// <param name="environmentVariables">Optional environment variables as dictionary.</param>
        /// <returns>Result containing the shell name.</returns>
        public ResourceCreationResult CreateShell(
            PersistentShellType shellType,
            string? shellName = null,
            string? workingDirectory = null,
            Dictionary<string, string>? environmentVariables = null)
        {
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                // Generate a name if not provided
                if (string.IsNullOrEmpty(shellName))
                {
                    shellName = GenerateShellName(shellType);
                }
                
                // Check if shell name already exists
                if (_shells.ContainsKey(shellName))
                {
                    return new ResourceCreationResult(
                        shellName,
                        "Shell",
                        false,
                        $"Shell with name '{shellName}' already exists",
                        stopwatch.Elapsed);
                }

                // Normalize working directory
                if (!string.IsNullOrEmpty(workingDirectory))
                {
                    workingDirectory = Path.GetFullPath(workingDirectory);
                    if (!Directory.Exists(workingDirectory))
                    {
                        return new ResourceCreationResult(
                            shellName,
                            "Shell",
                            false,
                            $"Working directory '{workingDirectory}' does not exist",
                            stopwatch.Elapsed);
                    }
                }

                // Create shell session
                ShellSession shell;
                switch (shellType)
                {
                    case PersistentShellType.Bash:
                        shell = new BashShellSession(shellName);
                        break;
                    case PersistentShellType.Cmd:
                        shell = new CmdShellSession(shellName);
                        break;
                    case PersistentShellType.PowerShell:
                        shell = new PowershellShellSession(shellName);
                        break;
                    default:
                        return new ResourceCreationResult(
                            shellName,
                            "Shell",
                            false,
                            $"Unsupported shell type: {shellType}",
                            stopwatch.Elapsed);
                }

                // Initialize shell
                shell.Initialize();
                
                // Set working directory if provided
                if (!string.IsNullOrEmpty(workingDirectory))
                {
                    string cdCommand = GetChangeDirectoryCommand(shellType, workingDirectory);
                    shell.ExecuteCommandAsync(cdCommand, 10000).Wait();
                }

                // Set environment variables if provided
                if (environmentVariables != null && environmentVariables.Count > 0)
                {
                    foreach (var kvp in environmentVariables)
                    {
                        string envCommand = GetSetEnvironmentVariableCommand(shellType, kvp.Key, kvp.Value);
                        shell.ExecuteCommandAsync(envCommand, 5000).Wait();
                    }
                }

                // Store shell in registry
                _shells[shellName] = shell;
                
                // Register shell process with resource monitor
                var process = shell.GetUnderlyingProcess();
                if (process != null)
                {
                    _resourceMonitor.RegisterResource(process, shellName, "Shell");
                }
                
                return new ResourceCreationResult(
                    shellName,
                    "Shell",
                    true,
                    string.Empty,
                    stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                return new ResourceCreationResult(
                    shellName ?? "unknown",
                    "Shell",
                    false,
                    $"Failed to create shell: {ex.Message}",
                    stopwatch.Elapsed);
            }
        }

        /// <summary>
        /// Creates an auto-promoted shell from a timed-out command.
        /// </summary>
        /// <param name="shellType">Type of shell to create.</param>
        /// <param name="originalCommand">The original command that timed out.</param>
        /// <param name="workingDirectory">Optional working directory.</param>
        /// <returns>Name of the created shell.</returns>
        public string CreateAutoPromotedShell(PersistentShellType shellType, string originalCommand, string? workingDirectory = null)
        {
            // Generate a unique name for auto-promoted shell
            string timestamp = DateTime.Now.ToString("yyyyMMddTHHmmss");
            int id = Interlocked.Increment(ref _autoShellCounter);
            string shellName = $"auto-{shellType.ToString().ToLower()}-{timestamp}-{id:X4}";
            
            var result = CreateShell(shellType, shellName, workingDirectory);
            
            if (result.Success)
            {
                // We add a comment to the shell to help identify its origin
                var shellTypeStr = shellType.ToString().ToLower();
                string commentCommand = shellTypeStr == "bash" 
                    ? $"echo \"# Auto-promoted shell created from timed-out command: {originalCommand}\" > /dev/null"
                    : shellTypeStr == "powershell" 
                        ? $"Write-Host \"# Auto-promoted shell created from timed-out command: {originalCommand}\" > $null"
                        : $"echo # Auto-promoted shell created from timed-out command: {originalCommand} > nul";
                
                GetShell(shellName)?.ExecuteCommandAsync(commentCommand, 5000).Wait();
                
                // For auto-promoted shells, we set a shorter idle timeout
                SetShellTimeout(shellName, _autoPromotedShellTimeout);
            }
            
            return shellName;
        }

        /// <summary>
        /// Gets a shell by name.
        /// </summary>
        /// <param name="shellName">Name of the shell.</param>
        /// <returns>The shell session, or null if not found.</returns>
        public ShellSession GetShell(string shellName)
        {
            if (string.IsNullOrEmpty(shellName) || !_shells.TryGetValue(shellName, out var shell))
            {
                return null!;
            }
            
            return shell;
        }

        /// <summary>
        /// Executes a command in a named shell.
        /// </summary>
        /// <param name="shellName">Name of the shell.</param>
        /// <param name="command">Command to execute.</param>
        /// <param name="timeoutMs">Timeout in milliseconds.</param>
        /// <param name="waitForShellMs">Optional time to wait for shell to become available.</param>
        /// <returns>Result of the command execution.</returns>
        public async Task<ShellCommandResult> ExecuteInShellAsync(
            string shellName,
            string command,
            int timeoutMs = 60000,
            int? waitForShellMs = null)
        {
            var stopwatch = Stopwatch.StartNew();
            
            // Try to get the shell
            ShellSession shell = GetShell(shellName);
            if (shell == null)
            {
                return new ShellCommandResult(
                    string.Empty,
                    string.Empty,
                    -1,
                    false,
                    shellName,
                    false,
                    false,
                    $"Shell '{shellName}' not found",
                    stopwatch.Elapsed);
            }

            // Check if shell is busy
            if (IsShellBusy(shellName, out var currentCommand))
            {
                // If waitForShellMs is provided, wait for the shell to become available
                if (waitForShellMs.HasValue && waitForShellMs.Value > 0)
                {
                    bool becameAvailable = await WaitForShellAvailabilityAsync(shellName, waitForShellMs.Value);
                    if (!becameAvailable)
                    {
                        return new ShellCommandResult(
                            string.Empty,
                            string.Empty,
                            -1,
                            false,
                            shellName,
                            false,
                            false,
                            $"Shell '{shellName}' is busy with another command and did not become available within {waitForShellMs.Value}ms",
                            stopwatch.Elapsed);
                    }
                }
                else
                {
                    // Otherwise, fail immediately
                    return new ShellCommandResult(
                        string.Empty,
                        string.Empty,
                        -1,
                        false,
                        shellName,
                        false,
                        false,
                        $"Shell '{shellName}' is busy with command: {currentCommand?.Command}",
                        stopwatch.Elapsed);
                }
            }

            // Mark shell as busy with this command
            var commandInfo = new ShellCommandInfo(command, DateTime.Now, timeoutMs);
            _runningCommands[shellName] = commandInfo;

            try
            {
                // Update last activity time
                shell.UpdateLastActivityTime();
                
                // Execute the command
                var result = await shell.ExecuteCommandAsync(command, timeoutMs);
                
                // Return the result
                return ShellCommandResult.FromPersistentShellCommandResult(result, shellName);
            }
            catch (Exception ex)
            {
                return new ShellCommandResult(
                    string.Empty,
                    ex.Message,
                    -1,
                    false,
                    shellName,
                    false,
                    false,
                    $"Error executing command: {ex.Message}",
                    stopwatch.Elapsed);
            }
            finally
            {
                // Mark shell as no longer busy
                _runningCommands.TryRemove(shellName, out _);
            }
        }

        /// <summary>
        /// Checks if a shell is busy executing a command.
        /// </summary>
        /// <param name="shellName">Name of the shell.</param>
        /// <param name="commandInfo">Information about the currently executing command, if any.</param>
        /// <returns>True if the shell is busy, false otherwise.</returns>
        public bool IsShellBusy(string shellName, out ShellCommandInfo commandInfo)
        {
            commandInfo = null!;
            
            if (string.IsNullOrEmpty(shellName) || !_shells.ContainsKey(shellName))
            {
                return false;
            }
            
            bool result = _runningCommands.TryGetValue(shellName, out var tempCommandInfo);
            commandInfo = tempCommandInfo!;
            return result;
        }

        /// <summary>
        /// Waits for a shell to become available (not busy).
        /// </summary>
        /// <param name="shellName">Name of the shell.</param>
        /// <param name="timeoutMs">Maximum time to wait in milliseconds.</param>
        /// <returns>True if the shell became available within the timeout, false otherwise.</returns>
        public async Task<bool> WaitForShellAvailabilityAsync(string shellName, int timeoutMs)
        {
            if (string.IsNullOrEmpty(shellName) || !_shells.ContainsKey(shellName))
            {
                return false;
            }

            var endTime = DateTime.Now.AddMilliseconds(timeoutMs);
            
            while (DateTime.Now < endTime)
            {
                if (!IsShellBusy(shellName, out _))
                {
                    return true;
                }
                
                await Task.Delay(100);
            }
            
            return !IsShellBusy(shellName, out _);
        }

        /// <summary>
        /// Sends input to a shell.
        /// </summary>
        /// <param name="shellName">Name of the shell.</param>
        /// <param name="input">Text to send as input.</param>
        /// <returns>True if input was sent successfully, false otherwise.</returns>
        public async Task<bool> SendInputToShellAsync(string shellName, string input)
        {
            var shell = GetShell(shellName);
            if (shell == null)
            {
                return false;
            }
            
            try
            {
                // Update last activity time
                shell.UpdateLastActivityTime();
                
                return await shell.SendInputAsync(input);
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Waits for output matching a pattern from a shell.
        /// </summary>
        /// <param name="shellName">Name of the shell.</param>
        /// <param name="pattern">Regular expression pattern to wait for.</param>
        /// <param name="timeoutMs">Timeout in milliseconds, -1 for indefinite.</param>
        /// <returns>The matched output, or null if timeout or error.</returns>
        public async Task<string> WaitForShellOutputAsync(string shellName, string pattern, int timeoutMs = -1)
        {
            var shell = GetShell(shellName);
            if (shell == null)
            {
                return null!;
            }
            
            try
            {
                // Update last activity time
                shell.UpdateLastActivityTime();
                
                return await shell.WaitForOutputPatternAsync(pattern, timeoutMs);
            }
            catch
            {
                return null!;
            }
        }

        /// <summary>
        /// Renames a shell.
        /// </summary>
        /// <param name="oldName">Current name of the shell.</param>
        /// <param name="newName">New name for the shell.</param>
        /// <returns>True if renamed successfully, false otherwise.</returns>
        public bool RenameShell(string oldName, string newName)
        {
            if (string.IsNullOrEmpty(oldName) || string.IsNullOrEmpty(newName))
            {
                return false;
            }
            
            // Check if old name exists and new name doesn't
            if (!_shells.TryGetValue(oldName, out var shell) || _shells.ContainsKey(newName))
            {
                return false;
            }
            
            // Don't allow renaming busy shells
            if (IsShellBusy(oldName, out _))
            {
                return false;
            }
            
            // Remove old entry and add new one
            if (_shells.TryRemove(oldName, out shell))
            {
                shell.Rename(newName);
                _shells[newName] = shell;
                
                // Update resource monitor
                _resourceMonitor.UnregisterResource(oldName);
                var process = shell.GetUnderlyingProcess();
                if (process != null)
                {
                    _resourceMonitor.RegisterResource(process, newName, "Shell");
                }
                
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Sets a custom timeout for a shell.
        /// </summary>
        /// <param name="shellName">Name of the shell.</param>
        /// <param name="idleTimeout">Idle timeout after which the shell will be terminated.</param>
        /// <returns>True if timeout was set, false otherwise.</returns>
        public bool SetShellTimeout(string shellName, TimeSpan idleTimeout)
        {
            var shell = GetShell(shellName);
            if (shell == null)
            {
                return false;
            }
            
            shell.IdleTimeout = idleTimeout;
            return true;
        }

        /// <summary>
        /// Gets the state of a shell.
        /// </summary>
        /// <param name="shellName">Name of the shell.</param>
        /// <returns>Shell state, or Terminated if the shell is not found.</returns>
        public ShellState GetShellState(string shellName)
        {
            var shell = GetShell(shellName);
            if (shell == null)
            {
                return ShellState.Terminated;
            }
            
            if (IsShellBusy(shellName, out _))
            {
                return ShellState.Busy;
            }
            
            return shell.State;
        }

        /// <summary>
        /// Gets information about a shell.
        /// </summary>
        /// <param name="shellName">Name of the shell.</param>
        /// <returns>Dictionary containing shell information.</returns>
        public Dictionary<string, string> GetShellInfo(string shellName)
        {
            var info = new Dictionary<string, string>();
            
            var shell = GetShell(shellName);
            if (shell == null)
            {
                info["State"] = "Not Found";
                return info;
            }
            
            info["Name"] = shellName;
            info["Type"] = shell.GetShellType().ToString();
            info["State"] = GetShellState(shellName).ToString();
            info["CreatedAt"] = shell.CreationTime.ToString("yyyy-MM-dd HH:mm:ss");
            info["LastActivity"] = shell.LastActivityTime.ToString("yyyy-MM-dd HH:mm:ss");
            info["IdleTimeout"] = shell.IdleTimeout.ToString();
            
            IsShellBusy(shellName, out var commandInfo);
            if (commandInfo != null)
            {
                info["CurrentCommand"] = commandInfo.Command;
                info["CommandStarted"] = commandInfo.StartTime.ToString("yyyy-MM-dd HH:mm:ss");
                info["CommandRunningTime"] = commandInfo.RunningTime.ToString(@"hh\:mm\:ss");
            }
            
            var resourceUsage = _resourceMonitor.GetResourceUsage(shellName);
            if (resourceUsage != null)
            {
                info["MemoryUsage"] = $"{resourceUsage.MemoryBytes / 1024 / 1024} MB";
                info["CpuUsage"] = $"{resourceUsage.CpuPercent:F1}%";
            }
            
            return info;
        }

        /// <summary>
        /// Gets a list of all shell names.
        /// </summary>
        /// <returns>List of shell names.</returns>
        public List<string> GetAllShellNames()
        {
            return _shells.Keys.ToList();
        }

        /// <summary>
        /// Terminates a shell and removes it from the registry.
        /// </summary>
        /// <param name="shellName">Name of the shell to terminate.</param>
        /// <param name="force">Whether to force kill if graceful termination fails.</param>
        /// <returns>Result of the termination.</returns>
        public TerminationResult TerminateShell(string shellName, bool force = false)
        {
            var stopwatch = Stopwatch.StartNew();
            bool wasRunning = false;
            
            try
            {
                if (string.IsNullOrEmpty(shellName) || !_shells.TryRemove(shellName, out var shell))
                {
                    return new TerminationResult(
                        false,
                        false,
                        null,
                        false,
                        $"Shell '{shellName}' not found",
                        stopwatch.Elapsed);
                }
                
                wasRunning = shell.State != ShellState.Terminated;
                
                // Remove from running commands if present
                _runningCommands.TryRemove(shellName, out _);
                
                // Unregister from resource monitor
                _resourceMonitor.UnregisterResource(shellName);
                
                // Terminate the shell
                if (force)
                {
                    shell.ForceShutdown();
                }
                else
                {
                    shell.Shutdown();
                }
                
                return new TerminationResult(
                    wasRunning,
                    force,
                    null,
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
                    $"Error terminating shell: {ex.Message}",
                    stopwatch.Elapsed);
            }
        }

        /// <summary>
        /// Shuts down all shells.
        /// </summary>
        public void ShutdownAllShells()
        {
            foreach (var shellName in _shells.Keys.ToList())
            {
                TerminateShell(shellName);
            }
            
            _shells.Clear();
            _runningCommands.Clear();
        }

        /// <summary>
        /// Gets resource usage for a shell.
        /// </summary>
        /// <param name="shellName">Name of the shell.</param>
        /// <returns>Resource usage information, or null if not available.</returns>
        public ResourceUsage GetShellResourceUsage(string shellName)
        {
            return _resourceMonitor.GetResourceUsage(shellName);
        }

        /// <summary>
        /// Sets resource limits for a shell.
        /// </summary>
        /// <param name="shellName">Name of the shell.</param>
        /// <param name="limits">Resource limits to set.</param>
        /// <returns>True if limits were set, false otherwise.</returns>
        public bool SetShellResourceLimits(string shellName, ResourceLimits limits)
        {
            return _resourceMonitor.SetResourceLimits(shellName, limits);
        }

        private string GenerateShellName(PersistentShellType shellType)
        {
            string baseName = shellType.ToString().ToLower();
            int counter = 1;
            string name;
            
            do
            {
                name = $"{baseName}-{counter}";
                counter++;
            } while (_shells.ContainsKey(name));
            
            return name;
        }

        private string GetChangeDirectoryCommand(PersistentShellType shellType, string directory)
        {
            string escapedDir = EscapePathForShell(shellType, directory);
            
            return shellType switch
            {
                PersistentShellType.Bash => $"cd \"{escapedDir}\"",
                PersistentShellType.Cmd => $"cd /d \"{escapedDir}\"",
                PersistentShellType.PowerShell => $"Set-Location -Path \"{escapedDir}\"",
                _ => $"cd \"{escapedDir}\""
            };
        }

        private string GetSetEnvironmentVariableCommand(PersistentShellType shellType, string name, string value)
        {
            string escapedValue = EscapeValueForShell(shellType, value);
            
            return shellType switch
            {
                PersistentShellType.Bash => $"export {name}=\"{escapedValue}\"",
                PersistentShellType.Cmd => $"set \"{name}={escapedValue}\"",
                PersistentShellType.PowerShell => $"$env:{name} = \"{escapedValue}\"",
                _ => $"export {name}=\"{escapedValue}\""
            };
        }

        private string EscapePathForShell(PersistentShellType shellType, string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }
            
            // Escape based on shell type
            switch (shellType)
            {
                case PersistentShellType.Bash:
                    // For Bash, escape spaces, parentheses, etc.
                    return path.Replace("\"", "\\\"")
                               .Replace("$", "\\$")
                               .Replace("`", "\\`")
                               .Replace("!", "\\!");
                case PersistentShellType.Cmd:
                    // For CMD, escape special characters
                    return path.Replace("\"", "\\\"")
                               .Replace("&", "^&")
                               .Replace("|", "^|")
                               .Replace("(", "^(")
                               .Replace(")", "^)")
                               .Replace("<", "^<")
                               .Replace(">", "^>");
                case PersistentShellType.PowerShell:
                    // For PowerShell, escape special characters
                    return path.Replace("\"", "`\"")
                               .Replace("$", "`$")
                               .Replace("`", "``")
                               .Replace("(", "\\(")
                               .Replace(")", "\\)");
                default:
                    return path;
            }
        }

        private string EscapeValueForShell(PersistentShellType shellType, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            
            // Escape based on shell type (similar to path escaping but potentially different)
            switch (shellType)
            {
                case PersistentShellType.Bash:
                    return value.Replace("\"", "\\\"")
                               .Replace("$", "\\$")
                               .Replace("`", "\\`")
                               .Replace("!", "\\!");
                case PersistentShellType.Cmd:
                    return value.Replace("\"", "\\\"")
                               .Replace("&", "^&")
                               .Replace("|", "^|")
                               .Replace("(", "^(")
                               .Replace(")", "^)")
                               .Replace("<", "^<")
                               .Replace(">", "^>");
                case PersistentShellType.PowerShell:
                    return value.Replace("\"", "`\"")
                               .Replace("$", "`$")
                               .Replace("`", "``");
                default:
                    return value;
            }
        }

        private void CleanupIdleShells(object state)
        {
            try
            {
                var now = DateTime.Now;
                var shellsToTerminate = new List<string>();
                
                // Find shells that have been idle for too long
                foreach (var kvp in _shells)
                {
                    string shellName = kvp.Key;
                    ShellSession shell = kvp.Value;
                    
                    // Skip busy shells
                    if (IsShellBusy(shellName, out _))
                    {
                        continue;
                    }
                    
                    // Check idle time
                    TimeSpan idleTime = now - shell.LastActivityTime;
                    if (idleTime > shell.IdleTimeout)
                    {
                        shellsToTerminate.Add(shellName);
                    }
                }
                
                // Terminate idle shells
                foreach (var shellName in shellsToTerminate)
                {
                    ConsoleHelpers.WriteDebugLine($"Terminating idle shell: {shellName}");
                    TerminateShell(shellName);
                }
            }
            catch (Exception ex)
            {
                ConsoleHelpers.WriteDebugLine($"Error in NamedShellManager cleanup: {ex.Message}");
            }
        }
    }
}