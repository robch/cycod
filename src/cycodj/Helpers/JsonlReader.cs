using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using CycoDj.Models;

namespace CycoDj.Helpers;

public static class JsonlReader
{
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
            var messages = new List<ChatMessage>();
            
            Logger.Info($"Reading {lines.Length} lines from {Path.GetFileName(filePath)}");
            
            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                
                try
                {
                    var msg = JsonSerializer.Deserialize<ChatMessage>(line);
                    if (msg != null)
                    {
                        messages.Add(msg);
                    }
                }
                catch (JsonException ex)
                {
                    Logger.Warning($"Failed to parse line: {ex.Message}");
                    // Continue parsing other lines
                }
            }
            
            var conversation = new Conversation
            {
                Id = Path.GetFileNameWithoutExtension(filePath),
                FilePath = filePath,
                Timestamp = TimestampHelpers.ParseTimestamp(filePath),
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
