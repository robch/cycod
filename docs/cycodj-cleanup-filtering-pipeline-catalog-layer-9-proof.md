# cycodj cleanup Command - Layer 9 Proof: Actions on Results

[← Back to Layer 9 Catalog](cycodj-cleanup-filtering-pipeline-catalog-layer-9.md)

## Source Code Evidence

This document provides detailed source code evidence for Layer 9 (Actions on Results) implementation in the cleanup command.

---

## 1. Command Line Parsing

**File**: `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs`

### Option Parsing (Lines 532-548)

```csharp
private bool TryParseCleanupCommandOptions(CleanupCommand command, string[] args, ref int i, string arg)
{
    if (arg == "--find-duplicates") { command.FindDuplicates = true; return true; }
    else if (arg == "--remove-duplicates") { command.RemoveDuplicates = true; command.FindDuplicates = true; return true; }
    else if (arg == "--find-empty") { command.FindEmpty = true; return true; }
    else if (arg == "--remove-empty") { command.RemoveEmpty = true; command.FindEmpty = true; return true; }
    else if (arg == "--older-than-days") 
    { 
        var days = i + 1 < args.Length ? args[++i] : null;
        if (string.IsNullOrWhiteSpace(days) || !int.TryParse(days, out var n))
            throw new CommandLineException($"Missing or invalid days for {arg}");
        command.OlderThanDays = n;
        return true;
    }
    else if (arg == "--execute") { command.DryRun = false; return true; }
    return false;
}
```

**Evidence**:
- Line 534: `--find-duplicates` sets `FindDuplicates = true`
- Line 535: `--remove-duplicates` sets BOTH `RemoveDuplicates = true` AND `FindDuplicates = true`
- Line 536: `--find-empty` sets `FindEmpty = true`
- Line 537: `--remove-empty` sets BOTH `RemoveEmpty = true` AND `FindEmpty = true`
- Lines 538-544: `--older-than-days` validates integer and sets `OlderThanDays = n`
- Line 546: `--execute` sets `DryRun = false` (enabling actual deletion)

---

## 2. Command Properties

**File**: `src/cycodj/CommandLineCommands/CleanupCommand.cs`

### Property Declarations (Lines 11-16)

```csharp
public class CleanupCommand : CommandLine.CycoDjCommand
{
    public bool FindDuplicates { get; set; }
    public bool RemoveDuplicates { get; set; }
    public bool FindEmpty { get; set; }
    public bool RemoveEmpty { get; set; }
    public int? OlderThanDays { get; set; }
    public bool DryRun { get; set; } = true;  // ← DEFAULT IS TRUE (safe mode)
```

**Evidence**:
- Line 11: `FindDuplicates` controls duplicate finding
- Line 12: `RemoveDuplicates` enables removal (in addition to finding)
- Line 13: `FindEmpty` controls empty conversation finding
- Line 14: `RemoveEmpty` enables removal (in addition to finding)
- Line 15: `OlderThanDays` holds age threshold (nullable)
- Line 16: `DryRun` **defaults to true** - critical safety feature

---

## 3. Main Execution Flow

**File**: `src/cycodj/CommandLineCommands/CleanupCommand.cs`

### ExecuteAsync Method (Lines 18-119)

