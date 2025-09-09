using Microsoft.Extensions.AI;
using System.Text;
using System.Text.Json;

/// <summary>
/// Helper class for compacting chat history using AI-based summarization.
/// </summary>
public static class ChatHistoryCompactionHelper
{
    /// <summary>
    /// Default number of recent messages to preserve during compaction.
    /// </summary>
    public const int DefaultPreserveMessages = 4;
    
    /// <summary>
    /// Default maximum token target for compacted history.
    /// </summary>
    public const int DefaultMaxTokenTarget = 16000;
    
    /// <summary>
    /// Compaction modes for chat history.
    /// </summary>
    public enum CompactionMode
    {
        /// <summary>
        /// No compaction, use traditional trimming instead.
        /// </summary>
        None,
        
        /// <summary>
        /// Simple summarization with basic context preservation.
        /// </summary>
        Simple,
        
        /// <summary>
        /// Full detailed summarization with structured sections.
        /// </summary>
        Full
    }
    
    /// <summary>
    /// Parses a string compaction mode into the enum value.
    /// </summary>
    public static CompactionMode ParseCompactionMode(string? mode)
    {
        if (string.IsNullOrEmpty(mode))
            return CompactionMode.None;
            
        return mode.ToLowerInvariant() switch
        {
            "full" => CompactionMode.Full,
            "simple" => CompactionMode.Simple,
            "none" => CompactionMode.None,
            _ => CompactionMode.Full // Default to full for unrecognized values
        };
    }
    
    /// <summary>
    /// Attempts to trim messages to target token limits using both trimming and compaction if needed.
    /// </summary>
    /// <param name="messages">The messages to trim.</param>
    /// <param name="maxPromptTarget">Maximum prompt token target.</param>
    /// <param name="maxToolTarget">Maximum tool token target.</param>
    /// <param name="maxChatTarget">Maximum chat token target.</param>
    /// <param name="compactionMode">The compaction mode to use if trimming is insufficient.</param>
    /// <param name="preserveLastMessages">Number of recent messages to preserve intact.</param>
    /// <returns>True if messages were successfully managed within token limits.</returns>
    public static async Task<bool> TryTrimToTargetWithCompactionAsync(
        IList<ChatMessage> messages,
        int maxPromptTarget,
        int maxToolTarget,
        int maxChatTarget,
        CompactionMode compactionMode,
        int preserveLastMessages = DefaultPreserveMessages)
    {
        // Check if we need to do anything
        if (!messages.IsTooBig(maxChatTarget))
        {
            return true; // Already under token limit
        }
        
        // Try traditional trimming first
        bool trimmed = messages.TryTrimToTarget(maxPromptTarget, maxToolTarget, maxChatTarget);
        
        // If trimming was sufficient or compaction is disabled, return the trimming result
        if (trimmed || compactionMode == CompactionMode.None)
        {
            return trimmed;
        }
        
        // If we're still over the token limit, use compaction
        return await CompactChatHistoryAsync(
            messages, 
            compactionMode, 
            maxChatTarget, 
            preserveLastMessages,
            maxPromptTarget,
            maxToolTarget);
    }
    
