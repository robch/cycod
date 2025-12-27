# cycod alias/prompt/mcp/github - Layer 3: Content Filter

**Layer Purpose**: Define what content WITHIN selected containers (alias/prompt/mcp files, command lists) to show, filter, or process.

## Overview

The `alias`, `prompt`, `mcp`, and `github` command groups in cycod manage named resources (aliases, prompts, MCP servers, GitHub credentials/models). Like `config` commands, Layer 3 content filtering is minimal since these commands operate on structured named resources rather than free-form content.

## Command Groups

### alias Commands
- **alias list**: Lists all aliases (optionally filtered by scope)
- **alias get**: Retrieves a specific alias by name
- **alias add**: Adds a new alias
- **alias delete**: Deletes an alias

### prompt Commands
- **prompt list**: Lists all saved prompts (optionally filtered by scope)
- **prompt get**: Retrieves a specific prompt by name
- **prompt create**: Creates/updates a prompt
- **prompt delete**: Deletes a prompt

### mcp Commands
- **mcp list**: Lists all MCP server configurations (optionally filtered by scope)
- **mcp get**: Retrieves a specific MCP server config by name
- **mcp add**: Adds a new MCP server configuration
- **mcp remove**: Removes an MCP server configuration

### github Commands
- **github login**: Authenticates with GitHub (stores credentials in config scope)
- **github models**: Lists available GitHub AI models

## Content Filter Mechanisms

### 1. Name-Based Content Selection (`get` commands)

All `get` commands filter to a single named resource:

**Positional Argument**:
- First positional arg: Resource name (alias name, prompt name, MCP server name)

**Processing**:
1. Name is validated (must not be empty)
2. Resource file is located in the specified scope (or any scope if `--any`)
3. Single resource content is displayed

**Commands**: `alias get`, `prompt get`, `mcp get`

**Source**: See [Layer 3 Proof](cycod-alias-prompt-mcp-github-filtering-pipeline-catalog-layer-3-proof.md#name-based-selection)

### 2. Scope-Based Content Filtering (`list` commands)

All `list` commands filter resources by scope:

**Options**:
- `--global`, `-g`: Show only global scope
- `--user`, `-u`: Show only user scope
- `--local`, `-l`: Show only local scope
- `--any`, `-a` (default): Show all scopes

**Processing**:
1. Determine which scopes to display based on option
2. For each scope, enumerate all resources in that scope's directory
3. Display each scope's resources with a location header

**Commands**: `alias list`, `prompt list`, `mcp list`

**Source**: See [Layer 3 Proof](cycod-alias-prompt-mcp-github-filtering-pipeline-catalog-layer-3-proof.md#scope-based-filtering)

### 3. Content Tokenization (alias add)

When adding an alias, the content undergoes tokenization to split into individual command-line arguments:

**Tokenization Process** (`AliasAddCommand.TokenizeAliasValue`):
```csharp
// Lines 119-166 of AliasAddCommand.cs
- Split on whitespace, respecting quotes
- Handle escaped quotes (\")
- Preserve quoted strings as single arguments
- Remove "cycod" prefix if accidentally included
```

**Purpose**: Transforms a single command-line string into individual alias file lines for proper expansion later.

**Source**: See [Layer 3 Proof](cycod-alias-prompt-mcp-github-filtering-pipeline-catalog-layer-3-proof.md#alias-content-tokenization)

### 4. GitHub Models Filtering (github models)

The `github models` command may filter available models based on configuration, though the primary filtering is by provider:

**Processing**:
- Lists all available models for the GitHub Models service
- No user-controlled content filtering (displays all available models)

**Source**: See [Layer 3 Proof](cycod-alias-prompt-mcp-github-filtering-pipeline-catalog-layer-3-proof.md#github-models)

## No Advanced Content Filtering

Like `config` commands, these resource management commands do NOT have:
- ❌ Template variable substitution (names and content are literal)
- ❌ Token-based filtering (resources are small files)
- ❌ Content removal mechanisms
- ❌ Pattern matching or regex filtering
- ❌ Context expansion

This is appropriate because these commands manage named resources (metadata) rather than processing content.

## Content Filter Relationship to Other Layers

### From Layer 2 (Container Filter)
- Layer 2 determines WHICH scope/directory to access
- Layer 3 determines WHICH specific resource within that scope to show (all, or single name)

### To Layer 4 (Content Removal)
- Layer 3 selects content to include
- Layer 4 (not applicable - no removal mechanisms)

### To Layer 6 (Display Control)
- Layer 3 defines which resources to show
- Layer 6 controls HOW they are formatted and displayed

## Command-Line Options Summary

| Command | Options | Layer 3 Impact |
|---------|---------|----------------|
| `alias list` | `--global`, `--user`, `--local`, `--any` | Filters which scopes to display |
| `alias get` | `<name>` (positional) | Filters to single alias |
| `alias add` | `<name> <content>` (positional) | Targets single alias, tokenizes content |
| `alias delete` | `<name>` (positional) | Targets single alias (write operation) |
| `prompt list` | `--global`, `--user`, `--local`, `--any` | Filters which scopes to display |
| `prompt get` | `<name>` (positional) | Filters to single prompt |
| `prompt create` | `<name> <text>` (positional) | Targets single prompt (write operation) |
| `prompt delete` | `<name>` (positional) | Targets single prompt (write operation) |
| `mcp list` | `--global`, `--user`, `--local`, `--any` | Filters which scopes to display |
| `mcp get` | `<name>` (positional) | Filters to single MCP config |
| `mcp add` | `<name> --command/--url ...` | Targets single MCP config (write operation) |
| `mcp remove` | `<name>` (positional) | Targets single MCP config (write operation) |
| `github login` | (none) | No content filtering, interactive authentication |
| `github models` | (none) | No content filtering, displays all models |

## Implementation Notes

1. **Name Resolution**: `get`, `add`/`create`, `delete`/`remove` commands resolve names to file paths within the scope
2. **Scope Resolution**: The scope option (Layer 2) determines which directory to access; Layer 3 operates on files within those directories
3. **No Template Processing**: Resource names and content are stored and displayed literally
4. **Atomic Operations**: Write commands target single resources atomically
5. **File-Based Storage**: Each resource is stored as a separate file (e.g., `myalias.alias`, `myprompt.prompt`, `myserver.mcp.json`)

## See Also

- **[Layer 3 Proof](cycod-alias-prompt-mcp-github-filtering-pipeline-catalog-layer-3-proof.md)**: Detailed source code evidence
- **[Layer 2: Container Filter](cycod-alias-prompt-mcp-github-filtering-pipeline-catalog-layer-2.md)**: How scopes/directories are selected
- **[Layer 6: Display Control](cycod-alias-prompt-mcp-github-filtering-pipeline-catalog-layer-6.md)**: How resources are formatted and displayed
