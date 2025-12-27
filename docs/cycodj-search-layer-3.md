# cycodj search Command - Layer 3: Content Filtering

## Overview

**Layer 3: Content Filtering** determines what content WITHIN selected conversation containers should be shown or highlighted. For the `search` command, this layer is richly implemented with multiple filtering dimensions.

## Purpose

The search command's Layer 3 implementation filters message-level content based on:
1. **Text/Pattern Matching**: Search query (text or regex)
2. **Role Filtering**: User-only, assistant-only, or all messages
3. **Line-Level Matching**: Matches within message content lines

## Command-Line Options

### Primary Content Filter
- **Positional arg (Query)**: The search text or pattern to find
  - Parsed in: `CycoDjCommandLineOptions.TryParseOtherCommandArg()` lines 308-311
  - Property: `SearchCommand.Query` (line 10)

###Role-Based Message Filtering
- **`--user-only`**, **`-u`**: Search only user messages
  - Parsed in: `CycoDjCommandLineOptions.TryParseSearchCommandOptions()` lines 133-137
  - Property: `SearchCommand.UserOnly` (line 15)
  
- **`--assistant-only`**, **`-a`**: Search only assistant messages
  - Parsed in: `CycoDjCommandLineOptions.TryParseSearchCommandOptions()` lines 138-142
  - Property: `SearchCommand.AssistantOnly` (line 16)

### Pattern Matching Options
- **`--case-sensitive`**, **`-c`**: Enable case-sensitive search
  - Parsed in: `CycoDjCommandLineOptions.TryParseSearchCommandOptions()` lines 117-121
  - Property: `SearchCommand.CaseSensitive` (line 13)
  
- **`--regex`**, **`-r`**: Use regex pattern matching
  - Parsed in: `CycoDjCommandLineOptions.TryParseSearchCommandOptions()` lines 122-126
  - Property: `SearchCommand.UseRegex` (line 14)

## Implementation Flow

### 1. Query Validation
**Location**: `SearchCommand.GenerateSearchOutput()` lines 46-50

```csharp
if (string.IsNullOrWhiteSpace(Query))
{
    sb.AppendLine("ERROR: Search query is required.");
    return sb.ToString();
}
```

**Requirement**: A non-empty search query must be provided.

### 2. Conversation-Level Search
**Location**: `SearchCommand.GenerateSearchOutput()` lines 114-132

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

Only conversations with matches are included in results.

### 3. Message Filtering by Role
**Location**: `SearchCommand.SearchConversation()` lines 192-201

```csharp
for (int i = 0; i < conversation.Messages.Count; i++)
{
    var message = conversation.Messages[i];
    
    // Filter by role if specified
    if (UserOnly && message.Role != "user") continue;
    if (AssistantOnly && message.Role != "assistant") continue;
    
    // Skip system messages unless explicitly searching all
    if (message.Role == "system" && (UserOnly || AssistantOnly)) continue;
    
    // ... search in message content
}
```

**Behavior**:
- `--user-only`: Only searches messages with role="user"
- `--assistant-only`: Only searches messages with role="assistant"
- No flags: Searches all messages (user, assistant, tool, system)
- System messages are skipped when role filtering is active

### 4. Text/Pattern Search
**Location**: `SearchCommand.SearchText()` lines 222-262

#### 4a. Line-by-Line Processing
```csharp
var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.None);

for (int lineNum = 0; lineNum < lines.Length; lineNum++)
{
    var line = lines[lineNum];
    if (string.IsNullOrEmpty(line)) continue;
    
    // ... pattern matching
}
```

**Behavior**: Empty lines are skipped from search.

#### 4b. Regex Pattern Matching
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

**Behavior**:
- Creates Regex with appropriate case sensitivity
- Returns match position and length for highlighting
- Handles invalid regex gracefully

#### 4c. Literal String Search
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

**Behavior**:
- Uses string comparison with appropriate case sensitivity
- Returns match position and length

### 5. Match Accumulation
**Location**: `SearchCommand.SearchConversation()` lines 203-217

```csharp
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
```

**Data Structure**: `SearchMatch` contains:
- `MessageIndex`: Position of message in conversation
- `Message`: Full message object
- `MatchedLines`: List of (lineNumber, line, matchStart, matchLength) tuples

