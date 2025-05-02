# --global

The `--global` option (or its alias `-g`) specifies that an operation should apply at the global scope level, affecting all users on the system.

## Overview

CycoD uses a three-tier scoping system for configurations, aliases, prompts, and MCP servers:

- **Local**: Applied only to the current directory (default for most operations)
- **User**: Applied to the current user across all folders
- **Global**: Applied to all users on the computer

The `--global` option changes the scope of the command to operate at the global level.

## Syntax

```
--global
-g
```

## Usage

The `--global` option can be used with several CycoD commands:

### For Config Commands

- `cycod config list --global`: List only global configuration settings
- `cycod config get KEY --global`: Get a configuration setting from global scope
- `cycod config set KEY VALUE --global`: Set a configuration setting in global scope
- `cycod config clear KEY --global`: Clear a configuration setting from global scope
- `cycod config add KEY VALUE --global`: Add a value to a list setting in global scope
- `cycod config remove KEY VALUE --global`: Remove a value from a list setting in global scope

### For Alias Commands

- `cycod alias list --global`: List only global aliases
- `cycod alias get ALIAS_NAME --global`: Get an alias from global scope
- `cycod alias delete ALIAS_NAME --global`: Delete an alias from global scope
- `cycod --save-global-alias ALIAS`: Save current options as an alias in global scope

### For Prompt Commands

- `cycod prompt list --global`: List only global prompts
- `cycod prompt get PROMPT_NAME --global`: Get a prompt from global scope
- `cycod prompt delete PROMPT_NAME --global`: Delete a prompt from global scope
- `cycod prompt create PROMPT_NAME PROMPT_TEXT --global`: Create a prompt in global scope

### For MCP Commands

- `cycod mcp list --global`: List only global MCP servers
- `cycod mcp get SERVER_NAME --global`: Get an MCP server from global scope
- `cycod mcp add SERVER_NAME [OPTIONS] --global`: Add an MCP server in global scope
- `cycod mcp remove SERVER_NAME --global`: Remove an MCP server from global scope

## Examples

### Setting a default AI model globally

```bash
cycod config set OPENAI_CHAT_MODEL_NAME gpt-4o --global
```

### Creating a prompt available to all users

```bash
cycod prompt create debug "Debug the following code and explain the issues: {code}" --global
```

### Adding an MCP server for all users

```bash
cycod mcp add system-api --url https://internal-api.example.com/mcp --global
```

### Listing all global aliases

```bash
cycod alias list --global
```

### Saving a command as a global alias

```bash
cycod --use-openai --openai-chat-model-name gpt-4 --save-global-alias productivity
```

## Storage Location

Global scope settings, aliases, prompts, and MCP servers are stored in system-wide locations accessible to all users on the computer:

- **Windows**: typically in `C:\ProgramData\.cycod\`
- **macOS/Linux**: typically in `/usr/local/share/.cycod/` or similar system-wide location

## When to Use Global Scope

Use the global scope when:

1. You want settings to apply to all users on the system
2. You're configuring a shared system or setting company-wide defaults
3. You want to provide common aliases, prompts, or MCP servers to all users

## Security Considerations

- For security-sensitive settings like API keys, it's recommended to use the user scope (`--user`) rather than global scope to limit exposure
- Be aware that global configurations are accessible to other users on the system
- Administrative privileges may be required to modify global settings

## Notes

- Items in the local scope take precedence over user scope, which takes precedence over global scope
- For most commands, if no scope is specified, local scope is used by default
- For `list` and `get` commands, if no scope is specified, the `--any` scope is used which searches all scopes

## Related

- [`--local`](/reference/cli/options/local.md): Apply command at local scope (current directory)
- [`--user`](/reference/cli/options/user.md): Apply command at user scope (current user)
- [`--any`](/reference/cli/options/any.md): Apply command across all scopes
- [`--save-global-alias`](/reference/cli/options/save-global-alias.md): Save current options as an alias in global scope