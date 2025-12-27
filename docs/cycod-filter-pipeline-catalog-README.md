# cycod CLI - Filtering Pipeline Catalog

## Overview

This catalog documents the 9-layer filtering pipeline for all commands in the **cycod** CLI tool. Each command is analyzed across the conceptual layers that control how targets are selected, filtered, processed, and displayed.

## The 9 Conceptual Layers

1. **TARGET SELECTION** - What to search/operate on (primary search space)
2. **CONTAINER FILTER** - Which containers to include/exclude based on properties
3. **CONTENT FILTER** - What content within containers to show
4. **CONTENT REMOVAL** - What content to actively remove from display
5. **CONTEXT EXPANSION** - How to expand around matches
6. **DISPLAY CONTROL** - How to present results
7. **OUTPUT PERSISTENCE** - Where to save results
8. **AI PROCESSING** - AI-assisted analysis and processing
9. **ACTIONS ON RESULTS** - What to do with results

## cycod Commands

The cycod CLI has the following commands:

### Primary Commands

- **[chat](cycod-chat-README.md)** - Interactive AI chat (default command)
- **[help](cycod-help-README.md)** - Display help information
- **[version](cycod-version-README.md)** - Display version information

### Configuration Commands

- **[config list](cycod-config-list-README.md)** - List configuration entries
- **[config get](cycod-config-get-README.md)** - Get configuration value
- **[config set](cycod-config-set-README.md)** - Set configuration value
- **[config clear](cycod-config-clear-README.md)** - Clear configuration value
- **[config add](cycod-config-add-README.md)** - Add to configuration list
- **[config remove](cycod-config-remove-README.md)** - Remove from configuration list

### Alias Commands

- **[alias list](cycod-alias-list-README.md)** - List aliases
- **[alias get](cycod-alias-get-README.md)** - Get alias content
- **[alias add](cycod-alias-add-README.md)** - Add/update alias
- **[alias delete](cycod-alias-delete-README.md)** - Delete alias

### Prompt Commands

- **[prompt list](cycod-prompt-list-README.md)** - List saved prompts
- **[prompt get](cycod-prompt-get-README.md)** - Get prompt content
- **[prompt create](cycod-prompt-create-README.md)** - Create/update prompt
- **[prompt delete](cycod-prompt-delete-README.md)** - Delete prompt

### MCP (Model Context Protocol) Commands

- **[mcp list](cycod-mcp-list-README.md)** - List MCP server configurations
- **[mcp get](cycod-mcp-get-README.md)** - Get MCP server configuration
- **[mcp add](cycod-mcp-add-README.md)** - Add MCP server configuration
- **[mcp remove](cycod-mcp-remove-README.md)** - Remove MCP server configuration

### GitHub Commands

- **[github login](cycod-github-login-README.md)** - Authenticate with GitHub
- **[github models](cycod-github-models-README.md)** - List available GitHub models


---

## Layer 8 Completion Status

### ✅ COMPLETE - AI Processing Layer Documented

Layer 8 documents how commands use AI models to process, analyze, and generate content.

| Command Group | Layer 8 Doc | Layer 8 Proof | Status |
|---------------|-------------|---------------|--------|
| **chat** | [📝 Complete](cycod-chat-filtering-pipeline-catalog-layer-8.md) | [📝 Complete](cycod-chat-filtering-pipeline-catalog-layer-8-proof.md) | ✅ Comprehensive AI processing (9 providers, MCP tools, multi-modal) |
| **config** | [📝 Complete](cycod-config-filtering-pipeline-catalog-layer-8.md) | [📝 Complete](cycod-config-filtering-pipeline-catalog-layer-8-proof.md) | ⚪ Not applicable - CRUD operations |
| **alias/prompt/mcp/github** | [📝 Complete](cycod-alias-prompt-mcp-github-filtering-pipeline-catalog-layer-8.md) | [📝 Complete](cycod-alias-prompt-mcp-github-filtering-pipeline-catalog-layer-8-proof.md) | ⚪ Not applicable - Resource management |

### Layer 8 Key Findings

