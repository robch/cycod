# cycodj list: Layer 1 - TARGET SELECTION

## Purpose

Layer 1 defines **what conversations to search** - the primary search space. For the `list` command, this means selecting which conversation history files to examine based on time criteria or count limits.

---

## Mechanisms

### 1. Default Behavior: Last 20 Conversations

If no filtering options are provided, the command defaults to showing the last 20 conversations.

**Implementation**: `ListCommand.cs` lines 97-104

### 2. Time-Based Filtering

#### After/Before Properties (Time Range)
Set via:
- `--today`: Sets After to today 00:00:00, Before to now
- `--yesterday`: Sets After to yesterday 00:00:00, Before to yesterday 23:59:59
- `--after TIME`, `--time-after TIME`: Sets After property
- `--before TIME`, `--time-before TIME`: Sets Before property  
- `--date-range RANGE`, `--time-range RANGE`: Sets both After and Before

**Implementation**: 
- Parser: `CycoDjCommandLineOptions.cs` lines 128-165
- Usage: `ListCommand.cs` lines 62-80

#### Date Property (Legacy)
Set via:
- `--date DATE`, `-d DATE`: Filters to specific calendar date

**Implementation**: 
- Parser: `CycoDjCommandLineOptions.cs` lines 113-123
- Usage: `ListCommand.cs` lines 82-95

### 3. Count-Based Limiting

#### Last Property
Set via:
- `--last N`: Shows last N conversations

**Note**: The `--last` option supports TWO modes:
1. **Count mode**: `--last 10` → Show last 10 conversations
2. **Time mode**: `--last 7d` → Show last 7 days of conversations (converted to After/Before)

**Smart Detection**: `CycoDjCommandLineOptions.cs` `ParseLastValue()` method (lines 319-351) determines if the value is a time specification or a count.

**Implementation**:
- Parser: `CycoDjCommandLineOptions.cs` lines 124-134
- Usage: `ListCommand.cs` lines 98-115

---

## Data Flow

### Step 1: Parse Command Line Options

**Source**: `CycoDjCommandLineOptions.cs` `TryParseListCommandOptions()`

The parser reads command line arguments and sets properties:

```
--date "2024-01-15"   → command.Date = "2024-01-15"
--last 10             → command.Last = 10
--last 7d             → command.After = 7 days ago, command.Before = now
--today               → command.After = today 00:00:00, command.Before = now
--yesterday           → command.After = yesterday 00:00:00, command.Before = yesterday 23:59:59
--after "2024-01-01"  → command.After = 2024-01-01 00:00:00
--before "2024-01-31" → command.Before = 2024-01-31 23:59:59
```

### Step 2: Find All History Files

**Source**: `ListCommand.cs` line 52

```csharp
var files = HistoryFileHelpers.FindAllHistoryFiles();
```

This retrieves all chat history files (`.jsonl` files) from the history directory.

### Step 3: Apply Time Filtering

**Source**: `ListCommand.cs` lines 62-95

Priority order:
1. **After/Before** (if set) → Filter by time range
2. **Date** (if set) → Filter by calendar date  
3. **No filters** → Apply default limit (20)

```csharp
// Filter by time range if After/Before are set
if (After.HasValue || Before.HasValue)
{
    files = HistoryFileHelpers.FilterByDateRange(files, After, Before);
}
// Filter by date if specified (backward compatibility)
else if (!string.IsNullOrEmpty(Date))
{
    if (DateTime.TryParse(Date, out var dateFilter))
    {
        files = HistoryFileHelpers.FilterByDate(files, dateFilter);
    }
}
```

### Step 4: Apply Count Limiting

**Source**: `ListCommand.cs` lines 97-115

```csharp
// Apply sensible default limit if not specified and no filters
var effectiveLimit = Last;
if (effectiveLimit == 0 && !After.HasValue && !Before.HasValue && string.IsNullOrEmpty(Date))
{
    effectiveLimit = 20; // Default to last 20 conversations
}

// Limit to last N if specified or defaulted
if (effectiveLimit > 0 && files.Count > effectiveLimit)
{
    files = files.Take(effectiveLimit).ToList();
}
```

---

## Command Line Option Details

### `--date DATE`, `-d DATE`

**Type**: String  
**Default**: null  
**Effect**: Filters conversations to those from the specified calendar date  

**Parsing**: `CycoDjCommandLineOptions.cs` lines 113-123

### `--last N` or `--last TIMESPEC`

**Type**: Int (for count) OR String (for timespec, converted to After/Before)  
**Default**: 0 (no limit)  
**Effect**: 
- Count mode: Limits to last N conversations
- Time mode: Filters to conversations within timespec

**Parsing**: `CycoDjCommandLineOptions.cs` lines 124-134  
**Smart Detection**: `CycoDjCommandLineOptions.cs` `ParseLastValue()` lines 319-351

### `--today`

**Type**: Flag  
**Effect**: Sets After to today 00:00:00, Before to current time  

**Parsing**: `CycoDjCommandLineOptions.cs` lines 142-147

### `--yesterday`

**Type**: Flag  
**Effect**: Sets After to yesterday 00:00:00, Before to yesterday 23:59:59  

**Parsing**: `CycoDjCommandLineOptions.cs` lines 149-154

### `--after TIME`, `--time-after TIME`

**Type**: String (time specification)  
**Default**: null  
**Effect**: Sets After property to parsed timestamp  

**Parsing**: `CycoDjCommandLineOptions.cs` lines 156-164  
**Helper**: `TimeSpecHelpers.ParseSingleTimeSpec()` in `src/common/Helpers/TimeSpecHelpers.cs`

### `--before TIME`, `--time-before TIME`

**Type**: String (time specification)  
**Default**: null  
**Effect**: Sets Before property to parsed timestamp  

**Parsing**: `CycoDjCommandLineOptions.cs` lines 166-174  
**Helper**: `TimeSpecHelpers.ParseSingleTimeSpec()` in `src/common/Helpers/TimeSpecHelpers.cs`

### `--date-range RANGE`, `--time-range RANGE`

**Type**: String (time range specification like `7d..` or `2024-01-01..2024-01-31`)  
**Effect**: Sets both After and Before properties  

**Parsing**: `CycoDjCommandLineOptions.cs` lines 176-184  
**Helper**: `TimeSpecHelpers.ParseTimeSpecRange()` in `src/common/Helpers/TimeSpecHelpers.cs`

---

## TimeSpec Format

Time specifications support:

**Relative Times**:
- `7d` - 7 days
- `3h` - 3 hours  
- `30m` - 30 minutes
- `90s` - 90 seconds

**Negative (Ago)**:
- `-7d` - 7 days ago

**Ranges**:
- `7d..` - Last 7 days to now
- `-7d..` - Same as above
- `2024-01-01..2024-01-31` - Specific date range

**Keywords**:
- `today` - Today's date
- `yesterday` - Yesterday's date

**Implementation**: `src/common/Helpers/TimeSpecHelpers.cs`

---

## Proof Documentation

See [cycodj-list-filtering-pipeline-catalog-layer-1-proof.md](cycodj-list-filtering-pipeline-catalog-layer-1-proof.md) for:
- Complete source code excerpts with line numbers
- Parser tracing from command line args to property values
- Data flow from properties to file filtering logic
- Helper method implementations
