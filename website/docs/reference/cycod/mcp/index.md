# mcp Command

The `cycod mcp` command allows you to manage Model Context Protocol (MCP) servers for CYCOD.

## Syntax

```bash
cycod mcp SUBCOMMAND [options]
```

## Subcommands

| Subcommand | Description |
|------------|-------------|
| [`list`](list.md) | List defined MCP servers |
| [`get`](get.md) | Get details of a specific MCP server |
| [`add`](add.md) | Add a new MCP server |
| [`remove`](remove.md) | Remove an MCP server |

## Options

| Option | Description |
|--------|-------------|
| `--global`, `-g` | Operate on global MCP servers (all users) |
| `--user`, `-u` | Operate on user MCP servers (current user) |
| `--local`, `-l` | Operate on local MCP servers (default) |
| `--any`, `-a` | Include MCP servers from all scopes (default for 'list' and 'get' commands) |

## MCP Scopes

CYCOD supports three MCP server scopes:

- **Local**: MCP servers apply only to the current directory, stored in `.cycod/mcp-servers.json`
- **User**: MCP servers apply to the current user across all directories, stored in `~/.cycod/mcp-servers.json`
- **Global**: MCP servers apply to all users on the system, stored in a system-wide location

## What is MCP?

Model Context Protocol (MCP) is a mechanism that allows CYCOD to communicate with external tools and services. MCP servers can provide:

- Context from databases
- Information from code repositories
- Access to external APIs
- Custom tools and functions
- Real-time data sources

MCP servers run as separate processes that CYCOD can connect to, enhancing AI models with additional capabilities and information.

## Examples

List all MCP servers from all scopes:

```bash
cycod mcp list
```

List only user MCP servers:

```bash
cycod mcp list --user
```

Get details of a specific MCP server:

```bash
cycod mcp get postgres-server
```

Add a new MCP server:

```bash
cycod mcp add postgres-server --command "python" --arg "db_server.py" --arg "--verbose"
```

Remove an MCP server:

```bash
cycod mcp remove postgres-server
```

## Notes

- MCP servers from the local scope take precedence over user scope, which takes precedence over global scope
- MCP servers are started automatically when needed by CYCOD
- Use the `--mcp-server` option with CYCOD to specify which MCP servers to use for a chat session