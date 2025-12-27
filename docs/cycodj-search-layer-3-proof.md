# cycodj search Command - Layer 3: Content Filtering - PROOF

## Source Code Evidence

This document provides detailed source code evidence for Layer 3 (Content Filtering) implementation in the `search` command.

---

## 1. Command-Line Option Parsing

### 1.1 Query (Positional Argument)

**File**: `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs`

**Lines 308-311**:
```csharp
override protected bool TryParseOtherCommandArg(Command? command, string arg)
{
    if (command is SearchCommand searchCommand)
    {
        searchCommand.Query = arg;
        return true;
    }

    return false;
}
```

**Evidence**: The positional argument is assigned to `SearchCommand.Query`.

---

### 1.2 Search Options Parsing

**File**: `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs`

**Lines 95-187** (method: `TryParseSearchCommandOptions`):

#### Case-Sensitive Option
**Lines 117-121**:
```csharp
else if (arg == "--case-sensitive" || arg == "-c")
{
    command.CaseSensitive = true;
    return true;
}
```

#### Regex Option
**Lines 122-126**:
```csharp
else if (arg == "--regex" || arg == "-r")
{
    command.UseRegex = true;
    return true;
}
```

#### User-Only Option
**Lines 127-131**:
```csharp
else if (arg == "--user-only" || arg == "-u")
{
    command.UserOnly = true;
    return true;
}
```

#### Assistant-Only Option
**Lines 132-136**:
```csharp
else if (arg == "--assistant-only" || arg == "-a")
{
    command.AssistantOnly = true;
    return true;
}
```

**Evidence**: All Layer 3 filtering options are parsed with their short aliases.

---

## 2. SearchCommand Class Properties

**File**: `src/cycodj/CommandLineCommands/SearchCommand.cs`

**Lines 10-20**:
```csharp
public class SearchCommand : CommandLine.CycoDjCommand
{
    public string? Query { get; set; }
    public string? Date { get; set; }
    public int? Last { get; set; }
    public bool CaseSensitive { get; set; }
    public bool UseRegex { get; set; }
    public bool UserOnly { get; set; }
    public bool AssistantOnly { get; set; }
    public int ContextLines { get; set; } = 2;
    public bool ShowBranches { get; set; } = false;
    public int? MessageCount { get; set; } = null;
    public bool ShowStats { get; set; } = false;
```

**Evidence**: 
- `Query` stores the search pattern (line 10)
- `CaseSensitive` flag (line 13)
- `UseRegex` flag (line 14)
- `UserOnly` flag (line 15)
- `AssistantOnly` flag (line 16)

---

## 3. Query Validation

**File**: `src/cycodj/CommandLineCommands/SearchCommand.cs`

**Lines 46-50** (method: `GenerateSearchOutput`):
```csharp
if (string.IsNullOrWhiteSpace(Query))
{
    sb.AppendLine("ERROR: Search query is required.");
    return sb.ToString();
}
```

**Evidence**: The search command requires a non-empty query. If missing, it returns an error message.

---

## 4. Conversation-Level Search Orchestration

**File**: `src/cycodj/CommandLineCommands/SearchCommand.cs`

**Lines 114-132** (method: `GenerateSearchOutput`):
```csharp
foreach (var file in files)
{
    try
    {
        var conversation = CycoDj.Helpers.JsonlReader.ReadConversation(file);
        if (conversation != null)
        {
            var conversationMatches = SearchConversation(conversation);
            if (conversationMatches.Any())
            {
                matches.Add((conversation, conversationMatches));
            }
        }
    }
    catch (Exception ex)
    {
        Logger.Warning($"Failed to search conversation {file}: {ex.Message}");
    }
}
```

**Evidence**: 
- Each conversation file is processed individually
- Only conversations with matches are added to results
- Exceptions are logged but don't stop the search

---

## 5. Message Filtering by Role

**File**: `src/cycodj/CommandLineCommands/SearchCommand.cs`

**Lines 188-220** (method: `SearchConversation`):

