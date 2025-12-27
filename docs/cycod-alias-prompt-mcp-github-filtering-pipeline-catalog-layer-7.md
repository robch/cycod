# cycod alias/prompt/mcp/github - Layer 7: Output Persistence

## Overview

This document covers Layer 7 (Output Persistence) for the simpler resource management commands in cycod:

- **alias** commands: Manage command aliases
- **prompt** commands: Manage prompt templates
- **mcp** commands: Manage MCP server configurations
- **github** commands: GitHub authentication and model listing

All of these follow similar file-based persistence patterns to config commands but with specialized file locations and formats.

---

## alias Commands

### Output Persistence Strategy

Alias commands save command shortcuts to dedicated alias files.

#### File Locations

| Scope | Path |
|-------|------|
| Global | `/etc/cycod/aliases/` or `C:\ProgramData\cycod\aliases\` |
| User | `~/.config/cycod/aliases/` or `%APPDATA%\cycod\aliases\` |
| Local | `./.cycod/aliases/` |

**File naming:** Each alias is a separate file named `{aliasName}`

**File format:** Plain text, one argument per line

#### alias list

**Output:**
- **Console display**: Lists all aliases from selected scope(s)
- **No file writes**: Read-only operation

**Example:**
```bash
cycod alias list --user
# Output:
# test-cmd
# quick-chat
# analyze
```

#### alias get <name>

**Output:**
- **Console display**: Contents of the alias file
- **No file writes**: Read-only operation

**Example:**
```bash
cycod alias get test-cmd --user
# Output:
# chat
# --input
# Run tests
```

#### alias add <name> <content...>

**Output:**
- **File write**: Creates or updates alias file
- **Console display**: Confirmation

**File operations:**
1. Determine alias file path from scope and name
2. Write content to file (one arg per line)
3. Set executable permissions (Unix)

**Example:**
```bash
cycod alias add my-alias chat --input "Hello world" --user
# Creates: ~/.config/cycod/aliases/my-alias
# Content:
# chat
# --input
# Hello world
```

#### alias delete <name>

**Output:**
- **File write**: Deletes alias file
- **Console display**: Confirmation

**File operations:**
1. Determine alias file path
2. Delete file
3. Display success message

---

## prompt Commands

### Output Persistence Strategy

Prompt commands save prompt templates to dedicated prompt files.

#### File Locations

| Scope | Path |
|-------|------|
| Global | `/etc/cycod/prompts/` or `C:\ProgramData\cycod\prompts\` |
| User | `~/.config/cycod/prompts/` or `%APPDATA%\cycod\prompts\` |
| Local | `./.cycod/prompts/` |

**File naming:** Each prompt is a separate file named `{promptName}`

**File format:** Plain text, prompt content

#### prompt list

**Output:**
- **Console display**: Lists all prompts from selected scope(s)
- **No file writes**: Read-only operation

**Example:**
```bash
cycod prompt list --user
# Output:
# code-review
# explain-error
# refactor-suggestion
```

#### prompt get <name>

**Output:**
- **Console display**: Contents of the prompt file
- **No file writes**: Read-only operation

**Example:**
```bash
cycod prompt get code-review --user
# Output:
# Please review this code for:
# - Correctness
# - Performance
# - Security issues
```

#### prompt create <name> <text>

**Output:**
- **File write**: Creates or updates prompt file
- **Console display**: Confirmation

**File operations:**
1. Determine prompt file path from scope and name
2. Write prompt text to file
3. Display success message

**Example:**
```bash
cycod prompt create my-prompt "Explain this code in simple terms" --user
# Creates: ~/.config/cycod/prompts/my-prompt
# Content: Explain this code in simple terms
```

#### prompt delete <name>

**Output:**
- **File write**: Deletes prompt file
- **Console display**: Confirmation

**File operations:**
1. Determine prompt file path
2. Delete file
3. Display success message

---

## mcp Commands

### Output Persistence Strategy

MCP commands manage Model Context Protocol server configurations within the main config file.

#### File Locations

MCP configurations are stored in the **same config files** as regular settings:

| Scope | Path |
|-------|------|
| Global | `/etc/cycod/config.json` or `C:\ProgramData\cycod\config.json` |
| User | `~/.config/cycod/config.json` or `%APPDATA%\cycod\config.json` |
| Local | `./.cycod.json` |

**Configuration structure:**
```json
{
  "mcp.servers": {
    "server-name": {
      "command": "node",
      "args": ["server.js"],
      "env": {
        "VAR": "value"
      },
      "transport": "stdio"
    }
  }
}
```

#### mcp list

**Output:**
- **Console display**: Lists all MCP server configurations from selected scope(s)
- **No file writes**: Read-only operation

**Example:**
```bash
cycod mcp list --user
# Output:
# my-server (stdio)
#   command: node
#   args: server.js
#   env: API_KEY=***
```

#### mcp get <name>

**Output:**
- **Console display**: Details of specific MCP server configuration
- **No file writes**: Read-only operation

**Example:**
```bash
cycod mcp get my-server --user
# Output:
# my-server:
#   transport: stdio
#   command: node
#   args: server.js
#   env: API_KEY=***
```

#### mcp add <name> --command <cmd> [--arg ...] [--env ...]

**Output:**
- **File write**: Adds or updates MCP server configuration in config file
- **Console display**: Confirmation

**File operations:**
1. Load config file
2. Add/update `mcp.servers.{name}` entry
3. Write config file
4. Display confirmation

**Example:**
```bash
cycod mcp add my-server --command node --arg server.js --env API_KEY=secret --user
# Updates: ~/.config/cycod/config.json
# Adds mcp.servers.my-server entry
```

#### mcp add <name> --url <url>

**Output:**
- **File write**: Adds SSE-based MCP server configuration
- **Console display**: Confirmation

**File operations:** Same as `--command` variant, but with `transport: "sse"` and `url` field

**Example:**
```bash
cycod mcp add remote-server --url https://example.com/mcp --user
# Updates: ~/.config/cycod/config.json
# Sets transport to "sse"
```

#### mcp remove <name>

**Output:**
- **File write**: Removes MCP server configuration from config file
- **Console display**: Confirmation

**File operations:**
1. Load config file
2. Remove `mcp.servers.{name}` entry
3. Write config file
4. Display confirmation

---

## github Commands

### Output Persistence Strategy

GitHub commands store authentication tokens and cache model information in config files.

#### github login

**Output:**
- **File write**: Stores GitHub API token in config file
- **Console display**: Success message and instructions

**File operations:**
1. Open browser for GitHub OAuth flow
2. Receive authentication token
3. Store token in config file at specified scope
4. Display confirmation

**Configuration key:**
```json
{
  "github.api-token": "ghp_xxxxxxxxxxxx"
}
```

**Example:**
```bash
cycod github login --user
# Opens browser for authentication
# Stores token in ~/.config/cycod/config.json
```

#### github models

**Output:**
- **Console display**: Lists available GitHub models
- **Optional file write**: May cache model list

**File operations:**
1. Read `github.api-token` from config
2. Query GitHub API for available models
3. Display models to console
4. (Optional) Cache results in config file

**Example:**
```bash
cycod github models
# Output:
# gpt-4
# gpt-3.5-turbo
# claude-3-opus
# ...
```

---

## Common Patterns Across Commands

### Scope Flags (All Commands)

All alias/prompt/mcp commands support the same scope flags as config commands:

| Flag | Description |
|------|-------------|
| `--global`, `-g` | System-wide scope |
| `--user`, `-u` | User-specific scope (default for writes) |
| `--local`, `-l` | Project-specific scope |
| `--any`, `-a` | All scopes (for list/get operations) |

### Default Scope

**For write operations (add, create, set, delete, remove):**
- Default: `--user` scope
- Rationale: User-specific is safest default (doesn't require admin, doesn't pollute project)

**For read operations (list, get):**
- Default: `--any` scope (alias, prompt, mcp)
- Rationale: Show all available resources

### File Format Differences

| Command Group | File Format | Storage |
|---------------|-------------|---------|
| config | JSON (single config file) | Key-value in config.json |
| alias | Plain text (one file per alias) | Separate files in aliases/ folder |
| prompt | Plain text (one file per prompt) | Separate files in prompts/ folder |
| mcp | JSON (within config file) | Nested object in config.json |
| github | JSON (within config file) | Token stored in config.json |

### Console Output Patterns

All commands follow consistent display patterns:

**List commands:**
```
Scope (location):
item1
item2
item3
```

**Get commands:**
```
name:
  content or details
