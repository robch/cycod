using System;
using System.Linq;
using System.Threading.Tasks;
using CycoDj.Analyzers;
using CycoDj.CommandLine;
using CycoDj.Helpers;

namespace CycoDj.CommandLineCommands;

public class ListCommand : CycoDjCommand
{
    public string? Date { get; set; }
    public int Last { get; set; } = 0;
    public bool ShowBranches { get; set; } = false;

    public override string GetHelpTopic()
    {
        // When list is the default command and --help is used without explicit "list",
        // show the main usage help instead of list-specific help
        return "usage";
    }

    public override async Task<int> ExecuteAsync()
    {
        var output = GenerateListOutput();
        
        // Apply instructions if provided
        var finalOutput = ApplyInstructionsIfProvided(output);
        ConsoleHelpers.WriteLine(finalOutput);
        
        return await Task.FromResult(0);
    }
    
    private string GenerateListOutput()
    {
        var sb = new System.Text.StringBuilder();
        
        sb.AppendLine("## Chat History Conversations");
        sb.AppendLine();
        
        // Find all history files
        var files = HistoryFileHelpers.FindAllHistoryFiles();
        
        if (files.Count == 0)
        {
            sb.AppendLine("WARNING: No chat history files found");
            var historyDir = HistoryFileHelpers.GetHistoryDirectory();
            sb.AppendLine($"Expected location: {historyDir}");
            return sb.ToString();
        }
        
        // Filter by date if specified
        if (!string.IsNullOrEmpty(Date))
        {
            if (DateTime.TryParse(Date, out var dateFilter))
            {
                files = HistoryFileHelpers.FilterByDate(files, dateFilter);
                sb.AppendLine($"Filtered by date: {dateFilter:yyyy-MM-dd} ({files.Count} files)");
                sb.AppendLine();
            }
            else
            {
                sb.AppendLine($"ERROR: Invalid date format: {Date}");
                return sb.ToString();
            }
        }
        
        // Apply sensible default limit if not specified and no date filter
        var effectiveLimit = Last;
        if (effectiveLimit == 0 && string.IsNullOrEmpty(Date))
        {
            effectiveLimit = 20; // Default to last 20 conversations
            sb.AppendLine($"Showing last {effectiveLimit} conversations (use --last N to change, or --date to filter)");
            sb.AppendLine();
        }
        
        // Limit to last N if specified or defaulted
        if (effectiveLimit > 0 && files.Count > effectiveLimit)
        {
            files = files.Take(effectiveLimit).ToList();
            if (Last > 0)
            {
                sb.AppendLine($"Showing last {effectiveLimit} conversations");
                sb.AppendLine();
            }
        }
        
        // Read and display conversations
        var conversations = JsonlReader.ReadConversations(files);
        
        if (conversations.Count == 0)
        {
            sb.AppendLine("WARNING: No conversations could be read");
            return sb.ToString();
        }
        
        // Detect branches
        BranchDetector.DetectBranches(conversations);
        
        // Display conversations
        foreach (var conv in conversations)
        {
            var timestamp = TimestampHelpers.FormatTimestamp(conv.Timestamp, "datetime");
            var userCount = conv.Messages.Count(m => m.Role == "user");
            var assistantCount = conv.Messages.Count(m => m.Role == "assistant");
            var toolCount = conv.Messages.Count(m => m.Role == "tool");
            
            // Show indent if this is a branch
            var indent = conv.ParentId != null ? "  ↳ " : "";
            
            // Show timestamp and title
            sb.Append($"{indent}{timestamp} - ");
            
            if (!string.IsNullOrEmpty(conv.Metadata?.Title))
            {
                sb.Append($"{conv.Metadata.Title} ");
                sb.AppendLine($"({conv.Id})");
            }
            else
            {
                sb.AppendLine($"{conv.Id}");
            }
            
            sb.AppendLine($"{indent}  Messages: {userCount} user, {assistantCount} assistant, {toolCount} tool");
            
            // Show branch info if ShowBranches is enabled
            if (ShowBranches)
            {
                if (conv.ParentId != null)
                {
                    sb.AppendLine($"{indent}  Branch of: {conv.ParentId}");
                }
                if (conv.BranchIds.Count > 0)
                {
                    sb.AppendLine($"{indent}  Branches: {conv.BranchIds.Count}");
                }
                if (conv.ToolCallIds.Count > 0)
                {
                    sb.AppendLine($"{indent}  Tool calls: {conv.ToolCallIds.Count}");
                }
            }
            
            // Show preview - brief overview with just one message
            var userMessages = conv.Messages.Where(m => m.Role == "user" && !string.IsNullOrWhiteSpace(m.Content)).ToList();
            
            if (userMessages.Any())
            {
                // For branches, show last message (what's new)
                // For non-branches, show first message
                var messageToShow = conv.ParentId != null 
                    ? userMessages.Last() 
                    : userMessages.First();
                
                var preview = messageToShow.Content.Length > 80 
                    ? messageToShow.Content.Substring(0, 80) + "..." 
                    : messageToShow.Content;
                preview = preview.Replace("\n", " ").Replace("\r", "");
                
                sb.AppendLine($"{indent}  > {preview}");
                
                // Show indicator if there are more messages
                if (userMessages.Count > 1)
                {
                    sb.AppendLine($"{indent}    ... and {userMessages.Count - 1} more");
                }
            }
            
            sb.AppendLine();
        }
        
        sb.AppendLine($"Total: {conversations.Count} conversation(s)");
        
        // Show branch statistics
        var branchedConvs = conversations.Count(c => c.ParentId != null);
        if (branchedConvs > 0)
        {
            sb.AppendLine($"Branches: {branchedConvs} conversation(s) are branches of others");
        }
        
        return sb.ToString();
    }
}
