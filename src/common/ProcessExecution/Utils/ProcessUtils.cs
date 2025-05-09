using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

/// <summary>
/// Utility methods for process preparation and argument handling.
/// </summary>
public static class ProcessUtils
{
    /// <summary>
    /// Escapes a command line argument for direct process execution (no shell).
    /// </summary>
    /// <param name="arg">The argument to escape.</param>
    /// <returns>The escaped argument.</returns>
    public static string EscapeProcessArgument(string arg)
    {
        if (string.IsNullOrEmpty(arg)) return arg;
        
        // Check if already quoted
        var alreadyDoubleQuoted = arg.StartsWith("\"") && arg.EndsWith("\"");
        if (alreadyDoubleQuoted) return arg;

        // If no special characters, return as is
        var noSpacesOrSpecialChars = !arg.Contains(" ") && !arg.Contains("\\") && !arg.Contains("\"");
        if (noSpacesOrSpecialChars) return arg;

        // Escape backslashes and quotes
        var escaped = arg.Replace("\\", "\\\\").Replace("\"", "\\\"");
        
        // Add quotes if needed
        var needsDoubleQuotes = escaped.Contains(" ") || escaped.Contains("\\") || escaped.Contains("\"");
        return needsDoubleQuotes ? $"\"{escaped}\"" : escaped;
    }
    
    /// <summary>
    /// Escapes a command or argument for Bash shell.
    /// </summary>
    /// <param name="arg">The argument to escape.</param>
    /// <param name="forStdin">Whether the argument is for stdin injection vs. command line.</param>
    /// <returns>The escaped argument.</returns>
    public static string EscapeBashArgument(string arg, bool forStdin = false)
    {
        if (string.IsNullOrEmpty(arg)) return arg;
        
        if (forStdin)
        {
            // For stdin injection, we need to escape single quotes differently
            return arg.Replace("'", "'\\''");
        }
        else
        {
            // For command line arguments, use single quotes
            bool needsQuoting = arg.Contains("'") || arg.Contains(" ") || arg.Contains("\"") || 
                                arg.Contains("\\") || arg.Contains(";") || arg.Contains("|") || 
                                arg.Contains("<") || arg.Contains(">") || arg.Contains("&");
                                
            if (needsQuoting)
            {
                // Escape single quotes with '\'' pattern
                return $"'{arg.Replace("'", "'\\''")}'";
            }
            
            return arg;
        }
    }
    
    /// <summary>
    /// Escapes a command or argument for CMD shell.
    /// </summary>
    /// <param name="arg">The argument to escape.</param>
    /// <param name="forStdin">Whether the argument is for stdin injection vs. command line.</param>
    /// <returns>The escaped argument.</returns>
    public static string EscapeCmdArgument(string arg, bool forStdin = false)
    {
        if (string.IsNullOrEmpty(arg)) return arg;
        
        // CMD has different escaping depending on whether we're using it directly or through stdin
        if (forStdin)
        {
            // For stdin, we need to escape special characters with ^
            return arg
                .Replace("^", "^^")
                .Replace("&", "^&")
                .Replace("|", "^|")
                .Replace("<", "^<")
                .Replace(">", "^>")
                .Replace("(", "^(")
                .Replace(")", "^)")
                .Replace("%", "^%");
        }
        else
        {
            // For command line, handle quotes differently
            bool needsQuotes = arg.Contains(" ") || arg.Contains("&") || arg.Contains("|") || 
                                arg.Contains("%") || arg.Contains("<") || arg.Contains(">");
            
            if (needsQuotes)
            {
                // Double up internal quotes
                arg = arg.Replace("\"", "\"\"");
                return $"\"{arg}\"";
            }
            
            return arg;
        }
    }
    