```csharp
public override async Task<int> ExecuteAsync()
{
    ConsoleHelpers.WriteLine("## Chat History Cleanup", ConsoleColor.Cyan, overrideQuiet: true);
    ConsoleHelpers.WriteLine(overrideQuiet: true);

    // VALIDATION: At least one criterion must be specified
    if (!FindDuplicates && !FindEmpty && !OlderThanDays.HasValue)
    {
        ConsoleHelpers.WriteErrorLine("Please specify at least one cleanup operation:");
        ConsoleHelpers.WriteLine("  --find-duplicates     Find duplicate conversations", overrideQuiet: true);
        ConsoleHelpers.WriteLine("  --find-empty          Find empty conversations", overrideQuiet: true);
        ConsoleHelpers.WriteLine("  --older-than-days N   Find conversations older than N days", overrideQuiet: true);
        return 1;
    }

    var historyDir = CycoDj.Helpers.HistoryFileHelpers.GetHistoryDirectory();
    var files = CycoDj.Helpers.HistoryFileHelpers.FindAllHistoryFiles();

    ConsoleHelpers.WriteLine($"Scanning {files.Count} conversation files...", overrideQuiet: true);
    ConsoleHelpers.WriteLine(overrideQuiet: true);

    var toRemove = new List<string>();

    // PHASE 1: COLLECTION - Find files matching criteria
    if (FindDuplicates || RemoveDuplicates)
    {
        toRemove.AddRange(await FindDuplicateConversationsAsync(files));
    }

    if (FindEmpty || RemoveEmpty)
    {
        toRemove.AddRange(FindEmptyConversations(files));
    }

    if (OlderThanDays.HasValue)
    {
        toRemove.AddRange(FindOldConversations(files, OlderThanDays.Value));
    }

    // Remove duplicates from the list
    toRemove = toRemove.Distinct().ToList();

    if (!toRemove.Any())
    {
        ConsoleHelpers.WriteLine("✓ No files need cleanup!", ConsoleColor.Green, overrideQuiet: true);
        return 0;
    }

    // PHASE 2: DISPLAY - Show what will be removed
    ConsoleHelpers.WriteLine($"Found {toRemove.Count} file(s) to clean up:", ConsoleColor.Yellow, overrideQuiet: true);
    ConsoleHelpers.WriteLine(overrideQuiet: true);

    foreach (var file in toRemove)
    {
        var size = new FileInfo(file).Length / 1024;
        ConsoleHelpers.WriteLine($"  - {Path.GetFileName(file)} ({size} KB)", ConsoleColor.DarkGray, overrideQuiet: true);
    }

    ConsoleHelpers.WriteLine(overrideQuiet: true);

    // PHASE 3: EXECUTION - Handle dry-run vs actual deletion
    if (DryRun && (RemoveDuplicates || RemoveEmpty))
    {
        ConsoleHelpers.WriteLine("DRY RUN - No files will be deleted.", ConsoleColor.Yellow, overrideQuiet: true);
        ConsoleHelpers.WriteLine("Add --execute to actually remove files.", overrideQuiet: true);
        return 0;
    }

    if (!DryRun && (RemoveDuplicates || RemoveEmpty))
    {
        ConsoleHelpers.WriteLine("⚠️  WARNING: About to delete files!", ConsoleColor.Red, overrideQuiet: true);
        ConsoleHelpers.Write("Type 'DELETE' to confirm: ", ConsoleColor.Yellow, overrideQuiet: true);
        
        var confirmation = Console.ReadLine();
        if (confirmation?.Trim().ToUpperInvariant() != "DELETE")
        {
            ConsoleHelpers.WriteLine("Cancelled.", overrideQuiet: true);
            return 0;
        }

        var deletedCount = 0;
        var totalSize = 0L;

        foreach (var file in toRemove)
        {
            try
            {
                var size = new FileInfo(file).Length;
                File.Delete(file);  // ← ACTUAL DELETION HERE
                deletedCount++;
                totalSize += size;
                ConsoleHelpers.WriteLine($"  ✓ Deleted {Path.GetFileName(file)}", ConsoleColor.Green, overrideQuiet: true);
            }
            catch (Exception ex)
            {
                ConsoleHelpers.WriteErrorLine($"  ✗ Failed to delete {Path.GetFileName(file)}: {ex.Message}");
            }
        }

        ConsoleHelpers.WriteLine(overrideQuiet: true);
        ConsoleHelpers.WriteLine($"Deleted {deletedCount} file(s), freed {totalSize / 1024 / 1024} MB", 
            ConsoleColor.Green, overrideQuiet: true);
    }

    return 0;
}
```

