# cycodj list Command - Layer 5: Context Expansion

## Status: NOT IMPLEMENTED

The `list` command does **not implement Layer 5 (Context Expansion)**.

## What is Layer 5?

Layer 5 (Context Expansion) provides the ability to show surrounding lines or messages around matched content to provide context.

## Why Not Implemented in list?

The `list` command displays conversation summaries with message previews. It does not perform content searching or matching, so there are no "matches" around which to expand context.

### What list Does Instead

The list command provides **message previews** (Layer 6: Display Control):
- `--messages [N|all]` - Controls how many preview messages to show per conversation
- Default: 3 messages
- Shows first N messages for root conversations
- Shows last N messages for branched conversations

This is **NOT context expansion** because:
1. It's not expanding around matches
2. It's a fixed preview count, not relative to search results
3. It applies to all conversations uniformly, not based on content relevance

## Example of What list Shows

```bash
cycodj list --messages 2
```

Output:
```
2024-01-15 10:30:00 - Project Discussion (chat-history-20240115-103000-123456)
  Messages: 5 user, 4 assistant, 0 tool
  > How do we implement the new feature?
  > I need help with the database schema
```

This shows 2 preview messages from the conversation, but it's not expanding around specific matched content.

## Related Options

### Display Control (Layer 6)
- `--messages [N|all]` - Preview message count
- `--branches` - Show branch information
- `--stats` - Show statistics

These options control display but do not provide context expansion around matches.

## If Context Expansion Were Implemented

If Layer 5 were added to the list command, it might work like:
- `--preview-context N` - Show N messages before/after conversations containing recent activity
- `--related-messages N` - Show N related messages when listing branches

However, this is speculative - the current implementation does not include these features.

## Comparison with Other Commands

| Command | Layer 5 Support | Implementation |
|---------|----------------|----------------|
| **list** | ❌ No | N/A |
| **show** | ❌ No | Shows entire messages |
| **search** | ✅ Yes | `--context N` for line-level context |
| **branches** | ❌ No | N/A |
| **stats** | ❌ No | Aggregate data only |
| **cleanup** | ❌ No | File operations only |

## Navigation

- [← Layer 4: Content Removal](cycodj-list-filtering-pipeline-catalog-layer-4.md)
- [→ Layer 6: Display Control](cycodj-list-filtering-pipeline-catalog-layer-6.md)
- [↑ list Command Overview](cycodj-list-filtering-pipeline-catalog-README.md)
- [↑ cycodj Main Catalog](cycodj-filtering-pipeline-catalog-README.md)

## See Also

- [search Layer 5](cycodj-search-filtering-pipeline-catalog-layer-5.md) - The only cycodj command that implements context expansion
