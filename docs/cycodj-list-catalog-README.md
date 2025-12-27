# cycodj list: Filtering Pipeline Catalog

## Command Overview

The `list` command displays conversations from chat history with optional time-based filtering and message previews.

**Default behavior**: Lists the last 20 conversations if no filters are specified.

---

## Layer Documentation

### [Layer 1: TARGET SELECTION](cycodj-list-filtering-pipeline-catalog-layer-1.md)
What conversations to search (time-based filtering, date ranges, "last N" conversations)

### [Layer 2: CONTAINER FILTERING](cycodj-list-filtering-pipeline-catalog-layer-2.md)
Which conversation files to include (implicit filtering by timestamp from Layer 1)

### [Layer 3: CONTENT FILTERING](cycodj-list-filtering-pipeline-catalog-layer-3.md)
What content within conversations to show (message previews with configurable count)

### [Layer 4: CONTENT REMOVAL](cycodj-list-filtering-pipeline-catalog-layer-4.md)
What content to actively remove (NOT IMPLEMENTED for list command)

### [Layer 5: CONTEXT EXPANSION](cycodj-list-filtering-pipeline-catalog-layer-5.md)
How to expand around matches (NOT APPLICABLE for list command)

### [Layer 6: DISPLAY CONTROL](cycodj-list-filtering-pipeline-catalog-layer-6.md)
How to present results (`--stats`, `--branches`, `--messages`)

### [Layer 7: OUTPUT PERSISTENCE](cycodj-list-filtering-pipeline-catalog-layer-7.md)
Where to save results (`--save-output`)

### [Layer 8: AI PROCESSING](cycodj-list-filtering-pipeline-catalog-layer-8.md)
AI-assisted analysis (`--instructions`, `--use-built-in-functions`, `--save-chat-history`)

### [Layer 9: ACTIONS ON RESULTS](cycodj-list-filtering-pipeline-catalog-layer-9.md)
What to do with results (NONE - read-only command)

---

## Command Properties

From `ListCommand.cs`:

```csharp
public class ListCommand : CycoDjCommand
{
    public string? Date { get; set; }
    public int Last { get; set; } = 0;
    public bool ShowBranches { get; set; } = false;
    public int? MessageCount { get; set; } = null; // null = use default (3)
    public bool ShowStats { get; set; } = false;
    
    // Inherited from CycoDjCommand:
    // public DateTime? After { get; set; }
    // public DateTime? Before { get; set; }
    // public string? Instructions { get; set; }
    // public bool UseBuiltInFunctions { get; set; } = false;
    // public string? SaveChatHistory { get; set; }
    // public string? SaveOutput { get; set; }
}
```

---

## Command Line Options

### Time Filtering (Layer 1)
- `--date DATE`, `-d DATE`: Filter by specific date (backward compat)
- `--last N`: Show last N conversations (conversation count)
- `--last TIMESPEC`: Show conversations from timespec (e.g., `7d`, `-7d`)
- `--today`: Show today's conversations (shortcut)
- `--yesterday`: Show yesterday's conversations (shortcut)
- `--after TIME`, `--time-after TIME`: Show conversations after time
- `--before TIME`, `--time-before TIME`: Show conversations before time
- `--date-range RANGE`, `--time-range RANGE`: Specify time range

### Display Control (Layer 6)
- `--messages [N|all]`: Number of message previews to show (default: 3)
- `--stats`: Show statistical summary
- `--branches`: Show branch information

### Output Persistence (Layer 7)
- `--save-output FILE`: Save output to file

### AI Processing (Layer 8)
- `--instructions TEXT`: AI instructions to process output
- `--use-built-in-functions`: Enable AI to use built-in functions
- `--save-chat-history FILE`: Save AI processing chat history

---

## Usage Examples

```bash
# List last 20 conversations (default)
cycodj list

# List last 10 conversations
cycodj list --last 10

# List today's conversations
cycodj list --today

# List conversations from last 7 days
cycodj list --last 7d

# List conversations with time range
cycodj list --after "2024-01-01" --before "2024-01-31"

# List with branch info and stats
cycodj list --branches --stats

# List with more message previews
cycodj list --messages 5

# List and save to file
cycodj list --save-output conversations.md

# List and apply AI instructions
cycodj list --instructions "Summarize these conversations"
```

---

## Default Behavior

If no time filtering is specified, the list command applies a sensible default:
- **Default limit**: Last 20 conversations
- **Reason**: Prevents overwhelming output for users with large history

See `ListCommand.cs` lines 97-104 for implementation details.

---

## Data Flow

1. **Parse command line options** → Set properties on ListCommand
2. **Find all history files** → `HistoryFileHelpers.FindAllHistoryFiles()`
3. **Apply time filters** → Filter file list based on After/Before/Date/Last
4. **Read conversations** → `JsonlReader.ReadConversations(files)`
5. **Detect branches** → `BranchDetector.DetectBranches(conversations)`
6. **Generate output** → Format conversations with previews
7. **Apply AI instructions** (if provided)
8. **Save or display** → Either save to file or print to console

---

## Source Code References

- **Command**: `src/cycodj/CommandLineCommands/ListCommand.cs`
- **Parser**: `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs` (lines 96-174)
- **Base Class**: `src/cycodj/CommandLine/CycoDjCommand.cs`
- **Helpers**: 
  - `src/cycodj/Helpers/HistoryFileHelpers.cs`
  - `src/cycodj/Helpers/JsonlReader.cs`
  - `src/cycodj/Analyzers/BranchDetector.cs`
