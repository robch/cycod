# cycodj search - Layer 8: AI Processing - PROOF

## Source Code Evidence

This document provides detailed line-by-line evidence from the source code for how Layer 8 (AI Processing) is implemented in the `search` command.

---

## 1. Command Class Declaration

**File**: `src/cycodj/CommandLineCommands/SearchCommand.cs`
**Lines**: 8-20

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
    public int? MessageCount { get; set; } = null; // null = use default (3)
    public bool ShowStats { get; set; } = false;
```

**Evidence**:
- Line 8: `SearchCommand` inherits from `CycoDjCommand`
- Inherits `Instructions`, `UseBuiltInFunctions`, `SaveChatHistory` properties from base class
- Lines 10-20: Search-specific properties (Query, filters, display options)

---

## 2. Execution Flow

### 2.1 SearchCommand ExecuteAsync Method

**File**: `src/cycodj/CommandLineCommands/SearchCommand.cs`
**Lines**: 23-40

```csharp
public override async System.Threading.Tasks.Task<int> ExecuteAsync()
{
    var output = GenerateSearchOutput();
    
    // Apply instructions if provided
    var finalOutput = ApplyInstructionsIfProvided(output);
    
    // Save to file if --save-output was provided
    if (SaveOutputIfRequested(finalOutput))
    {
        return await System.Threading.Tasks.Task.FromResult(0);
    }
    
    // Otherwise print to console
    ConsoleHelpers.WriteLine(finalOutput);
    
    return await System.Threading.Tasks.Task.FromResult(0);
}
```

**Evidence**:
- Line 25: Calls `GenerateSearchOutput()` to perform search and format results
- Line 28: **Calls `ApplyInstructionsIfProvided(output)` - THIS IS LAYER 8**
- Line 28: Receives AI-processed (or unchanged) output as `finalOutput`
- Line 31: Proceeds to Layer 7 (Output Persistence)
- Line 36: Or prints to console if no file save requested

**Key Points**:
- Identical execution pattern to ListCommand
- AI processing happens AFTER search results generation (Line 25)
- AI processing happens BEFORE output persistence (Line 31)
- Standard pipeline order maintained

---

## 3. Search Output Generation

### 3.1 GenerateSearchOutput Method Structure

**File**: `src/cycodj/CommandLineCommands/SearchCommand.cs`
**Lines**: 42-186

```csharp
private string GenerateSearchOutput()
{
    var sb = new System.Text.StringBuilder();
    
    if (string.IsNullOrWhiteSpace(Query))
    {
        sb.AppendLine("ERROR: Search query is required.");
        return sb.ToString();
    }

    sb.AppendLine($"## Searching conversations for: \"{Query}\"");
    sb.AppendLine();

    // Find and parse conversations
    var historyDir = CycoDj.Helpers.HistoryFileHelpers.GetHistoryDirectory();
    var files = CycoDj.Helpers.HistoryFileHelpers.FindAllHistoryFiles();
    
    // ... filtering logic (Lines 59-109) ...
    
    // Parse conversations and search
    var matches = new List<(Models.Conversation conversation, List<SearchMatch> searchMatches)>();
    
    foreach (var file in files)
    {
        // ... search each conversation (Lines 114-132) ...
    }

    // Display results
    if (!matches.Any())
    {
        sb.AppendLine("No matches found.");
        return sb.ToString();
    }

    sb.AppendLine($"Found {matches.Count} conversation(s) with matches:");
    sb.AppendLine();

    foreach (var (conversation, searchMatches) in matches)
    {
        AppendConversationMatches(sb, conversation, searchMatches);
    }

    sb.AppendLine();
    sb.AppendLine($"Total: {matches.Sum(m => m.searchMatches.Count)} match(es) in {matches.Count} conversation(s)");
    
    // Add statistics if requested
    if (ShowStats)
    {
        // ... statistics output (Lines 154-183) ...
    }
    
    return sb.ToString();
}
```

**Evidence**:
- Lines 42-186: Complete search result generation
- Line 52: Query header
- Lines 114-132: Actual search through conversations
- Lines 136-147: Display search results
- Lines 154-183: Optional statistics
- Line 186: **Returns complete markdown output**
- This output becomes input to Layer 8 at Line 28 of ExecuteAsync

### 3.2 What AI Receives as Input

The AI receives the complete formatted search output including:

**Header** (Line 52):
```markdown
## Searching conversations for: "authentication"
```

**Filter Information** (Lines 64-76):
```markdown
Filtered by time range: 2024-01-01 12:00 to 2024-01-31 23:59
```

**Match Summary** (Line 141):
```markdown
Found 5 conversation(s) with matches:
```

**Each Matched Conversation** (Lines 144-147):
```markdown
### 2024-01-15 09:30:00 - Implementing OAuth
    File: chat-history-20240115-093000.jsonl
    Matches: 3

  [user] Message #1
  > ... context before match ...
  > matched line with authentication mentioned
  > ... context after match ...
