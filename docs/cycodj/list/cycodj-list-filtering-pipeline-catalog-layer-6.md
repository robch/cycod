# cycodj list - Layer 6: Display Control

## Overview

Layer 6 (Display Control) in the `list` command controls **how conversations are presented** to the user, including:
- Message preview counts
- Statistics display
- Branch visualization
- Formatting and indentation

## CLI Options

### `--messages [N|all]`
Controls how many user messages to preview for each conversation.

- **No value**: Uses command default (3 messages)
- **N** (number): Shows N messages per conversation
- **all**: Shows all messages (set to `int.MaxValue`)

**Example:**
```bash
cycodj list --messages 5      # Show 5 messages per conversation
cycodj list --messages all    # Show all messages
cycodj list --messages         # Show default (3)
```

### `--stats`
Enables statistics summary at the end of the output.

Shows:
- Total conversations
- Total messages (broken down by role: user, assistant, tool)
- Average messages per conversation
- Branched conversations count and percentage

**Example:**
```bash
cycodj list --stats
```

### `--branches`
Enables branch information display for each conversation.

Shows:
- Parent ID (if conversation is a branch)
- Number of child branches
- Number of tool calls

**Example:**
```bash
cycodj list --branches
```

## Implementation Summary

The display control layer in `list` command:

1. **Message Preview Control**:
   - Reads `MessageCount` property (nullable int)
   - If null, uses default of 3
   - Shows different messages for branches vs. roots:
     - **Branches**: Last N messages (what's new)
     - **Roots**: First N messages
   - Truncates message preview to 200 characters
   - Shows "... and X more" if there are additional messages

2. **Statistics Display**:
   - Controlled by `ShowStats` boolean
   - Generates comprehensive statistics section
   - Includes message breakdowns and percentages
   - Shows branch statistics

3. **Branch Visualization**:
   - Controlled by `ShowBranches` boolean
   - Adds indentation for branched conversations (`  ↳ `)
   - Shows parent/child relationships
   - Displays tool call counts

4. **Formatting**:
   - Timestamp formatting using `TimestampHelpers`
   - Message role counts
   - Hierarchical display with indentation
   - Section separators for statistics

## Layer Flow

```
Input: Conversations (filtered from previous layers)
  ↓
Apply MessageCount to determine preview length
  ↓
For each conversation:
  - Format timestamp
  - Show indent if branch (ShowBranches check)
  - Display title or ID
  - Show message counts
  - Show branch info (if ShowBranches enabled)
  - Show message previews (based on MessageCount)
  ↓
Show totals
  ↓
Show statistics section (if ShowStats enabled)
  ↓
Output: Formatted display text
```

## Related Layers

- **Layer 1 (Target Selection)**: Determines which conversations to display
- **Layer 2 (Container Filtering)**: Filters conversations before display
- **Layer 3 (Content Filtering)**: N/A for list command
- **Layer 7 (Output Persistence)**: Receives formatted output for saving

## See Also

- [Layer 6 Proof Document](./cycodj-list-filtering-pipeline-catalog-layer-6-proof.md) - Source code evidence
- [List Command Overview](../cycodj-filtering-pipeline-catalog-README.md#list-command)
