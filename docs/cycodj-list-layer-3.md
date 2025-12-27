# cycodj list Command - Layer 3: Content Filtering

## Overview

**Layer 3: Content Filtering** determines what content WITHIN selected conversation containers should be shown or highlighted. For the `list` command, Layer 3 implementation is **minimal** - it focuses on which messages to preview rather than filtering message content by pattern.

## Purpose

The list command's Layer 3 focuses on:
1. **Message Selection for Preview**: Choosing which messages to show in conversation previews
2. **Role-Based Selection**: Showing only user messages in previews (not a search filter)
3. **Content Truncation**: Limiting preview length (Layer 6 overlap)

## Command-Line Options

### No Direct Layer 3 Options

The `list` command has **no explicit Layer 3 content filtering options**. There is no:
- ❌ `--contains` or `--line-contains` (no pattern matching)
- ❌ `--user-only` or `--assistant-only` (no role filtering for display)
- ❌ `--regex` or `--case-sensitive` (no search capability)

### Related Options (Not True Layer 3)

**`--messages [N|all]`**: Controls how many message previews to show
- This is **Layer 6 (Display Control)**, not Layer 3 (Content Filtering)
- Parsed in: `CycoDjCommandLineOptions.TryParseDisplayOptions()` lines 26-49
- Property: `ListCommand.MessageCount` (line 15)
- Default: 3 messages

## Implementation Flow

### 1. Message Selection for Preview

**Location**: `ListCommand.GenerateListOutput()` lines 172-200

```csharp
// Show preview - configurable number of messages
var messageCount = MessageCount ?? 3; // Default to 3 messages
var userMessages = conv.Messages.Where(m => m.Role == "user" && !string.IsNullOrWhiteSpace(m.Content)).ToList();

if (userMessages.Any() && messageCount > 0)
{
    // For branches, show last N messages (what's new)
    // For non-branches, show first N messages
    var messagesToShow = conv.ParentId != null 
        ? userMessages.TakeLast(Math.Min(messageCount, userMessages.Count))
        : userMessages.Take(Math.Min(messageCount, userMessages.Count));
    
    foreach (var msg in messagesToShow)
    {
        var preview = msg.Content.Length > 200 
            ? msg.Content.Substring(0, 200) + "..." 
            : msg.Content;
        preview = preview.Replace("\n", " ").Replace("\r", "");
        
        sb.AppendLine($"{indent}  > {preview}");
    }
    
    // Show indicator if there are more messages
    var shownCount = messagesToShow.Count();
    if (userMessages.Count > shownCount)
    {
        sb.AppendLine($"{indent}    ... and {userMessages.Count - shownCount} more");
    }
}
```

**Behavior**:
- **Filters to user messages only** (role filter, but not user-configurable)
- **Filters out empty messages** (whitespace content)
- **Branch-aware selection**: 
  - Branches: Show last N messages (most recent)
  - Non-branches: Show first N messages (conversation start)

## Content Filter Behavior

### Implicit Filtering
The list command has **implicit, non-configurable** content filtering:

1. **Role Filter**: Hard-coded to show only `role="user"` messages
2. **Empty Content Filter**: Skips messages with null/whitespace content
3. **Preview Truncation**: Truncates long messages to 200 characters

### No User-Configurable Filtering
Unlike the `search` command, `list` provides **no way** for users to:
- Filter by pattern/text
- Choose which roles to preview
- Exclude certain content types
- Search within previews

## Comparison to search Command Layer 3

| Feature | list | search |
|---------|------|--------|
| Pattern matching | ❌ | ✅ Text/Regex |
| Role filtering (user-controlled) | ❌ | ✅ --user-only, --assistant-only |
| Case sensitivity | ❌ | ✅ --case-sensitive |
| Line-level filtering | ❌ | ✅ Line-by-line search |
| Match highlighting | ❌ | ✅ Match positions tracked |
| Role filtering (implicit) | ✅ User only | ⚠️ Configurable |

## Relationship to Other Layers

### Layer 1 (Target Selection)
Layer 1 determines which conversations to list. Layer 3 doesn't further filter - it just selects preview messages.

### Layer 2 (Container Filtering)
Layer 2 can filter conversations by time. Layer 3 operates on the resulting set.

### Layer 6 (Display Control)
- **`MessageCount`** property controls how many previews to show (Layer 6)
- **Preview truncation** (200 chars) is a display concern (Layer 6)
- Layer 3's role filter enables Layer 6's preview display

## Feature Gaps

### Missing Layer 3 Features
1. **Content-based filtering**: No `--contains` to filter conversations by message content
2. **Role selection**: Cannot choose to preview assistant messages or tool outputs
3. **Pattern matching**: Cannot search/filter within previews
4. **Content type filtering**: Cannot filter by message content type
5. **Empty conversation detection**: No option to hide conversations with no user messages

### Potential Enhancements
1. **`--preview-role`**: Choose which role to preview (user, assistant, both)
2. **`--contains-preview`**: Only show conversations where preview messages contain pattern
3. **`--non-empty-only`**: Hide conversations with no meaningful messages
4. **`--preview-length`**: Configure preview truncation length
5. **`--preview-first`** / **`--preview-last`**: Control which messages to preview

## Why Layer 3 is Minimal

The `list` command is designed for **quick overview**, not detailed search. Its minimal Layer 3 implementation reflects this:

1. **Performance**: No pattern matching = faster listing
2. **Simplicity**: Fewer options = easier to use
3. **Separation of concerns**: Use `search` for content filtering
4. **Preview focus**: Showing user messages provides context for conversation identification

## Data Flow Summary

```
Conversations (from Layer 1/2)
    ↓
For each conversation:
    ↓
Filter messages: role="user" AND content NOT empty
    ↓
Select preview messages:
    - Branch: Last N messages
    - Non-branch: First N messages
    ↓
Truncate to 200 chars
    ↓
Format for display (Layer 6)
```

## Source Code Reference

**Primary File**:
- `src/cycodj/CommandLineCommands/ListCommand.cs` (lines 172-200)

**Key Methods**:
- `ListCommand.GenerateListOutput()` - Message preview selection

**Related Proof**: [cycodj-list-layer-3-proof.md](cycodj-list-layer-3-proof.md)

## Conclusion

The `list` command has **very limited Layer 3 implementation**. It:
- ✅ Filters to user messages for previews (hard-coded)
- ✅ Skips empty messages
- ❌ Provides NO user-configurable content filtering
- ❌ Has NO pattern matching capability
- ❌ Cannot filter by text/regex

For content-based filtering, users should use the `search` command instead.
