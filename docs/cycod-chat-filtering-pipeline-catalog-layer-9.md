# cycod `chat` Command - Layer 9: Actions on Results

## Overview

**Layer 9** focuses on actions performed on or with the conversation results. For the `chat` command, this layer encompasses all the **executable operations** that can be triggered during or after chat interactions.

## Command

```bash
cycod chat [options]
cycod [options]  # chat is the default command
```

## Purpose

Layer 9 (Actions on Results) in the `chat` command enables:
1. **Interactive Conversation** - Conduct real-time conversations with AI
2. **Function Calling / Tool Execution** - Execute tools and functions during conversation
3. **Slash Commands** - Execute special in-chat commands
4. **History Management** - Save, load, and clear conversation history
5. **Title Generation** - Auto-generate and refresh conversation titles
6. **File Operations** - Perform file I/O through tools
7. **Process Execution** - Execute shell commands and background processes
8. **Image Processing** - Capture screenshots and process images
9. **Code Exploration** - Search and analyze codebases

## Layer 9 Categories

### 1. Interactive Conversation Execution

The primary action is conducting an interactive chat session.

**Mechanism**: The main execution loop (lines 155-201) repeatedly:
1. Prompts for user input
2. Processes input through slash command router
3. Sends to AI for response generation
4. Displays assistant responses
5. Auto-saves conversation state

**Source**: `src/cycod/CommandLineCommands/ChatCommand.cs`
- Lines 54-216: `ExecuteAsync()` method
- Lines 155-201: Main conversation loop

**Command-line control**:
- `--interactive`: Enable/disable interactive mode (default: enabled)
- `--input`, `--instruction`: Provide non-interactive input
- No input + no interactive = exits with warning

