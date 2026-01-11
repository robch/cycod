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
            var output = GenerateStatsOutput();
            
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
        
        private string GenerateStatsOutput()
        {
            var sb = new System.Text.StringBuilder();
            
            sb.AppendLine("## Chat History Statistics");
            sb.AppendLine();

            // Find and parse conversations
            var historyDir = CycoDj.Helpers.HistoryFileHelpers.GetHistoryDirectory();
            var files = CycoDj.Helpers.HistoryFileHelpers.FindAllHistoryFiles();

            // Filter by time range if After/Before are set
            if (After.HasValue || Before.HasValue)
            {
                files = CycoDj.Helpers.HistoryFileHelpers.FilterByDateRange(files, After, Before);
            }
            // Filter by date if specified (backward compat)
            else if (!string.IsNullOrWhiteSpace(Date))
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
                    sb.AppendLine($"ERROR: Invalid date format: {Date}");
                    return sb.ToString();
                }
            }

            // Limit number of files if --last specified (as count)
            if (Last.HasValue && Last.Value > 0)
            {
                files = files.OrderByDescending(f => CycoDj.Helpers.TimestampHelpers.ParseTimestamp(f))
                    .Take(Last.Value)
                    .ToList();
            }

            if (!files.Any())
            {
                sb.AppendLine("No conversations found.");
                return sb.ToString();
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
            AppendOverallStats(sb, conversations);
            
            if (ShowDates)
            {
                sb.AppendLine();
                AppendDateStats(sb, conversations);
            }

            if (ShowTools)
            {
                sb.AppendLine();
                AppendToolUsageStats(sb, conversations);
            }

            return sb.ToString();
        }

        private void AppendOverallStats(System.Text.StringBuilder sb, List<Models.Conversation> conversations)
        {
            sb.AppendLine("### Overall Statistics");
            sb.AppendLine();

            var totalConversations = conversations.Count;
            var totalMessages = conversations.Sum(c => c.Messages.Count);
            var totalUserMessages = conversations.Sum(c => c.Messages.Count(m => m.Role == "user"));
            var totalAssistantMessages = conversations.Sum(c => c.Messages.Count(m => m.Role == "assistant"));
            var totalToolMessages = conversations.Sum(c => c.Messages.Count(m => m.Role == "tool"));

            var avgMessagesPerConv = totalMessages / (double)totalConversations;
            var avgUserPerConv = totalUserMessages / (double)totalConversations;

            sb.AppendLine($"**Conversations:** {totalConversations:#,##0}");
            sb.AppendLine($"**Total Messages:** {totalMessages:#,##0}");
            sb.AppendLine($"  - User: {totalUserMessages:#,##0} ({totalUserMessages * 100.0 / totalMessages:F1}%)");
            sb.AppendLine($"  - Assistant: {totalAssistantMessages:#,##0} ({totalAssistantMessages * 100.0 / totalMessages:F1}%)");
            sb.AppendLine($"  - Tool: {totalToolMessages:#,##0} ({totalToolMessages * 100.0 / totalMessages:F1}%)");
            sb.AppendLine();
            sb.AppendLine($"**Average per conversation:**");
            sb.AppendLine($"  - Messages: {avgMessagesPerConv:F1}");
            sb.AppendLine($"  - User messages: {avgUserPerConv:F1}");

            // Find longest conversation
            var longest = conversations.OrderByDescending(c => c.Messages.Count).First();
            sb.AppendLine();
            sb.AppendLine($"**Longest conversation:** {longest.Messages.Count} messages");
            sb.AppendLine($"  {longest.Timestamp:yyyy-MM-dd HH:mm:ss} - {longest.Metadata?.Title ?? longest.Id}");
        }

        private void AppendDateStats(System.Text.StringBuilder sb, List<Models.Conversation> conversations)
        {
            sb.AppendLine("### Activity by Date");
            sb.AppendLine();

            var byDate = conversations
                .GroupBy(c => c.Timestamp.Date)
                .OrderByDescending(g => g.Key)
                .Take(10)
                .ToList();

            sb.AppendLine($"{"Date",-12} {"Convs",6} {"Msgs",7} {"User",6} {"Asst",6} {"Tool",6}");
            sb.AppendLine(new string('-', 50));

            foreach (var group in byDate)
            {
                var convCount = group.Count();
                var msgCount = group.Sum(c => c.Messages.Count);
                var userCount = group.Sum(c => c.Messages.Count(m => m.Role == "user"));
                var asstCount = group.Sum(c => c.Messages.Count(m => m.Role == "assistant"));
                var toolCount = group.Sum(c => c.Messages.Count(m => m.Role == "tool"));

                var dateStr = group.Key.ToString("yyyy-MM-dd");
                sb.AppendLine($"{dateStr,-12} {convCount,6} {msgCount,7} {userCount,6} {asstCount,6} {toolCount,6}");
            }
        }

        private void AppendToolUsageStats(System.Text.StringBuilder sb, List<Models.Conversation> conversations)
        {
            sb.AppendLine("### Tool Usage Statistics");
            sb.AppendLine();

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
                sb.AppendLine("No tool usage found.");
                return;
            }

            var totalToolCalls = toolCalls.Values.Sum();
            sb.AppendLine($"**Total tool calls:** {totalToolCalls:#,##0}");
            sb.AppendLine();

            sb.AppendLine($"{"Tool Name",-40} {"Count",8} {"%",6}");
            sb.AppendLine(new string('-', 56));

            foreach (var tool in toolCalls.OrderByDescending(kv => kv.Value).Take(20))
            {
                var percentage = tool.Value * 100.0 / totalToolCalls;
                sb.AppendLine($"{tool.Key,-40} {tool.Value,8:#,##0} {percentage,5:F1}%");
            }
        }
    }
}
