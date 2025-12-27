# cycodj show - Layer 1 Proof: SOURCE CODE EVIDENCE

## Overview

This document provides complete source code evidence for Layer 1 (TARGET SELECTION) implementation in the `show` command, with exact line numbers and code excerpts.

---

## Parser Evidence

### Command Registration

**File**: `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs`

**Lines 37-48**: Command factory recognizes "show"
```csharp
37:     override protected Command? NewCommandFromName(string commandName)
38:     {
39:         var lowerCommandName = commandName.ToLowerInvariant();
40:         
41:         if (lowerCommandName.StartsWith("list")) return new ListCommand();
42:         if (lowerCommandName.StartsWith("show")) return new ShowCommand();
43:         if (lowerCommandName.StartsWith("branches")) return new BranchesCommand();
44:         if (lowerCommandName.StartsWith("search")) return new SearchCommand();
45:         if (lowerCommandName.StartsWith("stats")) return new StatsCommand();
46:         if (lowerCommandName.StartsWith("cleanup")) return new CleanupCommand();
47:         
48:         return base.NewCommandFromName(commandName);
49:     }
```

### ShowCommand Option Parsing

**File**: `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs`

**Lines 368-405**: ShowCommand option parsing
```csharp
368:     private bool TryParseShowCommandOptions(ShowCommand command, string[] args, ref int i, string arg)
369:     {
370:         // First positional argument is the conversation ID
371:         if (!arg.StartsWith("-") && string.IsNullOrEmpty(command.ConversationId))
372:         {
373:             command.ConversationId = arg;
374:             return true;
375:         }
376:         
377:         // Try common display options first
378:         if (TryParseDisplayOptions(command, args, ref i, arg))
379:         {
380:             return true;
381:         }
382:         
383:         if (arg == "--show-tool-calls")
384:         {
385:             command.ShowToolCalls = true;
386:             return true;
387:         }
388:         else if (arg == "--show-tool-output")
389:         {
390:             command.ShowToolOutput = true;
391:             return true;
392:         }
393:         else if (arg == "--max-content-length")
394:         {
395:             var length = i + 1 < args.Length ? args[++i] : null;
396:             if (string.IsNullOrWhiteSpace(length) || !int.TryParse(length, out var n))
397:             {
398:                 throw new CommandLineException($"Missing or invalid length for {arg}");
399:             }
400:             command.MaxContentLength = n;
401:             return true;
402:         }
403:         
404:         return false;
405:     }
```

**Key Lines**:
- **Line 370-375**: Positional argument parsing for conversation ID
  - Checks: `!arg.StartsWith("-")` (not an option)
  - Checks: `string.IsNullOrEmpty(command.ConversationId)` (first positional arg)
  - Sets: `command.ConversationId = arg`
- **Line 378**: Calls `TryParseDisplayOptions()` for display options (Layer 6)
- **Line 383-402**: Display control options (Layer 6, not Layer 1)

### No Time Filtering Options

**IMPORTANT**: The `show` command does NOT call `TryParseTimeOptions()`:

```csharp
// Compare to search/list/branches/stats which have:
// if (TryParseTimeOptions(command, args, ref i, arg))

// ShowCommand does NOT have this line!
```

This is intentional - show command doesn't support time filtering.

---

## Execution Evidence

### ShowCommand Class Definition

**File**: `src/cycodj/CommandLineCommands/ShowCommand.cs`

**Lines 10-16**: Properties for target selection
```csharp
10: public class ShowCommand : CycoDjCommand
11: {
12:     public string ConversationId { get; set; } = string.Empty;
13:     public bool ShowToolCalls { get; set; } = false;
14:     public bool ShowToolOutput { get; set; } = false;
15:     public int MaxContentLength { get; set; } = 500;
16:     public bool ShowStats { get; set; } = false;
```

**Key Property for Layer 1**:
- **Line 12**: `ConversationId` - Required conversation identifier
- **NO** `Date`, `Last`, `After`, `Before` properties (unlike list/search)

### Target Selection Implementation

**Lines 37-67**: GenerateShowOutput - Target selection logic
```csharp
37:     private string GenerateShowOutput()
38:     {
39:         var sb = new System.Text.StringBuilder();
40:         
41:         if (string.IsNullOrEmpty(ConversationId))
42:         {
43:             sb.AppendLine("ERROR: Conversation ID is required");
44:             sb.AppendLine("Usage: cycodj show <conversation-id>");
45:             return sb.ToString();
46:         }
47: 
48:         // Find the conversation file
49:         var files = HistoryFileHelpers.FindAllHistoryFiles();
50:         var matchingFile = files.FirstOrDefault(f => 
51:             f.Contains(ConversationId) || 
52:             System.IO.Path.GetFileNameWithoutExtension(f) == ConversationId);
53: 
54:         if (matchingFile == null)
55:         {
56:             sb.AppendLine($"ERROR: Conversation not found: {ConversationId}");
57:             sb.AppendLine($"Searched {files.Count} chat history files");
58:             return sb.ToString();
59:         }
60: 
61:         // Read the conversation
62:         var conversation = JsonlReader.ReadConversation(matchingFile);
63:         if (conversation == null)
64:         {
65:             sb.AppendLine($"ERROR: Failed to read conversation from: {matchingFile}");
66:             return sb.ToString();
67:         }
```

