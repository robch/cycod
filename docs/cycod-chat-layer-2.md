# cycod chat - Layer 2: Container Filter

## Layer Purpose

**Container Filter** determines which "containers" of context are included or excluded when processing the chat. In the context of the `chat` command, containers include:

1. **Chat History Files** - Previous conversation history
2. **Template Files** - Prompt templates for variable substitution
3. **MCP Servers** - Model Context Protocol servers providing tools/resources
4. **Configuration Entries** - Settings that control chat behavior

This layer acts as a **pre-processing filter** that determines what contextual resources are available before content is generated or processed.

## Container Types in Chat Command

### 1. Chat History Containers

**Purpose**: Load previous conversation context

#### Options

##### `--chat-history [FILE]`
- **Behavior**: Use specified file for both input and output history
- **Default**: `chat-history.jsonl` if no file specified
- **Effect**: 
  - If file exists: loads as input history
  - Always: saves to this file as output
  - Sets `LoadMostRecentChatHistory = false`
- **Parser Location**: Lines 549-557 in `CycoDevCommandLineOptions.cs`
- **Example**: `cycod --chat-history my-conversation.jsonl`

##### `--input-chat-history FILE`
- **Behavior**: Load existing chat history file as input only
- **Validation**: File must exist
- **Effect**: Sets `InputChatHistory`, disables `LoadMostRecentChatHistory`
- **Parser Location**: Lines 558-565
- **Example**: `cycod --input-chat-history previous-chat.jsonl`

##### `--continue`
- **Behavior**: Load the most recent chat history file automatically
- **Effect**: 
  - Sets `LoadMostRecentChatHistory = true`
  - Clears `InputChatHistory`
- **Parser Location**: Lines 566-570
- **Use Case**: Resume previous conversation without specifying file
- **Example**: `cycod --continue --input "Let's continue where we left off"`

#### Container Selection Logic

The chat command uses this priority order for selecting history containers:

1. **Explicit file via `--input-chat-history`**: Highest priority, must exist
2. **Explicit file via `--chat-history`**: Used if file exists
3. **Most recent via `--continue`**: Searches for most recent `.jsonl` file
4. **No history**: Start fresh conversation

