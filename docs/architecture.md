# cycodj Architecture

## Overview

cycodj is a standalone .NET console application for analyzing cycod chat history files. It follows the same architectural patterns as other cycod tools (cycodmd, cycodt, cycodgr).

**IMPORTANT:** See [adding-new-cli-tool.md](adding-new-cli-tool.md) for infrastructure requirements (solution file, CI/CD, build scripts) based on how `cycodgr` was added.

## Technology Stack

- **.NET 9.0** - Target framework (matching cycod)
- **C#** - Primary language
- **CommandLineParser** - For command-line argument parsing
- **System.Text.Json** - For JSONL parsing
- **LINQ** - For data analysis and queries

## Project Structure

```
src/cycodj/
├── cycodj.csproj              # Project file
├── Program.cs                 # Entry point, command routing
├── Commands/                  # Command implementations
│   ├── ListCommand.cs         # List conversations
│   ├── ShowCommand.cs         # Show conversation details
│   ├── JournalCommand.cs      # Generate daily journal
│   └── BranchesCommand.cs     # Show conversation tree
├── Models/                    # Data models
│   ├── ChatMessage.cs         # Individual message
│   ├── ToolCall.cs            # Tool call information
│   ├── Conversation.cs        # Complete conversation
│   ├── ConversationMetadata.cs # Metadata (title, etc)
│   └── ConversationTree.cs    # Branch relationships
├── Analyzers/                 # Analysis logic
│   ├── ConversationAnalyzer.cs    # Main analysis orchestration
│   ├── BranchDetector.cs          # Detect conversation branches
│   ├── ContentSummarizer.cs       # Summarize content
│   └── TimelineBuilder.cs         # Build chronological view
├── Helpers/                   # Utility classes
│   ├── JsonlReader.cs         # Read and parse JSONL files
│   ├── HistoryFileHelpers.cs  # Find and list history files
│   └── TimestampHelpers.cs    # Parse filename timestamps
└── Formatters/                # Output formatting
    ├── ConsoleFormatter.cs    # Console output with colors
    ├── JournalFormatter.cs    # Daily journal format
    └── TreeFormatter.cs       # Tree visualization
```

## Core Components

### 1. Models

#### ChatMessage
```csharp
public class ChatMessage
{
    public string Role { get; set; }          // "user", "assistant", "tool", "system"
    public string Content { get; set; }       // Message content
    public List<ToolCall>? ToolCalls { get; set; }  // Tool invocations
    public string? ToolCallId { get; set; }   // Links tool response to call
}
```

#### Conversation
```csharp
public class Conversation
{
    public string Id { get; set; }                    // From filename
    public string FilePath { get; set; }              // Full path
    public DateTime Timestamp { get; set; }           // Parsed from filename
    public ConversationMetadata? Metadata { get; set; } // Title, etc
    public List<ChatMessage> Messages { get; set; }
    
    // For branch detection
    public List<string> ToolCallIds { get; set; }     // Ordered list of IDs
    public string? ParentId { get; set; }             // Branch parent
    public List<string> BranchIds { get; set; }       // Child branches
}
```

### 2. Analyzers

#### BranchDetector
Core algorithm for detecting conversation relationships:

```csharp
public class BranchDetector
{
    // Build conversation tree from all files
    public ConversationTree BuildTree(List<Conversation> conversations)
    {
        // 1. Extract tool_call_id sequences from each conversation
        // 2. Find common prefixes between conversations
        // 3. Identify parent-child relationships
        // 4. Build tree structure
    }
    
    // Find conversations that share common history
    private Dictionary<string, List<Conversation>> GroupBySharedPrefix()
    {
        // Group conversations with matching tool_call_id prefixes
    }
    
    // Calculate how much two conversations overlap
    private int GetCommonPrefixLength(Conversation a, Conversation b)
    {
        // Compare tool_call_id lists
        // Return index where they diverge
    }
}
```

**Algorithm Details:**
1. Extract ordered list of tool_call_ids from each conversation
2. Build a prefix tree (trie) of tool_call_id sequences
3. Conversations sharing longer prefixes are more closely related
4. Identify parent as conversation with exact prefix match
5. Multiple children = multiple branches from same point

#### ContentSummarizer
Extract and summarize meaningful content:

```csharp
public class ContentSummarizer
{
    // Extract user questions/inputs
    public List<string> GetUserMessages(Conversation conv, 
        bool excludeLarge = true, int maxLength = 10000)
    
    // Get assistant text responses (not tool outputs)
    public List<string> GetAssistantResponses(Conversation conv,
        bool abbreviate = true, int maxLength = 500)
    
    // Identify if tool output is large and should be abbreviated
    public bool IsLargeToolOutput(ChatMessage msg, int threshold = 1000)
    
    // Generate conversation summary
    public string Summarize(Conversation conv, int maxLength = 200)
}
```

### 3. Helpers

