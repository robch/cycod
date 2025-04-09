using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

/// <summary>
/// Provides methods for working with chat history files.
/// </summary>
public static class ChatHistoryFileHelpers
{
    /// <summary>
    /// Finds the most recent chat history file across all scopes.
    /// </summary>
    /// <returns>The full path to the most recent chat history file, or null if none found</returns>
    public static string? FindMostRecentChatHistoryFile()
    {
        // Find regular chat history files
        var userScopeHistoryFiles = ScopeFileHelpers.FindFilesInScope("chat-history*.jsonl", "history", ConfigFileScope.User);
        var localScopeHistoryFiles = ScopeFileHelpers.FindFilesInScope("chat-history*.jsonl", "history", ConfigFileScope.Local);
        var localFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "chat-history*.jsonl");
        
        // Find exception chat history files - these can also be valid to continue from
        var userScopeExceptionFiles = ScopeFileHelpers.FindFilesInScope("exception-chat-history*.jsonl", "history", ConfigFileScope.User);
        var localScopeExceptionFiles = ScopeFileHelpers.FindFilesInScope("exception-chat-history*.jsonl", "history", ConfigFileScope.Local);
        var localExceptionFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "exception-chat-history*.jsonl");

        // Combine and sort all files by last write time
        var mostRecentFiles = userScopeHistoryFiles
            .Concat(localScopeHistoryFiles)
            .Concat(localFiles)
            .Concat(userScopeExceptionFiles)
            .Concat(localScopeExceptionFiles)
            .Concat(localExceptionFiles)
            .OrderByDescending(f => {
                var fi = new FileInfo(f);
                return $"{fi.LastWriteTime.ToFileTime()} {fi.Name}";
            });
        
        var mostRecent = mostRecentFiles.FirstOrDefault();
        if (!string.IsNullOrEmpty(mostRecent))
        {
            ConsoleHelpers.WriteLine($"Loading: {mostRecent}\n", ConsoleColor.DarkGray);
        }
        
        return mostRecent;
    }

    /// <summary>
    /// Ensures the chat history directory exists in the specified scope.
    /// </summary>
    /// <param name="scope">The scope to create the directory in</param>
    /// <returns>The full path to the history directory</returns>
    public static string EnsureHistoryDirectory(ConfigFileScope scope = ConfigFileScope.User)
    {
        return ScopeFileHelpers.EnsureDirectoryInScope("history", scope);
    }

    /// <summary>
    /// Grounds an input chat history filename.
    /// </summary>
    /// <param name="inputFileName">The input filename</param>
    /// <param name="loadMostRecent">Whether to load the most recent chat history file</param>
    /// <returns>The grounded filename</returns>
    public static string? GroundInputChatHistoryFileName(string? inputFileName, bool loadMostRecent)
    {
        var mostRecentChatHistoryFileName = loadMostRecent
            ? FindMostRecentChatHistoryFile()
            : null;

        var mostRecentChatHistoryFileExists = FileHelpers.FileExists(mostRecentChatHistoryFileName);
        if (mostRecentChatHistoryFileExists)
        {
            inputFileName = mostRecentChatHistoryFileName;
        }

        return FileHelpers.GetFileNameFromTemplate(inputFileName ?? "chat-history.jsonl", inputFileName);
    }

    /// <summary>
    /// Grounds an output chat history filename.
    /// </summary>
    /// <param name="outputFileName">The output filename</param>
    /// <param name="autoSave">Whether to auto-save the chat history</param>
    /// <param name="scope">The scope to save to</param>
    /// <returns>The grounded filename</returns>
    public static string? GroundOutputChatHistoryFileName(string? outputFileName)
    {
        var userSpecified = !string.IsNullOrEmpty(outputFileName);
        var shouldAutoSave = !userSpecified && ConfigStore.Instance.GetFromAnyScope(KnownSettings.AppAutoSaveChatHistory).AsBool(true);
        if (shouldAutoSave)
        {
            var historyDir = EnsureHistoryDirectory();
            outputFileName = Path.Combine(historyDir, "chat-history-{time}.jsonl");
        }

        return FileHelpers.GetFileNameFromTemplate(outputFileName ?? "chat-history.jsonl", outputFileName);
    }

    /// <summary>
    /// Grounds an output trajectory filename.
    /// </summary>
    /// <param name="outputFileName">The output filename</param>
    /// <param name="autoSave">Whether to auto-save the trajectory</param>
    /// <param name="scope">The scope to save to</param>
    /// <returns>The grounded filename</returns>
    public static string? GroundOutputTrajectoryFileName(string? outputFileName)
    {
        var userSpecified = !string.IsNullOrEmpty(outputFileName);
        var shouldAutoSave = !userSpecified && ConfigStore.Instance.GetFromAnyScope(KnownSettings.AppAutoSaveTrajectory).AsBool(true);
        if (shouldAutoSave)
        {
            var historyDir = EnsureHistoryDirectory();
            outputFileName = Path.Combine(historyDir, "trajectory-{time}.jsonl");
        }

        return FileHelpers.GetFileNameFromTemplate(outputFileName ?? "trajectory.jsonl", outputFileName);
    }
}