using Microsoft.Extensions.AI;

/// <summary>
/// Handles the /title slash command for viewing and setting conversation titles.
/// Supports subcommands: view, set, lock, unlock, refresh
/// </summary>
public class SlashTitleCommandHandler : SlashCommandBase
{
    public override string CommandName => "title";
    private string? _conversationFilePath;
    
    public SlashTitleCommandHandler()
    {
        // Register subcommands
        _subcommands["view"] = HandleView;
        _subcommands["set"] = HandleSet;
        _subcommands["lock"] = HandleLock;
        _subcommands["unlock"] = HandleUnlock;
        _subcommands["refresh"] = HandleRefresh;
    }
    
    /// <summary>
    /// Sets the conversation file path for title refresh operations.
    /// </summary>
    public void SetFilePath(string? conversationFilePath)
    {
        _conversationFilePath = conversationFilePath;
    }
    
    /// <summary>
    /// Handles the default /title command (no subcommand) - shows title and help.
    /// </summary>
    protected override SlashCommandResult HandleDefault(FunctionCallingChat chat)
    {
        ShowTitleAndHelp(chat.Metadata);
        return SlashCommandResult.Handled;
    }
    
    /// <summary>
    /// Handles /title view - shows current title and status only.
    /// </summary>
    private SlashCommandResult HandleView(string[] args, FunctionCallingChat chat)
    {
        ShowCurrentTitle(chat.Metadata);
        return SlashCommandResult.Handled;
    }
    
    /// <summary>
    /// Handles /title set <text> - sets title and locks it.
    /// </summary>
    private SlashCommandResult HandleSet(string[] args, FunctionCallingChat chat)
    {
        if (args.Length == 0)
        {
            ConsoleHelpers.WriteLine("Error: /title set requires a title. Usage: /title set <text>\n", ConsoleColor.Red);
            return SlashCommandResult.Handled;
        }
        
        var title = string.Join(" ", args);
        SetUserTitle(chat, title);
        return SlashCommandResult.NeedsSave; // 🚀 Request immediate save
    }
    
    /// <summary>
    /// Handles /title lock - locks current title from AI changes.
    /// </summary>
    private SlashCommandResult HandleLock(string[] args, FunctionCallingChat chat)
    {
        var metadata = chat.Metadata ?? ConversationMetadataHelpers.CreateDefault();
        
        if (metadata.TitleLocked)
        {
            ConsoleHelpers.WriteLine("Title is already locked.\n", ConsoleColor.DarkGray);
            return SlashCommandResult.Handled;
        }
        else
        {
            metadata.TitleLocked = true;
            chat.UpdateMetadata(metadata);
            
            // Update console title (no change in title text, but ensure it's set)
            ConsoleTitleHelper.UpdateWindowTitle(metadata);
            
            ConsoleHelpers.WriteLine("Title locked from AI changes.\n", ConsoleColor.DarkGray);
            return SlashCommandResult.NeedsSave; // 🚀 Request immediate save
        }
    }
    
    /// <summary>
    /// Handles /title unlock - unlocks title to allow AI regeneration.
    /// </summary>
    private SlashCommandResult HandleUnlock(string[] args, FunctionCallingChat chat)
    {
        var metadata = chat.Metadata ?? ConversationMetadataHelpers.CreateDefault();
        
        if (!metadata.TitleLocked)
        {
            ConsoleHelpers.WriteLine("Title is already unlocked.\n", ConsoleColor.DarkGray);
            return SlashCommandResult.Handled;
        }
        else
        {
            metadata.TitleLocked = false;
            chat.UpdateMetadata(metadata);
            
            // Update console title (no change in title text, but ensure it's set)
            ConsoleTitleHelper.UpdateWindowTitle(metadata);
            
            ConsoleHelpers.WriteLine("Title unlocked - AI can now regenerate the title.\n", ConsoleColor.DarkGray);
            return SlashCommandResult.NeedsSave; // 🚀 Request immediate save
        }
    }
    
    /// <summary>
    /// Handles /title refresh - generates new title from current conversation.
    /// </summary>
    private SlashCommandResult HandleRefresh(string[] args, FunctionCallingChat chat)
    {
        // Check if title is locked
        if (chat.Metadata?.TitleLocked == true)
        {
            ConsoleHelpers.WriteLine("Cannot refresh title: title is locked. Use /title unlock first.\n", ConsoleColor.Red);
            return SlashCommandResult.Handled;
        }
        
        // Check if we have a file path
        if (string.IsNullOrEmpty(_conversationFilePath))
        {
            ConsoleHelpers.WriteLine("Error: No conversation file available for title refresh.\n", ConsoleColor.Red);
            return SlashCommandResult.Handled;
        }
        
        // Validate conversation has enough content
        if (!HasSufficientContentForTitleGeneration(_conversationFilePath))
        {
            ConsoleHelpers.WriteLine("Cannot refresh title: conversation needs at least one assistant message.\n", ConsoleColor.Red);
            return SlashCommandResult.Handled;
        }
        
        // Start async title generation
        ConsoleHelpers.WriteLine("Generating new title...\n", ConsoleColor.DarkGray);
        _ = Task.Run(async () => await RefreshTitleAsync(chat, _conversationFilePath));
        
        return SlashCommandResult.Handled; // No immediate save - async operation will handle it
    }
    
