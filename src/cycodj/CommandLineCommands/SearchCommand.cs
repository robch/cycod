using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace CycoDj.CommandLineCommands
{
    public class SearchCommand : CommandLine.CycoDjCommand
    {
        public string? Query { get; set; }
        public string? Date { get; set; }
        public int? Last { get; set; }
        public bool CaseSensitive { get; set; }
        public bool UseRegex { get; set; }
        public bool UserOnly { get; set; }
        public bool AssistantOnly { get; set; }
        public int ContextLines { get; set; } = 2;

        public override async System.Threading.Tasks.Task<int> ExecuteAsync()
        {
            var output = GenerateSearchOutput();
            
            // Apply instructions if provided
            var finalOutput = ApplyInstructionsIfProvided(output);
            ConsoleHelpers.WriteLine(finalOutput);
            
            return await System.Threading.Tasks.Task.FromResult(0);
        }
        
        private string GenerateSearchOutput()
        {
            var sb = new System.Text.StringBuilder();
            
            if (string.IsNullOrWhiteSpace(Query))
            {
                sb.AppendLine("ERROR: Search query is required.");
                return sb.ToString();
            }

            sb.AppendLine($"## Searching conversations for: \"{Query}\"");
            sb.AppendLine();

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
                    sb.AppendLine($"ERROR: Invalid date format: {Date}");
                    return sb.ToString();
                }
            }

            // Limit number of files if --last specified
            if (Last.HasValue && Last.Value > 0)
            {
                files = files.OrderByDescending(f => CycoDj.Helpers.TimestampHelpers.ParseTimestamp(f))
                    .Take(Last.Value)
                    .OrderBy(f => CycoDj.Helpers.TimestampHelpers.ParseTimestamp(f))
                    .ToList();
            }

            if (!files.Any())
            {
                sb.AppendLine("No conversations found matching the criteria.");
                return sb.ToString();
            }

            // Parse conversations and search
            var matches = new List<(Models.Conversation conversation, List<SearchMatch> searchMatches)>();
            
            foreach (var file in files)
            {
                try
                {
                    var conversation = CycoDj.Helpers.JsonlReader.ReadConversation(file);
                    if (conversation != null)
                    {
                        var conversationMatches = SearchConversation(conversation);
                        if (conversationMatches.Any())
                        {
                            matches.Add((conversation, conversationMatches));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warning($"Failed to search conversation {file}: {ex.Message}");
                }
            }

            // Display results
            if (!matches.Any())
            {
                sb.AppendLine("No matches found.");
                return sb.ToString();
            }

            sb.AppendLine($"Found {matches.Count} conversation(s) with matches:");
            sb.AppendLine();

            foreach (var (conversation, searchMatches) in matches)
            {
                AppendConversationMatches(sb, conversation, searchMatches);
            }

            sb.AppendLine();
            sb.AppendLine($"Total: {matches.Sum(m => m.searchMatches.Count)} match(es) in {matches.Count} conversation(s)");
            
            return sb.ToString();
        }

        private List<SearchMatch> SearchConversation(Models.Conversation conversation)
        {
            var matches = new List<SearchMatch>();
            
            for (int i = 0; i < conversation.Messages.Count; i++)
            {
                var message = conversation.Messages[i];
                
                // Filter by role if specified
                if (UserOnly && message.Role != "user") continue;
                if (AssistantOnly && message.Role != "assistant") continue;
                
                // Skip system messages unless explicitly searching all
                if (message.Role == "system" && (UserOnly || AssistantOnly)) continue;

                // Search in message content
                if (!string.IsNullOrWhiteSpace(message.Content))
                {
                    var messageMatches = SearchText(message.Content);
                    if (messageMatches.Any())
                    {
                        matches.Add(new SearchMatch
                        {
                            MessageIndex = i,
                            Message = message,
                            MatchedLines = messageMatches
                        });
                    }
                }
            }

            return matches;
        }

        private List<(int lineNumber, string line, int matchStart, int matchLength)> SearchText(string text)
        {
            var matches = new List<(int lineNumber, string line, int matchStart, int matchLength)>();
            var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.None);

            for (int lineNum = 0; lineNum < lines.Length; lineNum++)
            {
                var line = lines[lineNum];
                if (string.IsNullOrEmpty(line)) continue;

                if (UseRegex)
                {
                    try
                    {
                        var regexOptions = CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
                        var regex = new Regex(Query!, regexOptions);
                        var match = regex.Match(line);
                        if (match.Success)
                        {
                            matches.Add((lineNum, line, match.Index, match.Length));
                        }
                    }
                    catch (Exception ex)
                    {
                        ConsoleHelpers.WriteErrorLine($"Invalid regex pattern: {ex.Message}");
                        return matches;
                    }
                }
                else
                {
                    var comparison = CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
                    var index = line.IndexOf(Query!, comparison);
                    if (index >= 0)
                    {
                        matches.Add((lineNum, line, index, Query!.Length));
                    }
                }
            }

            return matches;
        }

        private void AppendConversationMatches(System.Text.StringBuilder sb, Models.Conversation conversation, List<SearchMatch> matches)
        {
            var title = conversation.Metadata?.Title ?? $"conversation-{conversation.Id}";
            var timestamp = conversation.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");

            sb.AppendLine($"### {timestamp} - {title}");
            sb.AppendLine($"    File: {conversation.FilePath}");
            sb.AppendLine($"    Matches: {matches.Count}");
            sb.AppendLine();

            foreach (var match in matches)
            {
                var role = match.Message.Role;
                sb.AppendLine($"  [{role}] Message #{match.MessageIndex + 1}");

                // Show matched lines with context
                var allLines = match.Message.Content.Split(new[] { '\r', '\n' }, StringSplitOptions.None);
                var matchedLineNumbers = match.MatchedLines.Select(m => m.lineNumber).Distinct().ToHashSet();

                for (int i = 0; i < allLines.Length; i++)
                {
                    var isMatch = matchedLineNumbers.Contains(i);
                    var showContext = matchedLineNumbers.Any(ln => Math.Abs(ln - i) <= ContextLines);

                    if (showContext || isMatch)
                    {
                        var prefix = isMatch ? "  > " : "    ";
                        var line = allLines[i];
                        sb.AppendLine(prefix + line);
                    }
                }

                sb.AppendLine();
            }
        }

        private class SearchMatch
        {
            public int MessageIndex { get; set; }
            public Models.ChatMessage Message { get; set; } = null!;
            public List<(int lineNumber, string line, int matchStart, int matchLength)> MatchedLines { get; set; } = new();
        }
    }
}
