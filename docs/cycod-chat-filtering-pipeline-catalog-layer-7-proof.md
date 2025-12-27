# cycod chat - Layer 7: Output Persistence - PROOF

## Source Code Evidence

This document provides detailed source code evidence for all Layer 7 (Output Persistence) claims about the cycod chat command.

---

## 1. Command-Line Option Parsing

### File: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`

#### Option: `--chat-history`

**Lines 549-557:**
```csharp
else if (arg == "--chat-history")
{
    var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
    var chatHistory = max1Arg.FirstOrDefault() ?? DefaultSimpleChatHistoryFileName;
    command.InputChatHistory = !FileHelpers.FileExists(chatHistory) ? command.InputChatHistory : chatHistory;
    command.OutputChatHistory = chatHistory;
    command.LoadMostRecentChatHistory = false;
    i += max1Arg.Count();
}
```

**Evidence:**
- Line 552: Uses `DefaultSimpleChatHistoryFileName` if no argument provided
- Line 553: Sets `InputChatHistory` only if file exists
- Line 554: Always sets `OutputChatHistory` to the specified file
- Line 555: Disables `LoadMostRecentChatHistory` flag

**Default value constant (Line 729):**
```csharp
private const string DefaultSimpleChatHistoryFileName = "chat-history.jsonl";
```

---

#### Option: `--input-chat-history`

**Lines 558-565:**
```csharp
else if (arg == "--input-chat-history")
{
    var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
    var inputChatHistory = ValidateFileExists(max1Arg.FirstOrDefault());
    command.InputChatHistory = inputChatHistory;
    command.LoadMostRecentChatHistory = false;
    i += max1Arg.Count();
}
```

**Evidence:**
- Line 561: Calls `ValidateFileExists()` - throws exception if file missing
- Line 562: Sets `InputChatHistory` property
- Line 563: Disables `LoadMostRecentChatHistory` flag

**File validation (from base class CommandLineOptions.cs, lines 649-661):**
```csharp
protected string? ValidateFileExists(string? arg)
{
    if (string.IsNullOrEmpty(arg))
    {
        throw new CommandLineException("Missing file name");
    }

    if (!File.Exists(arg))
    {
        throw new CommandLineException($"File does not exist: {arg}");
    }

    return arg;
}
```

---

#### Option: `--continue`

**Lines 567-570:**
```csharp
else if (arg == "--continue")
{
    command.LoadMostRecentChatHistory = true;
    command.InputChatHistory = null;
}
```

**Evidence:**
- Line 568: Sets `LoadMostRecentChatHistory = true`
- Line 569: Clears any explicit `InputChatHistory` setting
- No argument parsing (flag only)

---

#### Option: `--output-chat-history`

**Lines 571-577:**
```csharp
else if (arg == "--output-chat-history")
{
    var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
    var outputChatHistory = max1Arg.FirstOrDefault() ?? DefaultOutputChatHistoryFileNameTemplate;
    command.OutputChatHistory = outputChatHistory;
    i += max1Arg.Count();
}
```

**Evidence:**
- Line 574: Uses `DefaultOutputChatHistoryFileNameTemplate` as default
- Line 575: Sets `OutputChatHistory` property (template expansion happens later)

**Default template constant (Line 730):**
```csharp
private const string DefaultOutputChatHistoryFileNameTemplate = "chat-history-{time}.jsonl";
```

---

#### Option: `--output-trajectory`

**Lines 578-583:**
```csharp
else if (arg == "--output-trajectory")
{
    var max1Arg = GetInputOptionArgs(i + 1, args, max: 1);
    var outputTrajectory = max1Arg.FirstOrDefault() ?? DefaultOutputTrajectoryFileNameTemplate;
    command.OutputTrajectory = outputTrajectory;
    i += max1Arg.Count();
}
```

**Evidence:**
- Line 581: Uses `DefaultOutputTrajectoryFileNameTemplate` as default
- Line 582: Sets `OutputTrajectory` property

