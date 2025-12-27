# cycodj branches Command - Layer 5: Context Expansion

## Status: NOT IMPLEMENTED

The `branches` command does **not implement Layer 5 (Context Expansion)**.

## What is Layer 5?

Layer 5 (Context Expansion) provides the ability to show surrounding lines or messages around matched content to provide context.

## Why Not Implemented in branches?

The `branches` command displays conversation branching structure as a tree. It does not perform content searching or matching, so there are no "matches" around which to expand context.

### What branches Does Instead

The branches command provides **message previews** (Layer 6: Display Control):
- `--messages [N|all]` - Controls how many preview messages to show per conversation node
- Default: 0 messages (no previews)
- Shows last N messages for branches (what's new)
- Shows first N messages for roots (starting point)

This is **NOT context expansion** because:
1. It's not expanding around matches
2. It's a fixed preview count per node, not relative to search results
3. It applies to tree navigation, not content discovery

## Example of What branches Shows

```bash
cycodj branches --messages 2
```

Output:
```
## Conversation Tree

📁 2024-01-15 10:30:00 - Initial Discussion
   Messages: 5 user, 4 assistant
   Tool calls: 3

  > How do we implement this?
  > What are the requirements?

  ├─ 2024-01-15 11:00:00 - Branch: Database Approach
     Messages: 3 user, 2 assistant
     Tool calls: 1
     
     > Let's use PostgreSQL
     > What about migrations?

  ├─ 2024-01-15 11:30:00 - Branch: NoSQL Approach
     Messages: 2 user, 2 assistant
     Tool calls: 0
     
     > MongoDB might be better
```

This shows message previews for each node in the tree, but it's not expanding around specific matched content.

## Related Options

### Display Control (Layer 6)
- `--messages [N|all]` - Preview message count per node
- `--verbose`, `-v` - Show detailed branch information
- `--stats` - Show tree statistics

These options control tree display but do not provide context expansion around matches.

## Related to Container Filtering (Layer 2)

The branches command DOES implement Layer 2:
- `--conversation <id>`, `-c <id>` - Show branches for specific conversation

This filters which conversation tree to display, but still doesn't provide content-based context expansion.

## If Context Expansion Were Implemented

If Layer 5 were added to the branches command, it might work like:
- `--preview-context N` - Show N messages before/after branch points
- `--divergence-context N` - Show context around where branches diverge from parent
- Combined with search: `--search "topic" --context 2` - Show matching nodes with context

However, this is speculative - the current implementation does not include these features.

## Comparison with Other Commands

| Command | Layer 5 Support | Implementation |
|---------|----------------|----------------|
| **list** | ❌ No | Message previews (not context) |
| **show** | ❌ No | Shows entire messages |
| **search** | ✅ Yes | `--context N` for line-level context |
| **branches** | ❌ No | Message previews (not context) |
| **stats** | ❌ No | Aggregate data only |
| **cleanup** | ❌ No | File operations only |

## Use Case Distinction

The branches command focuses on **structural navigation**:
- Understanding conversation lineage
- Identifying branch points
- Comparing different conversation paths

This is different from the search command's **content discovery** focus:
- Finding specific text or patterns
- Showing matched content with context
- Analyzing conversation content

## Navigation

- [← Layer 4: Content Removal](cycodj-branches-filtering-pipeline-catalog-layer-4.md)
- [→ Layer 6: Display Control](cycodj-branches-filtering-pipeline-catalog-layer-6.md)
- [↑ branches Command Overview](cycodj-branches-filtering-pipeline-catalog-README.md)
- [↑ cycodj Main Catalog](cycodj-filtering-pipeline-catalog-README.md)

## See Also

- [search Layer 5](cycodj-search-filtering-pipeline-catalog-layer-5.md) - The only cycodj command that implements context expansion
- [branches Layer 2](cycodj-branches-filtering-pipeline-catalog-layer-2.md) - Conversation filtering (container level)
