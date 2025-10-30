/// <summary>
/// Represents a pending notification message for the user.
/// </summary>
public class NotificationMessage
{
    /// <summary>
    /// The type of notification (e.g., "title", "description").
    /// </summary>
    public NotificationType Type { get; set; } = NotificationType.Title;
    
    /// <summary>
    /// The content of the notification.
    /// </summary>
    public string Content { get; set; } = string.Empty;
    
    /// <summary>
    /// How this notification should be formatted when displayed.
    /// </summary>
    public NotificationFormat Format { get; set; } = NotificationFormat.UpdatedTo;
    
    /// <summary>
    /// When the notification was created.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}