using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using ProcessExecution;
using ShellExecution.Results;

/// <summary>
/// Provides a unified interface for managing shell commands and background processes.
/// </summary>
public class ShellAndProcessHelperFunctions
{
    [Description("Executes a command in a shell with adaptive timeout handling. If the command completes within the expected timeout, returns the result directly. If it exceeds the timeout, automatically converts to a managed shell that can be interacted with later.")]
    public async Task<string> RunShellCommand(
        [Description("The command to execute")] string command,
        [Description("Shell type to use: bash, cmd, powershell")] string shellType = "bash",
        [Description("Expected timeout in milliseconds")] int expectedTimeout = 60000,
        [Description("Working directory for the command")] string workingDir = "",
        [Description("Environment variables as JSON object")] string environmentVariables = "",
        [Description("Maximum number of characters to display per line.")] int maxCharsPerLine = 500,
        [Description("Maximum total number of characters to display.")] int maxTotalChars = 100000)
    {
        if (string.IsNullOrEmpty(command))
        {
            return "Error: Command cannot be empty";
        }

        // Parse shell type
        PersistentShellType parsedShellType;
        switch (shellType.ToLowerInvariant())
        {
            case "bash":
                parsedShellType = PersistentShellType.Bash;
                break;
            case "cmd":
                parsedShellType = PersistentShellType.Cmd;
                break;
            case "powershell":
                parsedShellType = PersistentShellType.PowerShell;
                break;
            default:
                return $"Error: Unknown shell type '{shellType}'. Valid values are: bash, cmd, powershell";
        }

        // Parse environment variables if provided
        Dictionary<string, string>? envVars = null;
        if (!string.IsNullOrEmpty(environmentVariables))
        {
            try
            {
                envVars = JsonSerializer.Deserialize<Dictionary<string, string>>(environmentVariables);
            }
            catch (Exception ex)
            {
                return $"Error: Failed to parse environment variables: {ex.Message}";
            }
        }

        try
        {
            // Create a temporary shell for this command
            string tempShellName = $"temp-{Guid.NewGuid():N}";
            var shellResult = NamedShellProcessManager.Instance.CreateShell(parsedShellType, tempShellName, workingDir, envVars);

            if (!shellResult.Success)
            {
                return $"Error creating shell: {shellResult.ErrorMessage}";
            }

            // Execute the command with timeout
            var result = await NamedShellProcessManager.Instance.ExecuteInShellAsync(tempShellName, command, expectedTimeout);

            // If command timed out, promote the shell to a named shell
            if (result.TimedOut)
            {
                // Generate name for auto-promoted shell
                string promotedShellName = NamedShellProcessManager.GenerateAutoPromotedShellName(parsedShellType);
                
                // Promote the existing temporary shell instead of creating new and terminating old
                bool promotionSuccess = NamedShellProcessManager.Instance.PromoteShellToNamed(tempShellName, promotedShellName);
                
                if (!promotionSuccess)
                {
                    // Fallback: terminate temp shell and return error
                    NamedShellProcessManager.Instance.TerminateShell(tempShellName);
                    return $"Error: Failed to promote shell to named shell '{promotedShellName}'";
                }
                
                // Return information about the promoted shell
                // Truncate the outputs separately before serializing
                var truncatedOutput = TextTruncationHelper.TruncateOutput(result.MergedOutput, maxCharsPerLine, maxTotalChars / 2);
                var truncatedError = TextTruncationHelper.TruncateOutput(result.Stderr, maxCharsPerLine, maxTotalChars / 2);
                
                return JsonSerializer.Serialize(new
                {
                    status = "stillRunning",
                    shellName = promotedShellName,
                    outputSoFar = truncatedOutput,
                    errorSoFar = truncatedError,
                    runningTime = expectedTimeout
                }, new JsonSerializerOptions { WriteIndented = true });
            }
            else
            {
                // Command completed within timeout, terminate the temporary shell
                NamedShellProcessManager.Instance.TerminateShell(tempShellName);
                
                // Return the truncated command output
                return TextTruncationHelper.TruncateOutput(result.ToAiString(), maxCharsPerLine, maxTotalChars);
            }
        }
        catch (Exception ex)
        {
            return $"Error executing command: {ex.Message}";
        }
    }

