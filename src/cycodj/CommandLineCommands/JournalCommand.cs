using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using CycoDj.Analyzers;
using CycoDj.CommandLine;
using CycoDj.Helpers;
using CycoDj.Models;

namespace CycoDj.CommandLineCommands;

public class JournalCommand : CycoDjCommand
{
    public string? Date { get; set; }
    public int LastDays { get; set; } = 1;
    public bool Detailed { get; set; } = false;
    public string? Instructions { get; set; }
    public bool UseBuiltInFunctions { get; set; } = false;
    public string? SaveChatHistory { get; set; }

    public override async Task<int> ExecuteAsync()
    {
        // Determine date range
        DateTime startDate, endDate;
        
        if (!string.IsNullOrEmpty(Date))
        {
            if (Date.Equals("today", StringComparison.OrdinalIgnoreCase))
            {
                startDate = DateTime.Today;
                endDate = DateTime.Today.AddDays(1);
            }
            else if (DateTime.TryParse(Date, out var parsedDate))
            {
                startDate = parsedDate.Date;
                endDate = startDate.AddDays(1);
            }
            else
            {
                ConsoleHelpers.WriteErrorLine($"Invalid date format: {Date}");
                ConsoleHelpers.WriteLine("Use 'today' or a date like '2024-12-20'", ConsoleColor.Gray);
                return 1;
            }
        }
        else
        {
            // Default: last N days
            endDate = DateTime.Today.AddDays(1);
            startDate = DateTime.Today.AddDays(-LastDays + 1);
        }
        
        // Find and filter files
        var files = HistoryFileHelpers.FindAllHistoryFiles();
        var filteredFiles = files.Where(f =>
        {
            var timestamp = TimestampHelpers.ParseTimestamp(f);
            return timestamp >= startDate && timestamp < endDate;
        }).ToList();
        
        if (filteredFiles.Count == 0)
        {
            ConsoleHelpers.WriteWarning($"No conversations found between {startDate:yyyy-MM-dd} and {endDate.AddDays(-1):yyyy-MM-dd}");
            return 1;
        }
        
        // Read conversations
        var conversations = JsonlReader.ReadConversations(filteredFiles);
        if (conversations.Count == 0)
        {
            ConsoleHelpers.WriteWarning("No conversations could be read");
            return 1;
        }
        
        // Detect branches
        BranchDetector.DetectBranches(conversations);
        
        // Generate journal output
        var journalOutput = GenerateJournalOutput(conversations, startDate, endDate);
        
        // Apply instructions if provided
        if (!string.IsNullOrEmpty(Instructions))
        {
            var instructedOutput = AiInstructionProcessor.ApplyInstructions(
                Instructions, 
                journalOutput, 
                UseBuiltInFunctions, 
                SaveChatHistory);
            
            ConsoleHelpers.WriteLine(instructedOutput);
        }
        else
        {
            ConsoleHelpers.WriteLine(journalOutput);
        }
        
        return await Task.FromResult(0);
    }
    
    private string GenerateJournalOutput(List<Conversation> conversations, DateTime startDate, DateTime endDate)
    {
        var sb = new System.Text.StringBuilder();
        
        // Group by date
        var conversationsByDate = conversations
            .GroupBy(c => c.Timestamp.Date)
            .OrderBy(g => g.Key)
            .ToList();
        
        // Display journal
        sb.AppendLine("═".PadRight(80, '═'));
        if (startDate.Date == endDate.Date.AddDays(-1))
        {
            sb.AppendLine($"## Journal for {startDate:dddd, MMMM d, yyyy}");
        }
        else
        {
            sb.AppendLine($"## Journal: {startDate:MMM d} - {endDate.AddDays(-1):MMM d, yyyy}");
        }
        sb.AppendLine("═".PadRight(80, '═'));
        sb.AppendLine();
        
        foreach (var dateGroup in conversationsByDate)
        {
            var date = dateGroup.Key;
            var dayConvs = dateGroup.OrderBy(c => c.Timestamp).ToList();
            
            // Day header
            sb.AppendLine($"### {date:dddd, MMMM d, yyyy}");
            sb.AppendLine();
            
            // Group conversations by time period (morning, afternoon, evening)
            var morning = dayConvs.Where(c => c.Timestamp.Hour < 12).ToList();
            var afternoon = dayConvs.Where(c => c.Timestamp.Hour >= 12 && c.Timestamp.Hour < 17).ToList();
            var evening = dayConvs.Where(c => c.Timestamp.Hour >= 17).ToList();
            
            if (morning.Count > 0)
            {
                AppendTimePeriod(sb, "Morning", morning, Detailed);
            }
            
            if (afternoon.Count > 0)
            {
                AppendTimePeriod(sb, "Afternoon", afternoon, Detailed);
            }
            
            if (evening.Count > 0)
            {
                AppendTimePeriod(sb, "Evening", evening, Detailed);
            }
            
            // Day summary
            var totalMessages = dayConvs.Sum(c => c.Messages.Count);
            var totalUserMessages = dayConvs.Sum(c => c.Messages.Count(m => m.Role == "user"));
            var branchCount = dayConvs.Count(c => c.ParentId != null);
            
            sb.Append($"  Day Summary: {dayConvs.Count} conversations, {totalUserMessages} interactions");
            if (branchCount > 0)
            {
                sb.Append($", {branchCount} branches");
            }
            sb.AppendLine();
            sb.AppendLine();
        }
        
        // Overall summary
        var totalConversations = conversations.Count;
        var totalBranches = conversations.Count(c => c.ParentId != null);
        
        sb.AppendLine("─".PadRight(80, '─'));
        sb.Append($"Total: {totalConversations} conversations");
        if (totalBranches > 0)
        {
            sb.Append($" ({totalBranches} branches)");
        }
        sb.AppendLine();
        
        return sb.ToString();
    }
    
