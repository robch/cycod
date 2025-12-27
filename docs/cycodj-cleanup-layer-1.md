# cycodj cleanup - Layer 1: TARGET SELECTION

## Overview

The `cleanup` command finds and optionally removes problematic conversation files. Layer 1 (TARGET SELECTION) for this command is **SPECIAL** - it selects conversations based on their TYPE (duplicates, empty, old) rather than time ranges.

## Implementation Summary

The `cleanup` command implements **SPECIAL** target selection with type-based mechanisms:

1. **By type**: Duplicates, empty conversations
2. **By age**: Conversations older than N days
3. **NO time-range filtering** (no --today, --after, --before, etc.)
4. **NO count limiting** (no --last)

## Target Selection Options

### Type-Based Selection

#### Find Duplicates
- `--find-duplicates` - Find conversations with identical content signatures
- `--remove-duplicates` - Find AND remove duplicates (implies --find-duplicates)

#### Find Empty
- `--find-empty` - Find conversations with no user/assistant messages
- `--remove-empty` - Find AND remove empty conversations (implies --find-empty)

### Age-Based Selection

- `--older-than-days <N>` - Find conversations older than N days
  - Example: `--older-than-days 90` finds conversations 90+ days old
  - Uses conversation timestamp from filename

### Action Control

- `--execute` - Actually perform removal (default is dry-run)
- Default: `DryRun = true` (safe mode - shows what would be removed)

### Required Options

**At least one** selection criterion must be provided:
- `--find-duplicates` OR
- `--find-empty` OR
- `--older-than-days`

If none provided, shows usage error.

## Processing Pipeline

### Step 1: Validate Selection Criteria
```
IF (!FindDuplicates AND !FindEmpty AND !OlderThanDays)
  ERROR: "Please specify at least one cleanup operation"
  Show available options
```

### Step 2: Find All History Files
```
files = HistoryFileHelpers.FindAllHistoryFiles()
```

### Step 3: Find Target Files by Type

#### For Duplicates
```
FOR EACH file in files
  Read conversation
  Create signature (first 10 user/assistant messages)
  Group by signature
FOR EACH group with count > 1
  Mark oldest as KEEP
  Mark rest as REMOVE
```

#### For Empty
```
FOR EACH file in files
  Read conversation
  Count user/assistant messages
  IF count == 0
    Mark as REMOVE
```

#### For Old
```
cutoffDate = Now - OlderThanDays
FOR EACH file in files
  Extract timestamp from filename
  IF timestamp < cutoffDate
    Mark as REMOVE
```

### Step 4: Display Results
```
Show files to be removed
Show total count and size
```

### Step 5: Execute Removal (if --execute)
```
IF DryRun
  Show: "DRY RUN - No files will be deleted"
  Show: "Add --execute to actually remove files"
ELSE
  Prompt for confirmation: "Type 'DELETE' to confirm"
  IF confirmed
    Delete marked files
    Show results
```

## Priority/Precedence

1. **Criteria validation** - At least one criterion required
2. **File discovery** - Find all conversation files
3. **Type-based selection** - Apply all specified criteria
4. **Deduplication** - Remove duplicates from removal list
5. **Safety check** - Default to dry-run unless --execute

## No Time Filtering

The cleanup command **DOES NOT** support time-range filtering:
- ❌ No `--today`, `--yesterday`
- ❌ No `--after`, `--before`
- ❌ No `--date-range`, `--time-range`
- ❌ No `--date`, `-d`
- ❌ No `--last`

**Rationale**: Cleanup operates on file characteristics (duplicates, empty, age) not time ranges. Use `--older-than-days` for age-based selection.

## Examples

### Example 1: Find Duplicates (Dry Run)
```bash
cycodj cleanup --find-duplicates
# Shows: Duplicate groups with files to keep/remove
# Action: None (dry-run)
```

### Example 2: Remove Duplicates
```bash
cycodj cleanup --remove-duplicates --execute
# Shows: Duplicates found
# Prompts: Type 'DELETE' to confirm
# Action: Removes duplicate files (keeps newest in each group)
```

### Example 3: Find Empty Conversations
```bash
cycodj cleanup --find-empty
# Shows: Empty conversation files
# Action: None (dry-run)
```

### Example 4: Remove Empty Conversations
```bash
cycodj cleanup --remove-empty --execute
# Shows: Empty conversations found
# Prompts: Type 'DELETE' to confirm
# Action: Removes empty files
```

### Example 5: Find Old Conversations
```bash
cycodj cleanup --older-than-days 90
# Shows: Conversations older than 90 days
# Action: None (dry-run, no --find/--remove flag)
```

### Example 6: Combined Criteria
```bash
cycodj cleanup --find-duplicates --find-empty --older-than-days 30
# Shows: Files matching ANY criterion
# Action: None (dry-run)
```

