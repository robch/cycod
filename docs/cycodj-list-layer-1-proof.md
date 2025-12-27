# cycodj list - Layer 1 Proof: SOURCE CODE EVIDENCE

## Overview

This document provides complete source code evidence for Layer 1 (TARGET SELECTION) implementation in the `list` command, with exact line numbers and code excerpts.

---

## Parser Evidence

### Command Registration

**File**: `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs`

**Lines 14-17**: Default command is `ListCommand`
```csharp
14:     override protected Command? NewDefaultCommand()
15:     {
16:         // Default to list command if no command specified
17:         return new ListCommand();
18:     }
```

**Lines 37-48**: Command factory recognizes "list"
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

### Time-Range Filtering Options (Modern)

**File**: `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs`

**Lines 275-311**: ListCommand option parsing
```csharp
275:     private bool TryParseListCommandOptions(ListCommand command, string[] args, ref int i, string arg)
276:     {
277:         // Try common display options first
278:         if (TryParseDisplayOptions(command, args, ref i, arg))
279:         {
280:             return true;
281:         }
282:         
283:         // Try common time options
284:         if (TryParseTimeOptions(command, args, ref i, arg))
285:         {
286:             return true;
287:         }
288:         
289:         if (arg == "--date" || arg == "-d")
290:         {
291:             var date = i + 1 < args.Length ? args[++i] : null;
292:             if (string.IsNullOrWhiteSpace(date))
293:             {
294:                 throw new CommandLineException($"Missing date value for {arg}");
295:             }
296:             command.Date = date;
297:             return true;
298:         }
299:         else if (arg == "--last")
300:         {
301:             var value = i + 1 < args.Length ? args[++i] : null;
302:             if (string.IsNullOrWhiteSpace(value))
303:             {
304:                 throw new CommandLineException($"Missing value for {arg}");
305:             }
306:             
307:             ParseLastValue(command, arg, value);
308:             return true;
309:         }
310:         
311:         return false;
312:     }
```

**Lines 216-273**: Common time option parsing
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

### Smart Detection for --last

**Lines 554-594**: ParseLastValue method
```csharp
554:     private void ParseLastValue(CycoDjCommand command, string arg, string value)
555:     {
556:         // Smart detection: TIMESPEC vs conversation count
557:         if (IsTimeSpec(value))
558:         {
559:             // Parse as TIMESPEC
560:             try
561:             {
562:                 // For --last context, relative times should go BACKWARD (ago)
563:                 // If value is like "7d", convert to "-7d.." (7 days ago to now)
564:                 var timeSpec = value;
565:                 if (System.Text.RegularExpressions.Regex.IsMatch(value, @"^\d+[dhms]", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
566:                 {
567:                     timeSpec = "-" + value + ".."; // Make it a range from N ago to now
568:                 }
569:                 
570:                 var (after, before) = TimeSpecHelpers.ParseTimeSpecRange(arg, timeSpec);
571:                 command.After = after;
572:                 command.Before = before;
573:             }
574:             catch (Exception ex)
575:             {
576:                 throw new CommandLineException($"Invalid time specification for --last: {value}\n{ex.Message}");
577:             }
578:         }
579:         else
580:         {
581:             // Parse as conversation count (for ListCommand, SearchCommand, etc.)
582:             if (!int.TryParse(value, out var count))
583:             {
584:                 throw new CommandLineException($"Invalid number for --last: {value}");
585:             }
586:             
587:             // Set Last property if it exists on the command
588:             var lastProp = command.GetType().GetProperty("Last");
589:             if (lastProp != null)
590:             {
591:                 lastProp.SetValue(command, count);
592:             }
593:         }
594:     }
```

**Lines 599-616**: IsTimeSpec detection logic
```csharp
599:     private static bool IsTimeSpec(string value)
600:     {
601:         if (string.IsNullOrWhiteSpace(value)) return false;
602:         
603:         // Has range syntax?
604:         if (value.Contains("..")) return true;
605:         
606:         // Is keyword?
607:         if (value.Equals("today", StringComparison.OrdinalIgnoreCase)) return true;
608:         if (value.Equals("yesterday", StringComparison.OrdinalIgnoreCase)) return true;
609:         
610:         // Has time units (d, h, m, s)?
611:         if (System.Text.RegularExpressions.Regex.IsMatch(value, @"[dhms]", System.Text.RegularExpressions.RegexOptions.IgnoreCase)) 
612:             return true;
613:         
614:         // Pure number = conversation count
615:         return false;
616:     }
```

---

## Execution Evidence

### ListCommand Class Definition

**File**: `src/cycodj/CommandLineCommands/ListCommand.cs`

