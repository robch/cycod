/// <summary>
/// Helper for formatting notifications according to their specified format type.
/// </summary>
public static class NotificationFormatter
{
    /// <summary>
    /// Formats a notification message according to its format type.
    /// </summary>
    /// <param name="notification">The notification to format</param>
    /// <returns>The formatted message string</returns>
    public static string Format(NotificationMessage notification)
    {
        return notification.Format switch
        {
            NotificationFormat.UpdatedTo => FormatUpdatedTo(notification),
            NotificationFormat.Status => FormatStatus(notification),
            NotificationFormat.Plain => FormatPlain(notification),
            NotificationFormat.Success => FormatSuccess(notification),
            NotificationFormat.Error => FormatError(notification),
            _ => FormatUpdatedTo(notification) // Default fallback
        };
    }
    
    private static string FormatUpdatedTo(NotificationMessage notification)
    {
        var typeString = notification.Type.ToString().ToLowerInvariant();
        var typeCapitalized = char.ToUpper(typeString[0]) + typeString[1..];
        return $"[{typeCapitalized} updated to: \"{notification.Content}\"]";
    }
    
    private static string FormatStatus(NotificationMessage notification)
    {
        var typeString = notification.Type.ToString().ToLowerInvariant();
        var typeCapitalized = char.ToUpper(typeString[0]) + typeString[1..];
        return $"[{typeCapitalized}: {notification.Content}]";
    }
    
    private static string FormatPlain(NotificationMessage notification)
    {
        return $"[{notification.Content}]";
    }
    
    private static string FormatSuccess(NotificationMessage notification)
    {
        var typeString = notification.Type.ToString().ToLowerInvariant();
        var typeCapitalized = char.ToUpper(typeString[0]) + typeString[1..];
        return $"[✓ {typeCapitalized}: {notification.Content}]";
    }
    
    private static string FormatError(NotificationMessage notification)
    {
        var typeString = notification.Type.ToString().ToLowerInvariant();
        var typeCapitalized = char.ToUpper(typeString[0]) + typeString[1..];
        return $"[✗ {typeCapitalized} failed: {notification.Content}]";
    }
}