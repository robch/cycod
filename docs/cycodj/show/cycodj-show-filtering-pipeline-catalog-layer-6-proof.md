# cycodj show - Layer 6: Display Control - PROOF

## Parser Evidence

### Option Parsing: `--show-tool-calls`

**File**: `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs`

**Lines 325-331**: Parsing `--show-tool-calls` option
```csharp
if (arg == "--show-tool-calls")
{
    command.ShowToolCalls = true;
    return true;
}
```

**Note**: Parsed in `TryParseShowCommandOptions()` method

### Option Parsing: `--show-tool-output`

**File**: `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs`

**Lines 332-338**: Parsing `--show-tool-output` option
```csharp
else if (arg == "--show-tool-output")
{
    command.ShowToolOutput = true;
    return true;
}
```

### Option Parsing: `--max-content-length`

**File**: `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs`

**Lines 339-348**: Parsing `--max-content-length` option
```csharp
else if (arg == "--max-content-length")
{
    var length = i + 1 < args.Length ? args[++i] : null;
    if (string.IsNullOrWhiteSpace(length) || !int.TryParse(length, out var n))
    {
        throw new CommandLineException($"Missing or invalid length for {arg}");
    }
    command.MaxContentLength = n;
    return true;
}
```

### Option Parsing: `--stats`

**File**: `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs`

**Lines 156-161**: Parsing `--stats` option (shared across commands)
```csharp
// --stats
else if (arg == "--stats")
{
    SetShowStats(command, true);
    return true;
}
```

---

## Property Evidence

### ShowCommand Properties

**File**: `src/cycodj/CommandLineCommands/ShowCommand.cs`

**Lines 12-16**: Property declarations
```csharp
public string ConversationId { get; set; } = string.Empty;
public bool ShowToolCalls { get; set; } = false;
public bool ShowToolOutput { get; set; } = false;
public int MaxContentLength { get; set; } = 500;
public bool ShowStats { get; set; } = false;
```

**Key Points**:
- `ShowToolCalls` defaults to `false` (line 13)
- `ShowToolOutput` defaults to `false` (line 14)
- `MaxContentLength` defaults to `500` (line 15)
- `ShowStats` defaults to `false` (line 16)

---

## Execution Evidence

### System Message Visibility Control

**File**: `src/cycodj/CommandLineCommands/ShowCommand.cs`

**Lines 139-145**: System message handling
```csharp
// Skip system messages unless verbose
if (msg.Role == "system" && !ConsoleHelpers.IsVerbose())
{
    sb.AppendLine($"[{messageNumber}] {msg.Role} (system prompt - use --verbose to show)");
    sb.AppendLine();
    continue;
}
```

**Analysis**:
- Line 140: Checks global `--verbose` flag (not command-specific)
- Line 142: Shows placeholder for system messages
- Line 144: Skips full system message content unless verbose

### Content Truncation Control

**File**: `src/cycodj/CommandLineCommands/ShowCommand.cs`

**Lines 151-173**: Content truncation implementation
```csharp
// Message content
var content = msg.Content ?? string.Empty;

// Limit content length for tool outputs unless ShowToolOutput is enabled
if (msg.Role == "tool" && !ShowToolOutput && content.Length > MaxContentLength)
{
    var truncated = content.Substring(0, MaxContentLength);
    sb.AppendLine(truncated);
    sb.AppendLine($"... (truncated {content.Length - MaxContentLength} chars, use --show-tool-output to see all)");
}
else
{
    // Limit other messages too for readability
    if (content.Length > MaxContentLength * 3)
    {
        var truncated = content.Substring(0, MaxContentLength * 3);
        sb.AppendLine(truncated);
        sb.AppendLine($"... (truncated {content.Length - MaxContentLength * 3} chars)");
    }
    else
    {
        sb.AppendLine(content);
    }
}
```

**Analysis**:
- Lines 154-159: Tool message truncation
  - Line 154: Checks `msg.Role == "tool"` AND `!ShowToolOutput`
  - Truncates to `MaxContentLength` (default 500)
  - Line 158: Shows helpful message about `--show-tool-output`
- Lines 163-167: Other message truncation
  - Truncates to `MaxContentLength * 3` (default 1500)
  - Line 166: Shows truncation message without hint
- Lines 169-171: Full content display
  - When content fits within limits

### Tool Call Display Control

**File**: `src/cycodj/CommandLineCommands/ShowCommand.cs`

**Lines 175-183**: Tool call information display
```csharp
// Show tool calls if enabled
if (ShowToolCalls && msg.ToolCalls != null && msg.ToolCalls.Count > 0)
{
    sb.AppendLine($"Tool Calls: {msg.ToolCalls.Count}");
    foreach (var toolCall in msg.ToolCalls)
    {
        sb.AppendLine($"  - {toolCall.Id}: {toolCall.Function?.Name ?? "unknown"}");
    }
}
```

**Analysis**:
- Line 176: Guarded by `ShowToolCalls && msg.ToolCalls != null && msg.ToolCalls.Count > 0`
- Line 178: Shows count of tool calls
- Lines 179-182: Lists each tool call with ID and function name

### Tool Call ID Reference

**File**: `src/cycodj/CommandLineCommands/ShowCommand.cs`

**Lines 185-189**: Tool response reference display
```csharp
// Show tool call ID for tool responses
if (msg.Role == "tool" && !string.IsNullOrEmpty(msg.ToolCallId))
{
    sb.AppendLine($"(responding to: {msg.ToolCallId})");
}
```

