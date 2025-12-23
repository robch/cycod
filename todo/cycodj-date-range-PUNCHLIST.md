# Punch List: Date/Time/Range Support for cycodj

## Summary

**Goal:** Add cycodmd-style TIMESPEC support to cycodj for consistency across cycod tools.

**Good news:** The TIMESPEC parsing code already exists in `src/common/Helpers/TimeSpecHelpers.cs` (shared code!). We just need to wire it up in cycodj.

**No changes needed to cycodmd** - we're copying its pattern, not modifying it.

---

## The Plan: Match What's in cycodmd

### What cycodmd Has (That We Want)

**Options:**
- `--modified <timespec>` - Match files modified in period
- `--after <timespec>` (alias for `--modified-after`)
- `--before <timespec>` (alias for `--modified-before`)
- `--modified-after <timespec>` - After specific time
- `--modified-before <timespec>` - Before specific time
- `--created <timespec>` - Match created in period
- `--created-after <timespec>`
- `--created-before <timespec>`
- `--accessed <timespec>` - Match accessed in period
- `--accessed-after <timespec>`
- `--accessed-before <timespec>`
- `--anytime <timespec>` - Match ANY time attribute
- `--anytime-after <timespec>`
- `--anytime-before <timespec>`

**TIMESPEC Formats:**
- Absolute: "2023-09-01", "September 1, 2023"
- Relative: "3d", "4h", "5m"
- Combined: "2d4h3m"
- Keywords: "today", "yesterday"
- Ranges: "2023-01-01..2023-12-31", "3d..", "..yesterday"

### What cycodj Currently Has

**Options:**
- `--date <date>` - Single date or "today"
- `--last <n>` - Last N conversations (count, not time)
- `--last-days <n>` - Only in JournalCommand

**Properties used:**
- `Date` (string)
- `Last` (int)
- `LastDays` (int, journal only)

---

## Punch List

### 1. ✅ Verify TimeSpecHelpers is accessible
**Location:** `src/common/Helpers/TimeSpecHelpers.cs`  
**Status:** EXISTS! Already shared code.  
**Action:** None needed - already available to cycodj.

---

### 2. Update cycodj Command Base Classes

**File:** `src/cycodj/CommandLine/CycoDjCommand.cs`

**Add properties for time filtering:**
```csharp
// Date/time filtering (like cycodmd)
public DateTime? After { get; set; }
public DateTime? Before { get; set; }
public DateTime? CreatedAfter { get; set; }
public DateTime? CreatedBefore { get; set; }
```

**Note:** For conversations, we probably only need:
- Modified time → Use conversation timestamp
- No separate "created" vs "accessed" (conversations created once)
- So we can simplify to just `After` and `Before`

---

### 3. Update Command Line Options Parser

**File:** `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs`

**Add option parsing (copy pattern from cycodmd):**
```csharp
// In ProcessOptions method:
else if (arg == "--after" || arg == "--time-after")
{
    var timeSpec = GetNextArg(i++, args);
    command.After = TimeSpecHelpers.ParseSingleTimeSpec(arg, timeSpec, isAfter: true);
}
else if (arg == "--before" || arg == "--time-before")
{
    var timeSpec = GetNextArg(i++, args);
    command.Before = TimeSpecHelpers.ParseSingleTimeSpec(arg, timeSpec, isAfter: false);
}
else if (arg == "--date-range" || arg == "--time-range")
{
    var timeSpec = GetNextArg(i++, args);
    var (after, before) = TimeSpecHelpers.ParseTimeSpecRange(arg, timeSpec);
    command.After = after;
    command.Before = before;
}
```

**Also support existing --date with TIMESPEC:**
```csharp
else if (arg == "--date" || arg == "--time")
{
    var timeSpec = GetNextArg(i++, args);
    // If it's a range, parse as range
    if (timeSpec.Contains(".."))
    {
        var (after, before) = TimeSpecHelpers.ParseTimeSpecRange(arg, timeSpec);
        command.After = after;
        command.Before = before;
    }
    else
    {
        // Single date/time - represents a period
        var (after, before) = TimeSpecHelpers.ParseTimeSpecRange(arg, timeSpec);
        command.After = after;
        command.Before = before;
    }
}
```

**Keep backward compatibility:**
```csharp
// Keep --last-days for journal command (already exists)
else if (arg == "--last-days")
{
    var daysStr = GetNextArg(i++, args);
    if (command is JournalCommand journalCmd)
    {
        journalCmd.LastDays = int.Parse(daysStr);
    }
}
```