```

**Summary Stats** (Line 150):
```markdown
Total: 12 match(es) in 5 conversation(s)
```

**Optional Statistics** (Lines 154-183):
```markdown
═══════════════════════════════════════
## Statistics Summary
═══════════════════════════════════════

Total conversations searched: 50
Conversations with matches: 5
Total matches: 12
...
```

---

## 4. Search-Specific Context for AI

### 4.1 Search Parameters Available to AI

The AI receives output that reflects these search parameters:

**Query** (Lines 46-50):
```csharp
if (string.IsNullOrWhiteSpace(Query))
{
    sb.AppendLine("ERROR: Search query is required.");
    return sb.ToString();
}
```
- Query is included in output header (Line 52)
- AI knows what was searched for

**Case Sensitivity** (Lines 232-236):
```csharp
else
{
    var comparison = CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;
    var index = line.IndexOf(Query!, comparison);
    // ...
}
```
- Affects which matches are found
- AI sees only the actual matches (not the setting itself)

**Regex Mode** (Lines 221-231):
```csharp
if (UseRegex)
{
    try
    {
        var regexOptions = CaseSensitive ? RegexOptions.None : RegexOptions.IgnoreCase;
        var regex = new Regex(Query!, regexOptions);
        var match = regex.Match(line);
        // ...
    }
}
```
- Affects match patterns
- AI sees the matches without knowing if regex was used

**Role Filtering** (Lines 196-198):
```csharp
// Filter by role if specified
if (UserOnly && message.Role != "user") continue;
if (AssistantOnly && message.Role != "assistant") continue;
```
- Filters which messages are searched
- AI only sees matches from the allowed roles

**Context Lines** (Lines 282-293):
```csharp
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
```
- Line 286: `ContextLines` property determines context range
- Default is 2 lines (Line 17: `public int ContextLines { get; set; } = 2;`)
- AI receives matched line plus N lines before/after
- More context = better AI understanding

### 4.2 Statistics Available to AI

**If `--stats` is used** (Lines 153-183):

```csharp
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
    // ... more statistics ...
}
```

**Evidence**:
- Line 153: Statistics only added if `ShowStats` is true
- Lines 163-182: Detailed statistics about:
  - Total conversations searched
  - Conversations with matches
  - Match count
  - Message role distribution
  - Branching information
- AI can use these statistics for analysis

---

## 5. AI Processing Integration

### 5.1 Base Class Method (Same as List Command)

**File**: `src/cycodj/CommandLine/CycoDjCommand.cs`
**Lines**: 37-52

```csharp
/// <summary>
/// Apply instructions to output if --instructions was provided
/// </summary>
protected string ApplyInstructionsIfProvided(string output)
{
    if (string.IsNullOrEmpty(Instructions))
    {
        return output;
    }
    
    return AiInstructionProcessor.ApplyInstructions(
        Instructions, 
        output, 
        UseBuiltInFunctions, 
        SaveChatHistory);
}
```

**Evidence**:
- Same implementation as all CycoDjCommand subclasses
- Line 42: Check if Instructions is set
- Line 44: Pass-through if no instructions
- Lines 47-51: AI processing if instructions present

---

## 6. Data Flow Diagram

```
┌─────────────────────────────────────────────────────────┐
│ SearchCommand.ExecuteAsync() [Line 23-40]              │
└─────────────────────────────────────────────────────────┘
                        │
                        ▼
    ┌──────────────────────────────────────────┐
    │ GenerateSearchOutput() [Line 25]        │
    │ - Performs search                       │
    │ - Finds matches                         │
    │ - Formats results with context          │
    │ Returns: string (formatted markdown)    │
    └──────────────────────────────────────────┘
                        │
                        ▼
    ┌────────────────────────────────────────────────────┐
    │ ApplyInstructionsIfProvided(output) [Line 28]     │
    │ *** LAYER 8: AI PROCESSING ***                    │
    │                                                    │
    │ AI receives:                                      │
    │ - Search query                                    │
    │ - Matched conversations with context              │
    │ - Message role info                               │
    │ - Optional statistics                             │
    └────────────────────────────────────────────────────┘
                        │
            ┌───────────┴───────────┐
            ▼                       ▼
      ┌──────────┐          ┌─────────────────────┐
      │ No       │          │ Yes Instructions    │
      │ Instruct.│          │ Set                 │
      └──────────┘          └─────────────────────┘
            │                       │
            │                       ▼
            │         ┌──────────────────────────────────────┐
            │         │ AiInstructionProcessor               │
            │         │ .ApplyInstructions(                  │
            │         │   Instructions,                      │
            │         │   output,                            │
            │         │   UseBuiltInFunctions,               │
            │         │   SaveChatHistory)                   │
            │         │                                      │
            │         │ AI can:                              │
            │         │ - Summarize search results           │
            │         │ - Extract patterns                   │
            │         │ - Correlate findings                 │
            │         │ - Transform format                   │
            │         │ - Use built-in functions (if enabled)│
            │         └──────────────────────────────────────┘
            │                       │
            └───────────┬───────────┘
                        ▼
                  finalOutput
                        │
                        ▼
    ┌────────────────────────────────────────────────────┐
    │ SaveOutputIfRequested(finalOutput) [Line 31]      │
    │ *** LAYER 7: OUTPUT PERSISTENCE ***               │
    └────────────────────────────────────────────────────┘