    private void AppendTimePeriod(System.Text.StringBuilder sb, string period, List<Conversation> conversations, bool detailed)
    {
        sb.AppendLine($"####  {period} ({conversations.Count} conversations)");
        sb.AppendLine();
        
        foreach (var conv in conversations)
        {
            var time = TimestampHelpers.FormatTimestamp(conv.Timestamp, "time");
            var indent = conv.ParentId != null ? "  ↳ " : "";
            
            // Time and title/ID
            sb.Append($"{indent}{time} - ");
            
            if (!string.IsNullOrEmpty(conv.Metadata?.Title))
            {
                sb.AppendLine(conv.Metadata.Title);
            }
            else
            {
                sb.AppendLine(conv.Id);
            }
            
            // User messages summary - journal shows more context than list
            var userMessages = conv.Messages.Where(m => m.Role == "user" && !string.IsNullOrWhiteSpace(m.Content)).ToList();
            if (userMessages.Count > 0)
            {
                // For branches, show last messages to see what's different
                // For non-branches, show first messages
                // Show 3 messages to give good narrative context
                var messagesToShow = new List<string>();
                
                if (conv.ParentId != null)
                {
                    // Branch: show last 3 messages
                    var lastMessages = userMessages.Skip(Math.Max(0, userMessages.Count - 3)).Take(3);
                    
                    foreach (var msg in lastMessages)
                    {
                        var preview = msg.Content.Length > 100 
                            ? msg.Content.Substring(0, 100) + "..." 
                            : msg.Content;
                        preview = preview.Replace("\n", " ").Replace("\r", "");
                        messagesToShow.Add(preview);
                    }
                }
                else
                {
                    // Non-branch: show first 3 messages
                    var firstMessages = userMessages.Take(3);
                    
                    foreach (var msg in firstMessages)
                    {
                        var preview = msg.Content.Length > 100 
                            ? msg.Content.Substring(0, 100) + "..." 
                            : msg.Content;
                        preview = preview.Replace("\n", " ").Replace("\r", "");
                        messagesToShow.Add(preview);
                    }
                }
                
                // Display messages
                foreach (var message in messagesToShow)
                {
                    sb.AppendLine($"{indent}  > {message}");
                }
                
                // Show count of remaining messages
                var shownCount = messagesToShow.Count;
                if (userMessages.Count > shownCount)
                {
                    sb.AppendLine($"{indent}  ... and {userMessages.Count - shownCount} more");
                }
            }
            
            // Detailed mode: show tool usage and file operations
            if (detailed)
            {
                // Tool usage statistics
                var toolStats = ContentSummarizer.GetToolCallStatistics(conv);
                if (toolStats.Count > 0)
                {
                    var topTools = toolStats.OrderByDescending(kvp => kvp.Value).Take(5);
                    var toolSummary = string.Join(", ", topTools.Select(kvp => $"{kvp.Key} ({kvp.Value}x)"));
                    sb.AppendLine($"{indent}  Tools: {toolSummary}");
                }
                
                // Files modified
                var files = ContentSummarizer.GetFilesModified(conv);
                if (files.Count > 0)
                {
                    var fileList = files.Count <= 3 
                        ? string.Join(", ", files) 
                        : string.Join(", ", files.Take(3)) + $" +{files.Count - 3} more";
                    sb.AppendLine($"{indent}  Files: {fileList}");
                }
            }
            
            // Message count
            var msgCount = conv.Messages.Count(m => m.Role == "user" || m.Role == "assistant");
            sb.AppendLine($"{indent}  ({msgCount} messages)");
            
            sb.AppendLine();
        }
    }
    
}
