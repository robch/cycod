using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CycoDj.CommandLineCommands
{
    public class StatsCommand : CommandLine.CycoDjCommand
    {
        public string? Date { get; set; }
        public int? Last { get; set; }
        public bool ShowTools { get; set; }
        public bool ShowDates { get; set; } = true;

        public override async Task<int> ExecuteAsync()
        {
            ConsoleHelpers.WriteLine("## Chat History Statistics", ConsoleColor.Cyan, overrideQuiet: true);
            ConsoleHelpers.WriteLine(overrideQuiet: true);

            // Find and parse conversations
            var historyDir = CycoDj.Helpers.HistoryFileHelpers.GetHistoryDirectory();
            var files = CycoDj.Helpers.HistoryFileHelpers.FindAllHistoryFiles();

            // Filter by date if specified
            if (!string.IsNullOrWhiteSpace(Date))
            {
                if (Date.ToLowerInvariant() == "today")
                {
                    files = CycoDj.Helpers.HistoryFileHelpers.FilterByDate(files, DateTime.Today);
                }
                else if (DateTime.TryParse(Date, out var targetDate))
                {
                    files = CycoDj.Helpers.HistoryFileHelpers.FilterByDate(files, targetDate);
                }
                else
                {
                    ConsoleHelpers.WriteErrorLine($"Invalid date format: {Date}");
                    return 1;
                }
            }

            // Limit number of files if --last specified
            if (Last.HasValue && Last.Value > 0)
            {
                files = files.OrderByDescending(f => CycoDj.Helpers.TimestampHelpers.ParseTimestamp(f))
                    .Take(Last.Value)
                    .ToList();
            }

            if (!files.Any())
            {
                ConsoleHelpers.WriteLine("No conversations found.", ConsoleColor.Yellow, overrideQuiet: true);
                return 0;
            }

            // Parse conversations
            var conversations = new List<Models.Conversation>();
            foreach (var file in files)
            {
                try
                {
                    var conversation = CycoDj.Helpers.JsonlReader.ReadConversation(file);
                    if (conversation != null)
                    {
                        conversations.Add(conversation);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warning($"Failed to load conversation {file}: {ex.Message}");
                }
            }

            // Calculate statistics
            DisplayOverallStats(conversations);
            
            if (ShowDates)
            {
                ConsoleHelpers.WriteLine(overrideQuiet: true);
                DisplayDateStats(conversations);
            }

            if (ShowTools)
            {
                ConsoleHelpers.WriteLine(overrideQuiet: true);
                DisplayToolUsageStats(conversations);
            }

            return 0;
        }

        private void DisplayOverallStats(List<Models.Conversation> conversations)
        {
            ConsoleHelpers.WriteLine("### Overall Statistics", ConsoleColor.White, overrideQuiet: true);
            ConsoleHelpers.WriteLine(overrideQuiet: true);

            var totalConversations = conversations.Count;
            var totalMessages = conversations.Sum(c => c.Messages.Count);
            var totalUserMessages = conversations.Sum(c => c.Messages.Count(m => m.Role == "user"));
            var totalAssistantMessages = conversations.Sum(c => c.Messages.Count(m => m.Role == "assistant"));
            var totalToolMessages = conversations.Sum(c => c.Messages.Count(m => m.Role == "tool"));

            var avgMessagesPerConv = totalMessages / (double)totalConversations;
            var avgUserPerConv = totalUserMessages / (double)totalConversations;

            ConsoleHelpers.WriteLine($"**Conversations:** {totalConversations:#,##0}", overrideQuiet: true);
            ConsoleHelpers.WriteLine($"**Total Messages:** {totalMessages:#,##0}", overrideQuiet: true);
            ConsoleHelpers.WriteLine($"  - User: {totalUserMessages:#,##0} ({totalUserMessages * 100.0 / totalMessages:F1}%)", 
                ConsoleColor.Green, overrideQuiet: true);
            ConsoleHelpers.WriteLine($"  - Assistant: {totalAssistantMessages:#,##0} ({totalAssistantMessages * 100.0 / totalMessages:F1}%)", 
                ConsoleColor.Cyan, overrideQuiet: true);
            ConsoleHelpers.WriteLine($"  - Tool: {totalToolMessages:#,##0} ({totalToolMessages * 100.0 / totalMessages:F1}%)", 
                ConsoleColor.Gray, overrideQuiet: true);
            ConsoleHelpers.WriteLine(overrideQuiet: true);
            ConsoleHelpers.WriteLine($"**Average per conversation:**", overrideQuiet: true);
            ConsoleHelpers.WriteLine($"  - Messages: {avgMessagesPerConv:F1}", overrideQuiet: true);
            ConsoleHelpers.WriteLine($"  - User messages: {avgUserPerConv:F1}", overrideQuiet: true);

            // Find longest conversation
            var longest = conversations.OrderByDescending(c => c.Messages.Count).First();
            ConsoleHelpers.WriteLine(overrideQuiet: true);
            ConsoleHelpers.WriteLine($"**Longest conversation:** {longest.Messages.Count} messages", overrideQuiet: true);
            ConsoleHelpers.WriteLine($"  {longest.Timestamp:yyyy-MM-dd HH:mm:ss} - {longest.Metadata?.Title ?? longest.Id}", 
                ConsoleColor.DarkGray, overrideQuiet: true);
        }

        private void DisplayDateStats(List<Models.Conversation> conversations)
        {
            ConsoleHelpers.WriteLine("### Activity by Date", ConsoleColor.White, overrideQuiet: true);
            ConsoleHelpers.WriteLine(overrideQuiet: true);

            var byDate = conversations
                .GroupBy(c => c.Timestamp.Date)
                .OrderByDescending(g => g.Key)
                .Take(10)
                .ToList();

            ConsoleHelpers.WriteLine($"{"Date",-12} {"Convs",6} {"Msgs",7} {"User",6} {"Asst",6} {"Tool",6}", 
                ConsoleColor.DarkGray, overrideQuiet: true);
            ConsoleHelpers.WriteLine(new string('-', 50), ConsoleColor.DarkGray, overrideQuiet: true);

            foreach (var group in byDate)
            {
                var convCount = group.Count();
                var msgCount = group.Sum(c => c.Messages.Count);
                var userCount = group.Sum(c => c.Messages.Count(m => m.Role == "user"));
                var asstCount = group.Sum(c => c.Messages.Count(m => m.Role == "assistant"));
                var toolCount = group.Sum(c => c.Messages.Count(m => m.Role == "tool"));

                var dateStr = group.Key.ToString("yyyy-MM-dd");
                var isToday = group.Key == DateTime.Today;

                if (isToday)
                {
                    ConsoleHelpers.Write($"{dateStr,-12} ", ConsoleColor.Yellow, overrideQuiet: true);
                }
                else
                {
                    ConsoleHelpers.Write($"{dateStr,-12} ", overrideQuiet: true);
                }

                ConsoleHelpers.WriteLine($"{convCount,6} {msgCount,7} {userCount,6} {asstCount,6} {toolCount,6}", 
                    overrideQuiet: true);
            }
        }

        private void DisplayToolUsageStats(List<Models.Conversation> conversations)
        {
            ConsoleHelpers.WriteLine("### Tool Usage Statistics", ConsoleColor.White, overrideQuiet: true);
            ConsoleHelpers.WriteLine(overrideQuiet: true);

            // Collect all tool calls
            var toolCalls = new Dictionary<string, int>();
            foreach (var conversation in conversations)
            {
                foreach (var message in conversation.Messages)
                {
                    if (message.ToolCalls != null)
                    {
                        foreach (var toolCall in message.ToolCalls)
                        {
                            var toolName = toolCall.Function?.Name ?? "Unknown";
                            toolCalls[toolName] = toolCalls.GetValueOrDefault(toolName, 0) + 1;
                        }
                    }
                }
            }

            if (!toolCalls.Any())
            {
                ConsoleHelpers.WriteLine("No tool usage found.", ConsoleColor.DarkGray, overrideQuiet: true);
                return;
            }

            var totalToolCalls = toolCalls.Values.Sum();
            ConsoleHelpers.WriteLine($"**Total tool calls:** {totalToolCalls:#,##0}", overrideQuiet: true);
            ConsoleHelpers.WriteLine(overrideQuiet: true);

            ConsoleHelpers.WriteLine($"{"Tool Name",-40} {"Count",8} {"%",6}", 
                ConsoleColor.DarkGray, overrideQuiet: true);
            ConsoleHelpers.WriteLine(new string('-', 56), ConsoleColor.DarkGray, overrideQuiet: true);

            foreach (var tool in toolCalls.OrderByDescending(kv => kv.Value).Take(20))
            {
                var percentage = tool.Value * 100.0 / totalToolCalls;
                ConsoleHelpers.WriteLine($"{tool.Key,-40} {tool.Value,8:#,##0} {percentage,5:F1}%", 
                    overrideQuiet: true);
            }
        }
    }
}
