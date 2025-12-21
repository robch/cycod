using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CycoDj.CommandLineCommands
{
    public class ExportCommand : CommandLine.CycoDjCommand
    {
        public string? OutputFile { get; set; }
        public string? Date { get; set; }
        public int? Last { get; set; }
        public string? ConversationId { get; set; }
        public bool IncludeToolOutput { get; set; }
        public bool IncludeBranches { get; set; } = true;
        public bool Overwrite { get; set; }

        public override async Task<int> ExecuteAsync()
        {
            if (string.IsNullOrWhiteSpace(OutputFile))
            {
                ConsoleHelpers.WriteErrorLine("Output file is required. Use --output/-o to specify.");
                return 1;
            }

            // Check if file exists and overwrite not specified
            if (File.Exists(OutputFile) && !Overwrite)
            {
                ConsoleHelpers.WriteErrorLine($"File already exists: {OutputFile}");
                ConsoleHelpers.WriteLine("Use --overwrite to replace the existing file.", ConsoleColor.Yellow);
                return 1;
            }

            ConsoleHelpers.WriteLine($"## Exporting conversations to markdown: {OutputFile}", ConsoleColor.Cyan, overrideQuiet: true);
            ConsoleHelpers.WriteLine(overrideQuiet: true);

            // Find and parse conversations
            var historyDir = CycoDj.Helpers.HistoryFileHelpers.GetHistoryDirectory();
            var files = CycoDj.Helpers.HistoryFileHelpers.FindAllHistoryFiles();

            // Filter by specific conversation ID if provided
            if (!string.IsNullOrWhiteSpace(ConversationId))
            {
                files = files.Where(f => Path.GetFileNameWithoutExtension(f).Contains(ConversationId)).ToList();
            }
            // Filter by date if specified
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
                    ConsoleHelpers.WriteErrorLine($"Invalid date format: {Date}");
                    return 1;
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
                ConsoleHelpers.WriteLine("No conversations found matching the criteria.", ConsoleColor.Yellow, overrideQuiet: true);
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

            // Detect branches if needed
            if (IncludeBranches)
            {
                CycoDj.Analyzers.BranchDetector.DetectBranches(conversations);
            }

            // Generate markdown
            var markdown = GenerateMarkdown(conversations);
            
            // Apply instructions if provided
            markdown = ApplyInstructionsIfProvided(markdown);

            // Write to file
            try
            {
                await File.WriteAllTextAsync(OutputFile, markdown);
                ConsoleHelpers.WriteLine($"✓ Exported {conversations.Count} conversation(s) to {OutputFile}", 
                    ConsoleColor.Green, overrideQuiet: true);
                ConsoleHelpers.WriteLine($"  File size: {new FileInfo(OutputFile).Length / 1024} KB", 
                    ConsoleColor.DarkGray, overrideQuiet: true);
                return 0;
            }
            catch (Exception ex)
            {
                ConsoleHelpers.WriteErrorLine($"Failed to write file: {ex.Message}");
                Logger.Error($"Export failed: {ex}");
                return 1;
            }
        }

        private string GenerateMarkdown(List<Models.Conversation> conversations)
        {
            var sb = new StringBuilder();

            // Title and metadata
            sb.AppendLine("# Chat History Export");
            sb.AppendLine();
            sb.AppendLine($"**Generated:** {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
            sb.AppendLine($"**Conversations:** {conversations.Count}");
            
            if (!string.IsNullOrWhiteSpace(Date))
                sb.AppendLine($"**Date Filter:** {Date}");
            if (Last.HasValue)
                sb.AppendLine($"**Limit:** Last {Last} conversations");
            if (!string.IsNullOrWhiteSpace(ConversationId))
                sb.AppendLine($"**Conversation ID:** {ConversationId}");
                
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();

            // Table of contents
            sb.AppendLine("## Table of Contents");
            sb.AppendLine();
            
            for (int i = 0; i < conversations.Count; i++)
            {
                var conv = conversations[i];
                var title = conv.Metadata?.Title ?? $"Conversation {i + 1}";
                var timestamp = conv.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
                var anchor = $"conv-{i + 1}";
                
                var indent = "";
                if (IncludeBranches && !string.IsNullOrEmpty(conv.ParentId))
                {
                    indent = "  - ";
                }
                else
                {
                    indent = $"{i + 1}. ";
                }
                
                sb.AppendLine($"{indent}[{timestamp} - {title}](#{anchor})");
            }
            
            sb.AppendLine();
            sb.AppendLine("---");
            sb.AppendLine();

            // Conversations
            for (int i = 0; i < conversations.Count; i++)
            {
                var conv = conversations[i];
                AppendConversation(sb, conv, i + 1);
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private void AppendConversation(StringBuilder sb, Models.Conversation conversation, int index)
        {
            var title = conversation.Metadata?.Title ?? $"Conversation {index}";
            var timestamp = conversation.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");
            
            sb.AppendLine($"## <a name=\"conv-{index}\"></a>{index}. {title}");
            sb.AppendLine();
            sb.AppendLine($"**Time:** {timestamp}  ");
            sb.AppendLine($"**ID:** `{conversation.Id}`  ");
            sb.AppendLine($"**File:** `{Path.GetFileName(conversation.FilePath)}`  ");
            
            if (IncludeBranches && !string.IsNullOrEmpty(conversation.ParentId))
            {
                sb.AppendLine($"**Branched from:** `{conversation.ParentId}`  ");
            }
            
            var userMsgCount = conversation.Messages.Count(m => m.Role == "user");
            var assistantMsgCount = conversation.Messages.Count(m => m.Role == "assistant");
            var toolMsgCount = conversation.Messages.Count(m => m.Role == "tool");
            
            sb.AppendLine($"**Messages:** {conversation.Messages.Count} total ({userMsgCount} user, {assistantMsgCount} assistant, {toolMsgCount} tool)");
            sb.AppendLine();
            
            sb.AppendLine("---");
            sb.AppendLine();

            // Messages
            for (int i = 0; i < conversation.Messages.Count; i++)
            {
                var message = conversation.Messages[i];
                
                // Skip system messages
                if (message.Role == "system") continue;
                
                // Skip tool messages unless explicitly included
                if (message.Role == "tool" && !IncludeToolOutput) continue;

                AppendMessage(sb, message, i + 1);
                sb.AppendLine();
            }
        }

        private void AppendMessage(StringBuilder sb, Models.ChatMessage message, int index)
        {
            var roleIcon = message.Role switch
            {
                "user" => "👤",
                "assistant" => "🤖",
                "tool" => "🔧",
                _ => "💬"
            };

            sb.AppendLine($"### {roleIcon} {message.Role.ToUpperInvariant()} (Message #{index})");
            sb.AppendLine();

            if (!string.IsNullOrWhiteSpace(message.Content))
            {
                // Format content as blockquote for user messages
                if (message.Role == "user")
                {
                    var lines = message.Content.Split(new[] { '\r', '\n' }, StringSplitOptions.None);
                    foreach (var line in lines)
                    {
                        sb.AppendLine($"> {line}");
                    }
                }
                // Tool output in code block
                else if (message.Role == "tool")
                {
                    sb.AppendLine("```");
                    sb.AppendLine(message.Content);
                    sb.AppendLine("```");
                }
                // Assistant content as normal text
                else
                {
                    sb.AppendLine(message.Content);
                }
                sb.AppendLine();
            }

            // Tool calls
            if (message.ToolCalls != null && message.ToolCalls.Any())
            {
                sb.AppendLine("**Tool Calls:**");
                sb.AppendLine();
                
                foreach (var toolCall in message.ToolCalls)
                {
                    sb.AppendLine($"- `{toolCall.Function?.Name ?? "Unknown"}` (ID: `{toolCall.Id}`)");
                    
                    if (toolCall.Function?.Arguments != null)
                    {
                        var args = toolCall.Function.Arguments.ToString();
                        if (!string.IsNullOrWhiteSpace(args) && args.Length < 200)
                        {
                            sb.AppendLine($"  - Arguments: `{args}`");
                        }
                    }
                }
                sb.AppendLine();
            }

            // Tool call ID (for tool responses)
            if (!string.IsNullOrWhiteSpace(message.ToolCallId))
            {
                sb.AppendLine($"**In response to:** `{message.ToolCallId}`");
                sb.AppendLine();
            }
        }
    }
}