**Analysis**:
- Always displayed for tool messages (not conditional on ShowToolCalls)
- Shows which tool call this response corresponds to

### Statistics Display Control

**File**: `src/cycodj/CommandLineCommands/ShowCommand.cs`

**Lines 197-226**: Statistics section implementation
```csharp
// Add statistics if requested
if (ShowStats)
{
    sb.AppendLine();
    sb.AppendLine("═══════════════════════════════════════");
    sb.AppendLine("## Conversation Statistics");
    sb.AppendLine("═══════════════════════════════════════");
    sb.AppendLine();
    
    var totalMessages = conv.Messages.Count;
    var totalUserMessages = conv.Messages.Count(m => m.Role == "user");
    var totalAssistantMessages = conv.Messages.Count(m => m.Role == "assistant");
    var totalToolMessages = conv.Messages.Count(m => m.Role == "tool");
    var toolCalls = conv.Messages.Where(m => m.ToolCalls != null).Sum(m => m.ToolCalls!.Count);
    
    sb.AppendLine($"Total messages: {totalMessages}");
    sb.AppendLine($"  User: {totalUserMessages} ({totalUserMessages * 100.0 / totalMessages:F1}%)");
    sb.AppendLine($"  Assistant: {totalAssistantMessages} ({totalAssistantMessages * 100.0 / totalMessages:F1}%)");
    sb.AppendLine($"  Tool: {totalToolMessages} ({totalToolMessages * 100.0 / totalMessages:F1}%)");
    sb.AppendLine();
    sb.AppendLine($"Tool calls: {toolCalls}");
    
    if (!string.IsNullOrEmpty(conv.ParentId))
    {
        sb.AppendLine($"This is a branch (parent: {conv.ParentId})");
    }
    
    sb.AppendLine();
    sb.AppendLine("═══════════════════════════════════════");
}
```

**Analysis**:
- Line 198: Guarded by `ShowStats` boolean
- Lines 201-203: Statistics header with separators
- Lines 206-210: Message count aggregation
- Lines 212-215: Message breakdown with percentages
- Line 217: Tool call count
- Lines 219-222: Branch information (if applicable)

### Header Display

**File**: `src/cycodj/CommandLineCommands/ShowCommand.cs`

**Lines 76-107**: Conversation header display
```csharp
// Display header
sb.AppendLine("═".PadRight(80, '═'));

if (!string.IsNullOrEmpty(conv.Metadata?.Title))
{
    sb.AppendLine($"## {conv.Metadata.Title}");
}
else
{
    sb.AppendLine($"## Conversation: {conv.Id}");
}

sb.AppendLine("═".PadRight(80, '═'));
sb.AppendLine();

// Display metadata
var timestamp = TimestampHelpers.FormatTimestamp(conv.Timestamp, "datetime");
sb.AppendLine($"Timestamp: {timestamp}");
sb.AppendLine($"File: {conv.FilePath}");
sb.AppendLine($"Messages: {conv.Messages.Count} total");

var userCount = conv.Messages.Count(m => m.Role == "user");
var assistantCount = conv.Messages.Count(m => m.Role == "assistant");
var toolCount = conv.Messages.Count(m => m.Role == "tool");
var systemCount = conv.Messages.Count(m => m.Role == "system");

sb.Append($"  - {userCount} user, {assistantCount} assistant, {toolCount} tool");
if (systemCount > 0)
{
    sb.Append($", {systemCount} system");
}
sb.AppendLine();
```

**Analysis**:
- Lines 77, 88: Header separators (80 characters wide)
- Lines 79-86: Title or conversation ID
- Lines 92-107: Metadata display (timestamp, file, message counts)

---

## Data Flow

### Call Stack for Display Control

```
ShowCommand.ExecuteAsync()
  ↓
ShowCommand.GenerateShowOutput()
  ↓
  Display header (lines 76-107)
  ↓
  For each message:
    Read ShowToolCalls property (line 176)
    Read ShowToolOutput property (line 154)
    Read MaxContentLength property (lines 154, 163)
    - Check system message visibility (line 140)
    - Apply content truncation (lines 154-173)
    - Display tool calls if enabled (lines 175-183)
    - Display tool call ID reference (lines 185-189)
  ↓
  Read ShowStats property (line 198)
  Conditionally append statistics (lines 197-226)
  ↓
  Return formatted string
```

### Property Flow

```
Command Line Args
  ↓
CycoDjCommandLineOptions.TryParseShowCommandOptions()
  ↓
Direct property assignment: ShowToolCalls, ShowToolOutput, MaxContentLength
SetShowStats() for stats
  ↓
ShowCommand properties
  ↓
ShowCommand.GenerateShowOutput() reads all properties
  ↓
Controls display formatting
```

---

## Summary

Layer 6 (Display Control) in the `show` command has **4 primary options**:

1. **`--show-tool-calls`**: Shows tool call details (IDs and function names)
2. **`--show-tool-output`**: Disables tool output truncation
3. **`--max-content-length <N>`**: Controls truncation threshold (default: 500)
4. **`--stats`**: Enables detailed statistics section

Plus one global option affecting show:
- **`--verbose`**: Shows full system messages (global CommandLineOptions flag)

Key characteristics:
- Most granular display control among all cycodj commands
- Focus on content verbosity management
- Separate controls for tool-related content
- Configurable truncation thresholds
- Helpful truncation messages guide users to appropriate options

The implementation provides fine-grained control over conversation display, particularly for tool-heavy conversations with large outputs.