---

### 4. Update HistoryFileHelpers

**File:** `src/cycodj/Helpers/HistoryFileHelpers.cs`

**Current method:**
```csharp
public static List<string> FilterByDate(List<string> files, DateTime date)
{
    return files.Where(f =>
    {
        var timestamp = TimestampHelpers.ParseTimestamp(f);
        return timestamp.Date == date.Date;
    }).ToList();
}
```

**Add new method for range filtering:**
```csharp
public static List<string> FilterByDateRange(List<string> files, DateTime? after, DateTime? before)
{
    return files.Where(f =>
    {
        var timestamp = TimestampHelpers.ParseTimestamp(f);
        
        if (after.HasValue && timestamp < after.Value)
            return false;
            
        if (before.HasValue && timestamp > before.Value)
            return false;
            
        return true;
    }).ToList();
}
```

---

### 5. Update Each Command to Use New Filtering

#### a. ListCommand
**File:** `src/cycodj/CommandLineCommands/ListCommand.cs`

**Current:**
```csharp
// Filter by date if specified
if (!string.IsNullOrEmpty(Date))
{
    if (DateTime.TryParse(Date, out var dateFilter))
    {
        files = HistoryFileHelpers.FilterByDate(files, dateFilter);
```

**Change to:**
```csharp
// Filter by time range if specified
if (After.HasValue || Before.HasValue)
{
    files = HistoryFileHelpers.FilterByDateRange(files, After, Before);
    
    // Show what range we're filtering by
    if (After.HasValue && Before.HasValue)
        sb.AppendLine($"Filtered by date range: {After:yyyy-MM-dd} to {Before:yyyy-MM-dd} ({files.Count} files)");
    else if (After.HasValue)
        sb.AppendLine($"Filtered by date after: {After:yyyy-MM-dd} ({files.Count} files)");
    else if (Before.HasValue)
        sb.AppendLine($"Filtered by date before: {Before:yyyy-MM-dd} ({files.Count} files)");
```

**Also support legacy --date string parsing:**
```csharp
// Backward compatibility: Parse old --date if After/Before not set
if (!After.HasValue && !Before.HasValue && !string.IsNullOrEmpty(Date))
{
    // Try to parse as TIMESPEC
    try
    {
        var (after, before) = TimeSpecHelpers.ParseTimeSpecRange("--date", Date);
        files = HistoryFileHelpers.FilterByDateRange(files, after, before);
    }
    catch
    {
        // Fall back to old behavior
        if (DateTime.TryParse(Date, out var dateFilter))
        {
            files = HistoryFileHelpers.FilterByDate(files, dateFilter);
        }
    }
}
```

#### b. JournalCommand
**File:** `src/cycodj/CommandLineCommands/JournalCommand.cs`

**Current:**
```csharp
// Determine date range
DateTime startDate, endDate;

if (!string.IsNullOrEmpty(Date))
{
    if (Date.Equals("today", StringComparison.OrdinalIgnoreCase))
    {
        startDate = DateTime.Today;
        endDate = DateTime.Today.AddDays(1);
    }
    else if (DateTime.TryParse(Date, out var parsedDate))
    {
        startDate = parsedDate.Date;
        endDate = startDate.AddDays(1);
    }
```

**Change to:**
```csharp
// Determine date range
DateTime startDate, endDate;

// Priority 1: Use After/Before if set (from --after/--before/--date-range)
if (After.HasValue || Before.HasValue)
{
    startDate = After ?? DateTime.MinValue;
    endDate = Before ?? DateTime.MaxValue;
}
// Priority 2: Use LastDays if set (from --last-days, backward compat)
else if (LastDays > 0)
{
    endDate = DateTime.Today.AddDays(1);
    startDate = DateTime.Today.AddDays(-LastDays + 1);
}
// Priority 3: Parse old --date string if present
else if (!string.IsNullOrEmpty(Date))
{
    try
    {
        var (after, before) = TimeSpecHelpers.ParseTimeSpecRange("--date", Date);
        startDate = after ?? DateTime.Today;
        endDate = before ?? DateTime.Today.AddDays(1);
    }
    catch
    {
        // Fall back to old parsing
        if (Date.Equals("today", StringComparison.OrdinalIgnoreCase))
        {
            startDate = DateTime.Today;
            endDate = DateTime.Today.AddDays(1);
        }
        // ... etc
    }
}
else
{
    // Default: today only
    startDate = DateTime.Today;
    endDate = DateTime.Today.AddDays(1);
}
```

