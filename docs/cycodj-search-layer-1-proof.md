# cycodj search - Layer 1 Proof: SOURCE CODE EVIDENCE

## Overview

This document provides complete source code evidence for Layer 1 (TARGET SELECTION) implementation in the `search` command, with exact line numbers and code excerpts.

---

## Parser Evidence

### Command Registration

**File**: `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs`

**Lines 37-48**: Command factory recognizes "search"
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

### SearchCommand Option Parsing

**File**: `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs`

**Lines 407-481**: SearchCommand option parsing
```csharp
407:     private bool TryParseSearchCommandOptions(SearchCommand command, string[] args, ref int i, string arg)
408:     {
409:         // First positional argument is the search query
410:         if (!arg.StartsWith("-") && string.IsNullOrEmpty(command.Query))
411:         {
412:             command.Query = arg;
413:             return true;
414:         }
415:         
416:         // Try common display options first
417:         if (TryParseDisplayOptions(command, args, ref i, arg))
418:         {
419:             return true;
420:         }
421:         
422:         // Try common time options
423:         if (TryParseTimeOptions(command, args, ref i, arg))
424:         {
425:             return true;
426:         }
427:         
428:         if (arg == "--date" || arg == "-d")
429:         {
430:             var date = i + 1 < args.Length ? args[++i] : null;
431:             if (string.IsNullOrWhiteSpace(date))
432:             {
433:                 throw new CommandLineException($"Missing date value for {arg}");
434:             }
435:             command.Date = date;
436:             return true;
437:         }
438:         else if (arg == "--last")
439:         {
440:             var value = i + 1 < args.Length ? args[++i] : null;
441:             if (string.IsNullOrWhiteSpace(value))
442:             {
443:                 throw new CommandLineException($"Missing value for {arg}");
444:             }
445:             
446:             ParseLastValue(command, arg, value);
447:             return true;
448:         }
449:         else if (arg == "--case-sensitive" || arg == "-c")
450:         {
451:             command.CaseSensitive = true;
452:             return true;
453:         }
454:         else if (arg == "--regex" || arg == "-r")
455:         {
456:             command.UseRegex = true;
457:             return true;
458:         }
459:         else if (arg == "--user-only" || arg == "-u")
460:         {
461:             command.UserOnly = true;
462:             return true;
463:         }
464:         else if (arg == "--assistant-only" || arg == "-a")
465:         {
466:             command.AssistantOnly = true;
467:             return true;
468:         }
469:         else if (arg == "--context" || arg == "-C")
470:         {
471:             var lines = i + 1 < args.Length ? args[++i] : null;
472:             if (string.IsNullOrWhiteSpace(lines) || !int.TryParse(lines, out var n))
473:             {
474:                 throw new CommandLineException($"Missing or invalid context lines for {arg}");
475:             }
476:             command.ContextLines = n;
477:             return true;
478:         }
479:         
480:         return false;
481:     }
```

**Key Lines**:
- **Line 410-413**: Positional argument parsing for query (first non-option arg)
- **Line 423**: Calls `TryParseTimeOptions()` for time filtering (same as list)
- **Line 428-437**: `--date` option parsing (legacy)
- **Line 438-448**: `--last` option parsing with smart detection
- **Line 449-478**: Content filtering options (Layer 3, not Layer 1)

### Time Filtering Options (Shared)

**File**: `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs`

