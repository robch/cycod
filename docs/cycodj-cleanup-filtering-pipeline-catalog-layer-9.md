# cycodj cleanup Command - Layer 9: Actions on Results

[← Back to cleanup README](cycodj-cleanup-filtering-pipeline-catalog-README.md)

## Overview

**Layer 9: Actions on Results** - Perform operations on search results.

For the `cleanup` command, this layer represents the **primary purpose** of the command: to find and optionally delete conversation files that match certain criteria (duplicates, empty, old).

## Implementation Status

✅ **IMPLEMENTED** - The cleanup command performs file deletion actions based on user criteria.

---

## Command Line Options

### Action Control Options

| Option | Parsed At | Description |
|--------|-----------|-------------|
| `--find-duplicates` | Line 534 | Find duplicate conversations (analysis only) |
| `--remove-duplicates` | Line 535 | Find AND remove duplicate conversations |
| `--find-empty` | Line 536 | Find empty conversations (analysis only) |
| `--remove-empty` | Line 537 | Find AND remove empty conversations |
| `--older-than-days N` | Line 538-544 | Find conversations older than N days |
| `--execute` | Line 546 | Execute removal (disable dry-run mode) |

---

## Behavior

### Two-Stage Action Pattern

The cleanup command uses a two-stage pattern:

1. **FIND** - Identify files matching criteria (always performed)
2. **REMOVE** - Delete identified files (optional, controlled by flags)

### Safety Mechanisms

1. **Dry-Run by Default**: `DryRun` property defaults to `true` (line 16 of CleanupCommand.cs)
   - Must explicitly use `--execute` to actually delete files
   - Prevents accidental data loss

2. **Confirmation Prompt**: Even with `--execute`, requires typing "DELETE" to confirm (lines 84-92 of CleanupCommand.cs)
   - Interactive safety check
   - Can be cancelled at the last moment

3. **Separate Find/Remove Flags**:
   - `--find-duplicates` only finds (analysis)
   - `--remove-duplicates` finds AND enables removal
   - Same pattern for `--find-empty` / `--remove-empty`

---

## Action Types

### 1. Duplicate Removal

**Trigger**: `--remove-duplicates`

**Algorithm** (line 121-186):
1. Signature-based duplicate detection
2. Groups files with identical signatures
3. Keeps newest file in each group
4. Marks others for removal

**Signature**: First 10 user/assistant messages (role + content length) - line 137-140

### 2. Empty Conversation Removal

**Trigger**: `--remove-empty`

**Criteria** (line 202-203):
- Conversations with zero `user` or `assistant` messages
- System/tool-only conversations are considered empty

### 3. Old Conversation Removal

**Trigger**: `--older-than-days N`

**Criteria** (line 235):
- Files with timestamp older than `DateTime.Now.AddDays(-N)`
- Based on filename timestamp parsing

---

## Execution Flow

### Phase 1: Collection (lines 40-56)

```
FindDuplicates? → FindDuplicateConversationsAsync()
FindEmpty? → FindEmptyConversations()
OlderThanDays? → FindOldConversations()
→ Combine all into toRemove list
→ Deduplicate list
```

### Phase 2: Display (lines 58-72)

```
Show count of files to remove
List each file with size
```

### Phase 3: Execution (lines 75-116)

```
IF DryRun AND (RemoveDuplicates OR RemoveEmpty):
  → Display "DRY RUN" message
  → Exit (no deletion)

IF NOT DryRun AND (RemoveDuplicates OR RemoveEmpty):
  → Show WARNING
  → Prompt for "DELETE" confirmation
  → IF confirmed:
    → Delete each file
    → Track success/failure
    → Display results
```

---

## Implementation Details

### File Deletion

**Location**: Lines 97-111 of CleanupCommand.cs

