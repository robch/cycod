# cycodj list Command - Filtering Pipeline Catalog

## Overview

The `list` command is the **default command** for cycodj. It lists conversations from chat history with optional filtering and preview messages.

**Purpose**: Browse conversation history efficiently

**Default Behavior**: Shows last 20 conversations if no filters specified

## Command Syntax

```bash
cycodj list [options]
cycodj [options]  # list is default
```

## Layer Implementation Summary

| Layer | Status | Complexity | Description |
|-------|--------|------------|-------------|
| 1. Target Selection | ✅ Implemented | **RICH** | Time filtering, count limiting, smart detection |
| 2. Container Filtering | ❌ Not implemented | - | No container-level filtering |
| 3. Content Filtering | ❌ Not implemented | - | No message content filtering |
| 4. Content Removal | ❌ Not implemented | - | No content removal |
| 5. Context Expansion | ❌ Not implemented | - | No context expansion |
| 6. Display Control | ✅ Implemented | **RICH** | Message previews, stats, branches |
| 7. Output Persistence | ✅ Implemented | **BASIC** | Save output to file |
| 8. AI Processing | ✅ Implemented | **BASIC** | AI instructions, function calls |
| 9. Actions on Results | ❌ Not implemented | - | Read-only command |

## Layer Documentation

### Layer 1: TARGET SELECTION ✅
**[View Details](cycodj-list-layer-1.md)** | **[View Proof](cycodj-list-layer-1-proof.md)**

Determines which conversation files to list.

**Options**:
- Time-range filtering: `--today`, `--yesterday`, `--after`, `--before`, `--date-range`, `--time-range`
- Legacy date filtering: `--date`, `-d`
- Smart count/timespec: `--last N|timespec`
- Default: Last 20 conversations

**Complexity**: RICH - Multiple filtering mechanisms with smart detection

### Layer 2-5: NOT IMPLEMENTED ❌

