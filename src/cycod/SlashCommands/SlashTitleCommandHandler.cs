using Microsoft.Extensions.AI;

/// <summary>
/// Handles the /title slash command for viewing and setting conversation titles.
/// Supports subcommands: view, set, lock, unlock, refresh
/// </summary>
public class SlashTitleCommandHandler : SlashCommandBase
{
    public override string CommandName => "title";
    private string? _inputChatHistoryPath;
    private string? _autoSaveOutputChatHistoryPath;
    
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
    /// Sets both input and output file paths for smart title operations.
    /// </summary>
    public void SetFilePaths(string? inputChatHistoryPath, string? autoSaveOutputChatHistoryPath)
    {
        _inputChatHistoryPath = inputChatHistoryPath;
        _autoSaveOutputChatHistoryPath = autoSaveOutputChatHistoryPath;
    }
    
    /// <summary>
    /// Determines the best file to read conversation content from.
    /// Prefers input file if it has content, falls back to auto-save file.
    /// </summary>
    private string? GetConversationReadFilePath()
    {
        // If we have an input file and it exists with content, prefer it
        if (!string.IsNullOrEmpty(_inputChatHistoryPath) && File.Exists(_inputChatHistoryPath))
        {
            try
            {
                var fileInfo = new FileInfo(_inputChatHistoryPath);
                if (fileInfo.Length > 0)
                {
                    ConsoleHelpers.WriteDebugLine($"Using input file for title operations: {_inputChatHistoryPath}");
                    return _inputChatHistoryPath;
                }
            }
            catch (Exception ex)
            {
                ConsoleHelpers.WriteDebugLine($"Error checking input file: {ex.Message}");
            }
        }
        
        // Fall back to auto-save file
        if (!string.IsNullOrEmpty(_autoSaveOutputChatHistoryPath))
        {
            ConsoleHelpers.WriteDebugLine($"Using auto-save file for title operations: {_autoSaveOutputChatHistoryPath}");
            return _autoSaveOutputChatHistoryPath;
        }
        
        // No valid file paths available
        ConsoleHelpers.WriteDebugLine("No valid file paths available for title operations");
        return null;
    }
    
    /// <summary>
    /// Handles the default /title command (no subcommand) - shows title and help.
    /// </summary>
    protected override SlashCommandResult HandleDefault(FunctionCallingChat chat)
    {
        ShowTitleAndHelp(chat);
        return SlashCommandResult.Handled;
    }
    
