# cycodj stats Command - Layer 9: Actions on Results

[← Back to stats README](cycodj-stats-filtering-pipeline-catalog-README.md)

## Overview

**Layer 9: Actions on Results** - Perform operations on search results.

For the `stats` command, this layer is **NOT IMPLEMENTED**. The stats command is a read-only analysis command that computes and displays statistical aggregations but does not perform any actions on conversations.

## Implementation Status

❌ **NOT IMPLEMENTED** - The stats command does not modify, delete, or transform conversation files.

---

## Purpose of stats Command

The `stats` command is designed to:
- **Calculate** aggregate statistics across conversations
- **Display** message counts, averages, distributions
- **Show** activity by date (optionally)
- **Present** tool usage statistics (optionally)

It is intentionally a **read-only** command focused on Layer 6 (Display Control) for statistical analysis.

---

## What Actions ARE NOT Present

### No File Modification
- Does not edit conversations based on statistics
- Does not update metadata with computed values
- Does not normalize or clean data

### No File Deletion
- Does not remove outlier conversations
- Does not delete based on statistical thresholds
- Does not archive inactive conversations

### No Data Transformation
- Does not export statistics to databases
- Does not create summary reports (beyond display output)
- Does not generate charts or graphs

---

## Related Commands with Actions

For operations ON conversations, see:
- **[cleanup](cycodj-cleanup-filtering-pipeline-catalog-layer-9.md)** - Delete duplicate/empty/old conversations

---

## Command Line Options

The stats command has **NO** Layer 9 options. All options relate to other layers:

| Layer | Options |
|-------|---------|
| 1 - Target Selection | `--date`, `--last`, `--today`, `--yesterday`, `--after`, `--before`, `--date-range`, `--time-range` |
| 6 - Display Control | `--show-tools`, `--no-dates` |
| 7 - Output Persistence | `--save-output` |
| 8 - AI Processing | `--instructions`, `--use-built-in-functions`, `--save-chat-history` |

---

## Execution Flow

```
Parse Options
  ↓
Find Conversation Files (Layer 1 - time filtering)
  ↓
Read & Parse All Conversations
  ↓
Calculate Statistics (in-memory aggregations)
  ├→ Overall Stats (message counts, averages)
  ├→ Date Stats (activity by date) - if enabled
  └→ Tool Usage Stats - if enabled
  ↓
Generate Display Output (Layer 6)
  ↓
Apply AI Instructions (Layer 8) - optional
  ↓
Save to File (Layer 7) OR Display to Console
  ↓
END (no actions performed)
```

---

## Design Rationale

The stats command follows the **principle of least surprise**:
- Users expect `stats` to be safe and non-destructive
- Computing statistics should never modify source data
- Statistical analysis is for **insights**, not **modification**

---

## See Also

- [Layer 6: Display Control](cycodj-stats-filtering-pipeline-catalog-layer-6.md) - How stats presents aggregations
- [Layer 7: Output Persistence](cycodj-stats-filtering-pipeline-catalog-layer-7.md) - How stats saves output
- [Layer 8: AI Processing](cycodj-stats-filtering-pipeline-catalog-layer-8.md) - How stats applies AI instructions

---

## Proof

See [Layer 9 Proof](cycodj-stats-filtering-pipeline-catalog-layer-9-proof.md) for source code evidence confirming no actions are performed.
