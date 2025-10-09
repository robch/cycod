/// <summary>
/// Handles the /title slash command for viewing and setting conversation titles.
/// </summary>
public class SlashTitleCommandHandler
{
    /// <summary>
    /// Attempts to handle a /title command.
    /// </summary>
    /// <param name="userPrompt">The user's input prompt</param>
    /// <param name="chat">The current chat instance</param>
    /// <returns>True if the command was handled, false otherwise</returns>
    public bool TryHandle(string userPrompt, FunctionCallingChat chat)
    {
        if (!userPrompt.StartsWith("/title"))
        {
            return false;
        }

        var titleText = userPrompt.Substring("/title".Length).Trim();
        
        if (string.IsNullOrEmpty(titleText))
        {
            // Show current title and lock status
            ShowCurrentTitle(chat.Metadata);
        }
        else
        {
            // Set new title and lock it
            SetUserTitle(chat, titleText);
        }
        
        return true; // Skip assistant response
    }
    
    /// <summary>
    /// Displays the current title and its lock status.
    /// </summary>
    /// <param name="metadata">Conversation metadata</param>
    private void ShowCurrentTitle(ConversationMetadata? metadata)
    {
        var title = metadata?.Title ?? "No title set";
        var status = metadata?.TitleLocked == true 
            ? "Locked (user-edited)" 
            : "Unlocked (AI-generated or default)";
        
        ConsoleHelpers.WriteLine($"Current title: \"{title}\"", ConsoleColor.Yellow);
        ConsoleHelpers.WriteLine($"Status: {status}\n", ConsoleColor.DarkGray);
    }
    
    /// <summary>
    /// Sets a user-provided title and marks it as locked.
    /// </summary>
    /// <param name="chat">The chat instance</param>
    /// <param name="title">The new title</param>
    private void SetUserTitle(FunctionCallingChat chat, string title)
    {
        // Initialize metadata if not present
        var metadata = chat.Metadata ?? ConversationMetadataHelpers.CreateDefault();
        
        // Set and lock the title
        ConversationMetadataHelpers.SetUserTitle(metadata, title);
        
        // Update the chat's metadata
        chat.UpdateMetadata(metadata);
        
        ConsoleHelpers.WriteLine($"Title updated to: \"{title}\" (locked from AI changes)\n", ConsoleColor.Yellow);
    }
}