    /// <summary>
    /// Shows current title and available subcommands (default /title behavior).
    /// </summary>
    private void ShowTitleAndHelp(ConversationMetadata? metadata)
    {
        ShowCurrentTitle(metadata);
        ShowHelp(metadata);
    }
    
    /// <summary>
    /// Shows available title subcommands.
    /// </summary>
    protected override void ShowHelp(ConversationMetadata? metadata)
    {
        ConsoleHelpers.WriteLine("Available commands:", ConsoleColor.DarkGray);
        ConsoleHelpers.WriteLine("  /title view         Show current title and lock status", ConsoleColor.DarkGray);
        ConsoleHelpers.WriteLine("  /title set <text>   Set title and lock from AI changes", ConsoleColor.DarkGray);
        ConsoleHelpers.WriteLine("  /title lock         Lock current title from AI changes", ConsoleColor.DarkGray);
        ConsoleHelpers.WriteLine("  /title unlock       Unlock title to allow AI regeneration", ConsoleColor.DarkGray);
        ConsoleHelpers.WriteLine("  /title refresh      Generate new title from current conversation\n", ConsoleColor.DarkGray);
    }
    
    /// <summary>
    /// Displays the current title and its lock status.
    /// </summary>
    private void ShowCurrentTitle(ConversationMetadata? metadata)
    {
        var title = metadata?.Title ?? "No title set";
        var status = metadata?.TitleLocked == true ? "locked" : "unlocked";
        
        ConsoleHelpers.WriteLine($"Current title: \"{title}\" ({status})", ConsoleColor.DarkGray);
        ConsoleHelpers.WriteLine();
    }
    
    /// <summary>
    /// Sets a user-provided title and marks it as locked.
    /// </summary>
    private void SetUserTitle(FunctionCallingChat chat, string title)
    {
        // Initialize metadata if not present
        var metadata = chat.Metadata ?? ConversationMetadataHelpers.CreateDefault();
        
        // Set and lock the title
        ConversationMetadataHelpers.SetUserTitle(metadata, title);
        
        // Update the chat's metadata
        chat.UpdateMetadata(metadata);
        
        // Update console title immediately
        ConsoleTitleHelper.UpdateWindowTitle(metadata);
        
        ConsoleHelpers.WriteLine($"Title updated to: \"{title}\" (locked from AI changes)\n", ConsoleColor.DarkGray);
    }
    
