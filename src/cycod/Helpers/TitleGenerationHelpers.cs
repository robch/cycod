using Microsoft.Extensions.AI;
using System.Text;

/// <summary>
/// Helper methods for generating, sanitizing, and managing conversation titles.
/// Provides shared functionality for automatic title generation and manual title refresh operations.
/// </summary>
public static class TitleGenerationHelpers
{
    /// <summary>
    /// CycodMD CLI wrapper instance for executing title generation commands.
    /// </summary>
    private static readonly CycoDmdCliWrapper _cycoDmdWrapper = new CycoDmdCliWrapper();
    /// <summary>
    /// Default system prompt for title generation.
    /// </summary>
    private const string DefaultSystemPrompt = "Generate a concise title for this conversation (3-5 words). No markdown formatting allowed. Only return the title text. Do not explain your thought process in any way. Simply return the title.";
    
    /// <summary>
    /// Timeout in milliseconds for title generation command execution.
    /// </summary>
    public const int TitleGenerationTimeoutMs = 30000;
    
    /// <summary>
    /// Maximum length for display titles before truncation.
    /// </summary>
    public const int MaxTitleDisplayLength = 80;
    
    /// <summary>
    /// Length to truncate to when title exceeds maximum display length.
    /// </summary>
    public const int TitleTruncationLength = 77;

