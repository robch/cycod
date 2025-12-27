# cycodj list - Layer 6: Display Control - PROOF

## Parser Evidence

### Option Parsing: `--messages`

**File**: `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs`

**Lines 121-138**: Parsing `--messages` option
```csharp
// --messages [N|all]
if (arg == "--messages")
{
    // Check if next arg is a value (not another option)
    if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
    {
        var value = args[++i];
        if (value.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            // Set to a large number (all messages)
            SetMessageCount(command, int.MaxValue);
        }
        else if (int.TryParse(value, out var count))
        {
            SetMessageCount(command, count);
        }
        else
        {
            throw new CommandLineException($"Invalid value for --messages: {value}");
        }
    }
```

**Lines 125-138** (continued): Default behavior when no value provided
```csharp
    else
    {
        // No value provided, set to null (use command default)
        SetMessageCount(command, null);
    }
    return true;
}
```

**Lines 201-207**: Helper method `SetMessageCount`
```csharp
private void SetMessageCount(CycoDjCommand command, int? value)
{
    var prop = command.GetType().GetProperty("MessageCount");
    if (prop != null)
    {
        prop.SetValue(command, value);
    }
}
```

### Option Parsing: `--stats`

**File**: `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs`

**Lines 142-148**: Parsing `--stats` option
```csharp
// --stats
else if (arg == "--stats")
{
    SetShowStats(command, true);
    return true;
}
```

**Lines 209-215**: Helper method `SetShowStats`
```csharp
private void SetShowStats(CycoDjCommand command, bool value)
{
    var prop = command.GetType().GetProperty("ShowStats");
    if (prop != null)
    {
        prop.SetValue(command, value);
    }
}
```

### Option Parsing: `--branches`

**File**: `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs`

**Lines 150-156**: Parsing `--branches` option
```csharp
// --branches (for list/search commands)
else if (arg == "--branches")
{
    SetShowBranches(command, true);
    return true;
}
```

**Lines 217-223**: Helper method `SetShowBranches`
```csharp
private void SetShowBranches(CycoDjCommand command, bool value)
{
    var prop = command.GetType().GetProperty("ShowBranches");
    if (prop != null)
    {
        prop.SetValue(command, value);
    }
}
```

---

## Property Evidence

### ListCommand Properties

**File**: `src/cycodj/CommandLineCommands/ListCommand.cs`

**Lines 12-16**: Property declarations
```csharp
public string? Date { get; set; }
public int Last { get; set; } = 0;
public bool ShowBranches { get; set; } = false;
public int? MessageCount { get; set; } = null; // null = use default (3)
public bool ShowStats { get; set; } = false;
```

**Key Points**:
- `MessageCount` is nullable: `null` means use default
- `ShowBranches` defaults to `false`
- `ShowStats` defaults to `false`

---

## Execution Evidence

### Message Preview Control

**File**: `src/cycodj/CommandLineCommands/ListCommand.cs`

**Lines 172-200**: Message preview implementation
```csharp
// Show preview - configurable number of messages
var messageCount = MessageCount ?? 3; // Default to 3 messages
var userMessages = conv.Messages.Where(m => m.Role == "user" && !string.IsNullOrWhiteSpace(m.Content)).ToList();

if (userMessages.Any() && messageCount > 0)
{
    // For branches, show last N messages (what's new)
    // For non-branches, show first N messages
    var messagesToShow = conv.ParentId != null 
        ? userMessages.TakeLast(Math.Min(messageCount, userMessages.Count))
        : userMessages.Take(Math.Min(messageCount, userMessages.Count));
    
    foreach (var msg in messagesToShow)
    {
        var preview = msg.Content.Length > 200 
            ? msg.Content.Substring(0, 200) + "..." 
            : msg.Content;
        preview = preview.Replace("\n", " ").Replace("\r", "");
        
        sb.AppendLine($"{indent}  > {preview}");
    }
    
    // Show indicator if there are more messages
    var shownCount = messagesToShow.Count();
    if (userMessages.Count > shownCount)
    {
        sb.AppendLine($"{indent}    ... and {userMessages.Count - shownCount} more");
    }
}
```

**Analysis**:
- Line 173: `MessageCount ?? 3` - Uses default of 3 if null
- Lines 178-182: Different behavior for branches vs. roots
- Lines 184-189: Truncate preview to 200 characters
- Lines 194-197: Show "... and X more" indicator

### Branch Display Control

**File**: `src/cycodj/CommandLineCommands/ListCommand.cs`

**Lines 137-170**: Branch information display
```csharp
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

sb.AppendLine($"{indent}  Messages: {userCount} user, {assistantCount} assistant, {toolCount} tool");

// Show branch info if ShowBranches is enabled
if (ShowBranches)
{
    if (conv.ParentId != null)
    {
        sb.AppendLine($"{indent}  Branch of: {conv.ParentId}");
    }
    if (conv.BranchIds.Count > 0)
    {
        sb.AppendLine($"{indent}  Branches: {conv.BranchIds.Count}");
    }
    if (conv.ToolCallIds.Count > 0)
    {
        sb.AppendLine($"{indent}  Tool calls: {conv.ToolCallIds.Count}");
    }
}
```

