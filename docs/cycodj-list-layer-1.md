# cycodj list - Layer 1: TARGET SELECTION

## Overview

The `list` command is the default command for cycodj. Layer 1 (TARGET SELECTION) determines which conversation files to list based on time filtering and count limiting.

## Implementation Summary

The `list` command implements **RICH** target selection with multiple mechanisms:

1. **Time-range filtering** (primary, modern approach)
2. **Legacy date filtering** (backward compatibility)
3. **Count limiting** (smart: conversation count OR timespec)
4. **Default limiting** (20 conversations if no filters)

## Target Selection Options

### Time-Range Filtering (Modern)

#### Shortcuts
- `--today` - Today's conversations (calendar day from midnight to now)
- `--yesterday` - Yesterday's conversations (previous calendar day)

#### Absolute/Relative Times
- `--after <timespec>`, `--time-after <timespec>` - Conversations after specific time
- `--before <timespec>`, `--time-before <timespec>` - Conversations before specific time
- `--date-range <range>`, `--time-range <range>` - Conversations within time range

#### Timespec Format
- Absolute: `2024-01-15`, `2024-01-15T10:30:00`
- Relative: `7d` (7 days ago), `-7d` (7 days ago), `2h` (2 hours ago)
- Range: `7d..`, `..yesterday`, `2024-01-01..2024-01-31`

### Legacy Date Filtering

- `--date <date>`, `-d <date>` - Conversations on specific date
  - Format: `YYYY-MM-DD` or parseable date string
  - Special: `today` keyword supported

### Count Limiting (Smart Detection)

- `--last <N>` - EITHER last N conversations OR timespec
  - If N is pure integer: last N conversations
  - If N has time units (d/h/m/s): time-based filter
  - Examples:
    - `--last 10` → last 10 conversations
    - `--last 7d` → conversations from last 7 days
    - `--last 2h` → conversations from last 2 hours

### Default Behavior

If NO filters are specified:
- Shows **last 20 conversations** (default limit)
- User is informed: "Showing last 20 conversations (use --last N to change, or --date to filter)"

## Processing Pipeline

### Step 1: Find All History Files
```
files = HistoryFileHelpers.FindAllHistoryFiles()
```

### Step 2: Apply Time-Range Filter (if specified)
```
IF (After.HasValue OR Before.HasValue)
  files = HistoryFileHelpers.FilterByDateRange(files, After, Before)
```

### Step 3: Apply Legacy Date Filter (if specified)
```
ELSE IF (Date is not empty)
  Parse Date as DateTime
  files = HistoryFileHelpers.FilterByDate(files, dateFilter)
```

### Step 4: Apply Default Limit (if no filters)
```
IF (no filters and Last == 0)
  effectiveLimit = 20 (default)
```

### Step 5: Apply Count Limit (if specified or defaulted)
```
IF (effectiveLimit > 0 AND files.Count > effectiveLimit)
  files = files.Take(effectiveLimit)
```

## Priority/Precedence

1. **Time-range filters** (`--today`, `--after`, `--before`, `--date-range`) - Highest priority
2. **Legacy date filter** (`--date`) - If no time-range filters
3. **Default limit** (20) - If no filters at all
4. **Count limit** (`--last N`) - Applied AFTER time/date filters

## Smart Detection Logic

The `--last` option uses smart detection:

```
IsTimeSpec(value):
  - Contains ".." → timespec range
  - Equals "today" or "yesterday" → timespec keyword
  - Contains 'd', 'h', 'm', or 's' → timespec with units
  - Pure integer → conversation count
```

When timespec detected:
- Automatically converts to backward range: `7d` → `-7d..` (7 days ago to now)
- Calls `TimeSpecHelpers.ParseTimeSpecRange()`
- Sets `After` and `Before` properties

When count detected:
- Sets `Last` property
- Applied as `files.Take(count)`

## Examples

### Example 1: Time-Range Shortcuts
```bash
cycodj list --today
# Shows: conversations from midnight today to now
# After = DateTime.Today, Before = DateTime.Now

cycodj list --yesterday
# Shows: conversations from previous calendar day
# After = Today.AddDays(-1), Before = Today.AddTicks(-1)
```

### Example 2: Absolute Time Ranges
```bash
cycodj list --after 2024-01-01 --before 2024-01-31
# Shows: conversations in January 2024

cycodj list --date-range 2024-01-01..2024-01-31
# Same as above, more concise
```

### Example 3: Relative Time Ranges
```bash
cycodj list --last 7d
# Smart detection: timespec → last 7 days
# Converts to: After = 7 days ago, Before = now

cycodj list --last 10
# Smart detection: count → last 10 conversations
# Applied as: files.Take(10)
```

### Example 4: Legacy Date Filter
```bash
cycodj list --date 2024-01-15
# Shows: conversations on specific date
# Uses FilterByDate() method

cycodj list --date today
# Shows: today's conversations (legacy syntax)
```

### Example 5: Default Behavior
```bash
cycodj list
# No filters specified
# Shows: last 20 conversations (default limit)
```

### Example 6: Combining Filters
```bash
cycodj list --after 2024-01-01 --last 5
# First: filter by time (after 2024-01-01)
# Then: limit to 5 conversations
```

## Related Files

- **Implementation**: [cycodj-list-layer-1-proof.md](cycodj-list-layer-1-proof.md)
- **Parser**: [cycodj-list-layer-1-proof.md#parser-evidence](cycodj-list-layer-1-proof.md#parser-evidence)
- **Execution**: [cycodj-list-layer-1-proof.md#execution-evidence](cycodj-list-layer-1-proof.md#execution-evidence)

## See Also

- [TimeSpecHelpers](cycodj-list-layer-1-proof.md#timespechelpers) - Time specification parsing
- [HistoryFileHelpers](cycodj-list-layer-1-proof.md#historyfilehelpers) - File filtering utilities
- [Smart Detection Logic](cycodj-list-layer-1-proof.md#smart-detection) - Timespec vs count detection

---

**Next Layer**: [Layer 6: Display Control](cycodj-list-layer-6.md) (Layer 2-5 not implemented)