## Content Filter Behavior

### Inclusivity Logic
1. **Query Requirement**: At least one occurrence of the query pattern
2. **Role Filter (if specified)**: Message role must match `--user-only` or `--assistant-only`
3. **Non-Empty Content**: Message must have non-whitespace content
4. **Non-Empty Lines**: Lines within message must be non-empty

### Filter Composition
Filters are applied in sequence (AND logic):
```
Message passes Layer 3 if:
  - Message has content (not null/whitespace)
  AND
  - Message role matches filter (if role filter specified)
  AND
  - Message content contains query pattern
  AND
  - At least one non-empty line matches pattern
```

## Relationship to Other Layers

### Layer 1 (Target Selection)
Layer 1 determines which conversation files to search. Layer 3 then searches within those files.

### Layer 2 (Container Filtering)
Layer 2 can filter conversations by time. Layer 3 searches within the resulting conversation set.

### Layer 5 (Context Expansion)
Layer 5 uses the matched line numbers from Layer 3 to show context around matches (see `--context` option).

### Layer 6 (Display Control)
Layer 6 uses Layer 3's match information to format output (highlighting, message counts).

## Examples

### Example 1: Simple Text Search
```bash
cycodj search "async/await"
```
**Behavior**:
- Searches all messages (user, assistant, tool)
- Case-insensitive by default
- Literal string match

### Example 2: User Messages Only
```bash
cycodj search "bug" --user-only
```
**Behavior**:
- Only searches messages with role="user"
- Ignores assistant, tool, and system messages

### Example 3: Regex with Case Sensitivity
```bash
cycodj search "TODO|FIXME" --regex --case-sensitive
```
**Behavior**:
- Uses regex pattern matching
- Case-sensitive search (only matches exact case)
- Finds either "TODO" or "FIXME"

### Example 4: Assistant Responses About Errors
```bash
cycodj search "error" --assistant-only
```
**Behavior**:
- Only searches assistant messages
- Case-insensitive match for "error"

## Feature Gaps and Opportunities

### Missing Features
1. **Content-based conversation filtering**: No `--conversation-contains` option (different from message search)
2. **Metadata search**: Cannot search in conversation titles/metadata
3. **Tool call search**: Cannot search within tool call parameters
4. **Multi-pattern search**: No `--all-of` or `--any-of` for multiple patterns
5. **Negative patterns**: No `--not-contains` to exclude matches
6. **Structured content filtering**: No JSON/code-aware search

### Potential Enhancements
1. **Field-specific search**: `--in-title`, `--in-metadata`
2. **Fuzzy matching**: `--fuzzy` for approximate matches
3. **Boolean queries**: `--and`, `--or`, `--not` operators
4. **Search history**: Save/reuse search queries
5. **Syntax highlighting**: Highlight matches in output (already has match positions)

## Data Flow Summary

```
Input Query
    ↓
Role Filter (--user-only / --assistant-only)
    ↓
Message Content Filter (non-empty, non-system)
    ↓
Line-by-Line Search
    ↓
Pattern Matching (literal or regex)
    ↓
Match Collection (with line numbers and positions)
    ↓
Result Aggregation
    ↓
Output to Layer 6 (Display)
```

## Performance Considerations

### Efficiency
- **Early exit**: Skips messages that don't match role filter
- **Line-by-line**: Processes large messages incrementally
- **Lazy evaluation**: Only loads conversations that pass Layer 1/2 filters

### Potential Optimizations
- **Regex compilation**: Could cache compiled regex patterns
- **Parallel search**: Could search conversations in parallel
- **Indexed search**: Could build full-text index for faster searches

## Source Code Reference

**Primary Files**:
- `src/cycodj/CommandLineCommands/SearchCommand.cs` (lines 1-308)
- `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs` (lines 95-187)

**Key Methods**:
- `SearchCommand.GenerateSearchOutput()` - Main search orchestration
- `SearchCommand.SearchConversation()` - Per-conversation search
- `SearchCommand.SearchText()` - Pattern matching logic
- `CycoDjCommandLineOptions.TryParseSearchCommandOptions()` - Option parsing

**Related Proof**: [cycodj-search-layer-3-proof.md](cycodj-search-layer-3-proof.md)
