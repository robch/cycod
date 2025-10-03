using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

/// <summary>
/// Wrapper class for executing CYCODMD CLI commands and processing their output.
/// </summary>
public class CycoDmdCliWrapper
{
    /// <summary>
    /// Truncates command output according to line and total character limits.
    /// </summary>
    private string TruncateCommandOutput(string output, int maxCharsPerLine, int maxTotalChars)
    {
        var outputLines = output.Split('\n', StringSplitOptions.None)
            .Select(line => line.TrimEnd('\r'))
            .ToArray();
            
        var needLineTruncs = outputLines.Any(line => line.Length > maxCharsPerLine);
        var linesAfterTrunc = outputLines.Select(line => 
            needLineTruncs && line.Length > maxCharsPerLine
                ? line.Substring(0, maxCharsPerLine - 1) + "…" 
                : line)
            .ToArray();
        
        var formatted = string.Join("\n", linesAfterTrunc);
                
        var needTotalTrunc = formatted.Length > maxTotalChars;
        var formattedAfterTrunc = needTotalTrunc
            ? formatted.Substring(0, maxTotalChars - 1) + "…"
            : formatted;
        
        var noTruncations = !needLineTruncs && !needTotalTrunc;
        if (noTruncations) return formattedAfterTrunc;
        
        var onlyLinesTrunc = needLineTruncs && !needTotalTrunc;
        if (onlyLinesTrunc) return formattedAfterTrunc + "\n" + $"[Note: Lines with … were truncated ({maxCharsPerLine} char limit)]";
        
        var formattedLineCount = linesAfterTrunc.Count();
        var truncatedCount = formattedLineCount - formattedAfterTrunc.Split('\n').Length;

        var warning = needLineTruncs
            ? $"[{truncatedCount} more lines truncated; lines with … exceeded {maxCharsPerLine} char limit]"
            : $"[{truncatedCount} more lines truncated ({maxTotalChars} char total limit)]";

        return formattedAfterTrunc + "\n" + warning;
    }
    
    /// <summary>
    /// Executes an CYCODMD command and returns the output.
    /// </summary>
    /// <param name="arguments">The arguments to pass to the CYCODMD command</param>
    /// <param name="timeoutMs">Timeout in milliseconds</param>
    /// <returns>The command output as a string</returns>
    public async Task<string> ExecuteCycoDmdCommandAsync(string arguments, int timeoutMs = 120000)
    {
        try
        {
            // Always log the command at Info level for better tracking
            Logger.Info($"Executing CYCODMD command: cycodmd {arguments}");
            
            if (ConsoleHelpers.IsVerbose())
            {
                ConsoleHelpers.WriteLine($"Executing CYCODMD command: cycodmd {arguments}");
            }

            // Create a process builder to execute the cycodmd command
            var processBuilder = new RunnableProcessBuilder()
                .WithFileName("cycodmd")
                .WithArguments(arguments)
                .WithTimeout(timeoutMs)
                .WithVerboseLogging(ConsoleHelpers.IsVerbose());

            // Add callbacks if verbose logging is enabled
            if (ConsoleHelpers.IsVerbose())
            {
                processBuilder.OnOutput(line => {
                    var trimmedLine = line.TrimEnd(new char[] { '\r', '\n' });
                    ConsoleHelpers.WriteLine(trimmedLine, ConsoleColor.DarkMagenta);
                });
                
                processBuilder.OnError(line => {
                    var trimmedLine = line.TrimEnd(new char[] { '\r', '\n' });
                    ConsoleHelpers.WriteErrorLine(trimmedLine);
                });
            }

            // Run the process and get the result
            var result = await processBuilder.RunAsync();

            string output = result.MergedOutput.TrimEnd();
            
            // Add exit code message if not successful
            if (result.ExitCode != 0)
            {
                output += Environment.NewLine + $"<cycodmd command exited with code {result.ExitCode}>";
            }
            
            // Handle the case where the process timed out
            if (result.CompletionState == ProcessCompletionState.TimedOut)
            {
                output = $"<waiting {timeoutMs}ms for cycodmd to finish, timed out>\n{output}";
            }
            
            // Write debug files if verbose is enabled
            if (ConsoleHelpers.IsVerbose())
            {
                var baseFileName = FileHelpers.GetFileNameFromTemplate("cycodmd_command.txt", "cycodmd-debug-{time}");
                Thread.Sleep(10); // Wait a bit, so template filename w/ time doesn't conflict with anything else

                ConsoleHelpers.WriteLine($"ExecuteCycoDmdCommandAsync inputs/outputs: {baseFileName}-*", ConsoleColor.DarkMagenta, overrideQuiet: true);
                FileHelpers.WriteAllText($"{baseFileName}-command.txt", $"cycodmd {arguments}");
                FileHelpers.WriteAllText($"{baseFileName}-stdout.txt", result.StandardOutput);
                FileHelpers.WriteAllText($"{baseFileName}-stderr.txt", result.StandardError);
                FileHelpers.WriteAllText($"{baseFileName}-merged.txt", result.MergedOutput);
            }
            
            return output;
        }
        catch (Exception ex)
        {
            return $"<cycodmd command exited with error: {ex.Message}>";
        }
    }