**Lines 216-273**: Common time option parsing (same code as list)
```csharp
216:     private bool TryParseTimeOptions(CycoDjCommand command, string[] args, ref int i, string arg)
217:     {
218:         // --today shortcut (calendar day)
219:         if (arg == "--today")
220:         {
221:             command.After = DateTime.Today;
222:             command.Before = DateTime.Now;
223:             return true;
224:         }
225:         
226:         // --yesterday shortcut (calendar day)
227:         else if (arg == "--yesterday")
228:         {
229:             command.After = DateTime.Today.AddDays(-1);
230:             command.Before = DateTime.Today.AddTicks(-1);
231:             return true;
232:         }
233:         
234:         // --after <timespec>
235:         else if (arg == "--after" || arg == "--time-after")
236:         {
237:             var timeSpec = i + 1 < args.Length ? args[++i] : null;
238:             if (string.IsNullOrWhiteSpace(timeSpec))
239:             {
240:                 throw new CommandLineException($"Missing timespec value for {arg}");
241:             }
242:             command.After = TimeSpecHelpers.ParseSingleTimeSpec(arg, timeSpec, isAfter: true);
243:             return true;
244:         }
245:         
246:         // --before <timespec>
247:         else if (arg == "--before" || arg == "--time-before")
248:         {
249:             var timeSpec = i + 1 < args.Length ? args[++i] : null;
250:             if (string.IsNullOrWhiteSpace(timeSpec))
251:             {
252:                 throw new CommandLineException($"Missing timespec value for {arg}");
253:             }
254:             command.Before = TimeSpecHelpers.ParseSingleTimeSpec(arg, timeSpec, isAfter: false);
255:             return true;
256:         }
257:         
258:         // --date-range <range> or --time-range <range>
259:         else if (arg == "--date-range" || arg == "--time-range")
260:         {
261:             var timeSpec = i + 1 < args.Length ? args[++i] : null;
262:             if (string.IsNullOrWhiteSpace(timeSpec))
263:             {
264:                 throw new CommandLineException($"Missing timespec range for {arg}");
265:             }
266:             var (after, before) = TimeSpecHelpers.ParseTimeSpecRange(arg, timeSpec);
267:             command.After = after;
268:             command.Before = before;
269:             return true;
270:         }
271:         
272:         return false;
273:     }
```

### Smart Detection (Shared)

**Lines 554-616**: Same smart detection code as list command
- `ParseLastValue()` method [Lines 554-594]
- `IsTimeSpec()` method [Lines 599-616]

See [cycodj-list-layer-1-proof.md](cycodj-list-layer-1-proof.md) for complete smart detection evidence.

---

## Execution Evidence

### SearchCommand Class Definition

**File**: `src/cycodj/CommandLineCommands/SearchCommand.cs`

**Lines 8-20**: Properties for target selection
```csharp
8:     public class SearchCommand : CommandLine.CycoDjCommand
9:     {
10:         public string? Query { get; set; }
11:         public string? Date { get; set; }
12:         public int? Last { get; set; }
13:         public bool CaseSensitive { get; set; }
14:         public bool UseRegex { get; set; }
15:         public bool UserOnly { get; set; }
16:         public bool AssistantOnly { get; set; }
17:         public int ContextLines { get; set; } = 2;
18:         public bool ShowBranches { get; set; } = false;
19:         public int? MessageCount { get; set; } = null; // null = use default (3)
20:         public bool ShowStats { get; set; } = false;
```

