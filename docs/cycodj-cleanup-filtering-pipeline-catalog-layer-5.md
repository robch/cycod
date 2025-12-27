# cycodj cleanup Command - Layer 5: Context Expansion

## Status: NOT IMPLEMENTED

The `cleanup` command does **not implement Layer 5 (Context Expansion)**.

## What is Layer 5?

Layer 5 (Context Expansion) provides the ability to show surrounding lines or messages around matched content to provide context.

## Why Not Implemented in cleanup?

The `cleanup` command performs **file management operations** on conversation history files. It does not display message content or perform content searching, so there are no "matches" around which to expand context.

### What cleanup Does Instead

The cleanup command identifies and optionally removes files:
- `--find-duplicates` / `--remove-duplicates` - Find/remove duplicate conversations
- `--find-empty` / `--remove-empty` - Find/remove empty conversations
- `--older-than-days N` - Find conversations older than N days
- `--execute` - Actually perform deletions (default is dry-run)

This is **NOT context expansion** because:
1. No message content is displayed
2. Operations are at the file level, not content level
3. Output shows file names and metadata, not content

## Example of What cleanup Shows

```bash
cycodj cleanup --find-duplicates
```

Output:
```
## Chat History Cleanup

Scanning 150 conversation files...

### Finding Duplicate Conversations

Found 2 group(s) of duplicates:

  Duplicate group (2 files):
    KEEP: chat-history-20240115-103000-123456.jsonl
    remove: chat-history-20240115-102959-123455.jsonl

  Duplicate group (3 files):
    KEEP: chat-history-20240114-150000-789012.jsonl
    remove: chat-history-20240114-145959-789011.jsonl
    remove: chat-history-20240114-145958-789010.jsonl

DRY RUN - No files will be deleted.
Add --execute to actually remove files.
```

Shows file names only - no message content or context.

## Related Options

### Actions on Results (Layer 9)
The cleanup command DOES implement Layer 9:
- `--execute` - Perform actual file deletions
- `--remove-duplicates` - Remove duplicate files
- `--remove-empty` - Remove empty conversation files

This is about **actions** on files, not content expansion.

### Display Control (Layer 6)
- File listing with metadata (size, date)
- Dry-run preview before deletion
- Confirmation prompt before executing

These control the cleanup process display but do not show message content.

## If Context Expansion Were Implemented

If Layer 5 were added to the cleanup command, it might work like:
- `--show-content` - Show sample messages from files being considered for cleanup
- `--preview-messages N` - Show N messages from each file in dry-run mode
- `--why-duplicate` - Show comparison of duplicate content with context

However, this would add significant complexity and change the command's focus from efficient file management to content analysis.

## Comparison with Other Commands

| Command | Layer 5 Support | Implementation |
|---------|----------------|----------------|
| **list** | ❌ No | Message previews (not context) |
| **show** | ❌ No | Shows entire messages |
| **search** | ✅ Yes | `--context N` for line-level context |
| **branches** | ❌ No | Message previews (not context) |
| **stats** | ❌ No | Aggregate data only |
| **cleanup** | ❌ No | File operations only |

## Use Case Distinction

The cleanup command focuses on **file management**:
- Disk space management
- Removing duplicates
- Archiving old conversations
- Data hygiene

This is fundamentally different from **content-focused commands** that work with message content.

## Recommended Workflow

Before using cleanup's `--execute` option, users can:

1. **Identify files to clean**:
```bash
cycodj cleanup --find-duplicates --older-than-days 90
```

2. **Review content if needed** (using other commands):
```bash
# List conversations being considered for cleanup
cycodj list --last 90d

# Show specific conversation before deletion
cycodj show chat-history-20240115-103000-123456
```

3. **Execute cleanup**:
```bash
cycodj cleanup --remove-duplicates --older-than-days 90 --execute
```

## Safety Features

The cleanup command has built-in safeguards:
- **Dry-run by default**: Must explicitly use `--execute`
- **Confirmation prompt**: Requires typing "DELETE" to confirm
- **Keeps newest**: For duplicates, always keeps the most recent file
- **Detailed logging**: Shows which files will be affected

These are safety features in Layer 6 (Display Control) and Layer 9 (Actions), not context expansion.

## Navigation

- [← Layer 4: Content Removal](cycodj-cleanup-filtering-pipeline-catalog-layer-4.md)
- [→ Layer 6: Display Control](cycodj-cleanup-filtering-pipeline-catalog-layer-6.md)
- [↑ cleanup Command Overview](cycodj-cleanup-filtering-pipeline-catalog-README.md)
- [↑ cycodj Main Catalog](cycodj-filtering-pipeline-catalog-README.md)

## See Also

- [search Layer 5](cycodj-search-filtering-pipeline-catalog-layer-5.md) - The only cycodj command that implements context expansion
- [cleanup Layer 9](cycodj-cleanup-filtering-pipeline-catalog-layer-9.md) - Actions on files
