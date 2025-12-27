# cycodj branches/stats - Layer 1 Proof: SOURCE CODE EVIDENCE

## Overview

This document provides source code evidence for Layer 1 (TARGET SELECTION) for both `branches` and `stats` commands. Since they share the same implementation pattern (identical to `list`), they're documented together.

---

## branches Command Evidence

### Command Registration

**File**: `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs`

**Lines 43**: branches command registration
```csharp
43:         if (lowerCommandName.StartsWith("branches")) return new BranchesCommand();
```

### Parser Implementation

**Lines 314-366**: Complete option parsing
```csharp
314:     private bool TryParseBranchesCommandOptions(BranchesCommand command, string[] args, ref int i, string arg)
315:     {
316:         // Try common display options first
317:         if (TryParseDisplayOptions(command, args, ref i, arg))
318:         {
319:             return true;
320:         }
321:         
322:         // Try common time options
323:         if (TryParseTimeOptions(command, args, ref i, arg))
324:         {
325:             return true;
326:         }
327:         
328:         if (arg == "--date" || arg == "-d")
329:         {
330:             var date = i + 1 < args.Length ? args[++i] : null;
331:             if (string.IsNullOrWhiteSpace(date))
332:             {
333:                 throw new CommandLineException($"Missing date value for {arg}");
334:             }
335:             command.Date = date;
336:             return true;
337:         }
338:         else if (arg == "--last")
339:         {
340:             var value = i + 1 < args.Length ? args[++i] : null;
341:             if (string.IsNullOrWhiteSpace(value))
342:             {
343:                 throw new CommandLineException($"Missing value for {arg}");
344:             }
345:             
346:             ParseLastValue(command, arg, value);
347:             return true;
348:         }
349:         else if (arg == "--conversation" || arg == "-c")
350:         {
351:             var conv = i + 1 < args.Length ? args[++i] : null;
352:             if (string.IsNullOrWhiteSpace(conv))
353:             {
354:                 throw new CommandLineException($"Missing conversation value for {arg}");
355:             }
356:             command.Conversation = conv;
357:             return true;
358:         }
359:         else if (arg == "--verbose" || arg == "-v")
360:         {
361:             command.Verbose = true;
362:             return true;
363:         }
364:         
365:         return false;
366:     }
```

### Execution Implementation

**File**: `src/cycodj/CommandLineCommands/BranchesCommand.cs`

**Lines 38-111**: Target selection in GenerateBranchesOutput
```csharp
38:     private string GenerateBranchesOutput()
39:     {
40:         var sb = new System.Text.StringBuilder();
41:         
42:         // Find all history files
43:         var files = HistoryFileHelpers.FindAllHistoryFiles();
44:         
45:         if (files.Count == 0)
46:         {
47:             sb.AppendLine("WARNING: No chat history files found");
48:             return sb.ToString();
49:         }
50:         
51:         // Filter by time range if After/Before are set
52:         if (After.HasValue || Before.HasValue)
53:         {
54:             files = HistoryFileHelpers.FilterByDateRange(files, After, Before);
55:             
56:             if (After.HasValue && Before.HasValue)
57:             {
58:                 sb.AppendLine($"Filtered by time range: {After:yyyy-MM-dd HH:mm} to {Before:yyyy-MM-dd HH:mm} ({files.Count} files)");
59:             }
60:             else if (After.HasValue)
61:             {
62:                 sb.AppendLine($"Filtered: after {After:yyyy-MM-dd HH:mm} ({files.Count} files)");
63:             }
64:             else if (Before.HasValue)
65:             {
66:                 sb.AppendLine($"Filtered: before {Before:yyyy-MM-dd HH:mm} ({files.Count} files)");
67:             }
68:             sb.AppendLine();
69:         }
70:         // Filter by date if specified (backward compat)
71:         else if (!string.IsNullOrEmpty(Date))
72:         {
73:             if (DateTime.TryParse(Date, out var dateFilter))
74:             {
75:                 files = HistoryFileHelpers.FilterByDate(files, dateFilter);
76:             }
77:             else
78:             {
79:                 sb.AppendLine($"ERROR: Invalid date format: {Date}");
80:                 return sb.ToString();
81:             }
82:         }
83:         
84:         // Read conversations
85:         var conversations = JsonlReader.ReadConversations(files);
86:         
87:         if (conversations.Count == 0)
88:         {
89:             sb.AppendLine("WARNING: No conversations could be read");
90:             return sb.ToString();
91:         }
92:         
93:         // Apply --last N limit if specified
94:         if (Last > 0)
95:         {
96:             conversations = conversations
97:                 .OrderByDescending(c => c.Timestamp)
98:                 .Take(Last)
99:                 .OrderBy(c => c.Timestamp)
100:                 .ToList();
101:         }
102:         
103:         // Build conversation tree
104:         var tree = BranchDetector.BuildTree(conversations);
105:         
106:         // If specific conversation requested, show just that branch
107:         if (!string.IsNullOrEmpty(Conversation))
108:         {
109:             AppendSingleConversationBranches(sb, tree);
110:             return sb.ToString();
111:         }
```

