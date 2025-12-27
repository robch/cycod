# cycodj list Command: Layer 2 - Container Filtering (PROOF)

This document provides **source code evidence** for all assertions made in [cycodj-list-layer-2.md](cycodj-list-layer-2.md).

---

## Assertion 1: No Explicit Container Filtering Options

**Claim**: The `list` command does not expose CLI options for filtering individual conversations based on properties or content.

### Proof 1.1: Command-Line Parser

**File**: `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs`  
**Method**: `TryParseListCommandOptions`  
**Lines**: 275-312

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

**Analysis**:
- Only parses: `--messages`, `--stats`, `--branches` (display options from `TryParseDisplayOptions`)
- Only parses: `--today`, `--yesterday`, `--after`, `--before`, `--date-range` (time options from `TryParseTimeOptions`)
- Only parses: `--date`, `--last` (target selection options)
- **NO OPTIONS** for content-based or property-based container filtering

### Proof 1.2: Command Properties

**File**: `src/cycodj/CommandLineCommands/ListCommand.cs`  
**Lines**: 10-16

```csharp
public class ListCommand : CycoDjCommand
{
    public string? Date { get; set; }
    public int Last { get; set; } = 0;
    public bool ShowBranches { get; set; } = false;
    public int? MessageCount { get; set; } = null; // null = use default (3)
    public bool ShowStats { get; set; } = false;
```

**Analysis**:
- `Date` and `Last`: Target selection (Layer 1)
- `ShowBranches`, `MessageCount`, `ShowStats`: Display options (Layer 6)
- **NO PROPERTIES** for container filtering (e.g., `TitleContains`, `MinMessages`, `HasToolCalls`)

---

## Assertion 2: Default Conversation Limit

**Claim**: When no filters are specified, the command defaults to showing the last 20 conversations.

### Proof 2.1: Default Limit Logic

**File**: `src/cycodj/CommandLineCommands/ListCommand.cs`  
**Method**: `GenerateListOutput`  
**Lines**: 97-104

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

**Analysis**:
- **Line 98**: `var effectiveLimit = Last;` - Start with user-specified limit
- **Line 99**: Check conditions:
  - `effectiveLimit == 0`: User didn't specify `--last N`
  - `!After.HasValue`: No `--after` or `--today` specified
  - `!Before.HasValue`: No `--before` specified
  - `string.IsNullOrEmpty(Date)`: No `--date` specified
- **Line 101**: If all conditions true, set `effectiveLimit = 20`
- **Line 102**: Display informational message to user

### Proof 2.2: Where After/Before Come From

**File**: `src/cycodj/CommandLine/CycoDjCommand.cs`  
**Lines**: 12-14

```csharp
// Common properties for time filtering
public DateTime? After { get; set; }
public DateTime? Before { get; set; }
```

**Analysis**:
- These properties are set by Layer 1 time filtering options
- They are checked in Layer 2 to determine if explicit filtering was requested

---

## Assertion 3: Explicit Result Limiting

**Claim**: If `--last N` was specified (as a conversation count), limit results to N conversations.

### Proof 3.1: Applying the Limit

**File**: `src/cycodj/CommandLineCommands/ListCommand.cs`  
**Method**: `GenerateListOutput`  
**Lines**: 106-115

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

**Analysis**:
- **Line 107**: Check if limit needed: `effectiveLimit > 0` AND `files.Count > effectiveLimit`
- **Line 109**: Apply limit using `Take(effectiveLimit)`
- **Lines 110-113**: If user explicitly specified `--last N` (not defaulted), show message

### Proof 3.2: Parsing --last as Count

**File**: `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs`  
**Method**: `ParseLastValue`  
**Lines**: 554-594

```csharp
private void ParseLastValue(CycoDjCommand command, string arg, string value)
{
    // Smart detection: TIMESPEC vs conversation count
    if (IsTimeSpec(value))
    {
        // ... (parse as time specification, sets After/Before)
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

**Analysis**:
- **Line 557**: Detects if value is a time spec (e.g., "7d") or a count (e.g., "10")
- **Lines 579-593**: If NOT a time spec, parse as integer count
- **Line 590**: Set `Last` property on command using reflection

### Proof 3.3: IsTimeSpec Logic

**File**: `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs`  
**Method**: `IsTimeSpec`  
**Lines**: 599-616

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

**Analysis**:
- A pure number (e.g., "20") returns `false` (NOT a time spec)
- Therefore, "20" is parsed as a conversation count in `ParseLastValue`

---

## Assertion 4: No Content-Based or Property-Based Filtering

**Claim**: The `list` command does not filter conversations based on their title, content, message count, or other properties.

### Proof 4.1: Conversation Reading

**File**: `src/cycodj/CommandLineCommands/ListCommand.cs`  
**Method**: `GenerateListOutput`  
**Lines**: 117-128

```csharp
// Read and display conversations
var conversations = JsonlReader.ReadConversations(files);