    /// <summary>
    /// Checks if the conversation has sufficient content for title generation.
    /// </summary>
    private bool HasSufficientContentForTitleGeneration(string conversationFilePath)
    {
        try
        {
            if (!File.Exists(conversationFilePath))
            {
                return false;
            }
            
            // Load and parse the conversation
            var (metadata, messages) = AIExtensionsChatHelpers.ReadChatHistoryFromFileWithMetadata(conversationFilePath, useOpenAIFormat: ChatHistoryDefaults.UseOpenAIFormat);
            
            // Need at least one assistant message for meaningful conversation
            return messages.Any(m => m.Role == ChatRole.Assistant);
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteDebugLine($"Error checking conversation content: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Asynchronously generates and updates the conversation title.
    /// </summary>
    private async Task RefreshTitleAsync(FunctionCallingChat chat, string conversationFilePath)
    {
        try
        {
            // Reuse existing title generation logic from ChatCommand
            var generatedTitle = await GenerateTitleUsingCycodmd(conversationFilePath);
            
            if (!string.IsNullOrEmpty(generatedTitle))
            {
                // Update metadata with new title (unlocked since it's AI-generated)
                var metadata = chat.Metadata ?? ConversationMetadataHelpers.CreateDefault();
                ConversationMetadataHelpers.SetGeneratedTitle(metadata, generatedTitle);
                chat.UpdateMetadata(metadata);
                
                // Save the updated conversation
                chat.SaveChatHistoryToFile(conversationFilePath, useOpenAIFormat: ChatHistoryDefaults.UseOpenAIFormat);
                
                // Update console title immediately
                ConsoleTitleHelper.UpdateWindowTitle(metadata);
                
                // Set pending notification for next assistant response
                chat.SetPendingNotification(NotificationType.Title, generatedTitle);
            }
            else
            {
                ConsoleHelpers.WriteLine("Failed to generate new title.\n", ConsoleColor.Red);
            }
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteDebugLine($"Error during title refresh: {ex.Message}");
            ConsoleHelpers.WriteLine("Failed to refresh title due to an error.\n", ConsoleColor.Red);
        }
    }
    
    /// <summary>
    /// Generates a title using cycodmd (replicates ChatCommand.GenerateTitleAsync logic).
    /// </summary>
    private async Task<string?> GenerateTitleUsingCycodmd(string conversationFilePath)
    {
        // Save original environment variables
        var originalTitleGeneration = Environment.GetEnvironmentVariable("CYCOD_DISABLE_TITLE_GENERATION");
        var originalAutoSaveChat = Environment.GetEnvironmentVariable("CYCOD_AUTO_SAVE_CHAT_HISTORY");
        var originalAutoSaveTrajectory = Environment.GetEnvironmentVariable("CYCOD_AUTO_SAVE_TRAJECTORY");
        var originalAutoSaveLog = Environment.GetEnvironmentVariable("CYCOD_AUTO_SAVE_LOG");
        
        // Set environment variables for child process
        Environment.SetEnvironmentVariable("CYCOD_DISABLE_TITLE_GENERATION", "true");
        Environment.SetEnvironmentVariable("CYCOD_AUTO_SAVE_CHAT_HISTORY", "false");
        Environment.SetEnvironmentVariable("CYCOD_AUTO_SAVE_TRAJECTORY", "false");
        Environment.SetEnvironmentVariable("CYCOD_AUTO_SAVE_LOG", "false");
        
        string? tempFilePath = null;
        
        try
        {
            // Check if file exists
            if (!File.Exists(conversationFilePath))
            {
                ConsoleHelpers.WriteDebugLine($"❌ File does not exist for title generation: {conversationFilePath}");
                return null;
            }
            
            // Create filtered content with only user and assistant messages
            var filteredContent = CreateFilteredConversationContent(conversationFilePath);
            if (string.IsNullOrEmpty(filteredContent))
            {
                ConsoleHelpers.WriteDebugLine($"❌ No meaningful conversation content found for title generation");
                return null;
            }
            
            // Write filtered content to temp file
            tempFilePath = Path.GetTempFileName();
            await File.WriteAllTextAsync(tempFilePath, filteredContent);
            
            ConsoleHelpers.WriteDebugLine($"Created filtered temp file for title refresh: {tempFilePath}");
            
            // Convert temp file path to Unix-style for bash
            var bashPath = tempFilePath.Replace("\\", "/");
            
            // Generate title using cycodmd
            var command = $"cat \"{bashPath}\" | cycodmd - --instructions \"Generate a concise title for this conversation (3-5 words)\"";
            
            ConsoleHelpers.WriteDebugLine($"Title refresh command: {command}");
            
            var result = await BashShellSession.Instance.ExecuteCommandAsync(command, timeoutMs: 30000);
            
            ConsoleHelpers.WriteDebugLine($"Command exit code: {result.ExitCode}");
            ConsoleHelpers.WriteDebugLine($"Command output: {result.MergedOutput}");
            
            if (result.ExitCode != 0 || result.IsTimeout)
            {
                ConsoleHelpers.WriteDebugLine($"Title refresh command failed with exit code {result.ExitCode}");
                return null;
            }
            
            var title = result.MergedOutput?.Trim();
            return string.IsNullOrEmpty(title) ? null : SanitizeTitle(title);
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteDebugLine($"Title refresh failed: {ex.Message}");
            return null;
        }
        finally
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
            Environment.SetEnvironmentVariable("CYCOD_DISABLE_TITLE_GENERATION", originalTitleGeneration);
            Environment.SetEnvironmentVariable("CYCOD_AUTO_SAVE_CHAT_HISTORY", originalAutoSaveChat);
            Environment.SetEnvironmentVariable("CYCOD_AUTO_SAVE_TRAJECTORY", originalAutoSaveTrajectory);
            Environment.SetEnvironmentVariable("CYCOD_AUTO_SAVE_LOG", originalAutoSaveLog);
            
            ConsoleHelpers.WriteDebugLine("Restored original environment variables");
        }
    }
    
    /// <summary>
    /// Creates filtered conversation content containing only user and assistant messages.
    /// </summary>
    private string CreateFilteredConversationContent(string conversationFilePath)
    {
        try
        {
            // Load and parse the conversation
            var (metadata, messages) = AIExtensionsChatHelpers.ReadChatHistoryFromFileWithMetadata(conversationFilePath, useOpenAIFormat: ChatHistoryDefaults.UseOpenAIFormat);
            
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
            var content = new System.Text.StringBuilder();
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
        catch (Exception ex)
        {
            ConsoleHelpers.WriteDebugLine($"Failed to create filtered conversation content: {ex.Message}");
            return string.Empty;
        }
    }
    
    /// <summary>
    /// Cleans and formats a generated title.
    /// </summary>
    private string SanitizeTitle(string title)
    {
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
        if (sanitized.Length > 80)
        {
            sanitized = sanitized.Substring(0, 77) + "...";
        }
        
        ConsoleHelpers.WriteDebugLine($"Sanitized title: '{title}' → '{sanitized}'");
        
        return sanitized;
    }
}