```

**Write commands:**
```
Success message
File: /path/to/file
```

### Error Handling

**File not found (get/delete operations):**
- Error message: "Alias/Prompt/MCP not found: {name}"
- Exit code: Non-zero

**Permission denied:**
- Error message: "Permission denied: {path}"
- Exit code: Non-zero

**Invalid configuration:**
- Error message: "Invalid configuration: {details}"
- Exit code: Non-zero

---

## Data Flow Summaries

### alias add Flow

```
1. Parse command line
   ├─ Name → AliasAddCommand.AliasName
   ├─ Content → AliasAddCommand.Content
   └─ Scope → AliasAddCommand.Scope (default: User)

2. Determine file path
   └─ AliasFileHelpers.GetAliasFilePath(name, scope)
      └─ Returns: ~/.config/cycod/aliases/{name}

3. Write file
   └─ File.WriteAllLines(path, contentLines)

4. Display confirmation
   └─ Console.WriteLine($"Alias '{name}' created")
```

### mcp add Flow

```
1. Parse command line
   ├─ Name → McpAddCommand.Name
   ├─ Command/URL → McpAddCommand.Command or .Url
   ├─ Args → McpAddCommand.Args (list)
   ├─ Env → McpAddCommand.EnvironmentVars (list)
   └─ Scope → McpAddCommand.Scope (default: User)