if (conversations.Count == 0)
{
    sb.AppendLine("WARNING: No conversations could be read");
    return sb.ToString();
}

// Detect branches
BranchDetector.DetectBranches(conversations);

// Display conversations
foreach (var conv in conversations)
```

**Analysis**:
- **Line 118**: Read ALL conversations from the file list
- **Line 127**: Detect branches for ALL conversations
- **Line 130**: Display ALL conversations (no filtering loop)
- **NO CODE** between reading and displaying that filters conversations

### Proof 4.2: Display Loop Has No Filtering

**File**: `src/cycodj/CommandLineCommands/ListCommand.cs`  
**Method**: `GenerateListOutput`  
**Lines**: 130-203

```csharp
// Display conversations
foreach (var conv in conversations)
{
    var timestamp = TimestampHelpers.FormatTimestamp(conv.Timestamp, "datetime");
    var userCount = conv.Messages.Count(m => m.Role == "user");
    var assistantCount = conv.Messages.Count(m => m.Role == "assistant");
    var toolCount = conv.Messages.Count(m => m.Role == "tool");
    
    // Show indent if this is a branch
    var indent = conv.ParentId != null ? "  ↳ " : "";
    
    // Show timestamp and title
    sb.Append($"{indent}{timestamp} - ");
    
    if (!string.IsNullOrEmpty(conv.Metadata?.Title))
    {
        sb.Append($"{conv.Metadata.Title} ");
        sb.AppendLine($"({conv.Id})");
    }
    else
    {
        sb.AppendLine($"{conv.Id}");
    }
    
    // ... (rest of display logic)
    
    sb.AppendLine();
}
```

**Analysis**:
- Simple `foreach` loop over `conversations`
- No `if` conditions that skip conversations
- Every conversation in the list is displayed
- **NO FILTERING** at the conversation level

---

## Assertion 5: Data Flow

**Claim**: The data flow is: Layer 1 Output → Check limits → Apply limit → Read conversations → Layer 3

### Proof 5.1: Method Execution Order

**File**: `src/cycodj/CommandLineCommands/ListCommand.cs`  
**Method**: `GenerateListOutput`  
**Lines**: 52-130 (key sections)

```csharp
// Line 52: Find all history files
var files = HistoryFileHelpers.FindAllHistoryFiles();

// Lines 62-95: Layer 1 filtering (time-based file filtering)

// Lines 97-104: Layer 2 - Apply default limit
var effectiveLimit = Last;
if (effectiveLimit == 0 && !After.HasValue && !Before.HasValue && string.IsNullOrEmpty(Date))
{
    effectiveLimit = 20;
    // ...
}

// Lines 106-115: Layer 2 - Apply explicit limit
if (effectiveLimit > 0 && files.Count > effectiveLimit)
{
    files = files.Take(effectiveLimit).ToList();
    // ...
}

// Lines 117-127: Read conversations (transition to Layer 3)
var conversations = JsonlReader.ReadConversations(files);
// ...
BranchDetector.DetectBranches(conversations);

// Lines 130+: Display loop (Layer 3 content filtering happens here for message previews)
foreach (var conv in conversations)
```

**Analysis**:
- Clear sequential execution
- Layer 1 (time filtering) → Layer 2 (result limiting) → Layer 3 (read and display)
- No intermingling of layers

---

## Summary of Evidence

| Assertion | Evidence Files | Key Lines |
|-----------|---------------|-----------|
| No explicit filtering options | `CycoDjCommandLineOptions.cs` | 275-312 |
| No filtering properties | `ListCommand.cs` | 10-16 |
| Default limit of 20 | `ListCommand.cs` | 97-104 |
| Explicit --last N limit | `ListCommand.cs` | 106-115 |
| --last parsing as count | `CycoDjCommandLineOptions.cs` | 579-593 |
| No content/property filtering | `ListCommand.cs` | 117-128, 130-203 |
| Sequential data flow | `ListCommand.cs` | 52-130 |

All assertions in the Layer 2 documentation are **proven** by the source code evidence above.

---

[← Back to Layer 2 Documentation](cycodj-list-layer-2.md)
