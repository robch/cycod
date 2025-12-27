# cycodj show Command - Layer 3: Content Filtering

## Overview

**Layer 3: Content Filtering** for the `show` command is **NOT IMPLEMENTED**. The show command displays the full content of a single conversation without any content-based filtering.

## Purpose

The show command is designed to display a **complete conversation** with all messages. It does not filter message content - it shows everything.

## Command-Line Options

### No Layer 3 Options

The `show` command has **zero Layer 3 content filtering options**:
- ❌ NO `--contains` or pattern matching
- ❌ NO `--user-only` or `--assistant-only` (no role filtering)
- ❌ NO `--regex` or `--case-sensitive`
- ❌ NO line-level filtering

### Related Options (Not Layer 3)

The show command has display control options (Layer 6):
- `--show-tool-calls`: Display tool call details (Layer 6)
- `--show-tool-output`: Display full tool output (Layer 6)
- `--max-content-length`: Truncate content length (Layer 6)

**None of these are Layer 3** - they control display, not filtering.

## Implementation

### Message Display Loop

**Location**: `ShowCommand.GenerateShowOutput()` lines 133-193

```csharp
// Display messages
var messageNumber = 0;
foreach (var msg in conv.Messages)
{
    messageNumber++;
    
    // Skip system messages unless verbose
    if (msg.Role == "system" && !ConsoleHelpers.IsVerbose())
    {
        sb.AppendLine($"[{messageNumber}] {msg.Role} (system prompt - use --verbose to show)");
        sb.AppendLine();
        continue;
    }
    
    // Message header
    sb.AppendLine($"[{messageNumber}] {msg.Role.ToUpper()}");
    
    // Message content
    var content = msg.Content ?? string.Empty;
    
    // Limit content length for tool outputs unless ShowToolOutput is enabled
    if (msg.Role == "tool" && !ShowToolOutput && content.Length > MaxContentLength)
    {
        var truncated = content.Substring(0, MaxContentLength);
        sb.AppendLine(truncated);
        sb.AppendLine($"... (truncated {content.Length - MaxContentLength} chars, use --show-tool-output to see all)");
    }
    else
    {
        // Limit other messages too for readability
        if (content.Length > MaxContentLength * 3)
        {
            var truncated = content.Substring(0, MaxContentLength * 3);
            sb.AppendLine(truncated);
            sb.AppendLine($"... (truncated {content.Length - MaxContentLength * 3} chars)");
        }
        else
        {
            sb.AppendLine(content);
        }
    }
    
    // ... tool call display
}
```

### System Message Hiding (Not Layer 3)

**Lines 140-145**:
```csharp
// Skip system messages unless verbose
if (msg.Role == "system" && !ConsoleHelpers.IsVerbose())
{
    sb.AppendLine($"[{messageNumber}] {msg.Role} (system prompt - use --verbose to show)");
    sb.AppendLine();
    continue;
}
```

**This is NOT Layer 3** because:
- It's controlled by global `--verbose` flag (not show-specific)
- It hides entire messages, not filtering content within them
- It's a display optimization, not content filtering

### Content Truncation (Layer 6, Not Layer 3)

**Lines 154-173**: Content truncation logic

**This is NOT Layer 3** because:
- It doesn't filter WHICH content to show
- It only limits HOW MUCH to display
- It's a display concern (Layer 6)

## Content Filter Behavior

### What IS Filtered
- **Nothing** - All message content is shown (subject to display truncation)

### What is NOT Filtered
- ❌ No pattern matching
- ❌ No role-based content filtering
- ❌ No line-level filtering
- ❌ No search capability
- ⚠️ System messages hidden by --verbose (not Layer 3)

## Comparison to search Command

| Feature | show | search |
|---------|------|--------|
| Pattern matching | ❌ | ✅ |
| Role filtering | ❌ | ✅ |
| Line-level filtering | ❌ | ✅ |
| Content search | ❌ | ✅ |
| Display all content | ✅ | ❌ |

## Why No Layer 3?

The show command's purpose is to **display everything** in a conversation. Layer 3 filtering would contradict this purpose:
- Users want to see the full conversation
- Filtering would hide important context
- Search command already provides content filtering

## Feature Gaps

### Potential Layer 3 Features (Low Priority)
1. **`--role-filter`**: Only show messages from specific role(s)
2. **`--contains-highlight`**: Highlight (not filter) matching text
3. **`--skip-role`**: Hide messages from specific role(s)
4. **`--message-range`**: Show only messages N-M

**Recommendation**: Use `search` for content filtering, `show` for full display.

## Data Flow Summary

```
Conversation ID (from Layer 1)
    ↓
Load full conversation
    ↓
Display ALL messages (no filtering)
    ↓
(Optional: Hide system messages if not --verbose)
    ↓
Format for display (Layer 6)
```

## Source Code Reference

**Primary File**:
- `src/cycodj/CommandLineCommands/ShowCommand.cs` (lines 133-193)

**Key Evidence**:
- NO pattern matching methods
- NO role filtering properties
- NO content filtering logic

**Related Proof**: [cycodj-show-layer-3-proof.md](cycodj-show-layer-3-proof.md)

## Conclusion

The `show` command has **ZERO Layer 3 implementation**. It intentionally displays all content without filtering. This is by design - the command is meant for full conversation display, not filtered search.
