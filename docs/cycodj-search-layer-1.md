# cycodj search - Layer 1: TARGET SELECTION

## Overview

The `search` command searches for text within conversation message content. Layer 1 (TARGET SELECTION) determines which conversation files to search through before performing the content search (Layer 3).

## Implementation Summary

The `search` command implements **RICH** target selection, identical to the `list` command:

1. **Time-range filtering** (primary, modern approach)
2. **Legacy date filtering** (backward compatibility)
3. **Count limiting** (smart: conversation count OR timespec)
4. **Required: search query** (positional argument)

## Target Selection Options

### Required: Search Query

- **Positional argument** (first non-option argument)
- The text/pattern to search for in conversation messages
- Must be provided (command fails if missing)

### Time-Range Filtering (Modern)

#### Shortcuts
- `--today` - Search today's conversations
- `--yesterday` - Search yesterday's conversations

#### Absolute/Relative Times
- `--after <timespec>`, `--time-after <timespec>` - Search conversations after time
- `--before <timespec>`, `--time-before <timespec>` - Search conversations before time
- `--date-range <range>`, `--time-range <range>` - Search within time range

#### Timespec Format
- Absolute: `2024-01-15`, `2024-01-15T10:30:00`
- Relative: `7d` (7 days ago), `-7d` (7 days ago), `2h` (2 hours ago)
- Range: `7d..`, `..yesterday`, `2024-01-01..2024-01-31`

### Legacy Date Filtering

- `--date <date>`, `-d <date>` - Search conversations on specific date
  - Format: `YYYY-MM-DD` or parseable date string
  - Special: `today` keyword supported

### Count Limiting (Smart Detection)

- `--last <N>` - EITHER last N conversations OR timespec
  - If N is pure integer: search last N conversations
  - If N has time units (d/h/m/s): time-based filter
  - Examples:
    - `--last 10` → search last 10 conversations
    - `--last 7d` → search conversations from last 7 days
    - `--last 2h` → search conversations from last 2 hours

## Processing Pipeline

### Step 1: Validate Query
```
IF Query is null or empty
  ERROR: "Search query is required."
```

### Step 2: Find All History Files
```
files = HistoryFileHelpers.FindAllHistoryFiles()
```

### Step 3: Apply Time-Range Filter (if specified)
```
IF (After.HasValue OR Before.HasValue)
  files = HistoryFileHelpers.FilterByDateRange(files, After, Before)
```

### Step 4: Apply Legacy Date Filter (if specified)
```
ELSE IF (Date is not empty)
  Parse Date as DateTime
  files = HistoryFileHelpers.FilterByDate(files, dateFilter)
```

### Step 5: Apply Count Limit (if specified)
```
IF (Last.HasValue AND Last.Value > 0)
  files = files.OrderByDescending(ParseTimestamp)
                .Take(Last.Value)
                .OrderBy(ParseTimestamp)
```

### Step 6: Search Within Selected Files
```
FOR EACH file in files
  conversation = Read conversation
  matches = SearchConversation(conversation, query)  // Layer 3
  IF matches found
    Add to results
```

## Priority/Precedence

1. **Query validation** - Must have search query (fail early)
2. **Time-range filters** (`--today`, `--after`, `--before`, `--date-range`) - Highest priority
3. **Legacy date filter** (`--date`) - If no time-range filters
4. **Count limit** (`--last N`) - Applied AFTER time/date filters
5. **NO default limit** - Unlike `list`, searches ALL matching conversations by default

## Differences from list Command

| Aspect | list | search |
|--------|------|--------|
| **Default limit** | 20 conversations | NO limit (searches all) |
| **Required argument** | None | Query required |
| **Purpose** | Browse all conversations | Find specific content |
| **Performance** | Fast (small default set) | Slower (searches all by default) |

## Smart Detection Logic

Identical to `list` command:

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
- Applied as sort + take: `files.OrderByDescending(...).Take(count)`

## Examples

### Example 1: Basic Search (All Conversations)
```bash
cycodj search "ConPTY"
# Searches: ALL conversations for "ConPTY"
# No time limit, no count limit
```

### Example 2: Search Today's Conversations
```bash
cycodj search "error" --today
# Searches: Only today's conversations for "error"
```

### Example 3: Search Last 10 Conversations
```bash
cycodj search "async" --last 10
# Searches: Only the 10 most recent conversations for "async"
```

### Example 4: Search Last 7 Days
```bash
cycodj search "bug" --last 7d
# Searches: Conversations from last 7 days for "bug"
```

### Example 5: Search Specific Date Range
```bash
cycodj search "feature request" --date-range 2024-01-01..2024-01-31
# Searches: Conversations in January 2024
```

### Example 6: Search with Time Filter
```bash
cycodj search "performance" --after 2024-01-01
# Searches: Conversations after 2024-01-01 for "performance"
```

### Example 7: Legacy Date Search
```bash
cycodj search "deployment" --date 2024-01-15
# Searches: Conversations on 2024-01-15 for "deployment"
```

### Example 8: Combining Filters
```bash
cycodj search "timeout" --after 2024-01-01 --before 2024-02-01
# Searches: Conversations in January 2024 for "timeout"
```

## Performance Implications

### Without Time/Count Filters
- **Impact**: Searches ALL conversations in history
- **Time**: Can be slow for large histories (100s-1000s of conversations)
- **Recommendation**: Use `--last N` or `--last Nd` to limit scope

### With Time Filters
- **Impact**: Reduces file set before parsing
- **Efficiency**: Only parses conversations in time range
- **Benefit**: Significant speedup for large histories

### With Count Limit
- **Impact**: Limits to N most recent conversations
- **Efficiency**: Parses only N conversations
- **Benefit**: Predictable search time

### Best Practices
- For recent issues: `--last 20` or `--last 7d`
- For specific dates: `--date` or `--date-range`
- For exhaustive search: no limit (but be patient)
- For iterative narrowing: start broad, add filters

## Related Files

- **Implementation**: [cycodj-search-layer-1-proof.md](cycodj-search-layer-1-proof.md)
- **Parser**: [cycodj-search-layer-1-proof.md#parser-evidence](cycodj-search-layer-1-proof.md#parser-evidence)
- **Execution**: [cycodj-search-layer-1-proof.md#execution-evidence](cycodj-search-layer-1-proof.md#execution-evidence)

## See Also

- [Layer 3: Content Filtering](cycodj-search-layer-3.md) - How search query is applied to messages
- [Layer 5: Context Expansion](cycodj-search-layer-5.md) - How context lines are shown around matches
- [TimeSpecHelpers](cycodj-search-layer-1-proof.md#timespechelpers) - Time specification parsing
- [HistoryFileHelpers](cycodj-search-layer-1-proof.md#historyfilehelpers) - File filtering utilities

---

**Next Layer**: [Layer 3: Content Filtering](cycodj-search-layer-3.md) (Layer 2 not implemented)
