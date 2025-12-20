using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CycoDj.Models;

namespace CycoDj.Analyzers;

/// <summary>
/// Analyzes and summarizes conversation content.
/// </summary>
public class ContentSummarizer
{
    /// <summary>
    /// Extracts user messages from a conversation.
    /// </summary>
    public static List<ChatMessage> GetUserMessages(Conversation conv, bool excludeLarge = true, int maxLength = 10000)
    {
        var userMessages = conv.Messages
            .Where(m => m.Role == "user")
            .ToList();

        if (excludeLarge)
        {
            userMessages = userMessages
                .Where(m => m.Content.Length <= maxLength)
                .ToList();
        }

        return userMessages;
    }

    /// <summary>
    /// Extracts assistant text responses (not tool outputs) from a conversation.
    /// </summary>
    public static List<ChatMessage> GetAssistantResponses(Conversation conv, bool excludeWithToolCalls = false)
    {
        var assistantMessages = conv.Messages
            .Where(m => m.Role == "assistant")
            .ToList();

        if (excludeWithToolCalls)
        {
            // Exclude messages that only have tool calls and no text content
            assistantMessages = assistantMessages
                .Where(m => !string.IsNullOrWhiteSpace(m.Content) || m.ToolCalls == null || m.ToolCalls.Count == 0)
                .ToList();
        }

        return assistantMessages;
    }

    /// <summary>
    /// Gets tool messages from a conversation.
    /// </summary>
    public static List<ChatMessage> GetToolMessages(Conversation conv)
    {
        return conv.Messages
            .Where(m => m.Role == "tool")
            .ToList();
    }

    /// <summary>
    /// Gets system messages from a conversation.
    /// </summary>
    public static List<ChatMessage> GetSystemMessages(Conversation conv)
    {
        return conv.Messages
            .Where(m => m.Role == "system")
            .ToList();
    }

