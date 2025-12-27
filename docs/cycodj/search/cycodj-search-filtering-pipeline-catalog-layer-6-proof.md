# cycodj search - Layer 6: Display Control - PROOF

## Parser Evidence

### Option Parsing: `--branches`

**File**: `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs`

**Lines 163-168**: Parsing `--branches` option (shared with list command)
```csharp
// --branches (for list/search commands)
else if (arg == "--branches")
{
    SetShowBranches(command, true);
    return true;
}
```

**Lines 203-209**: Helper method `SetShowBranches`
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

**Lines 194-200**: Helper method `SetShowStats`
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

---

## Property Evidence

### SearchCommand Properties

**File**: `src/cycodj/CommandLineCommands/SearchCommand.cs`

**Lines 10-20**: Property declarations
```csharp
public string? Query { get; set; }
public string? Date { get; set; }
public int? Last { get; set; }
public bool CaseSensitive { get; set; }
public bool UseRegex { get; set; }
public bool UserOnly { get; set; }
public bool AssistantOnly { get; set; }
public int ContextLines { get; set; } = 2;
public bool ShowBranches { get; set; } = false;
public int? MessageCount { get; set; } = null; // null = use default (3)
public bool ShowStats { get; set; } = false;
```

**Key Points**:
- `ShowBranches` defaults to `false` (line 18)
- `ShowStats` defaults to `false` (line 20)
- `MessageCount` is present but not used in display (line 19)

---

## Execution Evidence

### Match Display Format

**File**: `src/cycodj/CommandLineCommands/SearchCommand.cs`

**Lines 264-298**: Match formatting implementation
```csharp
private void AppendConversationMatches(System.Text.StringBuilder sb, Models.Conversation conversation, List<SearchMatch> matches)
{
    var title = conversation.Metadata?.Title ?? $"conversation-{conversation.Id}";
    var timestamp = conversation.Timestamp.ToString("yyyy-MM-dd HH:mm:ss");

    sb.AppendLine($"### {timestamp} - {title}");
    sb.AppendLine($"    File: {conversation.FilePath}");
    sb.AppendLine($"    Matches: {matches.Count}");
    sb.AppendLine();

    foreach (var match in matches)
    {
        var role = match.Message.Role;
        sb.AppendLine($"  [{role}] Message #{match.MessageIndex + 1}");

        // Show matched lines with context
        var allLines = match.Message.Content.Split(new[] { '\r', '\n' }, StringSplitOptions.None);
        var matchedLineNumbers = match.MatchedLines.Select(m => m.lineNumber).Distinct().ToHashSet();

        for (int i = 0; i < allLines.Length; i++)
        {
            var isMatch = matchedLineNumbers.Contains(i);
            var showContext = matchedLineNumbers.Any(ln => Math.Abs(ln - i) <= ContextLines);

            if (showContext || isMatch)
            {
                var prefix = isMatch ? "  > " : "    ";
                var line = allLines[i];
                sb.AppendLine(prefix + line);
            }
        }

        sb.AppendLine();
    }
}
```

**Analysis**:
- Lines 266-268: Conversation header with title and timestamp
- Lines 269-270: File path and match count
- Line 275: Message role and number display
- Lines 283-291: Match vs. context line differentiation
  - Line 289: Matched lines use `"  > "` prefix
  - Line 289: Context lines use `"    "` (4 spaces) prefix

### Statistics Display

**File**: `src/cycodj/CommandLineCommands/SearchCommand.cs`

**Lines 152-183**: Statistics section implementation
```csharp
// Add statistics if requested
if (ShowStats)
{
    var conversations = matches.Select(m => m.conversation).ToList();
    
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
    var branchCount = conversations.Count(c => c.ParentId != null);
    
    sb.AppendLine($"Total conversations searched: {files.Count}");
    sb.AppendLine($"Conversations with matches: {conversations.Count}");
    sb.AppendLine($"Total matches: {matches.Sum(m => m.searchMatches.Count)}");
    sb.AppendLine();
    sb.AppendLine($"Total messages: {totalMessages:N0}");
    sb.AppendLine($"  User: {totalUserMessages:N0} ({totalUserMessages * 100.0 / totalMessages:F1}%)");
    sb.AppendLine($"  Assistant: {totalAssistantMessages:N0} ({totalAssistantMessages * 100.0 / totalMessages:F1}%)");
    sb.AppendLine($"  Tool: {totalToolMessages:N0} ({totalToolMessages * 100.0 / totalMessages:F1}%)");
    sb.AppendLine();
    sb.AppendLine($"Average messages/conversation: {avgMessages:F1}");
    sb.AppendLine($"Branched conversations: {branchCount} ({branchCount * 100.0 / conversations.Count:F1}%)");
    sb.AppendLine();
    sb.AppendLine("═══════════════════════════════════════");
}
```

**Analysis**:
- Line 153: Guarded by `ShowStats` boolean
- Lines 158-161: Statistics header with separators
- Lines 163-168: Message count aggregation by role
- Lines 170-172: Search-specific metrics:
  - Total conversations searched
  - Conversations with matches (subset)
  - Total match count
- Lines 174-180: Standard conversation statistics

### Result Summary Display

**File**: `src/cycodj/CommandLineCommands/SearchCommand.cs`

**Lines 149-150**: Match summary
```csharp
sb.AppendLine();
sb.AppendLine($"Total: {matches.Sum(m => m.searchMatches.Count)} match(es) in {matches.Count} conversation(s)");
```

**Analysis**:
- Always displayed (not conditional)
- Shows aggregate match count and conversation count

---

## Data Flow

### Call Stack for Display Control

```
SearchCommand.ExecuteAsync()
  ↓
SearchCommand.GenerateSearchOutput()
  ↓
  Read ShowStats property (line 153)
  ↓
  For each conversation with matches:
    - AppendConversationMatches() (line 146)
      - Format conversation header (lines 266-270)
      - For each match:
        - Format message role/number (line 275)
        - Format matched lines with ">" prefix (lines 283-291)
  ↓
  Show match summary (line 150)
  ↓
  Conditionally append statistics (lines 152-183)
  ↓
  Return formatted string
```

### Property Flow

```
Command Line Args
  ↓
CycoDjCommandLineOptions.TryParseDisplayOptions()
  ↓
SetShowStats() / SetShowBranches()
  ↓
Reflection: Set property on SearchCommand instance
  ↓
SearchCommand properties: ShowStats, ShowBranches
  ↓
SearchCommand.GenerateSearchOutput() reads ShowStats
  ↓
Controls statistics display
```

---

## Differences from LIST Command

### Similar Options
- `--stats`: Same implementation pattern
- `--branches`: Inherited property but not actively used in display

### Different Display Logic
1. **No message preview control**: Search shows matched content, not previews
2. **Match-specific formatting**: Uses `>` prefix for matched lines
3. **Context-aware display**: Shows context lines around matches (Layer 5)
4. **Search-specific statistics**: Includes "conversations searched" and "total matches"

---

## Summary

Layer 6 (Display Control) in the `search` command has **2 primary options**:

1. **`--stats`**: Enables detailed statistics section with search-specific metrics
2. **`--branches`**: Inherited property (not actively used in search display)

Key characteristics:
- Simpler than LIST command (no message count control)
- Focus on match presentation with context
- Statistics include search-specific metrics
- Match vs. context differentiation with prefixes

The implementation provides clear, hierarchical display of search results with optional statistical analysis.
