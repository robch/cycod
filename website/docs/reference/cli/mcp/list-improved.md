# mcp list

Lists all configured Model Context Protocol (MCP) servers.

## Syntax

```bash
chatx mcp list [options]
```

## Description

The `chatx mcp list` command displays all configured Model Context Protocol (MCP) servers. By default, it shows servers from all scopes (local, user, and global). MCP servers provide AI models with access to external tools, databases, APIs, and other resources.

## Options

| Option | Description |
|--------|-------------|
| `--global`, `-g` | Show only global MCP servers (available to all users) |
| `--user`, `-u` | Show only user MCP servers (available to current user) |
| `--local`, `-l` | Show only local MCP servers (available in current directory) |
| `--any`, `-a` | Show MCP servers from all scopes (default) |
| `--json`, `-j` | Output results in JSON format |

## Examples

### List all MCP servers from all scopes

```bash
chatx mcp list
```

Example output:
```
MCP SERVERS (scope: global)
  system-api:
    url: https://internal-api.example.com/mcp
    auto_start: true

MCP SERVERS (scope: user)
  postgres-server:
    command: python db_server.py
    args: ["--connection-string", "postgresql://user:pass@localhost:5432/mydb"]
    env: {"DB_PASSWORD": "******"}
    auto_start: true

  github-tools:
    command: node github_tools.js
    port: 8766
    env: {"GITHUB_TOKEN": "******"}
    auto_start: false

MCP SERVERS (scope: local)
  code-search:
    command: python code_search.py
    args: ["--repo-path", "/path/to/repo"]
    auto_start: true
```

### List only user-scoped MCP servers

```bash
chatx mcp list --user
```

Example output:
```
MCP SERVERS (scope: user)
  postgres-server:
    command: python db_server.py
    args: ["--connection-string", "postgresql://user:pass@localhost:5432/mydb"]
    env: {"DB_PASSWORD": "******"}
    auto_start: true

  github-tools:
    command: node github_tools.js
    port: 8766
    env: {"GITHUB_TOKEN": "******"}
    auto_start: false
```

### Output MCP server list in JSON format

```bash
chatx mcp list --json
```

Example output:
```json
{
  "global": {
    "system-api": {
      "url": "https://internal-api.example.com/mcp",
      "auto_start": true
    }
  },
  "user": {
    "postgres-server": {
      "command": "python db_server.py",
      "args": ["--connection-string", "postgresql://user:pass@localhost:5432/mydb"],
      "env": {"DB_PASSWORD": "******"},
      "auto_start": true
    },
    "github-tools": {
      "command": "node github_tools.js",
      "port": 8766,
      "env": {"GITHUB_TOKEN": "******"},
      "auto_start": false
    }
  },
  "local": {
    "code-search": {
      "command": "python code_search.py",
      "args": ["--repo-path", "/path/to/repo"],
      "auto_start": true
    }
  }
}
```

## Output Details

The command output includes the following information for each MCP server:

- **Name**: The identifier used to reference the MCP server
- **Scope**: Whether it's local, user, or global
- **Command/URL**: Either the command to execute (for STDIO servers) or the URL (for SSE servers)
- **Arguments**: Any command-line arguments passed to the server
- **Environment Variables**: Environment variables set for the server (sensitive values are masked)
- **Configuration**: Additional server-specific configurations

## Related Commands

- [`chatx mcp get`](get.md) - Display details of a specific MCP server
- [`chatx mcp add`](add.md) - Add a new MCP server
- [`chatx mcp remove`](remove.md) - Remove an MCP server

## Notes

- If no MCP servers exist in the specified scope(s), the command will output a message indicating this
- For security, sensitive values in environment variables (like passwords and API keys) are masked in the output
- Use this command to verify your MCP server configurations and troubleshoot connectivity issues