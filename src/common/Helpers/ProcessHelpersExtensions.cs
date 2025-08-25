using System;
using System.Collections.Generic;
using System.Threading.Tasks;

/// <summary>
/// Extension methods for the ProcessHelpers class.
/// </summary>
public static partial class ProcessHelpers
{
    /// <summary>
    /// Generic argument escaping based on the platform.
    /// </summary>
    public static string EscapeArgument(string argument)
    {
        if (string.IsNullOrEmpty(argument))
        {
            return argument;
        }
        
        // Cache common arguments to avoid repeated escaping
        if (_escapedArgumentsCache.TryGetValue(argument, out var cachedResult))
        {
            return cachedResult;
        }
        
        string result;
        if (OperatingSystem.IsWindows())
        {
            result = EscapeProcessArgument(argument);
        }
        else
        {
            result = EscapeBashArgument(argument);
        }
        
        // Cache the result if the cache isn't too big
        if (_escapedArgumentsCache.Count < 100)
        {
            _escapedArgumentsCache[argument] = result;
        }
        
        return result;
    }
    
    // Cache for escaped arguments
    private static readonly Dictionary<string, string> _escapedArgumentsCache = new();

    /// <summary>
    /// Executes a process and returns the exit code.
    /// </summary>
    public static int ExecuteProcess(string fileName, string arguments, string? workingDirectory = null, int timeout = 60000, Dictionary<string, string>? environment = null)
    {
        var result = RunProcess($"{fileName} {arguments}", workingDirectory, environment, null, timeout);
        return result.ExitCode;
    }

    /// <summary>
    /// Executes a process and returns the exit code, stdout, and stderr.
    /// </summary>
    public static (int exitCode, string stdout, string stderr) ExecuteProcessWithOutput(string fileName, string arguments, string? workingDirectory = null, int timeout = 60000, Dictionary<string, string>? environment = null)
    {
        var result = RunProcess($"{fileName} {arguments}", workingDirectory, environment, null, timeout);
        return (result.ExitCode, result.StandardOutput, result.StandardError);
    }

    /// <summary>
    /// Executes a shell command and returns the exit code.
    /// </summary>
    public static int ExecuteShell(string shell, string command, string? workingDirectory = null, int timeout = 60000, Dictionary<string, string>? environment = null)
    {
        var result = RunShellScript(shell, command, null, workingDirectory, environment, null, timeout);
        return result.ExitCode;
    }

    /// <summary>
    /// Executes a shell command and returns the exit code, stdout, and stderr.
    /// </summary>
    public static (int exitCode, string stdout, string stderr) ExecuteShellWithOutput(string shell, string command, string? workingDirectory = null, int timeout = 60000, Dictionary<string, string>? environment = null)
    {
        var result = RunShellScript(shell, command, null, workingDirectory, environment, null, timeout);
        return (result.ExitCode, result.StandardOutput, result.StandardError);
    }
}