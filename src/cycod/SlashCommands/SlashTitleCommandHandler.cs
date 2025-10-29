using Microsoft.Extensions.AI;
using System.Linq;

/// <summary>
/// Handles the /title slash command for viewing and setting conversation titles.
/// Supports subcommands: view, set, lock, unlock, refresh, revert.
/// Implements the clean ISlashCommandHandler interface.
/// </summary>
public class SlashTitleCommandHandler : ISlashCommandHandler
{
    private string? _inputChatHistoryPath;
    private string? _autoSaveOutputChatHistoryPath;

    /// <summary>
    /// Parses arguments from a command string.
    /// </summary>
    private string[] ParseArgs(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return Array.Empty<string>();

        return input.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
    }
    
    /// <summary>
    /// Checks if this handler can process the given command.
    /// </summary>
    public bool CanHandle(string userPrompt)
    {
        return userPrompt.StartsWith("/title");
    }

    /// <summary>
    /// Handles the title command synchronously with support for all subcommands.
    /// </summary>
    public SlashCommandResult Handle(string userPrompt, FunctionCallingChat chat)
    {
        var afterCommand = userPrompt.Substring("/title".Length).Trim();
        var args = ParseArgs(afterCommand);
        
        if (args.Length == 0)
        {
            return HandleDefault(chat);
        }
        
        var subcommand = args[0].ToLowerInvariant();
        
        // Handle each subcommand
        switch (subcommand)
        {
            case "view":
                return HandleView(args.Skip(1).ToArray(), chat);
            case "set":
                var afterSet = afterCommand.Substring(3).Trim(); // Remove "set" and trim
                return HandleSetWithRawInput(afterSet, chat);
            case "lock":
                return HandleLock(args.Skip(1).ToArray(), chat);
            case "unlock":
                return HandleUnlock(args.Skip(1).ToArray(), chat);
            case "refresh":
                return HandleRefresh(args.Skip(1).ToArray(), chat);
            case "revert":
                return HandleRevert(args.Skip(1).ToArray(), chat);
            default:
                ConsoleHelpers.WriteLine($"Unknown subcommand '{subcommand}' for /title.\n", ConsoleColor.Yellow);
                ShowHelp(chat.Conversation.Metadata);
                return SlashCommandResult.Success();
        }
    }
    
    /// <summary>
    /// Sets both input and output file paths for smart title operations.
    /// Required for scenarios where title commands are used with --continue or --input-chat-history
    /// but the output file doesn't exist yet (e.g., first command is "/title refresh").
    /// </summary>
    /// <param name="inputChatHistoryPath">Path to read original conversation from for title generation</param>
    /// <param name="autoSaveOutputChatHistoryPath">Path to save conversation with updated title</param>
    public void SetFilePaths(string? inputChatHistoryPath, string? autoSaveOutputChatHistoryPath)
    {
        _inputChatHistoryPath = inputChatHistoryPath;
        _autoSaveOutputChatHistoryPath = autoSaveOutputChatHistoryPath;
    }
    
    /// <summary>
    /// Checks if there's a valid conversation file available for saving metadata.
    /// </summary>
    private bool HasValidConversationFile()
    {
        return !string.IsNullOrEmpty(_inputChatHistoryPath) || !string.IsNullOrEmpty(_autoSaveOutputChatHistoryPath);
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
    private SlashCommandResult HandleDefault(FunctionCallingChat chat)
    {
        ShowTitleAndHelp(chat);
        return SlashCommandResult.Success();
    }
    
    /// <summary>
    /// Handles /title view - shows current title and status only.
    /// </summary>
    private SlashCommandResult HandleView(string[] args, FunctionCallingChat chat)
    {
        ShowCurrentTitle(chat);
        return SlashCommandResult.Success();
    }
    
    /// <summary>
    /// Handles /title set <text> - sets title and locks it. (Legacy method, not used)
    /// </summary>
    private SlashCommandResult HandleSet(string[] args, FunctionCallingChat chat)
    {
        // This method is no longer used - HandleSetWithRawInput is used instead
        throw new NotImplementedException("Use HandleSetWithRawInput instead");
    }
    
    /// <summary>
    /// Handles /title set with raw input validation - requires surrounding double quotes.
    /// </summary>
    private SlashCommandResult HandleSetWithRawInput(string input, FunctionCallingChat chat)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            ConsoleHelpers.WriteLine("Error: Titles must be enclosed in double quotes. Usage: /title set \"<title>\"\n", ConsoleColor.Red);
            return SlashCommandResult.Success();
        }
        
