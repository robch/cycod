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
    public int? MessageCount { get; set; } = null; // null = use default (3)
    public bool ShowStats { get; set; } = false;

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
        
        // Save to file if --save-output was provided
        if (SaveOutputIfRequested(finalOutput))
        {
            return await Task.FromResult(0);
        }
        
        // Otherwise print to console
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
        
        // Filter by time range if After/Before are set (from --today, --yesterday, --last <timespec>, etc.)
        if (After.HasValue || Before.HasValue)
        {
            files = HistoryFileHelpers.FilterByDateRange(files, After, Before);
            
            if (After.HasValue && Before.HasValue)
            {
                sb.AppendLine($"Filtered by time range: {After:yyyy-MM-dd HH:mm} to {Before:yyyy-MM-dd HH:mm} ({files.Count} files)");
            }
            else if (After.HasValue)
            {
                sb.AppendLine($"Filtered: after {After:yyyy-MM-dd HH:mm} ({files.Count} files)");
            }
            else if (Before.HasValue)
            {
                sb.AppendLine($"Filtered: before {Before:yyyy-MM-dd HH:mm} ({files.Count} files)");
            }
            sb.AppendLine();
        }
        // Filter by date if specified (backward compatibility)
        else if (!string.IsNullOrEmpty(Date))
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
        
        // Apply sensible default limit if not specified and no filters
        var effectiveLimit = Last;
        if (effectiveLimit == 0 && !After.HasValue && !Before.HasValue && string.IsNullOrEmpty(Date))
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
            
            // Show preview - configurable number of messages
            var messageCount = MessageCount ?? 3; // Default to 3 messages
            var userMessages = conv.Messages.Where(m => m.Role == "user" && !string.IsNullOrWhiteSpace(m.Content)).ToList();
            
            if (userMessages.Any() && messageCount > 0)
            {
                // For branches, show last N messages (what's new)
                // For non-branches, show first N messages
                var messagesToShow = conv.ParentId != null 
                    ? userMessages.TakeLast(Math.Min(messageCount, userMessages.Count))
                    : userMessages.Take(Math.Min(messageCount, userMessages.Count));
                
                foreach (var msg in messagesToShow)
                {
                    var preview = msg.Content.Length > 200 
                        ? msg.Content.Substring(0, 200) + "..." 
                        : msg.Content;
                    preview = preview.Replace("\n", " ").Replace("\r", "");
                    
                    sb.AppendLine($"{indent}  > {preview}");
                }
                
                // Show indicator if there are more messages
                var shownCount = messagesToShow.Count();
                if (userMessages.Count > shownCount)
                {
                    sb.AppendLine($"{indent}    ... and {userMessages.Count - shownCount} more");
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
        
        // Add statistics if requested
        if (ShowStats && conversations.Any())
        {
            sb.AppendLine();
            sb.AppendLine("═══════════════════════════════════════");
            sb.AppendLine("## Statistics Summary");
            sb.AppendLine("═══════════════════════════════════════");
            sb.AppendLine();
            
            var totalMessages = conversations.Sum(c => c.Messages.Count);
            var totalUserMessages = conversations.Sum(c => c.Messages.Count(m => m.Role == "user"));
            var totalAssistantMessages = conversations.Sum(c => c.Messages.Count(m => m.Role == "assistant"));
            var totalToolMessages = conversations.Sum(c => c.Messages.Count(m => m.Role == "tool"));
            var avgMessages = totalMessages / (double)conversations.Count;
            
            sb.AppendLine($"Total conversations: {conversations.Count}");
            sb.AppendLine($"Total messages: {totalMessages:N0}");
            sb.AppendLine($"  User: {totalUserMessages:N0} ({totalUserMessages * 100.0 / totalMessages:F1}%)");
            sb.AppendLine($"  Assistant: {totalAssistantMessages:N0} ({totalAssistantMessages * 100.0 / totalMessages:F1}%)");
            sb.AppendLine($"  Tool: {totalToolMessages:N0} ({totalToolMessages * 100.0 / totalMessages:F1}%)");
            sb.AppendLine();
            sb.AppendLine($"Average messages/conversation: {avgMessages:F1}");
            sb.AppendLine($"Branched conversations: {branchedConvs} ({branchedConvs * 100.0 / conversations.Count:F1}%)");
            sb.AppendLine();
            sb.AppendLine("═══════════════════════════════════════");
        }
        
        return sb.ToString();
    }
}