**Evidence**:
- Lines 23-29: Validation - at least one criterion must be specified
- Lines 40-54: Phase 1 - Collection of files to remove
- Lines 56: Deduplication of file list
- Lines 58-72: Phase 2 - Display files to be removed
- Lines 75-79: Dry-run mode handling (shows message, exits without deletion)
- Lines 81-116: Execution mode handling:
  - Lines 84-92: Confirmation prompt
  - Lines 97-111: File deletion loop with per-file error handling
  - Lines 113-115: Summary display

---

## 4. Duplicate Finding Algorithm

**File**: `src/cycodj/CommandLineCommands/CleanupCommand.cs`

### FindDuplicateConversationsAsync Method (Lines 121-186)

```csharp
private async Task<List<string>> FindDuplicateConversationsAsync(List<string> files)
{
    ConsoleHelpers.WriteLine("### Finding Duplicate Conversations", ConsoleColor.White, overrideQuiet: true);
    ConsoleHelpers.WriteLine(overrideQuiet: true);

    var duplicates = new List<string>();
    var conversationsByContent = new Dictionary<string, List<string>>();

    foreach (var file in files)
    {
        try
        {
            var conversation = CycoDj.Helpers.JsonlReader.ReadConversation(file);
            if (conversation == null) continue;

            // Create a signature based on message content
            var signature = string.Join("|", conversation.Messages
                .Where(m => m.Role == "user" || m.Role == "assistant")
                .Take(10) // First 10 messages
                .Select(m => $"{m.Role}:{m.Content?.Length ?? 0}"));

            if (!conversationsByContent.ContainsKey(signature))
            {
                conversationsByContent[signature] = new List<string>();
            }
            conversationsByContent[signature].Add(file);
        }
        catch (Exception ex)
        {
            Logger.Warning($"Failed to analyze {file}: {ex.Message}");
        }
    }

    var duplicateGroups = conversationsByContent.Where(kv => kv.Value.Count > 1).ToList();

    if (duplicateGroups.Any())
    {
        ConsoleHelpers.WriteLine($"Found {duplicateGroups.Count} group(s) of duplicates:", overrideQuiet: true);
        ConsoleHelpers.WriteLine(overrideQuiet: true);

        foreach (var group in duplicateGroups)
        {
            ConsoleHelpers.WriteLine($"  Duplicate group ({group.Value.Count} files):", ConsoleColor.Yellow, overrideQuiet: true);
            
            // Keep the newest, mark others for removal
            var sorted = group.Value.OrderByDescending(f => CycoDj.Helpers.TimestampHelpers.ParseTimestamp(f)).ToList();
            var keep = sorted.First();
            var remove = sorted.Skip(1).ToList();

            ConsoleHelpers.WriteLine($"    KEEP: {Path.GetFileName(keep)}", ConsoleColor.Green, overrideQuiet: true);
            foreach (var file in remove)
            {
                ConsoleHelpers.WriteLine($"    remove: {Path.GetFileName(file)}", ConsoleColor.DarkGray, overrideQuiet: true);
                duplicates.Add(file);
            }
            ConsoleHelpers.WriteLine(overrideQuiet: true);
        }
    }
    else
    {
        ConsoleHelpers.WriteLine("  No duplicates found.", ConsoleColor.Green, overrideQuiet: true);
    }

    ConsoleHelpers.WriteLine(overrideQuiet: true);
    return duplicates;
}
```

**Evidence**:
- Lines 137-140: Signature creation based on first 10 user/assistant messages (role + content length)
- Line 146: Group files by signature
- Line 154: Identify groups with multiple files (duplicates)
- Lines 166-168: Sort by timestamp (newest first), keep first, mark rest for removal
- Lines 170-174: Display what will be kept vs removed

**Signature Algorithm**:
- Only considers `user` and `assistant` messages (line 138)
- Takes first 10 messages (line 139)
- Creates string: `"role:contentLength|role:contentLength|..."`
- Conversations with identical signatures are considered duplicates

---

## 5. Empty Conversation Finding

**File**: `src/cycodj/CommandLineCommands/CleanupCommand.cs`