    /// <summary>
    /// Escapes an argument for use in a command line.
    /// </summary>
    /// <param name="arg">The argument to escape</param>
    /// <returns>The escaped argument</returns>
    public string EscapeArgument(string arg)
    {
        return ProcessHelpers.EscapeProcessArgument(arg);
    }

    /// <summary>
    /// Escapes a regex pattern for use in command line arguments.
    /// Specifically designed to prevent double-escaping backslashes in regex patterns.
    /// </summary>
    /// <param name="pattern">The regex pattern to escape</param>
    /// <returns>The escaped regex pattern</returns>
    public string EscapeRegexPattern(string pattern)
    {
        if (string.IsNullOrEmpty(pattern)) return "\"\"";

        // Only escape quotes and leave backslashes as they are for regex patterns
        var escaped = pattern.Replace("\"", "\\\"");
        
        // Add surrounding quotes if needed
        if (escaped.Contains(' ') || escaped.Contains('\t') || escaped.Contains('|') || 
            escaped.Contains('<') || escaped.Contains('>') || escaped.Contains('&'))
        {
            escaped = $"\"{escaped}\"";
        }
        
        return escaped;
    }

    #region Search Codebase Methods
    
    /// <summary>
    /// Executes CYCODMD command with flexible query options for files and content.
    /// </summary>
    public async Task<string> ExecuteQueryFilesCommand(
        string[] filePatterns,
        string[]? excludePatterns = null,
        
        // File-level filtering
        string? fileContains = null,
        string? fileNotContains = null, 
        string? modifiedAfter = null,
        string? modifiedBefore = null,
        
        // Content extraction
        string? searchPattern = null,
        string? lineContains = null,
        string? removeAllLines = null,
        
        // Presentation
        int linesBeforeAndAfter = 0,
        bool lineNumbers = true,
        bool highlightMatches = false, // Internal parameter for highlighting
        
        // Limits
        int maxFiles = 50,
        int maxCharsPerLine = 500,
        int maxTotalChars = 100000)
    {
        var arguments = BuildQueryFilesArguments(
            filePatterns, excludePatterns, fileContains, fileNotContains,
            modifiedAfter, modifiedBefore, searchPattern, lineContains, 
            removeAllLines, linesBeforeAndAfter, lineNumbers, highlightMatches);
            
        var rawOutput = await ExecuteCycoDmdCommandAsync(arguments);
        return TruncateCommandOutput(rawOutput, maxCharsPerLine, maxTotalChars);
    }
    
    /// <summary>
    /// Builds command arguments for querying files with flexible options.
    /// </summary>
    private string BuildQueryFilesArguments(
        string[] filePatterns,
        string[]? excludePatterns,
        string? fileContains,
        string? fileNotContains, 
        string? modifiedAfter,
        string? modifiedBefore,
        string? searchPattern,
        string? lineContains,
        string? removeAllLines,
        int linesBeforeAndAfter,
        bool lineNumbers,
        bool highlightMatches)
    {
        var sb = new StringBuilder();
        
        // Add file patterns
        foreach (var pattern in filePatterns)
            sb.Append($"{EscapeArgument(pattern)} ");
            
        // Add exclude patterns
        if (excludePatterns?.Length > 0 && !IsNullString(excludePatterns[0]))
        {
            sb.Append("--exclude ");
            foreach (var pattern in excludePatterns)
                if (!IsNullString(pattern))
                    sb.Append($"{EscapeArgument(pattern)} ");
        }
        
        if (IsValidParameter(fileContains))
            sb.Append($"--file-contains {EscapeRegexPattern(fileContains!)} ");
            
        if (IsValidParameter(fileNotContains))
            sb.Append($"--file-not-contains {EscapeRegexPattern(fileNotContains!)} ");
            
        if (IsValidParameter(modifiedAfter))
            sb.Append($"--modified-after {EscapeArgument(modifiedAfter!)} ");
            
        if (IsValidParameter(modifiedBefore))
            sb.Append($"--modified-before {EscapeArgument(modifiedBefore!)} ");
        
        if (IsValidParameter(searchPattern))
        {
            sb.Append($"--contains {EscapeRegexPattern(searchPattern!)} ");
            if (linesBeforeAndAfter > 0)
                sb.Append($"--lines {linesBeforeAndAfter} ");
        }
        else if (IsValidParameter(lineContains))
        {
            sb.Append($"--line-contains {EscapeRegexPattern(lineContains!)} ");
            if (linesBeforeAndAfter > 0)
                sb.Append($"--lines {linesBeforeAndAfter} ");
        }
        
        if (IsValidParameter(removeAllLines))
            sb.Append($"--remove-all-lines {EscapeRegexPattern(removeAllLines!)} ");
            
        if (lineNumbers)
            sb.Append("--line-numbers ");
            
        if (highlightMatches)
            sb.Append("--highlight-matches ");
            
        return sb.ToString().Trim();
    }
    
