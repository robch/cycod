//
// Copyright information and license can be added here
//

using System.ComponentModel;

/// <summary>
/// Helper functions for code exploration, web research, and documentation generation
/// using the CYCODMD CLI tool under the hood.
/// </summary>
public class CodeExplorationHelperFunctions
{
    #region Code Exploration Functions
    
    [ReadOnly(true)]
    [Description("Search across files in a codebase to find content matching specific patterns. Returns formatted markdown with file matches, similar to using Ctrl+Shift+F in an IDE but with more processing options. Great for exploring unfamiliar codebases.")]
    public async Task<string> SearchCodebaseForPattern(
        [Description("File glob patterns to search")] string[] filePatterns,
        [Description("Regex pattern to search for within files")] string contentPattern,
        [Description("File glob patterns to exclude")] string[]? excludePatterns = null,
        [Description("Include line numbers in results")] bool showLineNumbers = true,
        [Description("Number of context lines to show before and after matches")] int? contextLines = null)
    {
        return await _cycoDmdWrapper.ExecuteSearchCodebaseCommand(
            filePatterns,
            contentPattern,
            excludePatterns,
            showLineNumbers,
            contextLines);
    }
    
    [ReadOnly(true)]
    [Description("Find complete files that contain specific patterns and return their content. Useful for identifying relevant files in a large codebase when you need full file context, not just matching lines.")]
    public async Task<string> FindFilesContainingPattern(
        [Description("File glob patterns to search")] string[] filePatterns,
        [Description("Regex pattern that must exist somewhere in the files")] string contentPattern,
        [Description("File glob patterns to exclude")] string[]? excludePatterns = null)
    {
        return await _cycoDmdWrapper.ExecuteFindFilesContainingPatternCommand(filePatterns, contentPattern, excludePatterns);
    }
    
    #endregion
    
    #region Documentation Generation Functions
    
    [ReadOnly(true)]
    [Description("Convert various file types to well-formatted markdown. Supports documentation files, source code, images, PDFs, and more. Useful for generating documentation or reports from existing files.")]
    public async Task<string> ConvertFilesToMarkdown(
        [Description("Specific file paths or glob patterns to convert (e.g. [\"README.md\", \"*.pdf\", \"docs/*.docx\")")] string[] filePaths,
        [Description("Instructions for AI to format or structure the output (e.g. \"create a table of contents\" or \"highlight important sections\")")] string? formattingInstructions = null,
        [Description("Whether to include line numbers for code files")] bool showLineNumbers = false)
    {
        return await _cycoDmdWrapper.ExecuteConvertFilesToMarkdownCommand(filePaths, formattingInstructions, showLineNumbers);
    }
    
    #endregion
    
    #region Web Research Functions
    
    [Description("Search the web for information and create a markdown summary of results. Great for research tasks and gathering information from multiple sources without leaving your workflow.")]
    public async Task<string> ResearchWebTopic(
        [Description("Search query or research topic")] string searchQuery,
        [Description("Whether to download and include actual page content (not just links)")] bool includePageContent = true,
        [Description("Maximum number of search results to process")] int maxResults = 5,
        [Description("Search engine to use: 'google', 'bing', 'duckduckgo', or 'yahoo'")] string searchEngine = "duckduckgo",
        [Description("Whether to strip HTML formatting from page content")] bool stripHtml = true)
    {
        return await _cycoDmdWrapper.ExecuteResearchWebTopicCommand(searchQuery, includePageContent, maxResults, searchEngine, stripHtml);
    }
    
    [Description("Get content from specific web pages and convert to markdown. Useful for extracting information from known websites and creating summaries or documentation.")]
    public async Task<string> ExtractContentFromWebPages(
        [Description("URLs of web pages to extract content from")] string[] urls,
        [Description("Whether to strip HTML formatting")] bool stripHtml = true)
    {
        return await _cycoDmdWrapper.ExecuteExtractContentFromWebPagesCommand(urls, stripHtml);
    }
    
    #endregion
    
    private readonly CycoDmdCliWrapper _cycoDmdWrapper = new CycoDmdCliWrapper();
}