    [Description("Creates a named shell that persists across commands. Useful for workflows requiring multiple related commands that share state, environment variables, or working directory.")]
    public string CreateNamedShell(
        [Description("Shell type: bash, cmd, powershell")] string shellType = "bash",
        [Description("Optional custom name for the shell")] string shellName = "",
        [Description("Working directory for the shell")] string workingDirectory = "",
        [Description("Environment variables as JSON object")] string environmentVariables = "")
    {
        // Parse shell type
        PersistentShellType parsedShellType;
        switch (shellType.ToLowerInvariant())
        {
            case "bash":
                parsedShellType = PersistentShellType.Bash;
                break;
            case "cmd":
                parsedShellType = PersistentShellType.Cmd;
                break;
            case "powershell":
                parsedShellType = PersistentShellType.PowerShell;
                break;
            default:
                return $"Error: Unknown shell type '{shellType}'. Valid values are: bash, cmd, powershell";
        }

        // Parse environment variables if provided
        Dictionary<string, string>? envVars = null;
        if (!string.IsNullOrEmpty(environmentVariables))
        {
            try
            {
                envVars = JsonSerializer.Deserialize<Dictionary<string, string>>(environmentVariables);
            }
            catch (Exception ex)
            {
                return $"Error: Failed to parse environment variables: {ex.Message}";
            }
        }

        try
        {
            var result = NamedShellProcessManager.Instance.CreateShell(parsedShellType, shellName, workingDirectory, envVars);
            return result.ToAiString();
        }
        catch (Exception ex)
        {
            return $"Error creating shell: {ex.Message}";
        }
    }

    [Description("Starts a background process that runs independently. Ideal for servers, watchers, or other long-running processes that need to operate in the background while you perform other tasks.")]
    public string StartNamedProcess(
        [Description("The executable to run")] string executablePath,
        [Description("Arguments to pass to the process")] string processArguments = "",
        [Description("Optional custom name for the process")] string processName = "",
        [Description("Working directory for the process")] string workingDirectory = "",
        [Description("Environment variables as JSON object")] string environmentVariables = "")
    {
        if (string.IsNullOrEmpty(executablePath))
        {
            return "Error: Executable path cannot be empty";
        }

        // Parse environment variables if provided
        Dictionary<string, string>? envVars = null;
        if (!string.IsNullOrEmpty(environmentVariables))
        {
            try
            {
                envVars = JsonSerializer.Deserialize<Dictionary<string, string>>(environmentVariables);
            }
            catch (Exception ex)
            {
                return $"Error: Failed to parse environment variables: {ex.Message}";
            }
        }

        try
        {
            var result = NamedProcessManager.StartNamedProcess(
                executablePath,
                processName,
                processArguments,
                workingDirectory,
                envVars);

            return result.ToAiString();
        }
        catch (Exception ex)
        {
            return $"Error starting process: {ex.Message}";
        }
    }

    [Description("Executes a command in an existing named shell. Use this to run commands in shells created with CreateNamedShell or shells that were automatically created when a RunShellCommand exceeded its timeout.")]
    public async Task<string> ExecuteInShell(
        [Description("Shell name from CreateNamedShell or auto-created shell")] string shellName,
        [Description("Command to execute in the shell")] string command,
        [Description("Timeout in milliseconds")] int timeoutMs = 60000,
        [Description("Maximum number of characters to display per line.")] int maxCharsPerLine = 500,
        [Description("Maximum total number of characters to display.")] int maxTotalChars = 100000)
    {
        if (string.IsNullOrEmpty(shellName))
        {
            return "Error: Shell name cannot be empty";
        }

        if (string.IsNullOrEmpty(command))
        {
            return "Error: Command cannot be empty";
        }

        try
        {
            var result = await NamedShellProcessManager.Instance.ExecuteInShellAsync(shellName, command, timeoutMs);
            return TextTruncationHelper.TruncateOutput(result.ToAiString(), maxCharsPerLine, maxTotalChars);
        }
        catch (Exception ex)
        {
            return $"Error executing command in shell '{shellName}': {ex.Message}";
        }
    }

