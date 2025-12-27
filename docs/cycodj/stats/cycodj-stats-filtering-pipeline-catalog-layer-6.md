# cycodj stats - Layer 6: Display Control

## Overview

Layer 6 (Display Control) in the `stats` command controls **how statistics are formatted and displayed**, including:
- Tool usage statistics visibility
- Date-based activity breakdown visibility
- Statistics formatting and presentation

## CLI Options

### `--show-tools`
Enables detailed tool usage statistics section.

Shows:
- Total tool call count
- Tool usage breakdown by tool name
- Percentage of total calls per tool
- Top 20 most-used tools

**Example:**
```bash
cycodj stats --show-tools
```

### `--no-dates`
Disables the date-based activity breakdown section.

By default, stats command shows activity by date (last 10 days). This option suppresses that section.

**Example:**
```bash
cycodj stats --no-dates
```

## Implementation Summary

1. **Overall Statistics** (always shown):
   - Total conversations and messages
   - Message breakdowns by role
   - Averages per conversation
   - Longest conversation

2. **Activity by Date** (default: shown, controlled by `ShowDates`):
   - Last 10 days of activity
   - Conversations and messages per day
   - Breakdown by message role

3. **Tool Usage Statistics** (controlled by `ShowTools`):
   - Total tool calls
   - Top 20 tools by usage count
   - Percentages for each tool

## See Also

- [Layer 6 Proof Document](./cycodj-stats-filtering-pipeline-catalog-layer-6-proof.md)
- [Stats Command Overview](../cycodj-filtering-pipeline-catalog-README.md#stats-command)