**Key Lines**:
- **Line 41-46**: Validation - ConversationId is required
- **Line 49**: Find ALL history files (no filtering)
- **Line 50-52**: Matching logic
  - `f.Contains(ConversationId)` - Substring match
  - `Path.GetFileNameWithoutExtension(f) == ConversationId` - Exact match
- **Line 50**: `.FirstOrDefault()` - First match wins (no disambiguation)
- **Line 54-59**: Error handling for no match found
- **Line 62**: Read the matched conversation

### Execution Flow Diagram

```
ExecuteAsync() [Line 18]
    ↓
GenerateShowOutput() [Line 37]
    ↓
Step 1: Validate ConversationId [Lines 41-46]
    IF ConversationId is null or empty
        ERROR: "Conversation ID is required"
        Show usage
        RETURN early
    ↓
Step 2: Find all files [Line 49]
    files = HistoryFileHelpers.FindAllHistoryFiles()
    (NO time filtering applied)
    ↓
Step 3: Find matching file [Lines 50-52]
    matchingFile = files.FirstOrDefault(f =>
        f.Contains(ConversationId) OR
        Path.GetFileNameWithoutExtension(f) == ConversationId)
    ↓
Step 4: Validate match found [Lines 54-59]
    IF matchingFile is null
        ERROR: "Conversation not found"
        Show search count
        RETURN early
    ↓
Step 5: Read conversation [Line 62]
    conversation = JsonlReader.ReadConversation(matchingFile)
    ↓
Step 6: Display conversation [Lines 69-229]
    (Layer 3 and Layer 6 processing)
```

---

## Comparison: show vs list/search

### Property Differences

| Property | list | search | show | Purpose |
|----------|------|--------|------|---------|
| `ConversationId` | ❌ | ❌ | ✅ `string` | Target conversation |
| `Query` | ❌ | ✅ `string?` | ❌ | Search query |
| `Date` | ✅ `string?` | ✅ `string?` | ❌ | Legacy date filter |
| `Last` | ✅ `int` | ✅ `int?` | ❌ | Count limit |
| `After`/`Before` | ✅ Inherited | ✅ Inherited | ✅ Inherited (unused!) | Time range |

**Note**: `show` inherits `After`/`Before` from `CycoDjCommand` but never uses them!

### Parser Call Differences

| Parser Method | list | search | show | Purpose |
|---------------|------|--------|------|---------|
| `TryParseTimeOptions()` | ✅ Line 284 | ✅ Line 423 | ❌ NOT called | Time filtering |
| `TryParseDisplayOptions()` | ✅ Line 278 | ✅ Line 417 | ✅ Line 378 | Display control |
| Positional arg handling | ❌ None | ✅ Query (410) | ✅ ConversationId (371) | Required input |

### Execution Differences

| Step | list | search | show |
|------|------|--------|------|
| **Validate input** | ❌ Optional | ✅ Query required | ✅ ConversationId required |
| **Find files** | ✅ All files | ✅ All files | ✅ All files |
| **Time filter** | ✅ FilterByDateRange | ✅ FilterByDateRange | ❌ No filtering |
| **Legacy date** | ✅ FilterByDate | ✅ FilterByDate | ❌ No filtering |
| **Count limit** | ✅ Take(N) | ✅ Sort+Take(N) | ❌ No limiting |
| **Match logic** | ❌ N/A | ❌ N/A | ✅ Contains/Exact |
| **Result count** | Multiple | Multiple | Exactly 1 |

---

## Matching Logic Evidence

### Matching Algorithm

**File**: `src/cycodj/CommandLineCommands/ShowCommand.cs`

**Lines 50-52**: Matching predicate
```csharp
50:         var matchingFile = files.FirstOrDefault(f => 
51:             f.Contains(ConversationId) || 
52:             System.IO.Path.GetFileNameWithoutExtension(f) == ConversationId);
```

**Breakdown**:
1. `f.Contains(ConversationId)` - Substring match in full path
2. `||` - Logical OR (either condition matches)
3. `Path.GetFileNameWithoutExtension(f) == ConversationId` - Exact filename match
4. `.FirstOrDefault()` - Returns first match or null

### Match Examples

#### Example 1: Exact Filename Match
```
ConversationId = "conversation-20240115-103045-a1b2c3d4"
File = "C:\Users\...\conversation-20240115-103045-a1b2c3d4.jsonl"

Path.GetFileNameWithoutExtension(file) = "conversation-20240115-103045-a1b2c3d4"
Match: TRUE (exact match)
```

