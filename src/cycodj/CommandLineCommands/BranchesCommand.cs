using System;
using System.Linq;
using System.Threading.Tasks;
using CycoDj.Analyzers;
using CycoDj.CommandLine;
using CycoDj.Helpers;

namespace CycoDj.CommandLineCommands;

public class BranchesCommand : CycoDjCommand
{
    public string? Date { get; set; }
    public string? Conversation { get; set; }
    public bool Verbose { get; set; } = false;
    public int Last { get; set; } = 0;
    public int? MessageCount { get; set; } = null; // null = use default (0 for branches)
    public bool ShowStats { get; set; } = false;

    public override async Task<int> ExecuteAsync()
    {
        var output = GenerateBranchesOutput();
        
        // Apply instructions if provided
        var finalOutput = ApplyInstructionsIfProvided(output);
        
        // Save to file if --save-output was provided
        if (SaveOutputIfRequested(finalOutput))
        {
            return await Task.FromResult(0);
        }
        
        // Otherwise print to console
        ConsoleHelpers.WriteLine(finalOutput);
        
        return await Task.FromResult(0);
    }
    
    private string GenerateBranchesOutput()
    {
        var sb = new System.Text.StringBuilder();
        
        // Find all history files
        var files = HistoryFileHelpers.FindAllHistoryFiles();
        
        if (files.Count == 0)
        {
            sb.AppendLine("WARNING: No chat history files found");
            return sb.ToString();
        }
        
        // Filter by time range if After/Before are set
        if (After.HasValue || Before.HasValue)
        {
            files = HistoryFileHelpers.FilterByDateRange(files, After, Before);
            
            if (After.HasValue && Before.HasValue)
            {
                sb.AppendLine($"Filtered by time range: {After:yyyy-MM-dd HH:mm} to {Before:yyyy-MM-dd HH:mm} ({files.Count} files)");
            }
            else if (After.HasValue)
            {
                sb.AppendLine($"Filtered: after {After:yyyy-MM-dd HH:mm} ({files.Count} files)");
            }
            else if (Before.HasValue)
            {
                sb.AppendLine($"Filtered: before {Before:yyyy-MM-dd HH:mm} ({files.Count} files)");
            }
            sb.AppendLine();
        }
        // Filter by date if specified (backward compat)
        else if (!string.IsNullOrEmpty(Date))
        {
            if (DateTime.TryParse(Date, out var dateFilter))
            {
                files = HistoryFileHelpers.FilterByDate(files, dateFilter);
            }
            else
            {
                sb.AppendLine($"ERROR: Invalid date format: {Date}");
                return sb.ToString();
            }
        }
        
        // Read conversations
        var conversations = JsonlReader.ReadConversations(files);
        
        if (conversations.Count == 0)
        {
            sb.AppendLine("WARNING: No conversations could be read");
            return sb.ToString();
        }
        
        // Apply --last N limit if specified
        if (Last > 0)
        {
            conversations = conversations
                .OrderByDescending(c => c.Timestamp)
                .Take(Last)
                .OrderBy(c => c.Timestamp)
                .ToList();
        }
        
        // Build conversation tree
        var tree = BranchDetector.BuildTree(conversations);
        
        // If specific conversation requested, show just that branch
        if (!string.IsNullOrEmpty(Conversation))
        {
            AppendSingleConversationBranches(sb, tree);
            return sb.ToString();
        }
        
        // Show full tree
        sb.AppendLine("## Conversation Tree");
        sb.AppendLine();
        
        if (tree.Roots.Count == 0)
        {
            sb.AppendLine("WARNING: No root conversations found (all conversations appear to be orphaned branches)");
            return sb.ToString();
        }
        
        // Display each root and its descendants
        foreach (var root in tree.Roots.OrderBy(r => r.Timestamp))
        {
            AppendConversationTree(sb, root, tree, 0);
        }
        
        // Show statistics
        sb.AppendLine();
        sb.AppendLine($"Total conversations: {tree.TotalConversations}");
        sb.AppendLine($"Root conversations: {tree.RootCount}");
        
        var branchedCount = tree.AllConversations.Count(c => c.ParentId != null);
        if (branchedCount > 0)
        {
            sb.AppendLine($"Branched conversations: {branchedCount}");
        }
        
        // Add detailed statistics if requested
        if (ShowStats && tree.AllConversations.Any())
        {
            sb.AppendLine();
            sb.AppendLine("═══════════════════════════════════════");
            sb.AppendLine("## Statistics Summary");
            sb.AppendLine("═══════════════════════════════════════");
            sb.AppendLine();
            
            var totalMessages = tree.AllConversations.Sum(c => c.Messages.Count);
            var totalUserMessages = tree.AllConversations.Sum(c => c.Messages.Count(m => m.Role == "user"));
            var totalAssistantMessages = tree.AllConversations.Sum(c => c.Messages.Count(m => m.Role == "assistant"));
            var totalToolMessages = tree.AllConversations.Sum(c => c.Messages.Count(m => m.Role == "tool"));
            var avgMessages = totalMessages / (double)tree.AllConversations.Count();
            var avgDepth = tree.AllConversations.Average(c => (double)GetDepth(c, tree));
            
            sb.AppendLine($"Total conversations: {tree.TotalConversations}");
            sb.AppendLine($"Root conversations: {tree.RootCount}");
            sb.AppendLine($"Branched conversations: {branchedCount} ({branchedCount * 100.0 / tree.TotalConversations:F1}%)");
            sb.AppendLine();
            sb.AppendLine($"Total messages: {totalMessages:N0}");
            sb.AppendLine($"  User: {totalUserMessages:N0} ({totalUserMessages * 100.0 / totalMessages:F1}%)");
            sb.AppendLine($"  Assistant: {totalAssistantMessages:N0} ({totalAssistantMessages * 100.0 / totalMessages:F1}%)");
            sb.AppendLine($"  Tool: {totalToolMessages:N0} ({totalToolMessages * 100.0 / totalMessages:F1}%)");
            sb.AppendLine();
            sb.AppendLine($"Average messages/conversation: {avgMessages:F1}");
            sb.AppendLine($"Average branch depth: {avgDepth:F1}");
            sb.AppendLine();
            sb.AppendLine("═══════════════════════════════════════");
        }
        
        return sb.ToString();
    }
    
