# READY TO IMPLEMENT: Date/Time Support for cycodj

## Decision Summary

✅ **Smart `--last` detection** (TIMESPEC vs count)  
✅ **Add `--today` and `--yesterday` shortcuts** (calendar days only)  
✅ **Full TIMESPEC support** (matching cycodmd)  
❌ **No multi-day shortcuts** (no --this-week, etc.)

---

## What We're Building

### 1. Shortcuts for Calendar Days
```bash
cycodj list --today              # This calendar day (midnight to now)
cycodj list --yesterday          # Yesterday's calendar day (full day)
```

**NOT rolling hours!**
- `--today` ≠ last 24 hours
- `--today` = since midnight this morning
- `--yesterday` = all of yesterday (00:00:00 to 23:59:59.999)

### 2. Smart --last Detection
```bash
cycodj list --last 20            # Count (pure number)
cycodj list --last 7d            # Time (has "d")
cycodj list --last today         # Time (keyword)
cycodj list --last 3d..yesterday # Time (range)
```

**Automatic detection:**
- Pure number → conversation count
- Has [d,h,m,s,..] or keywords → TIMESPEC

### 3. Full TIMESPEC Support
```bash
# Absolute dates
cycodj list --date "2023-12-20"
cycodj list --after "2023-01-01" --before "2023-12-31"

# Relative times
cycodj list --last 7d            # 7 days ago
cycodj list --last 4h            # 4 hours ago
cycodj list --last 30m           # 30 minutes ago

# Keywords
cycodj list --last today
cycodj list --last yesterday

# Ranges
cycodj journal --date-range "7d..today"
cycodj export --date-range "2023-01-01..2023-12-31" -o year.md

# Open ranges
cycodj list --date-range "3d.."          # 3 days ago to now
cycodj list --date-range "..yesterday"   # Up to yesterday

# Explicit boundaries
cycodj stats --after 7d --before 3d      # Between 7 and 3 days ago
```

---

## Implementation Checklist

### Phase 1: Foundation (30 minutes)
- [ ] Add `After` and `Before` properties to `CycoDjCommand`
- [ ] Add `FilterByDateRange()` to `HistoryFileHelpers`
- [ ] Add `IsTimeSpec()` helper function
- [ ] Verify `TimeSpecHelpers` is accessible (it already is!)

### Phase 2: Option Parsing (45 minutes)
**File:** `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs`

- [ ] Add `--today` shortcut parsing
- [ ] Add `--yesterday` shortcut parsing
- [ ] Add smart `--last` detection (TIMESPEC vs count)
- [ ] Add `--after` option parsing
- [ ] Add `--before` option parsing
- [ ] Add `--date-range` option parsing
- [ ] Update `--date` to support TIMESPEC

### Phase 3: Update Commands (60 minutes)
**Files:** `src/cycodj/CommandLineCommands/*.cs`

- [ ] Update `ListCommand` to use After/Before
- [ ] Update `JournalCommand` to use After/Before
- [ ] Update `SearchCommand` to use After/Before
- [ ] Update `BranchesCommand` to use After/Before
- [ ] Update `StatsCommand` to use After/Before
- [ ] Update `ExportCommand` to use After/Before

### Phase 4: Documentation (30 minutes)
**Files:** `src/cycodj/Help/*.md`

- [ ] Document `--today` and `--yesterday`
- [ ] Document smart `--last` (show both uses)
- [ ] Document TIMESPEC format
- [ ] Add examples for all formats
- [ ] Show relationship between options

### Phase 5: Testing (70 minutes)
- [ ] Test `--today` (at various times of day)
- [ ] Test `--yesterday` (including midnight edges)
- [ ] Test smart `--last` with counts (20, 50, 100)
- [ ] Test smart `--last` with times (7d, 4h, today)
- [ ] Test `--date-range` with absolute dates
- [ ] Test `--date-range` with relative times
- [ ] Test open ranges ("3d..", "..yesterday")
- [ ] Test `--after` and `--before` separately
- [ ] Test backward compatibility (old --date, --last-days)
- [ ] Test edge cases (midnight boundaries, timezone)