#### c. SearchCommand, BranchesCommand, StatsCommand, ExportCommand
**Same pattern** - replace date string parsing with TIMESPEC parsing using After/Before properties.

---

### 6. Update Help Documentation

**Files to update:**
- `src/cycodj/Help/*.md` (all help files that mention --date)

**Add to help:**
```
TIME SPECIFICATIONS (TIMESPEC):

  Absolute dates:
    --date "2023-09-01"
    --date "September 1, 2023"

  Relative times:
    --date 3d              # 3 days ago
    --date 7d              # 7 days ago (last week)
    --date 30d             # 30 days ago (last month)
    --date 4h              # 4 hours ago

  Keywords:
    --date today
    --date yesterday

  Ranges:
    --date-range "2023-01-01..2023-12-31"
    --date-range "7d..today"           # Last week to today
    --date-range "3d.."                # Last 3 days to now
    --date-range "..yesterday"         # Everything up to yesterday

  Explicit after/before:
    --after 7d
    --before yesterday
    --after 2023-01-01 --before 2023-12-31
```

---

### 7. Backward Compatibility Check

**Ensure these still work:**
- ✅ `cycodj list --date today` (keep string parsing as fallback)
- ✅ `cycodj list --date 2023-12-20` (keep string parsing as fallback)
- ✅ `cycodj journal --last-days 7` (keep LastDays property)
- ✅ `cycodj list --last 20` (unrelated to time, keeps working)

**New capabilities:**
- ✅ `cycodj list --date 7d` (parse as TIMESPEC)
- ✅ `cycodj list --date-range "7d..today"`
- ✅ `cycodj list --after 7d --before yesterday`
- ✅ `cycodj journal --date 7d` (instead of --last-days 7)
- ✅ `cycodj stats --date-range "2023-01-01..2023-12-31"`

---

### 8. Testing

**Create test cases for:**
1. Absolute dates: `--date "2023-12-20"`
2. Relative times: `--date 7d`, `--date 3d`
3. Keywords: `--date today`, `--date yesterday`
4. Ranges: `--date-range "7d..today"`, `--date-range "2023-01-01..2023-12-31"`
5. Open ranges: `--date-range "3d.."`, `--date-range "..yesterday"`
6. Explicit after/before: `--after 7d`, `--before yesterday`
7. Backward compat: Old string dates still work

---

## Summary

### Changes Needed:
1. ✅ TimeSpecHelpers - Already exists in common!
2. ✏️ Add After/Before properties to CycoDjCommand
3. ✏️ Add option parsing in CycoDjCommandLineOptions
4. ✏️ Add FilterByDateRange to HistoryFileHelpers
5. ✏️ Update 6 commands (list, journal, search, branches, stats, export)
6. ✏️ Update help documentation
7. ✏️ Test backward compatibility

### Changes NOT Needed:
- ❌ No changes to cycodmd (we're copying, not modifying)
- ❌ No changes to TimeSpecHelpers (already perfect)
- ❌ No new files needed (all exists or in existing files)

### Backward Compatibility Strategy:
- Keep old `Date` string property as fallback
- Keep `LastDays` for journal
- Try TIMESPEC parsing first, fall back to old parsing
- All old commands still work exactly as before
- New TIMESPEC syntax is additive, not breaking

---

## Estimated Effort

**Small changes:**
- Add properties: 5 minutes
- Add option parsing: 15 minutes
- Add FilterByDateRange: 5 minutes

**Medium changes:**
- Update each command: 10 minutes × 6 = 60 minutes
- Handle backward compatibility: 20 minutes

**Documentation:**
- Update help files: 30 minutes

**Testing:**
- Create test cases: 30 minutes
- Run and verify: 20 minutes

**Total: ~3 hours**

Most of it is mechanical - copy the pattern from cycodmd, apply to cycodj commands.

---

## The Beauty of This Approach

1. **Code reuse:** TimeSpecHelpers already exists and works!
2. **Consistency:** Same syntax across cycod tools
3. **Backward compatible:** Old commands still work
4. **Additive:** New power without breaking changes
5. **Well-tested:** cycodmd already uses this, so it's proven

**We're not inventing, we're reusing!** 🎯
