//
// Copyright information and license can be added here
//

using System.ComponentModel;
using System.Text;

/// <summary>
/// Helper functions for code exploration, web research, and documentation generation
/// using the CYCODMD CLI tool under the hood.
/// </summary>
public class CodeExplorationHelperFunctions
{
    #region Code Exploration Functions
    
    [ReadOnly(true)]
    [Description("Find files matching patterns and filters. Returns file paths only, without content.")]
    public async Task<string> FindFiles(
        [Description("File glob patterns to search (e.g., **/*.cs, src/*.md)")] string[] filePatterns,
        [Description("File glob patterns to exclude")] string[]? excludePatterns = null,
        [Description("Only include files containing this regex pattern")] string fileContains = "",
        [Description("Exclude files containing this regex pattern")] string fileNotContains = "",
        [Description("Only files modified after this time (e.g., '3d', '2023-01-01')")] string modifiedAfter = "",
        [Description("Only files modified before this time")] string modifiedBefore = "",
        [Description("Maximum number of files to process.")] int maxFiles = 50)
    {
        Logger.Info($"FindFiles called with filePatterns: [{string.Join(", ", filePatterns)}]");
        Logger.InfoIf(!string.IsNullOrEmpty(fileContains), $"File contains: '{fileContains}'");

        return await _cycoDmdWrapper.ExecuteFindFilesCommand(
            filePatterns, excludePatterns, fileContains, fileNotContains,
            modifiedAfter, modifiedBefore, maxFiles);
    }
    
    [ReadOnly(true)]
    [Description("Search for content within files and display matches with context. Shows file content with highlighted matches.")]
    public async Task<string> FindInFiles(
        [Description("File glob patterns to search (e.g., **/*.cs, src/*.md)")] string[] filePatterns,
        [Description("File glob patterns to exclude")] string[]? excludePatterns = null,
        [Description("Only include files containing this regex pattern")] string fileContains = "",
        [Description("Exclude files containing this regex pattern")] string fileNotContains = "",
        [Description("Only files modified after this time (e.g., '3d', '2023-01-01')")] string modifiedAfter = "",
        [Description("Only files modified before this time")] string modifiedBefore = "",
        [Description("Search pattern to find and highlight with context lines")] string searchPattern = "",
        [Description("Only show lines containing this regex pattern. Applied after removeAllLines filter.")] string lineContains = "",
        [Description("Remove lines containing this regex pattern. Applied first, before other filters.")] string removeAllLines = "",
        [Description("Number of lines to show before and after matches. 0 = matches only.")] int linesBeforeAndAfter = 0,
        [Description("Include line numbers in output.")] bool lineNumbers = true,
        [Description("Maximum number of files to process.")] int maxFiles = 50,
        [Description("Maximum number of characters to display per line.")] int maxCharsPerLine = 500,
        [Description("Maximum total number of characters to display.")] int maxTotalChars = 100000)
    {
        Logger.Info($"FindInFiles called with filePatterns: [{string.Join(", ", filePatterns)}]");
        Logger.InfoIf(!string.IsNullOrEmpty(searchPattern), $"Search pattern: '{searchPattern}'");
        Logger.InfoIf(!string.IsNullOrEmpty(lineContains), $"Line contains: '{lineContains}'");
        Logger.InfoIf(!string.IsNullOrEmpty(fileContains), $"File contains: '{fileContains}'");

        // Determine if we should highlight matches (when linesBeforeAndAfter > 0)
        var shouldHighlight = linesBeforeAndAfter > 0;
        
        return await _cycoDmdWrapper.ExecuteFindInFilesCommand(
            filePatterns, excludePatterns, fileContains, fileNotContains,
            modifiedAfter, modifiedBefore, searchPattern, lineContains,
            removeAllLines, linesBeforeAndAfter, lineNumbers, shouldHighlight,
            maxFiles, maxCharsPerLine, maxTotalChars);
    }
    
    #endregion
    
    #region Documentation Generation Functions
    
    [ReadOnly(true)]
    [Description("Convert various file types to well-formatted markdown. Supports documentation files, source code, images, PDFs, and more. Useful for generating documentation or reports from existing files.")]
    public async Task<string> ConvertFilesToMarkdown(
        [Description("Specific file paths or glob patterns to convert (e.g. [\"README.md\", \"*.pdf\", \"docs/*.docx\")")] string[] filePaths,
        [Description("Instructions for AI to format or structure the output (e.g. \"create a table of contents\" or \"highlight important sections\")")] string formattingInstructions = "",
        [Description("Whether to include line numbers for code files")] bool showLineNumbers = false,
        [Description("Maximum number of characters to display per line.")] int maxCharsPerLine = 500,
        [Description("Maximum total number of characters to display.")] int maxTotalChars = 100000)
    {
        return await _cycoDmdWrapper.ExecuteConvertFilesToMarkdownCommand(
            filePaths, 
            formattingInstructions, 
            showLineNumbers,
            maxCharsPerLine,
            maxTotalChars);
    }
    
    #endregion
    
    #region Web Research Functions
    
    [Description("Search the web for information and create a markdown summary of results. Great for research tasks and gathering information from multiple sources without leaving your workflow.")]
    public async Task<string> ResearchWebTopic(
        [Description("Search query or research topic")] string searchQuery,
        [Description("Whether to download and include actual page content (not just links)")] bool includePageContent = true,
        [Description("Maximum number of search results to process")] int maxResults = 5,
        [Description("Search engine to use: 'google', 'bing', 'duckduckgo', or 'yahoo'")] string searchEngine = "duckduckgo",
        [Description("Whether to strip HTML formatting from page content")] bool stripHtml = true,
        [Description("Maximum number of characters to display per line.")] int maxCharsPerLine = 500,
        [Description("Maximum total number of characters to display.")] int maxTotalChars = 100000)
    {
        return await _cycoDmdWrapper.ExecuteResearchWebTopicCommand(
            searchQuery, 
            includePageContent, 
            maxResults, 
            searchEngine, 
            stripHtml,
            null, // processingInstructions
            maxCharsPerLine,
            maxTotalChars);
    }
    
    [Description("Get content from specific web pages and convert to markdown. Useful for extracting information from known websites and creating summaries or documentation.")]
    public async Task<string> ExtractContentFromWebPages(
        [Description("URLs of web pages to extract content from")] string[] urls,
        [Description("Whether to strip HTML formatting")] bool stripHtml = true,
        [Description("Maximum number of characters to display per line.")] int maxCharsPerLine = 500,
        [Description("Maximum total number of characters to display.")] int maxTotalChars = 100000)
    {
        return await _cycoDmdWrapper.ExecuteExtractContentFromWebPagesCommand(
            urls, 
            stripHtml,
            null, // pageProcessingInstructions
            null, // finalProcessingInstructions
            maxCharsPerLine,
            maxTotalChars);
    }
    
    #endregion
    
    private readonly CycoDmdCliWrapper _cycoDmdWrapper = new CycoDmdCliWrapper();
}