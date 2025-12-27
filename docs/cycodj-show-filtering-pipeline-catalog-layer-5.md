# cycodj show Command - Layer 5: Context Expansion

## Status: NOT IMPLEMENTED

The `show` command does **not implement Layer 5 (Context Expansion)**.

## What is Layer 5?

Layer 5 (Context Expansion) provides the ability to show surrounding lines or messages around matched content to provide context.

## Why Not Implemented in show?

The `show` command displays complete conversations in full detail. It does not perform searching or matching, so there are no "matches" around which to expand context.

### What show Does Instead

The show command displays **entire messages** without filtering:
- Shows all messages in the conversation sequentially
- Can truncate very long messages for readability (`--max-content-length`)
- Can show/hide tool calls and tool output (`--show-tool-calls`, `--show-tool-output`)

This is **NOT context expansion** because:
1. There are no search matches to expand around
2. All content is shown by default (not selectively based on matches)
3. The focus is on complete conversation display, not contextual snippets

## Example of What show Does

```bash
cycodj show chat-history-20240115-103000-123456
```

Output:
```
═══════════════════════════════════════════════════════════════════════
## Project Discussion
═══════════════════════════════════════════════════════════════════════

Timestamp: 2024-01-15 10:30:00
File: chat-history-20240115-103000-123456.jsonl
Messages: 9 total
  - 5 user, 4 assistant, 0 tool

[1] USER
How do we implement the new feature?

[2] ASSISTANT
Let me break down the implementation approach...

[3] USER
What about the database schema?

[... etc ...]
```

Shows the entire conversation, not context around specific matches.

## Related Options

### Display Control (Layer 6)
- `--show-tool-calls` - Show tool call details
- `--show-tool-output` - Show full tool output (not truncated)
- `--max-content-length N` - Truncate long messages to N characters
- `--stats` - Show conversation statistics

These options control display formatting but do not provide context expansion.

## If Context Expansion Were Implemented

If Layer 5 were added to the show command, it might work like:
- Combined with a search query: `cycodj show <id> --search "error" --context 3`
- Show only portions of messages matching criteria with context

However, this is speculative - the current implementation shows full conversations without selective context.

## Comparison with Other Commands

| Command | Layer 5 Support | Implementation |
|---------|----------------|----------------|
| **list** | ❌ No | Message previews (not context) |
| **show** | ❌ No | Shows entire messages |
| **search** | ✅ Yes | `--context N` for line-level context |
| **branches** | ❌ No | Message previews (not context) |
| **stats** | ❌ No | Aggregate data only |
| **cleanup** | ❌ No | File operations only |

## Relationship to search Command

The `search` command is complementary to `show`:
- **search**: Find conversations and show matched snippets with context
- **show**: Display a specific conversation in full detail

Typical workflow:
```bash
# 1. Search to find relevant conversations
cycodj search "database error" --context 3

# 2. Show full conversation after finding it
cycodj show chat-history-20240115-103000-123456
```

## Navigation

- [← Layer 4: Content Removal](cycodj-show-filtering-pipeline-catalog-layer-4.md)
- [→ Layer 6: Display Control](cycodj-show-filtering-pipeline-catalog-layer-6.md)
- [↑ show Command Overview](cycodj-show-filtering-pipeline-catalog-README.md)
- [↑ cycodj Main Catalog](cycodj-filtering-pipeline-catalog-README.md)

## See Also

- [search Layer 5](cycodj-search-filtering-pipeline-catalog-layer-5.md) - The only cycodj command that implements context expansion