#### JsonlReader
```csharp
public class JsonlReader
{
    // Read and parse JSONL file
    public static Conversation ReadConversation(string filePath)
    {
        // 1. Read all lines
        // 2. Check first line for metadata
        // 3. Parse each line as ChatMessage
        // 4. Extract tool_call_ids
        // 5. Return Conversation object
    }
    
    // Parse single JSONL line to ChatMessage
    private static ChatMessage? ParseMessage(string line)
    {
        // Handle JSON deserialization
        // Handle errors gracefully
    }
}
```

#### HistoryFileHelpers
```csharp
public class HistoryFileHelpers
{
    // Find all chat history files in user scope
    public static List<string> FindAllHistoryFiles()
    
    // Get history directory path
    public static string GetHistoryDirectory()
    
    // Filter files by date range
    public static List<string> FilterByDateRange(
        List<string> files, DateTime? start, DateTime? end)
}
```

#### TimestampHelpers
```csharp
public static class TimestampHelpers
{
    // Extract timestamp from filename: "chat-history-1754437373970.jsonl"
    public static DateTime ParseTimestamp(string filename)
    {
        // Extract numeric part
        // Convert from Unix epoch milliseconds
        // Return DateTime in local time
    }
    
    // Format timestamp for display
    public static string FormatTimestamp(DateTime dt, string format = "default")
}
```

### 4. Formatters

#### JournalFormatter
```csharp
public class JournalFormatter
{
    // Generate daily journal for given date
    public string FormatDailyJournal(
        DateTime date, 
        List<Conversation> conversations,
        ConversationTree tree)
    {
        // Group by time periods (morning, afternoon, evening)
        // Show user actions and brief assistant summaries
        // Include branch indicators
        // Use console colors for readability
    }
}
```

## Data Flow

### List Command
```
User Input: cycodj list --date 2024-12-20
    ↓
HistoryFileHelpers.FindAllHistoryFiles()
    ↓
JsonlReader.ReadConversation() for each file
    ↓
TimestampHelpers.FilterByDate()
    ↓
Sort by timestamp
    ↓
ConsoleFormatter.FormatList()
    ↓
Output to console
```

### Journal Command
```
User Input: cycodj journal --date 2024-12-20
    ↓
HistoryFileHelpers.FindAllHistoryFiles()
    ↓
JsonlReader.ReadConversation() for matching files
    ↓
BranchDetector.BuildTree()
    ↓
ContentSummarizer.Summarize() for each
    ↓
JournalFormatter.FormatDailyJournal()
    ↓
Output to console
```

### Branches Command
```
User Input: cycodj branches
    ↓
HistoryFileHelpers.FindAllHistoryFiles()
    ↓
JsonlReader.ReadConversation() for each file
    ↓
BranchDetector.BuildTree()
    ↓
TreeFormatter.FormatTree()
    ↓
Output to console (with indentation/colors)
```

## Performance Considerations

### Lazy Loading
- Don't read all files upfront
- Load only files in requested date range
- Cache parsed conversations if needed

### Indexing (Future)
If performance becomes an issue with large histories:
- Create SQLite index of: filename, timestamp, title, tool_call_ids
- Update index incrementally as new files added
- Query index instead of scanning all files

### Memory Management
- Stream large files instead of loading fully
- Limit message content storage for large tool outputs
- Use pagination for large result sets

## Error Handling

### File Reading Errors
- Corrupted JSONL: Skip line, log warning
- Missing files: Handle gracefully, show message
- Permission errors: Clear error message to user

### Parsing Errors
- Invalid JSON: Log line number, continue
- Missing fields: Use defaults, don't fail
- Unknown roles: Accept but mark as unknown

### Branch Detection Edge Cases
- Circular references: Detect and break
- Orphaned conversations: Show as separate roots
- Ambiguous branches: Show all possibilities

## Testing Strategy

### Unit Tests
- JsonlReader with various valid/invalid inputs
- BranchDetector algorithm with known tree structures
- TimestampHelpers with edge cases
- ContentSummarizer with different message types

### Integration Tests
- End-to-end command tests using test history files
- Branch detection with real conversation patterns
- Journal generation for various date ranges

### Test Data
Create fixtures with:
- Simple linear conversation
- Branched conversations (2-3 branches)
- Complex tree (multiple levels)
- Edge cases (empty, single message, etc)

## Dependencies

### Internal (from cycod)
- May reuse some helper classes if appropriate
- Follow same patterns as cycod/cycodmd/cycodt

### External (NuGet)
- `CommandLineParser` - Command line parsing
- `System.Text.Json` - JSON deserialization
- No AI/ML libraries needed (this is analysis only)

## Future Enhancements

### Phase 2+
- **Search**: Full-text search across conversations
- **Export**: Markdown, HTML, PDF output
- **Statistics**: Charts/graphs of usage patterns
- **Interactive Mode**: TUI for browsing conversations
- **Cleanup Tools**: Identify and remove redundant files
- **Merge Branches**: Combine related conversations

### Advanced Features
- **AI Summarization**: Use AI to generate better summaries
- **Topic Extraction**: Automatically categorize conversations
- **Relationship Visualization**: GraphViz output
- **Conversation Replay**: Step through conversation interactively
