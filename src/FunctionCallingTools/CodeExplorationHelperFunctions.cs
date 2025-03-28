//
// Copyright information and license can be added here
//

using System;
using System.Threading.Tasks;

/// <summary>
/// Helper functions for code exploration, web research, and documentation generation
/// using the MDX CLI tool under the hood.
/// </summary>
public class CodeExplorationHelperFunctions
{
    private readonly MdxCliWrapper _mdxWrapper = new MdxCliWrapper();
    
    #region Code Exploration Functions
    
    [HelperFunctionDescription("Search across files in a codebase to find content matching specific patterns. Returns formatted markdown with file matches, similar to using Ctrl+Shift+F in an IDE but with more processing options. Great for exploring unfamiliar codebases.")]
    public async Task<string> SearchCodebaseForPattern(
        [HelperFunctionParameterDescription("File glob patterns to search")] string[] filePatterns,
        [HelperFunctionParameterDescription("Regex pattern to search for within files")] string contentPattern,
        [HelperFunctionParameterDescription("File glob patterns to exclude")] string[]? excludePatterns = null,
        [HelperFunctionParameterDescription("Include line numbers in results")] bool showLineNumbers = true,
        [HelperFunctionParameterDescription("Number of context lines to show before and after matches")] int? contextLines = null,
        [HelperFunctionParameterDescription("Instructions for AI to process or filter results")] string? processingInstructions = null)
    {
        return await _mdxWrapper.ExecuteSearchCodebaseCommand(
            filePatterns,
            contentPattern,
            excludePatterns,
            showLineNumbers,
            contextLines,
            processingInstructions);
    }
    
    [HelperFunctionDescription("Find complete files that contain specific patterns and return their content. Useful for identifying relevant files in a large codebase when you need full file context, not just matching lines.")]
    public async Task<string> FindFilesContainingPattern(
        [HelperFunctionParameterDescription("File glob patterns to search")] string[] filePatterns,
        [HelperFunctionParameterDescription("Regex pattern that must exist somewhere in the files")] string contentPattern,
        [HelperFunctionParameterDescription("File glob patterns to exclude")] string[]? excludePatterns = null,
        [HelperFunctionParameterDescription("Maximum number of files to return")] int maxFiles = 10,
        [HelperFunctionParameterDescription("Instructions for AI to process each file")] string? processingInstructions = null)
    {
        return await _mdxWrapper.ExecuteFindFilesContainingPatternCommand(
            filePatterns,
            contentPattern,
            excludePatterns,
            maxFiles,
            processingInstructions);
    }
    
    #endregion
    
    #region Documentation Generation Functions
    
    [HelperFunctionDescription("Convert various file types to well-formatted markdown. Supports documentation files, source code, images, PDFs, and more. Useful for generating documentation or reports from existing files.")]
    public async Task<string> ConvertFilesToMarkdown(
        [HelperFunctionParameterDescription("Specific file paths or glob patterns to convert (e.g. [\"README.md\", \"*.pdf\", \"docs/*.docx\")")] string[] filePaths,
        [HelperFunctionParameterDescription("Instructions for AI to format or structure the output (e.g. \"create a table of contents\" or \"highlight important sections\")")] string? formattingInstructions = null,
        [HelperFunctionParameterDescription("Whether to include line numbers for code files")] bool showLineNumbers = false)
    {
        return await _mdxWrapper.ExecuteConvertFilesToMarkdownCommand(
            filePaths,
            formattingInstructions,
            showLineNumbers);
    }
    
    #endregion
    
    #region Web Research Functions
    
    [HelperFunctionDescription("Search the web for information and create a markdown summary of results. Great for research tasks and gathering information from multiple sources without leaving your workflow.")]
    public async Task<string> ResearchWebTopic(
        [HelperFunctionParameterDescription("Search query or research topic")] string searchQuery,
        [HelperFunctionParameterDescription("Whether to download and include actual page content (not just links)")] bool includePageContent = true,
        [HelperFunctionParameterDescription("Maximum number of search results to process")] int maxResults = 5,
        [HelperFunctionParameterDescription("Search engine to use: 'google', 'bing', 'duckduckgo', or 'yahoo'")] string searchEngine = "duckduckgo",
        [HelperFunctionParameterDescription("Whether to strip HTML formatting from page content")] bool stripHtml = true,
        [HelperFunctionParameterDescription("Instructions for AI to process or filter the results (e.g. 'focus on technical details', 'summarize each page in one paragraph')")] string? processingInstructions = null)
    {
        return await _mdxWrapper.ExecuteResearchWebTopicCommand(
            searchQuery,
            includePageContent,
            maxResults,
            searchEngine,
            stripHtml,
            processingInstructions);
    }
    
    [HelperFunctionDescription("Get content from specific web pages and convert to markdown. Useful for extracting information from known websites and creating summaries or documentation.")]
    public async Task<string> ExtractContentFromWebPages(
        [HelperFunctionParameterDescription("URLs of web pages to extract content from")] string[] urls,
        [HelperFunctionParameterDescription("Whether to strip HTML formatting")] bool stripHtml = true,
        [HelperFunctionParameterDescription("Instructions for AI to process each page (e.g. 'extract the main article', 'identify key concepts')")] string? pageProcessingInstructions = null,
        [HelperFunctionParameterDescription("Instructions for AI to process the combined output of all pages")] string? finalProcessingInstructions = null)
    {
        return await _mdxWrapper.ExecuteExtractContentFromWebPagesCommand(
            urls,
            stripHtml,
            pageProcessingInstructions,
            finalProcessingInstructions);
    }
    
    #endregion
    
    #region Command Analysis Functions
    
    [HelperFunctionDescription("Run commands and convert their output to formatted markdown. Useful for analyzing command results, system information, or any command-line tool output.")]
    public async Task<string> RunCommandsAndFormatOutput(
        [HelperFunctionParameterDescription("Commands to execute (each will run separately)")] string[] commands,
        [HelperFunctionParameterDescription("Shell to use: 'cmd', 'bash', or 'powershell'")] string shell = "cmd",
        [HelperFunctionParameterDescription("Instructions for AI to process the command output (e.g. 'create a table of processes', 'highlight errors')")] string? processingInstructions = null)
    {
        return await _mdxWrapper.ExecuteRunCommandsAndFormatOutputCommand(
            commands,
            shell,
            processingInstructions);
    }
    
    #endregion
}