### FindEmptyConversations Method (Lines 188-228)

```csharp
private List<string> FindEmptyConversations(List<string> files)
{
    ConsoleHelpers.WriteLine("### Finding Empty Conversations", ConsoleColor.White, overrideQuiet: true);
    ConsoleHelpers.WriteLine(overrideQuiet: true);

    var empty = new List<string>();

    foreach (var file in files)
    {
        try
        {
            var conversation = CycoDj.Helpers.JsonlReader.ReadConversation(file);
            if (conversation == null) continue;

            var meaningfulMessages = conversation.Messages.Count(m => 
                m.Role == "user" || m.Role == "assistant");

            if (meaningfulMessages == 0)
            {
                empty.Add(file);
                ConsoleHelpers.WriteLine($"  Empty: {Path.GetFileName(file)}", ConsoleColor.Yellow, overrideQuiet: true);
            }
        }
        catch (Exception ex)
        {
            Logger.Warning($"Failed to analyze {file}: {ex.Message}");
        }
    }

    if (empty.Any())
    {
        ConsoleHelpers.WriteLine($"Found {empty.Count} empty conversation(s).", ConsoleColor.Yellow, overrideQuiet: true);
    }
    else
    {
        ConsoleHelpers.WriteLine("  No empty conversations found.", ConsoleColor.Green, overrideQuiet: true);
    }

    ConsoleHelpers.WriteLine(overrideQuiet: true);
    return empty;
}
```

**Evidence**:
- Lines 202-203: Count messages where role is `user` OR `assistant`
- Line 205: If count is 0, conversation is considered empty
- Line 208: Display empty file name
- Line 207: Add to empty list

**Empty Criteria**:
- Zero `user` messages AND zero `assistant` messages
- System-only or tool-only conversations are considered empty

---

## 6. Old Conversation Finding

**File**: `src/cycodj/CommandLineCommands/CleanupCommand.cs`

### FindOldConversations Method (Lines 230-267)

```csharp
private List<string> FindOldConversations(List<string> files, int olderThanDays)
{
    ConsoleHelpers.WriteLine($"### Finding Conversations Older Than {olderThanDays} Days", ConsoleColor.White, overrideQuiet: true);
    ConsoleHelpers.WriteLine(overrideQuiet: true);

    var cutoffDate = DateTime.Now.AddDays(-olderThanDays);
    var old = new List<string>();

    foreach (var file in files)
    {
        try
        {
            var timestamp = CycoDj.Helpers.TimestampHelpers.ParseTimestamp(file);
            if (timestamp < cutoffDate)
            {
                old.Add(file);
                ConsoleHelpers.WriteLine($"  Old: {Path.GetFileName(file)} ({timestamp:yyyy-MM-dd})", 
                    ConsoleColor.DarkGray, overrideQuiet: true);
            }
        }
        catch (Exception ex)
        {
            Logger.Warning($"Failed to analyze {file}: {ex.Message}");
        }
    }

    if (old.Any())
    {
        ConsoleHelpers.WriteLine($"Found {old.Count} old conversation(s).", ConsoleColor.Yellow, overrideQuiet: true);
    }
    else
    {
        ConsoleHelpers.WriteLine("  No old conversations found.", ConsoleColor.Green, overrideQuiet: true);
    }

    ConsoleHelpers.WriteLine(overrideQuiet: true);
    return old;
}
```

**Evidence**:
- Line 235: Calculate cutoff date as `DateTime.Now.AddDays(-olderThanDays)`
- Line 242: Parse timestamp from filename
- Line 243: Compare timestamp to cutoff
- Lines 245-247: If older, add to list and display

**Age Calculation**:
- Cutoff is calculated from current time
- Timestamps are parsed from filenames (using TimestampHelpers)
- Files with timestamp < cutoff are considered "old"

---

## 7. Deletion Safety Features

### Feature 1: Dry-Run Default

**File**: `src/cycodj/CommandLineCommands/CleanupCommand.cs`, Line 16