**Key Properties for Layer 1**:
- **Line 10**: `Query` - Required search query (positional arg)
- **Line 11**: `Date` - Legacy date filter
- **Line 12**: `Last` - Count limit (int? vs list's int)
- **Inherited**: `After`, `Before` from `CycoDjCommand` base class

### Target Selection Implementation

**Lines 42-109**: GenerateSearchOutput - Target selection logic
```csharp
42:         private string GenerateSearchOutput()
43:         {
44:             var sb = new System.Text.StringBuilder();
45:             
46:             if (string.IsNullOrWhiteSpace(Query))
47:             {
48:                 sb.AppendLine("ERROR: Search query is required.");
49:                 return sb.ToString();
50:             }
51: 
52:             sb.AppendLine($"## Searching conversations for: \"{Query}\"");
53:             sb.AppendLine();
54: 
55:             // Find and parse conversations
56:             var historyDir = CycoDj.Helpers.HistoryFileHelpers.GetHistoryDirectory();
57:             var files = CycoDj.Helpers.HistoryFileHelpers.FindAllHistoryFiles();
58:             
59:             // Filter by time range if After/Before are set
60:             if (After.HasValue || Before.HasValue)
61:             {
62:                 files = CycoDj.Helpers.HistoryFileHelpers.FilterByDateRange(files, After, Before);
63:                 
64:                 if (After.HasValue && Before.HasValue)
65:                 {
66:                     sb.AppendLine($"Filtered by time range: {After:yyyy-MM-dd HH:mm} to {Before:yyyy-MM-dd HH:mm}");
67:                 }
68:                 else if (After.HasValue)
69:                 {
70:                     sb.AppendLine($"Filtered: after {After:yyyy-MM-dd HH:mm}");
71:                 }
72:                 else if (Before.HasValue)
73:                 {
74:                     sb.AppendLine($"Filtered: before {Before:yyyy-MM-dd HH:mm}");
75:                 }
76:                 sb.AppendLine();
77:             }
78:             // Filter by date if specified (backward compat)
79:             else if (!string.IsNullOrWhiteSpace(Date))
80:             {
81:                 if (Date.ToLowerInvariant() == "today")
82:                 {
83:                     files = CycoDj.Helpers.HistoryFileHelpers.FilterByDate(files, DateTime.Today);
84:                 }
85:                 else if (DateTime.TryParse(Date, out var targetDate))
86:                 {
87:                     files = CycoDj.Helpers.HistoryFileHelpers.FilterByDate(files, targetDate);
88:                 }
89:                 else
90:                 {
91:                     sb.AppendLine($"ERROR: Invalid date format: {Date}");
92:                     return sb.ToString();
93:                 }
94:             }
95: 
96:             // Limit number of files if --last specified (as count)
97:             if (Last.HasValue && Last.Value > 0)
98:             {
99:                 files = files.OrderByDescending(f => CycoDj.Helpers.TimestampHelpers.ParseTimestamp(f))
100:                     .Take(Last.Value)
101:                     .OrderBy(f => CycoDj.Helpers.TimestampHelpers.ParseTimestamp(f))
102:                     .ToList();
103:             }
104: 
105:             if (!files.Any())
106:             {
107:                 sb.AppendLine("No conversations found matching the criteria.");
108:                 return sb.ToString();
109:             }
```

**Key Differences from list**:
- **Line 46-50**: Query validation (REQUIRED for search)
- **Line 97**: `Last.HasValue` check (nullable int, vs list's int with default 0)
- **Line 99-102**: Sort by timestamp DESC, take N, then sort ASC (ensures chronological order)
- **NO default limit**: If no filters, searches ALL conversations

### Execution Flow Diagram

```
ExecuteAsync() [Line 23]
    ↓
GenerateSearchOutput() [Line 42]
    ↓
Step 1: Validate query [Lines 46-50]
    IF Query is null or empty
        ERROR: "Search query is required."
        RETURN early
    ↓
Step 2: Find all files [Line 57]
    files = HistoryFileHelpers.FindAllHistoryFiles()
    ↓
Step 3: Apply time-range filter (if After/Before set) [Lines 60-77]
    IF (After.HasValue OR Before.HasValue)
        files = HistoryFileHelpers.FilterByDateRange(files, After, Before)
    ↓
Step 4: Apply legacy date filter (else if) [Lines 79-94]
    ELSE IF (Date is not empty)
        files = HistoryFileHelpers.FilterByDate(files, dateFilter)
    ↓
Step 5: Apply count limit (if specified) [Lines 97-103]
    IF (Last.HasValue AND Last.Value > 0)
        files = Sort DESC → Take(Last) → Sort ASC
    ↓
Step 6: Search each file [Lines 114-132]
    FOR EACH file in files
        conversation = Read conversation
        matches = SearchConversation(conversation)  // Layer 3
```

---

## Comparison: list vs search

### Property Type Differences

| Property | list Command | search Command | Reason |
|----------|--------------|----------------|--------|
| `Query` | ❌ Not present | ✅ `string?` | Search requires query |
| `Date` | ✅ `string?` | ✅ `string?` | Same (legacy filter) |
| `Last` | ✅ `int` (default: 0) | ✅ `int?` (nullable) | list has default limit |
| `After`/`Before` | ✅ Inherited | ✅ Inherited | Same (from base) |

### Default Behavior Differences

| Aspect | list | search |
|--------|------|--------|
| **No filters** | Last 20 conversations | ALL conversations |
| **Required arg** | None | Query required |
| **Performance** | Fast (default 20) | Potentially slow |
| **Default limit code** | Lines 98-104 in ListCommand | NOT present in SearchCommand |

**list default limit code** (NOT in search):
```csharp
98:         var effectiveLimit = Last;
99:         if (effectiveLimit == 0 && !After.HasValue && !Before.HasValue && string.IsNullOrEmpty(Date))
100:         {
101:             effectiveLimit = 20; // Default to last 20 conversations
102:             sb.AppendLine($"Showing last {effectiveLimit} conversations (use --last N to change, or --date to filter)");
103:             sb.AppendLine();
104:         }
```

**search has NO equivalent** - searches all conversations by default.

### Count Limiting Differences

**list** (Line 107 in ListCommand.cs):
```csharp
107:         if (effectiveLimit > 0 && files.Count > effectiveLimit)
108:         {
109:             files = files.Take(effectiveLimit).ToList();
```
- Uses `effectiveLimit` (which includes default 20)
- Simple `.Take()`

**search** (Lines 97-102 in SearchCommand.cs):
```csharp
97:             if (Last.HasValue && Last.Value > 0)
98:             {
99:                 files = files.OrderByDescending(f => CycoDj.Helpers.TimestampHelpers.ParseTimestamp(f))
100:                     .Take(Last.Value)
101:                     .OrderBy(f => CycoDj.Helpers.TimestampHelpers.ParseTimestamp(f))
102:                     .ToList();
103:             }
```
- Checks `Last.HasValue` (nullable)
- Sorts DESC → Take → Sort ASC (to maintain chronological order)
- NO default limiting

---

## Data Flow Summary

### Input Options → Properties → Filtering

| CLI Option | Property Set | Used In | Filter Method | Same as list? |
|------------|--------------|---------|---------------|---------------|
| `query` (positional) | `Query` | Line 46 | Validation | ❌ Unique to search |
| `--today` | `After`, `Before` | Line 60 | `FilterByDateRange()` | ✅ Same |
| `--yesterday` | `After`, `Before` | Line 60 | `FilterByDateRange()` | ✅ Same |
| `--after <time>` | `After` | Line 60 | `FilterByDateRange()` | ✅ Same |
| `--before <time>` | `Before` | Line 60 | `FilterByDateRange()` | ✅ Same |
| `--date-range <range>` | `After`, `Before` | Line 60 | `FilterByDateRange()` | ✅ Same |
| `--date <date>` | `Date` | Line 79 | `FilterByDate()` | ✅ Same |
| `--last 10` (count) | `Last` | Line 97 | Sort + `.Take()` | ⚠️ Different (no default) |
| `--last 7d` (timespec) | `After`, `Before` | Line 60 | `FilterByDateRange()` | ✅ Same |
| (no options) | (none) | N/A | Search ALL | ❌ Different (list=20) |

---

## Verification Tests

To verify this implementation, run:

```bash
# Test 1: Query required
cycodj search
# Expected: ERROR: Search query is required.

# Test 2: Search all conversations
cycodj search "ConPTY"
# Expected: Searches ALL conversation files

# Test 3: Search today's conversations
cycodj search "error" --today
# Expected: Only today's conversations searched

# Test 4: Search last 10 conversations
cycodj search "async" --last 10
# Expected: Only 10 most recent conversations searched

# Test 5: Search last 7 days (timespec)
cycodj search "bug" --last 7d
# Expected: Conversations from last 7 days searched

# Test 6: Search specific date range
cycodj search "feature" --date-range 2024-01-01..2024-01-31
# Expected: Only January 2024 conversations searched

# Test 7: Legacy date filter
cycodj search "deployment" --date 2024-01-15
# Expected: Only 2024-01-15 conversations searched

# Test 8: Combining filters
cycodj search "timeout" --after 2024-01-01 --before 2024-02-01
# Expected: Only January 2024 conversations searched
```

---

## Conclusion

This proof document demonstrates:

1. ✅ **Parser correctly handles** positional query argument (Line 410-413)
2. ✅ **Time filtering identical to list** (uses same `TryParseTimeOptions()`)
3. ✅ **Smart detection identical to list** (uses same `ParseLastValue()` and `IsTimeSpec()`)
4. ✅ **Query validation required** (Lines 46-50)
5. ✅ **NO default limiting** (unlike list which defaults to 20)
6. ✅ **Count limiting uses sort** (DESC → Take → ASC to maintain order)

**Key Differences from list**:
- Requires query argument (positional)
- NO default conversation limit
- Count limiting includes sorting step
- `Last` is nullable int (vs list's int)

**Shared Implementation**:
- Time-range filtering options (identical)
- Legacy date filtering (identical)
- Smart detection for `--last` (identical)
- Helper classes (identical)

**Source Files Analyzed**:
- `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs` (parser - Lines 407-481)
- `src/cycodj/CommandLineCommands/SearchCommand.cs` (execution - Lines 8-109)
- `src/cycodj/CommandLine/CycoDjCommand.cs` (base class)
- `src/cycodj/Helpers/HistoryFileHelpers.cs` (filtering)
- `src/common/Helpers/TimeSpecHelpers.cs` (parsing)

**Total Lines of Evidence**: 120+ lines across 5 files