    /// <summary>
    /// Compacts the chat history using AI-based summarization.
    /// </summary>
    /// <param name="messages">The chat messages to compact.</param>
    /// <param name="mode">The compaction mode (None, Simple, or Full).</param>
    /// <param name="maxTokenTarget">The maximum token target for the compacted history.</param>
    /// <param name="preserveLastMessages">Number of recent messages to preserve intact.</param>
    /// <param name="maxPromptTokenTarget">Maximum prompt token target (0 means use maxTokenTarget).</param>
    /// <param name="maxToolTokenTarget">Maximum tool token target (0 means use maxTokenTarget).</param>
    /// <returns>True if compaction was performed, false otherwise.</returns>
    public static async Task<bool> CompactChatHistoryAsync(
        IList<ChatMessage> messages, 
        CompactionMode mode = CompactionMode.Full, 
        int maxTokenTarget = DefaultMaxTokenTarget,
        int preserveLastMessages = DefaultPreserveMessages,
        int maxPromptTokenTarget = 0,
        int maxToolTokenTarget = 0)
    {
        // Skip compaction if mode is None or if there aren't enough messages
        if (mode == CompactionMode.None || messages == null || messages.Count <= 2)
        {
            return false; // No compaction to perform
        }
        
        // Ensure we have a valid number of messages to preserve
        preserveLastMessages = Math.Max(1, Math.Min(preserveLastMessages, messages.Count - 2));
        
        // Check if the chat history is too big and needs compaction
        if (!messages.IsTooBig(maxTokenTarget))
        {
            return false; // No need to compact
        }
        
        // The system message (typically the first message) should be preserved
        var systemMessage = messages.FirstOrDefault(m => m.Role == ChatRole.System);
        if (systemMessage == null)
        {
            // If no system message found, we'll consider the first message as one to preserve
            systemMessage = messages[0];
        }
        
        // Get the last N messages to preserve
        var messagesToPreserve = messages
            .Skip(messages.Count - preserveLastMessages)
            .ToList();
        
        // Calculate which messages will be summarized
        var messagesToSummarize = messages
            .Skip(messages.IndexOf(systemMessage) + 1)
            .Take(messages.Count - preserveLastMessages - 1)
            .ToList();
        
        if (messagesToSummarize.Count == 0)
        {
            return false; // No messages to summarize
        }
        
        ConsoleHelpers.WriteDebugLine($"Compacting chat history with {messagesToSummarize.Count} messages using {mode} mode...");
        ConsoleHelpers.WriteLine("\n[Compacting conversation history using AI...]", ConsoleColor.DarkYellow);
        
        // Create a chat client for summarization
        var chatClient = ChatClientFactory.CreateChatClient(out var options);
        
        // Build the prompt for summarization
        var summarizationPrompt = GetSummarizationPrompt(mode);
        
        // Create a new chat with just the system prompt
        var chat = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.System, summarizationPrompt)
        };
        
        // Check if we need to aggressively trim content for summarization
        var formattedHistory = FormatMessagesForSummarization(messagesToSummarize);
        var promptTokenLimit = maxPromptTokenTarget > 0 ? maxPromptTokenTarget : 60000; // Conservative limit
        
        if (EstimateTokenCount(formattedHistory) > promptTokenLimit)
        {
            ConsoleHelpers.WriteLine("[Content too large for summarization, applying aggressive trimming...]", ConsoleColor.DarkYellow);
            formattedHistory = AggressivelyTrimForSummarization(messagesToSummarize, promptTokenLimit);
        }
        
        chat.Add(new ChatMessage(ChatRole.User, $"Here is the conversation history to summarize:\n\n{formattedHistory}"));
        
        // Ask for the summary
        var summaryRequest = "Please summarize this conversation history following the instructions provided. Focus on capturing the key information while significantly reducing the overall length.";
        chat.Add(new ChatMessage(ChatRole.User, summaryRequest));
        
        try
        {
            // Get the summary from the AI using FunctionCallingChat
            var functionFactory = new FunctionFactory();
            var completionChat = new FunctionCallingChat(chatClient, "", functionFactory, options);
            
            // Add our messages manually
            completionChat.ClearChatHistory(); // Clear the default system message
            
            // Add the system prompt
            completionChat.AddUserMessage(summarizationPrompt);
            
            // Add the history to summarize
            completionChat.AddUserMessage($"Here is the conversation history to summarize:\n\n{formattedHistory}");
            
            // Ask for the summary
            var summary = await completionChat.CompleteChatStreamingAsync(summaryRequest);
            
            if (string.IsNullOrEmpty(summary))
            {
                ConsoleHelpers.WriteWarningLine("Failed to generate summary: Empty response.");
                return FallbackToBasicTruncation(messages, systemMessage, messagesToPreserve, messagesToSummarize.Count);
            }
            
            // Create the new compacted history
            var compactedHistory = new List<ChatMessage>
            {
                systemMessage // Keep the original system message
            };
            
            // Add a special summary message as an assistant message rather than system
            var summaryMessage = new ChatMessage(ChatRole.Assistant, $"[CONVERSATION SUMMARY]\n{summary}");
            
            // Add metadata to indicate this is a summary
            summaryMessage.AuthorName = "SummaryEngine";
            if (summaryMessage.AdditionalProperties == null)
                summaryMessage.AdditionalProperties = new();
            
            summaryMessage.AdditionalProperties["isSummary"] = "true";
            summaryMessage.AdditionalProperties["summaryMode"] = mode.ToString();
            summaryMessage.AdditionalProperties["summarizedMessages"] = messagesToSummarize.Count.ToString();
            
            compactedHistory.Add(summaryMessage);
            
            // Add the preserved messages
            compactedHistory.AddRange(messagesToPreserve);
            
            // Replace the original messages with the compacted ones
            messages.Clear();
            foreach (var message in compactedHistory)
            {
                messages.Add(message);
            }
            
            ConsoleHelpers.WriteDebugLine($"Chat history compacted: {messagesToSummarize.Count} messages summarized.");
            ConsoleHelpers.WriteLine("[Conversation history compacted]", ConsoleColor.DarkYellow);
            
            return true;
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteWarningLine($"Failed to compact chat history: {ex.Message}");
            ConsoleHelpers.WriteLine("Falling back to basic truncation...", ConsoleColor.DarkYellow);
            return FallbackToBasicTruncation(messages, systemMessage, messagesToPreserve, messagesToSummarize.Count);
        }
    }
    
    /// <summary>
    /// Formats messages for the summarization prompt.
    /// </summary>
    private static string FormatMessagesForSummarization(IList<ChatMessage> messages)
    {
        var sb = new StringBuilder();
        
        foreach (var message in messages)
        {
            sb.AppendLine($"# {message.Role.ToString().ToUpperInvariant()}:");
            
            foreach (var content in message.Contents)
            {
                if (content is TextContent textContent)
                {
                    sb.AppendLine(textContent.Text);
                }
                else if (content is FunctionCallContent functionCall)
                {
                    sb.AppendLine($"[Function Call: {functionCall.Name}]");
                    sb.AppendLine($"Arguments: {JsonSerializer.Serialize(functionCall.Arguments)}");
                }
                else if (content is FunctionResultContent functionResult)
                {
                    var resultText = functionResult.Result?.ToString() ?? "null";
                    // Truncate very long function results
                    if (resultText.Length > 1000)
                    {
                        resultText = resultText.Substring(0, 997) + "...";
                    }
                    sb.AppendLine($"[Function Result: {functionResult.CallId}]");
                    sb.AppendLine(resultText);
                }
                else if (content is UriContent uriContent)
                {
                    sb.AppendLine($"[Image: {uriContent.Uri}]");
                }
                else if (content is DataContent dataContent)
                {
                    sb.AppendLine($"[Data: {dataContent.MediaType}, {dataContent.Data.Length} bytes]");
                }
                else
                {
                    sb.AppendLine($"[Unsupported Content Type: {content.GetType().Name}]");
                }
            }
            
            sb.AppendLine();
        }
        
        return sb.ToString();
    }
    
    /// <summary>
    /// Gets the appropriate summarization prompt based on the specified mode.
    /// </summary>
    private static string GetSummarizationPrompt(CompactionMode mode)
    {
        return mode == CompactionMode.Full 
            ? GetFullSummarizationPrompt() 
            : GetSimpleSummarizationPrompt();
    }
    
    /// <summary>
    /// Gets the full detailed summarization prompt.
    /// </summary>
    private static string GetFullSummarizationPrompt()
    {
        return @"You are a conversation summarization system that creates detailed, comprehensive summaries of chat interactions. Your task is to create a concise yet complete summary of the conversation history provided, preserving all essential information.

## Summary Structure
Your summary should include these sections in order:

1. Conversation Overview:
   - Primary Objectives: All explicit user requests and overarching goals
   - Session Context: High-level narrative of conversation flow and key phases
   - User Intent Evolution: How user's needs or direction changed throughout conversation

2. Technical Foundation:
   - Core technologies, frameworks, libraries used
   - Configuration details and architectural patterns
   - Environment constraints and setup specifics

3. Codebase Status:
   - Key files discussed with purposes and current states
   - Important code segments with explanations
   - Dependencies between components

4. Problem Resolution:
   - Technical problems, bugs, or challenges faced
   - Solutions implemented and reasoning
   - Ongoing troubleshooting efforts or known issues
   - Important insights or patterns discovered

5. Progress Tracking:
   - Successfully implemented tasks
   - Partially complete work with status
   - Validated features or code

6. Active Work State:
   - Current focus at time of summarization
   - Recent conversation context
   - Working code being modified recently
   - Specific problem or feature being addressed

7. Recent Operations:
   - Last commands executed
   - Tool results summary
   - Pre-summary state
   - Operation context and relationship to user goals

8. Continuation Plan:
   - Pending tasks with details
   - Priority information
   - Immediate next steps

## Quality Guidelines
- Include exact filenames, function names, variable names, and technical terms
- Capture all context needed to continue the conversation
- Write for someone who needs to pick up exactly where the conversation left off
- Include direct quotes for task specifications
- Include enough detail for complex technical decisions
- Present information in a way that builds understanding progressively

This summary should significantly reduce the token count while preserving the full technical and contextual richness of the original conversation.";
    }
    
    /// <summary>
    /// Gets the simple summarization prompt.
    /// </summary>
    private static string GetSimpleSummarizationPrompt()
    {
        return @"You are a conversation summarization system that creates concise summaries of chat interactions. Your task is to create a brief summary of the conversation history provided, capturing the most important points while significantly reducing length.

Summarize the conversation in a structured way that includes:

1. Main user goals and requests
2. Key decisions and information shared
3. Tools used and important results
4. Current status and next steps

Keep the summary concise and focused on what's needed to continue the conversation. Preserve exact names of files, functions, and technical terms. The summary should be much shorter than the original conversation while maintaining all critical context.";
    }
    
    /// <summary>
    /// Estimates token count from text using a conservative approximation.
    /// </summary>
    private static int EstimateTokenCount(string text)
    {
        // Conservative estimate: ~4 characters per token
        return text.Length / 4;
    }
    
    /// <summary>
    /// Aggressively trims conversation content to fit within token limits for summarization.
    /// </summary>
    private static string AggressivelyTrimForSummarization(IList<ChatMessage> messages, int tokenLimit)
    {
        var sb = new StringBuilder();
        var targetLength = tokenLimit * 4; // Convert tokens back to approximate character count
        var maxResultLength = 200; // Much shorter than the normal 1000 chars
        var maxTextLength = 500; // Trim long text content
        
        foreach (var message in messages)
        {
            sb.AppendLine($"# {message.Role.ToString().ToUpperInvariant()}:");
            
            foreach (var content in message.Contents)
            {
                if (content is TextContent textContent)
                {
                    var text = textContent.Text;
                    if (text.Length > maxTextLength)
                    {
                        text = text.Substring(0, maxTextLength - 10) + "...[trimmed]";
                    }
                    sb.AppendLine(text);
                }
                else if (content is FunctionCallContent functionCall)
                {
                    sb.AppendLine($"[Function Call: {functionCall.Name}]");
                    // Skip arguments to save space - just note the function was called
                }
                else if (content is FunctionResultContent functionResult)
                {
                    var resultText = functionResult.Result?.ToString() ?? "null";
                    if (resultText.Length > maxResultLength)
                    {
                        // Take first and last parts to preserve context
                        var firstPart = resultText.Substring(0, maxResultLength / 2);
                        var lastPart = resultText.Substring(resultText.Length - maxResultLength / 2);
                        resultText = $"{firstPart}...[trimmed]...{lastPart}";
                    }
                    sb.AppendLine($"[Function Result: {functionResult.CallId}]");
                    sb.AppendLine(resultText);
                }
                else if (content is UriContent uriContent)
                {
                    sb.AppendLine($"[Image: {uriContent.Uri}]");
                }
                else if (content is DataContent dataContent)
                {
                    sb.AppendLine($"[Data: {dataContent.MediaType}, {dataContent.Data.Length} bytes]");
                }
                else
                {
                    sb.AppendLine($"[{content.GetType().Name}]");
                }
            }
            
            sb.AppendLine();
            
            // Check if we're approaching the limit and need to truncate
            if (sb.Length > targetLength * 0.8) // Stop at 80% of target to leave room for remaining messages
            {
                sb.AppendLine("...[remaining messages truncated for summarization]");
                break;
            }
        }
        
        return sb.ToString();
    }
    
    /// <summary>
    /// Fallback method that performs basic truncation when summarization fails.
    /// </summary>
    private static bool FallbackToBasicTruncation(
        IList<ChatMessage> messages, 
        ChatMessage systemMessage, 
        List<ChatMessage> messagesToPreserve,
        int originalMessageCount)
    {
        var truncationMessage = new ChatMessage(
            ChatRole.System, 
            $"[CONVERSATION TRUNCATED] Previous conversation history ({originalMessageCount} messages) was too large to summarize and has been truncated to preserve recent context.");
        
        // Add metadata to indicate this is a truncation
        truncationMessage.AuthorName = "TruncationEngine";
        if (truncationMessage.AdditionalProperties == null)
            truncationMessage.AdditionalProperties = new();
        
        truncationMessage.AdditionalProperties["isTruncation"] = "true";
        truncationMessage.AdditionalProperties["truncatedMessages"] = originalMessageCount.ToString();
        
        var newHistory = new List<ChatMessage> { systemMessage, truncationMessage };
        newHistory.AddRange(messagesToPreserve);
        
        // Replace the original messages
        messages.Clear();
        foreach (var message in newHistory)
        {
            messages.Add(message);
        }
        
        ConsoleHelpers.WriteLine($"Applied basic truncation: kept system message + {messagesToPreserve.Count} recent messages", ConsoleColor.DarkYellow);
        return true;
    }
}