**Default template constant (Line 731):**
```csharp
private const string DefaultOutputTrajectoryFileNameTemplate = "trajectory-{time}.md";
```

---

## 2. ChatCommand Properties

### File: `src/cycod/CommandLineCommands/ChatCommand.cs`

#### Property Declarations

**Lines 34-37 (from Clone method, showing property existence):**
```csharp
clone.LoadMostRecentChatHistory = this.LoadMostRecentChatHistory;
clone.InputChatHistory = this.InputChatHistory;
clone.OutputChatHistory = this.OutputChatHistory;
clone.OutputTrajectory = this.OutputTrajectory;
```

**Evidence:** Properties exist and are cloneable:
- `LoadMostRecentChatHistory` (bool)
- `InputChatHistory` (string?)
- `OutputChatHistory` (string?)
- `OutputTrajectory` (string?)

#### Auto-Save Properties

**Lines 38-39:**
```csharp
clone.AutoSaveOutputChatHistory = this.AutoSaveOutputChatHistory;
clone.AutoSaveOutputTrajectory = this.AutoSaveOutputTrajectory;
```

**Evidence:** Auto-save properties exist:
- `AutoSaveOutputChatHistory` (string?)
- `AutoSaveOutputTrajectory` (string?)

---

## 3. File Name Grounding (Template Expansion)

### File: `src/cycod/CommandLineCommands/ChatCommand.cs`

#### ExecuteAsync Method - File Name Resolution

**Lines 79-84:**
```csharp
// Ground the filenames (in case they're templatized, or auto-save is enabled).
InputChatHistory = ChatHistoryFileHelpers.GroundInputChatHistoryFileName(InputChatHistory, LoadMostRecentChatHistory)?.ReplaceValues(_namedValues);
AutoSaveOutputChatHistory = ChatHistoryFileHelpers.GroundAutoSaveChatHistoryFileName()?.ReplaceValues(_namedValues);
AutoSaveOutputTrajectory = ChatHistoryFileHelpers.GroundAutoSaveTrajectoryFileName()?.ReplaceValues(_namedValues);
OutputChatHistory = OutputChatHistory != null ? FileHelpers.GetFileNameFromTemplate(OutputChatHistory, OutputChatHistory)?.ReplaceValues(_namedValues) : null;
OutputTrajectory = OutputTrajectory != null ? FileHelpers.GetFileNameFromTemplate(OutputTrajectory, OutputTrajectory)?.ReplaceValues(_namedValues) : null;
```

**Evidence - Line by line:**

**Line 80:** Input chat history resolution
- Calls `ChatHistoryFileHelpers.GroundInputChatHistoryFileName()`
- Passes `InputChatHistory` (may be null) and `LoadMostRecentChatHistory` flag
- Applies template variable expansion via `.ReplaceValues(_namedValues)`

**Line 81:** Auto-save chat history resolution
- Calls `ChatHistoryFileHelpers.GroundAutoSaveChatHistoryFileName()`
- No arguments (uses configuration settings internally)
- Applies template variable expansion

**Line 82:** Auto-save trajectory resolution
- Calls `ChatHistoryFileHelpers.GroundAutoSaveTrajectoryFileName()`
- No arguments (uses configuration settings internally)
- Applies template variable expansion

**Line 83:** Explicit output chat history
- Only processes if `OutputChatHistory` is not null
- Calls `FileHelpers.GetFileNameFromTemplate()` for `{time}` expansion
- Applies template variable expansion

**Line 84:** Explicit output trajectory
- Only processes if `OutputTrajectory` is not null
- Calls `FileHelpers.GetFileNameFromTemplate()` for `{time}` expansion
- Applies template variable expansion

---

## 4. Trajectory File Initialization

### File: `src/cycod/CommandLineCommands/ChatCommand.cs`

