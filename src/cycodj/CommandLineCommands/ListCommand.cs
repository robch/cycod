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

    public override async Task<int> ExecuteAsync()
    {
        ConsoleHelpers.WriteLine("## Chat History Conversations", ConsoleColor.Cyan);
        ConsoleHelpers.WriteLine();
        
        // Find all history files
        var files = HistoryFileHelpers.FindAllHistoryFiles();
        
        if (files.Count == 0)
        {
            ConsoleHelpers.WriteWarning("No chat history files found");
            var historyDir = HistoryFileHelpers.GetHistoryDirectory();
            ConsoleHelpers.WriteLine($"Expected location: {historyDir}", ConsoleColor.Gray);
            return 1;
        }
        
        // Filter by date if specified
        if (!string.IsNullOrEmpty(Date))
        {
            if (DateTime.TryParse(Date, out var dateFilter))
            {
                files = HistoryFileHelpers.FilterByDate(files, dateFilter);
                ConsoleHelpers.WriteLine($"Filtered by date: {dateFilter:yyyy-MM-dd} ({files.Count} files)", ConsoleColor.Gray);
                ConsoleHelpers.WriteLine();
            }
            else
            {
                ConsoleHelpers.WriteErrorLine($"Invalid date format: {Date}");
                return 1;
            }
        }
        
        // Apply sensible default limit if not specified and no date filter
        var effectiveLimit = Last;
        if (effectiveLimit == 0 && string.IsNullOrEmpty(Date))
        {
            effectiveLimit = 20; // Default to last 20 conversations
            ConsoleHelpers.WriteLine($"Showing last {effectiveLimit} conversations (use --last N to change, or --date to filter)", ConsoleColor.Gray);
            ConsoleHelpers.WriteLine();
        }
        
        // Limit to last N if specified or defaulted
        if (effectiveLimit > 0 && files.Count > effectiveLimit)
        {
            files = files.Take(effectiveLimit).ToList();
            if (Last > 0)
            {
                ConsoleHelpers.WriteLine($"Showing last {effectiveLimit} conversations", ConsoleColor.Gray);
                ConsoleHelpers.WriteLine();
            }
        }
        
        // Read and display conversations
        var conversations = JsonlReader.ReadConversations(files);
        
        if (conversations.Count == 0)
        {
            ConsoleHelpers.WriteWarning("No conversations could be read");
            return 1;
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
            
            // Show timestamp
            ConsoleHelpers.Write($"{indent}{timestamp}", ConsoleColor.White);
            ConsoleHelpers.Write($" - ", ConsoleColor.Gray);
            
            // Show title if available, otherwise show ID
            if (!string.IsNullOrEmpty(conv.Metadata?.Title))
            {
                ConsoleHelpers.Write($"{conv.Metadata.Title}", ConsoleColor.Cyan);
                ConsoleHelpers.Write($" ", ConsoleColor.Gray);
                ConsoleHelpers.WriteLine($"({conv.Id})", ConsoleColor.DarkGray);
            }
            else
            {
                ConsoleHelpers.WriteLine($"{conv.Id}", ConsoleColor.Cyan);
            }
            
            ConsoleHelpers.Write($"{indent}  Messages: ", ConsoleColor.Gray);
            ConsoleHelpers.Write($"{userCount} user", ConsoleColor.Green);
            ConsoleHelpers.Write($", ", ConsoleColor.Gray);
            ConsoleHelpers.Write($"{assistantCount} assistant", ConsoleColor.Blue);
            ConsoleHelpers.Write($", ", ConsoleColor.Gray);
            ConsoleHelpers.WriteLine($"{toolCount} tool", ConsoleColor.DarkGray);
            
            // Show branch info if ShowBranches is enabled
            if (ShowBranches)
            {
                if (conv.ParentId != null)
                {
                    ConsoleHelpers.WriteLine($"{indent}  Branch of: {conv.ParentId}", ConsoleColor.Yellow);
                }
                if (conv.BranchIds.Count > 0)
                {
                    ConsoleHelpers.WriteLine($"{indent}  Branches: {conv.BranchIds.Count}", ConsoleColor.Yellow);
                }
                if (conv.ToolCallIds.Count > 0)
                {
                    ConsoleHelpers.WriteLine($"{indent}  Tool calls: {conv.ToolCallIds.Count}", ConsoleColor.DarkGray);
                }
            }
            
            // Show first user message as preview if available
            var firstUserMsg = conv.Messages.FirstOrDefault(m => m.Role == "user");
            if (firstUserMsg != null && !string.IsNullOrWhiteSpace(firstUserMsg.Content))
            {
                var preview = firstUserMsg.Content.Length > 80 
                    ? firstUserMsg.Content.Substring(0, 80) + "..." 
                    : firstUserMsg.Content;
                preview = preview.Replace("\n", " ").Replace("\r", "");
                ConsoleHelpers.WriteLine($"{indent}  > {preview}", ConsoleColor.DarkGray);
            }
            
            ConsoleHelpers.WriteLine();
        }
        
        ConsoleHelpers.WriteLine($"Total: {conversations.Count} conversation(s)", ConsoleColor.Green);
        
        // Show branch statistics
        var branchedConvs = conversations.Count(c => c.ParentId != null);
        if (branchedConvs > 0)
        {
            ConsoleHelpers.WriteLine($"Branches: {branchedConvs} conversation(s) are branches of others", ConsoleColor.Yellow);
        }
        
        return await Task.FromResult(0);
    }
}
