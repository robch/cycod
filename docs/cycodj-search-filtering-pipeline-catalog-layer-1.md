# cycodj search Command - Layer 1: Target Selection

## Overview

Layer 1 (Target Selection) determines **which conversation files** to search. The search command provides comprehensive time-based filtering to narrow down the set of conversations before performing content search.

## Implementation Summary

The search command supports multiple time-filtering mechanisms:

1. **Convenience shortcuts**: `--today`, `--yesterday`
2. **Explicit boundaries**: `--after <timespec>`, `--before <timespec>`
3. **Range specifications**: `--date-range <range>`, `--time-range <range>`
4. **Legacy date filtering**: `--date <date>`, `-d <date>`
5. **Smart last**: `--last <N|timespec>` (count or time specification)

## Command Line Options

### `--today`

Search conversations from today (calendar day).

**Parsing**: Sets `After = DateTime.Today` and `Before = DateTime.Now`

**Example**:
```bash
cycodj search "error" --today
```

### `--yesterday`

Search conversations from yesterday (calendar day).

**Parsing**: Sets `After = Yesterday 00:00` and `Before = Today 00:00 - 1 tick`

**Example**:
```bash
cycodj search "TODO" --yesterday
```

### `--after <timespec>` / `--time-after <timespec>`

Search conversations after the specified time.

**Timespec formats**:
- Absolute: `2024-01-01`, `2024-01-15T10:30:00`
- Relative: `-7d` (7 days ago), `-2h` (2 hours ago)

**Example**:
```bash
cycodj search "bug" --after 2024-01-01
cycodj search "feature" --after -7d
```

### `--before <timespec>` / `--time-before <timespec>`

Search conversations before the specified time.

**Example**:
```bash
cycodj search "deprecated" --before 2024-06-01
cycodj search "old" --before -30d
```

### `--date-range <range>` / `--time-range <range>`

Search conversations within a time range.

**Range format**: `<start>..<end>`

**Example**:
```bash
cycodj search "sprint" --date-range 2024-01-01..2024-01-31
cycodj search "recent" --time-range -7d..
```

### `--date <date>` / `-d <date>`

Search conversations from a specific date (backward compatibility).

**Special value**: `today`

**Example**:
```bash
cycodj search "meeting" --date 2024-01-15
cycodj search "standup" -d today
```

### `--last <N|timespec>`

Smart option that accepts either:
- **Count**: Last N conversations (e.g., `--last 10`)
- **Timespec**: Time range (e.g., `--last 7d`)

**Automatic detection**: Uses `IsTimeSpec()` helper to distinguish

**Examples**:
```bash
# Last 20 conversations
cycodj search "bug" --last 20

# Last 7 days
cycodj search "feature" --last 7d

# Last 2 hours
cycodj search "error" --last 2h
```

## Implementation Flow

### 1. Command Line Parsing

**File**: `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs`

**Methods**:
- `TryParseTimeOptions()` - Handles `--today`, `--yesterday`, `--after`, `--before`, `--date-range` (lines 216-273)
- `TryParseSearchCommandOptions()` - Handles search-specific options including `--date`, `--last` (lines 407-481)
- `ParseLastValue()` - Smart detection for `--last` (lines 554-594)
- `IsTimeSpec()` - Determines if value is timespec vs. count (lines 599-616)

### 2. File Discovery

**File**: `src/cycodj/Helpers/HistoryFileHelpers.cs`

**Method**: `FindAllHistoryFiles()`

Finds all `chat-history-*.jsonl` files in the history directory.

### 3. Time-Based Filtering

**File**: `src/cycodj/CommandLineCommands/SearchCommand.cs`

**Lines 59-94**: Apply time filters to file list

**Filter priority**:
1. If `After` or `Before` is set (from `--today`, `--yesterday`, `--after`, `--before`, `--date-range`):
   - Use `HistoryFileHelpers.FilterByDateRange()`
2. Else if `Date` is set (from `--date`):
   - Use `HistoryFileHelpers.FilterByDate()`
3. If `Last` is set (as count):
   - Take last N files ordered by timestamp

**Helper Methods**:
- `HistoryFileHelpers.FilterByDateRange(files, After, Before)` - Filters by time range
- `HistoryFileHelpers.FilterByDate(files, targetDate)` - Filters by specific date
- `TimestampHelpers.ParseTimestamp(fileName)` - Extracts timestamp from filename

