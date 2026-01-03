using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CycoDj.Helpers;

public static class HistoryFileHelpers
{
    /// <summary>
    /// Gets the history directory path for the user
    /// </summary>
    public static string GetHistoryDirectory()
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(userProfile, ".cycod", "history");
    }
    
    /// <summary>
    /// Finds all chat history files in the user's history directory
    /// </summary>
    public static List<string> FindAllHistoryFiles()
    {
        var historyDir = GetHistoryDirectory();
        
        if (!Directory.Exists(historyDir))
        {
            Logger.Warning($"History directory not found: {historyDir}");
            return new List<string>();
        }
        
        try
        {
            var files = Directory.GetFiles(historyDir, "chat-history-*.jsonl")
                .OrderByDescending(f => f)
                .ToList();
                
            Logger.Info($"Found {files.Count} chat history files");
            return files;
        }
        catch (Exception ex)
        {
            Logger.Error($"Error reading history directory: {ex.Message}");
            return new List<string>();
        }
    }
    
    /// <summary>
    /// Filters files by date range
    /// </summary>
    public static List<string> FilterByDateRange(List<string> files, DateTime? after, DateTime? before)
    {
        return files.Where(f =>
        {
            var timestamp = TimestampHelpers.ParseTimestamp(f);
            if (timestamp == DateTime.MinValue) return false;
            
            if (after.HasValue && timestamp < after.Value) return false;
            if (before.HasValue && timestamp > before.Value) return false;
            
            return true;
        }).ToList();
    }
    
    /// <summary>
    /// Filters files by specific date (ignores time component)
    /// </summary>
    public static List<string> FilterByDate(List<string> files, DateTime date)
    {
        return files.Where(f =>
        {
            var timestamp = TimestampHelpers.ParseTimestamp(f);
            return timestamp.Date == date.Date;
        }).ToList();
    }
}
