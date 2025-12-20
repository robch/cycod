using System;
using System.Linq;
using System.Threading.Tasks;
using CycoDj.CommandLine;
using CycoDj.Helpers;

namespace CycoDj.CommandLineCommands;

public class ListCommand : CycoDjCommand
{
    public string? Date { get; set; }
    public int Last { get; set; } = 0;

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
        
        // Limit to last N if specified
        if (Last > 0 && files.Count > Last)
        {
            files = files.Take(Last).ToList();
            ConsoleHelpers.WriteLine($"Showing last {Last} conversations", ConsoleColor.Gray);
            ConsoleHelpers.WriteLine();
        }
        
        // Read and display conversations
        var conversations = JsonlReader.ReadConversations(files);
        
        if (conversations.Count == 0)
        {
            ConsoleHelpers.WriteWarning("No conversations could be read");
            return 1;
        }
        
        // Display conversations
        foreach (var conv in conversations)
        {
            var timestamp = TimestampHelpers.FormatTimestamp(conv.Timestamp, "datetime");
            var userCount = conv.Messages.Count(m => m.Role == "user");
            var assistantCount = conv.Messages.Count(m => m.Role == "assistant");
            var toolCount = conv.Messages.Count(m => m.Role == "tool");
            
            ConsoleHelpers.Write($"{timestamp}", ConsoleColor.White);
            ConsoleHelpers.Write($" - ", ConsoleColor.Gray);
            ConsoleHelpers.WriteLine($"{conv.Id}", ConsoleColor.Cyan);
            
            ConsoleHelpers.Write($"  Messages: ", ConsoleColor.Gray);
            ConsoleHelpers.Write($"{userCount} user", ConsoleColor.Green);
            ConsoleHelpers.Write($", ", ConsoleColor.Gray);
            ConsoleHelpers.Write($"{assistantCount} assistant", ConsoleColor.Blue);
            ConsoleHelpers.Write($", ", ConsoleColor.Gray);
            ConsoleHelpers.WriteLine($"{toolCount} tool", ConsoleColor.DarkGray);
            
            // Show first user message as preview if available
            var firstUserMsg = conv.Messages.FirstOrDefault(m => m.Role == "user");
            if (firstUserMsg != null && !string.IsNullOrWhiteSpace(firstUserMsg.Content))
            {
                var preview = firstUserMsg.Content.Length > 80 
                    ? firstUserMsg.Content.Substring(0, 80) + "..." 
                    : firstUserMsg.Content;
                preview = preview.Replace("\n", " ").Replace("\r", "");
                ConsoleHelpers.WriteLine($"  > {preview}", ConsoleColor.DarkGray);
            }
            
            ConsoleHelpers.WriteLine();
        }
        
        ConsoleHelpers.WriteLine($"Total: {conversations.Count} conversation(s)", ConsoleColor.Green);
        
        return await Task.FromResult(0);
    }
}
