using System.Collections.Concurrent;

/// <summary>
/// Manages notifications and generation tracking for conversations.
/// Now uses proper state machines internally for robust generation tracking.
/// </summary>
public class NotificationManager
{
    /// <summary>
    /// Pending notifications to show before next assistant response.
    /// </summary>
    private readonly ConcurrentQueue<NotificationMessage> _pendingNotifications = new();
    
    /// <summary>
    /// Manages generation state using proper state machines.
    /// </summary>
    private readonly Dictionary<NotificationType, GenerationStateMachine> _generationStates = new();

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
    /// <param name="format">How the notification should be formatted (defaults to UpdatedTo)</param>
    public void SetPending(NotificationType type, string content, NotificationFormat format = NotificationFormat.UpdatedTo)
    {
        _pendingNotifications.Enqueue(new NotificationMessage 
        { 
            Type = type.ToString().ToLowerInvariant(), 
            Content = content,
            Format = format
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
    /// Gets the state machine for a specific notification type, creating one if needed.
    /// </summary>
    private GenerationStateMachine GetStateMachine(NotificationType type)
    {
        if (!_generationStates.TryGetValue(type, out var stateMachine))
        {
            stateMachine = new GenerationStateMachine();
            _generationStates[type] = stateMachine;
        }
        return stateMachine;
    }

    /// <summary>
    /// Attempts to start generation for the specified type.
    /// </summary>
    /// <param name="type">The type of content being generated</param>
    /// <returns>True if generation was started, false if already in progress</returns>
    public bool TryStartGeneration(NotificationType type)
    {
        return GetStateMachine(type).TryStartGeneration();
    }
    
    /// <summary>
    /// Marks a content type as successfully generated.
    /// </summary>
    /// <param name="type">The type of content that finished generating</param>
    /// <param name="generatedContent">The content that was generated</param>
    /// <param name="format">How to format the success notification</param>
    public void CompleteGeneration(NotificationType type, string generatedContent, NotificationFormat format = NotificationFormat.UpdatedTo)
    {
        var stateMachine = GetStateMachine(type);
        if (stateMachine.MarkCompleted())
        {
            SetPending(type, generatedContent, format);
            stateMachine.Reset(); // Ready for next generation
        }
    }
    
    public void FailGeneration(NotificationType type, string errorMessage)
    {
        var stateMachine = GetStateMachine(type);
        if (stateMachine.MarkFailed(errorMessage))
        {
            SetPending(type, errorMessage, NotificationFormat.Error);
            stateMachine.Reset(); // Ready for next generation
        }
    }
    
    public bool IsGenerationInProgress(NotificationType type)
    {
        return GetStateMachine(type).CurrentState == GenerationState.Generating;
    }
    
    /// <summary>
    /// Gets detailed status information about generation state.
    /// </summary>
    /// <param name="type">The type of content to check</param>
    /// <returns>User-friendly status description</returns>
    public string GetGenerationStatus(NotificationType type)
    {
        return GetStateMachine(type).GetStatusDescription();
    }
    
    /// <summary>
    /// Resets generation state to idle (emergency cleanup only).
    /// </summary>
    public void ResetGeneration(NotificationType type)
    {
        GetStateMachine(type).Reset();
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