**chat command** (only command with AI processing):
- **9 AI Providers**: Anthropic, Azure Anthropic, AWS Bedrock, Azure OpenAI, Google Gemini, Grok, OpenAI, Test, GitHub Copilot
- **System Prompt Management**: `--system-prompt`, `--add-system-prompt` with template expansion and AGENTS.md integration
- **User Prompt Management**: `--add-user-prompt`, `--prompt` (auto-prefixes `/` for slash commands)
- **Input Instructions**: `--input`, `--instruction`, `--question`, `-q` with file expansion and stdin auto-loading
- **Template Processing**: `--var`, `--vars`, `--use-templates`, `--no-templates` with `{variable}` syntax
- **Tool Integration**: `--use-mcps`, `--mcp`, `--with-mcp`, `--no-mcps` for MCP servers + 11 built-in function groups
- **Conversation History**: `--chat-history`, `--input-chat-history`, `--continue`, `--output-chat-history` with JSONL format
- **Multi-Modal Input**: `--image` with glob pattern resolution for vision models
- **Token Management**: Separate budgets for prompt (80K), tools (40K), chat history (100K), and output tokens
- **Title Generation**: `--auto-generate-title` for conversation organization
- **Foreach Loops**: `--foreach VAR in VALUE1 VALUE2 ...` for batch processing with variable substitution

**config/alias/prompt/mcp/github commands** (no AI processing):
- Administrative CRUD operations on files/configurations
- **Enable** AI processing in other commands (set providers, keys, prompts, tools)
- **Do not perform** AI processing themselves
- Simple synchronous file I/O or HTTP requests
- No ChatClient, no conversation state, no function calling, no token management

### AI Processing Architecture

```
Command Line Options
        ↓
    Provider Selection (--use-anthropic, --use-openai, etc.)
        ↓
    System Prompt Setup (--system-prompt, AGENTS.md)
        ↓
    Tool Registration (Built-in + MCP servers)
        ↓
    ChatClient Initialization
        ↓
    FunctionCallingChat Creation
        ↓
    Conversation Loop:
        ├→ User Input (--input, interactive)
        ├→ Template Expansion ({variables})
        ├→ Image Resolution (--image patterns)
        ├→ AI API Call (streaming)
        ├→ Function Calls (MCP tools)
        ├→ Response Generation
        ├→ History Saving (JSONL)
        └→ Trajectory Logging (Markdown)
```

### Provider Configuration Mechanism

1. **Command-line options** → Set preferred provider (`--use-anthropic`)
2. **ConfigStore** → Stores provider name and API keys
3. **Environment variable** → `CYCOD_AI_PROVIDER` set for session
4. **ChatClientFactory** → Reads config/env to create appropriate `IChatClient`
5. **FunctionCallingChat** → Wraps client with tool-calling capabilities
6. **Conversation** → Manages message history, token budgets, tool results

### Token Budget Strategy

| Budget Type | Default | Purpose |
|-------------|---------|---------|
| **Prompt** | 80,000 tokens | System prompt + user prompts (persistent) |
| **Tool** | 40,000 tokens | Function definitions available to AI |
| **Chat** | 100,000 tokens | Conversation history (pruned FIFO) |
| **Output** | Provider default | Max tokens in AI response |

**Pruning Algorithm**:
1. Protect system prompt and persistent messages
2. Prune oldest non-persistent messages first
3. Summarize large tool results if needed
4. Error if protected content exceeds budget

### MCP Server Integration

**Two ways to add MCP servers**:
1. **Configured** (`--use-mcps`): Load from config files (managed by `mcp add` command)
2. **Ad-hoc** (`--with-mcp`): Inline server definition for one-off use

**Transport types**:
- **stdio**: Execute command, communicate via stdin/stdout
- **sse**: Connect to URL, use Server-Sent Events

**Function registration flow**:
```
MCP Server Config
    ↓
Connect to Server (stdio/sse)
    ↓
List Available Tools
    ↓
Register with McpFunctionFactory
    ↓
AI Can Call Tools
    ↓
Tool Results Returned to AI
```

### Multi-Modal Support

**Image processing**:
- `--image` accepts glob patterns (e.g., `*.png`, `screenshots/*.jpg`)
- Patterns resolved to actual file paths before AI call
- Files encoded (typically base64) by provider
- Sent alongside text prompt for vision model analysis
- Supports: PNG, JPEG, WebP, GIF, BMP (provider-dependent)

### Conversation History Format

