# cycodj stats Command - Layer 5: Context Expansion

## Status: NOT IMPLEMENTED

The `stats` command does **not implement Layer 5 (Context Expansion)**.

## What is Layer 5?

Layer 5 (Context Expansion) provides the ability to show surrounding lines or messages around matched content to provide context.

## Why Not Implemented in stats?

The `stats` command generates **aggregate statistics** about conversations. It does not display individual message content or perform searching, so there are no "matches" around which to expand context.

### What stats Does Instead

The stats command provides **statistical summaries**:
- Total conversation counts
- Message counts by role (user, assistant, tool)
- Activity by date
- Tool usage statistics (with `--show-tools`)
- Average metrics

This is **NOT context expansion** because:
1. No individual messages are shown
2. No content matching or searching occurs
3. Output is purely numerical/tabular data

## Example of What stats Shows

```bash
cycodj stats --today
```

Output:
```
## Chat History Statistics

### Overall Statistics

**Conversations:** 5
**Total Messages:** 47
  - User: 23 (48.9%)
  - Assistant: 21 (44.7%)
  - Tool: 3 (6.4%)

**Average per conversation:**
  - Messages: 9.4
  - User messages: 4.6

**Longest conversation:** 15 messages
  2024-01-15 10:30:00 - Project Discussion

### Activity by Date

Date         Convs   Msgs  User  Asst  Tool
2024-01-15      5     47    23    21     3
```

Pure statistics - no message content or context shown.

## Related Options

### Display Control (Layer 6)
- `--show-tools` - Include tool usage statistics
- `--no-dates` - Hide date-based statistics
- `--stats` - N/A (command itself is stats)

These options control which statistics are shown but do not provide context expansion.

## If Context Expansion Were Implemented

If Layer 5 were added to the stats command, it might work like:
- `--show-examples` - Show example messages for statistical categories
- `--sample-messages N` - Show N sample messages from conversations
- `--context-samples` - Show representative message snippets with context

However, this would fundamentally change the command's purpose from pure statistics to statistical analysis with examples, which is not the current design.

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

The stats command focuses on **quantitative analysis**:
- Conversation volume over time
- Message distribution by role
- Tool usage patterns
- Activity trends

This is fundamentally different from **content-focused commands**:
- search: Find and show specific content with context
- show: Display complete conversation content
- list: Browse conversations with previews

## Complementary Workflows

The stats command is often used as a preliminary analysis step:

```bash
# 1. Get overview statistics
cycodj stats --last 7d

# 2. Identify interesting patterns

# 3. Drill down with search
cycodj search "specific topic" --last 7d --context 3

# 4. View full conversations
cycodj show <conversation-id>
```

## Navigation

- [← Layer 4: Content Removal](cycodj-stats-filtering-pipeline-catalog-layer-4.md)
- [→ Layer 6: Display Control](cycodj-stats-filtering-pipeline-catalog-layer-6.md)
- [↑ stats Command Overview](cycodj-stats-filtering-pipeline-catalog-README.md)
- [↑ cycodj Main Catalog](cycodj-filtering-pipeline-catalog-README.md)

## See Also

- [search Layer 5](cycodj-search-filtering-pipeline-catalog-layer-5.md) - The only cycodj command that implements context expansion
