# cycod chat - Layer 7: Output Persistence

## Overview

Layer 7 controls **where and how results are saved** to persistent storage. For the chat command, this involves multiple output channels:

1. Chat history files (conversation memory)
2. Trajectory files (execution trace)
3. Auto-save mechanisms
4. Template-based file naming

## Implementation Summary

The chat command implements Layer 7 through:

- **Explicit output options**: `--chat-history`, `--output-chat-history`, `--output-trajectory`
- **Input/output separation**: `--input-chat-history` vs `--output-chat-history`
- **Auto-save behavior**: Automatic saving to timestamped files
- **Template expansion**: File naming with `{time}` and other variables
- **Continue mode**: `--continue` loads most recent history

## Command-Line Options

### Primary Output Control

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `--chat-history` | string | `chat-history.jsonl` | Both input and output history file |
| `--input-chat-history` | string | none | Read conversation from specific file |
| `--output-chat-history` | string | `chat-history-{time}.jsonl` | Save conversation to file (with template) |
| `--output-trajectory` | string | `trajectory-{time}.md` | Save execution trace/trajectory |
| `--continue` | flag | false | Load most recent chat history as input |

### Output Behavior Details

#### `--chat-history [FILE]`

Combines input and output into a single file reference:
- If file exists: loads as input
- Always saves to same file as output
- Disables `LoadMostRecentChatHistory`
- Default filename: `chat-history.jsonl`

**Example usage:**
```bash
cycod --chat-history my-session.jsonl
```

#### `--input-chat-history <FILE>`

Explicitly specifies input file only:
- File must exist (validated)
- Output goes to default or explicitly set output file
- Disables `LoadMostRecentChatHistory`

**Example usage:**
```bash
cycod --input-chat-history old-conversation.jsonl --output-chat-history new-conversation.jsonl
```

#### `--output-chat-history [FILE]`

Explicitly specifies output file only:
- Accepts template variables (`{time}`, etc.)
- Default: `chat-history-{time}.jsonl`
- Input determined separately

**Example usage:**
```bash
cycod --output-chat-history session-{time}.jsonl
```

#### `--continue`

Loads the most recent chat history automatically:
- Searches for latest timestamped history file
- Sets `LoadMostRecentChatHistory = true`
- Clears explicit `InputChatHistory` setting
- Allows conversation continuation without remembering filenames

**Example usage:**
```bash
cycod --continue "Continue our discussion about testing"
```

#### `--output-trajectory [FILE]`

Saves an execution trace/trajectory:
- Captures conversation flow and tool calls
- Markdown format
- Default: `trajectory-{time}.md`
- Accepts template variables

**Example usage:**
```bash
cycod --output-trajectory trace-{time}.md
```

## Template Variables

Output file names support template variable expansion:

| Variable | Description | Example |
|----------|-------------|---------|
| `{time}` | Current timestamp | `chat-history-20240315-143022.jsonl` |
| `{ProgramName}` | Program name (`cycod`) | `cycod-output.md` |
| Custom vars | User-defined via `--var` | `session-{sessionId}.jsonl` |

## Auto-Save Mechanism

The chat command has built-in auto-save behavior controlled by configuration:

### Auto-Save Chat History

- **Setting**: `app.auto-save-chat-history`
- **Default**: Enabled if not explicitly disabled
- **Filename**: Generated from `ChatHistoryFileHelpers.GroundAutoSaveChatHistoryFileName()`
- **Purpose**: Prevent data loss from crashes or unexpected exits

### Auto-Save Trajectory

- **Setting**: `app.auto-save-trajectory`
- **Default**: Enabled if not explicitly disabled
- **Filename**: Generated from `ChatHistoryFileHelpers.GroundAutoSaveTrajectoryFileName()`
- **Purpose**: Capture execution trace for debugging

### Auto-Save Location

Auto-save files are stored in a user-specific cache directory:
- Uses `ChatHistoryFileHelpers` to determine location
- Separate from explicit output files
- Timestamped to avoid overwrites

## File Format

### Chat History Format

Chat history files use **JSON Lines (.jsonl)** format:
- One JSON object per line
- Each line represents a message or event
- Easy to append, stream, and process incrementally

**Example:**
```jsonl
{"role":"system","content":"You are a helpful assistant."}
{"role":"user","content":"Hello, how are you?"}
{"role":"assistant","content":"I'm doing well, thanks for asking!"}
```

### Trajectory Format

Trajectory files use **Markdown (.md)** format:
- Human-readable execution trace
- Documents tool calls, decisions, and reasoning
- Includes timestamps and context

**Example:**
```markdown
# Conversation Trajectory

## 2024-03-15 14:30:22

### User Input
Hello, how are you?

### Tool Calls
- None

### Response
I'm doing well, thanks for asking!
```

## Data Flow

### Parsing Phase (CommandLineOptions)

```
CycoDevCommandLineOptions.TryParseChatCommandOptions()
├─ Lines 549-557: Parse --chat-history
│  ├─ Set InputChatHistory if file exists
│  ├─ Set OutputChatHistory to same file
│  └─ Disable LoadMostRecentChatHistory
├─ Lines 558-565: Parse --input-chat-history
│  ├─ Validate file exists
│  ├─ Set InputChatHistory
│  └─ Disable LoadMostRecentChatHistory
├─ Lines 567-570: Parse --continue
│  ├─ Enable LoadMostRecentChatHistory
│  └─ Clear InputChatHistory
├─ Lines 571-577: Parse --output-chat-history
│  └─ Set OutputChatHistory (with template)
└─ Lines 578-583: Parse --output-trajectory
   └─ Set OutputTrajectory (with template)
```