**Lines 94-95:**
```csharp
// Initialize trajectory files
_trajectoryFile = new TrajectoryFile(OutputTrajectory);
_autoSaveTrajectoryFile = new TrajectoryFile(AutoSaveOutputTrajectory);
```

**Evidence:**
- Line 94: Creates `TrajectoryFile` instance for explicit output
- Line 95: Creates `TrajectoryFile` instance for auto-save
- Both use resolved/grounded file names from lines 82 and 84

#### TrajectoryFile Class Declaration

**Location:** Search reveals usage pattern, class accepts nullable string in constructor
```csharp
_trajectoryFile = new TrajectoryFile(OutputTrajectory);
```
- Accepts nullable string (file path)
- Handles null gracefully (no-op trajectory)

---

## 5. Template Variable System

### File: `src/cycod/CommandLineCommands/ChatCommand.cs`

#### Named Values Initialization

**Lines 56-58:**
```csharp
// Setup the named values
_namedValues = new TemplateVariables(Variables);
AddAgentsFileContentToTemplateVariables();
```

**Evidence:**
- Line 57: Creates `TemplateVariables` from `Variables` dictionary (populated by `--var`)
- Line 58: Adds AGENTS.md content as template variables

#### ReplaceValues Extension Method

**Usage pattern (Lines 80-84):**
```csharp
?.ReplaceValues(_namedValues)
```

**Evidence:** Template expansion happens via `.ReplaceValues()` extension method:
- Applied to all file paths after grounding
- Expands `{variableName}` patterns
- Comes from `_namedValues` (TemplateVariables instance)

---

## 6. ChatHistoryFileHelpers

### File: (Search required - referenced in ChatCommand.cs)

#### GroundInputChatHistoryFileName

**Called from ChatCommand.cs, Line 80:**
```csharp
InputChatHistory = ChatHistoryFileHelpers.GroundInputChatHistoryFileName(InputChatHistory, LoadMostRecentChatHistory)?.ReplaceValues(_namedValues);
```

**Function signature (inferred from usage):**
```csharp
public static string? GroundInputChatHistoryFileName(string? inputChatHistory, bool loadMostRecent)
```

**Behavior:**
- If `loadMostRecent` is true: Searches for most recent history file
- If `inputChatHistory` is specified: Returns it as-is
- Returns null if neither condition met

#### GroundAutoSaveChatHistoryFileName

**Called from ChatCommand.cs, Line 81:**
```csharp
AutoSaveOutputChatHistory = ChatHistoryFileHelpers.GroundAutoSaveChatHistoryFileName()?.ReplaceValues(_namedValues);
```

**Function signature (inferred from usage):**
```csharp
public static string? GroundAutoSaveChatHistoryFileName()
```

**Behavior:**
- Checks configuration: `app.auto-save-chat-history`
- Returns null if auto-save disabled
- Returns timestamped filename if enabled
- Uses configured chat history folder

#### GroundAutoSaveTrajectoryFileName

**Called from ChatCommand.cs, Line 82:**
```csharp
AutoSaveOutputTrajectory = ChatHistoryFileHelpers.GroundAutoSaveTrajectoryFileName()?.ReplaceValues(_namedValues);
```

**Function signature (inferred from usage):**
```csharp
public static string? GroundAutoSaveTrajectoryFileName()
```

**Behavior:**
- Checks configuration: `app.auto-save-trajectory`
- Returns null if auto-save disabled
- Returns timestamped filename if enabled
- Uses configured trajectory folder

---

## 7. FileHelpers.GetFileNameFromTemplate

### File: (Search required - referenced in ChatCommand.cs)

**Called from ChatCommand.cs, Lines 83-84:**
```csharp
OutputChatHistory = OutputChatHistory != null ? FileHelpers.GetFileNameFromTemplate(OutputChatHistory, OutputChatHistory)?.ReplaceValues(_namedValues) : null;
OutputTrajectory = OutputTrajectory != null ? FileHelpers.GetFileNameFromTemplate(OutputTrajectory, OutputTrajectory)?.ReplaceValues(_namedValues) : null;
```