    [Description("Gets output from a shell or process. Provides options to retrieve specific output types, clear buffers, and wait for new output or patterns.")]
    public async Task<string> GetShellOrProcessOutput(
        [Description("Shell or process name")] string name,
        [Description("Output type: stdout, stderr, or all")] string outputType = "all",
        [Description("Whether to clear the output buffer after retrieval")] bool clearBuffer = false,
        [Description("Time to wait for new output in milliseconds (0 = don't wait)")] int waitTimeMs = 0,
        [Description("Pattern to wait for in the output (null = don't wait for pattern)")] string waitPattern = "",
        [Description("Maximum number of characters to display per line.")] int maxCharsPerLine = 500,
        [Description("Maximum total number of characters to display.")] int maxTotalChars = 100000)
    {
        if (string.IsNullOrEmpty(name))
        {
            return "Error: Shell or process name cannot be empty";
        }

        try
        {
            // Delegate to the centralized dispatcher to avoid duplication 
            // This prevents the bug where new shells were created when trying to get output
            var output = await NamedProcessDispatcher.GetOutputAsync(name, outputType, clearBuffer, waitTimeMs, waitPattern);
            return TextTruncationHelper.TruncateOutput(output, maxCharsPerLine, maxTotalChars);
        }
        catch (Exception ex)
        {
            return $"Error getting output: {ex.Message}";
        }
    }

    [Description("Sends input to a shell or process. Useful for interactive applications that require user input during execution.")]
    public string SendInputToShellOrProcess(
        [Description("Shell or process name")] string name,
        [Description("Text to send as input")] string inputText)
    {
        if (string.IsNullOrEmpty(name))
        {
            return "Error: Shell or process name cannot be empty";
        }

        // Input text will never be null since the parameter is defined as string (not string?)
        // but we'll keep this check for robustness
        if (string.IsNullOrEmpty(inputText))
        {
            inputText = string.Empty;
        }

        try
        {
            // Check if this is a shell
            var shell = NamedShellProcessManager.Instance.GetShell(name);
            
            if (shell != null)
            {
                // This is a shell
                bool success = NamedShellProcessManager.Instance.SendInputToShellAsync(name, inputText).Result;
                return success 
                    ? $"Input sent to shell '{name}'"
                    : $"Failed to send input to shell '{name}'";
            }
            else
            {
                // Check if it's a process
                if (NamedProcessManager.IsProcessRunning(name))
                {
                    bool success = NamedProcessManager.SendInputToProcess(name, inputText);
                    return success 
                        ? $"Input sent to process '{name}'"
                        : $"Failed to send input to process '{name}'";
                }
                else
                {
                    return $"Error: No shell or process found with name '{name}'";
                }
            }
        }
        catch (Exception ex)
        {
            return $"Error sending input: {ex.Message}";
        }
    }