    /// <summary>
    /// Generates a conversation title using cycodmd with environment variables to prevent infinite loops and auto-saving.
    /// </summary>
    /// <param name="conversationFilePath">Path to the conversation file</param>
    /// <param name="systemPrompt">Optional system prompt for title generation. Uses default if not provided.</param>
    /// <returns>Generated title or null if generation failed</returns>
    public static async Task<string?> GenerateTitleAsync(string conversationFilePath, string? systemPrompt = null)
    {
        if (string.IsNullOrWhiteSpace(conversationFilePath))
        {
            ConsoleHelpers.WriteDebugLine("Conversation file path is null or empty");
            return null;
        }

        try
        {
            // Check if file exists
            if (!File.Exists(conversationFilePath))
            {
                ConsoleHelpers.WriteDebugLine($"File does not exist for title generation: {conversationFilePath}");
                return null;
            }
            
            // Create filtered content with only user and assistant messages
            var filteredContent = CreateFilteredConversationContent(conversationFilePath);
            if (string.IsNullOrEmpty(filteredContent))
            {
                ConsoleHelpers.WriteDebugLine($"No meaningful conversation content found for title generation");
                return null;
            }
            
            ConsoleHelpers.WriteDebugLine($"Generated filtered content for title generation ({filteredContent.Length} characters)");
            
            // Use provided prompt or default
            var prompt = systemPrompt ?? DefaultSystemPrompt;
            
            ConsoleHelpers.WriteDebugLine($"Executing title generation using CycoDmdCliWrapper");
            var generatedTitle = await _cycoDmdWrapper.ExecuteGenerateTitleCommand(filteredContent, prompt, TitleGenerationTimeoutMs);
            
            ConsoleHelpers.WriteDebugLine($"Raw title result: '{generatedTitle}'");
            
            return string.IsNullOrEmpty(generatedTitle) ? null : SanitizeTitle(generatedTitle);
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteDebugLine($"Title generation failed: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Creates filtered conversation content containing only user and assistant messages.
    /// </summary>
    /// <param name="conversationFilePath">Path to the conversation file</param>
    /// <returns>Filtered conversation content as text</returns>
    public static string CreateFilteredConversationContent(string conversationFilePath)
    {
        try
        {
            // Load and parse the conversation with format auto-detection
            var conversation = TryReadChatHistoryWithFormatDetection(conversationFilePath);
            
            // Filter to only user and assistant messages
            var filteredMessages = conversation.Messages
                .Where(m => m.Role == ChatRole.User || m.Role == ChatRole.Assistant)
                .ToList();
            
            ConsoleHelpers.WriteDebugLine($"Filtered {conversation.Messages.Count} total messages down to {filteredMessages.Count} user/assistant messages");
            
            if (filteredMessages.Count == 0)
            {
                return string.Empty;
            }
            
            // Trim context if conversation is too large (more than 6 messages)
            if (filteredMessages.Count > 6)
            {
                var trimmedMessages = new List<ChatMessage>();
                var addedIndices = new HashSet<int>();
                
                // Find first user message
                var firstUserMsg = filteredMessages.FirstOrDefault(m => m.Role == ChatRole.User);
                if (firstUserMsg != null)
                {
                    var index = filteredMessages.IndexOf(firstUserMsg);
                    trimmedMessages.Add(firstUserMsg);
                    addedIndices.Add(index);
                }
                
                // Find first assistant message
                var firstAssistantMsg = filteredMessages.FirstOrDefault(m => m.Role == ChatRole.Assistant);
                if (firstAssistantMsg != null)
                {
                    var index = filteredMessages.IndexOf(firstAssistantMsg);
                    if (!addedIndices.Contains(index))
                    {
                        trimmedMessages.Add(firstAssistantMsg);
                        addedIndices.Add(index);
                    }
                }
                
                // Add third-to-last message (regardless of role)
                if (filteredMessages.Count >= 3)
                {
                    var thirdToLastIndex = filteredMessages.Count - 3;
                    if (!addedIndices.Contains(thirdToLastIndex))
                    {
                        trimmedMessages.Add(filteredMessages[thirdToLastIndex]);
                        addedIndices.Add(thirdToLastIndex);
                    }
                }
                
                // Add second-to-last message (regardless of role)
                if (filteredMessages.Count >= 2)
                {
                    var secondToLastIndex = filteredMessages.Count - 2;
                    if (!addedIndices.Contains(secondToLastIndex))
                    {
                        trimmedMessages.Add(filteredMessages[secondToLastIndex]);
                        addedIndices.Add(secondToLastIndex);
                    }
                }
                
                // Add last message (regardless of role)
                var lastIndex = filteredMessages.Count - 1;
                if (!addedIndices.Contains(lastIndex))
                {
                    trimmedMessages.Add(filteredMessages[lastIndex]);
                    addedIndices.Add(lastIndex);
                }
                
                // Sort by original order
                var sortedMessages = trimmedMessages
                    .Select(msg => new { Message = msg, Index = filteredMessages.IndexOf(msg) })
                    .OrderBy(x => x.Index)
                    .Select(x => x.Message)
                    .ToList();
                
                filteredMessages = sortedMessages;
                ConsoleHelpers.WriteDebugLine($"Trimmed large conversation context from {conversation.Messages.Count} to {filteredMessages.Count} key messages for title generation");
            }
            
            // Convert to simple text format for title generation
            var content = new StringBuilder();
            content.AppendLine("Conversation between User and Assistant:");
            content.AppendLine();
            
            foreach (var message in filteredMessages)
            {
                var roleLabel = message.Role == ChatRole.User ? "User" : "Assistant";
                content.AppendLine($"{roleLabel}: {message.Text}");
                content.AppendLine();
            }
            
            return content.ToString();
        }
        catch (FileNotFoundException ex)
        {
            ConsoleHelpers.WriteDebugLine($"Conversation file not found for content filtering: {ex.Message}");
            return string.Empty;
        }
        catch (UnauthorizedAccessException ex)
        {
            ConsoleHelpers.WriteDebugLine($"Access denied reading conversation file for filtering: {ex.Message}");
            return string.Empty;
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteDebugLine($"Failed to create filtered conversation content: {ex.Message}");
            return string.Empty;
        }
    }

    /// <summary>
    /// Attempts to read chat history with automatic format detection.
    /// Reads the file once and tries both formats on the same content.
    /// </summary>
    /// <param name="filePath">Path to the conversation file</param>
    /// <returns>A conversation object from whichever format successfully parsed content</returns>
    private static Conversation TryReadChatHistoryWithFormatDetection(string filePath)
    {
        try
        {
            // Read file once and split into lines
            var jsonl = FileHelpers.ReadAllText(filePath);
            var lines = jsonl.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            
            // Create conversation and try to load from lines with format detection
            var conversation = new Conversation();
            var success = conversation.LoadFromLines(lines);
            
            if (success)
            {
                return conversation;
            }
            else
            {
                ConsoleHelpers.WriteDebugLine("No messages found in either format - file may be empty or corrupted");
                return conversation; // Return empty conversation
            }
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteDebugLine($"Failed to read chat history with format detection: {ex.Message}");
            return new Conversation(); // Return empty conversation on failure
        }
    }
    


    /// <summary>
    /// Cleans and formats a generated title.
    /// </summary>
    /// <param name="title">Raw title to sanitize</param>
    /// <returns>Cleaned and formatted title</returns>
    public static string SanitizeTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return string.Empty;
        }

        // Remove quotes and extra whitespace
        var sanitized = title.Trim('"', '\'', ' ', '\n', '\r', '\t');
        
        // Remove any error messages or shell output that got mixed in
        var lines = sanitized.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
        
        // Find the actual title (skip error lines, bash prompts, etc.)
        foreach (var line in lines)
        {
            var cleanLine = line.Trim();
            // Skip obvious error/command lines
            if (cleanLine.StartsWith("/usr/bin/bash:") ||
                cleanLine.StartsWith("$") ||
                cleanLine.StartsWith("bash:") ||
                cleanLine.Contains("command not found") ||
                string.IsNullOrWhiteSpace(cleanLine))
            {
                continue;
            }
            
            // This looks like actual content
            sanitized = cleanLine;
            break;
        }
        
        // Final cleanup
        sanitized = sanitized.Trim('"', '\'', ' ', '\n', '\r', '\t');
        
        // Limit length for display
        if (sanitized.Length > MaxTitleDisplayLength)
        {
            sanitized = sanitized.Substring(0, TitleTruncationLength) + "...";
        }
        
        ConsoleHelpers.WriteDebugLine($"Sanitized title: '{title}' → '{sanitized}'");
        
        return sanitized;
    }

    /// <summary>
    /// Checks if a conversation file has sufficient content for title generation.
    /// Requires at least one assistant message for meaningful conversation.
    /// </summary>
    /// <param name="conversationFilePath">Path to the conversation file</param>
    /// <returns>True if the conversation has sufficient content for title generation</returns>
    public static bool HasSufficientContentForTitleGeneration(string conversationFilePath)
    {
        if (string.IsNullOrWhiteSpace(conversationFilePath))
        {
            return false;
        }

        try
        {
            if (!File.Exists(conversationFilePath))
            {
                return false;
            }
            
            // Load and parse the conversation
            var conversation = new Conversation(conversationFilePath, useOpenAIFormat: ChatHistoryDefaults.UseOpenAIFormat);
            
            // Need at least one assistant message for meaningful conversation
            return conversation.Messages.Any(m => m.Role == ChatRole.Assistant);
        }
        catch (FileNotFoundException ex)
        {
            ConsoleHelpers.WriteDebugLine($"Conversation file not found: {ex.Message}");
            return false;
        }
        catch (UnauthorizedAccessException ex)
        {
            ConsoleHelpers.WriteDebugLine($"Access denied to conversation file: {ex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteDebugLine($"Error checking conversation content: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Determines if a title should be generated based on messages and metadata.
    /// Combines message content validation with metadata state checking.
    /// </summary>
    /// <param name="messages">Chat messages to evaluate</param>
    /// <param name="metadata">Conversation metadata</param>
    /// <returns>True if title generation should proceed</returns>
    public static bool ShouldGenerateTitle(IList<ChatMessage> messages, ConversationMetadata? metadata)
    {
        var hasUserAssistantExchange = messages.Any(m => m.Role == ChatRole.Assistant);
        var shouldGenerate = ConversationMetadataHelpers.ShouldGenerateTitle(metadata);

        return hasUserAssistantExchange && shouldGenerate;
    }
}