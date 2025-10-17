using Microsoft.Extensions.AI;
using System.Text;

/// <summary>
/// Helper methods for generating, sanitizing, and managing conversation titles.
/// Provides shared functionality for automatic title generation and manual title refresh operations.
/// </summary>
public static class TitleGenerationHelpers
{
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

        var environmentBackup = SetupEnvironmentForTitleGeneration();
        string? tempFilePath = null;

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
            
            // Create temp file with filtered content
            tempFilePath = Path.GetTempFileName();
            await File.WriteAllTextAsync(tempFilePath, filteredContent);
            
            ConsoleHelpers.WriteDebugLine($"Created temp file for title generation: {tempFilePath}");
            
            // Use provided prompt or default
            var prompt = systemPrompt ?? DefaultSystemPrompt;
            
            // Convert to bash-compatible path
            var bashPath = tempFilePath.Replace("\\", "/");
            
            // Since we're using BashShellSession, always use bash commands
            var command = $"cat \"{bashPath}\" | cycodmd - --instructions \"{prompt}\"";

            ConsoleHelpers.WriteDebugLine($"Executing title generation command: {command}");
            var result = await BashShellSession.Instance.ExecuteCommandAsync(command, timeoutMs: TitleGenerationTimeoutMs);
            
            if (result.ExitCode != 0)
            {
                ConsoleHelpers.WriteDebugLine($"Title generation command failed with exit code {result.ExitCode}: {result.StandardError}");
                return null;
            }

            var title = result.MergedOutput?.Trim();
            ConsoleHelpers.WriteDebugLine($"Raw title result: '{title}'");
            
