# Understanding Claude Code with MCP (Model Context Protocol)

## What is MCP?

The Model Context Protocol (MCP) is an open protocol developed by Anthropic that enables Claude and other LLMs to access external tools and data sources. MCP follows a client-server architecture where:
- Claude Code acts as the client
- Specialized servers provide capabilities like database access, API integrations, or tool execution

## How MCP Works with Claude Code

### Basic Concepts

1. **Client-Server Architecture**:
   - Claude Code (client) connects to MCP servers
   - Each server provides specific capabilities (database access, APIs, etc.)
   - Claude can leverage multiple servers simultaneously

2. **Server Scopes**:
   - **Local-scoped**: Available only to you in the current project (default)
   - **Project-scoped**: Shared with everyone via `.mcp.json` file in version control
   - **User-scoped**: Available to you across all projects on your machine

3. **Server Types**:
   - **Stdio Servers**: Command-line processes that communicate via standard I/O
   - **SSE Servers**: Web servers that communicate via Server-Sent Events

## Setting Up MCP Servers

### Adding an MCP Server

```bash
# Basic syntax
claude mcp add <name> <command> [args...]

# Example: Adding a local server with environment variables
claude mcp add my-server -e API_KEY=123 -- /path/to/server arg1 arg2

# Example: Adding an SSE server
claude mcp add --transport sse sse-server https://example.com/sse-endpoint
```

### Managing MCP Servers

```bash
# List all configured servers
claude mcp list

# Get details for a specific server
claude mcp get my-server

# Remove a server
claude mcp remove my-server
```

### Server Scopes in Detail

```bash
# Local-scoped server (default)
claude mcp add my-private-server /path/to/server

# Project-scoped server (creates/updates .mcp.json)
claude mcp add shared-server -s project /path/to/server

# User-scoped server (available across all projects)
claude mcp add my-user-server -s user /path/to/server
```

The `.mcp.json` file created by project-scoped servers looks like:
```json
{
  "mcpServers": {
    "shared-server": {
      "command": "/path/to/server",
      "args": [],
      "env": {}
    }
  }
}
```

### Advanced MCP Configuration

```bash
# Add a server from JSON configuration
claude mcp add-json weather-api '{"type":"stdio","command":"/path/to/weather-cli","args":["--api-key","abc123"],"env":{"CACHE_DIR":"/tmp"}}'

# Import servers from Claude Desktop
claude mcp add-from-claude-desktop

# Reset your choices for project-scoped servers
claude mcp reset-project-choices
```

## MCP Server Examples

### Database Access with Postgres MCP

```bash
# Add a PostgreSQL server
claude mcp add postgres-server /path/to/postgres-mcp-server --connection-string "postgresql://user:pass@localhost:5432/mydb"
```

Then in your Claude session:
```
> describe the schema of our users table
> what are the most recent orders in the system?
> show me the relationship between customers and invoices
```

### Claude Code as an MCP Server

You can also use Claude Code itself as an MCP server for other applications:

```bash
# Start Claude as an MCP server
claude mcp serve
```

Then in another application like Claude Desktop, add this server with:
```json
{
  "command": "claude",
  "args": ["mcp", "serve"],
  "env": {}
}
```

## Environment Variables for MCP

Claude Code provides specific environment variables for controlling MCP behavior:

```bash
# Set MCP server startup timeout (in milliseconds)
MCP_TIMEOUT=10000 claude

# Set MCP tool execution timeout (in milliseconds)
MCP_TOOL_TIMEOUT=5000 claude
```

## Security Considerations with MCP

When using MCP servers:

1. **Trust Verification**: Only use trusted MCP servers; untrusted servers could pose security risks
2. **Approval System**: Claude Code will prompt for approval before using project-scoped servers
3. **Careful Network Access**: Be especially cautious with MCP servers that access the internet, as they can expose you to prompt injection risks
4. **Limited Permissions**: For database servers like Postgres MCP, use connection strings with limited permissions (read-only when possible)

## Best Practices for MCP

1. **Project Sharing**: Use project-scoped servers with `.mcp.json` for team collaboration
2. **Access Control**: Configure servers with minimal necessary permissions
3. **Environment Variable Handling**: Use `-e` flag to pass sensitive environment variables rather than hardcoding them
4. **Timeouts**: Set appropriate timeouts for your servers

## Troubleshooting MCP

1. **Server Status**: Check server status with `/mcp` command within Claude Code
2. **Verbose Logging**: Run with `claude --verbose` to get detailed logs
3. **Common Issues**:
   - Permission problems: Ensure proper file paths and permissions
   - Timeout errors: Adjust with `MCP_TIMEOUT` and `MCP_TOOL_TIMEOUT` variables
   - Connection errors: Verify network settings, particularly in containerized environments

## Implementing MCP in Other Tools

To implement MCP-like functionality in other LLM interfaces (like ChatX), you would need:

1. **Protocol Design**: Create a standardized communication protocol between the LLM client and external servers
2. **Server Management**: Build commands to add, list, and remove servers
3. **Scope Handling**: Implement local, user, and project scopes for server configurations
4. **Security Model**: Create a permissions system for approving server operations
5. **Data Connectors**: Develop specialized connectors for common data sources (databases, APIs)
6. **Team Sharing**: Enable sharing of server configurations via version control

The key challenge in implementing MCP is balancing flexibility and security - providing powerful external capabilities while maintaining appropriate safeguards against misuse or unintended access.