#### Example 2: Substring in Filename
```
ConversationId = "20240115-103045"
File = "C:\Users\...\conversation-20240115-103045-a1b2c3d4.jsonl"

file.Contains("20240115-103045") = TRUE
Match: TRUE (substring match)
```

#### Example 3: Substring in Path
```
ConversationId = "2024"
File = "C:\Users\...\history-2024-01\conversation-20240115-103045-a1b2c3d4.jsonl"

file.Contains("2024") = TRUE (matches in path!)
Match: TRUE (substring match in path)
```

#### Example 4: No Match
```
ConversationId = "nonexistent"
File = "C:\Users\...\conversation-20240115-103045-a1b2c3d4.jsonl"

file.Contains("nonexistent") = FALSE
Path.GetFileNameWithoutExtension(file) == "nonexistent" = FALSE
Match: FALSE
```

### Multiple Match Behavior

**No disambiguation** - If multiple files match, first one is used:

```csharp
50:         var matchingFile = files.FirstOrDefault(f => ...);
```

**Example scenario**:
```
ConversationId = "2024"
Files:
  1. conversation-20240115-103045-a1b2c3d4.jsonl
  2. conversation-20240115-144522-e5f6g7h8.jsonl
  3. conversation-20240116-091234-i9j0k1l2.jsonl

Result: File #1 (first match)
```

---

## Data Flow Summary

### Input Options → Properties → Execution

| CLI Input | Property Set | Used In | Operation |
|-----------|--------------|---------|-----------|
| `cycodj show abc123` | `ConversationId = "abc123"` | Line 41, 51-52 | Validation + Matching |
| (no positional arg) | `ConversationId = ""` | Line 41 | Error: ID required |
| (multiple matches) | N/A | Line 50 | First match selected |
| (no matches) | N/A | Line 54 | Error: not found |

### NO Time Filtering Data Flow

Unlike list/search, there is **NO data flow** for time filtering:

| Time Option | list/search | show |
|-------------|-------------|------|
| `--today` | Sets After/Before | ❌ Option not parsed |
| `--after` | Sets After | ❌ Option not parsed |
| `--before` | Sets Before | ❌ Option not parsed |
| `--date` | Sets Date | ❌ Option not parsed |
| `--last` | Sets Last or After/Before | ❌ Option not parsed |

**Why?**: Show command doesn't call `TryParseTimeOptions()` in its parser method.

---

## Verification Tests

To verify this implementation, run:

```bash
# Test 1: Show with valid full ID
cycodj show conversation-20240115-103045-a1b2c3d4
# Expected: Shows conversation with all messages

# Test 2: Show with partial ID (timestamp)
cycodj show 20240115-103045
# Expected: Shows first conversation matching this timestamp

# Test 3: Show with short ID suffix
cycodj show a1b2c3d4
# Expected: Shows first conversation with this suffix

# Test 4: Show with no ID
cycodj show
# Expected: ERROR: Conversation ID is required
#           Usage: cycodj show <conversation-id>

# Test 5: Show with nonexistent ID
cycodj show nonexistent-12345
# Expected: ERROR: Conversation not found: nonexistent-12345
#           Searched N chat history files

# Test 6: Verify time options don't work
cycodj show abc123 --today
# Expected: ERROR (--today not recognized for show command)

# Test 7: Verify --last doesn't work
cycodj show abc123 --last 5
# Expected: ERROR (--last not recognized for show command)
```

---

## Conclusion

This proof document demonstrates:

1. ✅ **Positional argument parsing** correctly captures ConversationId (Line 371-374)
2. ✅ **No time filtering** - show command doesn't call `TryParseTimeOptions()`
3. ✅ **Simple matching logic** - substring OR exact filename match
4. ✅ **First match wins** - no disambiguation if multiple matches
5. ✅ **Required validation** - fails early if no ID provided
6. ✅ **No count limiting** - always shows exactly 1 conversation

**Key Differences from list/search**:
- NO time filtering options (intentional design)
- NO count limiting (shows exactly 1)
- Requires conversation ID (fails without it)
- Searches ALL files (no filtering before match)
- Uses simple string matching (not time-based)

**Shared Implementation**:
- Uses same `HistoryFileHelpers.FindAllHistoryFiles()`
- Uses same `JsonlReader.ReadConversation()`
- Inherits from same base class (but doesn't use time properties)

**Source Files Analyzed**:
- `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs` (parser - Lines 368-405)
- `src/cycodj/CommandLineCommands/ShowCommand.cs` (execution - Lines 10-67)
- `src/cycodj/Helpers/HistoryFileHelpers.cs` (file finding)

**Total Lines of Evidence**: 80+ lines across 3 files
