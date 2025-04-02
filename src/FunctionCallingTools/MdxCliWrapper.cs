using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Wrapper class for executing MDX CLI commands and processing their output.
/// </summary>
public class MdxCliWrapper
{
    /// <summary>
    /// Executes an MDX command and returns the output.
    /// </summary>
    /// <param name="arguments">The arguments to pass to the MDX command</param>
    /// <param name="timeoutMs">Timeout in milliseconds</param>
    /// <returns>The command output as a string</returns>
    private async Task<string> ExecuteMdxCommandAsync(string arguments, int timeoutMs = 120000)
    {
        try
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "mdx",
                    Arguments = arguments,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    CreateNoWindow = true
                }
            };

            if (ConsoleHelpers.IsVerbose())
            {
                ConsoleHelpers.WriteLine($"Executing MDX command: {process.StartInfo.FileName} {process.StartInfo.Arguments}");
            }

            var stdoutBuffer = new StringBuilder();
            var stderrBuffer = new StringBuilder();
            var mergedBuffer = new StringBuilder();

            process.OutputDataReceived += (sender, e) => {
                if (e.Data != null)
                {
                    var line = e.Data.TrimEnd(new char[] { '\r', '\n' });
                    if (ConsoleHelpers.IsVerbose())
                    {
                        ConsoleHelpers.WriteLine(line, ConsoleColor.DarkMagenta);
                    }
                    stdoutBuffer.AppendLine(line);
                    mergedBuffer.AppendLine(line);
                }
            };

            process.ErrorDataReceived += (sender, e) => {
                if (e.Data != null)
                {
                    var line = e.Data.TrimEnd(new char[] { '\r', '\n' });
                    if (ConsoleHelpers.IsVerbose())
                    {
                        ConsoleHelpers.WriteErrorLine(line);
                    }
                    stderrBuffer.AppendLine(e.Data);
                    mergedBuffer.AppendLine(e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            using (var cts = new CancellationTokenSource())
            {
                var timeoutTask = Task.Delay(timeoutMs, cts.Token);
                var processExitTask = Task.Run(() => process.WaitForExit());
                
                var completedTask = await Task.WhenAny(processExitTask, timeoutTask);
                if (completedTask == timeoutTask)
                {
                    try { process.Kill(); } catch { }
                    return $"<waiting {timeoutMs}ms for mdx to finish, timed out>\n{mergedBuffer}";
                }
                
                cts.Cancel(); // Cancel the timeout task
                process.WaitForExit();
                
                var output = mergedBuffer.ToString().TrimEnd();
                if (process.ExitCode != 0)
                {
                    output += Environment.NewLine + $"<mdx command exited with code {process.ExitCode}>";
                }
                
                return output;
            }
        }
        catch (Exception ex)
        {
            return $"<mdx command exited with error: {ex.Message}>";
        }
    }

    #region Search Codebase Methods
    
    /// <summary>
    /// Executes MDX command to search codebase for specific patterns.
    /// </summary>
    public async Task<string> ExecuteSearchCodebaseCommand(
        string[] filePatterns,
        string contentPattern,
        string[]? excludePatterns = null,
        bool showLineNumbers = true,
        int? contextLines = null,
        string? processingInstructions = null)
    {
        var arguments = BuildSearchCodebaseArguments(
            filePatterns, 
            contentPattern, 
            excludePatterns, 
            showLineNumbers, 
            contextLines, 
            processingInstructions);
            
        var output = await ExecuteMdxCommandAsync(arguments);
        var noFilesFound = output.Contains("No files matched criteria") || output.Contains("No files found");
        var wasntRecursive = !contentPattern.Contains("**");
        if (noFilesFound && wasntRecursive)
        {
            output = $"{output}\n\n<You may want to try using '**' in your content pattern to search recursively.>";
        }

        return output;
    }
    
    /// <summary>
    /// Builds command arguments for searching codebase.
    /// </summary>
    private string BuildSearchCodebaseArguments(
        string[] filePatterns,
        string contentPattern,
        string[]? excludePatterns = null,
        bool showLineNumbers = true,
        int? contextLines = null,
        string? processingInstructions = null)
    {
        var sb = new StringBuilder();
        
        // Add file patterns
        foreach (var pattern in filePatterns)
        {
            sb.Append($"{EscapeArgument(pattern)} ");
        }
        
        // Add exclude patterns
        if (excludePatterns != null && excludePatterns.Length > 0)
        {
            sb.Append("--exclude ");
            foreach (var pattern in excludePatterns)
            {
                sb.Append($"{EscapeArgument(pattern)} ");
            }
        }
        
        // Add content pattern for searching within files
        if (!string.IsNullOrEmpty(contentPattern))
        {
            sb.Append($"--contains {EscapeArgument(contentPattern)} ");
        }
        
        // Add line numbers option
        if (showLineNumbers)
        {
            sb.Append("--line-numbers ");
        }
        
        // Add context lines
        if (contextLines.HasValue)
        {
            sb.Append($"--lines {contextLines.Value} ");
        }
        
        // Add processing instructions
        if (!string.IsNullOrEmpty(processingInstructions))
        {
            sb.Append($"--instructions {EscapeArgument(processingInstructions)} ");
        }
        
        return sb.ToString().Trim();
    }
    
    /// <summary>
    /// Executes MDX command to find files containing specific patterns.
    /// </summary>
    public async Task<string> ExecuteFindFilesContainingPatternCommand(
        string[] filePatterns,
        string contentPattern,
        string[]? excludePatterns = null,
        string? processingInstructions = null)
    {
        var arguments = BuildFindFilesContainingPatternArguments(
            filePatterns, 
            contentPattern, 
            excludePatterns,
            processingInstructions);
            
        var output = await ExecuteMdxCommandAsync(arguments);
        var noFilesFound = output.Contains("No files matched criteria") || output.Contains("No files found");
        var wasntRecursive = !contentPattern.Contains("**");
        if (noFilesFound && wasntRecursive)
        {
            output = $"{output}\n\n<You may want to try using '**' in your content pattern to search recursively.>";
        }

        return output;
    }
    
    /// <summary>
    /// Builds command arguments for finding files containing patterns.
    /// </summary>
    private string BuildFindFilesContainingPatternArguments(
        string[] filePatterns,
        string contentPattern,
        string[]? excludePatterns = null,
        string? processingInstructions = null)
    {
        var sb = new StringBuilder();
        
        // Add file patterns
        foreach (var pattern in filePatterns)
        {
            sb.Append($"{EscapeArgument(pattern)} ");
        }
        
        // Add exclude patterns
        if (excludePatterns != null && excludePatterns.Length > 0)
        {
            sb.Append("--exclude ");
            foreach (var pattern in excludePatterns)
            {
                sb.Append($"{EscapeArgument(pattern)} ");
            }
        }
        
        // Add processing instructions
        if (!string.IsNullOrEmpty(processingInstructions))
        {
            sb.Append($"--instructions {EscapeArgument(processingInstructions)} ");
        }
        
        return sb.ToString().Trim();
    }
    
    #endregion
    
    #region Documentation Generation Methods
    
    /// <summary>
    /// Executes MDX command to convert files to markdown.
    /// </summary>
    public async Task<string> ExecuteConvertFilesToMarkdownCommand(
        string[] filePaths,
        string? formattingInstructions = null,
        bool showLineNumbers = false)
    {
        var arguments = BuildConvertFilesToMarkdownArguments(
            filePaths, 
            formattingInstructions, 
            showLineNumbers);
            
        return await ExecuteMdxCommandAsync(arguments);
    }
    
    /// <summary>
    /// Builds command arguments for converting files to markdown.
    /// </summary>
    private string BuildConvertFilesToMarkdownArguments(
        string[] filePaths,
        string? formattingInstructions = null,
        bool showLineNumbers = false)
    {
        var sb = new StringBuilder();
        
        // Add file paths
        foreach (var path in filePaths)
        {
            sb.Append($"{EscapeArgument(path)} ");
        }
        
        // Add line numbers option
        if (showLineNumbers)
        {
            sb.Append("--line-numbers ");
        }
        
        // Add formatting instructions
        if (!string.IsNullOrEmpty(formattingInstructions))
        {
            sb.Append($"--instructions {EscapeArgument(formattingInstructions)} ");
        }
        
        return sb.ToString().Trim();
    }
    
    #endregion
    
    #region Web Research Methods
    
    /// <summary>
    /// Executes MDX command to research web topics.
    /// </summary>
    public async Task<string> ExecuteResearchWebTopicCommand(
        string searchQuery,
        bool includePageContent = true,
        int maxResults = 5,
        string searchEngine = "duckduckgo",
        bool stripHtml = true,
        string? processingInstructions = null)
    {
        var arguments = BuildResearchWebTopicArguments(
            searchQuery, 
            includePageContent, 
            maxResults, 
            searchEngine, 
            stripHtml, 
            processingInstructions);
            
        return await ExecuteMdxCommandAsync($"{arguments} --firefox");
    }
    
    /// <summary>
    /// Builds command arguments for researching web topics.
    /// </summary>
    private string BuildResearchWebTopicArguments(
        string searchQuery,
        bool includePageContent = true,
        int maxResults = 5,
        string searchEngine = "duckduckgo",
        bool stripHtml = true,
        string? processingInstructions = null)
    {
        var sb = new StringBuilder();
        
        // Start with web search command
        sb.Append("web search ");
        
        // Add search query
        sb.Append($"{EscapeArgument(searchQuery)} ");
        
        // Add search engine option
        sb.Append($"--{searchEngine.ToLower()} ");
        
        // Limit results
        sb.Append($"--max {maxResults} ");
        
        // Add option to get page content
        if (includePageContent)
        {
            sb.Append("--get ");
            
            // Strip HTML if requested
            if (stripHtml)
            {
                sb.Append("--strip ");
            }
        }
        
        // Add processing instructions
        if (!string.IsNullOrEmpty(processingInstructions))
        {
            sb.Append($"--instructions {EscapeArgument(processingInstructions)} ");
        }
        
        return sb.ToString().Trim();
    }
    
    /// <summary>
    /// Executes MDX command to extract content from web pages.
    /// </summary>
    public async Task<string> ExecuteExtractContentFromWebPagesCommand(
        string[] urls,
        bool stripHtml = true,
        string? pageProcessingInstructions = null,
        string? finalProcessingInstructions = null)
    {
        var arguments = BuildExtractContentFromWebPagesArguments(
            urls, 
            stripHtml, 
            pageProcessingInstructions, 
            finalProcessingInstructions);
            
        return await ExecuteMdxCommandAsync($"{arguments} --firefox");
    }
    
    /// <summary>
    /// Builds command arguments for extracting content from web pages.
    /// </summary>
    private string BuildExtractContentFromWebPagesArguments(
        string[] urls,
        bool stripHtml = true,
        string? pageProcessingInstructions = null,
        string? finalProcessingInstructions = null)
    {
        var sb = new StringBuilder();
        
        // Start with web get command
        sb.Append("web get ");
        
        // Add URLs
        foreach (var url in urls)
        {
            sb.Append($"{EscapeArgument(url)} ");
        }
        
        // Strip HTML if requested
        if (stripHtml)
        {
            sb.Append("--strip ");
        }
        
        // Add page processing instructions
        if (!string.IsNullOrEmpty(pageProcessingInstructions))
        {
            sb.Append($"--page-instructions {EscapeArgument(pageProcessingInstructions)} ");
        }
        
        // Add final processing instructions
        if (!string.IsNullOrEmpty(finalProcessingInstructions))
        {
            sb.Append($"--instructions {EscapeArgument(finalProcessingInstructions)} ");
        }
        
        return sb.ToString().Trim();
    }
    
    #endregion
    
    #region Command Analysis Methods
    
    /// <summary>
    /// Executes MDX command to run commands and format their output.
    /// </summary>
    public async Task<string> ExecuteRunCommandsAndFormatOutputCommand(
        string[] commands,
        string shell = "cmd",
        string? processingInstructions = null)
    {
        var arguments = BuildRunCommandsAndFormatOutputArguments(
            commands, 
            shell, 
            processingInstructions);
            
        return await ExecuteMdxCommandAsync(arguments);
    }
    
    /// <summary>
    /// Builds command arguments for running commands and formatting their output.
    /// </summary>
    private string BuildRunCommandsAndFormatOutputArguments(
        string[] commands,
        string shell = "cmd",
        string? processingInstructions = null)
    {
        var sb = new StringBuilder();
        
        // Start with run command
        sb.Append("run ");
        
        // Add shell option if not default
        if (!string.IsNullOrEmpty(shell) && shell.ToLower() != "cmd")
        {
            sb.Append($"--{shell.ToLower()} ");
        }
        
        // Add commands
        foreach (var cmd in commands)
        {
            sb.Append($"{EscapeArgument(cmd)} ");
        }
        
        // Add processing instructions
        if (!string.IsNullOrEmpty(processingInstructions))
        {
            sb.Append($"--instructions {EscapeArgument(processingInstructions)} ");
        }
        
        return sb.ToString().Trim();
    }
    
    #endregion
    
    private string EscapeArgument(string arg)
    {
        var alreadyDoubleQuoted = arg.StartsWith("\"") && arg.EndsWith("\"");
        if (alreadyDoubleQuoted) return arg;

        var noSpacesOrSlashesOrQuotes = !arg.Contains(" ") && !arg.Contains("\\") && !arg.Contains("\"");
        if (noSpacesOrSlashesOrQuotes) return arg;

        var escaped = arg.Replace("\\", "\\\\").Replace("\"", "\\\"");
        var needsDoubleQuotes = escaped.Contains(" ") || escaped.Contains("\\") || escaped.Contains("\"");
        return needsDoubleQuotes ? $"\"{escaped}\"" : escaped;
    }
}