**Lines 10-16**: Properties for target selection
```csharp
10: public class ListCommand : CycoDjCommand
11: {
12:     public string? Date { get; set; }
13:     public int Last { get; set; } = 0;
14:     public bool ShowBranches { get; set; } = false;
15:     public int? MessageCount { get; set; } = null; // null = use default (3)
16:     public bool ShowStats { get; set; } = false;
```

**Note**: `After` and `Before` properties are inherited from `CycoDjCommand` base class.

### Target Selection Implementation

**Lines 44-116**: GenerateListOutput - Target selection logic
```csharp
44:     private string GenerateListOutput()
45:     {
46:         var sb = new System.Text.StringBuilder();
47:         
48:         sb.AppendLine("## Chat History Conversations");
49:         sb.AppendLine();
50:         
51:         // Find all history files
52:         var files = HistoryFileHelpers.FindAllHistoryFiles();
53:         
54:         if (files.Count == 0)
55:         {
56:             sb.AppendLine("WARNING: No chat history files found");
57:             var historyDir = HistoryFileHelpers.GetHistoryDirectory();
58:             sb.AppendLine($"Expected location: {historyDir}");
59:             return sb.ToString();
60:         }
61:         
62:         // Filter by time range if After/Before are set (from --today, --yesterday, --last <timespec>, etc.)
63:         if (After.HasValue || Before.HasValue)
64:         {
65:             files = HistoryFileHelpers.FilterByDateRange(files, After, Before);
66:             
67:             if (After.HasValue && Before.HasValue)
68:             {
69:                 sb.AppendLine($"Filtered by time range: {After:yyyy-MM-dd HH:mm} to {Before:yyyy-MM-dd HH:mm} ({files.Count} files)");
70:             }
71:             else if (After.HasValue)
72:             {
73:                 sb.AppendLine($"Filtered: after {After:yyyy-MM-dd HH:mm} ({files.Count} files)");
74:             }
75:             else if (Before.HasValue)
76:             {
77:                 sb.AppendLine($"Filtered: before {Before:yyyy-MM-dd HH:mm} ({files.Count} files)");
78:             }
79:             sb.AppendLine();
80:         }
81:         // Filter by date if specified (backward compatibility)
82:         else if (!string.IsNullOrEmpty(Date))
83:         {
84:             if (DateTime.TryParse(Date, out var dateFilter))
85:             {
86:                 files = HistoryFileHelpers.FilterByDate(files, dateFilter);
87:                 sb.AppendLine($"Filtered by date: {dateFilter:yyyy-MM-dd} ({files.Count} files)");
88:                 sb.AppendLine();
89:             }
90:             else
91:             {
92:                 sb.AppendLine($"ERROR: Invalid date format: {Date}");
93:                 return sb.ToString();
94:             }
95:         }
96:         
97:         // Apply sensible default limit if not specified and no filters
98:         var effectiveLimit = Last;
99:         if (effectiveLimit == 0 && !After.HasValue && !Before.HasValue && string.IsNullOrEmpty(Date))
100:         {
101:             effectiveLimit = 20; // Default to last 20 conversations
102:             sb.AppendLine($"Showing last {effectiveLimit} conversations (use --last N to change, or --date to filter)");
103:             sb.AppendLine();
104:         }
105:         
106:         // Limit to last N if specified or defaulted
107:         if (effectiveLimit > 0 && files.Count > effectiveLimit)
108:         {
109:             files = files.Take(effectiveLimit).ToList();
110:             if (Last > 0)
111:             {
112:                 sb.AppendLine($"Showing last {effectiveLimit} conversations");
113:                 sb.AppendLine();
114:             }
115:         }
116:         
```

### Execution Flow Diagram

```
ExecuteAsync() [Line 25]
    ↓
GenerateListOutput() [Line 44]
    ↓
Step 1: Find all files [Line 52]
    files = HistoryFileHelpers.FindAllHistoryFiles()
    ↓
Step 2: Apply time-range filter (if After/Before set) [Lines 63-80]
    IF (After.HasValue OR Before.HasValue)
        files = HistoryFileHelpers.FilterByDateRange(files, After, Before)
    ↓
Step 3: Apply legacy date filter (else if) [Lines 82-95]
    ELSE IF (Date is not empty)
        files = HistoryFileHelpers.FilterByDate(files, dateFilter)
    ↓
Step 4: Apply default limit (if no filters) [Lines 98-104]
    IF (Last == 0 AND no time/date filters)
        effectiveLimit = 20
    ↓
Step 5: Apply count limit [Lines 107-115]
    IF (effectiveLimit > 0 AND files.Count > effectiveLimit)
        files = files.Take(effectiveLimit)
```