    private int GetDepth(Models.Conversation conv, Models.ConversationTree tree)
    {
        var depth = 0;
        var current = conv;
        while (current.ParentId != null && tree.ConversationLookup.TryGetValue(current.ParentId, out var parent))
        {
            depth++;
            current = parent;
        }
        return depth;
    }

    private void AppendConversationTree(System.Text.StringBuilder sb, Models.Conversation conv, Models.ConversationTree tree, int depth)
    {
        var indent = new string(' ', depth * 2);
        var branch = depth > 0 ? "├─ " : "📁 ";
        
        // Format timestamp
        var timestamp = TimestampHelpers.FormatTimestamp(conv.Timestamp, "datetime");
        
        // Show conversation
        var title = conv.GetDisplayTitle();
        var displayTitle = title.Length > 60 ? title.Substring(0, 60) + "..." : title;
        sb.AppendLine($"{indent}{branch}{timestamp} - {displayTitle}");
        
        // Show verbose info if requested
        if (Verbose)
        {
            var userCount = conv.Messages.Count(m => m.Role == "user");
            var assistantCount = conv.Messages.Count(m => m.Role == "assistant");
            
            sb.AppendLine($"{indent}   Messages: {userCount} user, {assistantCount} assistant");
            sb.AppendLine($"{indent}   Tool calls: {conv.ToolCallIds.Count}");
            
            if (conv.ParentId != null && tree.ConversationLookup.TryGetValue(conv.ParentId, out var parent))
            {
                var commonLength = GetCommonPrefixLength(parent.ToolCallIds, conv.ToolCallIds);
                var divergeAt = commonLength < parent.ToolCallIds.Count ? commonLength : parent.ToolCallIds.Count;
                sb.AppendLine($"{indent}   Branched at tool call #{divergeAt}");
            }
        }
        
        // Show messages if requested
        var messageCount = MessageCount ?? 0; // Default to 0 for branches
        if (messageCount > 0)
        {
            var userMessages = conv.Messages.Where(m => m.Role == "user" && !string.IsNullOrWhiteSpace(m.Content)).ToList();
            
            if (userMessages.Any())
            {
                // For branches, show last N messages (what's new)
                // For roots, show first N messages
                var messagesToShow = conv.ParentId != null 
                    ? userMessages.TakeLast(Math.Min(messageCount, userMessages.Count))
                    : userMessages.Take(Math.Min(messageCount, userMessages.Count));
                
                sb.AppendLine();
                foreach (var msg in messagesToShow)
                {
                    var preview = msg.Content.Length > 150 
                        ? msg.Content.Substring(0, 150) + "..." 
                        : msg.Content;
                    preview = preview.Replace("\n", " ").Replace("\r", "");
                    
                    sb.AppendLine($"{indent}   > {preview}");
                }
            }
        }
        
        // Recursively display children  
        var sortedBranchIds = conv.BranchIds
            .Select(id => new { Id = id, Timestamp = tree.ConversationLookup.TryGetValue(id, out var tempConv) ? tempConv.Timestamp : DateTime.MinValue })
            .OrderBy(x => x.Timestamp)
            .Select(x => x.Id)
            .ToList();
            
        foreach (var branchId in sortedBranchIds)
        {
            if (tree.ConversationLookup.TryGetValue(branchId, out var childBranch))
            {
                AppendConversationTree(sb, childBranch, tree, depth + 1);
            }
        }
    }

