# mcp remove

Deletes a Model Context Protocol (MCP) server configuration.

## Syntax

```bash
cycod mcp remove SERVER_NAME [options]
```

## Description

The `cycod mcp remove` command deletes a specified Model Context Protocol (MCP) server configuration. MCP servers provide capabilities like database access, API integrations, or tool execution to AI models during chat sessions.

When used without scope options, it searches for the server in all scopes and removes the first match it finds (starting with local, then user, then global scope).

## Arguments

| Argument | Description |
|----------|-------------|
| `SERVER_NAME` | Name of the MCP server to remove |

## Options

| Option | Description |
|--------|-------------|
| `--global`, `-g` | Remove only from global scope (all users) |
| `--user`, `-u` | Remove only from user scope (current user) |
| `--local`, `-l` | Remove only from local scope (current directory) |
| `--any`, `-a` | Look in all scopes and remove the first match (default) |
| `--yes`, `-y` | Skip confirmation prompt |

## Examples

### Example 1: Remove an MCP server from any scope

Remove an MCP server named "unused-server" from the first scope it's found in:

```bash
cycod mcp remove unused-server
```

### Example 2: Remove an MCP server from user scope

Remove an MCP server named "postgres-server" only from the user's configuration:

```bash
cycod mcp remove postgres-server --user
```

### Example 3: Remove a database server from local scope

Remove a database server from the current directory's configuration:

```bash
cycod mcp remove database --local
```

### Example 4: Remove an MCP server without confirmation

Remove a system-wide MCP server without asking for confirmation (requires appropriate permissions):

```bash
cycod mcp remove system-api --global --yes
```

## Output

By default, the command will ask for confirmation before removing:

```
Are you sure you want to remove MCP server 'postgres-server' from local scope? [y/N]:
```

Use the `--yes` or `-y` option to skip this confirmation.

When a matching server is found and successfully removed, the command outputs a confirmation message:

```
MCP server 'postgres-server' has been removed from local scope.
```

If the specified server isn't found in the requested scope, you'll get an error:

```
Error: MCP server 'unknown-server' not found in user scope.
```

## Notes

- The removal is permanent and cannot be undone
- If using the default behavior (or `--any` option), the command will remove the first matching MCP server found when searching in this order: local, user, global
- This command only removes the MCP server configuration from CycoD; if the actual server process was started separately, you may need to stop it manually

## Related Commands

- [mcp list](list.md) - List all available MCP servers
- [mcp get](get.md) - Display details of a specific MCP server
- [mcp add](add.md) - Create a new MCP server configuration