---

## Base Class Evidence

### CycoDjCommand Base Class

**File**: `src/cycodj/CommandLine/CycoDjCommand.cs`

Properties for time filtering (inherited by all commands):
```csharp
public DateTime? After { get; set; }
public DateTime? Before { get; set; }
```

These properties are set by:
- `--today` → Sets After and Before [Lines 221-222]
- `--yesterday` → Sets After and Before [Lines 229-230]
- `--after`, `--time-after` → Sets After [Line 242]
- `--before`, `--time-before` → Sets Before [Line 254]
- `--date-range`, `--time-range` → Sets both After and Before [Lines 267-268]
- `--last <timespec>` → Sets both After and Before [Lines 571-572]

---

## Helper Class Evidence

### HistoryFileHelpers

**Purpose**: Manages finding and filtering conversation history files

Key methods used by ListCommand:
- `FindAllHistoryFiles()` - Returns all .jsonl files in history directory
- `FilterByDateRange(files, after, before)` - Filters files by time range
- `FilterByDate(files, date)` - Filters files by specific date (legacy)

**File Location**: `src/cycodj/Helpers/HistoryFileHelpers.cs`

### TimeSpecHelpers

**Purpose**: Parses time specifications into DateTime values

Key methods used:
- `ParseSingleTimeSpec(arg, timeSpec, isAfter)` - Parses single timespec
- `ParseTimeSpecRange(arg, timeSpec)` - Parses timespec range (returns tuple)

Supported formats:
- Absolute: `2024-01-15`, `2024-01-15T10:30:00`
- Relative: `7d`, `-7d`, `2h`, `30m`
- Range: `7d..`, `..yesterday`, `2024-01-01..2024-01-31`
- Keywords: `today`, `yesterday`

**File Location**: `src/common/Helpers/TimeSpecHelpers.cs`

### TimestampHelpers

**Purpose**: Extracts timestamp from filename

Key method:
- `ParseTimestamp(filename)` - Extracts DateTime from conversation filename

**File Location**: `src/cycodj/Helpers/TimestampHelpers.cs`

---

## Data Flow Summary

### Input Options → Properties → Filtering

| CLI Option | Property Set | Used In | Filter Method |
|------------|--------------|---------|---------------|
| `--today` | `After`, `Before` | Line 63 | `FilterByDateRange()` |
| `--yesterday` | `After`, `Before` | Line 63 | `FilterByDateRange()` |
| `--after <time>` | `After` | Line 63 | `FilterByDateRange()` |
| `--before <time>` | `Before` | Line 63 | `FilterByDateRange()` |
| `--date-range <range>` | `After`, `Before` | Line 63 | `FilterByDateRange()` |
| `--date <date>` | `Date` | Line 82 | `FilterByDate()` |
| `--last 10` (count) | `Last` | Line 107 | `.Take()` |
| `--last 7d` (timespec) | `After`, `Before` | Line 63 | `FilterByDateRange()` |
| (no options) | `effectiveLimit = 20` | Line 107 | `.Take()` |

---

## Verification Tests

To verify this implementation, run:

```bash
# Test 1: Default behavior (20 conversations)
cycodj list

# Test 2: Today's conversations
cycodj list --today

# Test 3: Yesterday's conversations
cycodj list --yesterday

# Test 4: Last 5 conversations (count)
cycodj list --last 5

# Test 5: Last 7 days (timespec)
cycodj list --last 7d

# Test 6: Specific date range
cycodj list --after 2024-01-01 --before 2024-01-31

# Test 7: Concise range
cycodj list --date-range 2024-01-01..2024-01-31

# Test 8: Legacy date filter
cycodj list --date 2024-01-15

# Test 9: Combining filters
cycodj list --after 2024-01-01 --last 10
```

---

## Conclusion

This proof document demonstrates:

1. ✅ **Parser correctly recognizes** all time-filtering options
2. ✅ **Smart detection works** for `--last` (timespec vs count)
3. ✅ **Priority/precedence is correct**: time-range → legacy date → default limit → count limit
4. ✅ **Default behavior** shows 20 conversations when no filters
5. ✅ **All options are wired** from CLI to execution correctly

**Source Files Analyzed**:
- `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs` (parser)
- `src/cycodj/CommandLineCommands/ListCommand.cs` (execution)
- `src/cycodj/CommandLine/CycoDjCommand.cs` (base class)
- `src/cycodj/Helpers/HistoryFileHelpers.cs` (filtering)
- `src/common/Helpers/TimeSpecHelpers.cs` (parsing)

**Total Lines of Evidence**: 150+ lines across 5 files