2. Build MCP configuration object
   └─ {
         "command": "...",
         "args": [...],
         "env": {...},
         "transport": "stdio|sse"
      }

3. Write to config file
   └─ ConfigStore.Set("mcp.servers.{name}", mcpConfig, scope)
      └─ Updates config file JSON

4. Display confirmation
   └─ Console.WriteLine($"MCP server '{name}' added")
```

---

## Performance Considerations

### alias/prompt Commands (Individual Files)

**Advantages:**
- ✅ Parallel access (different aliases can be modified simultaneously)
- ✅ No JSON parsing overhead
- ✅ Simple text format
- ✅ Git-friendly (one file per resource)

**Disadvantages:**
- ❌ Many small files (filesystem overhead)
- ❌ Directory traversal for list operations
- ❌ No atomic multi-resource updates

### mcp/github Commands (Embedded in Config)

**Advantages:**
- ✅ Single file to manage
- ✅ Atomic updates (within single config file)
- ✅ Structured format (JSON)

**Disadvantages:**
- ❌ Concurrent access issues (entire config file locked)
- ❌ JSON parsing overhead
- ❌ Git conflicts (multiple resources in one file)

---

## Edge Cases

### Alias/Prompt Name Conflicts

**Scenario:** Same alias/prompt name in multiple scopes

**Behavior:**
- `list --any`: Shows all (with scope labels)
- `get --any`: Returns highest priority (Local > User > Global)
- `add`: Updates specified scope only

### MCP Server Name Conflicts

**Scenario:** Same MCP server name in multiple config scopes

**Behavior:**
- Same as config settings: highest priority scope wins
- Local overrides User overrides Global

### Special Characters in Names

**Alias/Prompt names:**
- Filesystem constraints apply (no `/`, `\`, `:`, etc.)
- Validation should prevent invalid names

**MCP names:**
- JSON key constraints apply (any string, but special chars may need escaping)

### Empty Content

**alias add with no content:**
- Creates empty alias file
- Alias expands to nothing (no-op)

**prompt create with no text:**
- Creates empty prompt file
- Prompt expands to nothing

---

## Summary

Layer 7 in alias/prompt/mcp/github commands provides:

✅ **File-based persistence**: Dedicated files or config entries
✅ **Scope management**: Global, User, Local options
✅ **Console feedback**: All operations confirm success
✅ **Simple formats**: Plain text or JSON

Key differences from config commands:

| Aspect | config | alias/prompt | mcp/github |
|--------|--------|--------------|------------|
| Storage | Config files | Individual files | Config files |
| Format | JSON | Plain text | JSON |
| Location | config.json | aliases/prompts/ folders | config.json |
| Granularity | Key-value | One file per resource | Nested in config |

## See Also

- [Layer 7 Proof (Source Evidence)](cycod-alias-prompt-mcp-github-filtering-pipeline-catalog-layer-7-proof.md)
- [All Layers](../cycod-filter-pipeline-catalog-README.md)