See [Layer 9 Proof - Interactive Conversation](cycod-chat-filtering-pipeline-catalog-layer-9-proof.md#interactive-conversation)

---

### 2. Function / Tool Calling

Execute functions and tools during conversation to extend AI capabilities.

**Built-in Tool Categories**:
1. **Date/Time Tools** - Get current date, time, timestamp
2. **Shell Command Tools** - Execute shell commands (bash, cmd, powershell)
3. **Background Process Tools** - Start, monitor, control long-running processes
4. **Editor Tools** - String replacement and text editing
5. **Thinking Tool** - Structured reasoning and thought processes
6. **Code Exploration** - Search files, read code, analyze structure
7. **Image Tools** - Add images to conversation
8. **Screenshot Tools** - Capture and add screenshots
9. **Shell/Process Management** - Advanced process control
10. **GitHub Search** - Search GitHub repositories and code

**MCP (Model Context Protocol) Tools**:
- Dynamically loaded from configuration
- Support stdio and SSE transports
- Custom tools via `--with-mcp` command

**Source**: `src/cycod/CommandLineCommands/ChatCommand.cs`
- Lines 103-116: Function factory initialization
- Lines 188-192: `CompleteChatStreamingAsync()` with function call handling

**Command-line control**:
- `--use-mcps [name...]`, `--mcp`: Enable MCP servers
- `--no-mcps`: Disable MCP servers
- `--with-mcp <command> [args...]`: Add inline MCP server

See [Layer 9 Proof - Function Calling](cycod-chat-filtering-pipeline-catalog-layer-9-proof.md#function-calling)

---

### 3. Slash Commands

Execute special commands within the chat session.

#### 3.1. `/prompt` Command

Load and insert prompt templates.

**Syntax**: `/prompt <name>` or `/<name>` (shorthand)

**Action**: Reads prompt template from file and returns content

**Source**: `src/cycod/SlashCommands/SlashPromptCommandHandler.cs`

#### 3.2. `/title` Commands

Manage conversation titles.

**Commands**:
- `/title <text>` - Set title explicitly
- `/title generate` - AI-generate title from conversation
- `/title refresh` - Regenerate title

**Action**: Updates conversation metadata and saves to file

**Auto-title**: Enabled by `--auto-generate-title` or config setting

**Source**: `src/cycod/SlashCommands/SlashTitleCommandHandler.cs`

#### 3.3. `/cycodmd` Command

Execute cycodmd commands inline.

**Syntax**: `/cycodmd <args...>`

**Action**: Spawns cycodmd process, captures output, adds to conversation

**Source**: `src/cycod/SlashCommands/SlashCycoDmdCommandHandler.cs`

#### 3.4. `/screenshot` Command

Capture and add screenshot to conversation.

**Syntax**: `/screenshot`

**Action**: Captures screen, saves to file, adds as image to next message

**Source**: `src/cycod/SlashCommands/SlashScreenshotCommandHandler.cs`

#### 3.5. `/save` Command

Save conversation history.

**Syntax**: 
- `/save` - Save to default file (chat-history.jsonl)
- `/save <filename>` - Save to specific file

**Action**: Writes conversation to JSONL file

**Source**: `src/cycod/CommandLineCommands/ChatCommand.cs`
- Lines 379-389: `HandleSaveChatHistoryCommand()`

#### 3.6. `/clear` Command

Clear conversation history.

**Syntax**: `/clear`

**Action**: Removes all messages from conversation, resets token counts

**Source**: `src/cycod/CommandLineCommands/ChatCommand.cs`
- Lines 370-377: `HandleClearChatHistoryCommand()`

#### 3.7. `/cost` Command

Display token usage statistics.

**Syntax**: `/cost`

**Action**: Shows input/output token counts

**Source**: `src/cycod/CommandLineCommands/ChatCommand.cs`
- Lines 391-395: `HandleShowCostCommand()`

#### 3.8. `/help` Command

Display help for chat commands.

**Syntax**: `/help`

**Action**: Shows available slash commands and their usage

**Source**: `src/cycod/CommandLineCommands/ChatCommand.cs`
- Lines 397-428: `HandleHelpCommand()`

**Slash Command Registration**:

The slash command router is initialized with handlers in `ExecuteAsync()`:
- Line 62: `SlashPromptCommandHandler` (sync)
- Line 64-65: `SlashTitleCommandHandler` (sync)
- Line 68: `SlashCycoDmdCommandHandler` (async)
- Line 69: `SlashScreenshotCommandHandler` (async)

See [Layer 9 Proof - Slash Commands](cycod-chat-filtering-pipeline-catalog-layer-9-proof.md#slash-commands)

---

### 4. History Management Actions

Save and load conversation state.

#### 4.1. Auto-save

Automatically saves conversation after each exchange.

**Triggers**:
- After assistant response (line 201)
- After slash command that needs save (line 327)
- Before exit (implicit in finally block)

**Files**:
- `AutoSaveOutputChatHistory` (from config or default pattern)
- `OutputChatHistory` (from `--output-chat-history` option)

**Source**: `src/cycod/CommandLineCommands/ChatCommand.cs`
- Lines 201, 327-336: Auto-save logic
- Lines 81-84: File path grounding

#### 4.2. Load History

Load existing conversation on startup.

**Options**:
- `--input-chat-history <file>`: Load specific file
- `--continue`: Load most recent chat history
- `--chat-history <file>`: Both input and output

**Source**: `src/cycod/CommandLineCommands/ChatCommand.cs`
- Lines 135-146: Load chat history

#### 4.3. Clear History

Remove all conversation messages.

**Command**: `/clear`

**Effect**: 
- Clears all messages
- Resets token counts
- Retains system prompt and tools

**Source**: Lines 370-377

See [Layer 9 Proof - History Management](cycod-chat-filtering-pipeline-catalog-layer-9-proof.md#history-management)

---

### 5. Title Generation Actions

Automatically or manually generate conversation titles.

#### 5.1. Auto-title on First Exchange

When enabled, generates title after first conversation exchange.

**Option**: `--auto-generate-title [true|false]`

**Config**: `App.AutoGenerateTitles`

**Source**: 
- `src/cycod/SlashCommands/SlashTitleCommandHandler.cs`
- Configuration in `CycoDevCommandLineOptions.cs` lines 684-691

#### 5.2. Manual Title Commands

Explicitly set or generate titles.

**Commands**:
- `/title <text>` - Set directly
- `/title generate` - AI-generate
- `/title refresh` - Regenerate

**Source**: `SlashTitleCommandHandler.cs`

See [Layer 9 Proof - Title Generation](cycod-chat-filtering-pipeline-catalog-layer-9-proof.md#title-generation)

---

### 6. File Operations Actions

Read, write, and manipulate files during conversation.

**Available through tools**:

1. **File Reading** (Code Exploration):
   - `FindFiles` - Locate files by pattern
   - `ViewFile` - Read file content with line ranges
   - `ViewFiles` - Read multiple files
   - `SearchInFiles` - Search file content

2. **File Editing** (Editor Tools):
   - `ReplaceOneInFile` - Replace single occurrence
   - `ReplaceMultipleInFile` - Replace multiple occurrences
   - `ReplaceAllInFiles` - Bulk replace across files
   - `Insert` - Insert content at line
   - `CreateFile` - Create new file
   - `ReplaceFileContent` - Replace entire file
   - `UndoEdit` - Undo last edit

3. **Image Files**:
   - `AddImageToConversation` - Add image file
   - `TakeScreenshot` - Capture and save screenshot

**Source**: `src/cycod/CommandLineCommands/ChatCommand.cs`
- Line 109: `CodeExplorationHelperFunctions`
- Line 107: `StrReplaceEditorHelperFunctions`
- Line 110: `ImageHelperFunctions`
- Line 111: `ScreenshotHelperFunctions`

See [Layer 9 Proof - File Operations](cycod-chat-filtering-pipeline-catalog-layer-9-proof.md#file-operations)

---

### 7. Process Execution Actions

Execute commands and manage processes.

**Tool Categories**:

1. **Shell Commands** (one-time execution):
   - `RunShellCommand` - Execute command, wait for completion
   - `RunBashCommand`, `RunCmdCommand`, `RunPowershellCommand` - Shell-specific

2. **Named Shells** (persistent shells):
   - `CreateNamedShell` - Start persistent shell
   - `ExecuteInShell` - Run command in existing shell
   - `GetShellOrProcessOutput` - Retrieve output
   - `SendInputToShellOrProcess` - Send input
   - `TerminateShellOrProcess` - Stop shell
   - `WaitForShellOrProcessExit` - Wait for completion
   - `WaitForShellOrProcessOutput` - Wait for output pattern

3. **Background Processes** (long-running):
   - `StartNamedProcess` - Start independent process
   - `GetShellOrProcessOutput` - Retrieve output
   - `SendInputToShellOrProcess` - Send input
   - `TerminateShellOrProcess` - Stop process
   - `WaitForShellOrProcessOutput` - Wait for output
   - `ListShellsAndProcesses` - List active shells/processes

**Source**: `src/cycod/CommandLineCommands/ChatCommand.cs`
- Line 105: `ShellCommandToolHelperFunctions`
- Line 106: `BackgroundProcessHelperFunctions`
- Line 112: `ShellAndProcessHelperFunctions`

See [Layer 9 Proof - Process Execution](cycod-chat-filtering-pipeline-catalog-layer-9-proof.md#process-execution)

---

### 8. Image Processing Actions

Add and process images in conversation.

**Methods**:

1. **Command-line images**:
   - `--image <pattern>` - Add image files matching pattern
   - Resolved before each message (line 185)
   - Cleared after use (line 186)

2. **Screenshot capture**:
   - `/screenshot` slash command
   - `TakeScreenshot` tool
   - Windows-only functionality

3. **Dynamic image addition**:
   - `AddImageToConversation` tool
   - Add images during conversation

**Source**:
- Line 110: `ImageHelperFunctions` 
- Line 111: `ScreenshotHelperFunctions`
- Lines 185-186: Image pattern resolution
- Command line option: Lines 668-673 in `CycoDevCommandLineOptions.cs`

See [Layer 9 Proof - Image Processing](cycod-chat-filtering-pipeline-catalog-layer-9-proof.md#image-processing)

---

### 9. Code Exploration Actions

Search and analyze codebase during conversation.

**Available Tools**:

1. **File Discovery**:
   - `FindFiles` - Find files by glob/regex patterns
   - Supports time filters, exclusions, content filters

2. **File Viewing**:
   - `ViewFile` - View single file with line ranges
   - `ViewFiles` - View multiple files
   - Supports filtering by line content

3. **Content Search**:
   - `SearchInFiles` - Search file content with regex
   - Context lines before/after matches
   - Exclusion patterns

4. **GitHub Search**:
   - `SearchGitHub` - Search GitHub repositories and code
   - Repository fingerprinting
   - Multi-language search

**Source**:
- Line 109: `CodeExplorationHelperFunctions`
- Line 113: `GitHubSearchHelperFunctions`

See [Layer 9 Proof - Code Exploration](cycod-chat-filtering-pipeline-catalog-layer-9-proof.md#code-exploration)

---

## Exit Actions

When exiting the chat session:

1. **Show pending notifications** - Display any queued notifications (line 165)
2. **Save conversation** - Auto-save to configured files
3. **Dispose resources** - Clean up AI client, MCP connections (lines 206-215)
4. **Return exit code** - 0 for success, 1 for errors

**Source**: `src/cycod/CommandLineCommands/ChatCommand.cs`
- Lines 203-216: Exit and cleanup

---

## Summary Table

| Action Category | Trigger | Options/Commands | Proof Link |
|----------------|---------|------------------|------------|
| **Interactive Conversation** | Main loop | `--interactive`, `--input` | [Proof](cycod-chat-filtering-pipeline-catalog-layer-9-proof.md#interactive-conversation) |
| **Function Calling** | During conversation | `--use-mcps`, `--with-mcp` | [Proof](cycod-chat-filtering-pipeline-catalog-layer-9-proof.md#function-calling) |
| **Slash Commands** | User input | `/prompt`, `/title`, `/cycodmd`, `/screenshot`, `/save`, `/clear`, `/cost`, `/help` | [Proof](cycod-chat-filtering-pipeline-catalog-layer-9-proof.md#slash-commands) |
| **History Management** | Auto/manual | `--input-chat-history`, `--output-chat-history`, `--continue`, `/save`, `/clear` | [Proof](cycod-chat-filtering-pipeline-catalog-layer-9-proof.md#history-management) |
| **Title Generation** | Auto/manual | `--auto-generate-title`, `/title` | [Proof](cycod-chat-filtering-pipeline-catalog-layer-9-proof.md#title-generation) |
| **File Operations** | Tool calls | Various file tools | [Proof](cycod-chat-filtering-pipeline-catalog-layer-9-proof.md#file-operations) |
| **Process Execution** | Tool calls | Shell/process tools | [Proof](cycod-chat-filtering-pipeline-catalog-layer-9-proof.md#process-execution) |
| **Image Processing** | Options/tools | `--image`, `/screenshot` | [Proof](cycod-chat-filtering-pipeline-catalog-layer-9-proof.md#image-processing) |
| **Code Exploration** | Tool calls | Code exploration tools | [Proof](cycod-chat-filtering-pipeline-catalog-layer-9-proof.md#code-exploration) |

---

## Data Flow

```
User Input
    ↓
Slash Command Router ──→ Execute slash command action
    ↓ (if not handled)
AI Processing (Layer 8)
    ↓
Function/Tool Calls ──→ Execute tool actions
    ↓
Assistant Response
    ↓
Auto-save History ──→ Save action
    ↓
Check Notifications ──→ Display pending notifications
    ↓
Next iteration or Exit ──→ Cleanup actions
```

---

## Key Mechanisms

### 1. Action Dispatching

Actions are dispatched through multiple mechanisms:

1. **Slash Command Router** (lines 62-69, 318-340)
   - Checks if input starts with `/`
   - Routes to registered handlers
   - Returns result indicating skip assistant, response text, needs save

2. **Built-in Command Handling** (lines 343-359)
   - Checks for `/save`, `/clear`, `/cost`, `/help`
   - Directly handles within ChatCommand

3. **Tool Execution** (lines 188-192)
   - AI requests tool via function calling
   - Function factory dispatches to tool implementation
   - Result returned to AI

### 2. Execution Context

All actions execute with:
- **Template variables** - Access to `{var}` expansions
- **Named values** - AGENTS.md content, user-defined variables
- **Current conversation** - Full history and metadata
- **Function factory** - Access to all registered tools
- **MCP clients** - Active connections to MCP servers

### 3. Result Persistence

Actions can persist results through:
- **Auto-save** - Conversation saved after each exchange
- **Manual save** - `/save` command
- **Trajectory files** - Detailed action log
- **File outputs** - Tool-generated files
- **Console output** - Visible feedback

---

## Navigation

- [← Layer 8: AI Processing](cycod-chat-filtering-pipeline-catalog-layer-8.md)
- [↑ Back to chat Command README](cycod-chat-filtering-pipeline-catalog-README.md)
- [↑ Back to cycod CLI README](cycod-filtering-pipeline-catalog-README.md)
