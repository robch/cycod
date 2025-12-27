# cycod chat - Layer 6: DISPLAY CONTROL

## Overview

Layer 6 controls **how results are presented** to the user. For the `chat` command, this includes interactive vs. non-interactive modes, verbosity levels, console output formatting, and real-time streaming of AI responses.

## Display Control Mechanisms

### 1. Interactive Mode Control

**Option**: `--interactive [true|false]`

**Purpose**: Controls whether the chat operates in interactive mode (multi-turn conversation) or batch mode (single execution).

**Default**: `true` (unless stdin/stdout is redirected)

**Behavior**:
- **Interactive mode** (`--interactive` or `--interactive true`):
  - Displays prompts: `User:`, `Assistant:`
  - Accepts multi-turn input via `Console.ReadLine()`
  - Continues until user types `exit` or sends EOF
  - Shows token usage after each turn
  - Provides real-time streaming output

- **Non-interactive mode** (`--interactive false`):
  - Processes `--input` / `--inputs` instructions only
  - No prompts displayed
  - Exits after processing all instructions
  - Suitable for scripting and automation

**Auto-detection**: If stdin or stdout is redirected (e.g., piping or redirection), interactive mode is automatically disabled, even if not explicitly specified.

**Source Code**: See [proof document](cycod-chat-layer-6-proof.md#interactive-mode) for line numbers.

---

### 2. Quiet Mode

**Option**: `--quiet`

**Purpose**: Suppress non-essential output, showing only final results.

**Default**: `false`

**Behavior**:
- Suppresses all output that doesn't have `overrideQuiet: true`
- Most informational messages are hidden
- Error messages and final results still displayed
- Useful for scripting where only output is needed

**Interaction with other modes**:
- Can be combined with `--interactive false` for minimal output
- `--question` / `-q` flags implicitly enable quiet mode

**Implementation**: Uses `ConsoleHelpers.IsQuiet()` checks throughout codebase. Only messages with `overrideQuiet: true` are displayed.

**Source Code**: See [proof document](cycod-chat-layer-6-proof.md#quiet-mode) for line numbers.

---

### 3. Verbose Mode

**Option**: `--verbose`

**Purpose**: Enable detailed diagnostic output showing internal operations.

**Default**: `false`

**Behavior**:
- Shows detailed MCP server operations
- Displays function call parameters and results
- Logs file operations and process executions
- Shows token counting and conversation management details
- Displays cycodmd and cycodgr wrapper operations

**Verbose Output Examples**:
- MCP server discovery and selection
- Function registration and invocation
- Chat history loading/saving operations
- Token usage calculations
- Subprocess execution details

**Implementation**: Uses `ConsoleHelpers.IsVerbose()` and `Logger.Verbose()` throughout codebase.

**Source Code**: See [proof document](cycod-chat-layer-6-proof.md#verbose-mode) for line numbers.

---

### 4. Debug Mode

**Option**: `--debug`

**Purpose**: Enable comprehensive debugging output including all verbose information plus additional debug traces.

**Default**: `false`

**Behavior**:
- Enables all verbose output
- Shows command-line argument parsing
- Displays configuration loading details
- Logs all internal state changes
- Writes debug information to console in real-time

**Implementation**: Sets `ConsoleHelpers.ConfigureDebug(true)` which enables both debug logging and verbose output.

**Source Code**: See [proof document](cycod-chat-layer-6-proof.md#debug-mode) for line numbers.

---

### 5. Streaming Output

**Behavior**: AI responses are streamed token-by-token in real-time.

**Implementation**:
- Uses `IAsyncEnumerable<StreamingChatCompletionUpdate>` from AI SDK
- Displays each token as it arrives
- Provides immediate feedback during long-running responses
- Works in both interactive and non-interactive modes

**Control**: Not directly controllable via options; always enabled for chat responses.

**Source Code**: See [proof document](cycod-chat-layer-6-proof.md#streaming-output) for line numbers.

---

### 6. Console Output Formatting

**Color-Coded Output**:
- **User prompts**: Standard console color
- **AI responses**: White text
- **Function calls**: Formatted with indentation
- **Error messages**: Red background (via `ConsoleHelpers.WriteErrorLine()`)
- **Warnings**: Yellow background (via `ConsoleHelpers.WriteWarningLine()`)
- **Success messages**: Green text
- **Debug info**: Dark gray or magenta

**Line Spacing**:
- Double newlines (`\n\n`) between turns in non-quiet mode
- Single newlines (`\n`) in quiet mode
- Extra spacing around function call results

**Source Code**: See [proof document](cycod-chat-layer-6-proof.md#console-output-formatting) for line numbers.

---

### 7. Function Call Display

**Behavior**: Shows function invocations and their results during chat execution.

**Format**:
```
FunctionName(
  param1: value1,
  param2: value2
) → Result:
  [function output]
```

**Control**:
- Always displayed in non-quiet mode
- Hidden in quiet mode unless `overrideQuiet: true`
- Detailed parameter display in verbose mode

**Source Code**: See [proof document](cycod-chat-layer-6-proof.md#function-call-display) for line numbers.

---

### 8. Token Usage Display

**Behavior**: Shows token counts after each AI interaction.

**Format**:
```
Tokens: 1234 in, 567 out
```

**When Displayed**:
- After each turn in interactive mode
- At the end of execution in non-interactive mode
- Hidden in quiet mode

**Implementation**: Tracked in `_totalTokensIn` and `_totalTokensOut` private fields in `ChatCommand`.

**Source Code**: See [proof document](cycod-chat-layer-6-proof.md#token-usage-display) for line numbers.

---

### 9. Console Title Updates

**Behavior**: Updates the terminal window title with the conversation title.

**When Updated**:
- After loading existing chat history (shows loaded conversation title)
- After `/title` slash command execution
- When auto-generating titles

**Implementation**: Uses `ConsoleTitleHelper.UpdateWindowTitle()` with conversation metadata.

**Fallback**: Fails gracefully in environments where console title cannot be set (CI/CD, certain terminals).

**Source Code**: See [proof document](cycod-chat-layer-6-proof.md#console-title-updates) for line numbers.

---

### 10. Multi-Line Input Detection

**Behavior**: Automatically detects and handles multi-line input during interactive mode.

**Triggers**:
- Input starts with ` ``` ` (triple backticks)
- Input starts with `"""` (triple quotes)
- User explicitly enters multi-line mode

**Termination**: Reads lines until matching closing delimiter is found.

**Implementation**: Uses `InteractivelyReadMultiLineInput()` method.

**Source Code**: See [proof document](cycod-chat-layer-6-proof.md#multi-line-input-detection) for line numbers.

---

## Option Interaction Matrix

| Option | Interactive | Quiet | Verbose | Debug | Result |
|--------|-------------|-------|---------|-------|--------|
| Default | ✅ | ❌ | ❌ | ❌ | Standard interactive chat |
| `--interactive false` | ❌ | ❌ | ❌ | ❌ | Batch processing, normal output |
| `--quiet` | ✅ | ✅ | ❌ | ❌ | Interactive with minimal output |
| `--quiet --interactive false` | ❌ | ✅ | ❌ | ❌ | Silent batch processing |
| `--verbose` | ✅ | ❌ | ✅ | ❌ | Detailed diagnostic output |
| `--debug` | ✅ | ❌ | ✅ | ✅ | Maximum diagnostic output |
| `--question` | ❌ | ✅ | ❌ | ❌ | Shorthand for quiet + non-interactive |

---

## Global vs. Command-Specific Options

### Global Options (Apply to All Commands)
- `--interactive [true|false]`
- `--quiet`
- `--verbose`
- `--debug`

Defined in: `src/common/CommandLine/CommandLineOptions.cs`

### Chat-Specific Display Behavior
- Real-time streaming output
- Function call formatting
- Token usage display
- Console title updates
- Multi-line input handling

Implemented in: `src/cycod/CommandLineCommands/ChatCommand.cs`

---

## Command-Line Parsing Details

### Parsing Location

**File**: `src/common/CommandLine/CommandLineOptions.cs`

**Methods**:
- `TryParseGlobalCommandLineOptions()` - Lines 331-403
  - `--interactive` - Lines 334-340
  - `--debug` - Lines 341-345
  - `--verbose` - Lines 346-349
  - `--quiet` - Lines 350-353

### Shortcuts and Aliases

**Question/Query Mode**:
- `--question` or `-q` → Enables `--quiet` + `--interactive false`
- `--questions` → Same as above, but for multiple inputs
- Parsed in `CycoDevCommandLineOptions.cs`, lines 506-510 and 531-535

---

## Implementation Classes

### Core Classes

1. **ConsoleHelpers** (`src/common/Helpers/ConsoleHelpers.cs`)
   - Manages quiet/verbose/debug state
   - Provides `Write()`, `WriteLine()` methods with `overrideQuiet` parameter
   - Handles color output

2. **Logger** (`src/common/Logger/Logger.cs`)
   - Provides `Logger.Verbose()` for diagnostic messages
   - Integrates with file and console logging

3. **ConsoleTitleHelper** (`src/cycod/Helpers/ConsoleTitleHelper.cs`)
   - Updates terminal window title
   - Handles platform differences and failures gracefully

4. **ChatCommand** (`src/cycod/CommandLineCommands/ChatCommand.cs`)
   - Implements streaming display
   - Manages interactive input loops
   - Controls token display
   - Handles function call formatting

---

## Display Control Flow

```
User Invocation
    ↓
CommandLineOptions.Parse() → Sets Interactive, Quiet, Verbose, Debug flags
    ↓
ProgramRunner.RunAsync() → Calls ConsoleHelpers.Configure(debug, verbose, quiet)
    ↓
ChatCommand.ExecuteAsync(interactive) → Receives effective interactive flag
    ↓
[Interactive Mode]
    ↓
Loop: Read input → Send to AI → Stream response → Display tokens
    ↑_______________________________________________________________|
    
[Non-Interactive Mode]
    ↓
Process all --input/--inputs → Send to AI → Stream response → Display tokens → Exit
```

---

## Related Documentation

- [Layer 1: Target Selection](cycod-chat-layer-1.md) - Input sources
- [Layer 7: Output Persistence](cycod-chat-layer-7.md) - Saving output
- [Layer 8: AI Processing](cycod-chat-layer-8.md) - AI interaction
- [Proof Document](cycod-chat-layer-6-proof.md) - Source code evidence

---

## Summary

Layer 6 (Display Control) for the `chat` command provides comprehensive control over how information is presented:

- **4 verbosity levels**: Interactive, Quiet, Verbose, Debug
- **Real-time streaming**: Token-by-token AI response display
- **Color-coded output**: Different message types use different colors
- **Function call formatting**: Structured display of tool invocations
- **Token tracking**: Usage statistics after each turn
- **Console title updates**: Terminal title reflects conversation
- **Multi-line input**: Automatic detection and handling
- **Platform-aware**: Handles redirection and terminal limitations

These controls provide flexibility for both interactive exploration and automated scripting scenarios.
