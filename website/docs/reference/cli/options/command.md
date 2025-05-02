---
title: "--command"
sidebar_label: "--command"
---

# --command

Specify the command to execute when creating an MCP server.

## Syntax

```bash
cycod mcp add <server-name> --command <command> [--arg <argument>] [--env <env-var>] [...]
```

## Description

The `--command` option is used with the `cycod mcp add` command to specify the command that should be executed to start a Model Context Protocol (MCP) server. This command is responsible for launching the MCP server process that will communicate with CycoD using the STDIO transport.

MCP servers provide AI models with capabilities such as:
- Database access
- API integrations
- Tool execution
- Document processing

This option is required when creating an MCP server that uses the STDIO transport. For SSE-based MCP servers, use the `--url` option instead.

## Examples

### Basic usage

```bash
cycod mcp add simple-server --command "/path/to/mcp-server"
```

### Specify a Python script

```bash
cycod mcp add python-server --command "python" --arg "mcp_server.py"
```

### Specify a Node.js server

```bash
cycod mcp add node-server --command "node" --arg "server.js"
```

### With environment variables

```bash
cycod mcp add api-tool --command "./api-tool" --env "API_KEY=abc123"
```

### With working directory

```bash
cycod mcp add database --command "python" --arg "db_server.py" --working-dir "./database-tools"
```

### Create a persistent server in user scope

```bash
cycod mcp add shared-tool --command "/usr/local/bin/tool-server" --user
```

## Related

- [--arg](./arg.md) - Specify arguments to pass to the command
- [--env](./env.md) - Set environment variables for the command
- [--url](./url.md) - Specify URL for SSE-based MCP servers
- [--working-dir](./working-dir.md) - Set the working directory for the command