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
        
        // Group by date
        var conversationsByDate = conversations
            .GroupBy(c => c.Timestamp.Date)
            .OrderBy(g => g.Key)
            .ToList();
        
        // Display journal
        ConsoleHelpers.WriteLine("═".PadRight(80, '═'), ConsoleColor.Cyan);
        if (startDate.Date == endDate.Date.AddDays(-1))
        {
            ConsoleHelpers.WriteLine($"## Journal for {startDate:dddd, MMMM d, yyyy}", ConsoleColor.Cyan);
        }
        else
        {
            ConsoleHelpers.WriteLine($"## Journal: {startDate:MMM d} - {endDate.AddDays(-1):MMM d, yyyy}", ConsoleColor.Cyan);
        }
        ConsoleHelpers.WriteLine("═".PadRight(80, '═'), ConsoleColor.Cyan);
        ConsoleHelpers.WriteLine();
        
        foreach (var dateGroup in conversationsByDate)
        {
            var date = dateGroup.Key;
            var dayConvs = dateGroup.OrderBy(c => c.Timestamp).ToList();
            
            // Day header
            ConsoleHelpers.WriteLine($"### {date:dddd, MMMM d, yyyy}", ConsoleColor.Yellow);
            ConsoleHelpers.WriteLine();
            
            // Group conversations by time period (morning, afternoon, evening)
            var morning = dayConvs.Where(c => c.Timestamp.Hour < 12).ToList();
            var afternoon = dayConvs.Where(c => c.Timestamp.Hour >= 12 && c.Timestamp.Hour < 17).ToList();
            var evening = dayConvs.Where(c => c.Timestamp.Hour >= 17).ToList();
            
            if (morning.Count > 0)
            {
                DisplayTimePeriod("Morning", morning, Detailed);
            }
            
            if (afternoon.Count > 0)
            {
                DisplayTimePeriod("Afternoon", afternoon, Detailed);
            }
            
            if (evening.Count > 0)
            {
                DisplayTimePeriod("Evening", evening, Detailed);
            }
            
            // Day summary
            var totalMessages = dayConvs.Sum(c => c.Messages.Count);
            var totalUserMessages = dayConvs.Sum(c => c.Messages.Count(m => m.Role == "user"));
            var branchCount = dayConvs.Count(c => c.ParentId != null);
            
            ConsoleHelpers.Write($"  Day Summary: ", ConsoleColor.Gray);
            ConsoleHelpers.Write($"{dayConvs.Count} conversations", ConsoleColor.White);
            ConsoleHelpers.Write($", ", ConsoleColor.Gray);
            ConsoleHelpers.Write($"{totalUserMessages} interactions", ConsoleColor.Green);
            if (branchCount > 0)
            {
                ConsoleHelpers.Write($", ", ConsoleColor.Gray);
                ConsoleHelpers.Write($"{branchCount} branches", ConsoleColor.Yellow);
            }
            ConsoleHelpers.WriteLine();
            ConsoleHelpers.WriteLine();
        }
        
        // Overall summary
        var totalConversations = conversations.Count;
        var totalBranches = conversations.Count(c => c.ParentId != null);
        
        ConsoleHelpers.WriteLine("─".PadRight(80, '─'), ConsoleColor.DarkGray);
        ConsoleHelpers.Write($"Total: ", ConsoleColor.Gray);
        ConsoleHelpers.Write($"{totalConversations} conversations", ConsoleColor.White);
        if (totalBranches > 0)
        {
            ConsoleHelpers.Write($" ({totalBranches} branches)", ConsoleColor.Yellow);
        }
        ConsoleHelpers.WriteLine();
        
        return await Task.FromResult(0);
    }
    
    private void DisplayTimePeriod(string period, List<Conversation> conversations, bool detailed)
    {
        ConsoleHelpers.WriteLine($"#### {period} ({conversations.Count} conversations)", ConsoleColor.Cyan);
        ConsoleHelpers.WriteLine();
        
        foreach (var conv in conversations)
        {
            var time = TimestampHelpers.FormatTimestamp(conv.Timestamp, "time");
            var indent = conv.ParentId != null ? "  ↳ " : "";
            
            // Time and title/ID
            ConsoleHelpers.Write($"{indent}{time}", ConsoleColor.White);
            ConsoleHelpers.Write(" - ", ConsoleColor.Gray);
            
            if (!string.IsNullOrEmpty(conv.Metadata?.Title))
            {
                ConsoleHelpers.WriteLine(conv.Metadata.Title, ConsoleColor.Cyan);
            }
            else
            {
                ConsoleHelpers.WriteLine(conv.Id, ConsoleColor.DarkCyan);
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
                for (int i = 0; i < messagesToShow.Count; i++)
                {
                    var color = i == 0 ? ConsoleColor.Green : ConsoleColor.DarkGreen;
                    ConsoleHelpers.WriteLine($"{indent}  > {messagesToShow[i]}", color);
                }
                
                // Show count of remaining messages
                var shownCount = messagesToShow.Count;
                if (userMessages.Count > shownCount)
                {
                    ConsoleHelpers.WriteLine($"{indent}  ... and {userMessages.Count - shownCount} more", ConsoleColor.DarkGray);
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
                    ConsoleHelpers.WriteLine($"{indent}  Tools: {toolSummary}", ConsoleColor.DarkCyan);
                }
                
                // Files modified
                var files = ContentSummarizer.GetFilesModified(conv);
                if (files.Count > 0)
                {
                    var fileList = files.Count <= 3 
                        ? string.Join(", ", files) 
                        : string.Join(", ", files.Take(3)) + $" +{files.Count - 3} more";
                    ConsoleHelpers.WriteLine($"{indent}  Files: {fileList}", ConsoleColor.DarkYellow);
                }
            }
            
            // Message count
            var msgCount = conv.Messages.Count(m => m.Role == "user" || m.Role == "assistant");
            ConsoleHelpers.WriteLine($"{indent}  ({msgCount} messages)", ConsoleColor.DarkGray);
            
            ConsoleHelpers.WriteLine();
        }
    }
}
