using System;
using System.Linq;
using System.Threading.Tasks;
using CycoDj.Analyzers;
using CycoDj.CommandLine;
using CycoDj.Helpers;

namespace CycoDj.CommandLineCommands;

public class ShowCommand : CycoDjCommand
{
    public string ConversationId { get; set; } = string.Empty;
    public bool ShowToolCalls { get; set; } = false;
    public bool ShowToolOutput { get; set; } = false;
    public int MaxContentLength { get; set; } = 500;

    public override async Task<int> ExecuteAsync()
    {
        if (string.IsNullOrEmpty(ConversationId))
        {
            ConsoleHelpers.WriteErrorLine("Conversation ID is required");
            ConsoleHelpers.WriteLine("Usage: cycodj show <conversation-id>", ConsoleColor.Gray);
            return 1;
        }

        // Find the conversation file
        var files = HistoryFileHelpers.FindAllHistoryFiles();
        var matchingFile = files.FirstOrDefault(f => 
            f.Contains(ConversationId) || 
            System.IO.Path.GetFileNameWithoutExtension(f) == ConversationId);

        if (matchingFile == null)
        {
            ConsoleHelpers.WriteErrorLine($"Conversation not found: {ConversationId}");
            ConsoleHelpers.WriteLine($"Searched {files.Count} chat history files", ConsoleColor.Gray);
            return 1;
        }

        // Read the conversation
        var conversation = JsonlReader.ReadConversation(matchingFile);
        if (conversation == null)
        {
            ConsoleHelpers.WriteErrorLine($"Failed to read conversation from: {matchingFile}");
            return 1;
        }

        // Load all conversations for branch detection
        var allConversations = JsonlReader.ReadConversations(files);
        BranchDetector.DetectBranches(allConversations);
        
        // Find our conversation in the list (with branch info populated)
        var conv = allConversations.FirstOrDefault(c => c.Id == conversation.Id) ?? conversation;

        // Display header
        ConsoleHelpers.WriteLine("═".PadRight(80, '═'), ConsoleColor.Cyan);
        
        if (!string.IsNullOrEmpty(conv.Metadata?.Title))
        {
            ConsoleHelpers.WriteLine($"## {conv.Metadata.Title}", ConsoleColor.Cyan);
        }
        else
        {
            ConsoleHelpers.WriteLine($"## Conversation: {conv.Id}", ConsoleColor.Cyan);
        }
        
        ConsoleHelpers.WriteLine("═".PadRight(80, '═'), ConsoleColor.Cyan);
        ConsoleHelpers.WriteLine();

        // Display metadata
        var timestamp = TimestampHelpers.FormatTimestamp(conv.Timestamp, "datetime");
        ConsoleHelpers.Write("Timestamp: ", ConsoleColor.Gray);
        ConsoleHelpers.WriteLine(timestamp, ConsoleColor.White);
        
        ConsoleHelpers.Write("File: ", ConsoleColor.Gray);
        ConsoleHelpers.WriteLine(conv.FilePath, ConsoleColor.DarkGray);
        
        ConsoleHelpers.Write("Messages: ", ConsoleColor.Gray);
        ConsoleHelpers.WriteLine($"{conv.Messages.Count} total", ConsoleColor.White);
        
        var userCount = conv.Messages.Count(m => m.Role == "user");
        var assistantCount = conv.Messages.Count(m => m.Role == "assistant");
        var toolCount = conv.Messages.Count(m => m.Role == "tool");
        var systemCount = conv.Messages.Count(m => m.Role == "system");
        
        ConsoleHelpers.Write("  - ", ConsoleColor.Gray);
        ConsoleHelpers.Write($"{userCount} user", ConsoleColor.Green);
        ConsoleHelpers.Write($", ", ConsoleColor.Gray);
        ConsoleHelpers.Write($"{assistantCount} assistant", ConsoleColor.Blue);
        ConsoleHelpers.Write($", ", ConsoleColor.Gray);
        ConsoleHelpers.Write($"{toolCount} tool", ConsoleColor.DarkGray);
        if (systemCount > 0)
        {
            ConsoleHelpers.Write($", ", ConsoleColor.Gray);
            ConsoleHelpers.Write($"{systemCount} system", ConsoleColor.DarkMagenta);
        }
        ConsoleHelpers.WriteLine();
        
        // Branch information
        if (conv.ParentId != null)
        {
            ConsoleHelpers.Write("Branch of: ", ConsoleColor.Yellow);
            ConsoleHelpers.WriteLine(conv.ParentId, ConsoleColor.White);
        }
        
        if (conv.BranchIds.Count > 0)
        {
            ConsoleHelpers.Write("Branches: ", ConsoleColor.Yellow);
            ConsoleHelpers.WriteLine($"{conv.BranchIds.Count} conversation(s) branch from this", ConsoleColor.White);
            foreach (var branchId in conv.BranchIds)
            {
                ConsoleHelpers.WriteLine($"  - {branchId}", ConsoleColor.DarkGray);
            }
        }
        
        if (conv.ToolCallIds.Count > 0)
        {
            ConsoleHelpers.Write("Tool Calls: ", ConsoleColor.Gray);
            ConsoleHelpers.WriteLine($"{conv.ToolCallIds.Count}", ConsoleColor.White);
        }
        
        ConsoleHelpers.WriteLine();
        ConsoleHelpers.WriteLine("─".PadRight(80, '─'), ConsoleColor.DarkGray);
        ConsoleHelpers.WriteLine();

        // Display messages
        var messageNumber = 0;
        foreach (var msg in conv.Messages)
        {
            messageNumber++;
            
            // Skip system messages unless verbose
            if (msg.Role == "system" && !ConsoleHelpers.IsVerbose())
            {
                ConsoleHelpers.WriteLine($"[{messageNumber}] {msg.Role} (system prompt - use --verbose to show)", ConsoleColor.DarkMagenta);
                ConsoleHelpers.WriteLine();
                continue;
            }
            
            // Message header
            var roleColor = msg.Role switch
            {
                "user" => ConsoleColor.Green,
                "assistant" => ConsoleColor.Blue,
                "tool" => ConsoleColor.DarkGray,
                "system" => ConsoleColor.DarkMagenta,
                _ => ConsoleColor.White
            };
            
            ConsoleHelpers.Write($"[{messageNumber}] ", ConsoleColor.DarkGray);
            ConsoleHelpers.WriteLine(msg.Role.ToUpper(), roleColor);
            
            // Message content
            var content = msg.Content ?? string.Empty;
            
            // Limit content length for tool outputs unless ShowToolOutput is enabled
            if (msg.Role == "tool" && !ShowToolOutput && content.Length > MaxContentLength)
            {
                var truncated = content.Substring(0, MaxContentLength);
                ConsoleHelpers.WriteLine(truncated, ConsoleColor.Gray);
                ConsoleHelpers.WriteLine($"... (truncated {content.Length - MaxContentLength} chars, use --show-tool-output to see all)", ConsoleColor.DarkGray);
            }
            else
            {
                // Limit other messages too for readability
                if (content.Length > MaxContentLength * 3)
                {
                    var truncated = content.Substring(0, MaxContentLength * 3);
                    ConsoleHelpers.WriteLine(truncated, roleColor);
                    ConsoleHelpers.WriteLine($"... (truncated {content.Length - MaxContentLength * 3} chars)", ConsoleColor.DarkGray);
                }
                else
                {
                    ConsoleHelpers.WriteLine(content, roleColor);
                }
            }
            
            // Show tool calls if enabled
            if (ShowToolCalls && msg.ToolCalls != null && msg.ToolCalls.Count > 0)
            {
                ConsoleHelpers.WriteLine($"Tool Calls: {msg.ToolCalls.Count}", ConsoleColor.Yellow);
                foreach (var toolCall in msg.ToolCalls)
                {
                    ConsoleHelpers.WriteLine($"  - {toolCall.Id}: {toolCall.Function?.Name ?? "unknown"}", ConsoleColor.DarkYellow);
                }
            }
            
            // Show tool call ID for tool responses
            if (msg.Role == "tool" && !string.IsNullOrEmpty(msg.ToolCallId))
            {
                ConsoleHelpers.WriteLine($"(responding to: {msg.ToolCallId})", ConsoleColor.DarkGray);
            }
            
            ConsoleHelpers.WriteLine();
        }
        
        ConsoleHelpers.WriteLine("─".PadRight(80, '─'), ConsoleColor.DarkGray);
        ConsoleHelpers.WriteLine($"End of conversation: {conv.Id}", ConsoleColor.Gray);

        return await Task.FromResult(0);
    }
}