**Key Differences from list**:
- **Line 94**: Checks `Last > 0` (int, not `Last.HasValue`)
- **NO default limit** (unlike list's 20)
- **Line 107-110**: Special handling for `--conversation` option

---

## stats Command Evidence

### Command Registration

**File**: `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs`

**Lines 45**: stats command registration
```csharp
45:         if (lowerCommandName.StartsWith("stats")) return new StatsCommand();
```

### Parser Implementation

**Lines 483-530**: Complete option parsing
```csharp
483:     private bool TryParseStatsCommandOptions(StatsCommand command, string[] args, ref int i, string arg)
484:     {
485:         // Try common display options first
486:         if (TryParseDisplayOptions(command, args, ref i, arg))
487:         {
488:             return true;
489:         }
490:         
491:         // Try common time options
492:         if (TryParseTimeOptions(command, args, ref i, arg))
493:         {
494:             return true;
495:         }
496:         
497:         if (arg == "--date" || arg == "-d")
498:         {
499:             var date = i + 1 < args.Length ? args[++i] : null;
500:             if (string.IsNullOrWhiteSpace(date))
501:             {
502:                 throw new CommandLineException($"Missing date value for {arg}");
503:             }
504:             command.Date = date;
505:             return true;
506:         }
507:         else if (arg == "--last")
508:         {
509:             var value = i + 1 < args.Length ? args[++i] : null;
510:             if (string.IsNullOrWhiteSpace(value))
511:             {
512:                 throw new CommandLineException($"Missing value for {arg}");
513:             }
514:             
515:             ParseLastValue(command, arg, value);
516:             return true;
517:         }
518:         else if (arg == "--show-tools")
519:         {
520:             command.ShowTools = true;
521:             return true;
522:         }
523:         else if (arg == "--no-dates")
524:         {
525:             command.ShowDates = false;
526:             return true;
527:         }
528:         
529:         return false;
530:     }
```

### Execution Implementation

**File**: `src/cycodj/CommandLineCommands/StatsCommand.cs`

**Lines 34-80**: Target selection in GenerateStatsOutput
```csharp
34:         private string GenerateStatsOutput()
35:         {
36:             var sb = new System.Text.StringBuilder();
37:             
38:             sb.AppendLine("## Chat History Statistics");
39:             sb.AppendLine();
40: 
41:             // Find and parse conversations
42:             var historyDir = CycoDj.Helpers.HistoryFileHelpers.GetHistoryDirectory();
43:             var files = CycoDj.Helpers.HistoryFileHelpers.FindAllHistoryFiles();
44: 
45:             // Filter by time range if After/Before are set
46:             if (After.HasValue || Before.HasValue)
47:             {
48:                 files = CycoDj.Helpers.HistoryFileHelpers.FilterByDateRange(files, After, Before);
49:             }
50:             // Filter by date if specified (backward compat)
51:             else if (!string.IsNullOrWhiteSpace(Date))
52:             {
53:                 if (Date.ToLowerInvariant() == "today")
54:                 {
55:                     files = CycoDj.Helpers.HistoryFileHelpers.FilterByDate(files, DateTime.Today);
56:                 }
57:                 else if (DateTime.TryParse(Date, out var targetDate))
58:                 {
59:                     files = CycoDj.Helpers.HistoryFileHelpers.FilterByDate(files, targetDate);
60:                 }
61:                 else
62:                 {
63:                     sb.AppendLine($"ERROR: Invalid date format: {Date}");
64:                     return sb.ToString();
65:                 }
66:             }
67: 
68:             // Limit number of files if --last specified (as count)
69:             if (Last.HasValue && Last.Value > 0)
70:             {
71:                 files = files.OrderByDescending(f => CycoDj.Helpers.TimestampHelpers.ParseTimestamp(f))
72:                     .Take(Last.Value)
73:                     .ToList();
74:             }
75: 
76:             if (!files.Any())
77:             {
78:                 sb.AppendLine("No conversations found.");
79:                 return sb.ToString();
80:             }
```

**Key Differences from list/branches**:
- **Line 69**: Checks `Last.HasValue` (nullable int, like search)
- **Line 71-73**: Simple sort+take (no re-sort like search)
- **NO default limit**

---

## Comparison Matrix

### Property Types

| Property | list | search | branches | stats |
|----------|------|--------|----------|-------|
| `Date` | `string?` | `string?` | `string?` | `string?` |
| `Last` | `int` (0) | `int?` (null) | `int` (0) | `int?` (null) |
| `After`/`Before` | Inherited | Inherited | Inherited | Inherited |
| Special | - | `Query` | `Conversation` | `ShowTools` |

### Parser Calls

| Method | list | search | branches | stats |
|--------|------|--------|----------|-------|
| `TryParseTimeOptions()` | ✅ Line 284 | ✅ Line 423 | ✅ Line 323 | ✅ Line 492 |
| `ParseLastValue()` | ✅ Line 307 | ✅ Line 446 | ✅ Line 346 | ✅ Line 515 |
| `--date` handling | ✅ Line 289 | ✅ Line 428 | ✅ Line 328 | ✅ Line 497 |
| `--last` handling | ✅ Line 299 | ✅ Line 438 | ✅ Line 338 | ✅ Line 507 |

### Default Behavior

| Command | Default Limit | Check Logic |
|---------|---------------|-------------|
| list | 20 conversations | `if (Last == 0 && no filters)` |
| search | NO limit | (none) |
| branches | NO limit | (none) |
| stats | NO limit | (none) |

### Count Limiting Implementation

**list** (Lines 98-115 in ListCommand.cs):
```csharp
var effectiveLimit = Last;
if (effectiveLimit == 0 && !After.HasValue && !Before.HasValue && string.IsNullOrEmpty(Date))
{
    effectiveLimit = 20; // DEFAULT
}
if (effectiveLimit > 0 && files.Count > effectiveLimit)
{
    files = files.Take(effectiveLimit).ToList();
}
```

**search** (Lines 97-103 in SearchCommand.cs):
```csharp
if (Last.HasValue && Last.Value > 0)
{
    files = files.OrderByDescending(...).Take(Last.Value).OrderBy(...).ToList();
}
```

**branches** (Lines 94-101 in BranchesCommand.cs):
```csharp
if (Last > 0)
{
    conversations = conversations.OrderByDescending(...).Take(Last).OrderBy(...).ToList();
}
```

**stats** (Lines 69-74 in StatsCommand.cs):
```csharp
if (Last.HasValue && Last.Value > 0)
{
    files = files.OrderByDescending(...).Take(Last.Value).ToList();
}
```

---

## Data Flow Summary

### Shared Time Filtering

All four commands use identical time filtering:

| Option | Property Set | Filter Method |
|--------|--------------|---------------|
| `--today` | `After`, `Before` | `FilterByDateRange()` |
| `--yesterday` | `After`, `Before` | `FilterByDateRange()` |
| `--after` | `After` | `FilterByDateRange()` |
| `--before` | `Before` | `FilterByDateRange()` |
| `--date-range` | `After`, `Before` | `FilterByDateRange()` |
| `--date` | `Date` | `FilterByDate()` |
| `--last 7d` | `After`, `Before` | `FilterByDateRange()` |

### Count Limiting Variations

| Command | Property | Type | Check | Sort |
|---------|----------|------|-------|------|
| list | `Last` | `int` | `> 0 \|\| default` | No |
| search | `Last` | `int?` | `.HasValue` | Yes (DESC→ASC) |
| branches | `Last` | `int` | `> 0` | Yes (DESC→ASC) |
| stats | `Last` | `int?` | `.HasValue` | Yes (DESC only) |

---

## Verification Tests

### branches Command

```bash
# Test 1: All conversations
cycodj branches
# Expected: All conversations in tree format

# Test 2: Time filtering
cycodj branches --today
# Expected: Only today's conversations

# Test 3: Count limiting
cycodj branches --last 10
# Expected: Last 10 conversations in tree

# Test 4: Smart timespec
cycodj branches --last 7d
# Expected: Last 7 days of conversations

# Test 5: Specific conversation
cycodj branches --conversation abc123
# Expected: Branch tree for conversation abc123
```

### stats Command

```bash
# Test 1: All statistics
cycodj stats
# Expected: Statistics for all conversations

# Test 2: Time filtering
cycodj stats --today
# Expected: Today's statistics

# Test 3: Count limiting
cycodj stats --last 100
# Expected: Statistics for last 100 conversations

# Test 4: Smart timespec
cycodj stats --last 30d
# Expected: Last 30 days statistics

# Test 5: With tool details
cycodj stats --today --show-tools
# Expected: Today's stats + tool usage breakdown
```

---

## Conclusion

### branches Command

✅ **Identical to list** except:
- NO default 20-conversation limit
- Adds `--conversation` option for subtree view
- Adds `--verbose` option for detailed branch info

### stats Command

✅ **Identical to list** except:
- NO default 20-conversation limit
- `Last` is nullable int (like search)
- Adds `--show-tools` for tool statistics
- Adds `--no-dates` to suppress date breakdown

### Shared Implementation

Both commands use:
- ✅ Same `TryParseTimeOptions()` method
- ✅ Same `ParseLastValue()` with smart detection
- ✅ Same `TimeSpecHelpers` for parsing
- ✅ Same `HistoryFileHelpers` for filtering
- ✅ Same time-range and legacy date filtering logic

**Source Files Analyzed**:
- `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs` (parser)
- `src/cycodj/CommandLineCommands/BranchesCommand.cs` (execution)
- `src/cycodj/CommandLineCommands/StatsCommand.cs` (execution)

**Total Lines of Evidence**: 150+ lines across 3 files