    /// <summary>
    /// Handles /title view - shows current title and status only.
    /// </summary>
    private SlashCommandResult HandleView(string[] args, FunctionCallingChat chat)
    {
        ShowCurrentTitle(chat);
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
        
        if (metadata.IsTitleLocked)
        {
            ConsoleHelpers.WriteLine("Title is already locked.\n", ConsoleColor.DarkGray);
            return SlashCommandResult.Handled;
        }
        else
        {
            metadata.IsTitleLocked = true;
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
        
        if (!metadata.IsTitleLocked)
        {
            ConsoleHelpers.WriteLine("Title is already unlocked.\n", ConsoleColor.DarkGray);
            return SlashCommandResult.Handled;
        }
        else
        {
            metadata.IsTitleLocked = false;
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
        if (chat.Metadata?.IsTitleLocked == true)
        {
            ConsoleHelpers.WriteLine("Cannot refresh title: title is locked. Use /title unlock first.\n", ConsoleColor.Red);
            return SlashCommandResult.Handled;
        }
        
        // Get the file path to read conversation content from
        var readFilePath = GetConversationReadFilePath();
        if (string.IsNullOrEmpty(readFilePath))
        {
            ConsoleHelpers.WriteLine("Error: No conversation file available for title refresh.\n", ConsoleColor.Red);
            return SlashCommandResult.Handled;
        }
        
        // Validate conversation has enough content
        if (!HasSufficientContentForTitleGeneration(readFilePath))
        {
            ConsoleHelpers.WriteLine("Cannot refresh title: conversation needs at least one assistant message.\n", ConsoleColor.Red);
            return SlashCommandResult.Handled;
        }
        
        // Start async title generation
        ConsoleHelpers.WriteLine("Generating new title...\n", ConsoleColor.DarkGray);
        _ = Task.Run(async () => await RefreshTitleAsync(chat, readFilePath));
        
        return SlashCommandResult.Handled; // No immediate save - async operation will handle it
    }
    
    /// <summary>
    /// Shows current title and available subcommands (default /title behavior).
    /// </summary>
    private void ShowTitleAndHelp(FunctionCallingChat chat)
    {
        ShowCurrentTitle(chat);
        ShowHelp(chat.Metadata);
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
    private void ShowCurrentTitle(FunctionCallingChat chat)
    {
        var metadata = chat.Metadata;
        var title = metadata?.Title ?? "No title set";
        
        // Determine status with generation awareness
        var titleGenerationInProgress = chat.IsGenerationInProgress(NotificationType.Title);
        var titleIsLocked = metadata?.IsTitleLocked == true;

        var status = titleGenerationInProgress 
            ? "AI title generation in progress..."
            : titleIsLocked ? "locked" : "unlocked";
        
        ConsoleHelpers.WriteLine($"Current title: \"{title}\" ({status})", ConsoleColor.DarkGray);
        ConsoleHelpers.WriteLine();
        
        // Clear any pending title notifications since user just viewed the title
        chat.ClearPendingNotificationsOfType(NotificationType.Title);
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
    /// Asynchronously generates and updates the conversation title.
    /// </summary>
    private async Task RefreshTitleAsync(FunctionCallingChat chat, string readFilePath)
    {
        // Mark title generation as in progress
        chat.SetGenerationInProgress(NotificationType.Title);
        
        try
        {
            // Reuse existing title generation logic from ChatCommand
            var generatedTitle = await GenerateTitleUsingCycodmd(readFilePath);
            
            if (!string.IsNullOrEmpty(generatedTitle))
            {
                // Update metadata with new title (unlocked since it's AI-generated)
                var metadata = chat.Metadata ?? ConversationMetadataHelpers.CreateDefault();
                ConversationMetadataHelpers.SetGeneratedTitle(metadata, generatedTitle);
                chat.UpdateMetadata(metadata);
                
                // Save the updated conversation to auto-save file (not the input file)
                var saveFilePath = _autoSaveOutputChatHistoryPath ?? readFilePath;
                chat.SaveChatHistoryToFile(saveFilePath, useOpenAIFormat: ChatHistoryDefaults.UseOpenAIFormat);
                
                ConsoleHelpers.WriteDebugLine($"Title refresh: read from {readFilePath}, saved to {saveFilePath}");
                
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
        catch (FileNotFoundException ex)
        {
            ConsoleHelpers.WriteDebugLine($"Conversation file not found during title refresh: {ex.Message}");
            ConsoleHelpers.WriteLine("Failed to refresh title - conversation file not found.\n", ConsoleColor.Red);
        }
        catch (UnauthorizedAccessException ex)
        {
            ConsoleHelpers.WriteDebugLine($"Access denied during title refresh: {ex.Message}");
            ConsoleHelpers.WriteLine("Failed to refresh title - access denied.\n", ConsoleColor.Red);
        }
        catch (Exception ex)
        {
            ConsoleHelpers.WriteDebugLine($"Error during title refresh: {ex.Message}");
            ConsoleHelpers.WriteLine("Failed to refresh title due to an error.\n", ConsoleColor.Red);
        }
        finally
        {
            // Clear generation status regardless of success or failure
            chat.ClearGenerationInProgress(NotificationType.Title);  
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
            var instructions = "Generate a concise title for this conversation (3-5 words)";
            var command = $"cat \"{bashPath}\" | cycodmd - --instructions \"{instructions}\"";
            
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
        catch (FileNotFoundException ex)
        {
            ConsoleHelpers.WriteDebugLine($"Required file not found for title generation: {ex.Message}");
            return null;
        }
        catch (UnauthorizedAccessException ex)
        {
            ConsoleHelpers.WriteDebugLine($"Access denied during title generation: {ex.Message}");
            return null;
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
                catch (IOException ex)
                {
                    ConsoleHelpers.WriteDebugLine($"Warning: IO error deleting temp file {tempFilePath}: {ex.Message}");
                }
                catch (UnauthorizedAccessException ex)
                {
                    ConsoleHelpers.WriteDebugLine($"Warning: Access denied deleting temp file {tempFilePath}: {ex.Message}");
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
            var (metadata, messages) = AIExtensionsChatHelpers.ReadChatHistoryFromFile(conversationFilePath, useOpenAIFormat: ChatHistoryDefaults.UseOpenAIFormat);
            
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