            return string.IsNullOrEmpty(title) ? null : SanitizeTitle(title);
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteDebugLine($"Title generation failed: {ex.Message}");
            return null;
        }
        finally
        {
            CleanupTitleGeneration(tempFilePath, environmentBackup);
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
            var (metadata, messages) = TryReadChatHistoryWithFormatDetection(conversationFilePath);
            
            // Filter to only user and assistant messages
            var filteredMessages = messages
                .Where(m => m.Role == ChatRole.User || m.Role == ChatRole.Assistant)
                .ToList();
            
            ConsoleHelpers.WriteDebugLine($"Filtered {messages.Count} total messages down to {filteredMessages.Count} user/assistant messages");
            
            if (filteredMessages.Count == 0)
            {
                return string.Empty;
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
    /// Tries the default format first, then falls back to the alternate format if no messages are found.
    /// </summary>
    /// <param name="filePath">Path to the conversation file</param>
    /// <returns>Tuple of (metadata, messages) from whichever format successfully parsed content</returns>
    private static (ConversationMetadata?, List<ChatMessage>) TryReadChatHistoryWithFormatDetection(string filePath)
    {
        // Try default format first
        var (metadata1, messages1) = TryReadWithFormat(filePath, ChatHistoryDefaults.UseOpenAIFormat);
        if (messages1.Count > 0)
        {
            ConsoleHelpers.WriteDebugLine($"Successfully parsed {messages1.Count} messages using default format (OpenAI: {ChatHistoryDefaults.UseOpenAIFormat})");
            return (metadata1, messages1);
        }
        
        // Try opposite format
        var alternateFormat = !ChatHistoryDefaults.UseOpenAIFormat;
        var (metadata2, messages2) = TryReadWithFormat(filePath, alternateFormat);
        if (messages2.Count > 0)
        {
            ConsoleHelpers.WriteDebugLine($"Successfully parsed {messages2.Count} messages using alternate format (OpenAI: {alternateFormat})");
            return (metadata2, messages2);
        }
        
        // No messages found in either format
        ConsoleHelpers.WriteDebugLine("No messages found in either format - file may be empty or corrupted");
        return (metadata1, new List<ChatMessage>());
    }
    
    /// <summary>
    /// Safely attempts to read chat history with a specific format, catching and logging any exceptions.
    /// </summary>
    /// <param name="filePath">Path to the conversation file</param>
    /// <param name="useOpenAIFormat">Whether to use OpenAI format (true) or Extensions.AI format (false)</param>
    /// <returns>Tuple of (metadata, messages) - returns empty list on failure</returns>
    private static (ConversationMetadata?, List<ChatMessage>) TryReadWithFormat(string filePath, bool useOpenAIFormat)
    {
        try
        {
            return AIExtensionsChatHelpers.ReadChatHistoryFromFile(filePath, useOpenAIFormat);
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteDebugLine($"Failed to read chat history with OpenAI format = {useOpenAIFormat}: {ex.Message}");
            return (null, new List<ChatMessage>());
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
            var (metadata, messages) = AIExtensionsChatHelpers.ReadChatHistoryFromFile(conversationFilePath, useOpenAIFormat: ChatHistoryDefaults.UseOpenAIFormat);
            
            // Need at least one assistant message for meaningful conversation
            return messages.Any(m => m.Role == ChatRole.Assistant);
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

    /// <summary>
    /// Sets up environment variables for title generation to prevent infinite loops and auto-saving.
    /// </summary>
    /// <returns>Environment backup for restoration</returns>
    private static EnvironmentBackup SetupEnvironmentForTitleGeneration()
    {
        // Save original environment variable values
        var originalEnvVars = new Dictionary<string, string?>
        {
            ["CYCOD_DISABLE_TITLE_GENERATION"] = Environment.GetEnvironmentVariable("CYCOD_DISABLE_TITLE_GENERATION"),
            ["CYCOD_AUTO_SAVE_CHAT_HISTORY"] = Environment.GetEnvironmentVariable("CYCOD_AUTO_SAVE_CHAT_HISTORY"),
            ["CYCOD_AUTO_SAVE_TRAJECTORY"] = Environment.GetEnvironmentVariable("CYCOD_AUTO_SAVE_TRAJECTORY"),
            ["CYCOD_AUTO_SAVE_LOG"] = Environment.GetEnvironmentVariable("CYCOD_AUTO_SAVE_LOG")
        };
        
        // Set environment variables for child process
        Environment.SetEnvironmentVariable("CYCOD_DISABLE_TITLE_GENERATION", "true");
        Environment.SetEnvironmentVariable("CYCOD_AUTO_SAVE_CHAT_HISTORY", "false");
        Environment.SetEnvironmentVariable("CYCOD_AUTO_SAVE_TRAJECTORY", "false");
        Environment.SetEnvironmentVariable("CYCOD_AUTO_SAVE_LOG", "false");
        
        return new EnvironmentBackup { OriginalValues = originalEnvVars };
    }

    /// <summary>
    /// Cleans up temp files and restores original environment variables.
    /// </summary>
    /// <param name="tempFilePath">Temp file to delete, if any</param>
    /// <param name="environmentBackup">Environment variables to restore</param>
    private static void CleanupTitleGeneration(string? tempFilePath, EnvironmentBackup environmentBackup)
    {
        // Clean up temp file
        if (tempFilePath != null && File.Exists(tempFilePath))
        {
            try
            {
                File.Delete(tempFilePath);
                ConsoleHelpers.WriteDebugLine($"Cleaned up temp file: {tempFilePath}");
            }
            catch (Exception ex)
            {
                ConsoleHelpers.WriteDebugLine($"Warning: Failed to delete temp file {tempFilePath}: {ex.Message}");
            }
        }
        
        // Restore original environment variables
        foreach (var kvp in environmentBackup.OriginalValues)
        {
            Environment.SetEnvironmentVariable(kvp.Key, kvp.Value);
        }
        
        ConsoleHelpers.WriteDebugLine("Restored original environment variables");
    }
}

/// <summary>
/// Helper class to store original environment variable values for restoration.
/// </summary>
internal class EnvironmentBackup
{
    /// <summary>
    /// Original environment variable values to restore.
    /// </summary>
    public Dictionary<string, string?> OriginalValues { get; set; } = new();
}