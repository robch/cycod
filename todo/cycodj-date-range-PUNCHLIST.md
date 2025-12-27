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

#### A. Add --today and --yesterday shortcuts
```csharp
// Shortcuts for common calendar days
else if (arg == "--today")
{
    command.After = DateTime.Today;
    command.Before = DateTime.Now; // Up to current moment
}
else if (arg == "--yesterday")
{
    command.After = DateTime.Today.AddDays(-1);
    command.Before = DateTime.Today.AddTicks(-1); // End of yesterday
}
```

#### B. Add TIMESPEC options
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

#### C. Make --last smart (detect TIMESPEC vs count)
```csharp
else if (arg == "--last")
{
    var value = GetNextArg(i++, args);
    
    // Smart detection: TIMESPEC vs conversation count
    if (IsTimeSpec(value))
    {
        // Parse as TIMESPEC
        try
        {
            var (after, before) = TimeSpecHelpers.ParseTimeSpecRange(arg, value);
            command.After = after;
            command.Before = before;
        }
        catch (Exception ex)
        {
            throw new CommandLineException($"Invalid time specification for --last: {value}\n{ex.Message}");
        }
    }
    else
    {
        // Parse as conversation count
        if (!int.TryParse(value, out var count))
        {
            throw new CommandLineException($"Invalid number for --last: {value}");
        }
        command.Last = count;
    }
}

// Helper: Detect if value is a TIMESPEC
private bool IsTimeSpec(string value)
{
    if (string.IsNullOrWhiteSpace(value)) return false;
    
    // Has range syntax?
    if (value.Contains("..")) return true;
    
    // Is keyword?
    if (value.Equals("today", StringComparison.OrdinalIgnoreCase)) return true;
    if (value.Equals("yesterday", StringComparison.OrdinalIgnoreCase)) return true;
    
    // Has time units (d, h, m, s)?
    if (Regex.IsMatch(value, @"[dhms]", RegexOptions.IgnoreCase)) return true;
    
    // Otherwise it's a plain number (conversation count)
    return false;
}
```

#### D. Update existing --date to support TIMESPEC
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
- ✅ `cycodj list --today` (shortcut!)
- ✅ `cycodj list --yesterday` (shortcut!)
- ✅ `cycodj list --last today` (TIMESPEC)
- ✅ `cycodj list --last 7d` (smart detection - TIMESPEC!)
- ✅ `cycodj list --last 20` (smart detection - count!)
- ✅ `cycodj list --date-range "7d..today"`
- ✅ `cycodj list --after 7d --before yesterday`
- ✅ `cycodj journal --last 7d` (replaces --last-days 7)
- ✅ `cycodj stats --date-range "2023-01-01..2023-12-31"`

---

### 8. Testing

**Create test cases for:**

#### Shortcuts:
1. `--today` - This calendar day (midnight to now)
2. `--yesterday` - Yesterday's calendar day (full day)

#### Smart --last detection:
3. `--last 20` - Last 20 conversations (pure number = count)
4. `--last 7d` - Last 7 days (has "d" = TIMESPEC)
5. `--last today` - Today (keyword = TIMESPEC)
6. `--last yesterday` - Yesterday (keyword = TIMESPEC)
7. `--last 3d..today` - Range (has ".." = TIMESPEC)

#### TIMESPEC formats:
8. Absolute dates: `--date "2023-12-20"`
9. Relative times: `--last 7d`, `--last 3d`
10. Keywords: `--last today`, `--last yesterday`
11. Ranges: `--date-range "7d..today"`, `--date-range "2023-01-01..2023-12-31"`
12. Open ranges: `--date-range "3d.."`, `--date-range "..yesterday"`
13. Explicit after/before: `--after 7d`, `--before yesterday`

#### Backward compatibility:
14. Old string dates still work: `--date today`, `--date "2023-12-20"`
15. `--last-days` still works in journal
16. Pure numbers in --last still work as count

#### Edge cases:
17. `--today` at midnight (edge of day)
18. `--yesterday` at midnight (edge of day)
19. `--last 1` (count, not timespec)
20. `--last 1d` (timespec, not count)

---

## Summary

