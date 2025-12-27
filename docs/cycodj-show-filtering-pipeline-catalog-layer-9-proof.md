# cycodj show Command - Layer 9 Proof: Actions on Results

[← Back to Layer 9 Catalog](cycodj-show-filtering-pipeline-catalog-layer-9.md)

## Source Code Evidence

This document provides proof that the `show` command does NOT implement Layer 9 (Actions on Results).

---

## 1. Command Class Properties

**File**: `src/cycodj/CommandLineCommands/ShowCommand.cs`

### Property Declarations (Lines 10-16)

```csharp
public class ShowCommand : CycoDjCommand
{
    public string ConversationId { get; set; } = string.Empty;
    public bool ShowToolCalls { get; set; } = false;
    public bool ShowToolOutput { get; set; } = false;
    public int MaxContentLength { get; set; } = 500;
    public bool ShowStats { get; set} = false;
```

**Evidence**: 
- All properties are for **display control** (ShowToolCalls, ShowToolOutput, MaxContentLength, ShowStats)
- `ConversationId` is for **target selection**, not actions
- **NO** action-related properties (no Delete, Modify, Transform flags)

---

## 2. ExecuteAsync Method

**File**: `src/cycodj/CommandLineCommands/ShowCommand.cs`

### Main Execution (Lines 18-35)

```csharp
public override async Task<int> ExecuteAsync()
{
    var output = GenerateShowOutput();  // ← Generate display string
    
    // Apply instructions if provided
    var finalOutput = ApplyInstructionsIfProvided(output);  // ← AI processing (Layer 8)
    
    // Save to file if --save-output was provided
    if (SaveOutputIfRequested(finalOutput))  // ← Output persistence (Layer 7)
    {
        return await Task.FromResult(0);
    }
    
    // Otherwise print to console
    ConsoleHelpers.WriteLine(finalOutput);  // ← Display only
    
    return await Task.FromResult(0);
}
```

