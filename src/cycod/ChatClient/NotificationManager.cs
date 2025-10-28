using System.Collections.Concurrent;

/// <summary>
/// Manages notifications and generation tracking for conversations.
/// </summary>
public class NotificationManager
{
    /// <summary>
    /// Pending notifications to show before next assistant response.
    /// </summary>
    private readonly ConcurrentQueue<NotificationMessage> _pendingNotifications = new();
    
    /// <summary>
    /// Tracks which types of content are currently being generated.
    /// </summary>
    private readonly HashSet<string> _activeGenerations = new();

    /// <summary>
    /// Stores the previous title for revert functionality.
    /// This is memory-only and does not persist across conversation restarts.
    /// </summary>
    private string? _oldTitle;

    /// <summary>
    /// Sets a pending notification to be shown before the next assistant response.
    /// </summary>
    /// <param name="type">The type of notification</param>
    /// <param name="content">The content of the notification</param>
    public void SetPending(NotificationType type, string content)
    {
        _pendingNotifications.Enqueue(new NotificationMessage 
        { 
            Type = type.ToString().ToLowerInvariant(), 
            Content = content 
        });
    }
    
    /// <summary>
    /// Checks if there are any pending notifications.
    /// </summary>
    /// <returns>True if there are pending notifications</returns>
    public bool HasPending()
    {
        return !_pendingNotifications.IsEmpty;
    }
    
    /// <summary>
    /// Gets and clears all pending notifications.
    /// </summary>
    /// <returns>Collection of pending notifications</returns>
    public IEnumerable<NotificationMessage> GetAndClearPending()
    {
        var notifications = new List<NotificationMessage>();
        while (_pendingNotifications.TryDequeue(out var notification))
        {
            notifications.Add(notification);
        }
        return notifications;
    }
    
    /// <summary>
    /// Clears pending notifications of a specific type.
    /// </summary>
    /// <param name="type">The type of notifications to clear</param>
    public void ClearPendingOfType(NotificationType type)
    {
        var typeString = type.ToString().ToLowerInvariant();
        var remainingNotifications = new List<NotificationMessage>();
        
        while (_pendingNotifications.TryDequeue(out var notification))
        {
            if (notification.Type != typeString)
            {
                remainingNotifications.Add(notification);
            }
        }
        
        foreach (var notification in remainingNotifications)
        {
            _pendingNotifications.Enqueue(notification);
        }
    }
    
    /// <summary>
    /// Marks a content type as currently being generated.
    /// </summary>
    /// <param name="type">The type of content being generated</param>
    public void SetGenerationInProgress(NotificationType type)
    {
        lock (_activeGenerations)
        {
            _activeGenerations.Add(type.ToString().ToLowerInvariant());
        }
    }
    
    /// <summary>
    /// Marks a content type as no longer being generated.
    /// </summary>
    /// <param name="type">The type of content that finished generating</param>
    public void ClearGenerationInProgress(NotificationType type)
    {
        lock (_activeGenerations)
        {
            _activeGenerations.Remove(type.ToString().ToLowerInvariant());
        }
    }
    
    /// <summary>
    /// Checks if a content type is currently being generated.
    /// </summary>
    /// <param name="type">The type of content to check</param>
    /// <returns>True if the content type is currently being generated</returns>
    public bool IsGenerationInProgress(NotificationType type)
    {
        lock (_activeGenerations)
        {
            return _activeGenerations.Contains(type.ToString().ToLowerInvariant());
        }
    }

    /// <summary>
    /// Gets the previous title stored for revert functionality.
    /// </summary>
    /// <returns>The old title, or null if none is stored</returns>
    public string? GetOldTitle()
    {
        return _oldTitle;
    }
    
    /// <summary>
    /// Sets the previous title for revert functionality.
    /// </summary>
    /// <param name="oldTitle">The title to store as the previous title</param>
    public void SetOldTitle(string? oldTitle)
    {
        _oldTitle = oldTitle;
    }
    
    /// <summary>
    /// Clears the stored previous title.
    /// </summary>
    public void ClearOldTitle()
    {
        _oldTitle = null;
    }
}