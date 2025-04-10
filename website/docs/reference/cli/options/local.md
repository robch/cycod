# --local

The `--local` option (or its alias `-l`) specifies that an operation should apply at the local scope level, affecting only the current directory.

## Overview

ChatX uses a three-tier scoping system for configurations, aliases, prompts, and MCP servers:

- **Local**: Applied only to the current directory (default for most operations)
- **User**: Applied to the current user across all folders
- **Global**: Applied to all users on the computer

The `--local` option explicitly sets the scope of the command to operate at the local level.

## Syntax

```
--local
-l
```

## Usage

The `--local` option can be used with several ChatX commands:

### For Config Commands

- `chatx config list --local`: List only local configuration settings
- `chatx config get KEY --local`: Get a configuration setting from local scope
- `chatx config set KEY VALUE --local`: Set a configuration setting in local scope
- `chatx config clear KEY --local`: Clear a configuration setting from local scope
- `chatx config add KEY VALUE --local`: Add a value to a list setting in local scope
- `chatx config remove KEY VALUE --local`: Remove a value from a list setting in local scope

### For Alias Commands

- `chatx alias list --local`: List only local aliases
- `chatx alias get ALIAS_NAME --local`: Get an alias from local scope
- `chatx alias delete ALIAS_NAME --local`: Delete an alias from local scope
- `chatx --save-local-alias ALIAS`: Save current options as an alias in local scope

### For Prompt Commands

- `chatx prompt list --local`: List only local prompts
- `chatx prompt get PROMPT_NAME --local`: Get a prompt from local scope
- `chatx prompt delete PROMPT_NAME --local`: Delete a prompt from local scope
- `chatx prompt create PROMPT_NAME PROMPT_TEXT --local`: Create a prompt in local scope

### For MCP Commands

- `chatx mcp list --local`: List only local MCP servers
- `chatx mcp get SERVER_NAME --local`: Get an MCP server from local scope
- `chatx mcp add SERVER_NAME [OPTIONS] --local`: Add an MCP server in local scope
- `chatx mcp remove SERVER_NAME --local`: Remove an MCP server from local scope

## Examples

### Setting a project-specific configuration

```bash
chatx config set app.preferredProvider azure-openai --local
```

### Creating a project-specific prompt

```bash
chatx prompt create code-review "Review the following code according to our team's standards:" --local
```

### Adding a local MCP server for a specific project

```bash
chatx mcp add project-db --command "./db-connector" --arg "--port=5432" --local
```

### Listing all local aliases

```bash
chatx alias list --local
```

### Saving a command as a local alias

```bash
chatx --use-openai --openai-chat-model-name gpt-4 --save-local-alias project-assistant
# Same as:
chatx --use-openai --openai-chat-model-name gpt-4 --save-alias project-assistant
```

## Storage Location

Local scope settings, aliases, prompts, and MCP servers are stored in the `.chatx` directory of your current working directory:

- **Windows**: `<current_directory>\.chatx\`
- **macOS/Linux**: `<current_directory>/.chatx/`

## When to Use Local Scope

Use the local scope when:

1. You want settings to apply only to a specific project or directory
2. You're working in a team environment and want to share settings via version control
3. You need different configurations for different projects
4. You want to override user or global settings for a specific context

## Default Behavior

Most `set`, `create`, `add`, and similar commands use the local scope by default if no scope option is specified.

For example, these commands are equivalent:

```bash
chatx config set DEBUG_MODE true --local
chatx config set DEBUG_MODE true  # Local is the default for set
```

## Version Control Considerations

Since local settings are stored within the current directory structure:

- They can be committed to a repository and shared with a team
- This makes them ideal for project-specific settings
- You may want to include the `.chatx` directory in version control for team settings
- For personal or sensitive settings, use the user scope instead

## Notes

- Items in the local scope take precedence over user scope, which takes precedence over global scope
- Local settings only apply within the directory where they were created
- The `--local` option is often implied as the default for most commands

## Related

- [`--user`](/reference/cli/options/user.md): Apply command at user scope (current user)
- [`--global`](/reference/cli/options/global.md): Apply command at global scope (all users)
- [`--any`](/reference/cli/options/any.md): Apply command across all scopes
- [`--save-local-alias`](/reference/cli/options/save-local-alias.md): Save current options as an alias in local scope
- [`--save-alias`](/reference/cli/options/save-alias.md): Alias for --save-local-alias