        // Check if there's a valid conversation file to save metadata to
        if (!HasValidConversationFile())
        {
            ConsoleHelpers.WriteLine("Error: No conversation file to save metadata to. Use --input-chat-history or create a conversation first.\n", ConsoleColor.Red);
            return SlashCommandResult.Success();
        }
        
        // Validate that input starts and ends with double quotes
        if (!input.StartsWith("\"") || !input.EndsWith("\"") || input.Length < 2)
        {
            ConsoleHelpers.WriteLine("Error: Titles must be enclosed in double quotes. Usage: /title set \"<title>\"\n", ConsoleColor.Red);
            return SlashCommandResult.Success();
        }
        
        // Extract content between outermost quotes
        var title = input.Substring(1, input.Length - 2);
        
        // Validate that the title is not empty or just whitespace
        if (string.IsNullOrWhiteSpace(title))
        {
            ConsoleHelpers.WriteLine("Error: Title cannot be empty. Please provide a title.\n", ConsoleColor.Red);
            return SlashCommandResult.Success();
        }
        
        SetUserTitle(chat, title);
        return SlashCommandResult.HandledWithSave(); // 🚀 Request immediate save
    }

    /// <summary>
    /// Handles /title lock - locks current title from AI changes.
    /// </summary>
    private SlashCommandResult HandleLock(string[] args, FunctionCallingChat chat)
    {
        // Check if there's a valid conversation file to save metadata to
        if (!HasValidConversationFile())
        {
            ConsoleHelpers.WriteLine("Error: No conversation file to save metadata to. Use --input-chat-history or create a conversation first.\n", ConsoleColor.Red);
            return SlashCommandResult.Success();
        }
        
        if (chat.Conversation.IsTitleLocked)
        {
            ConsoleHelpers.WriteLine("Title is already locked.\n", ConsoleColor.DarkGray);
            return SlashCommandResult.Success();
        }
        else
        {
            chat.Conversation.LockTitle();
            
            // Update console title (no change in title text, but ensure it's set)
            ConsoleTitleHelper.UpdateWindowTitle(chat.Conversation.Metadata);
            
            ConsoleHelpers.WriteLine("Title locked from AI changes.\n", ConsoleColor.DarkGray);
            return SlashCommandResult.HandledWithSave(); // 🚀 Request immediate save
        }
    }
    
    /// <summary>
    /// Handles /title unlock - unlocks title to allow AI regeneration.
    /// </summary>
    private SlashCommandResult HandleUnlock(string[] args, FunctionCallingChat chat)
    {
        // Check if there's a valid conversation file to save metadata to
        if (!HasValidConversationFile())
        {
            ConsoleHelpers.WriteLine("Error: No conversation file to save metadata to. Use --input-chat-history or create a conversation first.\n", ConsoleColor.Red);
            return SlashCommandResult.Success();
        }
        
        if (!chat.Conversation.IsTitleLocked)
        {
            ConsoleHelpers.WriteLine("Title is already unlocked.\n", ConsoleColor.DarkGray);
            return SlashCommandResult.Success();
        }
        else
        {
            chat.Conversation.UnlockTitle();
            
            // Update console title (no change in title text, but ensure it's set)
            ConsoleTitleHelper.UpdateWindowTitle(chat.Conversation.Metadata);
            
            ConsoleHelpers.WriteLine("Title unlocked - AI can now regenerate the title.\n", ConsoleColor.DarkGray);
            return SlashCommandResult.HandledWithSave(); // 🚀 Request immediate save
        }
    }
    
    /// <summary>
    /// Handles /title refresh - generates new title from current conversation.
    /// </summary>
    private SlashCommandResult HandleRefresh(string[] args, FunctionCallingChat chat)
    {
        // Get the file path to read conversation content from
        var readFilePath = GetConversationReadFilePath();
        if (string.IsNullOrEmpty(readFilePath))
        {
            ConsoleHelpers.WriteLine("Error: No conversation file available for title refresh.\n", ConsoleColor.Red);
            return SlashCommandResult.Success();
        }
        
        // Validate conversation has enough content
        if (!TitleGenerationHelpers.HasSufficientContentForTitleGeneration(readFilePath))
        {
            ConsoleHelpers.WriteLine("Cannot refresh title: conversation needs at least one assistant message.\n", ConsoleColor.Red);
            return SlashCommandResult.Success();
        }
        
        // Start async title generation
        ConsoleHelpers.WriteLine("Generating new title...\n", ConsoleColor.DarkGray);
        _ = Task.Run(async () => await RefreshTitleAsync(chat, readFilePath));
        
        return SlashCommandResult.Success(); // No immediate save - async operation will handle it
    }
    
    /// <summary>
    /// Handles /title revert - reverts to the previous title stored in memory.
    /// </summary>
    private SlashCommandResult HandleRevert(string[] args, FunctionCallingChat chat)
    {
        // Check if there's a valid conversation file to save metadata to
        if (!HasValidConversationFile())
        {
            ConsoleHelpers.WriteLine("Error: No conversation file to save metadata to. Use --input-chat-history or create a conversation first.\n", ConsoleColor.Red);
            return SlashCommandResult.Success();
        }
        
        // Check if there's an old title to revert to
        var oldTitle = chat.Notifications.GetOldTitle();
        if (string.IsNullOrEmpty(oldTitle))
        {
            ConsoleHelpers.WriteLine("No previous title to revert to.\n", ConsoleColor.Red);
            return SlashCommandResult.Success();
        }
        
        // Get current state for logging
        var currentTitle = chat.Conversation.Title;
        var currentLockStatus = chat.Conversation.IsTitleLocked;
        
        // Revert to old title while preserving current lock status
        var wasLocked = currentLockStatus;
        chat.Conversation.Metadata!.Title = oldTitle;
        chat.Conversation.Metadata!.IsTitleLocked = wasLocked; // Preserve lock status
        
        // Clear the old title since it's now used
        chat.Notifications.ClearOldTitle();
        
        // Update console title immediately
        ConsoleTitleHelper.UpdateWindowTitle(chat.Conversation.Metadata);
        
        ConsoleHelpers.WriteLine($"Title reverted to: \"{oldTitle}\"\n", ConsoleColor.DarkGray);
        
        // Clear any pending title notifications since user just reverted the title
        chat.Notifications.ClearPendingOfType(NotificationType.Title);
        
        return SlashCommandResult.HandledWithSave(); // Request immediate save
    }
    
    /// <summary>
    /// Shows current title and available subcommands (default /title behavior).
    /// </summary>
    private void ShowTitleAndHelp(FunctionCallingChat chat)
    {
        ShowCurrentTitle(chat);
        ShowHelp(chat.Conversation.Metadata);
    }
    
    /// <summary>
    /// Shows available title subcommands.
    /// </summary>
    private void ShowHelp(ConversationMetadata? metadata)
    {
        ConsoleHelpers.WriteLine("Available commands:", ConsoleColor.DarkGray);
        ConsoleHelpers.WriteLine("  /title view         Show current title and lock status", ConsoleColor.DarkGray);
        ConsoleHelpers.WriteLine("  /title set \"<text>\" Set title and lock from automatic AI changes", ConsoleColor.DarkGray);
        ConsoleHelpers.WriteLine("  /title lock         Lock current title from automatic AI changes", ConsoleColor.DarkGray);
        ConsoleHelpers.WriteLine("  /title unlock       Unlock title to allow automatic AI regeneration", ConsoleColor.DarkGray);
        ConsoleHelpers.WriteLine("  /title refresh      Generate new title from current conversation", ConsoleColor.DarkGray);
        ConsoleHelpers.WriteLine("  /title revert       Revert to previous title (if available)\n", ConsoleColor.DarkGray);
    }
    
    /// <summary>
    /// Displays the current title and its lock status.
    /// </summary>
    private void ShowCurrentTitle(FunctionCallingChat chat)
    {
        var title = chat.Conversation.Title;
        if (string.IsNullOrEmpty(title)) title = "[null]";
        var oldTitle = chat.Notifications.GetOldTitle() ?? "[null]";
        
        // Show meaningful user status: detailed generating status or lock status (filter out transient completion states)
        var isLocked = chat.Conversation.IsTitleLocked;
        var status = chat.Notifications.GetGenerationStatus(NotificationType.Title) != "Ready"
            ? chat.Notifications.GetGenerationStatus(NotificationType.Title)  // "Generating... (started 5s ago)"
            : isLocked ? "locked" : "unlocked";

        ConsoleHelpers.WriteLine($"Title:     {title}", ConsoleColor.DarkGray);
        ConsoleHelpers.WriteLine($"Previous:  {oldTitle}", ConsoleColor.DarkGray);
        ConsoleHelpers.WriteLine($"Status:    {status}", ConsoleColor.DarkGray);
        
        ConsoleHelpers.WriteLine();
        
        // Clear any pending title notifications since user just viewed the title
        chat.Notifications.ClearPendingOfType(NotificationType.Title);
    }
    
    /// <summary>
    /// Sets a user-provided title and marks it as locked.
    /// </summary>
    private void SetUserTitle(FunctionCallingChat chat, string title)
    {
        // Store current title as old title for revert functionality
        chat.Notifications.SetOldTitle(chat.Conversation.Title);
        
        // Set and lock the title
        chat.Conversation.SetUserTitle(title);
        
        // Update console title immediately
        ConsoleTitleHelper.UpdateWindowTitle(chat.Conversation.Metadata);

        ConsoleHelpers.WriteLine($"Title updated to: \"{title}\" (locked from AI changes)\n", ConsoleColor.DarkGray);
        
        // Clear any pending title notifications since user just set the title
        chat.Notifications.ClearPendingOfType(NotificationType.Title);
    }
    

    
    /// <summary>
    /// Asynchronously generates and updates the conversation title.
    /// </summary>
    private async Task RefreshTitleAsync(FunctionCallingChat chat, string readFilePath)
    {
        // Mark title generation as in progress
        if (!chat.Notifications.TryStartGeneration(NotificationType.Title))
        {
            // Race condition detected - log and return (don't show console message from background thread)
            ConsoleHelpers.WriteDebugLine("Title generation already in progress, skipping duplicate request");
            return;
        }
        
        try
        {
            // Use shared title generation logic
            var generatedTitle = await TitleGenerationHelpers.GenerateTitleAsync(readFilePath);
            
            if (!string.IsNullOrEmpty(generatedTitle))
            {
                // Store current title as old title for revert functionality
                chat.Notifications.SetOldTitle(chat.Conversation.Title);
                
                // Set generated title (unlocked since it's AI-generated)
                chat.Conversation.SetGeneratedTitle(generatedTitle);
                
                // Save the updated conversation to auto-save file (not the input file)
                var saveFilePath = _autoSaveOutputChatHistoryPath ?? readFilePath;
                chat.SaveChatHistoryToFile(saveFilePath, useOpenAIFormat: ChatHistoryDefaults.UseOpenAIFormat);
                
                ConsoleHelpers.WriteDebugLine($"Title refresh: read from {readFilePath}, saved to {saveFilePath}");
                
                // Update console title immediately
                ConsoleTitleHelper.UpdateWindowTitle(chat.Conversation.Metadata);
                
                // Complete generation successfully with notification
                chat.Notifications.CompleteGeneration(NotificationType.Title, generatedTitle, NotificationFormat.Success);
            }
            else
            {
                // Generation returned null/empty - treat as failure
                var errorMessage = "AI could not generate a suitable title for this conversation";
                chat.Notifications.FailGeneration(NotificationType.Title, errorMessage);
                ConsoleHelpers.WriteLine("Failed to generate new title.\n", ConsoleColor.Red);
                ConsoleHelpers.WriteDebugLine("Title generation returned null/empty result");
            }
        }
        catch (FileNotFoundException ex)
        {
            var errorMessage = "Conversation file not found - cannot refresh title";
            chat.Notifications.FailGeneration(NotificationType.Title, errorMessage);
            ConsoleHelpers.WriteDebugLine($"Conversation file not found during title refresh: {ex.Message}");
            ConsoleHelpers.WriteLine("Failed to refresh title - conversation file not found.\n", ConsoleColor.Red);
        }
        catch (UnauthorizedAccessException ex)
        {
            var errorMessage = "Access denied - cannot refresh title";
            chat.Notifications.FailGeneration(NotificationType.Title, errorMessage);
            ConsoleHelpers.WriteDebugLine($"Access denied during title refresh: {ex.Message}");
            ConsoleHelpers.WriteLine("Failed to refresh title - access denied.\n", ConsoleColor.Red);
        }
        catch (Exception ex)
        {
            var errorMessage = "Title generation failed - please try again later";
            chat.Notifications.FailGeneration(NotificationType.Title, errorMessage);
            ConsoleHelpers.WriteDebugLine($"Error during title refresh: {ex.Message}");
            ConsoleHelpers.WriteLine("Failed to refresh title due to an error.\n", ConsoleColor.Red);
        }
        finally
        {
            // Safety net: ensure state returns to idle if something unexpected happened  
            if (chat.Notifications.GetGenerationStatus(NotificationType.Title) != "Ready")
            {
                ConsoleHelpers.WriteDebugLine("Warning: Generation state was stuck in progress, forcing reset to idle");
                chat.Notifications.ResetGeneration(NotificationType.Title);
            }
        }
    }
    

}