**JSONL (JSON Lines) format**:
```jsonl
{"metadata": {"title": "Bug Analysis", "created": "2024-01-15T10:30:00Z"}}
{"role": "user", "content": "Analyze bug.cs"}
{"role": "assistant", "content": "I'll analyze the code..."}
{"role": "user", "content": "What about security?"}
{"role": "assistant", "content": "Security analysis:..."}
```

**Benefits**:
- Incremental writes (append-only)
- Crash-safe (incomplete last line ignored)
- Easy to parse line-by-line
- Git-friendly diffs

### Slash Command Integration

**Slash commands** (Layer 8 processors):
- `/prompt <name>` - Load saved prompt template
- `/title generate` - AI-generate conversation title
- `/title refresh` - Regenerate title from current conversation
- `/cycodmd <args>` - Execute cycodmd with AI result handling
- `/screenshot` - Capture screen and add to conversation

**Slash command handlers** registered before conversation loop (ChatCommand.cs L61-69).

---

## Command Categories by Filtering Complexity

### High Complexity (Full Pipeline)
- **chat** - Uses all 9 layers extensively

### Medium Complexity (Partial Pipeline)
- **config list** - Uses layers 1, 2, 6, 7
- **alias list** - Uses layers 1, 2, 6, 7
- **prompt list** - Uses layers 1, 2, 6, 7
- **mcp list** - Uses layers 1, 2, 6, 7

### Low Complexity (Minimal Pipeline)
- **config get/set/clear/add/remove** - Primarily layers 1, 6, 9
- **alias get/add/delete** - Primarily layers 1, 6, 9
- **prompt get/create/delete** - Primarily layers 1, 6, 9
- **mcp get/add/remove** - Primarily layers 1, 6, 9
- **github login/models** - Primarily layers 6, 9
- **help** - Display only (layer 6)
- **version** - Display only (layer 6)

## Documentation Structure

For each command, the following files exist:

- **{command}-README.md** - Overview and links to all layers
- **{command}-layer-{1-9}.md** - Detailed layer documentation
- **{command}-layer-{1-9}-proof.md** - Source code evidence for each layer

## Key Findings

### Most Complex Command
The **chat** command has the richest filtering pipeline with options spanning all 9 layers.

### Consistent Patterns Across Commands
- Scope filtering (`--global`, `--user`, `--local`, `--any`) appears in config, alias, prompt, and mcp commands
- Display control options (`--verbose`, `--quiet`, `--debug`) are global
- AI provider selection options are chat-specific but could apply to other commands

### Unique Features
- **chat**: Template variables, foreach loops, MCP integration, multi-modal input (images)
- **mcp add**: Complex configuration with transport types, args, environment variables

## Source Code References

### Command Line Parser
- **File**: `src/cycod/CommandLine/CycoDevCommandLineOptions.cs`
- **Lines 56-82**: Command name mapping
- **Lines 85-93**: Option parsing dispatch
- **Lines 95-210**: Positional argument parsing

### Command Implementations
- **Directory**: `src/cycod/Commands/`
- Each command has its own implementation file

### Helper Classes
- **ConfigStore**: `src/common/Config/ConfigStore.cs`
- **KnownSettings**: `src/common/Config/KnownSettings.cs`
- **FileHelpers**: `src/common/Helpers/FileHelpers.cs`
- **ConsoleHelpers**: `src/common/Helpers/ConsoleHelpers.cs`

## Navigation

- [Back to Main Catalog](CLI-Filtering-Patterns-Catalog.md)
- [Chat Command Details](cycod-chat-README.md) - Start with the most complex command

---

---

## Layer 7 Completion Status

### ✅ COMPLETE - Output Persistence Layer Documented

Layer 7 documents where and how results are saved to persistent storage for all cycod commands.

| Command Group | Layer 7 Doc | Layer 7 Proof | Status |
|---------------|-------------|---------------|--------|
| **chat** | [📝 Complete](cycod-chat-filtering-pipeline-catalog-layer-7.md) | [📝 Complete](cycod-chat-filtering-pipeline-catalog-layer-7-proof.md) | ✅ Comprehensive persistence (chat history, trajectory, auto-save) |
| **config** | [📝 Complete](cycod-config-filtering-pipeline-catalog-layer-7.md) | [📝 Complete](cycod-config-filtering-pipeline-catalog-layer-7-proof.md) | ✅ Full CRUD with multi-scope JSON files |
| **alias/prompt/mcp/github** | [📝 Complete](cycod-alias-prompt-mcp-github-filtering-pipeline-catalog-layer-7.md) | [📝 Complete](cycod-alias-prompt-mcp-github-filtering-pipeline-catalog-layer-7-proof.md) | ✅ Resource management persistence |