    private void AppendSingleConversationBranches(System.Text.StringBuilder sb, Models.ConversationTree tree)
    {
        // Find the conversation
        var conv = tree.AllConversations.FirstOrDefault(c => 
            c.Id.Contains(Conversation!) || c.GetDisplayTitle().Contains(Conversation!));
        
        if (conv == null)
        {
            sb.AppendLine($"ERROR: Conversation not found: {Conversation}");
            return;
        }
        
        sb.AppendLine($"## Branches for: {conv.GetDisplayTitle()}");
        sb.AppendLine();
        
        // Show parent chain
        if (conv.ParentId != null)
        {
            sb.AppendLine("Parent chain:");
            AppendParentChain(sb, conv, tree);
            sb.AppendLine();
        }
        
        // Show this conversation
        var timestamp = TimestampHelpers.FormatTimestamp(conv.Timestamp, "datetime");
        sb.AppendLine($"📍 {timestamp} - {conv.GetDisplayTitle()}");
        sb.AppendLine($"   Tool calls: {conv.ToolCallIds.Count}");
        sb.AppendLine();
        
        // Show children
        if (conv.BranchIds.Count > 0)
        {
            sb.AppendLine($"Branches ({conv.BranchIds.Count}):");
            foreach (var branchId in conv.BranchIds)
            {
                if (tree.ConversationLookup.TryGetValue(branchId, out var branch))
                {
                    var branchTimestamp = TimestampHelpers.FormatTimestamp(branch.Timestamp, "datetime");
                    sb.AppendLine($"  ├─ {branchTimestamp} - {branch.GetDisplayTitle()}");
                }
            }
        }
        else
        {
            sb.AppendLine("No branches from this conversation");
        }
    }

    private void AppendParentChain(System.Text.StringBuilder sb, Models.Conversation conv, Models.ConversationTree tree)
    {
        var chain = new System.Collections.Generic.List<Models.Conversation>();
        var current = conv;
        
        while (current.ParentId != null && tree.ConversationLookup.TryGetValue(current.ParentId, out var parent))
        {
            chain.Insert(0, parent);
            current = parent;
        }
        
        for (var i = 0; i < chain.Count; i++)
        {
            var indent = new string(' ', i * 2);
            var timestamp = TimestampHelpers.FormatTimestamp(chain[i].Timestamp, "datetime");
            sb.AppendLine($"{indent}↑ {timestamp} - {chain[i].GetDisplayTitle()}");
        }
    }

    private int GetCommonPrefixLength(System.Collections.Generic.List<string> a, System.Collections.Generic.List<string> b)
    {
        var length = 0;
        var minLength = Math.Min(a.Count, b.Count);
        
        for (var i = 0; i < minLength; i++)
        {
            if (a[i] == b[i])
                length++;
            else
                break;
        }
        
        return length;
    }
}