### Example 7: No Criteria
```bash
cycodj cleanup
# Output:
# ERROR: Please specify at least one cleanup operation:
#   --find-duplicates     Find duplicate conversations
#   --find-empty          Find empty conversations
#   --older-than-days N   Find conversations older than N days
```

## Duplicate Detection Algorithm

### Signature Generation

**File**: `src/cycodj/CommandLineCommands/CleanupCommand.cs`

**Lines 136-140**: Creates content signature
```csharp
var signature = string.Join("|", conversation.Messages
    .Where(m => m.Role == "user" || m.Role == "assistant")
    .Take(10) // First 10 messages
    .Select(m => $"{m.Role}:{m.Content?.Length ?? 0}"));
```

**Signature format**: `role:length|role:length|...`
- Example: `user:245|assistant:1024|user:123|assistant:890|...`
- Only considers user/assistant messages
- Uses content LENGTH, not content itself (privacy-friendly)
- Takes first 10 messages only

### Duplicate Grouping

**Lines 154-177**: Groups and selects files to keep/remove
```csharp
var duplicateGroups = conversationsByContent.Where(kv => kv.Value.Count > 1).ToList();

foreach (var group in duplicateGroups)
{
    // Keep the newest, mark others for removal
    var sorted = group.Value.OrderByDescending(f => TimestampHelpers.ParseTimestamp(f)).ToList();
    var keep = sorted.First();
    var remove = sorted.Skip(1).ToList();
    
    // keep: newest file
    // remove: all older duplicates
}
```

## Empty Detection Algorithm

### Empty Definition

A conversation is "empty" if it has **ZERO user or assistant messages**:

**Lines 202-203**: Counts meaningful messages
```csharp
var meaningfulMessages = conversation.Messages.Count(m => 
    m.Role == "user" || m.Role == "assistant");
```

**Note**: System messages and tool messages don't count.

### Empty Detection

**Lines 205-208**: Marks empty conversations
```csharp
if (meaningfulMessages == 0)
{
    empty.Add(file);
}
```

## Age Detection Algorithm

### Cutoff Calculation

**Lines 235**: Calculates cutoff date
```csharp
var cutoffDate = DateTime.Now.AddDays(-olderThanDays);
```

### Age Check

**Lines 240-243**: Checks file age
```csharp
var timestamp = TimestampHelpers.ParseTimestamp(file);
if (timestamp < cutoffDate)
{
    old.Add(file);
}
```

## Differences from Other Commands

| Feature | list/search/branches/stats | show | cleanup |
|---------|----------------------------|------|---------|
| **Time filtering** | ✅ Rich | ❌ | ❌ |
| **Count limiting** | ✅ | ❌ | ❌ |
| **Type selection** | ❌ | ❌ | ✅ Duplicates/Empty |
| **Age selection** | ❌ | ❌ | ✅ --older-than-days |
| **Required criteria** | ❌ Optional | ✅ ID required | ✅ At least one type |
| **Action on results** | ❌ Read-only | ❌ Read-only | ✅ Can delete |
| **Safety mode** | N/A | N/A | ✅ Dry-run default |

## Performance Implications

### File Scanning
- **Impact**: Must scan ALL conversation files
- **Time**: O(n) where n = total files
- **Note**: No filtering before reading files

### Duplicate Detection
- **Complexity**: O(n) to read + O(n) to group
- **Memory**: Keeps all signatures in memory
- **Optimization**: Only reads first 10 messages

### Empty Detection
- **Complexity**: O(n) to read files
- **Memory**: Minimal (just file paths)
- **Fast**: Simple message count check

### Age Detection
- **Complexity**: O(n) for timestamp parsing
- **Memory**: Minimal (just file paths)
- **Fast**: No file reading needed

## Related Files

- **Implementation**: [cycodj-cleanup-layer-1-proof.md](cycodj-cleanup-layer-1-proof.md)
- **Parser**: [cycodj-cleanup-layer-1-proof.md#parser-evidence](cycodj-cleanup-layer-1-proof.md#parser-evidence)
- **Execution**: [cycodj-cleanup-layer-1-proof.md#execution-evidence](cycodj-cleanup-layer-1-proof.md#execution-evidence)

## See Also

- [Layer 9: Actions on Results](cycodj-cleanup-layer-9.md) - File deletion logic
- [HistoryFileHelpers](cycodj-cleanup-layer-1-proof.md#historyfilehelpers) - File finding utilities
- [TimestampHelpers](cycodj-cleanup-layer-1-proof.md#timestamphelpers) - Timestamp extraction

---

**Next Layer**: [Layer 9: Actions on Results](cycodj-cleanup-layer-9.md) (Layers 2-8 not implemented)
