using System.Collections.Generic;

namespace CycoDj.Models;

/// <summary>
/// Represents a tree structure of conversations showing branching relationships.
/// </summary>
public class ConversationTree
{
    /// <summary>
    /// Root conversations (those without parents).
    /// </summary>
    public List<Conversation> Roots { get; set; } = new();

    /// <summary>
    /// Lookup dictionary for quick access to conversations by ID.
    /// </summary>
    public Dictionary<string, Conversation> ConversationLookup { get; set; } = new();

    /// <summary>
    /// Gets all conversations in the tree.
    /// </summary>
    public IEnumerable<Conversation> AllConversations => ConversationLookup.Values;

    /// <summary>
    /// Gets total number of conversations in the tree.
    /// </summary>
    public int TotalConversations => ConversationLookup.Count;

    /// <summary>
    /// Gets number of root conversations.
    /// </summary>
    public int RootCount => Roots.Count;
}
