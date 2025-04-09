# mcp add

Adds a new MCP server configuration.

## Syntax

```bash
chatx mcp add <server-name> [options]
```

## Description

The `chatx mcp add` command creates a new Model Context Protocol (MCP) server configuration. By default, it creates the configuration in the local scope.

## Arguments

| Argument | Description |
|----------|-------------|
| `<server-name>` | Name for the new MCP server |

## Options

| Option | Description |
|--------|-------------|
| `--command`, `-c` | Command to start the MCP server (required) |
| `--port`, `-p` | Port for the MCP server to listen on (default: auto-assigned) |
| `--auto-start`, `-a` | Automatically start the server when needed (default: true) |
| `--working-dir`, `-w` | Working directory for the server process |
| `--global`, `-g` | Add in global scope (all users) |
| `--user`, `-u` | Add in user scope (current user) |
| `--local`, `-l` | Add in local scope (current directory, default) |

## Examples

Add a PostgreSQL database MCP server in local scope:

```bash
chatx mcp add postgres-server --command "python db_server.py"
```

Add a GitHub tools MCP server in user scope with a specific port:

```bash
chatx mcp add github-tools --command "node github_tools.js" --port 8766 --user
```

Add a code search MCP server in global scope that doesn't auto-start:

```bash
chatx mcp add code-search --command "python code_search.py" --auto-start false --global
```

Add an MCP server with a specific working directory:

```bash
chatx mcp add api-server --command "npm start" --working-dir "./api-tools"
```

## Output

The command confirms successful creation:

```
MCP server 'postgres-server' has been added in local scope.
```

If an MCP server with the same name already exists in the specified scope, the command will display an error:

```
Error: MCP server 'postgres-server' already exists in local scope. Use 'mcp remove' first to replace it.
```

If the required command option is not provided, the command will display an error:

```
Error: The --command option is required when adding an MCP server.
```

## Using MCP Servers

After adding an MCP server, you can use it in your CHATX sessions:

```bash
# Use a specific MCP server
chatx --mcp-server postgres-server "Query for database information about customers"

# Use multiple MCP servers
chatx --mcp-servers postgres-server,github-tools "Find customer issues in GitHub"
```