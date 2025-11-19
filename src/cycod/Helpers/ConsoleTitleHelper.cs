/// <summary>
/// Helper for updating the console window title based on conversation metadata.
/// </summary>
public static class ConsoleTitleHelper
{
    private const string DefaultTitle = "cycod";
    private const string NoTitleSetText = "No title set";
    private const string GeneratedTitlePrefix = "conversation-";
    /// <summary>
    /// Updates the console window title based on conversation metadata.
    /// </summary>
    /// <param name="metadata">Current conversation metadata</param>
    public static void UpdateWindowTitle(ConversationMetadata? metadata)
    {
        try
        {
            var title = GetDisplayTitle(metadata);
            var titleIsValid = !string.IsNullOrEmpty(title);
            var titleIsNotDefault = title != NoTitleSetText;
            var titleIsNotGenerated = !title.StartsWith(GeneratedTitlePrefix);

            if (titleIsValid && titleIsNotDefault && titleIsNotGenerated)
            {
                Console.Title = title;
            }
            else
            {
                Console.Title = DefaultTitle;
            }
        }
        catch (Exception ex)
        {
            // Console.Title can fail in some environments (e.g., CI/CD, certain terminals)
            ConsoleHelpers.WriteDebugLine($"Failed to set console title: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Gets the display title from metadata, using fallback if needed.
    /// </summary>
    /// <param name="metadata">Conversation metadata</param>
    /// <returns>Display-friendly title</returns>
    private static string GetDisplayTitle(ConversationMetadata? metadata)
    {
        // Use metadata title if available
        if (!string.IsNullOrEmpty(metadata?.Title))
        {
            return metadata.Title;
        }
        
        // Ultimate fallback
        return "Untitled Conversation";
    }
}