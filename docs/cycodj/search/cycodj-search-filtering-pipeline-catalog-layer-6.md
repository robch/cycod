# cycodj search - Layer 6: Display Control

## Overview

Layer 6 (Display Control) in the `search` command controls **how search results are presented** to the user, including:
- Branch visualization in results
- Statistics summary
- Match highlighting and context display
- Result formatting

## CLI Options

### `--branches`
Enables branch information display in search results.

Shows:
- Which conversations are branches
- Parent/child relationships in search results

**Example:**
```bash
cycodj search "error" --branches
```

### `--stats`
Enables statistics summary at the end of search results.

Shows:
- Total conversations searched
- Conversations with matches
- Total matches found
- Message breakdowns by role
- Average messages per conversation
- Branched conversation statistics

**Example:**
```bash
cycodj search "async" --stats
```

## Implementation Summary

The display control layer in `search` command:

1. **Branch Visualization**:
   - Controlled by `ShowBranches` boolean
   - No direct impact on search output (inherited property)
   - Available for consistency with other commands

2. **Statistics Display**:
   - Controlled by `ShowStats` boolean
   - Generated at end of search results
   - Includes search-specific metrics:
     - Total conversations searched
     - Conversations with matches
     - Total match count
   - Plus standard conversation statistics

3. **Match Display Format**:
   - Each match shows:
     - Conversation title and timestamp
     - File path
     - Match count per conversation
     - Message role for each match
     - Message number within conversation
     - Matched lines with context (controlled by Layer 5)

4. **Result Formatting**:
   - Hierarchical structure: Conversation → Message → Matched lines
   - Match indicators: `>` prefix for matched lines
   - Context lines: indented without prefix
   - Section separators between conversations

## Layer Flow

```
Input: Search matches (from Layer 3)
  ↓
For each conversation with matches:
  - Format conversation header (timestamp, title, file)
  - Show match count
  - For each matched message:
    - Show message role and number
    - Show matched lines with ">" prefix
    - Show context lines (indented)
  ↓
Show match summary
  ↓
If ShowStats enabled:
  - Generate statistics section
  - Show search metrics
  - Show conversation statistics
  ↓
Output: Formatted search results
```

## Related Layers

- **Layer 3 (Content Filtering)**: Determines which messages/lines match
- **Layer 5 (Context Expansion)**: Determines how many context lines to show
- **Layer 6 (Display Control)**: Formats the results
- **Layer 7 (Output Persistence)**: Receives formatted output for saving

## See Also

- [Layer 6 Proof Document](./cycodj-search-filtering-pipeline-catalog-layer-6-proof.md) - Source code evidence
- [Search Command Overview](../cycodj-filtering-pipeline-catalog-README.md#search-command)
