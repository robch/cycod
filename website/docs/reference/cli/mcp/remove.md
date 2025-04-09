# mcp remove

Removes an MCP server configuration.

## Syntax

```bash
chatx mcp remove <server-name> [options]
```

## Description

The `chatx mcp remove` command deletes a specified Model Context Protocol (MCP) server configuration. By default, it attempts to delete from the local scope first, then user scope, then global scope.

## Arguments

| Argument | Description |
|----------|-------------|
| `<server-name>` | Name of the MCP server to remove |

## Options

| Option | Description |
|--------|-------------|
| `--global`, `-g` | Remove from global scope (all users) |
| `--user`, `-u` | Remove from user scope (current user) |
| `--local`, `-l` | Remove from local scope (current directory, default) |
| `--any`, `-a` | Remove from all scopes where found |
| `--yes`, `-y` | Skip confirmation prompt |

## Examples

Remove an MCP server named "postgres-server" from local scope:

```bash
chatx mcp remove postgres-server
```

Remove an MCP server named "github-tools" from user scope:

```bash
chatx mcp remove github-tools --user
```

Remove an MCP server named "code-search" from global scope without confirmation:

```bash
chatx mcp remove code-search --global --yes
```

Remove an MCP server named "legacy-server" from all scopes where it exists:

```bash
chatx mcp remove legacy-server --any
```

## Output

The command confirms successful removal:

```
MCP server 'postgres-server' has been removed from local scope.
```

If the MCP server is not found in the specified scope, the command will display an error:

```
Error: MCP server 'unknown-server' not found in local scope.
```

By default, the command will ask for confirmation before removing:

```
Are you sure you want to remove MCP server 'postgres-server' from local scope? [y/N]:
```

Use the `--yes` or `-y` option to skip the confirmation.

## Notes

This command only removes the MCP server configuration from CHATX. If the actual server process was started separately, you may need to stop it manually.