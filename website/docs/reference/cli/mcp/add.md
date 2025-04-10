# mcp add

Adds a new MCP server configuration.

## Syntax

```bash
chatx mcp add <server-name> [options]
```

## Description

The `chatx mcp add` command creates a new Model Context Protocol (MCP) server configuration. By default, it creates the configuration in the local scope.

MCP servers can use one of two transport types:
1. **STDIO transport** - Uses the `--command` parameter to specify a program that communicates via standard input/output
2. **SSE transport** - Uses the `--url` parameter to specify a Server-Sent Events endpoint URL

You must provide either `--command` (for STDIO) or `--url` (for SSE), but not both.

## Arguments

| Argument | Description |
|----------|-------------|
| `<server-name>` | Name for the new MCP server |

## Options

| Option | Description |
|--------|-------------|
| `--command`, `-c` | Command to start the MCP server (required for STDIO transport) |
| `--arg` | Argument to pass to the command (can be used multiple times) |
| `--env`, `-e` | Environment variable in KEY=VALUE format (can be used multiple times) |
| `--url` | URL for the SSE endpoint (required for SSE transport) |
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

Add a GitHub tools MCP server with command-line arguments:

```bash
chatx mcp add github-tools --command "node" --arg "github_tools.js" --arg "--port=8766"
```

Add a database MCP server with multiple arguments:

```bash
chatx mcp add postgres-server --command "/path/to/postgres-mcp-server" --arg "--connection-string" --arg "postgresql://user:pass@localhost:5432/mydb"
```

Add an MCP server with environment variables:

```bash
chatx mcp add weather-api --command "/path/to/weather-cli" --env "API_KEY=abc123" --env "CACHE_DIR=/tmp"
```

Add an SSE MCP server:

```bash
chatx mcp add sse-backend --url "https://example.com/sse-endpoint"
```

Add an SSE MCP server in user scope (available across all directories):

```bash
chatx mcp add shared-api --url "https://api.example.org/mcp-sse" --user
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

If neither the required `--command` nor `--url` option is provided, the command will display an error:

```
Error: Either --command or --url option is required when adding an MCP server.
```

If both `--command` and `--url` options are provided at the same time:

```
Error: Cannot use both --command and --url options. Specify either STDIO transport with --command or SSE transport with --url.
```

## Using MCP Servers

After adding an MCP server, you can use it in your CHATX sessions:

```bash
# Use a specific MCP server
chatx --mcp-server postgres-server "Query for database information about customers"

# Use multiple MCP servers
chatx --mcp-servers postgres-server,github-tools "Find customer issues in GitHub"
```