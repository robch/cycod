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
        var output = GenerateShowOutput();
        
        // Apply instructions if provided
        var finalOutput = ApplyInstructionsIfProvided(output);
        ConsoleHelpers.WriteLine(finalOutput);
        
        return await Task.FromResult(0);
    }
    
    private string GenerateShowOutput()
    {
        var sb = new System.Text.StringBuilder();
        
        if (string.IsNullOrEmpty(ConversationId))
        {
            sb.AppendLine("ERROR: Conversation ID is required");
            sb.AppendLine("Usage: cycodj show <conversation-id>");
            return sb.ToString();
        }

        // Find the conversation file
        var files = HistoryFileHelpers.FindAllHistoryFiles();
        var matchingFile = files.FirstOrDefault(f => 
            f.Contains(ConversationId) || 
            System.IO.Path.GetFileNameWithoutExtension(f) == ConversationId);

        if (matchingFile == null)
        {
            sb.AppendLine($"ERROR: Conversation not found: {ConversationId}");
            sb.AppendLine($"Searched {files.Count} chat history files");
            return sb.ToString();
        }

        // Read the conversation
        var conversation = JsonlReader.ReadConversation(matchingFile);
        if (conversation == null)
        {
            sb.AppendLine($"ERROR: Failed to read conversation from: {matchingFile}");
            return sb.ToString();
        }

        // Load all conversations for branch detection
        var allConversations = JsonlReader.ReadConversations(files);
        BranchDetector.DetectBranches(allConversations);
        
        // Find our conversation in the list (with branch info populated)
        var conv = allConversations.FirstOrDefault(c => c.Id == conversation.Id) ?? conversation;

        // Display header
        sb.AppendLine("═".PadRight(80, '═'));
        
        if (!string.IsNullOrEmpty(conv.Metadata?.Title))
        {
            sb.AppendLine($"## {conv.Metadata.Title}");
        }
        else
        {
            sb.AppendLine($"## Conversation: {conv.Id}");
        }
        
        sb.AppendLine("═".PadRight(80, '═'));
        sb.AppendLine();

        // Display metadata
        var timestamp = TimestampHelpers.FormatTimestamp(conv.Timestamp, "datetime");
        sb.AppendLine($"Timestamp: {timestamp}");
        sb.AppendLine($"File: {conv.FilePath}");
        sb.AppendLine($"Messages: {conv.Messages.Count} total");
        
        var userCount = conv.Messages.Count(m => m.Role == "user");
        var assistantCount = conv.Messages.Count(m => m.Role == "assistant");
        var toolCount = conv.Messages.Count(m => m.Role == "tool");
        var systemCount = conv.Messages.Count(m => m.Role == "system");
        
        sb.Append($"  - {userCount} user, {assistantCount} assistant, {toolCount} tool");
        if (systemCount > 0)
        {
            sb.Append($", {systemCount} system");
        }
        sb.AppendLine();
        
        // Branch information
        if (conv.ParentId != null)
        {
            sb.AppendLine($"Branch of: {conv.ParentId}");
        }
        
        if (conv.BranchIds.Count > 0)
        {
            sb.AppendLine($"Branches: {conv.BranchIds.Count} conversation(s) branch from this");
            foreach (var branchId in conv.BranchIds)
            {
                sb.AppendLine($"  - {branchId}");
            }
        }
        
        if (conv.ToolCallIds.Count > 0)
        {
            sb.AppendLine($"Tool Calls: {conv.ToolCallIds.Count}");
        }
        
        sb.AppendLine();
        sb.AppendLine("─".PadRight(80, '─'));
        sb.AppendLine();

        // Display messages
        var messageNumber = 0;
        foreach (var msg in conv.Messages)
        {
            messageNumber++;
            
            // Skip system messages unless verbose
            if (msg.Role == "system" && !ConsoleHelpers.IsVerbose())
            {
                sb.AppendLine($"[{messageNumber}] {msg.Role} (system prompt - use --verbose to show)");
                sb.AppendLine();
                continue;
            }
            
            // Message header
            sb.AppendLine($"[{messageNumber}] {msg.Role.ToUpper()}");
            
            // Message content
            var content = msg.Content ?? string.Empty;
            
            // Limit content length for tool outputs unless ShowToolOutput is enabled
            if (msg.Role == "tool" && !ShowToolOutput && content.Length > MaxContentLength)
            {
                var truncated = content.Substring(0, MaxContentLength);
                sb.AppendLine(truncated);
                sb.AppendLine($"... (truncated {content.Length - MaxContentLength} chars, use --show-tool-output to see all)");
            }
            else
            {
                // Limit other messages too for readability
                if (content.Length > MaxContentLength * 3)
                {
                    var truncated = content.Substring(0, MaxContentLength * 3);
                    sb.AppendLine(truncated);
                    sb.AppendLine($"... (truncated {content.Length - MaxContentLength * 3} chars)");
                }
                else
                {
                    sb.AppendLine(content);
                }
            }
            
            // Show tool calls if enabled
            if (ShowToolCalls && msg.ToolCalls != null && msg.ToolCalls.Count > 0)
            {
                sb.AppendLine($"Tool Calls: {msg.ToolCalls.Count}");
                foreach (var toolCall in msg.ToolCalls)
                {
                    sb.AppendLine($"  - {toolCall.Id}: {toolCall.Function?.Name ?? "unknown"}");
                }
            }
            
            // Show tool call ID for tool responses
            if (msg.Role == "tool" && !string.IsNullOrEmpty(msg.ToolCallId))
            {
                sb.AppendLine($"(responding to: {msg.ToolCallId})");
            }
            
            sb.AppendLine();
        }
        
        sb.AppendLine("─".PadRight(80, '─'));
        sb.AppendLine($"End of conversation: {conv.Id}");
        
        return sb.ToString();
    }
}