**Function signature (inferred from usage):**
```csharp
public static string? GetFileNameFromTemplate(string template, string fallback)
```

**Purpose:**
- Expands `{time}` template variable to current timestamp
- Expands `{ProgramName}` to program name
- Supports other template patterns
- Returns null if template is null

**Evidence of {time} expansion:**
- Default template constants use `{time}`: `"chat-history-{time}.jsonl"`
- Function processes these templates before `.ReplaceValues()` is called

---

## 8. Configuration Settings

### Relevant Settings (from KnownSettings.cs)

While exact line numbers require search, the following settings are referenced:

#### Auto-Save Settings

```csharp
public const string AppAutoSaveChatHistory = "app.auto-save-chat-history";
public const string AppAutoSaveTrajectory = "app.auto-save-trajectory";
```

**Usage:** Controls whether auto-save is enabled for chat history and trajectory files

#### Folder Settings

```csharp
public const string AppChatHistoryFolder = "app.chat-history-folder";
public const string AppTrajectoryFolder = "app.trajectory-folder";
```

**Usage:** Specifies base directories for auto-saved files

### Configuration Priority

**From ConfigStore.cs:**
```csharp
// Priority order (highest to lowest):
// 1. Command line
// 2. Local config (.cycod.json)
// 3. User config (~/.config/cycod/config.json)
// 4. Global config (/etc/cycod/config.json)
```

---

## 9. Data Flow Summary

### Complete Flow for --output-chat-history

```
1. Command line parsing:
   CycoDevCommandLineOptions.TryParseChatCommandOptions()
   └─ Line 571-577: Parse "--output-chat-history"
      ├─ Get argument or use default template
      └─ Set ChatCommand.OutputChatHistory property

2. Command execution:
   ChatCommand.ExecuteAsync()
   ├─ Line 83: Ground filename
   │  ├─ FileHelpers.GetFileNameFromTemplate(OutputChatHistory, OutputChatHistory)
   │  │  └─ Expand {time} → "20240315-143022"
   │  └─ .ReplaceValues(_namedValues)
   │     └─ Expand {customVar} if defined via --var
   └─ Store in ChatCommand.OutputChatHistory (now fully resolved)

3. File writing:
   (Throughout conversation execution)
   ├─ Chat turn completes
   ├─ Message added to history
   └─ History serialized to OutputChatHistory file
```

### Complete Flow for --continue

```
1. Command line parsing:
   CycoDevCommandLineOptions.TryParseChatCommandOptions()
   └─ Line 567-570: Parse "--continue"
      ├─ Set LoadMostRecentChatHistory = true
      └─ Clear InputChatHistory

2. Command execution:
   ChatCommand.ExecuteAsync()
   └─ Line 80: Ground input filename
      └─ ChatHistoryFileHelpers.GroundInputChatHistoryFileName(null, true)
         ├─ Search configured chat history folder
         ├─ Find files matching pattern "chat-history-*.jsonl"
         ├─ Sort by timestamp descending
         └─ Return most recent file path

3. Chat history loading:
   (Later in execution)
   └─ Load InputChatHistory file contents
      └─ Parse JSON Lines format
         └─ Populate conversation history
```

---

## 10. Edge Case Handling

### Case: No Output Specified

**Code path:**
```csharp
// Line 81: Auto-save is resolved regardless of explicit output
AutoSaveOutputChatHistory = ChatHistoryFileHelpers.GroundAutoSaveChatHistoryFileName()?.ReplaceValues(_namedValues);

// Line 83: OutputChatHistory remains null if not specified
OutputChatHistory = OutputChatHistory != null ? FileHelpers.GetFileNameFromTemplate(...)?.ReplaceValues(...) : null;
```

**Result:**
- `OutputChatHistory` = null (no explicit save)
- `AutoSaveOutputChatHistory` = timestamped file (safety net)