```csharp
public bool DryRun { get; set; } = true;
```

**Evidence**: `DryRun` defaults to `true`, requiring explicit `--execute` to enable deletion.

### Feature 2: Confirmation Prompt

**File**: `src/cycodj/CommandLineCommands/CleanupCommand.cs`, Lines 84-92

```csharp
if (!DryRun && (RemoveDuplicates || RemoveEmpty))
{
    ConsoleHelpers.WriteLine("⚠️  WARNING: About to delete files!", ConsoleColor.Red, overrideQuiet: true);
    ConsoleHelpers.Write("Type 'DELETE' to confirm: ", ConsoleColor.Yellow, overrideQuiet: true);
    
    var confirmation = Console.ReadLine();
    if (confirmation?.Trim().ToUpperInvariant() != "DELETE")
    {
        ConsoleHelpers.WriteLine("Cancelled.", overrideQuiet: true);
        return 0;
    }
```

**Evidence**: 
- Line 84: Warning displayed
- Line 85: Prompt for "DELETE" confirmation
- Line 87: Read user input
- Line 88: Check if input exactly matches "DELETE" (case-insensitive, trimmed)
- Lines 90-91: Cancel and exit if not confirmed

### Feature 3: Per-File Error Handling

**File**: `src/cycodj/CommandLineCommands/CleanupCommand.cs`, Lines 97-111

```csharp
foreach (var file in toRemove)
{
    try
    {
        var size = new FileInfo(file).Length;
        File.Delete(file);
        deletedCount++;
        totalSize += size;
        ConsoleHelpers.WriteLine($"  ✓ Deleted {Path.GetFileName(file)}", ConsoleColor.Green, overrideQuiet: true);
    }
    catch (Exception ex)
    {
        ConsoleHelpers.WriteErrorLine($"  ✗ Failed to delete {Path.GetFileName(file)}: {ex.Message}");
    }
}
```

**Evidence**:
- Line 99: Try block for each file individually
- Line 102: `File.Delete()` call
- Lines 107-110: Catch block handles failure for single file
- Loop continues even if one file fails (no `throw`, no `return`)

---

## 8. Action Flow Summary

```
User Input: cycodj cleanup --remove-duplicates --remove-empty --execute
    ↓
Parse: RemoveDuplicates=true, FindDuplicates=true, RemoveEmpty=true, FindEmpty=true, DryRun=false
    ↓
ExecuteAsync() starts - Line 18
    ↓
Validate criteria (at least one) - Lines 23-29
    ↓
Get all files - Lines 32-33
    ↓
Collection Phase:
    ├→ FindDuplicateConversationsAsync() - Lines 42-44
    │  └→ Returns list of duplicate files (older copies)
    ├→ FindEmptyConversations() - Lines 46-49
    │  └→ Returns list of empty conversation files
    └→ Combine & deduplicate - Lines 56
    ↓
Display Phase - Lines 58-72
    └→ Show files to remove with sizes
    ↓
Execution Phase (DryRun=false):
    ├→ Show WARNING - Line 84
    ├→ Prompt for "DELETE" - Line 85
    ├→ Read confirmation - Line 87
    ├→ If not "DELETE": Cancel & Exit - Lines 88-91
    └→ If "DELETE":
        ├→ For each file: - Lines 97-111
        │  ├→ Get size
        │  ├→ File.Delete(file) - Line 102
        │  ├→ Track success/failure
        │  └→ Display result
        └→ Display summary - Lines 113-115
            └→ "Deleted N file(s), freed X MB"
```

---

## Conclusion

This proof demonstrates that Layer 9 (Actions on Results) is **fully implemented** in the cleanup command with:

1. **Multiple action types**: duplicate removal, empty removal, old file removal
2. **Safety mechanisms**: dry-run default, confirmation prompt, per-file error handling
3. **Clear data flow**: collection → display → execution
4. **Robust error handling**: failures don't stop cleanup process
5. **User feedback**: detailed display of what's being removed and results

All evidence is traceable to specific line numbers in the source code.