### Full Method:
```csharp
private List<SearchMatch> SearchConversation(Models.Conversation conversation)
{
    var matches = new List<SearchMatch>();
    
    for (int i = 0; i < conversation.Messages.Count; i++)
    {
        var message = conversation.Messages[i];
        
        // Filter by role if specified
        if (UserOnly && message.Role != "user") continue;
        if (AssistantOnly && message.Role != "assistant") continue;
        
        // Skip system messages unless explicitly searching all
        if (message.Role == "system" && (UserOnly || AssistantOnly)) continue;

        // Search in message content
        if (!string.IsNullOrWhiteSpace(message.Content))
        {
            var messageMatches = SearchText(message.Content);
            if (messageMatches.Any())
            {
                matches.Add(new SearchMatch
                {
                    MessageIndex = i,
                    Message = message,
                    MatchedLines = messageMatches
                });
            }
        }
    }

    return matches;
}
```

### Role Filter Logic (Lines 196-201):
```csharp
// Filter by role if specified
if (UserOnly && message.Role != "user") continue;
if (AssistantOnly && message.Role != "assistant") continue;

// Skip system messages unless explicitly searching all
if (message.Role == "system" && (UserOnly || AssistantOnly)) continue;
```

**Evidence**:
- `UserOnly` skips all non-user messages (line 196)
- `AssistantOnly` skips all non-assistant messages (line 197)
- System messages are skipped when role filtering is active (line 200)
- Messages with null/whitespace content are skipped (line 203)

---

## 6. Text/Pattern Matching Implementation

**File**: `src/cycodj/CommandLineCommands/SearchCommand.cs`

**Lines 222-262** (method: `SearchText`):

### Full Method:
```csharp
private List<(int lineNumber, string line, int matchStart, int matchLength)> SearchText(string text)
{
    var matches = new List<(int lineNumber, string line, int matchStart, int matchLength)>();
    var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.None);

    for (int lineNum = 0; lineNum < lines.Length; lineNum++)
    {
        var line = lines[lineNum];
        if (string.IsNullOrEmpty(line)) continue;

        if (UseRegex)
        {
            try
            {
                var regexOptions = CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
                var regex = new Regex(Query!, regexOptions);
                var match = regex.Match(line);
                if (match.Success)
                {
                    matches.Add((lineNum, line, match.Index, match.Length));
                }
            }
            catch (Exception ex)
            {
                ConsoleHelpers.WriteErrorLine($"Invalid regex pattern: {ex.Message}");
                return matches;
            }
        }
        else
        {
            var comparison = CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
            var index = line.IndexOf(Query!, comparison);
            if (index >= 0)
            {
                matches.Add((lineNum, line, index, Query!.Length));
            }
        }
    }

    return matches;
}
```

### Line Splitting (Lines 224-225):
```csharp
var matches = new List<(int lineNumber, string line, int matchStart, int matchLength)>();
var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.None);
```

**Evidence**: Message content is split into lines for line-by-line searching.

### Empty Line Handling (Lines 229-230):
```csharp
var line = lines[lineNum];
if (string.IsNullOrEmpty(line)) continue;
```

**Evidence**: Empty lines are skipped from search.

### Regex Matching (Lines 232-248):
```csharp
if (UseRegex)
{
    try
    {
        var regexOptions = CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
        var regex = new Regex(Query!, regexOptions);
        var match = regex.Match(line);
        if (match.Success)
        {
            matches.Add((lineNum, line, match.Index, match.Length));
        }
    }
    catch (Exception ex)
    {
        ConsoleHelpers.WriteErrorLine($"Invalid regex pattern: {ex.Message}");
        return matches;
    }
}
```

**Evidence**:
- Regex options depend on `CaseSensitive` flag (line 236)
- Match position and length are captured (line 241)
- Invalid regex patterns are handled gracefully (lines 244-247)

### Literal String Matching (Lines 250-256):
```csharp
else
{
    var comparison = CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
    var index = line.IndexOf(Query!, comparison);
    if (index >= 0)
    {
        matches.Add((lineNum, line, index, Query!.Length));
    }
}
```

**Evidence**:
- String comparison type depends on `CaseSensitive` flag (line 252)
- Match position and length are captured (line 255)

---

## 7. Match Data Structure

**File**: `src/cycodj/CommandLineCommands/SearchCommand.cs`

**Lines 300-305** (internal class):
```csharp
private class SearchMatch
{
    public int MessageIndex { get; set; }
    public Models.ChatMessage Message { get; set; } = null!;
    public List<(int lineNumber, string line, int matchStart, int matchLength)> MatchedLines { get; set; } = new();
}
```

