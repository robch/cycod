using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

/// <summary>
/// Provides helper functions for managing long-running background processes.
/// </summary>
public class BackgroundProcessHelperFunctions
{
    // Regex pattern to detect shell-specific constructs - only checking for &&
    private static readonly Regex ShellConstructPattern = new Regex(@"&&");

    [Description("Starts a long-running process in the background and returns a cookie that can be used to manage it. " +
                 "Useful for starting web servers, watchers, or other processes that need to run continuously while you continue interacting. " +
                 "Unlike direct command execution, background processes don't block further commands. " +
                 "Note: The returned cookie is only valid for use with related process management functions and is not a system process ID.")]
    public string StartLongRunningProcess(
        [Description("The name of the executable or process to run")] string processName,
        [Description("Optional arguments to pass to the process")] string processArguments = "",
        [Description("Optional working directory for the process")] string? workingDirectory = null)
    {
        try
        {
            // Validate that the user isn't trying to use shell constructs
            if (string.IsNullOrEmpty(processName))
            {
                return "Error: Process name cannot be empty";
            }

            if (ContainsShellConstructs(processName) || ContainsShellConstructs(processArguments))
            {
                return "Error: The command contains '&&' which is a shell operator. " +
                       "This function executes processes directly without a shell. " +
                       "Please provide just the executable name and arguments without shell operators.";
            }

            string handle = BackgroundProcessManager.StartLongRunningProcess(processName, processArguments, workingDirectory);
            return $"Process started with cookie: {handle}";
        }
        catch (Exception ex)
        {
            return $"Error starting process: {ex.Message}";
        }
    }

    [Description("Gets the accumulated output from a background process without stopping it. " +
             "Use this to check progress of a background operation while it continues to run.")]
    public string GetLongRunningProcessOutput(
        [Description("Process cookie obtained from StartLongRunningProcess (not a system process ID)")] string processHandle,
        [Description("Whether to clear the output buffer after retrieving")] bool clearBuffer = false)
    {
        try
        {
            if (string.IsNullOrEmpty(processHandle))
            {
                return "Error: Process cookie cannot be empty";
            }

            var output = BackgroundProcessManager.GetLongRunningProcessOutput(processHandle, clearBuffer);
            bool isRunning = BackgroundProcessManager.IsLongRunningProcessRunning(processHandle);

            var sb = new StringBuilder();
            sb.AppendLine($"Process with cookie {processHandle} status: {(isRunning ? "Running" : "Terminated")}");
            sb.AppendLine("--- Standard Output ---");
            sb.AppendLine(output["stdout"]);
            
            if (!string.IsNullOrEmpty(output["stderr"]))
            {
                sb.AppendLine("--- Standard Error ---");
                sb.AppendLine(output["stderr"]);
            }
            
            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"Error getting process output for cookie: {ex.Message}";
        }
    }

    [Description("Checks if a background process is still running. " +
             "Use this to monitor the status of background operations before taking further actions.")]
    public bool IsLongRunningProcessRunning(
        [Description("Process cookie obtained from StartLongRunningProcess (not a system process ID)")] string processHandle)
    {
        try
        {
            if (string.IsNullOrEmpty(processHandle))
            {
                return false;
            }

            return BackgroundProcessManager.IsLongRunningProcessRunning(processHandle);
        }
        catch (Exception)
        {
            return false;
        }
    }

    [Description("Terminates a running background process. " +
             "Use this to clean up background operations when they're no longer needed.")]
    public string KillLongRunningProcess(
        [Description("Process cookie obtained from StartLongRunningProcess (not a system process ID)")] string processHandle,
        [Description("Whether to force kill the process if it doesn't terminate gracefully")] bool force = false)
    {
        try
        {
            if (string.IsNullOrEmpty(processHandle))
            {
                return "Error: Process cookie cannot be empty";
            }

            bool wasRunning = BackgroundProcessManager.IsLongRunningProcessRunning(processHandle);
            bool success = BackgroundProcessManager.KillLongRunningProcess(processHandle, force);

            if (success)
            {
                return wasRunning 
                    ? $"Process with cookie {processHandle} was successfully terminated" 
                    : $"Process with cookie {processHandle} was already terminated";
            }
            else
            {
                return $"Failed to terminate process with cookie {processHandle}";
            }
        }
        catch (Exception ex)
        {
            return $"Error killing process with cookie: {ex.Message}";
        }
    }

    [Description("Lists all currently running background processes. " +
             "Helps you keep track of what's running and allows you to manage multiple concurrent operations.")]
    public string ListLongRunningProcesses()
    {
        try
        {
            var processes = BackgroundProcessManager.GetAllProcesses();
            
            if (processes.Count == 0)
            {
                return "No background processes are running.";
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Running background processes: {processes.Count(p => p.IsRunning)}/{processes.Count}");
            sb.AppendLine();
            
            foreach (var process in processes)
            {
                sb.AppendLine($"Cookie: {process.Handle}");
                sb.AppendLine($"Started: {process.StartTime}");
                sb.AppendLine($"Status: {(process.IsRunning ? "Running" : "Terminated")}");
                sb.AppendLine(new string('-', 40));
            }
            
            return sb.ToString();
        }
        catch (Exception ex)
        {
            return $"Error listing processes: {ex.Message}";
        }
    }

    /// <summary>
    /// Checks if a string contains shell constructs like &&, |, ;, etc.
    /// </summary>
    /// <param name="input">The string to check.</param>
    /// <returns>True if shell constructs are found, false otherwise.</returns>
    private bool ContainsShellConstructs(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return false;
        }

        return ShellConstructPattern.IsMatch(input);
    }
}