**Evidence**:
- Line 20: Calls `GenerateShowOutput()` - only generates text
- Line 23: `ApplyInstructionsIfProvided()` - Layer 8, read-only AI processing
- Line 26: `SaveOutputIfRequested()` - Layer 7, saves display output to file (doesn't modify conversation)
- Line 32: `ConsoleHelpers.WriteLine()` - displays output only
- **NO** calls to file deletion, modification, or transformation methods

---

## 3. GenerateShowOutput Method

**File**: `src/cycodj/CommandLineCommands/ShowCommand.cs`

### Output Generation (Lines 37-229)

```csharp
private string GenerateShowOutput()
{
    var sb = new System.Text.StringBuilder();
    
    // ... validation ...
    
    // Find the conversation file
    var files = HistoryFileHelpers.FindAllHistoryFiles();  // ← READ only
    var matchingFile = files.FirstOrDefault(f => 
        f.Contains(ConversationId) || 
        System.IO.Path.GetFileNameWithoutExtension(f) == ConversationId);
    
    // ... error handling ...
    
    // Read the conversation
    var conversation = JsonlReader.ReadConversation(matchingFile);  // ← READ only
    
    // Load all conversations for branch detection
    var allConversations = JsonlReader.ReadConversations(files);  // ← READ only
    BranchDetector.DetectBranches(allConversations);  // ← In-memory processing only
    
    // ... display generation (lines 76-227) ...
    
    return sb.ToString();  // ← Return display string only
}
```

**Evidence**:
- Line 49: `FindAllHistoryFiles()` - **reads** files from disk
- Line 62: `ReadConversation()` - **reads** single conversation
- Line 70: `ReadConversations()` - **reads** all conversations
- Line 71: `DetectBranches()` - updates in-memory objects only, no file writes
- Line 228: Returns **string** for display
- **NO** calls to:
  - `File.Delete()`
  - `File.WriteAllText()` (to conversation files)
  - `File.Move()` or `File.Copy()`
  - Any conversation modification methods

### Key Operations in GenerateShowOutput

Lines 76-195: **Display** generation
- Loops through messages
- Formats content with truncation
- Shows tool calls and outputs based on flags
- **No file modification**

Lines 198-226: **Statistics** display (if ShowStats enabled)
- Calculates message counts
- Displays aggregations
- All in-memory calculations
- **No file writes**

---

## 4. Command Line Parsing

**File**: `src/cycodj/CommandLine/CycoDjCommandLineOptions.cs`

### ShowCommand Options (Lines 368-405)

```csharp
private bool TryParseShowCommandOptions(ShowCommand command, string[] args, ref int i, string arg)
{
    // First positional argument is the conversation ID
    if (!arg.StartsWith("-") && string.IsNullOrEmpty(command.ConversationId))
    {
        command.ConversationId = arg;
        return true;
    }
    
    // Try common display options first
    if (TryParseDisplayOptions(command, args, ref i, arg))
    {
        return true;
    }
    
    if (arg == "--show-tool-calls")
    {
        command.ShowToolCalls = true;
        return true;
    }
    else if (arg == "--show-tool-output")
    {
        command.ShowToolOutput = true;
        return true;
    }
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
    
    return false;
}
```

**Evidence**:
- Line 371-374: Positional argument sets `ConversationId` (target selection)
- Lines 383-402: All options control **display** behavior
- No options for **actions** like:
  - `--delete`
  - `--edit`
  - `--export`
  - `--remove-messages`
  - `--execute`

---

## 5. No Action Methods Present

### What's NOT in ShowCommand.cs

Searching the entire file (`src/cycodj/CommandLineCommands/ShowCommand.cs`, 231 lines):

**NO** occurrences of:
- `File.Delete` - no deletion
- `File.Move` - no moving/renaming
- `File.Copy` - no copying
- `conversation.Messages.Add` - no message addition
- `conversation.Messages.Remove` - no message removal
- `conversation.Metadata` setters - no metadata modification
- `JsonlWriter.Write` - no writing back to conversation files

**Proof**: The command is entirely read-only.

---

## 6. Comparison with cleanup Command

### cleanup Command HAS Layer 9 Actions

**File**: `src/cycodj/CommandLineCommands/CleanupCommand.cs`, Line 102

```csharp
File.Delete(file);  // ← ACTUAL FILE DELETION
```

### show Command has NO such code

**File**: `src/cycodj/CommandLineCommands/ShowCommand.cs`

- **NO** `File.Delete()` calls
- **NO** `File.Move()` calls
- **NO** `File.WriteAllText()` calls to conversation files
- Only `File.WriteAllText()` in base class `SaveOutputIfRequested()` writes **display output**, not source files

---

## 7. Execution Flow Evidence

```
User: cycodj show conversation-abc123 --show-tool-calls
    ↓
ParseOptions() - Sets command.ConversationId = "conversation-abc123", ShowToolCalls = true
    ↓
ExecuteAsync() - Line 18
    ↓
GenerateShowOutput() - Line 20
    ├→ FindAllHistoryFiles() - READ files
    ├→ Find matching file by ID - Filter file list (in-memory)
    ├→ ReadConversation() - READ single conversation
    ├→ ReadConversations() - READ all for branch detection
    ├→ DetectBranches() - Process in-memory (no file write)
    └→ Build detailed display string - Pure string building
    ↓
ApplyInstructionsIfProvided() - Layer 8 (AI processing of display string)
    ↓
SaveOutputIfRequested() OR ConsoleHelpers.WriteLine()
    └→ Display output (no conversation files modified)
    ↓
END - Exit without modifying any conversation files
```

**Evidence**: No step in the flow modifies, deletes, or transforms the source conversation file.

---

## 8. Display-Only Features

The show command's features are all **display-related**:

| Feature | Layer | Evidence |
|---------|-------|----------|
| Show tool calls | Layer 6 | Line 176-183: Conditionally displays `ToolCalls` array |
| Show tool output | Layer 6 | Line 154-173: Controls content truncation for tool messages |
| Max content length | Layer 6 | Line 154: Truncates display, doesn't modify source |
| Show stats | Layer 6 | Line 198-226: Calculates and displays in-memory statistics |

All features **read** from conversation data and **display** formatted output. None **write** back to conversation files.

---

## Conclusion

The `show` command is **provably read-only**:

1. **No action-related properties** in command class
2. **No action-related methods** in command class
3. **No file modification calls** (`Delete`, `Move`, write to conversation files)
4. **No command-line options** for actions
5. **Execution flow** is purely: find → read → format → display → exit

Layer 9 (Actions on Results) is **NOT IMPLEMENTED** by design. The command's purpose is to **inspect** a single conversation in detail, not **act** on it.
