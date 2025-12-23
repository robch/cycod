using System;
using System.Collections.Generic;
using System.Linq;
using CycoDj.Models;

namespace CycoDj.Analyzers;

/// <summary>
/// Detects conversation branching by analyzing shared tool_call_id sequences.
/// </summary>
public class BranchDetector
{
    /// <summary>
    /// Analyzes conversations and sets ParentId for branched conversations.
    /// </summary>
    public static void DetectBranches(List<Conversation> conversations)
    {
        foreach (var conv in conversations)
        {
            // Skip if no tool call IDs (can't detect branches)
            if (conv.ToolCallIds.Count == 0)
                continue;

            // Find potential parents (conversations with same prefix)
            var potentialParents = conversations
                .Where(other => other.Id != conv.Id)
                .Where(other => other.ToolCallIds.Count > 0)
                .Where(other => HasCommonPrefix(conv, other))
                .OrderByDescending(other => GetCommonPrefixLength(conv, other))
                .ToList();

            // Parent is the one that's an exact prefix (all its tool_call_ids match conv's beginning)
            foreach (var parent in potentialParents)
            {
                if (IsExactPrefix(parent.ToolCallIds, conv.ToolCallIds))
                {
                    conv.ParentId = parent.Id;
                    parent.BranchIds.Add(conv.Id);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Builds a conversation tree structure showing parent-child relationships.
    /// </summary>
    public static ConversationTree BuildTree(List<Conversation> conversations)
    {
        DetectBranches(conversations);

        var tree = new ConversationTree();
        
        // Find root conversations (those without parents)
        var roots = conversations.Where(c => c.ParentId == null).ToList();
        tree.Roots = roots;

        // Build lookup dictionary
        tree.ConversationLookup = conversations.ToDictionary(c => c.Id, c => c);

        return tree;
    }

    /// <summary>
    /// Checks if two conversations share at least one common tool_call_id at the start.
    /// </summary>
    private static bool HasCommonPrefix(Conversation a, Conversation b)
    {
        if (a.ToolCallIds.Count == 0 || b.ToolCallIds.Count == 0)
            return false;

        return a.ToolCallIds[0] == b.ToolCallIds[0];
    }

    /// <summary>
    /// Returns the number of matching tool_call_ids at the beginning of both lists.
    /// </summary>
    private static int GetCommonPrefixLength(Conversation a, Conversation b)
    {
        var length = 0;
        var minLength = Math.Min(a.ToolCallIds.Count, b.ToolCallIds.Count);

        for (var i = 0; i < minLength; i++)
        {
            if (a.ToolCallIds[i] == b.ToolCallIds[i])
                length++;
            else
                break;
        }

        return length;
    }

    /// <summary>
    /// Checks if the prefix list is an exact prefix of the full list.
    /// Prefix must be shorter and all elements must match.
    /// </summary>
    private static bool IsExactPrefix(List<string> prefix, List<string> full)
    {
        // Prefix must be shorter
        if (prefix.Count >= full.Count)
            return false;

        // All elements of prefix must match beginning of full
        for (var i = 0; i < prefix.Count; i++)
        {
            if (prefix[i] != full[i])
                return false;
        }

        return true;
    }

    /// <summary>
    /// Gets the branch depth of a conversation (how many ancestors it has).
    /// </summary>
    public static int GetBranchDepth(Conversation conv, Dictionary<string, Conversation> lookup)
    {
        var depth = 0;
        var current = conv;

        while (current.ParentId != null && lookup.TryGetValue(current.ParentId, out var parent))
        {
            depth++;
            current = parent;
        }

        return depth;
    }

    /// <summary>
    /// Gets all descendants of a conversation (children, grandchildren, etc.).
    /// </summary>
    public static List<Conversation> GetAllDescendants(Conversation conv, Dictionary<string, Conversation> lookup)
    {
        var descendants = new List<Conversation>();
        
        foreach (var branchId in conv.BranchIds)
        {
            if (lookup.TryGetValue(branchId, out var branch))
            {
                descendants.Add(branch);
                descendants.AddRange(GetAllDescendants(branch, lookup));
            }
        }

        return descendants;
    }
}