```

---

## 7. Complete Example Traces

### Example 1: Basic Search Without AI

**Command**:
```bash
cycodj search "async Task" --regex
```

**Execution Trace**:
```
1. CLI Parser:
   - Sets Query = "async Task"
   - Sets UseRegex = true
   - Instructions remains null

2. ExecuteAsync() Line 25:
   - Calls GenerateSearchOutput()
   - Searches conversations with regex pattern
   - Finds 8 matches in 3 conversations
   - Returns formatted search results

3. ExecuteAsync() Line 28:
   - Calls ApplyInstructionsIfProvided(output)
   - Instructions is null
   - Returns output unchanged
   - finalOutput = original search results

4. ExecuteAsync() Line 36:
   - Prints search results to console
```

**Output Example**:
```markdown
## Searching conversations for: "async Task"

Found 3 conversation(s) with matches:

### 2024-01-15 10:30:00 - Async Programming Discussion
    File: chat-history-20240115-103000.jsonl
    Matches: 3

  [user] Message #2
    How do I properly use async Task methods?
  > Looking at examples of async Task return types
    In C#, you should always use async Task instead of async void

...
```

### Example 2: Search with AI Analysis

**Command**:
```bash
cycodj search "authentication" --context 5 --instructions "Analyze these search results and identify: 1) Common authentication patterns mentioned, 2) Security concerns raised, 3) Best practices suggested"
```

**Execution Trace**:
```
1. CLI Parser:
   - Sets Query = "authentication"
   - Sets ContextLines = 5
   - Sets Instructions = "Analyze these search results..."

