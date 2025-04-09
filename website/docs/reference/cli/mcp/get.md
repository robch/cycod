# mcp get

Retrieves information about a specific MCP server.

## Syntax

```bash
chatx mcp get <server-name> [options]
```

## Description

The `chatx mcp get` command displays details about a specified Model Context Protocol (MCP) server. By default, it searches for the server in all scopes.

## Arguments

| Argument | Description |
|----------|-------------|
| `<server-name>` | Name of the MCP server to retrieve |

## Options

| Option | Description |
|--------|-------------|
| `--global`, `-g` | Look only in global scope (all users) |
| `--user`, `-u` | Look only in user scope (current user) |
| `--local`, `-l` | Look only in local scope (current directory) |
| `--any`, `-a` | Look in all scopes (default) |
| `--json`, `-j` | Output results in JSON format |

## Examples

Get details about an MCP server named "postgres-server":

```bash
chatx mcp get postgres-server
```

Get details about an MCP server named "github-tools" from user scope only:

```bash
chatx mcp get github-tools --user
```

Get details about an MCP server named "code-search" in JSON format:

```bash
chatx mcp get code-search --json
```

## Output

The command outputs detailed information about the specified MCP server:

```
MCP SERVER: postgres-server
  Command:    python db_server.py
  Port:       8765
  Auto Start: true
  Scope:      user
```

When using `--json` option, the output is formatted as JSON:

```json
{
  "name": "postgres-server",
  "command": "python db_server.py",
  "port": 8765,
  "auto_start": true,
  "scope": "user"
}
```

If the MCP server is not found, the command will display an error message:

```
Error: MCP server 'unknown-server' not found in any scope.
```