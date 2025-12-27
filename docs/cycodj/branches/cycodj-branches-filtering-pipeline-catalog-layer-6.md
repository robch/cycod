# cycodj branches - Layer 6: Display Control

## Overview

Layer 6 (Display Control) in the `branches` command controls **how conversation trees are visualized**, including:
- Tree visualization with indentation
- Message preview display
- Verbose metadata
- Statistics display

## CLI Options

### `--verbose` / `-v`
Enables verbose display of branch information.

Shows:
- Message counts per conversation
- Tool call counts
- Branch point information (where conversations diverged)

**Example:**
```bash
cycodj branches --verbose
```

### `--messages [N|all]`
Controls how many user messages to preview for each conversation in the tree.

- **No value**: Uses command default (0 messages for branches)
- **N** (number): Shows N messages per conversation
- **all**: Shows all messages

**Example:**
```bash
cycodj branches --messages 3
```

### `--stats`
Enables detailed statistics at the end of the tree display.

Shows:
- Total conversations and root count
- Branched conversation statistics
- Message counts and averages
- Average branch depth

**Example:**
```bash
cycodj branches --stats
```

## Implementation Summary

1. **Tree Visualization**:
   - Root conversations: `📁` icon
   - Branch conversations: `├─` symbol
   - Indentation: 2 spaces per depth level
   - Timestamp and title display

2. **Verbose Information**:
   - Controlled by `Verbose` boolean
   - Shows message counts (user, assistant)
   - Shows tool call counts
   - Shows branch point information

3. **Message Preview**:
   - Controlled by `MessageCount` (default: 0 for branches command)
   - For branches: Shows last N messages (what's new)
   - For roots: Shows first N messages

4. **Statistics Display**:
   - Controlled by `ShowStats` boolean
   - Tree-specific metrics: root count, branch count, average depth
   - Standard conversation statistics

## See Also

- [Layer 6 Proof Document](./cycodj-branches-filtering-pipeline-catalog-layer-6-proof.md)
- [Branches Command Overview](../cycodj-filtering-pipeline-catalog-README.md#branches-command)
