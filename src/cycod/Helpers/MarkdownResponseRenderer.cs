using ConsoleInk;
using System.Text;

namespace CycoD.Helpers;

public class MarkdownResponseRenderer : IDisposable
{
    private readonly MarkdownConsoleWriter? _markdownWriter;
    private readonly StringBuilder _markdownBuffer;
    private readonly bool _enableMarkdown;
    private readonly TextWriter _output;
    private bool _disposed = false;

    public MarkdownResponseRenderer(TextWriter output, bool enableMarkdown = true)
    {
        _enableMarkdown = enableMarkdown;
        _output = output;
        _markdownBuffer = new StringBuilder();

        if (_enableMarkdown)
        {
            var options = new MarkdownRenderOptions
            {
                ConsoleWidth = Console.WindowWidth > 0 ? Console.WindowWidth : 80,
                Theme = ConsoleTheme.Default,
                UseHyperlinks = true
            };
            _markdownWriter = new MarkdownConsoleWriter(_output, options);
        }
        else
        {
            _markdownWriter = null;
        }
    }

    public void WriteStreamingText(string text)
    {
        if (!_enableMarkdown)
        {
            // Fallback to plain text rendering
            ConsoleHelpers.Write(text, ConsoleColor.White, overrideQuiet: true);
            return;
        }

        _markdownBuffer.Append(text);

        // Check if we have complete markdown blocks that can be rendered
        var content = _markdownBuffer.ToString();
        var (completedContent, remainingContent) = ExtractCompletedMarkdownContent(content);

        if (!string.IsNullOrEmpty(completedContent))
        {
            try
            {
                _markdownWriter!.Write(completedContent);
                
                // Update buffer with remaining content
                _markdownBuffer.Clear();
                _markdownBuffer.Append(remainingContent);
            }
            catch (Exception)
            {
                // Fallback to plain text if markdown rendering fails
                ConsoleHelpers.Write(completedContent, ConsoleColor.White, overrideQuiet: true);
            }
        }
    }

    public void FlushRemainingContent()
    {
        if (!_enableMarkdown || _markdownBuffer.Length == 0)
            return;

        var remainingContent = _markdownBuffer.ToString();
        if (!string.IsNullOrEmpty(remainingContent))
        {
            try
            {
                _markdownWriter!.Write(remainingContent);
            }
            catch (Exception)
            {
                // Fallback to plain text if markdown rendering fails
                ConsoleHelpers.Write(remainingContent, ConsoleColor.White, overrideQuiet: true);
            }
        }

        _markdownBuffer.Clear();
    }

    private static (string completed, string remaining) ExtractCompletedMarkdownContent(string content)
    {
        // For streaming, we need to be careful about breaking markdown mid-block
        // Tables require complete structure to render properly
        
        var lines = content.Split('\n');
        if (lines.Length <= 1)
        {
            // Not enough content to determine completeness, wait for more
            return ("", content);
        }

        // Find the end of any complete table and process content before it
        var tableStart = -1;
        var tableEnd = -1;
        
        for (int i = 0; i < lines.Length - 1; i++) // -1 because last line might be incomplete
        {
            var line = lines[i].Trim();
            
            // Detect table start (header row followed by separator)
            if (tableStart == -1 && line.Contains('|') && i + 1 < lines.Length - 1)
            {
                var nextLine = lines[i + 1].Trim();
                if (nextLine.Contains('|') && nextLine.Contains('-'))
                {
                    tableStart = i;
                }
            }
            
            // Detect table end (non-table line after table content, or empty line)
            if (tableStart != -1 && tableEnd == -1)
            {
                if (string.IsNullOrWhiteSpace(line) || (!line.Contains('|')))
                {
                    tableEnd = i - 1;
                    break;
                }
            }
        }

        // If we're in the middle of an incomplete table, wait for more content
        if (tableStart != -1 && tableEnd == -1)
        {
            // Process content before the table, but hold the table
            if (tableStart > 0)
            {
                var beforeTable = lines.Take(tableStart);
                var completed = string.Join('\n', beforeTable) + '\n';
                var remaining = string.Join('\n', lines.Skip(tableStart));
                return (completed, remaining);
            }
            else
            {
                // Table starts at beginning, wait for more
                return ("", content);
            }
        }

        // No active table, process normally (keep last line as it might be incomplete)
        var completedLines = lines.Take(lines.Length - 1);
        var completedContent = string.Join('\n', completedLines);
        if (completedContent.Length > 0) completedContent += '\n';
        
        var remainingContent = lines.Last();

        return (completedContent, remainingContent);
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            FlushRemainingContent();
            _markdownWriter?.Dispose();
            _disposed = true;
        }
    }
}