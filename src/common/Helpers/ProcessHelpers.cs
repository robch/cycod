using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Text;


public static class ProcessHelpers
{
    public static RunnableProcessResult RunShellScript(string shell, string script, string? scriptArgs = null, string? workingDirectory = null, Dictionary<string, string>? envVars = null, string? input = null, int? timeout = null)
    {
        return RunShellScriptAsync(shell, script, scriptArgs, workingDirectory, envVars, input, timeout).GetAwaiter().GetResult();
    }

    public static async Task<RunnableProcessResult> RunShellScriptAsync(string shell, string script, string? scriptArgs = null, string? workingDirectory = null, Dictionary<string, string>? envVars = null, string? input = null, int? timeout = null)
    {
        var filesToDelete = new List<string>();

        try
        {
            // Log shell script execution at Info level
            Logger.Info($"Executing {shell} shell script {(scriptArgs != null ? $"with args: {scriptArgs}" : "")}");
            ConsoleHelpers.WriteDebugLine($"Script content length: {script?.Length ?? 0} characters");
            
            var scriptFileExt = GetShellScriptFileExtension(shell);
            var scriptWrapped = WrapScriptContent(shell, script);
            var scriptFileName = FileHelpers.WriteTextToTempFile(scriptWrapped, scriptFileExt)!;
            filesToDelete.Add(scriptFileName);
            ConsoleHelpers.WriteDebugLine($"Created temporary script file: {scriptFileName}");

            GetShellProcessNameAndArgsFormat(shell, out var processName, out var processArgsFormat);
            ConsoleHelpers.WriteDebugLine($"RunShellScriptAsync: {processName} {processArgsFormat}");
            var shellArgsFormatted = string.Format(processArgsFormat.Trim(), scriptFileName, scriptArgs).TrimEnd();
            ConsoleHelpers.WriteDebugLine($"RunShellScriptAsync: {processName} {shellArgsFormatted}");

            var hasScriptFileName = shellArgsFormatted.Contains(scriptFileName);
            if (!hasScriptFileName) 
            {
                var error = $"Script file name not found in shell arguments: {shellArgsFormatted}";
                Logger.Error(error);
                throw new InvalidOperationException(error);
            }

            var builder = new RunnableProcessBuilder()
                .WithFileName(processName)
                .WithArguments(shellArgsFormatted)
                .WithWorkingDirectory(workingDirectory)
                .WithEnvironmentVariables(envVars)
                .WithStandardInput(input)
                .WithTimeout(timeout);

            var startTime = DateTime.Now;
            var result = await builder.RunAsync();
            var duration = DateTime.Now - startTime;
            
            // Additional logging after execution
            if (result.ExitCode == 0)
            {
                Logger.Info($"Shell script executed successfully in {duration.TotalSeconds:F2}s with exit code: {result.ExitCode}");
            }
            else
            {
                Logger.Warning($"Shell script exited with non-zero code: {result.ExitCode} in {duration.TotalSeconds:F2}s");
                ConsoleHelpers.WriteDebugLine($"Script stderr: {result.StandardError}");
            }
            
            return result;
        }
        catch (Exception ex)
        {
            Logger.Error($"Shell script execution failed: {ex.Message}");
            throw;
        }
        finally
        {
            var skipDelete = ConsoleHelpers.IsDebug();
            if (!skipDelete)
            {
                filesToDelete?.ForEach(x => 
                {
                    try
                    {
                        File.Delete(x);
                        ConsoleHelpers.WriteDebugLine($"Deleted temporary script file: {x}");
                    }
                    catch (Exception ex)
                    {
                        Logger.Warning($"Failed to delete temporary script file {x}: {ex.Message}");
                    }
                });
            }
            else
            {
                ConsoleHelpers.WriteDebugLine($"Debug mode: Keeping temporary script files");
            }
        }
    }