**Process**:
1. Try to get file size before deletion
2. Call `File.Delete(file)`
3. Increment counter and accumulate size
4. Handle exceptions per-file (doesn't stop on failure)

**Output**:
- Success: Green checkmark + filename
- Failure: Red X + filename + error message
- Summary: Total deleted count + freed space in MB

### Error Handling

**Per-File Errors**: Caught and logged, but execution continues for remaining files (line 107-110)

**Prevents**: One failed deletion from blocking cleanup of other files

---

## Data Flow

```
Command Line Args
  ↓
Parse Options (--find-*, --remove-*, --older-than-days, --execute)
  ↓
Set Properties (FindDuplicates, RemoveDuplicates, FindEmpty, RemoveEmpty, OlderThanDays, DryRun)
  ↓
ExecuteAsync() - Line 18
  ↓
FindAllHistoryFiles() - Get all conversation files
  ↓
Collection Phase:
  ├→ FindDuplicateConversationsAsync() if FindDuplicates - Line 121
  ├→ FindEmptyConversations() if FindEmpty - Line 188
  └→ FindOldConversations() if OlderThanDays - Line 230
  ↓
Combine & Deduplicate toRemove list - Line 56
  ↓
Display Phase:
  ├→ Show files to remove - Lines 64-71
  └→ Exit if no files
  ↓
Execution Phase:
  ├→ If DryRun: Show message, exit - Lines 75-79
  └→ If NOT DryRun:
      ├→ Prompt for confirmation - Lines 84-92
      ├→ If confirmed:
      │   ├→ Delete each file - Lines 97-111
      │   └→ Display summary - Lines 113-115
      └→ If cancelled: Exit
```

---

## Key Properties

### From Command Class

| Property | Type | Default | Parsed At | Purpose |
|----------|------|---------|-----------|---------|
| `FindDuplicates` | bool | false | Line 534 | Enable duplicate finding |
| `RemoveDuplicates` | bool | false | Line 535 | Enable duplicate removal + finding |
| `FindEmpty` | bool | false | Line 536 | Enable empty finding |
| `RemoveEmpty` | bool | false | Line 537 | Enable empty removal + finding |
| `OlderThanDays` | int? | null | Line 538-544 | Age threshold in days |
| `DryRun` | bool | **true** | Line 546 (inverse) | Safety flag - prevent actual deletion |

### Important: Default Behavior

The `DryRun` property defaults to `true` (line 16 of CleanupCommand.cs), which means:
- By default, NO files are deleted
- User must explicitly use `--execute` to enable deletion
- This is a critical safety feature

---

## Examples

### Find Duplicates (No Removal)

```bash
cycodj cleanup --find-duplicates
```

**Result**: Lists duplicate groups, marks files for removal, but doesn't delete (dry-run)

### Remove Duplicates (With Execution)

```bash
cycodj cleanup --remove-duplicates --execute
```

**Result**: 
1. Finds duplicates
2. Shows confirmation prompt
3. Deletes older duplicates after confirmation
4. Displays summary

### Multiple Criteria

```bash
cycodj cleanup --remove-duplicates --remove-empty --older-than-days 90 --execute
```

**Result**: Finds and removes conversations that are:
- Duplicates (older copies), OR
- Empty (no user/assistant messages), OR
- Older than 90 days

### Dry Run (Implicit)

```bash
cycodj cleanup --remove-duplicates
```

**Result**: Shows what would be deleted, but doesn't actually delete (DryRun=true by default)

---

## Edge Cases

### No Criteria Specified

**Input**: `cycodj cleanup`

**Behavior**: Error message listing available options (lines 23-29)

### No Files Match Criteria

**Input**: `cycodj cleanup --find-empty`

**Behavior**: "✓ No files need cleanup!" message (line 60)

### User Cancels Confirmation

**Input**: `cycodj cleanup --remove-duplicates --execute` → types anything other than "DELETE"

**Behavior**: "Cancelled." message, no deletion (lines 88-91)

### File Deletion Fails

**Behavior**: Error logged for that file, continues with remaining files (lines 107-110)

---

## Safety Features

1. **Dry-Run Default**: Must explicitly opt-in to deletion
2. **Confirmation Prompt**: Must type "DELETE" exactly
3. **Per-File Error Handling**: One failure doesn't stop cleanup
4. **Detailed Logging**: Shows which files were deleted/failed
5. **Summary Display**: Shows freed space after cleanup

---

## Related Files

- **Command Implementation**: `src/cycodj/CommandLineCommands/CleanupCommand.cs`
- **Parser**: `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs` (lines 532-548)
- **Helper Functions**: None (cleanup logic is self-contained in CleanupCommand)

---

## Proof

See [Layer 9 Proof](cycodj-cleanup-filtering-pipeline-catalog-layer-9-proof.md) for detailed source code evidence with line numbers.