2. ExecuteAsync() Line 25:
   - Searches for "authentication"
   - Includes 5 lines context around each match
   - Finds matches in 7 conversations
   - Generates formatted results with extended context

3. ExecuteAsync() Line 28:
   - Calls ApplyInstructionsIfProvided(output)
   - Instructions is NOT null
   - Calls AiInstructionProcessor.ApplyInstructions(
       "Analyze these search results...",
       output,  // Full search results with 5-line context
       false,   // No built-in functions
       null     // No history save
     )
   - AI analyzes extended context
   - Returns structured analysis

4. ExecuteAsync() Line 36:
   - Prints AI analysis to console
```

**AI Output Example**:
```markdown
# Authentication Analysis

## 1. Common Authentication Patterns Mentioned

- **OAuth 2.0 / OpenID Connect**: Mentioned in 4 conversations
  - Users discussed token-based authentication
  - JWT validation and refresh tokens

- **API Key Authentication**: Found in 3 conversations
  - Simple header-based auth for internal services
  - Key rotation concerns raised

## 2. Security Concerns Raised

- Token expiration handling (3 mentions)
- Secure storage of credentials (2 mentions)
- HTTPS requirement for auth endpoints (mentioned once)

## 3. Best Practices Suggested

- Always use HTTPS for authentication
- Implement token refresh logic
- Never store plaintext passwords
- Use industry-standard libraries (IdentityServer, Auth0)
```

### Example 3: Search with AI and Function Calling

**Command**:
```bash
cycodj search "error handling" --user-only --instructions "For each conversation that discusses error handling, use the SearchFiles function to find related error handling code in the codebase and create a summary report linking conversations to actual implementations" --use-built-in-functions --save-output error-handling-report.md
```

**Execution Trace**:
```
1. CLI Parser:
   - Sets Query = "error handling"
   - Sets UserOnly = true (only search user messages)
   - Sets Instructions = "For each conversation..."
   - Sets UseBuiltInFunctions = true
   - Sets SaveOutput = "error-handling-report.md"

2. ExecuteAsync() Line 25:
   - Searches user messages only
   - Finds error handling mentions
   - Returns formatted search results

3. ExecuteAsync() Line 28:
   - Calls ApplyInstructionsIfProvided(output)
   - Calls AiInstructionProcessor.ApplyInstructions(
       "For each conversation...",
       output,
       true,                             // AI CAN use functions
       null
     )
   - AI analyzes search results
   - AI calls SearchFiles to find error handling code
   - AI correlates conversations with code
   - Returns comprehensive report

4. ExecuteAsync() Line 31:
   - SaveOutputIfRequested returns true
   - Saves AI report to "error-handling-report.md"
   - Returns 0 (no console output)
```

**AI Output Example** (saved to file):
```markdown
# Error Handling Analysis Report

## Conversation: 2024-01-15 - Exception Handling Best Practices

**Key Discussion Points:**
- User asked about try-catch best practices
- Discussion of specific vs. generic exception handling
- Mentioned importance of logging

**Related Code Found:**
- `src/common/ConsoleHelpers.cs:142-156` - Exception handling for console output
- `src/cycod/ChatCommand.cs:89-102` - Error handling in chat execution

**Code Pattern:**
```csharp
try
{
    // Operation
}
catch (SpecificException ex)
{
    Logger.Error($"Specific error: {ex.Message}");
    throw;
}
```

---

## Conversation: 2024-01-20 - Async Exception Handling

...
```

---

## 8. Integration with Other Layers

### 8.1 Layer 5 (Context Expansion) → Layer 8

**File**: `src/cycodj/CommandLineCommands/SearchCommand.cs`
**Lines**: 17, 282-293

```csharp
// Line 17: Property declaration
public int ContextLines { get; set; } = 2;