These layers are not implemented for the list command:
- **Layer 2** (Container Filtering): No filtering by conversation properties (e.g., title pattern, message count)
- **Layer 3** (Content Filtering): No filtering by message content
- **Layer 4** (Content Removal): No active content removal
- **Layer 5** (Context Expansion): No expansion around matches (list doesn't search)

### Layer 6: DISPLAY CONTROL ✅
**Documentation in progress**

Controls how conversations are displayed.

**Options**:
- `--messages [N|all]`: Control message preview count (default: 3)
- `--branches`: Show branch information
- `--stats`: Show statistics summary
- `--verbose`: Verbose output (global option)
- `--quiet`: Minimal output (global option)

**Features**:
- Conversation metadata (timestamp, title, ID)
- Message counts (user/assistant/tool)
- Branch visualization (indented branches)
- Message previews (truncated to 200 chars)
- Statistics aggregation (if `--stats` enabled)

**Complexity**: RICH - Multiple display options and formatting modes

### Layer 7: OUTPUT PERSISTENCE ✅
**Documentation in progress**

Saves output to files.

**Options**:
- `--save-output <file>`: Save to specified file

**Complexity**: BASIC - Simple file output

### Layer 8: AI PROCESSING ✅
**Documentation in progress**

AI-assisted processing of conversation list.

**Options**:
- `--instructions <text>`: AI processing instructions
- `--use-built-in-functions`: Enable AI function calls
- `--save-chat-history <file>`: Save AI interaction

**Complexity**: BASIC - Standard AI processing integration

### Layer 9: ACTIONS ON RESULTS ❌

Not implemented - list command is read-only.

## Common Usage Examples

### Example 1: Default List
```bash
cycodj list
# Shows: Last 20 conversations with 3-message previews
```

### Example 2: Today's Conversations
```bash
cycodj list --today
# Shows: All conversations from today (midnight to now)
```

### Example 3: Last 10 Conversations
```bash
cycodj list --last 10
# Shows: Most recent 10 conversations
```

### Example 4: Last 7 Days
```bash
cycodj list --last 7d
# Shows: All conversations from last 7 days
```

### Example 5: With Statistics
```bash
cycodj list --today --stats
# Shows: Today's conversations + aggregate statistics
```

### Example 6: More Message Previews
```bash
cycodj list --last 5 --messages 5
# Shows: Last 5 conversations with 5-message previews each
```

### Example 7: No Message Previews
```bash
cycodj list --messages 0
# Shows: Conversation metadata only, no message previews
```

### Example 8: Show All Messages
```bash
cycodj list --last 3 --messages all
# Shows: Last 3 conversations with ALL messages shown
```

### Example 9: Branch Visualization
```bash
cycodj list --today --branches
# Shows: Today's conversations with branch information
```

### Example 10: Save to File
```bash
cycodj list --last 7d --stats --save-output weekly-report.md
# Saves: Last 7 days with statistics to file
```

### Example 11: AI Analysis
```bash
cycodj list --today --instructions "Summarize the main topics discussed"
# AI analyzes and summarizes today's conversations
```

### Example 12: Specific Date Range
```bash
cycodj list --date-range 2024-01-01..2024-01-31
# Shows: All conversations in January 2024
```

## Command Properties

**File**: `src/cycodj/CommandLineCommands/ListCommand.cs`

```csharp
public class ListCommand : CycoDjCommand
{
    // Layer 1: Target Selection
    public string? Date { get; set; }                 // Legacy date filter
    public int Last { get; set; } = 0;                // Count limit
    // After/Before inherited from CycoDjCommand      // Time range filter
    
    // Layer 6: Display Control
    public bool ShowBranches { get; set; } = false;   // Show branch info
    public int? MessageCount { get; set; } = null;    // Message preview count (default: 3)
    public bool ShowStats { get; set; } = false;      // Show statistics
    
    // Layer 7: Output Persistence (inherited)
    // Layer 8: AI Processing (inherited)
}
```

## Key Behaviors

### Default Behavior
- **No options**: Shows last 20 conversations
- **Message preview**: 3 messages per conversation
- **Branch handling**: Shows branches indented with `↳` symbol
- **Preview truncation**: 200 characters per message

### Filtering Priority
1. Time-range filters (`--today`, `--after`, `--before`, etc.)
2. Legacy date filter (`--date`)
3. Default limit (20 conversations)
4. Count limit (`--last N`)

### Smart Detection
`--last` option uses smart detection:
- **Integer** → conversation count: `--last 10`
- **Timespec** → time range: `--last 7d`
- **Detection rules**:
  - Has `..` → timespec range
  - Equals `today`/`yesterday` → timespec keyword
  - Contains `d`, `h`, `m`, `s` → timespec with units
  - Pure integer → count

### Branch Visualization
When `--branches` is enabled or branches are detected:
- Root conversations: No indent
- Branched conversations: Indented with `↳` prefix
- Branch info shows: parent ID, child count, tool calls

### Message Preview Logic
- **For root conversations**: Shows FIRST N user messages
- **For branch conversations**: Shows LAST N user messages (what's new)
- Truncates to 200 characters
- Shows "... and X more" if more messages exist

## Source Files

### Implementation
- `src/cycodj/CommandLineCommands/ListCommand.cs` - Main command implementation
- `src/cycodj/CommandLine/CycoDjCommand.cs` - Base class with common properties

### Parser
- `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs` - Option parsing

### Helpers
- `src/cycodj/Helpers/HistoryFileHelpers.cs` - File finding and filtering
- `src/cycodj/Helpers/TimestampHelpers.cs` - Timestamp extraction
- `src/cycodj/Helpers/JsonlReader.cs` - JSONL file reading
- `src/cycodj/Analyzers/BranchDetector.cs` - Branch detection
- `src/common/Helpers/TimeSpecHelpers.cs` - Timespec parsing

## Related Commands

- [show](cycodj-show-README.md) - View single conversation in full detail
- [branches](cycodj-branches-README.md) - Visualize conversation tree structure
- [search](cycodj-search-README.md) - Search within conversation content
- [stats](cycodj-stats-README.md) - Detailed statistics about conversations

## Notes

### Why No Container Filtering?
The list command doesn't implement Layer 2 (container filtering) because:
- Time filtering (Layer 1) serves as the primary selection mechanism
- Conversation titles are already displayed, so visual filtering is easy
- Users can pipe to tools like `grep` for additional filtering
- The `search` command provides content-based filtering

### Why No Content Filtering?
The list command doesn't implement Layer 3 (content filtering) because:
- It's designed for browsing, not searching
- Use the `search` command for content-based filtering
- Message previews provide enough context for browsing

### Performance Considerations
- Default 20-conversation limit prevents overwhelming output
- Message preview truncation (200 chars) keeps output concise
- File filtering is done before parsing (efficient for large histories)
- Branch detection is done once for all conversations (O(n))

---

**Navigation**:
- [Back to Main Catalog](cycodj-filtering-pipeline-catalog-README.md)
- [Layer 1 Details](cycodj-list-layer-1.md)
- [Layer 1 Proof](cycodj-list-layer-1-proof.md)
