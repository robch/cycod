# AI Function: Memory Log Viewer

## Overview

Add a new AI function tool that allows the AI to inspect its own in-memory logging during execution. This will help with debugging when the AI is writing code or troubleshooting issues in this repository.

## Background

The codebase uses a `MemoryLogger` that stores logs in a circular buffer (max 10,000 lines) with thread-safe access. Currently there's no way for the AI to inspect these logs during execution, which would be valuable for debugging AI function calls and execution flow.

## Implementation Plan

### 1. Architecture Decision: Direct Processing Pattern

**Analysis**: After examining existing AI function implementations, there are two patterns:
- **Pattern 1 (Direct)**: ViewFile processes files inline with sophisticated filtering (range selection, removeAllLines, lineContains, context expansion)
- **Pattern 2 (External)**: SearchInFiles delegates to cycodmd external process via CycoDmdCliWrapper

**Decision**: Use **Pattern 1 (Direct Processing)** because:
- Memory logs are already in memory (no file I/O needed)
- Simple operations similar to ViewFile
- Avoid process spawning overhead
- Direct control over thread safety
- Can reuse ViewFile's proven filtering pipeline exactly

### 2. Create New AI Function Tool

**File**: `src/cycod/FunctionCallingTools/MemoryLogHelperFunctions.cs`

**Class Structure**:
```csharp
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Text;

public class MemoryLogHelperFunctions
{
    [ReadOnly(true)]
    [Description("Get current in-memory logs with filtering and range selection for debugging AI function calls")]
    public string GetMemoryLogs(
        [Description("Start log line number (1-indexed). Negative numbers count from end (-1 = last line). Default: 1")] int startLine = 1,
        [Description("End log line number. 0 or -1 = end of logs. Negative numbers count from end. Default: 0")] int endLine = 0,
        
        [Description("Only show lines containing this regex pattern. Applied after removeAllLines filter.")] string lineContains = "",
        [Description("Remove lines containing this regex pattern. Applied first, before other filters.")] string removeAllLines = "",
        
        [Description("Number of lines to show before and after lineContains matches.")] int linesBeforeAndAfter = 0,
        [Description("Include line numbers in output.")] bool lineNumbers = true,
        
        [Description("Maximum number of characters to display per line.")] int maxCharsPerLine = 500,
        [Description("Maximum total number of characters to display.")] int maxTotalChars = 100000
    )
    {
        // Implementation modeled after ViewFile in StrReplaceEditorHelperFunctions
    }
}
```

### 3. Implementation Details

#### Core Strategy: Copy ViewFile's Filtering Pipeline
**Source**: `src/cycod/FunctionCallingTools/StrReplaceEditorHelperFunctions.cs` ViewFile method

**Key Difference**: Replace `FileHelpers.ReadAllText()` with memory log access

#### Step-by-Step Implementation:
1. **Get Memory Log Data**: Access `MemoryLogger.Instance` and get logs as string array
2. **Copy ViewFile Logic**: Reuse exact filtering pipeline from ViewFile:
   - Negative indexing for startLine/endLine (lines 68-82 in ViewFile)
   - Range selection (lines 84-86)
   - removeAllLines filtering with regex (lines 88-120)
   - lineContains filtering with regex (lines 122-162)
   - Context expansion (lines 164-181)
   - Output formatting with FormatAndTruncateLines (lines 191-192)

#### Data Access Challenge:
**Issue**: `MemoryLogger._lines` (CircularBuffer) may be private
**Solutions**:
1. **Add public accessor method** to MemoryLogger: `public string[] GetAllLogs()`
2. **Use existing patterns** from Logger.DumpMemoryLogs() method
3. **Access via reflection** (less preferred)

**Recommended**: Add public method to MemoryLogger for clean access

#### Thread Safety:
- Use MemoryLogger's existing lock-free ticket system
- Follow pattern from MemoryLogger.Dump() method (lines 90-131)
- Lock range during read to ensure consistent snapshot

#### Implementation Pattern:
```csharp
public string GetMemoryLogs(/* parameters */)
{
    // 1. Get all log lines thread-safely
    var allLines = GetMemoryLogLinesArray(); // New helper method needed
    var logLineCount = allLines.Length;
    
    // 2-8. Copy exact logic from ViewFile lines 68-192:
    //   - Negative indexing handling
    //   - Range validation and clamping  
    //   - removeAllLines regex filtering
    //   - lineContains regex filtering
    //   - Context line expansion
    //   - Output formatting with FormatAndTruncateLines
    
    return result;
}
```

### 3. Register the Function

**File**: `src/cycod/CommandLineCommands/ChatCommand.cs`

Add to the function factory registration:
```csharp
factory.AddFunctions(new MemoryLogHelperFunctions());
```

