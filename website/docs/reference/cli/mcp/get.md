# mcp get

Retrieves information about a specific Model Context Protocol (MCP) server.

## Syntax

```bash
cycod mcp get <server-name> [--scope]
```

## Description

The `cycod mcp get` command displays detailed information about a specified Model Context Protocol (MCP) server. When an MCP server is found, the command displays the server name, file location and scope, command and arguments, and environment variables.

By default, the command searches for the server in all scopes (local, user, and global).

## Arguments

| Argument | Description |
|----------|-------------|
| `<server-name>` | Name of the MCP server to retrieve information about |

## Options

| Option | Description |
|--------|-------------|
| `--global`, `-g` | Look only in global scope (all users) |
| `--user`, `-u` | Look only in user scope (current user) |
| `--local`, `-l` | Look only in local scope (current directory) |
| `--any`, `-a` | Look in all scopes (default) |
| `--json`, `-j` | Output results in JSON format |

## Examples

### Example 1: Get details about an MCP server

Get detailed information about an MCP server named "postgres-server" from any scope:

```bash
cycod mcp get postgres-server
```

Output:
```
MCP SERVER: postgres-server
  File:       /home/user/.cycod/mcp-servers/postgres-server.json
  Scope:      user
  Command:    python db_server.py
  Arguments:  --connection-string postgresql://user:pass@localhost:5432/mydb
  Env Vars:   DB_PASSWORD=****** (1 variable)
  Auto Start: true
```

### Example 2: Get details about an MCP server from a specific scope

Get details about an MCP server named "github-tools" from user scope only:

```bash
cycod mcp get github-tools --user
```

Output:
```
MCP SERVER: github-tools
  File:       /home/user/.cycod/mcp-servers/github-tools.json
  Scope:      user
  Command:    node github_api.js
  Env Vars:   GITHUB_TOKEN=****** (1 variable)
  Auto Start: true
```

### Example 3: Get details about a URL-based MCP server

Get details about an SSE-based MCP server:

```bash
cycod mcp get weather-api
```

Output:
```
MCP SERVER: weather-api
  File:       .cycod/mcp-servers/weather-api.json
  Scope:      local
  URL:        https://example.com/weather-sse
  Auto Start: true
```

### Example 4: Get server details in JSON format

Get details about an MCP server in JSON format:

```bash
cycod mcp get code-search --json
```

Output:
```json
{
  "name": "code-search",
  "file": "/home/user/.cycod/mcp-servers/code-search.json",
  "scope": "user",
  "command": "python",
  "args": ["code_search.py", "--repo-dir", "/path/to/repo"],
  "env": {"GITHUB_TOKEN": "******"},
  "auto_start": true
}
```

## Error Messages

If the specified MCP server is not found in the requested scope, the command will display an error message:

```
Error: MCP server 'unknown-server' not found in user scope.
```

When searching in all scopes (`--any`) and the server is not found:

```
Error: MCP server 'unknown-server' not found in any scope.
```

## Related Commands

| Command | Description |
|---------|-------------|
| [`cycod mcp list`](list.md) | List all available MCP servers |
| [`cycod mcp add`](add.md) | Create a new MCP server configuration |
| [`cycod mcp remove`](remove.md) | Delete an MCP server configuration |

## See Also

- [Managing MCP Servers](../../../tutorials/managing-mcp-servers.md)
- [Using Environment Variables with MCP Servers](../../../tutorials/env-variables-mcp.md)
- [MCP Command Usage](../../../tutorials/mcp-command-usage.md)