    [Description("Waits for a shell or process to exit. Useful for ensuring that a process has completed before proceeding with subsequent operations.")]
    public async Task<string> WaitForShellOrProcessExit(
        [Description("Shell or process name")] string name,
        [Description("Timeout in milliseconds, -1 for indefinite")] int timeoutMs = -1)
    {
        if (string.IsNullOrEmpty(name))
        {
            return "Error: Shell or process name cannot be empty";
        }

        try
        {
            var startTime = DateTime.Now;
            
            // Check if this is a shell
            var shell = NamedShellProcessManager.Instance.GetShell(name);
            
            if (shell != null)
            {
                // This is a shell - wait for it to exit
                if (timeoutMs < 0)
                {
                    // Indefinite wait
                    while (true)
                    {
                        if (shell.State == ShellState.Terminated)
                        {
                            var duration = DateTime.Now - startTime;
                            return $"Shell '{name}' exited after {duration.TotalSeconds:F1} seconds";
                        }
                        
                        await Task.Delay(100);
                    }
                }
                else
                {
                    // Timed wait
                    var timeout = TimeSpan.FromMilliseconds(timeoutMs);
                    var endTime = startTime + timeout;
                    
                    while (DateTime.Now < endTime)
                    {
                        if (shell.State == ShellState.Terminated)
                        {
                            var duration = DateTime.Now - startTime;
                            return $"Shell '{name}' exited after {duration.TotalSeconds:F1} seconds";
                        }
                        
                        await Task.Delay(100);
                    }
                    
                    return $"Timeout waiting for shell '{name}' to exit after {timeout.TotalSeconds:F1} seconds";
                }
            }
            else
            {
                // Check if it's a process
                if (NamedProcessManager.IsProcessRunning(name))
                {
                    // Wait for process to exit
                    if (timeoutMs < 0)
                    {
                        // Indefinite wait
                        while (true)
                        {
                            if (!NamedProcessManager.IsProcessRunning(name))
                            {
                                var duration = DateTime.Now - startTime;
                                return $"Process '{name}' exited after {duration.TotalSeconds:F1} seconds";
                            }
                            
                            await Task.Delay(100);
                        }
                    }
                    else
                    {
                        // Timed wait
                        var timeout = TimeSpan.FromMilliseconds(timeoutMs);
                        var endTime = startTime + timeout;
                        
                        while (DateTime.Now < endTime)
                        {
                            if (!NamedProcessManager.IsProcessRunning(name))
                            {
                                var duration = DateTime.Now - startTime;
                                return $"Process '{name}' exited after {duration.TotalSeconds:F1} seconds";
                            }
                            
                            await Task.Delay(100);
                        }
                        
                        return $"Timeout waiting for process '{name}' to exit after {timeout.TotalSeconds:F1} seconds";
                    }
                }
                else
                {
                    return $"No shell or process found with name '{name}', or it has already exited";
                }
            }
        }
        catch (Exception ex)
        {
            return $"Error waiting for exit: {ex.Message}";
        }
    }

    [Description("Waits for output matching a pattern from a shell or process. Useful for detecting when a specific condition or state has been reached in a running application.")]
    public async Task<string> WaitForShellOrProcessOutput(
        [Description("Shell or process name")] string name,
        [Description("Regular expression pattern to wait for")] string waitPattern,
        [Description("Timeout in milliseconds, -1 for indefinite")] int timeoutMs = -1,
        [Description("Maximum number of characters to display per line.")] int maxCharsPerLine = 500,
        [Description("Maximum total number of characters to display.")] int maxTotalChars = 100000)
    {
        if (string.IsNullOrEmpty(name))
        {
            return "Error: Shell or process name cannot be empty";
        }

        if (string.IsNullOrEmpty(waitPattern))
        {
            return "Error: Pattern cannot be empty";
        }

        try
        {
            // Check if this is a shell
            var shell = NamedShellProcessManager.Instance.GetShell(name);
            
            if (shell != null)
            {
                // This is a shell - wait for pattern
                string result = await NamedShellProcessManager.Instance.WaitForShellOutputAsync(
                    name, waitPattern, timeoutMs);
                
                if (result == null)
                {
                    return $"Timeout waiting for pattern '{waitPattern}' in shell '{name}'";
                }
                
                return TextTruncationHelper.TruncateOutput($"Pattern matched: \"{result}\"", maxCharsPerLine, maxTotalChars);
            }
            else
            {
                // Check if it's a process
                if (NamedProcessManager.IsProcessRunning(name))
                {
                    // Wait for pattern in process output
                    bool matched = NamedProcessManager.WaitForProcessOutput(
                        name, waitPattern, timeoutMs, out string matchedOutput);
                    
                    if (!matched)
                    {
                        return $"Timeout waiting for pattern '{waitPattern}' in process '{name}'";
                    }
                    
                    return TextTruncationHelper.TruncateOutput($"Pattern matched: \"{matchedOutput}\"", maxCharsPerLine, maxTotalChars);
                }
                else
                {
                    return $"Error: No shell or process found with name '{name}'";
                }
            }
        }
        catch (Exception ex)
        {
            return $"Error waiting for output pattern: {ex.Message}";
        }
    }