### Changes Needed:
1. ✅ TimeSpecHelpers - Already exists in common!
2. ✏️ Add After/Before properties to CycoDjCommand
3. ✏️ Add option parsing in CycoDjCommandLineOptions:
   - `--today` and `--yesterday` shortcuts
   - Smart `--last` detection (TIMESPEC vs count)
   - `--after`, `--before`, `--date-range` options
4. ✏️ Add FilterByDateRange to HistoryFileHelpers
5. ✏️ Update 6 commands (list, journal, search, branches, stats, export)
6. ✏️ Update help documentation
7. ✏️ Test backward compatibility + new features

### Changes NOT Needed:
- ❌ No changes to cycodmd (we're copying, not modifying)
- ❌ No changes to TimeSpecHelpers (already perfect)
- ❌ No new files needed (all exists or in existing files)

### Backward Compatibility Strategy:
- Keep old `Date` string property as fallback
- Keep `LastDays` for journal
- Smart `--last`: detect TIMESPEC vs count automatically
- All old commands still work exactly as before
- New syntax is additive, not breaking

### New Features:
- ✅ `--today` and `--yesterday` shortcuts (calendar days)
- ✅ Smart `--last`: `--last 20` (count) vs `--last 7d` (time)
- ✅ Full TIMESPEC support: relative times, keywords, ranges
- ✅ Consistent with cycodmd's date handling

---

## Estimated Effort

**Small changes:**
- Add properties: 5 minutes
- Add --today/--yesterday shortcuts: 10 minutes
- Add IsTimeSpec helper: 5 minutes
- Add FilterByDateRange: 5 minutes

**Medium changes:**
- Smart --last detection: 15 minutes
- Add TIMESPEC option parsing: 20 minutes
- Update each command: 10 minutes × 6 = 60 minutes
- Handle backward compatibility: 20 minutes

**Documentation:**
- Update help files: 30 minutes

**Testing:**
- Create test cases: 40 minutes
- Run and verify: 30 minutes

**Total: ~4 hours**

Most of it is mechanical - copy patterns, apply to commands, test.

---

## After Implementation: What Users Can Do

### Shortcuts (Most Common)
```bash
# Calendar day shortcuts
cycodj list --today                    # This morning to now
cycodj list --yesterday                # All of yesterday
cycodj journal --today                 # Today's journal
cycodj search "bug" --yesterday        # Yesterday's bugs
```

### Smart --last (Automatic Detection)
```bash
# Conversation count (pure number)
cycodj list --last 20                  # Last 20 conversations
cycodj stats --last 50                 # Stats for last 50

# Time period (has units/keywords)
cycodj list --last 7d                  # Last 7 days
cycodj list --last 30d                 # Last 30 days
cycodj list --last today               # Today
cycodj list --last yesterday           # Yesterday
cycodj journal --last 7d               # Week journal (replaces --last-days 7)
```

### Full TIMESPEC Power
```bash
# Relative times
cycodj list --last 3d                  # Last 3 days
cycodj stats --last 4h                 # Last 4 hours
cycodj export --last 90d -o quarter.md # Last quarter

# Date ranges
cycodj journal --date-range "7d..today"           # Last week
cycodj export --date-range "2023-01-01..2023-12-31" -o year.md

# Open ranges
cycodj list --date-range "3d.."        # Last 3 days to now
cycodj search "TODO" --date-range "..yesterday"   # Up to yesterday

# Explicit boundaries
cycodj list --after 7d                 # After 7 days ago
cycodj list --before yesterday         # Before yesterday
cycodj stats --after "2023-01-01" --before "2023-12-31"
```

### Combined with Other Options
```bash
# Shortcuts + instructions
cycodj list --today --instructions "summarize main topics"

# Smart --last + search
cycodj search "bug" --last 7d --context 3

# Range + export
cycodj export --date-range "30d..today" --output month.md
```

### Backward Compatible (Still Work!)
```bash
# Old syntax continues to work
cycodj list --date today               # Still works
cycodj list --date 2023-12-20          # Still works
cycodj journal --last-days 7           # Still works
cycodj list --last 20                  # Still works (count detection)
```

---

## The Beauty of This Approach

1. **Code reuse:** TimeSpecHelpers already exists and works!
2. **Consistency:** Same syntax across cycod tools
3. **Backward compatible:** Old commands still work
4. **Additive:** New power without breaking changes
5. **Well-tested:** cycodmd already uses this, so it's proven

**We're not inventing, we're reusing!** 🎯
