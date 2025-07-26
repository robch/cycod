# --user

The `--user` option (or its alias `-u`) specifies that an operation should apply at the user scope level, affecting the current user across all directories.

## Overview

CycoD uses a three-tier scoping system for configurations, aliases, prompts, and MCP servers:

- **Local**: Applied only to the current directory (default for most operations)
- **User**: Applied to the current user across all folders
- **Global**: Applied to all users on the computer

The `--user` option changes the scope of the command to operate at the user level.

## Syntax

```
--user
-u
```

## Usage

The `--user` option can be used with several CycoD commands:

### For Config Commands

- `cycod config list --user`: List only user configuration settings
- `cycod config get KEY --user`: Get a configuration setting from user scope
- `cycod config set KEY VALUE --user`: Set a configuration setting in user scope
- `cycod config clear KEY --user`: Clear a configuration setting from user scope
- `cycod config add KEY VALUE --user`: Add a value to a list setting in user scope
- `cycod config remove KEY VALUE --user`: Remove a value from a list setting in user scope

### For Alias Commands

- `cycod alias list --user`: List only user aliases
- `cycod alias get ALIAS_NAME --user`: Get an alias from user scope
- `cycod alias delete ALIAS_NAME --user`: Delete an alias from user scope
- `cycod --save-user-alias ALIAS`: Save current options as an alias in user scope

### For Prompt Commands

- `cycod prompt list --user`: List only user prompts
- `cycod prompt get PROMPT_NAME --user`: Get a prompt from user scope
- `cycod prompt delete PROMPT_NAME --user`: Delete a prompt from user scope
- `cycod prompt create PROMPT_NAME PROMPT_TEXT --user`: Create a prompt in user scope

### For MCP Commands

- `cycod mcp list --user`: List only user MCP servers
- `cycod mcp get SERVER_NAME --user`: Get an MCP server from user scope
- `cycod mcp add SERVER_NAME [OPTIONS] --user`: Add an MCP server in user scope
- `cycod mcp remove SERVER_NAME --user`: Remove an MCP server from user scope

## Examples

### Setting a user-level API key

```bash
cycod config set OPENAI_API_KEY sk-1234567890abcdef --user
```

### Creating a personal prompt available in all projects

```bash
cycod prompt create translate "Translate the following text to Spanish:" --user
```

### Adding an MCP server for personal use

```bash
cycod mcp add personal-tools --command ~/bin/tools-server --arg --cache-dir --arg ~/.cache/tools --user
```

### Listing all user aliases

```bash
cycod alias list --user
```

### Saving a command as a user-level alias

```bash
cycod --use-openai --openai-chat-model-name gpt-4-turbo --save-user-alias my-assistant
```

### Disabling automatic chat history saving for all projects

```bash
cycod config set App.AutoSaveChatHistory false --user
```

## Storage Location

User scope settings, aliases, prompts, and MCP servers are stored in the `.cycod` directory under your user profile:

- **Windows**: `%USERPROFILE%\.cycod\`
- **macOS/Linux**: `~/.cycod/`

## When to Use User Scope

Use the user scope when:

1. You want settings to apply to all your projects and directories
2. You're configuring personal preferences
3. You're handling sensitive information like API keys
4. You want aliases, prompts, or MCP servers to be available regardless of which directory you're in

## Security Considerations

- User scope is ideal for storing API keys and other sensitive information since they're only accessible to the current user
- These settings won't be shared when committing code to version control
- User scope provides a good balance between convenience (available everywhere) and security (private to your user account)

## Notes

- Items in the local scope take precedence over user scope, which takes precedence over global scope
- For most commands, if no scope is specified, local scope is used by default
- For `list` and `get` commands, if no scope is specified, the `--any` scope is used which searches all scopes
- User scope is particularly useful for API keys and personal preferences that should be consistent across all projects

## Related

- [`--local`](/reference/cycod/options/local.md): Apply command at local scope (current directory)
- [`--global`](/reference/cycod/options/global.md): Apply command at global scope (all users)
- [`--any`](/reference/cycod/options/any.md): Apply command across all scopes
- [`--save-user-alias`](/reference/cycod/options/save-user-alias.md): Save current options as an alias in user scope