### Phase 6: Cleanup (15 minutes)
- [ ] Verify all old commands still work
- [ ] Check for consistent behavior across commands
- [ ] Final documentation review
- [ ] Commit with clear message

---

## Code Snippets (Ready to Use)

### IsTimeSpec Helper
```csharp
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
    
    // Pure number = conversation count
    return false;
}
```

### --today and --yesterday Shortcuts
```csharp
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

### Smart --last Detection
```csharp
else if (arg == "--last")
{
    var value = args[++i];
    
    if (IsTimeSpec(value))
    {
        // Parse as TIMESPEC
        var (after, before) = TimeSpecHelpers.ParseTimeSpecRange(arg, value);
        command.After = after;
        command.Before = before;
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
```

### FilterByDateRange
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

## Testing Script

```bash
# Shortcuts
cycodj list --today
cycodj list --yesterday
cycodj journal --today

# Smart --last (count)
cycodj list --last 20
cycodj list --last 50
cycodj list --last 1

# Smart --last (time)
cycodj list --last 7d
cycodj list --last today
cycodj list --last yesterday
cycodj list --last 3d..today

# Full TIMESPEC
cycodj journal --date-range "7d..today"
cycodj export --date-range "30d..today" -o month.md
cycodj stats --after 7d --before 3d
cycodj list --date-range "3d.."
cycodj search "bug" --date-range "..yesterday"

# Backward compat
cycodj list --date today
cycodj list --date 2023-12-20
cycodj journal --last-days 7
cycodj list --last 20
```

---

## Success Criteria

✅ All old commands still work (backward compatible)  
✅ `--today` and `--yesterday` work as calendar days  
✅ Smart `--last` detects count vs TIMESPEC automatically  
✅ Full TIMESPEC support (relative, absolute, ranges, keywords)  
✅ Help documentation is clear with examples  
✅ Consistent behavior across all commands  
✅ Edge cases handled (midnight boundaries)  
✅ Tests pass  

---

## Estimated Time

**Total: ~4 hours**

- Foundation: 30 min
- Parsing: 45 min
- Commands: 60 min
- Documentation: 30 min
- Testing: 70 min
- Cleanup: 15 min

---

## After This: What's Possible

```bash
# Daily workflow
cycodj list --today                    # What did I do today?
cycodj journal --yesterday             # Yesterday's journal

# Weekly review
cycodj stats --last 7d --show-tools    # This week's stats
cycodj journal --last 7d --detailed    # This week's journal

# Monthly report
cycodj export --last 30d -o month.md   # Export last month

# Project timeline
cycodj search "cycodgr" --date-range "2023-12-14..2023-12-19"

# Quarterly analysis
cycodj stats --last 90d --show-tools

# Yearly archive
cycodj export --date-range "2023-01-01..2023-12-31" -o 2023.md
```

**From "I need to run this 9 times" → "I run it once"** 🎯

---

## Ready to Go! 🚀

All decisions made:
- ✅ Smart `--last` (yes!)
- ✅ `--today` and `--yesterday` (yes!)
- ✅ Full TIMESPEC (matching cycodmd)
- ✅ No multi-day shortcuts (clear boundary)

All code snippets ready:
- ✅ IsTimeSpec helper
- ✅ Shortcut parsing
- ✅ Smart detection
- ✅ FilterByDateRange

All details documented:
- ✅ Punch list (todo/cycodj-date-range-PUNCHLIST.md)
- ✅ Smart --last discussion (todo/cycodj-smart-last-discussion.md)
- ✅ Shortcuts discussion (todo/cycodj-time-shortcuts-discussion.md)
- ✅ This implementation guide

**Let's build it!** 💪
