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

    public override async Task<int> ExecuteAsync()
    {
        // Find all history files
        var files = HistoryFileHelpers.FindAllHistoryFiles();
        
        if (files.Count == 0)
        {
            ConsoleHelpers.WriteWarning("No chat history files found");
            return 1;
        }
        
        // Filter by date if specified
        if (!string.IsNullOrEmpty(Date))
        {
            if (DateTime.TryParse(Date, out var dateFilter))
            {
                files = HistoryFileHelpers.FilterByDate(files, dateFilter);
            }
            else
            {
                ConsoleHelpers.WriteErrorLine($"Invalid date format: {Date}");
                return 1;
            }
        }
        
        // Read conversations
        var conversations = JsonlReader.ReadConversations(files);
        
        if (conversations.Count == 0)
        {
            ConsoleHelpers.WriteWarning("No conversations could be read");
            return 1;
        }
        
        // Build conversation tree
        var tree = BranchDetector.BuildTree(conversations);
        
        // If specific conversation requested, show just that branch
        if (!string.IsNullOrEmpty(Conversation))
        {
            ShowSingleConversationBranches(tree);
            return 0;
        }
        
        // Show full tree
        ConsoleHelpers.WriteLine("## Conversation Tree", ConsoleColor.Cyan);
        ConsoleHelpers.WriteLine();
        
        if (tree.Roots.Count == 0)
        {
            ConsoleHelpers.WriteWarning("No root conversations found (all conversations appear to be orphaned branches)");
            return 0;
        }
        
        // Display each root and its descendants
        foreach (var root in tree.Roots.OrderBy(r => r.Timestamp))
        {
            DisplayConversationTree(root, tree, 0);
        }
        
        // Show statistics
        ConsoleHelpers.WriteLine();
        ConsoleHelpers.WriteLine($"Total conversations: {tree.TotalConversations}", ConsoleColor.Green);
        ConsoleHelpers.WriteLine($"Root conversations: {tree.RootCount}", ConsoleColor.Green);
        
        var branchedCount = tree.AllConversations.Count(c => c.ParentId != null);
        if (branchedCount > 0)
        {
            ConsoleHelpers.WriteLine($"Branched conversations: {branchedCount}", ConsoleColor.Yellow);
        }
        
        return await Task.FromResult(0);
    }

    private void DisplayConversationTree(Models.Conversation conv, Models.ConversationTree tree, int depth)
    {
        var indent = new string(' ', depth * 2);
        var branch = depth > 0 ? "├─ " : "📁 ";
        
        // Format timestamp
        var timestamp = TimestampHelpers.FormatTimestamp(conv.Timestamp, "datetime");
        
        // Show conversation
        ConsoleHelpers.Write($"{indent}{branch}", ConsoleColor.Gray);
        ConsoleHelpers.Write($"{timestamp}", ConsoleColor.White);
        ConsoleHelpers.Write($" - ", ConsoleColor.Gray);
        
        var title = conv.GetDisplayTitle();
        var displayTitle = title.Length > 60 ? title.Substring(0, 60) + "..." : title;
        ConsoleHelpers.WriteLine($"{displayTitle}", ConsoleColor.Cyan);
        
        // Show verbose info if requested
        if (Verbose)
        {
            var userCount = conv.Messages.Count(m => m.Role == "user");
            var assistantCount = conv.Messages.Count(m => m.Role == "assistant");
            
            ConsoleHelpers.WriteLine($"{indent}   Messages: {userCount} user, {assistantCount} assistant", ConsoleColor.DarkGray);
            ConsoleHelpers.WriteLine($"{indent}   Tool calls: {conv.ToolCallIds.Count}", ConsoleColor.DarkGray);
            
            if (conv.ParentId != null && tree.ConversationLookup.TryGetValue(conv.ParentId, out var parent))
            {
                var commonLength = GetCommonPrefixLength(parent.ToolCallIds, conv.ToolCallIds);
                var divergeAt = commonLength < parent.ToolCallIds.Count ? commonLength : parent.ToolCallIds.Count;
                ConsoleHelpers.WriteLine($"{indent}   Branched at tool call #{divergeAt}", ConsoleColor.Yellow);
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
                DisplayConversationTree(childBranch, tree, depth + 1);
            }
        }
    }

    private void ShowSingleConversationBranches(Models.ConversationTree tree)
    {
        // Find the conversation
        var conv = tree.AllConversations.FirstOrDefault(c => 
            c.Id.Contains(Conversation!) || c.GetDisplayTitle().Contains(Conversation!));
        
        if (conv == null)
        {
            ConsoleHelpers.WriteErrorLine($"Conversation not found: {Conversation}");
            return;
        }
        
        ConsoleHelpers.WriteLine($"## Branches for: {conv.GetDisplayTitle()}", ConsoleColor.Cyan);
        ConsoleHelpers.WriteLine();
        
        // Show parent chain
        if (conv.ParentId != null)
        {
            ConsoleHelpers.WriteLine("Parent chain:", ConsoleColor.Yellow);
            ShowParentChain(conv, tree);
            ConsoleHelpers.WriteLine();
        }
        
        // Show this conversation
        var timestamp = TimestampHelpers.FormatTimestamp(conv.Timestamp, "datetime");
        ConsoleHelpers.WriteLine($"📍 {timestamp} - {conv.GetDisplayTitle()}", ConsoleColor.White);
        ConsoleHelpers.WriteLine($"   Tool calls: {conv.ToolCallIds.Count}", ConsoleColor.Gray);
        ConsoleHelpers.WriteLine();
        
        // Show children
        if (conv.BranchIds.Count > 0)
        {
            ConsoleHelpers.WriteLine($"Branches ({conv.BranchIds.Count}):", ConsoleColor.Yellow);
            foreach (var branchId in conv.BranchIds)
            {
                if (tree.ConversationLookup.TryGetValue(branchId, out var branch))
                {
                    var branchTimestamp = TimestampHelpers.FormatTimestamp(branch.Timestamp, "datetime");
                    ConsoleHelpers.WriteLine($"  ├─ {branchTimestamp} - {branch.GetDisplayTitle()}", ConsoleColor.Cyan);
                }
            }
        }
        else
        {
            ConsoleHelpers.WriteLine("No branches from this conversation", ConsoleColor.Gray);
        }
    }

    private void ShowParentChain(Models.Conversation conv, Models.ConversationTree tree)
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
            ConsoleHelpers.WriteLine($"{indent}↑ {timestamp} - {chain[i].GetDisplayTitle()}", ConsoleColor.DarkGray);
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
