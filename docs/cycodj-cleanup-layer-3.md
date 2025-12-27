# cycodj cleanup Command - Layer 3: Content Filtering

## Overview

**Layer 3: Content Filtering** for the `cleanup` command is **NOT IMPLEMENTED**. The cleanup command identifies conversations to clean up based on metadata, not message content.

## Purpose

The cleanup command finds conversations to remove based on:
- Duplicate detection (signature-based)
- Empty conversation detection (message count)
- Age filtering (timestamp-based)

None of these use content-based filtering (Layer 3).

## Command-Line Options

### No Layer 3 Options

- ❌ NO `--contains` or pattern matching
- ❌ NO `--user-only` or `--assistant-only`
- ❌ NO `--regex` or `--case-sensitive`
- ❌ NO message content filtering

### Related Options (Not Layer 3)

- `--find-duplicates`: Find duplicates (Layer 2 - based on signature, not content)
- `--remove-duplicates`: Remove duplicates (Layer 9 - Action)
- `--find-empty`: Find empty conversations (Layer 2 - message count)
- `--remove-empty`: Remove empty (Layer 9 - Action)
- `--older-than-days`: Age-based filtering (Layer 1 - timestamp)
- `--execute`: Execute cleanup (Layer 9 - Action)

## Implementation

### Duplicate Detection (Layer 2, Not Layer 3)

**Location**: `CleanupCommand.FindDuplicateConversationsAsync()` lines 121-186

```csharp
// Create a signature based on message content
var signature = string.Join("|", conversation.Messages
    .Where(m => m.Role == "user" || m.Role == "assistant")
    .Take(10) // First 10 messages
    .Select(m => $"{m.Role}:{m.Content?.Length ?? 0}"));
```

**This is NOT Layer 3** because:
- Uses message **length**, not content
- Creates a **signature** for duplicate detection
- Does NOT filter by pattern or search text
- This is **Layer 2 (Container Filtering)** - identifies which conversations are duplicates

### Empty Detection (Layer 2, Not Layer 3)

**Location**: `CleanupCommand.FindEmptyConversations()` lines 188-228

```csharp
var meaningfulMessages = conversation.Messages.Count(m => 
    m.Role == "user" || m.Role == "assistant");

if (meaningfulMessages == 0)
{
    empty.Add(file);
}
```

**This is NOT Layer 3** because:
- Counts messages, doesn't search content
- No pattern matching
- This is **Layer 2 (Container Filtering)** - identifies empty conversations

### Age-Based Filtering (Layer 1, Not Layer 3)

**Location**: `CleanupCommand.FindOldConversations()` lines 230-267

```csharp
var cutoffDate = DateTime.Now.AddDays(-olderThanDays);
var old = new List<string>();

foreach (var file in files)
{
    var timestamp = CycoDj.Helpers.TimestampHelpers.ParseTimestamp(file);
    if (timestamp < cutoffDate)
    {
        old.Add(file);
    }
}
```

**This is NOT Layer 3** because:
- Based on **timestamp**, not content
- This is **Layer 1 (Target Selection)** - filters by time

## Content Filter Behavior

### What IS NOT Filtered
- ❌ No pattern matching
- ❌ No message content search
- ❌ No role-based content filtering
- ❌ No text/regex filtering

### What IS Used (Not Layer 3)
- ✅ Message signatures (Layer 2)
- ✅ Message counts (Layer 2)
- ✅ Timestamps (Layer 1)

## Why No Layer 3?

The cleanup command's criteria are **structural**, not content-based:
- Duplicates: Based on signature (not content search)
- Empty: Based on message count (not content presence)
- Old: Based on timestamp (not content age)

Content filtering would be:
- `--contains "error"` - find conversations with error messages
- `--no-tool-calls` - find conversations without tool usage
- `--short-messages-only` - find conversations with brief messages

**None of these are implemented.**

## Source Code Reference

**Primary File**: `src/cycodj/CommandLineCommands/CleanupCommand.cs`

**Evidence**: NO content-based filtering - all criteria are metadata-based.

**Related Proof**: [cycodj-cleanup-layer-3-proof.md](cycodj-cleanup-layer-3-proof.md)

## Conclusion

The `cleanup` command has **ZERO Layer 3 implementation**. It uses:
- **Layer 1**: Timestamp-based filtering
- **Layer 2**: Signature and count-based identification
- **Layer 9**: File removal actions

No message content filtering is performed.
