using Microsoft.Extensions.AI;
using System.Linq;

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
        // Register subcommands (set is handled specially in TryHandle override)
        _subcommands["view"] = HandleView;
        _subcommands["lock"] = HandleLock;
        _subcommands["unlock"] = HandleUnlock;
        _subcommands["refresh"] = HandleRefresh;
    }
    
    /// <summary>
    /// Override to handle "set" subcommand with raw input validation for quotes.
    /// </summary>
    public override bool TryHandle(string userPrompt, FunctionCallingChat chat, out SlashCommandResult result)
    {
        if (!userPrompt.StartsWith($"/{CommandName}"))
        {
            result = SlashCommandResult.PassToAssistant;
            return false;
        }
        
        var afterCommand = userPrompt.Substring($"/{CommandName}".Length).Trim();
        var args = ParseArgs(afterCommand);
        
        if (args.Length == 0)
        {
            result = HandleDefault(chat);
            return true;
        }
        
        var subcommand = args[0].ToLowerInvariant();
        
        // Handle "set" specially with raw input validation
        if (subcommand == "set")
        {
            var afterSet = afterCommand.Substring(3).Trim(); // Remove "set" and trim
            result = HandleSetWithRawInput(afterSet, chat);
            return true;
        }
        
        // Handle other subcommands normally
        if (_subcommands.TryGetValue(subcommand, out var handler))
        {
            result = handler(args.Skip(1).ToArray(), chat);
            return true;
        }
        
        // Unknown subcommand
        ConsoleHelpers.WriteLine($"Unknown subcommand '{subcommand}' for /{CommandName}.\n", ConsoleColor.Yellow);
        result = SlashCommandResult.Handled;
        return true;
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
            return SlashCommandResult.Handled;
        }
        
        // Check if there's a valid conversation file to save metadata to
        if (!HasValidConversationFile())
        {
            ConsoleHelpers.WriteLine("Error: No conversation file to save metadata to. Use --input-chat-history or create a conversation first.\n", ConsoleColor.Red);
            return SlashCommandResult.Handled;
        }
        
        // Validate that input starts and ends with double quotes
        if (!input.StartsWith("\"") || !input.EndsWith("\"") || input.Length < 2)
        {
            ConsoleHelpers.WriteLine("Error: Titles must be enclosed in double quotes. Usage: /title set \"<title>\"\n", ConsoleColor.Red);
            return SlashCommandResult.Handled;
        }
        
        // Extract content between outermost quotes
        var title = input.Substring(1, input.Length - 2);
        
        // Validate that the title is not empty or just whitespace
        if (string.IsNullOrWhiteSpace(title))
        {
            ConsoleHelpers.WriteLine("Error: Title cannot be empty. Please provide a title.\n", ConsoleColor.Red);
            return SlashCommandResult.Handled;
        }
        
        SetUserTitle(chat, title);
        return SlashCommandResult.NeedsSave; // 🚀 Request immediate save
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
            return SlashCommandResult.Handled;
        }
        
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
        // Check if there's a valid conversation file to save metadata to
        if (!HasValidConversationFile())
        {
            ConsoleHelpers.WriteLine("Error: No conversation file to save metadata to. Use --input-chat-history or create a conversation first.\n", ConsoleColor.Red);
            return SlashCommandResult.Handled;
        }
        
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
        if (!TitleGenerationHelpers.HasSufficientContentForTitleGeneration(readFilePath))
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
        ConsoleHelpers.WriteLine("  /title set \"<text>\" Set title and lock from AI changes", ConsoleColor.DarkGray);
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
        var title = metadata?.Title ?? "[null]";
        
        // Determine status with generation awareness
        var titleGenerationInProgress = chat.IsGenerationInProgress(NotificationType.Title);
        var titleIsLocked = metadata?.IsTitleLocked == true;

        var status = titleGenerationInProgress
            ? "AI title generation in progress..."
            : titleIsLocked ? "locked" : "unlocked";

        ConsoleHelpers.WriteLine($"Title:   {title}", ConsoleColor.DarkGray);
        ConsoleHelpers.WriteLine($"Status:  {status}", ConsoleColor.DarkGray);
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
        
        // Clear any pending title notifications since user just set the title
        chat.ClearPendingNotificationsOfType(NotificationType.Title);
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
            // Use shared title generation logic
            var generatedTitle = await TitleGenerationHelpers.GenerateTitleAsync(readFilePath);
            
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
    

}