### 4. Modify MemoryLogger (if needed)

**File**: `src/common/Logger/MemoryLogger.cs`

**Add public accessor method** (if _lines is private):
```csharp
public string[] GetAllLogs()
{
    var lockTickets = LockAllSteps();
    try
    {
        return _lines.ReadAll().ToArray();
    }
    finally
    {
        // Dispose lock tickets in reverse order (LIFO)
        for (int i = lockTickets.Length - 1; i >= 0; i--)
        {
            lockTickets[i].Dispose();
        }
    }
}
```

### 5. Implementation Reference Points

**Primary Reference**: `src/cycod/FunctionCallingTools/StrReplaceEditorHelperFunctions.cs` ViewFile method (lines 56-280)

**Key Code Sections to Copy**:
- **Negative indexing logic** (lines 68-82): Handle negative startLine/endLine values
- **Range validation** (lines 73-86): Clamp and validate line ranges  
- **removeAllLines filtering** (lines 88-120): Regex-based line removal with error handling
- **lineContains filtering** (lines 122-162): Regex-based line matching with context tracking
- **Context expansion** (lines 164-181): Add lines before/after matches, avoid duplicates
- **FormatAndTruncateLines method** (lines 195-280): Line number formatting, highlighting, character limits

**Thread Safety Reference**: `src/common/Logger/MemoryLogger.cs` Dump method (lines 80-132)
- Use `LockAllSteps()` for consistent snapshot during read operations

### 6. Example Usage Scenarios

```csharp
// Get last 20 log entries
GetMemoryLogs(startLine: -20)

// Find errors with context
GetMemoryLogs(lineContains: "ERROR", linesBeforeAndAfter: 3)

// Remove debug noise and find function calls
GetMemoryLogs(removeAllLines: "DEBUG", lineContains: "ChatCommand")

// Get specific range of logs
GetMemoryLogs(startLine: 100, endLine: 200)

// Search for patterns with context
GetMemoryLogs(lineContains: "exception.*thrown", linesBeforeAndAfter: 2)
```

## Technical Architecture

### Memory Logger Access
- `MemoryLogger.Instance` - singleton access
- `CircularBuffer<string> _lines` - stores log entries  
- `ReadAll()` method - gets all current log entries
- Thread-safe via ticket system

### Parameter Design Rationale
- **Range Selection**: Follows ViewFile pattern with negative indexing
- **Filtering Pipeline**: Same as ViewFile - removeAllLines → lineContains → context expansion
- **No Log Level Filter**: Use removeAllLines for filtering instead (more flexible)
- **Parameter Ordering**: Follows established pattern from ViewFile/SearchInFiles

### Output Format
- Optional line numbers with original positions
- Regex highlighting around matches (if linesBeforeAndAfter > 0)
- Character truncation per line and total
- Similar formatting to other AI function tools

## Benefits

1. **Self-Debugging**: AI can inspect its own execution logs
2. **Problem Diagnosis**: See what happened during function calls
3. **Pattern Analysis**: Find sequences of events leading to issues  
4. **Context Understanding**: View what occurred around specific events
5. **Development Aid**: Valuable when AI is writing/debugging code for this repo

## Testing

- Test range selection with positive/negative indexing
- Test filtering pipeline with various regex patterns
- Test context line expansion
- Test character limits and truncation
- Test thread safety with concurrent logging
- Test edge cases (empty logs, invalid ranges, bad regex)

## Files to Modify

1. `src/cycod/FunctionCallingTools/MemoryLogHelperFunctions.cs` (new file)
2. `src/cycod/CommandLineCommands/ChatCommand.cs` (register function)
3. `src/common/Logger/MemoryLogger.cs` (add public accessor method if needed)

## Implementation Notes

- **Follow Pattern 1 (Direct Processing)** - model exactly after ViewFile, not SearchInFiles
- **Copy proven filtering logic** from ViewFile lines 56-280 for consistent behavior  
- **Use existing thread-safe patterns** from MemoryLogger.Dump() for data access
- **Reuse FormatAndTruncateLines helper** for consistent output formatting
- **Handle edge cases gracefully** with same error patterns as ViewFile (regex exceptions, empty results)
- **Ensure no circular logging** (the function logging about itself)
- **Test thoroughly** with same edge cases as ViewFile (negative indexing, invalid ranges, bad regex)

## Success Criteria

- AI can view recent memory logs with range selection
- AI can search for specific patterns in logs with regex
- AI can see context around important events  
- AI can filter out noise while preserving important information
- Function integrates seamlessly with existing AI tool ecosystem
- Performance is acceptable for debugging scenarios (sub-second response)
- Output format is consistent with other AI function tools
- Thread safety is maintained under concurrent access