### 4. Result Set

After Layer 1 filtering, the search command has a list of conversation files to search.

## Time Specification Format

### Absolute Times
- ISO 8601: `2024-01-15T10:30:00`
- Date only: `2024-01-15` (assumes 00:00:00)

### Relative Times
- Days: `-7d` (7 days ago), `7d` (7 days from now - rarely used)
- Hours: `-2h` (2 hours ago)
- Minutes: `-30m` (30 minutes ago)
- Seconds: `-60s` (60 seconds ago)

### Range Syntax
- `start..end` - From start to end
- `-7d..` - From 7 days ago to now
- `..2024-01-01` - From beginning to date
- `-7d..-1d` - From 7 days ago to 1 day ago

### Keywords
- `today` - Calendar day (00:00 to now)
- `yesterday` - Previous calendar day

## Parsing Implementation

See **[Layer 1 Proof](cycodj-search-filtering-pipeline-catalog-layer-1-proof.md)** for detailed source code evidence with line numbers.

## Filter Behavior

### Multiple Time Filters

If multiple time options are specified, the **most recent** takes precedence:

```bash
# --today overrides earlier --date
cycodj search "test" --date 2024-01-01 --today
```

### Smart --last Detection

The `--last` option automatically detects intent:

```bash
# Count (no time units or special chars)
--last 10        → Last 10 conversations

# Timespec (has time units or range syntax)
--last 7d        → Last 7 days
--last -2h       → Last 2 hours
--last today     → Today
--last 2024-01-01..  → From date to now
```

Detection uses regex patterns to identify time units (`d`, `h`, `m`, `s`), keywords (`today`, `yesterday`), and range syntax (`..`).

### Combination Examples

```bash
# Today's conversations, last 50 only
cycodj search "error" --today --last 50

# Date range, limit to 20 most recent
cycodj search "bug" --date-range -7d.. --last 20

# After specific time, yesterday only (conflicting - yesterday wins)
cycodj search "test" --after -7d --yesterday
```

## Performance Considerations

### File System Scanning

All time filtering operates on filenames (not file contents):
- Fast: Only reads directory listing
- Efficient: No need to parse file contents for filtering
- Scalable: Works with thousands of history files

### Timestamp Parsing

Conversation timestamps are encoded in filenames:
- Format: `chat-history-YYYYMMDD-HHMMSS-ffffff.jsonl`
- Fast parsing with regex pattern matching
- No file I/O required for time filtering

## Output Indication

When time filters are applied, the search command displays which filters were used:

```
Filtered by time range: 2024-01-01 00:00 to 2024-01-31 23:59 (45 files)
```

Or:

```
Filtered: after 2024-01-15 10:30 (12 files)
```

This helps users understand which conversations are being searched.

## Limitations

1. **No conversation property filtering**: Cannot filter by title, branch status, message count, etc. (that would be Layer 2)
2. **Filename-based only**: Time filtering relies on filename timestamps
3. **No content-based time filtering**: Cannot filter by last message time vs. conversation start time

## Related Options

While not strictly Layer 1, these options affect the search scope:

- **Layer 3**: `--user-only`, `--assistant-only` - Filter which messages to search (not which conversations)
- **Layer 6**: `--last` as count - Limits display count (not search scope when used as count)

## Comparison with Other Commands

| Command | Time Filtering | Options |
|---------|---------------|---------|
| **search** | ✅ Full support | All options |
| **list** | ✅ Full support | All options |
| **show** | ❌ No support | N/A |
| **branches** | ✅ Full support | All options |
| **stats** | ✅ Full support | All options |
| **cleanup** | ⚠️ Partial | `--older-than-days` only |

## Navigation

- [← Layer 0: Command Selection](cycodj-search-filtering-pipeline-catalog-README.md)
- [→ Layer 2: Container Filtering](cycodj-search-filtering-pipeline-catalog-layer-2.md)
- [↑ search Command Overview](cycodj-search-filtering-pipeline-catalog-README.md)
- [↑ cycodj Main Catalog](cycodj-filtering-pipeline-catalog-README.md)
- [📋 Source Code Proof →](cycodj-search-filtering-pipeline-catalog-layer-1-proof.md)
