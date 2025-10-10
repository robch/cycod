/// <summary>
/// Helper for updating the console window title based on conversation metadata.
/// </summary>
public static class ConsoleTitleHelper
{
    /// <summary>
    /// Updates the console window title based on conversation metadata.
    /// </summary>
    /// <param name="metadata">Current conversation metadata</param>
    public static void UpdateWindowTitle(ConversationMetadata? metadata)
    {
        try
        {
            var title = GetDisplayTitle(metadata);
            if (!string.IsNullOrEmpty(title) && title != "No title set" && !title.StartsWith("conversation-"))
            {
                Console.Title = title;
            }
            else
            {
                Console.Title = "cycod";
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
        return ConversationMetadataHelpers.GetDisplayTitle(metadata, "");
    }
}