### Layer 7 Key Findings

**chat command** (most comprehensive):
- **Chat history files**: `.jsonl` format, incremental writing, template-based naming
- **Trajectory files**: Markdown format, execution trace capture
- **Auto-save**: Automatic timestamped backups for safety
- **Template expansion**: `{time}`, `{variable}` support in file names
- **Continue mode**: `--continue` loads most recent history automatically
- **Dual output**: Explicit output + auto-save for safety

**config commands** (structured persistence):
- **Multi-scope files**: Global, User, Local, FileName scopes
- **JSON format**: Structured key-value storage
- **Atomic operations**: Read-modify-write cycle per operation
- **List management**: `config add`/`config remove` for multi-value settings
- **Console feedback**: All operations confirm changes
- **No file locking**: Last writer wins in concurrent scenarios

**alias/prompt commands** (simple file-based):
- **Individual files**: One file per alias/prompt
- **Plain text format**: No metadata overhead
- **Directory organization**: Separate folders per scope
- **Git-friendly**: One resource per file, easy to track
- **Tokenization**: Alias content parsed into command-line arguments

**mcp/github commands** (embedded in config):
- **Config file integration**: Stored in main `config.json`
- **Structured format**: JSON objects under `mcp.servers` key
- **Transport types**: stdio (command-based) or sse (URL-based)
- **Environment support**: `--env` for environment variables
- **Token storage**: GitHub API tokens in config

### Output Persistence Patterns

| Pattern | chat | config | alias/prompt | mcp/github |
|---------|------|--------|--------------|------------|
| **Storage type** | JSONL + MD | JSON | Plain text | JSON (embedded) |
| **File naming** | Template-based | Fixed by scope | One per resource | Fixed by scope |
| **Incremental writes** | ✅ Yes | ❌ No (full rewrite) | ❌ No | ❌ No (full config rewrite) |
| **Auto-save** | ✅ Yes | ❌ N/A | ❌ N/A | ❌ N/A |
| **Template vars** | ✅ Yes | ❌ No | ❌ No | ❌ No |
| **Multi-scope** | ❌ Single file | ✅ Global/User/Local | ✅ Global/User/Local | ✅ Global/User/Local |

### Performance Characteristics

**Incremental vs. Full Rewrite:**
- **chat history**: Incremental (append-only JSONL) → Low overhead, crash-safe
- **config/mcp**: Full rewrite (entire JSON file) → Higher overhead, requires read-modify-write
- **alias/prompt**: Single small file write → Minimal overhead

**Concurrency:**
- **No file locking** across all commands → Concurrent writes may corrupt files
- **chat history**: JSON Lines format provides some resilience (last line may be incomplete)
- **config files**: JSON corruption likely with concurrent writers

**File System Load:**
- **alias/prompt**: Many small files → Directory traversal overhead for list operations
- **config/mcp**: Few large files → Single file contention, but simpler management
- **chat history**: Growing files → May need periodic cleanup/archival

---


## Layer 5 Completion Status

### ✅ COMPLETE - Context Expansion Layer Documented

Layer 5 documents how commands expand context around selected content or provide additional context for processing.

| Command Group | Layer 5 Doc | Layer 5 Proof | Status |
|---------------|-------------|---------------|--------|
| **chat** | [📝 Complete](cycod-chat-filtering-pipeline-catalog-layer-5.md) | [📝 Complete](cycod-chat-filtering-pipeline-catalog-layer-5-proof.md) | ✅ Implements rich context expansion |
| **config** | [📝 Complete](cycod-config-filtering-pipeline-catalog-layer-5.md) | [📝 Complete](cycod-config-filtering-pipeline-catalog-layer-5-proof.md) | ⚪ Not applicable - CRUD operations |
| **alias** | [📝 Complete](cycod-alias-prompt-mcp-github-filtering-pipeline-catalog-layer-5.md) | [📝 Complete](cycod-alias-prompt-mcp-github-filtering-pipeline-catalog-layer-5-proof.md) | ⚪ Not applicable - Resource management |
| **prompt** | [📝 Complete](cycod-alias-prompt-mcp-github-filtering-pipeline-catalog-layer-5.md) | [📝 Complete](cycod-alias-prompt-mcp-github-filtering-pipeline-catalog-layer-5-proof.md) | ⚪ Not applicable - Resource management |
| **mcp** | [📝 Complete](cycod-alias-prompt-mcp-github-filtering-pipeline-catalog-layer-5.md) | [📝 Complete](cycod-alias-prompt-mcp-github-filtering-pipeline-catalog-layer-5-proof.md) | ⚪ Not applicable - Resource management |
| **github** | [📝 Complete](cycod-alias-prompt-mcp-github-filtering-pipeline-catalog-layer-5.md) | [📝 Complete](cycod-alias-prompt-mcp-github-filtering-pipeline-catalog-layer-5-proof.md) | ⚪ Not applicable - Authentication/listing |

