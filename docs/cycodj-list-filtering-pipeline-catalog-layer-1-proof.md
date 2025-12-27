# cycodj list: Layer 1 - TARGET SELECTION - Proof

This document provides **source code evidence** for all Layer 1 (TARGET SELECTION) mechanisms in the `list` command.

---

## Table of Contents

1. [Command Line Parser](#command-line-parser)
2. [Smart --last Detection](#smart-last-detection)
3. [Time Parsing Helpers](#time-parsing-helpers)
4. [ListCommand Execution](#listcommand-execution)
5. [History File Helpers](#history-file-helpers)
6. [Data Flow Trace](#data-flow-trace)

---

## Command Line Parser

### File: `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs`

#### Option Parsing for ListCommand

**Lines 96-134**: `TryParseListCommandOptions()` method

```csharp
private bool TryParseListCommandOptions(ListCommand command, string[] args, ref int i, string arg)
{
    // Try common display options first
    if (TryParseDisplayOptions(command, args, ref i, arg))
    {
        return true;
    }
    
    // Try common time options
    if (TryParseTimeOptions(command, args, ref i, arg))
    {
        return true;
    }
    
    if (arg == "--date" || arg == "-d")
    {
        var date = i + 1 < args.Length ? args[++i] : null;
        if (string.IsNullOrWhiteSpace(date))
        {
            throw new CommandLineException($"Missing date value for {arg}");
        }
        command.Date = date;
        return true;
    }
    else if (arg == "--last")
    {
        var value = i + 1 < args.Length ? args[++i] : null;
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new CommandLineException($"Missing value for {arg}");
        }
        
        ParseLastValue(command, arg, value);
        return true;
    }
    
    return false;
}
```

**Evidence**: 
- Line 113-123: `--date` / `-d` sets `command.Date` property
- Line 124-134: `--last` calls `ParseLastValue()` for smart detection

---

### Time Option Parsing (Shared Method)

**Lines 186-242**: `TryParseTimeOptions()` method

```csharp
private bool TryParseTimeOptions(CycoDjCommand command, string[] args, ref int i, string arg)
{
    // --today shortcut (calendar day)
    if (arg == "--today")
    {
        command.After = DateTime.Today;
        command.Before = DateTime.Now;
        return true;
    }
    
    // --yesterday shortcut (calendar day)
    else if (arg == "--yesterday")
    {
        command.After = DateTime.Today.AddDays(-1);
        command.Before = DateTime.Today.AddTicks(-1);
        return true;
    }
    
    // --after <timespec>
    else if (arg == "--after" || arg == "--time-after")
    {
        var timeSpec = i + 1 < args.Length ? args[++i] : null;
        if (string.IsNullOrWhiteSpace(timeSpec))
        {
            throw new CommandLineException($"Missing timespec value for {arg}");
        }
        command.After = TimeSpecHelpers.ParseSingleTimeSpec(arg, timeSpec, isAfter: true);
        return true;
    }
    
    // --before <timespec>
    else if (arg == "--before" || arg == "--time-before")
    {
        var timeSpec = i + 1 < args.Length ? args[++i] : null;
        if (string.IsNullOrWhiteSpace(timeSpec))
        {
            throw new CommandLineException($"Missing timespec value for {arg}");
        }
        command.Before = TimeSpecHelpers.ParseSingleTimeSpec(arg, timeSpec, isAfter: false);
        return true;
    }
    
    // --date-range <range> or --time-range <range>
    else if (arg == "--date-range" || arg == "--time-range")
    {
        var timeSpec = i + 1 < args.Length ? args[++i] : null;
        if (string.IsNullOrWhiteSpace(timeSpec))
        {
            throw new CommandLineException($"Missing timespec range for {arg}");
        }
        var (after, before) = TimeSpecHelpers.ParseTimeSpecRange(arg, timeSpec);
        command.After = after;
        command.Before = before;
        return true;
    }
    
    return false;
}
```

**Evidence**:
- Line 142-147: `--today` sets After=Today 00:00, Before=Now
- Line 149-154: `--yesterday` sets After=Yesterday 00:00, Before=Yesterday 23:59:59
- Line 156-164: `--after`/`--time-after` sets After property via TimeSpecHelpers
- Line 166-174: `--before`/`--time-before` sets Before property via TimeSpecHelpers
- Line 176-184: `--date-range`/`--time-range` sets both After and Before

---

## Smart --last Detection

### File: `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs`

**Lines 319-351**: `ParseLastValue()` method

```csharp
private void ParseLastValue(CycoDjCommand command, string arg, string value)
{
    // Smart detection: TIMESPEC vs conversation count
    if (IsTimeSpec(value))
    {
        // Parse as TIMESPEC
        try
        {
            // For --last context, relative times should go BACKWARD (ago)
            // If value is like "7d", convert to "-7d.." (7 days ago to now)
            var timeSpec = value;
            if (System.Text.RegularExpressions.Regex.IsMatch(value, @"^\d+[dhms]", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                timeSpec = "-" + value + ".."; // Make it a range from N ago to now
            }
            
            var (after, before) = TimeSpecHelpers.ParseTimeSpecRange(arg, timeSpec);
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
        // Parse as conversation count (for ListCommand, SearchCommand, etc.)
        if (!int.TryParse(value, out var count))
        {
            throw new CommandLineException($"Invalid number for --last: {value}");
        }
        
        // Set Last property if it exists on the command
        var lastProp = command.GetType().GetProperty("Last");
        if (lastProp != null)
        {
            lastProp.SetValue(command, count);
        }
    }
}
```

**Evidence**:
- Line 321: Calls `IsTimeSpec()` to determine mode
- Line 328-331: If looks like timespec without negative sign, converts `7d` → `-7d..`
- Line 333: Calls `TimeSpecHelpers.ParseTimeSpecRange()` to parse as time
- Line 341-349: Otherwise parses as integer count and sets `Last` property via reflection

---

### TimeSpec Detection Helper

**Lines 353-372**: `IsTimeSpec()` method

```csharp
private static bool IsTimeSpec(string value)
{
    if (string.IsNullOrWhiteSpace(value)) return false;
    
    // Has range syntax?
    if (value.Contains("..")) return true;
    
    // Is keyword?
    if (value.Equals("today", StringComparison.OrdinalIgnoreCase)) return true;
    if (value.Equals("yesterday", StringComparison.OrdinalIgnoreCase)) return true;
    
    // Has time units (d, h, m, s)?
    if (System.Text.RegularExpressions.Regex.IsMatch(value, @"[dhms]", System.Text.RegularExpressions.RegexOptions.IgnoreCase)) 
        return true;
    
    // Pure number = conversation count
    return false;
}
```

**Evidence**:
- Line 358: Range syntax (`..`) indicates timespec
- Line 361-362: Keywords `today`/`yesterday` indicate timespec
- Line 365-366: Presence of time units (`d`, `h`, `m`, `s`) indicates timespec
- Line 369: Pure numbers are treated as counts

---

## Time Parsing Helpers

### File: `src/common/Helpers/TimeSpecHelpers.cs`

#### ParseTimeSpecRange

**Lines 7-42**: `ParseTimeSpecRange()` method

```csharp
public static (DateTime? After, DateTime? Before) ParseTimeSpecRange(string optionName, string? timeSpec)
{
    if (string.IsNullOrEmpty(timeSpec))
        throw new CommandLineException($"Missing time specification for {optionName}");

    // Handle range syntax with ".."
    if (timeSpec.Contains(".."))
    {
        var parts = timeSpec.Split(new[] { ".." }, StringSplitOptions.None);
        if (parts.Length != 2)
            throw new CommandLineException($"Invalid range format for {optionName}: {timeSpec}");
            
        DateTime? after = null;
        DateTime? before = null;
        
        if (!string.IsNullOrEmpty(parts[0]))
            after = ParseSingleTimeSpec(optionName, parts[0], isAfter: true);
            
        if (!string.IsNullOrEmpty(parts[1]))
            before = ParseSingleTimeSpec(optionName, parts[1], isAfter: false);
            
        return (after, before);
    }
    else
    {
        // For single date/time (represents a time period)
        var parsed = ParseSingleTimeSpec(optionName, timeSpec, isAfter: true);
        
        // If it's a date without time, set before to end of day
        DateTime? before = parsed;
        if (parsed.HasValue && IsDateOnly(parsed.Value))
            before = parsed.Value.AddDays(1).AddTicks(-1);
            
        return (parsed, before);
    }
}
```

**Evidence**:
- Line 13-28: Range syntax handling (`start..end`)
- Line 30-40: Single value handling (converts date to full day range)

---

#### ParseSingleTimeSpec

**Lines 45-107**: `ParseSingleTimeSpec()` method

```csharp
public static DateTime? ParseSingleTimeSpec(string optionName, string? timeSpec, bool isAfter)
{
    if (string.IsNullOrEmpty(timeSpec))
        throw new CommandLineException($"Missing time specification for {optionName}");

    // Handle special keywords
    if (timeSpec.Equals("today", StringComparison.OrdinalIgnoreCase))
        return isAfter ? DateTime.Today : DateTime.Today.AddDays(1).AddTicks(-1);

    if (timeSpec.Equals("yesterday", StringComparison.OrdinalIgnoreCase))
        return isAfter ? DateTime.Today.AddDays(-1) : DateTime.Today.AddTicks(-1);

    // Handle absolute dates
    if (DateTime.TryParse(timeSpec, out var dateTime))
    {
        // If it's a date without time, adjust to start/end of day as appropriate
        if (IsDateOnly(dateTime))
        {
            return isAfter ? dateTime : dateTime.AddDays(1).AddTicks(-1);
        }
        return dateTime;
    }

    // Handle combined time specs like "2d4h3m"
    var combinedMatch = Regex.Match(timeSpec, @"^-?(\d+[dhms])+$", RegexOptions.IgnoreCase);
    if (combinedMatch.Success)
    {
        return ParseCombinedTimeSpec(timeSpec);
    }

    // Handle simple relative time specs like "3d" or "-2h"
    var match = Regex.Match(timeSpec, @"^(-?)(\d+)([dhms])$", RegexOptions.IgnoreCase);
    if (match.Success)
    {
        var sign = match.Groups[1].Value == "-" ? -1 : 1;
        var value = int.Parse(match.Groups[2].Value) * sign;
        var unit = match.Groups[3].Value.ToLower();
        
        var now = DateTime.Now;
        
        switch (unit)
        {
            case "d":
                // For days, use calendar day boundaries
                var date = now.Date.AddDays(value);
                return isAfter ? date : date.AddDays(1).AddTicks(-1);
            
            case "h":
                return now.AddHours(value);
            
            case "m":
                return now.AddMinutes(value);
            
            case "s":
                return now.AddSeconds(value);
            
            default:
                throw new CommandLineException($"Invalid time unit in {optionName}: {unit}");
        }
    }

    throw new CommandLineException($"Invalid time specification for {optionName}: {timeSpec}");
}
```

**Evidence**:
- Line 51-52: Keyword `today` handling
- Line 54-55: Keyword `yesterday` handling
- Line 58-66: Absolute date parsing
- Line 69-72: Combined timespec parsing (`2d4h3m`)
- Line 76-104: Simple relative timespec parsing (`7d`, `-2h`)

---

## ListCommand Execution

### File: `src/cycodj/CommandLineCommands/ListCommand.cs`

#### Default Behavior

**Lines 97-104**: Default limit application

```csharp
// Apply sensible default limit if not specified and no filters
var effectiveLimit = Last;
if (effectiveLimit == 0 && !After.HasValue && !Before.HasValue && string.IsNullOrEmpty(Date))
{
    effectiveLimit = 20; // Default to last 20 conversations
    sb.AppendLine($"Showing last {effectiveLimit} conversations (use --last N to change, or --date to filter)");
    sb.AppendLine();
}
```

**Evidence**:
- Line 98: Uses `Last` property as base
- Line 99: Checks if no filters are specified
- Line 101: Defaults to 20 conversations if no filters

---

#### Time Range Filtering

**Lines 62-80**: After/Before filtering

```csharp
// Filter by time range if After/Before are set (from --today, --yesterday, --last <timespec>, etc.)
if (After.HasValue || Before.HasValue)
{
    files = HistoryFileHelpers.FilterByDateRange(files, After, Before);
    
    if (After.HasValue && Before.HasValue)
    {
        sb.AppendLine($"Filtered by time range: {After:yyyy-MM-dd HH:mm} to {Before:yyyy-MM-dd HH:mm} ({files.Count} files)");
    }
    else if (After.HasValue)
    {
        sb.AppendLine($"Filtered: after {After:yyyy-MM-dd HH:mm} ({files.Count} files)");
    }
    else if (Before.HasValue)
    {
        sb.AppendLine($"Filtered: before {Before:yyyy-MM-dd HH:mm} ({files.Count} files)");
    }
    sb.AppendLine();
}
```

**Evidence**:
- Line 63: Checks for After/Before properties
- Line 65: Calls `HistoryFileHelpers.FilterByDateRange()`
- Line 67-78: Displays which filter was applied

---

#### Date Filtering (Legacy)

**Lines 82-95**: Date property filtering

```csharp
// Filter by date if specified (backward compatibility)
else if (!string.IsNullOrEmpty(Date))
{
    if (DateTime.TryParse(Date, out var dateFilter))
    {
        files = HistoryFileHelpers.FilterByDate(files, dateFilter);
        sb.AppendLine($"Filtered by date: {dateFilter:yyyy-MM-dd} ({files.Count} files)");
        sb.AppendLine();
    }
    else
    {
        sb.AppendLine($"ERROR: Invalid date format: {Date}");
        return sb.ToString();
    }
}
```

**Evidence**:
- Line 83: Checks for Date property
- Line 84: Parses date string
- Line 86: Calls `HistoryFileHelpers.FilterByDate()`

---

#### Count Limiting

**Lines 106-115**: Last N limiting

```csharp
// Limit to last N if specified or defaulted
if (effectiveLimit > 0 && files.Count > effectiveLimit)
{
    files = files.Take(effectiveLimit).ToList();
    if (Last > 0)
    {
        sb.AppendLine($"Showing last {effectiveLimit} conversations");
        sb.AppendLine();
    }
}
```

**Evidence**:
- Line 107: Checks if limit is set and files exceed limit
- Line 109: Takes first N files (files are already sorted newest first)

---

## History File Helpers

### File: `src/cycodj/Helpers/HistoryFileHelpers.cs`

#### Find All History Files

**Lines 22-46**: `FindAllHistoryFiles()` method

```csharp
public static List<string> FindAllHistoryFiles()
{
    var historyDir = GetHistoryDirectory();
    
    if (!Directory.Exists(historyDir))
    {
        Logger.Warning($"History directory not found: {historyDir}");
        return new List<string>();
    }
    
    try
    {
        var files = Directory.GetFiles(historyDir, "chat-history-*.jsonl")
            .OrderByDescending(f => f)
            .ToList();
            
        Logger.Info($"Found {files.Count} chat history files");
        return files;
    }
    catch (Exception ex)
    {
        Logger.Error($"Error reading history directory: {ex.Message}");
        return new List<string>();
    }
}
```

**Evidence**:
- Line 24: Gets history directory (`~/.cycod/history/`)
- Line 34: Finds all files matching `chat-history-*.jsonl`
- Line 35: Sorts by filename (descending = newest first, because of timestamp in filename)

---

#### Filter By Date Range

**Lines 51-63**: `FilterByDateRange()` method

```csharp
public static List<string> FilterByDateRange(List<string> files, DateTime? after, DateTime? before)
{
    return files.Where(f =>
    {
        var timestamp = TimestampHelpers.ParseTimestamp(f);
        if (timestamp == DateTime.MinValue) return false;
        
        if (after.HasValue && timestamp < after.Value) return false;
        if (before.HasValue && timestamp > before.Value) return false;
        
        return true;
    }).ToList();
}
```

**Evidence**:
- Line 55: Parses timestamp from filename
- Line 58: Filters out files before After time
- Line 59: Filters out files after Before time

---

#### Filter By Date

**Lines 68-75**: `FilterByDate()` method

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

**Evidence**:
- Line 72: Parses timestamp from filename
- Line 73: Compares only date portion (ignores time)

---

## Data Flow Trace

### Example 1: `cycodj list --last 10`

1. **Parser** (`CycoDjCommandLineOptions.cs:124-134`)
   - Reads `--last 10`
   - Calls `ParseLastValue(command, "--last", "10")`

2. **ParseLastValue** (`CycoDjCommandLineOptions.cs:319-351`)
   - Calls `IsTimeSpec("10")` → returns `false` (no time units)
   - Parses as integer: `10`
   - Sets `command.Last = 10` via reflection

3. **ListCommand** (`ListCommand.cs:52`)
   - Calls `HistoryFileHelpers.FindAllHistoryFiles()`
   - Gets all history files

4. **ListCommand** (`ListCommand.cs:98-115`)
   - `effectiveLimit = Last = 10`
   - Skips default limit (Last is already set)
   - Takes first 10 files from sorted list

---

### Example 2: `cycodj list --last 7d`

1. **Parser** (`CycoDjCommandLineOptions.cs:124-134`)
   - Reads `--last 7d`
   - Calls `ParseLastValue(command, "--last", "7d")`

2. **ParseLastValue** (`CycoDjCommandLineOptions.cs:319-351`)
   - Calls `IsTimeSpec("7d")` → returns `true` (has 'd' unit)
   - Converts `7d` → `-7d..` (7 days ago to now)
   - Calls `TimeSpecHelpers.ParseTimeSpecRange("--last", "-7d..")`

3. **TimeSpecHelpers** (`TimeSpecHelpers.cs:7-42`)
   - Parses range: `-7d` to empty string
   - Calls `ParseSingleTimeSpec("--last", "-7d", isAfter: true)`
   - Returns `(7 days ago, now)`
   - Sets `command.After = 7 days ago`, `command.Before = now`

4. **ListCommand** (`ListCommand.cs:62-80`)
   - Calls `HistoryFileHelpers.FilterByDateRange(files, After, Before)`
   - Filters to files within last 7 days

---

### Example 3: `cycodj list --today`

1. **Parser** (`CycoDjCommandLineOptions.cs:142-147`)
   - Reads `--today`
   - Sets `command.After = DateTime.Today` (today 00:00:00)
   - Sets `command.Before = DateTime.Now`

2. **ListCommand** (`ListCommand.cs:62-80`)
   - Calls `HistoryFileHelpers.FilterByDateRange(files, After, Before)`
   - Filters to files from today

---

### Example 4: `cycodj list --date "2024-01-15"`

1. **Parser** (`CycoDjCommandLineOptions.cs:113-123`)
   - Reads `--date "2024-01-15"`
   - Sets `command.Date = "2024-01-15"`

2. **ListCommand** (`ListCommand.cs:82-95`)
   - Parses `Date` as DateTime: `2024-01-15 00:00:00`
   - Calls `HistoryFileHelpers.FilterByDate(files, 2024-01-15)`
   - Filters to files from that calendar date

---

### Example 5: `cycodj list` (no options)

1. **Parser**
   - No time-related options parsed
   - `command.After = null`, `command.Before = null`, `command.Date = null`, `command.Last = 0`

2. **ListCommand** (`ListCommand.cs:97-104`)
   - `effectiveLimit = 0`
   - Condition true: no filters specified
   - Sets `effectiveLimit = 20` (default)

3. **ListCommand** (`ListCommand.cs:106-115`)
   - Takes first 20 files from sorted list

---

## Summary

This proof document demonstrates:

1. **Command line parsing** converts user input to command properties
2. **Smart --last detection** automatically determines count vs time mode
3. **Time parsing** handles multiple formats (keywords, relative, absolute, ranges)
4. **Default behavior** applies sensible 20-conversation limit when no filters specified
5. **File filtering** uses timestamp parsing to filter history files
6. **Priority order**: After/Before → Date → Default limit

All claims in the Layer 1 documentation are backed by source code evidence with exact line numbers and data flow tracing.