    /// <summary>
    /// Escapes a command or argument for PowerShell.
    /// </summary>
    /// <param name="arg">The argument to escape.</param>
    /// <param name="forStdin">Whether the argument is for stdin injection vs. command line.</param>
    /// <returns>The escaped argument.</returns>
    public static string EscapePowerShellArgument(string arg, bool forStdin = false)
    {
        if (string.IsNullOrEmpty(arg)) return arg;
        
        if (forStdin)
        {
            // For stdin injection, escape with backticks
            return arg
                .Replace("`", "``")
                .Replace("\"", "`\"")
                .Replace("$", "`$")
                .Replace("&", "`&")
                .Replace(";", "`;")
                .Replace("(", "`(")
                .Replace(")", "`)")
                .Replace("|", "`|");
        }
        else
        {
            // For command line, use double quotes with escaping
            bool needsQuotes = arg.Contains(" ") || arg.Contains("&") || arg.Contains(";") || 
                                arg.Contains("(") || arg.Contains(")") || arg.Contains("<") ||
                                arg.Contains(">") || arg.Contains("|") || arg.Contains("\"");
            
            if (needsQuotes)
            {
                // Escape double quotes with double quotes (PowerShell convention)
                arg = arg.Replace("\"", "\"\"");
                return $"\"{arg}\"";
            }
            
            return arg;
        }
    }
    
    /// <summary>
    /// Escapes an argument for the appropriate shell based on shell type.
    /// </summary>
    /// <param name="shellType">The type of shell.</param>
    /// <param name="arg">The argument to escape.</param>
    /// <param name="forStdin">Whether the argument is for stdin injection vs. command line.</param>
    /// <returns>The escaped argument.</returns>
    public static string EscapeShellArgument(ShellType shellType, string arg, bool forStdin = false)
    {
        switch (shellType)
        {
            case ShellType.Bash:
                return EscapeBashArgument(arg, forStdin);
            case ShellType.Cmd:
                return EscapeCmdArgument(arg, forStdin);
            case ShellType.PowerShell:
                return EscapePowerShellArgument(arg, forStdin);
            default:
                throw new ArgumentException($"Unsupported shell type: {shellType}");
        }
    }
    