### Execution Phase (ChatCommand)

```
ChatCommand.ExecuteAsync()
├─ Lines 80-84: Ground file names
│  ├─ Resolve InputChatHistory (LoadMostRecentChatHistory or explicit)
│  ├─ Resolve AutoSaveOutputChatHistory from helpers
│  ├─ Resolve AutoSaveOutputTrajectory from helpers
│  ├─ Expand templates in OutputChatHistory
│  └─ Expand templates in OutputTrajectory
├─ Lines 94-95: Initialize trajectory files
│  ├─ Create TrajectoryFile(OutputTrajectory)
│  └─ Create TrajectoryFile(AutoSaveOutputTrajectory)
└─ Later: Actual file writing during conversation
```

### File Writing

File writing occurs throughout the conversation:
- **After each turn**: Chat history updated
- **After tool calls**: Trajectory updated
- **On exit**: Final flush of all buffers

## Configuration Integration

### Relevant Settings

| Setting | Purpose | Default |
|---------|---------|---------|
| `app.auto-save-chat-history` | Enable/disable auto-save | `true` |
| `app.auto-save-trajectory` | Enable/disable trajectory | `true` |
| `app.chat-history-folder` | Base folder for history | User cache dir |
| `app.trajectory-folder` | Base folder for trajectories | User cache dir |

### Configuration Sources

Settings can be specified in:
1. Command line (`--app-auto-save-chat-history false`)
2. Local config (`.cycod.json`)
3. User config (`~/.config/cycod/config.json`)
4. Global config (`/etc/cycod/config.json`)

Priority: Command line > Local > User > Global

## Edge Cases and Behavior

### 1. No Explicit Output Specified

**Input:** `cycod "Hello"`

**Behavior:**
- Auto-save chat history to timestamped file
- Auto-save trajectory to timestamped file
- No explicit output files created

### 2. Chat History File Doesn't Exist

**Input:** `cycod --chat-history new-session.jsonl "Hello"`

**Behavior:**
- File treated as output only (no input)
- Creates new chat history file
- No error

### 3. Input File Doesn't Exist

**Input:** `cycod --input-chat-history missing.jsonl "Hello"`

**Behavior:**
- **Error**: "File does not exist: missing.jsonl"
- Validation occurs in `ValidateFileExists()` (CommandLineOptions.cs, line validation)

### 4. Both --chat-history and --input-chat-history Specified

**Input:** `cycod --chat-history a.jsonl --input-chat-history b.jsonl`

**Behavior:**
- Last option wins (standard CLI behavior)
- `--input-chat-history` overrides `--chat-history` input
- Output still goes to `a.jsonl` (if `--chat-history` was first)

### 5. --continue with Explicit Output

**Input:** `cycod --continue --output-chat-history new-session.jsonl`

**Behavior:**
- Loads most recent chat history as input
- Saves to `new-session.jsonl` as output
- Effectively "forks" the conversation

### 6. Template Variables Not Defined

**Input:** `cycod --output-chat-history session-{sessionId}.jsonl`

**Behavior:**
- If `sessionId` not defined via `--var sessionId=123`
- Template variable remains unexpanded: `session-{sessionId}.jsonl`
- May cause literal filename with braces

## Related Components

### Helper Classes

- **ChatHistoryFileHelpers**: File resolution and auto-save logic
- **TrajectoryFile**: Trajectory writing and formatting
- **TemplateVariables**: Template expansion
- **FileHelpers**: General file operations and template support

### File Discovery

The `--continue` option uses sophisticated file discovery:
1. Search in current directory
2. Search in chat history folder (from config)
3. Find most recent by timestamp
4. Parse filename patterns (`chat-history-*.jsonl`, `*-{time}.jsonl`)

## Performance Considerations

### Incremental Writing

Chat history files are written incrementally:
- After each message exchange
- Prevents data loss
- Allows real-time monitoring (`tail -f chat-history.jsonl`)

### Buffering

Trajectory files may buffer:
- Markdown formatting requires context
- Flushed at strategic points (tool calls, turns)
- Final flush on exit

### File Locking

No explicit file locking:
- Assumes single writer
- Concurrent reads safe (JSON Lines format)
- Concurrent writes not supported

## Summary

Layer 7 in the chat command provides:

✅ **Flexible output control**: Multiple file types, templates, auto-save
✅ **Conversation continuity**: Load previous history, continue sessions
✅ **Debugging support**: Trajectory files capture execution flow
✅ **Data safety**: Auto-save prevents loss from crashes
✅ **Template-based naming**: Dynamic filenames with variables

The implementation spans:
- Command-line parsing (CycoDevCommandLineOptions.cs)
- Execution setup (ChatCommand.cs, lines 80-95)
- File writing (throughout execution)
- Helper utilities (ChatHistoryFileHelpers, TrajectoryFile)

## See Also

- [Layer 7 Proof (Source Evidence)](cycod-chat-filtering-pipeline-catalog-layer-7-proof.md)
- [Chat Command Overview](cycod-chat-filtering-pipeline-catalog-README.md)
- [All Layers](../cycod-filter-pipeline-catalog-README.md)