### Layer 5 Key Findings

**chat command** (full implementation):
- **Chat history loading**: `--chat-history`, `--input-chat-history`, `--continue` provide conversational context
- **System prompt context**: `--system-prompt`, `--add-system-prompt` establish AI role and constraints
- **User prompt additions**: `--add-user-prompt`, `--prompt` provide instruction context
- **Variable context**: `--var`, `--vars` provide template variable expansion
- **AGENTS.md context**: Automatic project-specific context loading
- **MCP server context**: `--use-mcps`, `--mcp`, `--with-mcp` expand AI capabilities with external tools
- **Image context**: `--image` provides multi-modal visual context
- **Trajectory context**: `--output-trajectory` captures conversation flow

**config/alias/prompt/mcp/github commands** (not applicable):
- These are CRUD and resource management operations
- No search or content analysis requiring context expansion
- Single-item operations without surrounding context
- Layer 5 patterns (lines before/after, messages around match, etc.) don't apply

### Context Expansion Philosophy

The chat command's Layer 5 differs fundamentally from file/code search tools:
- **File search tools** (cycodmd, cycodgr): Spatial expansion (N lines before/after a match)
- **Conversation tools** (cycodj): Temporal expansion (N messages before/after a search result)
- **Chat tool** (cycod chat): Contextual enrichment (adding relevant information for AI processing)

Context types in cycod chat:
1. **Historical**: Previous conversation turns
2. **Instructional**: System and user prompts
3. **Variable**: Template values
4. **Project**: AGENTS.md content
5. **Tool**: MCP server capabilities
6. **Visual**: Image attachments

---

## Previous Layer Completion Status

### Layer 3 - Content Filter

✅ **COMPLETE** - All Command Groups Documented

| Command Group | Layer 3 Doc | Layer 3 Proof |
|---------------|-------------|---------------|
| **chat** | [✅ Complete](cycod-chat-filtering-pipeline-catalog-layer-3.md) | [✅ Complete](cycod-chat-filtering-pipeline-catalog-layer-3-proof.md) |
| **config** | [✅ Complete](cycod-config-filtering-pipeline-catalog-layer-3.md) | [✅ Complete](cycod-config-filtering-pipeline-catalog-layer-3-proof.md) |
| **alias/prompt/mcp/github** | [✅ Complete](cycod-alias-prompt-mcp-github-filtering-pipeline-catalog-layer-3.md) | [✅ Complete](cycod-alias-prompt-mcp-github-filtering-pipeline-catalog-layer-3-proof.md) |

### Layer 3 Key Findings

**chat command** (most complex):
- Token-based message filtering (MaxPromptTokenTarget, MaxToolTokenTarget, MaxChatTokenTarget)
- Template variable substitution with `{variable}` syntax
- System prompt override (`--system-prompt`) and additions (`--add-system-prompt`)
- User prompt additions (`--add-user-prompt`, `--prompt`)
- Input instruction content with file expansion and stdin auto-loading
- Image content with glob pattern resolution (`--image`)

**config commands** (minimal):
- Key-based selection (`config get <key>`)
- Scope-based filtering (`config list --global/--user/--local/--any`)
- Key normalization for known settings

**alias/prompt/mcp/github commands** (resource-based):
- Name-based selection (all `get` commands: `alias get <name>`, etc.)
- Scope-based filtering (all `list` commands)
- Content tokenization (`alias add` splits command-line string into arguments)
- No template processing or pattern matching

---

**Note**: This catalog focuses on factual documentation of current implementation, not recommendations or future improvements.