    /// <summary>
    /// Filters messages by role.
    /// </summary>
    public static List<ChatMessage> FilterByRole(Conversation conv, string role)
    {
        return conv.Messages
            .Where(m => m.Role.Equals(role, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Checks if a tool message output is large and should be abbreviated.
    /// </summary>
    public static bool IsLargeToolOutput(ChatMessage msg, int threshold = 1000)
    {
        if (msg.Role != "tool")
            return false;

        return msg.Content.Length > threshold;
    }

    /// <summary>
    /// Abbreviates large tool output for display.
    /// </summary>
    public static string AbbreviateToolOutput(ChatMessage msg, int maxLines = 5)
    {
        if (msg.Role != "tool")
            return msg.Content;

        var lines = msg.Content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        
        if (lines.Length <= maxLines)
            return msg.Content;

        var abbreviated = new StringBuilder();
        for (int i = 0; i < maxLines; i++)
        {
            abbreviated.AppendLine(lines[i]);
        }
        abbreviated.AppendLine($"... ({lines.Length - maxLines} more lines)");

        return abbreviated.ToString();
    }

    /// <summary>
    /// Generates a summary of a conversation.
    /// </summary>
    public static string SummarizeConversation(Conversation conv, int maxLength = 200)
    {
        var userMessages = GetUserMessages(conv, excludeLarge: true, maxLength: 500);
        
        if (userMessages.Count == 0)
        {
            return "(No user messages)";
        }

        // Take the first user message as the primary topic
        var firstUserMsg = userMessages.First().Content;
        
        // Truncate if needed
        if (firstUserMsg.Length > maxLength)
        {
            return firstUserMsg.Substring(0, maxLength) + "...";
        }

        return firstUserMsg;
    }

    /// <summary>
    /// Generates a detailed summary with user actions and assistant responses.
    /// </summary>
    public static string SummarizeConversationDetailed(Conversation conv, int maxUserMessages = 5, int maxAssistantLines = 10)
    {
        var summary = new StringBuilder();
        
        var userMessages = GetUserMessages(conv, excludeLarge: true, maxLength: 500);
        var assistantMessages = GetAssistantResponses(conv, excludeWithToolCalls: false);
        var toolMessages = GetToolMessages(conv);

        summary.AppendLine($"Conversation: {conv.GetDisplayTitle()}");
        summary.AppendLine($"Started: {conv.Timestamp:yyyy-MM-dd HH:mm:ss}");
        summary.AppendLine($"Messages: {conv.Messages.Count} total ({userMessages.Count} user, {assistantMessages.Count} assistant, {toolMessages.Count} tool)");
        
        if (conv.BranchIds.Count > 0)
        {
            summary.AppendLine($"Branches: {conv.BranchIds.Count}");
        }

        summary.AppendLine();

        // User messages
        if (userMessages.Count > 0)
        {
            summary.AppendLine("User:");
            var messagesToShow = userMessages.Take(maxUserMessages);
            foreach (var msg in messagesToShow)
            {
                var content = msg.Content.Length > 100 
                    ? msg.Content.Substring(0, 100) + "..." 
                    : msg.Content;
                summary.AppendLine($"  > {content}");
            }
            
            if (userMessages.Count > maxUserMessages)
            {
                summary.AppendLine($"  ... and {userMessages.Count - maxUserMessages} more");
            }
            summary.AppendLine();
        }

        // Assistant summary
        if (assistantMessages.Count > 0)
        {
            summary.AppendLine("Assistant (summary):");
            
            // Collect assistant text (not tool calls)
            var assistantTexts = assistantMessages
                .Where(m => !string.IsNullOrWhiteSpace(m.Content))
                .Select(m => m.Content.Trim())
                .Where(c => !string.IsNullOrEmpty(c))
                .ToList();

            if (assistantTexts.Any())
            {
                var combinedText = string.Join(" ", assistantTexts);
                var lines = combinedText.Split(new[] { '\n', '\r', '.' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(l => l.Trim())
                    .Where(l => !string.IsNullOrEmpty(l) && l.Length > 10) // Filter out very short fragments
                    .Take(maxAssistantLines);

                foreach (var line in lines)
                {
                    var displayLine = line.Length > 80 ? line.Substring(0, 80) + "..." : line;
                    summary.AppendLine($"  - {displayLine}");
                }
            }
            else
            {
                summary.AppendLine("  - (Tool calls only, no text responses)");
            }
        }

        return summary.ToString();
    }

    /// <summary>
    /// Extracts a conversation title from metadata or content.
    /// </summary>
    public static string ExtractTitle(Conversation conv)
    {
        // First, check metadata
        if (!string.IsNullOrEmpty(conv.Metadata?.Title))
        {
            return conv.Metadata.Title;
        }

        // Fall back to first user message
        var userMessages = GetUserMessages(conv, excludeLarge: true, maxLength: 500);
        if (userMessages.Count > 0)
        {
            var firstMsg = userMessages.First().Content;
            
            // Take first line or first 50 characters
            var firstLine = firstMsg.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? firstMsg;
            
            if (firstLine.Length > 50)
            {
                return firstLine.Substring(0, 50) + "...";
            }
            
            return firstLine;
        }

        // Fall back to ID
        return conv.Id;
    }

    /// <summary>
    /// Gets message count statistics for a conversation.
    /// </summary>
    public static (int user, int assistant, int tool, int system) GetMessageCounts(Conversation conv)
    {
        var userCount = conv.Messages.Count(m => m.Role == "user");
        var assistantCount = conv.Messages.Count(m => m.Role == "assistant");
        var toolCount = conv.Messages.Count(m => m.Role == "tool");
        var systemCount = conv.Messages.Count(m => m.Role == "system");

        return (userCount, assistantCount, toolCount, systemCount);
    }

    /// <summary>
    /// Checks if a user message is likely piped content (heuristic).
    /// </summary>
    public static bool IsPossiblyPipedContent(ChatMessage msg, int lengthThreshold = 5000)
    {
        if (msg.Role != "user")
            return false;

        // Heuristics:
        // 1. Very long content
        if (msg.Content.Length > lengthThreshold)
            return true;

        // 2. Contains structured data patterns (JSON, XML, etc.)
        if (msg.Content.TrimStart().StartsWith("{") || msg.Content.TrimStart().StartsWith("["))
            return true;

        if (msg.Content.TrimStart().StartsWith("<") && msg.Content.Contains("</"))
            return true;

        // 3. Contains many code markers
        if (msg.Content.Split(new[] { "```" }, StringSplitOptions.None).Length > 3)
            return true;

        return false;
    }
}