    /// <summary>
    /// Helper method to check if a parameter is valid (not null, empty, or the string "null")
    /// </summary>
    private static bool IsValidParameter(string? parameter)
    {
        return !string.IsNullOrEmpty(parameter) && !IsNullString(parameter);
    }
    
    /// <summary>
    /// Helper method to check if a string is the literal "null" value
    /// </summary>
    private static bool IsNullString(string? value)
    {
        return string.Equals(value, "null", StringComparison.OrdinalIgnoreCase);
    }
    
    #endregion
    
    #region Documentation Generation Methods
    
    /// <summary>
    /// Executes CYCODMD command to convert files to markdown.
    /// </summary>
    public async Task<string> ExecuteConvertFilesToMarkdownCommand(
        string[] filePaths,
        string? formattingInstructions = null,
        bool showLineNumbers = false,
        int maxCharsPerLine = 500,
        int maxTotalChars = 100000)
    {
        var arguments = BuildConvertFilesToMarkdownArguments(
            filePaths, 
            formattingInstructions, 
            showLineNumbers);
            
        var rawOutput = await ExecuteCycoDmdCommandAsync(arguments);
        return TruncateCommandOutput(rawOutput, maxCharsPerLine, maxTotalChars);
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
    /// Executes CYCODMD command to research web topics.
    /// </summary>
    public async Task<string> ExecuteResearchWebTopicCommand(
        string searchQuery,
        bool includePageContent = true,
        int maxResults = 5,
        string searchEngine = "duckduckgo",
        bool stripHtml = true,
        string? processingInstructions = null,
        int maxCharsPerLine = 500,
        int maxTotalChars = 100000)
    {
        var arguments = BuildResearchWebTopicArguments(
            searchQuery, 
            includePageContent, 
            maxResults, 
            searchEngine, 
            stripHtml, 
            processingInstructions);
            
        var rawOutput = await ExecuteCycoDmdCommandAsync($"{arguments} --firefox");
        return TruncateCommandOutput(rawOutput, maxCharsPerLine, maxTotalChars);
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
    /// Executes CYCODMD command to extract content from web pages.
    /// </summary>
    public async Task<string> ExecuteExtractContentFromWebPagesCommand(
        string[] urls,
        bool stripHtml = true,
        string? pageProcessingInstructions = null,
        string? finalProcessingInstructions = null,
        int maxCharsPerLine = 500,
        int maxTotalChars = 100000)
    {
        var arguments = BuildExtractContentFromWebPagesArguments(
            urls, 
            stripHtml, 
            pageProcessingInstructions, 
            finalProcessingInstructions);
            
        var rawOutput = await ExecuteCycoDmdCommandAsync($"{arguments} --firefox");
        return TruncateCommandOutput(rawOutput, maxCharsPerLine, maxTotalChars);
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
    /// Executes CYCODMD command to run commands and format their output.
    /// </summary>
    public async Task<string> ExecuteRunCommandsAndFormatOutputCommand(
        string[] commands,
        string shell = "cmd",
        string? processingInstructions = null,
        int maxCharsPerLine = 500,
        int maxTotalChars = 100000)
    {
        var arguments = BuildRunCommandsAndFormatOutputArguments(
            commands, 
            shell, 
            processingInstructions);
            
        var rawOutput = await ExecuteCycoDmdCommandAsync(arguments);
        return TruncateCommandOutput(rawOutput, maxCharsPerLine, maxTotalChars);
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
    
}