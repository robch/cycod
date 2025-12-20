using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using CycoDj.Models;

namespace CycoDj.Helpers;

public static class JsonlReader
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Reads a conversation from a JSONL file
    /// </summary>
    public static Conversation? ReadConversation(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                Logger.Warning($"File not found: {filePath}");
                return null;
            }
            
            var lines = File.ReadAllLines(filePath);
            if (lines.Length == 0)
            {
                Logger.Warning($"File is empty: {Path.GetFileName(filePath)}");
                return null;
            }
            
            Logger.Info($"Reading {lines.Length} lines from {Path.GetFileName(filePath)}");
            
            // Check if first line contains metadata
            var (metadata, messageStartIndex) = TryParseMetadata(lines[0]);
            
            if (metadata != null)
            {
                Logger.Info($"Found metadata with title: {metadata.Title ?? "(no title)"}");
            }
            
            var messages = new List<ChatMessage>();
            
            // Parse messages starting from the appropriate index
            for (int i = messageStartIndex; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;
                
                try
                {
                    var msg = JsonSerializer.Deserialize<ChatMessage>(line, JsonOptions);
                    if (msg != null)
                    {
                        messages.Add(msg);
                    }
                }
                catch (JsonException ex)
                {
                    Logger.Warning($"Failed to parse line {i + 1}: {ex.Message}");
                    // Continue parsing other lines
                }
            }
            
            var conversation = new Conversation
            {
                Id = Path.GetFileNameWithoutExtension(filePath),
                FilePath = filePath,
                Timestamp = TimestampHelpers.ParseTimestamp(filePath),
                Metadata = metadata,
                Messages = messages
            };
            
            // Extract tool_call_ids for branch detection
            conversation.ToolCallIds = messages
                .Where(m => !string.IsNullOrEmpty(m.ToolCallId))
                .Select(m => m.ToolCallId!)
                .ToList();
            
            Logger.Info($"Parsed conversation with {messages.Count} messages, {conversation.ToolCallIds.Count} tool call IDs");
            
            return conversation;
        }
        catch (Exception ex)
        {
            Logger.Error($"Error reading {filePath}: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Attempts to parse metadata from the first line of a JSONL file
    /// </summary>
    private static (ConversationMetadata? metadata, int messageStartIndex) TryParseMetadata(string firstLine)
    {
        if (string.IsNullOrWhiteSpace(firstLine) || !firstLine.TrimStart().StartsWith("{\"_meta\":"))
        {
            return (null, 0);
        }

        try
        {
            var wrapper = JsonSerializer.Deserialize<MetadataWrapper>(firstLine, JsonOptions);
            if (wrapper?._meta != null)
            {
                return (wrapper._meta, 1); // Skip first line, start messages at line 1
            }
        }
        catch (JsonException ex)
        {
            Logger.Warning($"Malformed metadata in first line: {ex.Message}");
        }

        return (null, 0); // Treat first line as regular message
    }
    
    /// <summary>
    /// Reads multiple conversations from files
    /// </summary>
    public static List<Conversation> ReadConversations(List<string> filePaths)
    {
        var conversations = new List<Conversation>();
        
        foreach (var file in filePaths)
        {
            var conv = ReadConversation(file);
            if (conv != null)
            {
                conversations.Add(conv);
            }
        }
        
        Logger.Info($"Successfully read {conversations.Count} of {filePaths.Count} conversations");
        return conversations;
    }
}
