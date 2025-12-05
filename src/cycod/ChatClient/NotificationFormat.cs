/// <summary>
/// Defines how a notification should be formatted when displayed to the user.
/// </summary>
public enum NotificationFormat
{
    /// <summary>
    /// Standard "[Type updated to: "content"]" format for content updates.
    /// </summary>
    UpdatedTo,
    
    /// <summary>
    /// Status format "[Type: content]" for general status messages.
    /// </summary>
    Status,
    
    /// <summary>
    /// Plain format "[content]" when the message is self-contained.
    /// </summary>
    Plain,
    
    /// <summary>
    /// Success format "[✓ Type: content]" for positive outcomes.
    /// </summary>
    Success,
    
    /// <summary>
    /// Error format "[✗ Type failed: content]" for failures and errors.
    /// </summary>
    Error
}