    /// <summary>
    /// Gets the default shell type based on the current operating system.
    /// </summary>
    /// <returns>The default shell type for the current OS.</returns>
    public static ShellType GetDefaultShellType()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return ShellType.Cmd;
        }
        else
        {
            return ShellType.Bash;
        }
    }
    
    /// <summary>
    /// Gets the appropriate shell executable file name based on shell type and platform.
    /// </summary>
    /// <param name="shellType">The type of shell.</param>
    /// <returns>The shell executable file name.</returns>
    public static string GetShellExecutable(ShellType shellType)
    {
        switch (shellType)
        {
            case ShellType.Cmd:
                return "cmd.exe";

            case ShellType.Bash:
                return OS.IsWindows()
                    ? EnsureFindCacheGetBashExe()
                    : "bash";

            case ShellType.PowerShell:
                return OS.IsWindows()
                    ? EnsureFindCacheGetPwshExe()
                    : "pwsh";

            default:
                throw new ArgumentException($"Unsupported shell type: {shellType}");
        }
    }
    
    /// <summary>
    /// Gets standard arguments to start a shell in interactive mode.
    /// </summary>
    /// <param name="shellType">The type of shell.</param>
    /// <returns>The arguments to start the shell.</returns>
    public static string GetShellStartupArguments(ShellType shellType)
    {
        switch (shellType)
        {
            case ShellType.Cmd:
                return "/K chcp 65001"; // Keep session open, set UTF-8 encoding
                
            case ShellType.Bash:
                return "--norc --noprofile";

            case ShellType.PowerShell:
                return "-NoExit -Command -"; // Keep session open, accept commands from stdin
                
            default:
                throw new ArgumentException($"Unsupported shell type: {shellType}");
        }
    }
    
    /// <summary>
    /// Splits a command line into executable and arguments.
    /// </summary>
    /// <param name="commandLine">The full command line.</param>
    /// <param name="fileName">Output parameter for the executable file name.</param>
    /// <param name="arguments">Output parameter for the arguments.</param>
    public static void SplitCommand(string commandLine, out string fileName, out string arguments)
    {
        if (string.IsNullOrEmpty(commandLine))
        {
            fileName = string.Empty;
            arguments = string.Empty;
            return;
        }
        
        commandLine = commandLine.Trim();
        
        // Check if the command starts with a quoted path
        if (commandLine.StartsWith("\""))
        {
            int closingQuoteIndex = commandLine.IndexOf('"', 1);
            if (closingQuoteIndex > 1)
            {
                // Extract filename from quotes
                fileName = commandLine.Substring(1, closingQuoteIndex - 1);
                
                // Get the rest as arguments, if any
                arguments = closingQuoteIndex < commandLine.Length - 1 
                    ? commandLine.Substring(closingQuoteIndex + 1).TrimStart() 
                    : string.Empty;
                
                return;
            }
        }
        
        // No quotes or invalid quotes, split on first space
        int spaceIndex = commandLine.IndexOf(' ');
        
        if (spaceIndex > 0)
        {
            fileName = commandLine.Substring(0, spaceIndex);
            arguments = commandLine.Substring(spaceIndex + 1).TrimStart();
        }
        else
        {
            // No space, the whole string is the filename
            fileName = commandLine;
            arguments = string.Empty;
        }
    }
    
    /// <summary>
    /// Configures a ProcessStartInfo with common settings.
    /// </summary>
    /// <param name="fileName">The process file name.</param>
    /// <param name="arguments">The process arguments.</param>
    /// <param name="encoding">The encoding to use for standard streams.</param>
    /// <returns>A configured ProcessStartInfo.</returns>
    public static ProcessStartInfo ConfigureProcessStartInfo(
        string fileName,
        string arguments)
    {
        return new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = true,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };
    }
    
    /// <summary>
    /// Generates a unique marker string for command completion detection.
    /// </summary>
    /// <returns>A unique marker string.</returns>
    public static string GenerateMarker()
    {
        // Create a marker that's unlikely to appear in normal output
        return $"__COMMAND_COMPLETION_MARKER_{Guid.NewGuid():N}__";
    }
    
    /// <summary>
    /// Creates a temporary file with optional content.
    /// </summary>
    /// <param name="content">Optional content to write to the file.</param>
    /// <param name="extension">Optional extension for the file.</param>
    /// <returns>The path to the created temporary file.</returns>
    public static string CreateTemporaryFile(string content = "", string? extension = null)
    {
        var tempFilePath = System.IO.Path.GetTempFileName();
        
        if (!string.IsNullOrEmpty(extension))
        {
            var newPath = $"{tempFilePath}.{extension.TrimStart('.')}";
            System.IO.File.Move(tempFilePath, newPath);
            tempFilePath = newPath;
        }
        
        if (!string.IsNullOrEmpty(content))
        {
            System.IO.File.WriteAllText(tempFilePath, content);
        }
        
        return tempFilePath;
    }

    private static string EnsureFindCacheGetBashExe()
    {
        var gitBash = FindCacheGitBashExe();
        if (gitBash == null || gitBash == "bash.exe")
        {
            throw new Exception("Could not find Git for Windows bash.exe in PATH!");
        }
        return gitBash;
    }

    private static string FindCacheGitBashExe()
    {
        var bashExe = "bash.exe";
        if (_cliCache.ContainsKey(bashExe))
        {
            return _cliCache[bashExe];
        }

        var found = FindGitBashExe();
        _cliCache[bashExe] = found;

        return found;
    }

    private static string FindGitBashExe()
    {
        var found = FileHelpers.FindFilesInOsPath("bash.exe");
        return found.Where(x => x.ToLower().Contains("git")).FirstOrDefault() ?? "bash.exe";
    }

    private static string EnsureFindCacheGetPwshExe()
    {
        var pwshExe = FindCachePwshExe();
        if (pwshExe == null || pwshExe == "pwsh.exe")
        {
            throw new Exception("Could not find PowerShell Core pwsh.exe in PATH!");
        }
        return pwshExe;
    }

    private static string FindCachePwshExe()
    {
        var pwshExe = "pwsh.exe";
        if (_cliCache.ContainsKey(pwshExe))
        {
            return _cliCache[pwshExe];
        }

        var found = FindPwshExe();
        _cliCache[pwshExe] = found;

        return found;
    }

    private static string FindPwshExe()
    {
        var found = FileHelpers.FindFilesInOsPath("pwsh.exe");
        return found.FirstOrDefault() ?? "powershell.exe";
    }

    private static Dictionary<string, string> _cliCache = new Dictionary<string, string>();
}