# cycodj list Command: Layer 2 - Container Filtering

## Overview

Layer 2 focuses on **which conversations (containers) to include or exclude** from the results. For the `list` command, this layer determines which conversations pass through after the initial file selection in Layer 1.

## Implementation Summary

The `list` command implements **implicit container filtering** through:
1. Default limiting to prevent overwhelming output
2. The ability to override defaults with explicit options

### Note on Container Filtering

The `list` command does NOT have explicit container-level filtering options like:
- `--conversation-contains <pattern>` (to filter by conversation title/content)
- `--min-messages <N>` (to filter by message count)
- `--has-branches` (to show only branched conversations)

Instead, conversations that pass Layer 1 (target selection) are ALL included, subject only to result limiting.

## CLI Options

### None (Implicit)

The `list` command does not expose CLI options specifically for Layer 2 container filtering.

**Evidence**: The command-line parser (`CycoDjCommandLineOptions.cs`) has no options that filter individual conversations based on their properties or content.

## Implementation Details

### Default Conversation Limit

**Source**: `src/cycodj/CommandLineCommands/ListCommand.cs`, lines 97-104

When no filters are specified, the command applies a sensible default:

```csharp
// Apply sensible default limit if not specified and no filters
var effectiveLimit = Last;
if (effectiveLimit == 0 && !After.HasValue && !Before.HasValue && string.IsNullOrEmpty(Date))
{
    effectiveLimit = 20; // Default to last 20 conversations
    sb.AppendLine($"Showing last {effectiveLimit} conversations (use --last N to change, or --date to filter)");
    sb.AppendLine();
}
```

**Behavior**:
- If NO Layer 1 filters are active (no `--after`, `--before`, `--date`) AND no `--last` is specified
- Apply default limit of 20 conversations
- Inform user about the default and how to change it

### Explicit Result Limiting

**Source**: `src/cycodj/CommandLineCommands/ListCommand.cs`, lines 106-115

If `--last N` was specified (and parsed as a conversation count):

```csharp
// Limit to last N if specified or defaulted
if (effectiveLimit > 0 && files.Count > effectiveLimit)
{
    files = files.Take(effectiveLimit).ToList();
    if (Last > 0)
    {
        sb.AppendLine($"Showing last {effectiveLimit} conversations");
        sb.AppendLine();
    }
}
```

**Behavior**:
- Take first N files from the list (which is already sorted by timestamp, most recent first)
- Display message if user explicitly requested `--last N`

## Data Flow

```
Layer 1 Output (List of Files)
    ↓
Check for filters/limits
    ↓
Apply default limit (20) if no filters specified
    ↓
OR apply explicit --last N limit
    ↓
files = limited subset
    ↓
Layer 3 (Read conversations from files)
```

## Design Note: Why No Explicit Container Filtering?

The `list` command is designed as a **browsing tool** rather than a **search tool**. Its philosophy is:
- Show recent conversations by default
- Let users apply time filters (Layer 1) to narrow the window
- Then show ALL conversations in that window

For content-based filtering (e.g., "conversations containing X"), users should use the `search` command instead.

### Potential Enhancement

If container-level filtering were added, it might look like:
- `--title-contains <pattern>`: Filter conversations by title
- `--min-messages <N>`: Show only conversations with N+ messages
- `--has-tool-calls`: Show only conversations with tool calls
- `--no-branches`: Exclude branch conversations

**Note**: These options do NOT currently exist.

## Relationship to Other Layers

- **Layer 1** (Target Selection): Provides the initial set of conversation files
- **Layer 3** (Content Filtering): Filters messages WITHIN the conversations that pass Layer 2
- **Layer 5** (Context Expansion): Controls how many messages to preview per conversation

## Proof

See [cycodj-list-layer-2-proof.md](cycodj-list-layer-2-proof.md) for detailed source code evidence of all assertions made in this document.

## Summary

| Aspect | Status |
|--------|--------|
| **Explicit Options** | ❌ None |
| **Implicit Behavior** | ✅ Default limit of 20, overridable with `--last N` |
| **Content-Based Filtering** | ❌ Not available (use `search` command) |
| **Property-Based Filtering** | ❌ Not available |
| **Result Limiting** | ✅ Via `--last N` (inherited from Layer 1 parsing) |

---

[← Layer 1: Target Selection](cycodj-list-layer-1.md) | [Layer 3: Content Filtering →](cycodj-list-layer-3.md)

[↑ Back to list Command Overview](cycodj-filtering-pipeline-catalog-README.md#list-command)