    public static RunnableProcessResult RunProcess(string command, string? workingDirectory = null, Dictionary<string, string>? envVars = null, string? input = null, int? timeout = null)
    {
        return RunProcessAsync(command, workingDirectory, envVars, input, timeout).GetAwaiter().GetResult();
    }

    public static async Task<RunnableProcessResult> RunProcessAsync(string command, string? workingDirectory = null, Dictionary<string, string>? envVars = null, string? input = null, int? timeout = null)
    {
        var processBuilder = new RunnableProcessBuilder()
            .WithCommandLine(command!)
            .WithWorkingDirectory(workingDirectory)
            .WithEnvironmentVariables(envVars)
            .WithStandardInput(input)
            .WithTimeout(timeout);

        var processResult = await processBuilder.RunAsync();
        return processResult;
    }

    public static async Task<(string, int)> RunProcessAsync(string processName, string arguments, int timeout = int.MaxValue)
    {
        var startInfo = new ProcessStartInfo()
        {
            FileName = processName,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        var sbOut = new StringBuilder();
        var sbErr = new StringBuilder();
        var sbMerged = new StringBuilder();

        using var process = new Process { StartInfo = startInfo };
        process.Start();

        var outDoneSignal = new ManualResetEvent(false);
        var errDoneSignal = new ManualResetEvent(false);
        process.OutputDataReceived += (sender, e) => AppendLineOrSignal(e.Data, sbOut, sbMerged, outDoneSignal);
        process.ErrorDataReceived += (sender, e) => AppendLineOrSignal(e.Data, sbErr, sbMerged, errDoneSignal);
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        var exitedNotKilled = WaitForExit(process, timeout);
        if (exitedNotKilled)
        {
            await Task.Run(() => {
                outDoneSignal.WaitOne();
                errDoneSignal.WaitOne();
            });
        }

        return (sbMerged.ToString(), process.ExitCode);
    }

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

    public static IEnumerable<string> SplitArguments(string? arguments)
    {
        if (string.IsNullOrEmpty(arguments))
        {
            return Enumerable.Empty<string>();
        }

        var args = new List<string>();
        var currentArg = new StringBuilder();
        bool inQuotes = false;
        char quoteChar = '\0';

        foreach (var c in arguments)
        {
            if (c == '"' || c == '\'')
            {
                if (inQuotes && c == quoteChar)
                {
                    // Closing quote
                    inQuotes = false;
                    args.Add(currentArg.ToString());
                    currentArg.Clear();
                }
                else if (!inQuotes)
                {
                    // Opening quote
                    inQuotes = true;
                    quoteChar = c;
                }
            }
            else if (char.IsWhiteSpace(c) && !inQuotes)
            {
                if (currentArg.Length > 0)
                {
                    args.Add(currentArg.ToString());
                    currentArg.Clear();
                }
            }
            else
            {
                currentArg.Append(c);
            }
        }

        // Add the last argument if any
        if (currentArg.Length > 0)
        {
            args.Add(currentArg.ToString());
        }

        return args;

    }

    public static string FindBashExe()
    {
        var gitBash = FindAndCacheGitBashExe();
        if (gitBash == null || gitBash == "bash.exe")
        {
            throw new Exception("Could not find Git for Windows bash.exe in PATH!");
        }
        return gitBash;
    }

    public static string FindPwshExe()
    {
        var pwshExe = FindAndCachePwshExe();
        if (pwshExe == null || pwshExe == "pwsh.exe")
        {
            throw new Exception("Could not find PowerShell Core pwsh.exe in PATH!");
        }
        return pwshExe;
    }

    private static string FindAndCacheGitBashExe()
    {
        var bashExe = "bash.exe";
        if (_cliCache.ContainsKey(bashExe))
        {
            return _cliCache[bashExe];
        }

        var found = FindNoCacheGitBashExe();
        _cliCache[bashExe] = found;

        return found;
    }

    private static string FindNoCacheGitBashExe()
    {
        var found = FileHelpers.FindFilesInOsPath("bash.exe");
        return found.Where(x => x.ToLower().Contains("git")).FirstOrDefault() ?? "bash.exe";
    }

    private static string FindAndCachePwshExe()
    {
        var pwshExe = "pwsh.exe";
        if (_cliCache.ContainsKey(pwshExe))
        {
            return _cliCache[pwshExe];
        }

        var found = FindNoCachePwshExe();
        _cliCache[pwshExe] = found;

        return found;
    }

    private static string FindNoCachePwshExe()
    {
        var found = FileHelpers.FindFilesInOsPath(OS.IsWindows() ? "pwsh.exe" : "pwsh");
        var tryPowerShellExe = found.Count() == 0 && OS.IsWindows();
        if (tryPowerShellExe)
        {
            found = FileHelpers.FindFilesInOsPath("powershell.exe");
        }

        return found.FirstOrDefault() ?? (OS.IsWindows() ? "powershell.exe" : "pwsh");
    }

    private static void GetShellProcessNameAndArgsFormat(string shell, out string shellProcessName, out string shellArgsFormat)
    {
        SplitCommand(shell, out var shellCommand, out shellArgsFormat);
        shellProcessName = shellCommand switch
        {
            "cmd" => "cmd.exe",
            "bash" => OS.IsWindows() ? FindBashExe() : "bash",
            "pwsh" or "powershell" => FindPwshExe(),
            _ => shellCommand
        };
        var needShellArgsFormat = string.IsNullOrEmpty(shellArgsFormat);
        if (needShellArgsFormat) shellArgsFormat = GetShellProcessArgsFormat(shellCommand);

        var missingCurlyBracketZero = !shellArgsFormat.Contains("{0}");
        shellArgsFormat = missingCurlyBracketZero ? shellArgsFormat + " {0}" : shellArgsFormat;

        var missingCurlyBracketOne = !shellArgsFormat.Contains("{1}");
        shellArgsFormat = missingCurlyBracketOne ? shellArgsFormat + " {1}" : shellArgsFormat;
    }

    private static string GetShellProcessArgsFormat(string shellType)
    {
        if (_argsFormat.TryGetValue(shellType, out var argFormat))
        {
            return argFormat;
        }
        return "";
    }

    private static string GetShellScriptFileExtension(string shellType)
    {
        if (_scriptExtension.TryGetValue(shellType, out var extension))
        {
            return extension;
        }
        return "";
    }

    private static string WrapScriptContent(string shellType, string contents)
    {
        switch (shellType)
        {
            case "cmd":
                // Note, use @echo off instead of using the /Q command line switch.
                // When /Q is used, echo can't be turned on.
                contents = $"@echo off{Environment.NewLine}{contents}";
                break;
            case "powershell":
            case "pwsh":
                var prepend = "$ErrorActionPreference = 'stop'";
                var append = @"if ((Test-Path -LiteralPath variable:\LASTEXITCODE)) { exit $LASTEXITCODE }";
                contents = $"{prepend}{Environment.NewLine}{contents}{Environment.NewLine}{append}";
                break;
        }
        return contents;
    }

    private static void AppendLineOrSignal(string? text, StringBuilder sb1, StringBuilder sb2, ManualResetEvent signal)
    {
        if (text != null)
        {
            sb1.AppendLine(text);
            sb2.AppendLine(text);
        }
        else
        {
            signal.Set();
        }
    }

    private static bool WaitForExit(Process process, int timeout)
    {
        var completed = process.WaitForExit(timeout);
        if (!completed)
        {
            var name = process.ProcessName;
            var message = $"Timedout! Stopping process ({name})...";
            ConsoleHelpers.WriteDebugLine(message);

            try
            {
                message = $"Timedout! Sending <ctrl-c> ...";
                ConsoleHelpers.WriteDebugLine(message);

                process.StandardInput.WriteLine("\x3"); // try ctrl-c first
                process.StandardInput.Close();

                ConsoleHelpers.WriteDebugLine($"{message} Sent!");

                completed = process.WaitForExit(200);
            }
            catch (Exception ex)
            {
                ConsoleHelpers.WriteDebugLine($"Timedout! Failed to send <ctrl-c>: {ex.Message}");
            }

            message = "Timedout! Sent <ctrl-c>" + (completed ? "; stopped" : "; trying Kill()");
            ConsoleHelpers.WriteDebugLine(message);

            if (!completed)
            {
                message = $"Timedout! Killing process ({name})...";
                ConsoleHelpers.WriteDebugLine(message);

                process.Kill();

                message = process.HasExited ? $"{message} Done." : $"{message} Failed!";
                ConsoleHelpers.WriteDebugLine(message);
            }
        }

        return completed;
    }

    private static Dictionary<string, string> _cliCache = new Dictionary<string, string>();

    // Common executable and script extensions on Windows
    private static readonly string[] WindowsExecutableExtensions = { ".exe", ".cmd", ".bat", ".ps1" };

    /// <summary>
    /// Finds an executable in the PATH with common Windows extensions.
    /// </summary>
    /// <param name="baseName">The base name of the executable without extension (e.g., "npm")</param>
    /// <returns>The full path of the executable if found, or the original name if not found</returns>
    public static string FindExecutableInPath(string baseName)
    {
        if (string.IsNullOrEmpty(baseName))
        {
            return baseName;
        }

        // If we're not on Windows or the file already has an extension or exists, return it
        if (!OS.IsWindows() || Path.HasExtension(baseName) || File.Exists(baseName))
        {
            return baseName;
        }

        // Check for name in cache
        if (_cliCache.TryGetValue(baseName, out var cachedPath))
        {
            return cachedPath;
        }

        // Try to find with various extensions
        foreach (string ext in WindowsExecutableExtensions)
        {
            // Skip if already has this extension
            if (baseName.EndsWith(ext, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string nameWithExt = baseName + ext;
            
            // First check local path
            if (File.Exists(nameWithExt))
            {
                _cliCache[baseName] = nameWithExt;
                return nameWithExt;
            }

            // Then check in PATH
            var foundInPath = FileHelpers.FindFilesInOsPath(nameWithExt).FirstOrDefault();
            if (!string.IsNullOrEmpty(foundInPath))
            {
                _cliCache[baseName] = foundInPath;
                return foundInPath;
            }
        }

        // Special handling for common npm and node tools in predictable locations
        if (baseName.Equals("npm", StringComparison.OrdinalIgnoreCase) || 
            baseName.Equals("npx", StringComparison.OrdinalIgnoreCase) || 
            baseName.Equals("node", StringComparison.OrdinalIgnoreCase))
        {
            string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            string nodejsPath = Path.Combine(programFiles, "nodejs");
            
            foreach (string ext in WindowsExecutableExtensions)
            {
                string fullPath = Path.Combine(nodejsPath, baseName + ext);
                if (File.Exists(fullPath))
                {
                    _cliCache[baseName] = fullPath;
                    return fullPath;
                }
            }
        }

        // If not found, cache the original name to avoid repeated searches
        _cliCache[baseName] = baseName;
        return baseName;
    }

    private static readonly Dictionary<string, string> _argsFormat = new(StringComparer.OrdinalIgnoreCase)
    {
        ["cmd"] = "/D /E:ON /V:OFF /S /C \"CALL \"{0}\" {1}\"",
        ["pwsh"] = "-command \"{0}\" {1}",
        ["powershell"] = "-Command \"{0}\" {1}",
        ["bash"] = "--noprofile --norc -e -o pipefail {0} {1}",
        ["sh"] = "-e {0} {1}",
        ["python"] = "{0} {1}"
    };

    private static readonly Dictionary<string, string> _scriptExtension = new(StringComparer.OrdinalIgnoreCase)
    {
        ["cmd"] = ".cmd",
        ["pwsh"] = ".ps1",
        ["powershell"] = ".ps1",
        ["bash"] = ".sh",
        ["sh"] = ".sh",
        ["python"] = ".py"
    };
}