**Evidence**:
- `MessageIndex`: Position of message in conversation (line 302)
- `Message`: Full message object (line 303)
- `MatchedLines`: List of matched lines with position information (line 304)

The tuple structure for `MatchedLines` includes:
- `lineNumber`: 0-based line number within message
- `line`: Full line content
- `matchStart`: Character index where match starts
- `matchLength`: Length of matched text

---

## 8. Integration with Display Layer (Layer 6)

**File**: `src/cycodj/CommandLineCommands/SearchCommand.cs`

**Lines 264-298** (method: `AppendConversationMatches`):
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

**Evidence**:
- Layer 3 match data (`match.MatchedLines`) is used by Layer 6 for display (line 282)
- Matched line numbers are extracted for highlighting (line 282)
- Context expansion uses `ContextLines` property (line 286 - Layer 5)

---

## 9. Match Result Summary

**File**: `src/cycodj/CommandLineCommands/SearchCommand.cs`

**Lines 149-151**:
```csharp
sb.AppendLine();
sb.AppendLine($"Total: {matches.Sum(m => m.searchMatches.Count)} match(es) in {matches.Count} conversation(s)");
```

**Evidence**: Total match count is calculated by summing matches across all conversations.

---

## 10. Statistics Integration (Layer 6)

**File**: `src/cycodj/CommandLineCommands/SearchCommand.cs`

**Lines 152-183** (when `--stats` is enabled):
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
    // ... more stats
}
```

**Evidence**: Statistics show how many conversations/messages were filtered by Layer 3.

---

## Summary

### Layer 3 Implementation Completeness

| Feature | Status | Evidence |
|---------|--------|----------|
| Query requirement | ✅ Complete | Lines 46-50 |
| Role filtering | ✅ Complete | Lines 196-200 |
| Case-sensitive search | ✅ Complete | Lines 236, 252 |
| Regex pattern matching | ✅ Complete | Lines 232-248 |
| Literal string matching | ✅ Complete | Lines 250-256 |
| Line-by-line search | ✅ Complete | Lines 225-230 |
| Empty line skipping | ✅ Complete | Line 230 |
| Match position tracking | ✅ Complete | Lines 241, 255 |
| Error handling (regex) | ✅ Complete | Lines 244-247 |
| Message content validation | ✅ Complete | Line 203 |

### Data Flow Verification

```
[Parsing] CycoDjCommandLineOptions.TryParseSearchCommandOptions (lines 95-187)
    ↓
[Properties] SearchCommand properties (lines 10-16)
    ↓
[Validation] Query validation (lines 46-50)
    ↓
[Orchestration] GenerateSearchOutput (lines 114-132)
    ↓
[Filtering] SearchConversation (lines 188-220)
    ├─ Role filter (lines 196-200)
    └─ Content search (lines 203-217)
        ↓
    [Matching] SearchText (lines 222-262)
    ├─ Regex (lines 232-248)
    └─ Literal (lines 250-256)
        ↓
    [Results] SearchMatch data structure (lines 300-305)
        ↓
    [Display] AppendConversationMatches (lines 264-298) [Layer 6]
```

### Source Files Referenced

1. **`src/cycodj/CommandLineCommands/SearchCommand.cs`**: Primary implementation (308 lines)
2. **`src/cycodj/CommandLine/CycoDjCommandLineOptions.cs`**: Option parsing (lines 95-187, 308-311)

### Line Number Summary

- **Option Parsing**: Lines 95-187, 308-311 (CycoDjCommandLineOptions.cs)
- **Properties**: Lines 10-16 (SearchCommand.cs)
- **Query Validation**: Lines 46-50 (SearchCommand.cs)
- **Conversation Search**: Lines 114-132 (SearchCommand.cs)
- **Message Filtering**: Lines 188-220 (SearchCommand.cs)
- **Text Matching**: Lines 222-262 (SearchCommand.cs)
- **Data Structure**: Lines 300-305 (SearchCommand.cs)
- **Display Integration**: Lines 264-298 (SearchCommand.cs)

---

**Verification Date**: 2025-06-XX  
**Source Code Version**: Current HEAD  
**Total Lines Analyzed**: 421 lines across 2 files