    [Description("Terminates a shell or process. Use this to clean up resources when they're no longer needed or to stop processes that have become unresponsive.")]
    public string TerminateShellOrProcess(
        [Description("Shell or process name")] string name,
        [Description("Whether to force kill if graceful termination fails")] bool force = false,
        [Description("Maximum number of characters to display per line.")] int maxCharsPerLine = 500,
        [Description("Maximum total number of characters to display.")] int maxTotalChars = 100000)
    {
        if (string.IsNullOrEmpty(name))
        {
            return "Error: Shell or process name cannot be empty";
        }

        try
        {
            // Check if this is a shell
            var shell = NamedShellProcessManager.Instance.GetShell(name);
            
            if (shell != null)
            {
                // This is a shell - terminate it
                var result = NamedShellProcessManager.Instance.TerminateShell(name, force);
                return TextTruncationHelper.TruncateOutput(result.ToAiString(), maxCharsPerLine, maxTotalChars);
            }
            else
            {
                // Check if it's a process
                if (NamedProcessManager.IsProcessRunning(name))
                {
                    var result = NamedProcessManager.TerminateProcess(name, force);
                    return TextTruncationHelper.TruncateOutput(result.ToAiString(), maxCharsPerLine, maxTotalChars);
                }
                else
                {
                    return $"No shell or process found with name '{name}', or it has already terminated";
                }
            }
        }
        catch (Exception ex)
        {
            return $"Error terminating shell or process: {ex.Message}";
        }
    }

    [Description("Lists all running shells and processes. Helps you keep track of what's running and allows you to manage multiple concurrent operations.")]
    public string ListShellsAndProcesses(
        [Description("Maximum number of characters to display per line.")] int maxCharsPerLine = 500,
        [Description("Maximum total number of characters to display.")] int maxTotalChars = 100000)
    {
        try
        {
            var shellNames = NamedShellProcessManager.Instance.GetAllShellNames();
            var processNames = NamedProcessManager.GetAllProcessNames();
            
            var result = $"Running shells and processes: {shellNames.Count + processNames.Count}\n\n";
            
            // Add shells
            foreach (var shellName in shellNames)
            {
                var shellInfo = NamedShellProcessManager.Instance.GetShellInfo(shellName);
                result += $"ID: {shellName}\n";
                result += $"Type: Shell ({shellInfo["Type"]})\n";
                result += $"State: {shellInfo["State"]}\n";
                result += $"Created: {shellInfo["CreatedAt"]}\n";
                result += $"Last Activity: {shellInfo["LastActivity"]}\n";
                
                if (shellInfo.ContainsKey("CurrentCommand"))
                {
                    result += $"Current Command: {shellInfo["CurrentCommand"]}\n";
                    result += $"Command Running Time: {shellInfo["CommandRunningTime"]}\n";
                }
                
                if (shellInfo.ContainsKey("MemoryUsage"))
                {
                    result += $"Memory Usage: {shellInfo["MemoryUsage"]}\n";
                }
                
                result += "----------------------------------------\n";
            }
            
            // Add processes
            foreach (var processInfo in NamedProcessManager.GetAllProcessInfo())
            {
                result += $"ID: {processInfo["Name"]}\n";
                result += $"Type: Process\n";
                result += $"State: {(processInfo["IsRunning"] == "True" ? "Running" : "Terminated")}\n";
                result += $"Started: {processInfo["StartTime"]}\n";
                result += $"Running Time: {processInfo["RunningTime"]}\n";
                
                if (processInfo.ContainsKey("MemoryUsage"))
                {
                    result += $"Memory Usage: {processInfo["MemoryUsage"]}\n";
                }
                
                result += "----------------------------------------\n";
            }
            
            return TextTruncationHelper.TruncateOutput(result, maxCharsPerLine, maxTotalChars);
        }
        catch (Exception ex)
        {
            return $"Error listing shells and processes: {ex.Message}";
        }
    }
}