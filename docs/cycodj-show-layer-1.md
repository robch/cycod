# cycodj show - Layer 1: TARGET SELECTION

## Overview

The `show` command displays a single conversation in full detail. Layer 1 (TARGET SELECTION) for this command is **SIMPLE** - it only requires a conversation ID.

## Implementation Summary

The `show` command implements **SIMPLE** target selection with a single mechanism:

1. **Conversation ID** (required positional argument)

## Target Selection Options

### Required: Conversation ID

- **Positional argument** (first non-option argument)
- The unique identifier or partial match for the conversation to display
- Must be provided (command fails if missing)
- Matching logic:
  - Exact filename match (without extension)
  - Substring match in filename

### No Time Filtering

The `show` command does NOT support time filtering options:
- ❌ No `--today`, `--yesterday`
- ❌ No `--after`, `--before`, `--date-range`, `--time-range`
- ❌ No `--date`, `-d`
- ❌ No `--last`

**Rationale**: When you already know the conversation ID, time filtering is unnecessary.

### No Count Limiting

The `show` command shows exactly ONE conversation:
- ❌ No `--last N` option
- ❌ No default limiting

**Rationale**: The command is designed to show a single, specific conversation.

## Processing Pipeline

### Step 1: Validate Conversation ID
```
IF ConversationId is null or empty
  ERROR: "Conversation ID is required"
  Show usage: "Usage: cycodj show <conversation-id>"
```

### Step 2: Find All History Files
```
files = HistoryFileHelpers.FindAllHistoryFiles()
```

### Step 3: Find Matching File
```
matchingFile = files.FirstOrDefault(f => 
    f.Contains(ConversationId) OR
    Path.GetFileNameWithoutExtension(f) == ConversationId)
```

### Step 4: Validate Match Found
```
IF matchingFile is null
  ERROR: "Conversation not found: {ConversationId}"
  Show: "Searched {files.Count} chat history files"
```

### Step 5: Read and Display Conversation
```
conversation = JsonlReader.ReadConversation(matchingFile)
Display conversation with all messages
```

## Matching Logic

### Exact Match (Priority 1)
```
Path.GetFileNameWithoutExtension(file) == ConversationId
```
Example:
- ConversationId: `conversation-20240115-103045-a1b2c3d4`
- Matches: `conversation-20240115-103045-a1b2c3d4.jsonl`

### Substring Match (Priority 2)
```
file.Contains(ConversationId)
```
Example:
- ConversationId: `20240115-103045`
- Matches: `conversation-20240115-103045-a1b2c3d4.jsonl`

### First Match Wins
If multiple files match, the first match is used:
```csharp
var matchingFile = files.FirstOrDefault(f => ...);
```

## Priority/Precedence

1. **Conversation ID validation** - Must be provided (fail early)
2. **Find all files** - Get complete file list
3. **Match by exact filename** - Check exact match first
4. **Match by substring** - Fallback to substring search
5. **First match wins** - No disambiguation if multiple matches

## Examples

### Example 1: Full Conversation ID
```bash
cycodj show conversation-20240115-103045-a1b2c3d4
# Matches: conversation-20240115-103045-a1b2c3d4.jsonl
# Shows: Complete conversation with all messages
```

### Example 2: Partial Timestamp
```bash
cycodj show 20240115-103045
# Matches: conversation-20240115-103045-a1b2c3d4.jsonl
# Shows: First conversation matching this timestamp
```

### Example 3: Short ID
```bash
cycodj show a1b2c3d4
# Matches: conversation-20240115-103045-a1b2c3d4.jsonl
# Shows: First conversation with this ID suffix
```

### Example 4: Missing Conversation
```bash
cycodj show nonexistent-id
# Output:
# ERROR: Conversation not found: nonexistent-id
# Searched 150 chat history files
```

### Example 5: No ID Provided
```bash
cycodj show
# Output:
# ERROR: Conversation ID is required
# Usage: cycodj show <conversation-id>
```

## Differences from Other Commands

| Feature | list/search/branches/stats | show |
|---------|----------------------------|------|
| **Time filtering** | ✅ Rich (--today, --after, etc.) | ❌ None |
| **Count limiting** | ✅ --last N / default | ❌ Shows exactly 1 |
| **Required arg** | ❌ Optional | ✅ Conversation ID required |
| **Target count** | Multiple conversations | Single conversation |
| **Selection method** | Time-based + count | ID-based |
| **Default behavior** | Show recent N | Error if no ID |

## Performance Implications

### File Search
- **Impact**: Must scan all files to find match
- **Time**: Linear scan O(n) where n = total conversation files
- **Note**: No early exit on exact match (uses FirstOrDefault on filtered list)

### Optimization Opportunities
1. Could exit early on exact filename match
2. Could use indexed lookup by ID
3. Could cache filename-to-path mapping

### Current Performance
- For typical histories (100-1000 files): Fast (milliseconds)
- For large histories (10,000+ files): Slower but acceptable

## Edge Cases

### Multiple Matches
If multiple files match the same substring:
- **Behavior**: First match is used (arbitrary)
- **Example**: `show 2024` might match multiple files from 2024
- **Recommendation**: Use more specific ID

### Case Sensitivity
- **Platform dependent**: Depends on file system
- **Windows**: Case-insensitive match
- **Linux/macOS**: Case-sensitive match

### Special Characters
- ConversationId can contain any characters
- Matching uses simple string contains/equals
- No regex, no escaping needed

## Related Files

- **Implementation**: [cycodj-show-layer-1-proof.md](cycodj-show-layer-1-proof.md)
- **Parser**: [cycodj-show-layer-1-proof.md#parser-evidence](cycodj-show-layer-1-proof.md#parser-evidence)
- **Execution**: [cycodj-show-layer-1-proof.md#execution-evidence](cycodj-show-layer-1-proof.md#execution-evidence)

## See Also

- [Layer 3: Content Filtering](cycodj-show-layer-3.md) - Role-based message filtering
- [Layer 6: Display Control](cycodj-show-layer-6.md) - How conversation is displayed
- [HistoryFileHelpers](cycodj-show-layer-1-proof.md#historyfilehelpers) - File finding utilities
- [list command](cycodj-list-layer-1.md) - For browsing multiple conversations

---

**Next Layer**: [Layer 3: Content Filtering](cycodj-show-layer-3.md) (Layer 2 not implemented)
