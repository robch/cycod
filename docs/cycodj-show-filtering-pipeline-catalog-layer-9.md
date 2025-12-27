# cycodj show Command - Layer 9: Actions on Results

[← Back to show README](cycodj-show-filtering-pipeline-catalog-README.md)

## Overview

**Layer 9: Actions on Results** - Perform operations on search results.

For the `show` command, this layer is **NOT IMPLEMENTED**. The show command is a read-only display command that shows a single conversation in detail but does not perform any actions on it.

## Implementation Status

❌ **NOT IMPLEMENTED** - The show command does not modify, delete, or transform conversation files.

---

## Purpose of show Command

The `show` command is designed to:
- **Display** a single conversation in full detail
- **Show** all messages with optional truncation
- **Display** tool calls and outputs (optionally)
- **Show** statistics (optionally)

It is intentionally a **read-only** command focused on Layer 6 (Display Control) and Layer 7 (Output Persistence).

---

## What Actions Are NOT Present

### No File Modification
- Does not edit conversation files
- Does not change conversation metadata
- Does not update messages or tool calls

### No File Deletion
- Does not remove the displayed conversation
- Does not delete messages within the conversation

### No Data Transformation
- Does not export to other formats
- Does not merge with other conversations
- Does not extract or archive portions

---

## Related Commands with Actions

For operations ON conversations, see:
- **[cleanup](cycodj-cleanup-filtering-pipeline-catalog-layer-9.md)** - Delete duplicate/empty/old conversations

---

## Command Line Options

The show command has **NO** Layer 9 options. All options relate to other layers:

| Layer | Options |
|-------|---------|
| 1 - Target Selection | `<conversation-id>` (positional) |
| 6 - Display Control | `--show-tool-calls`, `--show-tool-output`, `--max-content-length`, `--stats` |
| 7 - Output Persistence | `--save-output` |
| 8 - AI Processing | `--instructions`, `--use-built-in-functions`, `--save-chat-history` |

---

## Execution Flow

```
Parse Options (extract conversation ID)
  ↓
Find Conversation File by ID (Layer 1)
  ↓
Read & Parse Conversation
  ↓
Generate Detailed Display Output (Layer 6)
  ↓
Apply AI Instructions (Layer 8) - optional
  ↓
Save to File (Layer 7) OR Display to Console
  ↓
END (no actions performed)
```

---

## Design Rationale

The show command follows the **principle of least surprise**:
- Users expect `show` to be safe and non-destructive
- Viewing a conversation should never modify it
- Detailed display for inspection, not modification

---

## See Also

- [Layer 6: Display Control](cycodj-show-filtering-pipeline-catalog-layer-6.md) - How show presents the conversation
- [Layer 7: Output Persistence](cycodj-show-filtering-pipeline-catalog-layer-7.md) - How show saves output
- [Layer 8: AI Processing](cycodj-show-filtering-pipeline-catalog-layer-8.md) - How show applies AI instructions

---

## Proof

See [Layer 9 Proof](cycodj-show-filtering-pipeline-catalog-layer-9-proof.md) for source code evidence confirming no actions are performed.