**Source Reference**: See [Layer 2 Proof](cycod-chat-layer-2-proof.md#chat-history-container-selection)

### 2. Template Containers

**Purpose**: Enable/disable template variable substitution in prompts

Templates are **files** that contain placeholder variables like `{variable}` or `{{variable}}` that get substituted with values from `--var` or `--vars` options.

#### Options

##### `--use-templates [BOOL]`
- **Behavior**: Enable or disable template substitution
- **Default**: Enabled (true)
- **Values**: `true`, `1`, `false`, `0`
- **Effect**: Sets `UseTemplates` property
- **Parser Location**: Lines 432-438
- **Example**: `cycod --use-templates false --input "{{myvar}}"`

##### `--no-templates`
- **Behavior**: Shorthand to disable templates
- **Effect**: Sets `UseTemplates = false`
- **Parser Location**: Lines 439-442
- **Example**: `cycod --no-templates --input "I want literal {{braces}}"`

#### Template Container Logic

When `UseTemplates = true`:
- All prompt text (from `--input`, `--system-prompt`, `--add-user-prompt`, etc.) is processed for variables
- Variables defined with `--var` or `--vars` are substituted
- Template files referenced with `@filename` syntax are loaded and processed
- Undefined variables remain as-is or throw errors (depending on configuration)

When `UseTemplates = false`:
- All text is treated literally
- No variable substitution occurs
- `{variable}` and `{{variable}}` are preserved as-is

**Source Reference**: See [Layer 2 Proof](cycod-chat-layer-2-proof.md#template-container-logic)

### 3. MCP Server Containers

**Purpose**: Enable AI tools and resources via Model Context Protocol

MCP servers are **external processes** that provide tools, prompts, and resources that the AI can use during conversation.

#### Options

##### `--use-mcps [NAME...]` or `--mcp [NAME...]`
- **Behavior**: Enable specific MCP servers by name, or all if no names provided
- **Default**: No MCPs enabled
- **Wildcard**: `*` enables all configured MCPs
- **Effect**: Adds names to `UseMcps` list
- **Parser Location**: Lines 443-452
- **Examples**:
  - `cycod --use-mcps` (enable all configured MCPs)
  - `cycod --use-mcps filesystem web-search` (enable specific MCPs)
  - `cycod --mcp` (shorthand, enable all)

##### `--no-mcps`
- **Behavior**: Disable all MCP servers
- **Effect**: Clears `UseMcps` list
- **Parser Location**: Lines 453-456
- **Example**: `cycod --no-mcps --input "Answer without tools"`

##### `--with-mcp COMMAND [ARGS...]`
- **Behavior**: Dynamically create and enable an MCP server with stdio transport
- **Effect**: Creates `StdioMcpServerConfig` with command and args, adds to `WithStdioMcps`
- **Auto-naming**: Servers named as `mcp-1`, `mcp-2`, etc.
- **Parser Location**: Lines 457-469
- **Example**: `cycod --with-mcp node my-mcp-server.js`

#### MCP Container Selection Logic

The MCP container selection follows this logic:

1. **Start with empty set**: No MCPs by default
2. **Process `--use-mcps` / `--mcp`**:
   - If no arguments: add `*` (all configured MCPs)
   - If arguments: add each named MCP
3. **Process `--no-mcps`**: Clear all MCPs
4. **Process `--with-mcp`**: Add dynamic MCP servers

**Final MCP Set** = (Named MCPs from config matching `UseMcps` list) + (Dynamic MCPs from `WithStdioMcps`)

**Source Reference**: See [Layer 2 Proof](cycod-chat-layer-2-proof.md#mcp-container-selection)

### 4. Configuration Containers

**Purpose**: Load configuration settings that affect chat behavior

While not directly a "filter," configuration acts as a container that influences which other containers are available and how they behave.

#### Implicit Container Loading

Configuration containers are loaded implicitly through:
- Global config: `~/.cycod/config.yaml`
- User config: `~/.config/cycod/config.yaml`
- Local config: `./.cycod/config.yaml`
- Profile files: via `--profile NAME`
- Command-line overrides: via known settings options

#### Effect on Container Filtering

Configuration affects container selection:
- **MCP Definitions**: `App.Mcp.*` settings define available MCP servers
- **Template Paths**: `App.TemplatePath` settings define where templates are found
- **History Paths**: `App.ChatHistoryPath` settings define where history files are stored
- **Provider Selection**: `App.PreferredProvider` affects which AI provider is used

**Source Reference**: See [Layer 2 Proof](cycod-chat-layer-2-proof.md#configuration-container-loading)

## Container Filtering Flow

```
┌─────────────────────────────────────────────────────────────────┐
│ 1. Parse Command Line Options                                   │
│    - Identify --chat-history, --input-chat-history, --continue  │
│    - Identify --use-templates, --no-templates                   │
│    - Identify --use-mcps, --no-mcps, --with-mcp                 │
└────────────────────┬────────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────────┐
│ 2. Load Configuration Containers                                 │
│    - Load global/user/local config files                        │
│    - Apply command-line config overrides                        │
│    - Discover available MCP servers from config                 │
└────────────────────┬────────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────────┐
│ 3. Select Chat History Container                                │
│    - If --input-chat-history: load that file                    │
│    - Else if --chat-history and exists: load that file          │
│    - Else if --continue: find most recent history file          │
│    - Else: start with empty history                             │
└────────────────────┬────────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────────┐
│ 4. Select Template Container Behavior                           │
│    - If --no-templates: disable template processing             │
│    - Else if --use-templates: enable/disable based on value     │
│    - Else: use default (enabled)                                │
└────────────────────┬────────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────────┐
│ 5. Select MCP Server Containers                                 │
│    - If --no-mcps: clear all MCPs                               │
│    - If --use-mcps with no args: add all configured MCPs        │
│    - If --use-mcps with args: add named MCPs from config        │
│    - If --with-mcp: create and add dynamic MCP servers          │
│    - Result: Final set of active MCP servers                    │
└────────────────────┬────────────────────────────────────────────┘
                     │
                     ▼
┌─────────────────────────────────────────────────────────────────┐
│ 6. Containers Ready for Content Processing (Layer 3)            │
│    - History container: loaded messages                         │
│    - Template container: processing enabled/disabled            │
│    - MCP containers: available tools/resources                  │
│    - Config container: settings applied                         │
└─────────────────────────────────────────────────────────────────┘
```

## Container Filter Examples

### Example 1: Fresh Chat with All MCPs
```bash
cycod --use-mcps --input "What tools do you have?"
```
**Containers Selected**:
- History: None (fresh start)
- Templates: Enabled (default)
- MCPs: All configured in `~/.cycod/config.yaml`

### Example 2: Continue Previous Chat Without Templates
```bash
cycod --continue --no-templates --input "{{literal}}"
```
**Containers Selected**:
- History: Most recent `.jsonl` file
- Templates: Disabled (text is literal)
- MCPs: None (default)

### Example 3: Specific History with Dynamic MCP
```bash
cycod --input-chat-history debug-session.jsonl \
      --with-mcp python debug-tool.py \
      --input "Run the debugger"
```
**Containers Selected**:
- History: `debug-session.jsonl`
- Templates: Enabled (default)
- MCPs: Dynamic MCP running `python debug-tool.py`

### Example 4: Shared History File
```bash
cycod --chat-history team-discussion.jsonl --input "My thoughts..."
```
**Containers Selected**:
- History: `team-discussion.jsonl` (if exists, loaded as input)
- Output: Will save to `team-discussion.jsonl`
- Templates: Enabled (default)
- MCPs: None (default)

## Key Insights

### Container Filter vs. Content Filter

**Container Filter (Layer 2)** operates on **external resources**:
- What history files to load
- What template system to use
- What MCP servers to enable

**Content Filter (Layer 3)** operates on **message content**:
- What text to include in prompts
- What variables to substitute
- What system prompts to use

### Implicit vs. Explicit Containers

**Implicit Containers**:
- Configuration files (always loaded based on scope)
- Template files (loaded when referenced with `@filename`)

**Explicit Containers**:
- Chat history (explicitly specified or selected)
- MCP servers (explicitly enabled or disabled)

### Container Ordering Matters

The order of container selection affects behavior:
1. **Config first**: Settings must be loaded to know what containers exist
2. **History second**: Previous context must be loaded before new content
3. **Templates third**: Variable substitution setup before content processing
4. **MCPs fourth**: Tools must be available before AI interaction begins

## Related Layers

- **Layer 1 (Target Selection)**: Determines input content that will be processed with these containers
- **Layer 3 (Content Filter)**: Uses template containers to process content
- **Layer 5 (Context Expansion)**: Uses history containers to expand context
- **Layer 8 (AI Processing)**: Uses MCP containers to provide tools to AI

## Source Code Proof

For detailed source code evidence of all assertions in this document, see:
- [Layer 2 Proof](cycod-chat-layer-2-proof.md)

## Navigation

- [Back to Chat Overview](cycod-chat-README.md)
- [Layer 1: Target Selection](cycod-chat-layer-1.md)
- [Layer 3: Content Filter](cycod-chat-layer-3.md)
- [Layer 2 Proof: Source Code Evidence](cycod-chat-layer-2-proof.md) ⭐
