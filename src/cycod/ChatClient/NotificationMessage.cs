/// <summary>
/// Represents a pending notification message for the user.
/// </summary>
public class NotificationMessage
{
    /// <summary>
    /// The type of notification (e.g., "title", "description").
    /// </summary>
    public string Type { get; set; } = string.Empty;
    
    /// <summary>
    /// The content of the notification.
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// When the notification was created.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}