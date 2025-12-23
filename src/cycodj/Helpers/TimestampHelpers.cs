using System;
using System.IO;

namespace CycoDj.Helpers;

public static class TimestampHelpers
{
    /// <summary>
    /// Extracts timestamp from filename like "chat-history-1754437373970.jsonl"
    /// </summary>
    public static DateTime ParseTimestamp(string filename)
    {
        try
        {
            var name = Path.GetFileNameWithoutExtension(filename);
            var parts = name.Split('-');
            
            // Expected format: chat-history-{timestamp}
            if (parts.Length >= 3 && long.TryParse(parts[2], out var timestamp))
            {
                return DateTimeOffset.FromUnixTimeMilliseconds(timestamp).LocalDateTime;
            }
        }
        catch
        {
            // Fall through to return MinValue
        }
        
        return DateTime.MinValue;
    }
    
    /// <summary>
    /// Formats timestamp for display
    /// </summary>
    public static string FormatTimestamp(DateTime dt, string format = "default")
    {
        return format switch
        {
            "short" => dt.ToString("HH:mm:ss"),
            "date" => dt.ToString("yyyy-MM-dd"),
            "datetime" => dt.ToString("yyyy-MM-dd HH:mm:ss"),
            _ => dt.ToString("yyyy-MM-dd HH:mm:ss")
        };
    }
}