**Analysis**:
- Line 138: Indentation applied for branches (`  ↳ `)
- Lines 155-170: Branch details only shown if `ShowBranches` is true
- Lines 158-160: Parent ID display
- Lines 161-164: Child branches count
- Lines 165-168: Tool calls count

### Statistics Display Control

**File**: `src/cycodj/CommandLineCommands/ListCommand.cs`

**Lines 214-239**: Statistics section
```csharp
// Add statistics if requested
if (ShowStats && conversations.Any())
{
    sb.AppendLine();
    sb.AppendLine("═══════════════════════════════════════");
    sb.AppendLine("## Statistics Summary");
    sb.AppendLine("═══════════════════════════════════════");
    sb.AppendLine();
    
    var totalMessages = conversations.Sum(c => c.Messages.Count);
    var totalUserMessages = conversations.Sum(c => c.Messages.Count(m => m.Role == "user"));
    var totalAssistantMessages = conversations.Sum(c => c.Messages.Count(m => m.Role == "assistant"));
    var totalToolMessages = conversations.Sum(c => c.Messages.Count(m => m.Role == "tool"));
    var avgMessages = totalMessages / (double)conversations.Count;
    
    sb.AppendLine($"Total conversations: {conversations.Count}");
    sb.AppendLine($"Total messages: {totalMessages:N0}");
    sb.AppendLine($"  User: {totalUserMessages:N0} ({totalUserMessages * 100.0 / totalMessages:F1}%)");
    sb.AppendLine($"  Assistant: {totalAssistantMessages:N0} ({totalAssistantMessages * 100.0 / totalMessages:F1}%)");
    sb.AppendLine($"  Tool: {totalToolMessages:N0} ({totalToolMessages * 100.0 / totalMessages:F1}%)");
    sb.AppendLine();
    sb.AppendLine($"Average messages/conversation: {avgMessages:F1}");
    sb.AppendLine($"Branched conversations: {branchedConvs} ({branchedConvs * 100.0 / conversations.Count:F1}%)");
    sb.AppendLine();
    sb.AppendLine("═══════════════════════════════════════");
}
```

**Analysis**:
- Line 215: Guarded by `ShowStats && conversations.Any()`
- Lines 218-220: Statistics header with separator
- Lines 223-226: Aggregate message counts by role
- Lines 229-233: Message breakdown with percentages
- Line 235: Average messages calculation
- Line 236: Branch percentage

---

## Data Flow

### Call Stack for Display Control

```
ListCommand.ExecuteAsync()
  ↓
ListCommand.GenerateListOutput()
  ↓
  Read MessageCount property (line 173)
  Read ShowBranches property (line 155)
  Read ShowStats property (line 215)
  ↓
  For each conversation:
    - Apply indentation based on ParentId (line 138)
    - Format basic info (lines 140-153)
    - Conditionally show branch info (lines 155-170)
    - Conditionally show message preview (lines 172-200)
  ↓
  Conditionally append statistics (lines 214-239)
  ↓
  Return formatted string
```

### Property Flow

```
Command Line Args
  ↓
CycoDjCommandLineOptions.TryParseDisplayOptions()
  ↓
SetMessageCount() / SetShowStats() / SetShowBranches()
  ↓
Reflection: Set property on ListCommand instance
  ↓
ListCommand properties: MessageCount, ShowStats, ShowBranches
  ↓
ListCommand.GenerateListOutput() reads properties
  ↓
Controls display formatting
```

---

## Helper Methods

### Timestamp Formatting

**File**: `src/cycodj/CommandLineCommands/ListCommand.cs`

**Line 132**: Timestamp formatting
```csharp
var timestamp = TimestampHelpers.FormatTimestamp(conv.Timestamp, "datetime");
```

**Helper**: `TimestampHelpers.FormatTimestamp()`
- Formats DateTime to readable string
- "datetime" format: "yyyy-MM-dd HH:mm:ss"

### Branch Detection

**File**: `src/cycodj/CommandLineCommands/ListCommand.cs`

**Line 127**: Branch relationship detection
```csharp
BranchDetector.DetectBranches(conversations);
```

**Helper**: `BranchDetector.DetectBranches()`
- Populates `ParentId`, `BranchIds`, `ToolCallIds` properties
- Establishes parent-child relationships
- Required before branch display

---

## Summary

Layer 6 (Display Control) in the `list` command is controlled by three primary options:

1. **`--messages [N|all]`**: Controls message preview count (default: 3)
2. **`--stats`**: Enables detailed statistics section
3. **`--branches`**: Shows branch relationship information

All three are:
- Parsed by reflection-based helper methods
- Stored as properties on `ListCommand`
- Used to conditionally format output
- Independent and composable (can be used together)

The implementation provides flexible display control while maintaining clean separation between parsing (Layer 1-2), filtering (Layer 3-5), and presentation (Layer 6).
