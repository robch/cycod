# mcp list

Lists all MCP servers.

## Syntax

```bash
chatx mcp list [options]
```

## Description

The `chatx mcp list` command displays all defined Model Context Protocol (MCP) servers. By default, it shows servers from all scopes.

## Options

| Option | Description |
|--------|-------------|
| `--global`, `-g` | Show only global MCP servers (all users) |
| `--user`, `-u` | Show only user MCP servers (current user) |
| `--local`, `-l` | Show only local MCP servers (current directory) |
| `--any`, `-a` | Show MCP servers from all scopes (default) |
| `--json`, `-j` | Output results in JSON format |

## Examples

List all MCP servers from all scopes:

```bash
chatx mcp list
```

List only user MCP servers:

```bash
chatx mcp list --user
```

Output all MCP servers in JSON format:

```bash
chatx mcp list --json
```

## Output

The command outputs a list of MCP servers and their configurations:

```
MCP SERVERS (scope: user)
  postgres-server:
    command: python db_server.py
    port: 8765
    auto_start: true

  github-tools:
    command: node github_tools.js
    port: 8766
    auto_start: false

MCP SERVERS (scope: local)
  code-search:
    command: python code_search.py
    port: 8767
    auto_start: true
```

When using `--json` option, the output is formatted as JSON:

```json
{
  "user": {
    "postgres-server": {
      "command": "python db_server.py",
      "port": 8765,
      "auto_start": true
    },
    "github-tools": {
      "command": "node github_tools.js",
      "port": 8766,
      "auto_start": false
    }
  },
  "local": {
    "code-search": {
      "command": "python code_search.py",
      "port": 8767,
      "auto_start": true
    }
  }
}
```

If no MCP servers are found in the specified scope, the command will indicate this:

```
No MCP servers found in local scope.
```