// Lines 282-293: Context expansion logic
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
```

**Evidence**:
- Layer 5 (Context Expansion) runs BEFORE Layer 8
- `ContextLines` property (default 2) determines context amount
- More context (larger `--context` value) gives AI more information
- AI receives the expanded output from Line 186

**Impact on AI**:
- `--context 0`: AI sees only matched lines (minimal information)
- `--context 2`: AI sees match + 2 lines before/after (default, moderate context)
- `--context 10`: AI sees match + 10 lines before/after (maximum context, best understanding)

### 8.2 Layer 6 (Display Control) → Layer 8

**File**: `src/cycodj/CommandLineCommands/SearchCommand.cs`
**Lines**: 153-183

```csharp
// Add statistics if requested
if (ShowStats)
{
    var conversations = matches.Select(m => m.conversation).ToList();
    
    sb.AppendLine();
    sb.AppendLine("═══════════════════════════════════════");
    sb.AppendLine("## Statistics Summary");
    // ... statistics output ...
}
```

**Evidence**:
- Layer 6 (Display Control) runs BEFORE Layer 8
- `ShowStats` flag determines if statistics are included
- Statistics become part of the input to AI
- AI can use statistics for more informed analysis

**Impact on AI**:
- Without `--stats`: AI sees search results only
- With `--stats`: AI sees results + aggregate statistics (conversation counts, message distributions, etc.)
- Statistics help AI understand scope and patterns

### 8.3 Layer 8 → Layer 7 (Output Persistence)

**File**: `src/cycodj/CommandLineCommands/SearchCommand.cs`
**Lines**: 28-34

```csharp
// Apply instructions if provided
var finalOutput = ApplyInstructionsIfProvided(output);

// Save to file if --save-output was provided
if (SaveOutputIfRequested(finalOutput))
{
    return await System.Threading.Tasks.Task.FromResult(0);
}
```

**Evidence**:
- Line 28: AI processing produces `finalOutput`
- Line 31: `finalOutput` (AI-processed or original) goes to Layer 7
- AI-processed output is what gets saved to file

**Implications**:
- `--save-output` without `--instructions`: saves raw search results
- `--save-output` with `--instructions`: saves AI-processed analysis
- User can choose between raw data (for further processing) or AI analysis (for reporting)

---

## 9. Verification Checklist

✅ **Inheritance**:
- [x] SearchCommand inherits from CycoDjCommand (Line 8)
- [x] Inherits Instructions, UseBuiltInFunctions, SaveChatHistory properties

✅ **Execution Flow**:
- [x] AI processing called after search generation (Line 28)
- [x] AI processing called before output persistence (Line 31)
- [x] Pass-through when no instructions (base class Line 44)
- [x] AI invocation when instructions present (base class Lines 47-51)

✅ **Integration**:
- [x] Receives search results with context from Layer 5
- [x] Receives formatted output from Layer 6
- [x] Passes processed output to Layer 7
- [x] Works with --save-output
- [x] Works independently

✅ **Search-Specific Features**:
- [x] AI receives query in output (Line 52)
- [x] AI receives match context based on ContextLines (Lines 282-293)
- [x] AI receives role-filtered results (Lines 196-198)
- [x] AI receives statistics if ShowStats is true (Lines 153-183)

---

## Conclusion

This proof document demonstrates that Layer 8 (AI Processing) in the `search` command:

1. **Fully Implemented**: Uses shared base class implementation
2. **Properly Integrated**: Sits between search generation and output persistence
3. **Context-Aware**: Receives search results with configurable context lines
4. **Statistics-Enhanced**: Can receive aggregate statistics for analysis
5. **Flexible**: Supports function calling for cross-referencing with code/files
6. **Consistent**: Identical pattern to other cycodj commands

The evidence shows that AI processing receives rich search context including matches, surrounding lines, statistics, and can use built-in functions to enhance analysis beyond the search results themselves.
