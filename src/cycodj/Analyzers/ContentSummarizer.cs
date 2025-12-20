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
    /// Extracts user message content as strings.
    /// </summary>
    public static List<string> GetUserMessages(Conversation conv, bool excludeLarge = true, int maxLength = 10000)
    {
        if (conv == null) throw new ArgumentNullException(nameof(conv));
        if (conv.Messages == null) return new List<string>();

        var userMessages = conv.Messages
            .Where(m => m?.Role == "user" && m.Content != null)
            .Select(m => m.Content)
            .ToList();

        if (excludeLarge)
        {
            userMessages = userMessages
                .Where(c => c.Length <= maxLength)
                .ToList();
        }

        return userMessages;
    }

    /// <summary>
    /// Extracts user messages as ChatMessage objects (for accessing full message data).
    /// </summary>
    public static List<ChatMessage> GetUserMessagesRaw(Conversation conv, bool excludeLarge = true, int maxLength = 10000)
    {
        if (conv == null) throw new ArgumentNullException(nameof(conv));
        if (conv.Messages == null) return new List<ChatMessage>();

        var userMessages = conv.Messages
            .Where(m => m?.Role == "user" && m.Content != null)
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
    /// Extracts assistant text responses as strings (not tool outputs).
    /// </summary>
    public static List<string> GetAssistantResponses(Conversation conv, bool abbreviate = true, int maxLength = 500)
    {
        if (conv == null) throw new ArgumentNullException(nameof(conv));
        if (conv.Messages == null) return new List<string>();

        var assistantMessages = conv.Messages
            .Where(m => m?.Role == "assistant" && !string.IsNullOrWhiteSpace(m.Content))
            .Select(m => m.Content)
            .ToList();

        if (abbreviate)
        {
            assistantMessages = assistantMessages
                .Select(c => c.Length > maxLength ? c.Substring(0, maxLength) + "..." : c)
                .ToList();
        }

        return assistantMessages;
    }

    /// <summary>
    /// Extracts assistant messages as ChatMessage objects (for accessing tool calls, etc).
    /// </summary>
    public static List<ChatMessage> GetAssistantMessagesRaw(Conversation conv, bool excludeWithToolCallsOnly = false)
    {
        if (conv == null) throw new ArgumentNullException(nameof(conv));
        if (conv.Messages == null) return new List<ChatMessage>();

        var assistantMessages = conv.Messages
            .Where(m => m?.Role == "assistant")
            .ToList();

        if (excludeWithToolCallsOnly)
        {
            // Exclude messages that only have tool calls and no text content
            assistantMessages = assistantMessages
                .Where(m => !string.IsNullOrWhiteSpace(m.Content))
                .ToList();
        }

        return assistantMessages;
    }

    /// <summary>
    /// Gets tool messages from a conversation.
    /// </summary>
    public static List<ChatMessage> GetToolMessages(Conversation conv)
    {
        if (conv == null) throw new ArgumentNullException(nameof(conv));
        if (conv.Messages == null) return new List<ChatMessage>();

        return conv.Messages
            .Where(m => m?.Role == "tool")
            .ToList();
    }

    /// <summary>
    /// Gets system messages from a conversation.
    /// </summary>
    public static List<ChatMessage> GetSystemMessages(Conversation conv)
    {
        if (conv == null) throw new ArgumentNullException(nameof(conv));
        if (conv.Messages == null) return new List<ChatMessage>();

        return conv.Messages
            .Where(m => m?.Role == "system")
            .ToList();
    }

    /// <summary>
    /// Filters messages by role.
    /// </summary>
    public static List<ChatMessage> FilterByRole(Conversation conv, string role)
    {
        if (conv == null) throw new ArgumentNullException(nameof(conv));
        if (string.IsNullOrEmpty(role)) throw new ArgumentNullException(nameof(role));
        if (conv.Messages == null) return new List<ChatMessage>();

        return conv.Messages
            .Where(m => m != null && m.Role.Equals(role, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Extracts tool call information from assistant messages.
    /// </summary>
    public static List<(string toolName, string toolCallId)> GetToolCallsInvoked(Conversation conv)
    {
        if (conv == null) throw new ArgumentNullException(nameof(conv));
        if (conv.Messages == null) return new List<(string, string)>();

        var toolCalls = new List<(string, string)>();

        foreach (var msg in conv.Messages.Where(m => m?.Role == "assistant"))
        {
            if (msg.ToolCalls != null && msg.ToolCalls.Count > 0)
            {
                foreach (var toolCall in msg.ToolCalls)
                {
                    if (toolCall == null) continue;
                    var toolName = toolCall.Function?.Name ?? "unknown";
                    toolCalls.Add((toolName, toolCall.Id ?? "unknown"));
                }
            }
        }

        return toolCalls;
    }

    /// <summary>
    /// Gets a summary of actions taken (tool calls and their results).
    /// </summary>
    public static List<string> GetActionSummary(Conversation conv, int maxToolOutputLength = 100)
    {
        if (conv == null) throw new ArgumentNullException(nameof(conv));
        if (conv.Messages == null) return new List<string>();

        var actions = new List<string>();
        var toolCalls = GetToolCallsInvoked(conv);
        
        foreach (var (toolName, toolCallId) in toolCalls)
        {
            // Find the corresponding tool result
            var toolResult = conv.Messages
                .Where(m => m?.Role == "tool" && m.ToolCallId == toolCallId)
                .FirstOrDefault();

            if (toolResult != null && toolResult.Content != null)
            {
                var result = toolResult.Content.Length > maxToolOutputLength 
                    ? toolResult.Content.Substring(0, maxToolOutputLength) + "..." 
                    : toolResult.Content;
                
                // Extract just the first line or main result
                var firstLine = result.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? result;
                actions.Add($"{toolName}: {firstLine}");
            }
            else
            {
                actions.Add($"{toolName}: (no result)");
            }
        }

        return actions;
    }

    /// <summary>
    /// Checks if a tool message output is large and should be abbreviated.
    /// </summary>
    public static bool IsLargeToolOutput(ChatMessage msg, int threshold = 1000)
    {
        if (msg == null) return false;
        if (msg.Role != "tool") return false;
        if (msg.Content == null) return false;

        return msg.Content.Length > threshold;
    }

    /// <summary>
    /// Abbreviates large tool output for display.
    /// </summary>
    public static string AbbreviateToolOutput(ChatMessage msg, int maxLines = 5)
    {
        if (msg == null) return string.Empty;
        if (msg.Content == null) return string.Empty;
        if (msg.Role != "tool") return msg.Content;

        var lines = msg.Content.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        
        if (lines.Length <= maxLines)
            return msg.Content;

        var abbreviated = new StringBuilder();
        for (int i = 0; i < maxLines && i < lines.Length; i++)
        {
            abbreviated.AppendLine(lines[i]);
        }
        abbreviated.AppendLine($"... ({lines.Length - maxLines} more lines)");

        return abbreviated.ToString();
    }

    /// <summary>
    /// Generates a brief summary of a conversation (as specified in architecture).
    /// </summary>
    public static string Summarize(Conversation conv, int maxLength = 200)
    {
        if (conv == null) throw new ArgumentNullException(nameof(conv));
        
        var userMessages = GetUserMessages(conv, excludeLarge: true, maxLength: 500);
        
        if (userMessages.Count == 0)
        {
            return "(No user messages)";
        }

        // Take the first user message as the primary topic
        var firstUserMsg = userMessages.First();
        
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
    public static string SummarizeDetailed(Conversation conv, int maxUserMessages = 5, int maxAssistantResponses = 5, int maxActions = 10)
    {
        if (conv == null) throw new ArgumentNullException(nameof(conv));
        
        var summary = new StringBuilder();
        
        var userMessages = GetUserMessages(conv, excludeLarge: true, maxLength: 500);
        var assistantResponses = GetAssistantResponses(conv, abbreviate: true, maxLength: 200);
        var actions = GetActionSummary(conv, maxToolOutputLength: 100);
        var toolMessages = GetToolMessages(conv);

        summary.AppendLine($"Conversation: {conv.GetDisplayTitle()}");
        summary.AppendLine($"Started: {conv.Timestamp:yyyy-MM-dd HH:mm:ss}");
        summary.AppendLine($"Messages: {(conv.Messages?.Count ?? 0)} total ({userMessages.Count} user, {assistantResponses.Count} assistant, {toolMessages.Count} tool)");
        
        if (conv.BranchIds != null && conv.BranchIds.Count > 0)
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
                var content = msg.Length > 100 ? msg.Substring(0, 100) + "..." : msg;
                summary.AppendLine($"  > {content}");
            }
            
            if (userMessages.Count > maxUserMessages)
            {
                summary.AppendLine($"  ... and {userMessages.Count - maxUserMessages} more");
            }
            summary.AppendLine();
        }

        // Actions taken (tool calls with results)
        if (actions.Count > 0)
        {
            summary.AppendLine("Actions:");
            var actionsToShow = actions.Take(maxActions);
            foreach (var action in actionsToShow)
            {
                summary.AppendLine($"  - {action}");
            }
            
            if (actions.Count > maxActions)
            {
                summary.AppendLine($"  ... and {actions.Count - maxActions} more");
            }
            summary.AppendLine();
        }

        // Assistant responses (text only)
        if (assistantResponses.Count > 0)
        {
            summary.AppendLine("Assistant responses:");
            var responsesToShow = assistantResponses
                .Where(r => !string.IsNullOrWhiteSpace(r) && r.Length > 10) // Filter very short responses
                .Take(maxAssistantResponses);

            foreach (var response in responsesToShow)
            {
                var displayResponse = response.Length > 80 ? response.Substring(0, 80) + "..." : response;
                summary.AppendLine($"  - {displayResponse}");
            }
        }

        return summary.ToString();
    }

    /// <summary>
    /// Extracts a conversation title from metadata or content.
    /// </summary>
    public static string ExtractTitle(Conversation conv)
    {
        if (conv == null) throw new ArgumentNullException(nameof(conv));
        
        // First, check metadata
        if (!string.IsNullOrEmpty(conv.Metadata?.Title))
        {
            return conv.Metadata.Title;
        }

        // Fall back to first user message
        var userMessages = GetUserMessages(conv, excludeLarge: true, maxLength: 500);
        if (userMessages.Count > 0)
        {
            var firstMsg = userMessages.First();
            
            // Take first line or first 50 characters
            var firstLine = firstMsg.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? firstMsg;
            
            if (firstLine.Length > 50)
            {
                return firstLine.Substring(0, 50) + "...";
            }
            
            return firstLine;
        }

        // Fall back to ID (ensure never null)
        return conv.Id ?? "(Untitled)";
    }

    /// <summary>
    /// Gets message count statistics for a conversation.
    /// </summary>
    public static (int user, int assistant, int tool, int system) GetMessageCounts(Conversation conv)
    {
        if (conv == null) throw new ArgumentNullException(nameof(conv));
        if (conv.Messages == null) return (0, 0, 0, 0);

        var userCount = conv.Messages.Count(m => m?.Role == "user");
        var assistantCount = conv.Messages.Count(m => m?.Role == "assistant");
        var toolCount = conv.Messages.Count(m => m?.Role == "tool");
        var systemCount = conv.Messages.Count(m => m?.Role == "system");

        return (userCount, assistantCount, toolCount, systemCount);
    }

    /// <summary>
    /// Checks if a user message is likely piped content (heuristic).
    /// </summary>
    public static bool IsPossiblyPipedContent(ChatMessage msg, int lengthThreshold = 5000)
    {
        if (msg == null) return false;
        if (msg.Role != "user") return false;
        if (msg.Content == null) return false;

        // Heuristics:
        // 1. Very long content
        if (msg.Content.Length > lengthThreshold)
            return true;

        // 2. Contains structured data patterns (JSON, XML, etc.)
        var trimmed = msg.Content.TrimStart();
        if (trimmed.StartsWith("{") || trimmed.StartsWith("["))
            return true;

        if (trimmed.StartsWith("<") && msg.Content.Contains("</"))
            return true;

        // 3. Contains many code markers
        if (msg.Content.Split(new[] { "```" }, StringSplitOptions.None).Length > 3)
            return true;

        return false;
    }

    /// <summary>
    /// Gets a count of tool calls by tool name.
    /// </summary>
    public static Dictionary<string, int> GetToolCallStatistics(Conversation conv)
    {
        if (conv == null) throw new ArgumentNullException(nameof(conv));
        if (conv.Messages == null) return new Dictionary<string, int>();
        
        var stats = new Dictionary<string, int>();
        
        foreach (var msg in conv.Messages.Where(m => m?.Role == "assistant"))
        {
            if (msg.ToolCalls != null)
            {
                foreach (var toolCall in msg.ToolCalls)
                {
                    if (toolCall == null) continue;
                    var toolName = toolCall.Function?.Name ?? "unknown";
                    if (!stats.ContainsKey(toolName))
                    {
                        stats[toolName] = 0;
                    }
                    stats[toolName]++;
                }
            }
        }

        return stats;
    }
}