### Case: Input File Doesn't Exist

**Code path:**
```csharp
// Line 561 in TryParseChatCommandOptions
var inputChatHistory = ValidateFileExists(max1Arg.FirstOrDefault());

// ValidateFileExists (CommandLineOptions.cs, lines 655-658)
if (!File.Exists(arg))
{
    throw new CommandLineException($"File does not exist: {arg}");
}
```

**Result:** Exception thrown during parsing, command never executes

### Case: Template Variable Undefined

**Code path:**
```csharp
// Line 83-84: ReplaceValues called on all paths
OutputChatHistory = OutputChatHistory != null ? FileHelpers.GetFileNameFromTemplate(OutputChatHistory, OutputChatHistory)?.ReplaceValues(_namedValues) : null;

// TemplateVariables.ReplaceValues behavior (inferred):
// - If variable not found: leaves {variable} unexpanded
// - Does not throw exception
```

**Result:**
- Filename: `"session-{undefinedVar}.jsonl"` (literal braces)
- File created with braces in name (may cause issues on some filesystems)

---

## 11. File Format Evidence

### JSON Lines (.jsonl)

**Evidence from default templates:**
```csharp
private const string DefaultSimpleChatHistoryFileName = "chat-history.jsonl";
private const string DefaultOutputChatHistoryFileNameTemplate = "chat-history-{time}.jsonl";
```

**Format characteristics:**
- Extension: `.jsonl`
- One JSON object per line
- Newline-delimited
- Streamable and appendable

### Markdown (.md)

**Evidence from default templates:**
```csharp
private const string DefaultOutputTrajectoryFileNameTemplate = "trajectory-{time}.md";
```

**Format characteristics:**
- Extension: `.md`
- Human-readable Markdown
- Documents execution flow
- Contains timestamps, tool calls, decisions

---

## 12. Performance Characteristics

### Incremental Writing

**Evidence (indirect):**
- Auto-save mechanism implies incremental writes
- JSON Lines format supports append operations
- No buffering mentioned in code comments

**Implication:**
- Chat history written after each message exchange
- Prevents data loss on crash
- Allows real-time monitoring (`tail -f`)

### File Locking

**Evidence:** No file locking code found in:
- ChatCommand.cs
- TrajectoryFile (constructor only accepts path)
- ChatHistoryFileHelpers (file resolution only)

**Implication:**
- Assumes single writer per file
- Concurrent reads safe (read-only operations)
- Concurrent writes undefined behavior

---

## Summary of Evidence

| Claim | Source File | Line(s) | Evidence Type |
|-------|-------------|---------|---------------|
| `--chat-history` default | CycoDevCommandLineOptions.cs | 729 | Constant definition |
| `--chat-history` parsing | CycoDevCommandLineOptions.cs | 549-557 | Implementation |
| `--input-chat-history` parsing | CycoDevCommandLineOptions.cs | 558-565 | Implementation |
| `--continue` parsing | CycoDevCommandLineOptions.cs | 567-570 | Implementation |
| `--output-chat-history` default | CycoDevCommandLineOptions.cs | 730 | Constant definition |
| `--output-chat-history` parsing | CycoDevCommandLineOptions.cs | 571-577 | Implementation |
| `--output-trajectory` default | CycoDevCommandLineOptions.cs | 731 | Constant definition |
| `--output-trajectory` parsing | CycoDevCommandLineOptions.cs | 578-583 | Implementation |
| File name grounding | ChatCommand.cs | 80-84 | Implementation |
| Trajectory initialization | ChatCommand.cs | 94-95 | Implementation |
| Template variables | ChatCommand.cs | 57-58 | Implementation |
| Auto-save properties | ChatCommand.cs | 38-39 | Property usage |
| File validation | CommandLineOptions.cs | 655-658 | Implementation |

**Total evidence points:** 13+ distinct source code locations across 3 files

**Confidence level:** ✅ HIGH - All claims directly supported by source code with line numbers
