I'll analyze the information provided to understand how claude-code stores and manages MCP (Model Context Protocol) server configurations.

Let me search for information about the MCP server configuration files.

Let me search more broadly to find information about MCP server configurations:

Let me search specifically for information about JSON configuration files:

Let me search for all information about .mcp.json:

Let me use a more general search:

Based on the documentation in the provided files, let me explain how Claude Code stores and manages MCP (Model Context Protocol) server configurations:

# Claude Code MCP Server Storage and Management

## Storage Formats and Locations

Claude Code stores MCP server configurations in JSON files across three different locations, depending on the scope:

1. **Project-scoped** (.mcp.json):
   - Stored in a `.mcp.json` file at the root of your project
   - Should be committed to version control for team sharing
   - Format:
   ```json
   {
     "mcpServers": {
       "server-name": {
         "command": "/path/to/server",
         "args": [],
         "env": {}
       }
     }
   }
   ```

2. **Local-scoped** (default):
   - Stored in project-specific user settings
   - Available only to you when working in the current project
   - Previously called "project" scope in older versions

3. **User-scoped**:
   - Available to you across all projects on your machine
   - Private to your user account
   - Previously called "global" scope in older versions

## Command Structure

All MCP server operations are performed through the `claude mcp` command:

### Adding Servers

```bash
# Basic syntax
claude mcp add <name> <command> [args...]

# With environment variables
claude mcp add my-server -e API_KEY=123 -- /path/to/server arg1 arg2

# Adding an SSE server (instead of stdio)
claude mcp add --transport sse sse-server https://example.com/sse-endpoint

# Specify scope (local, project, or user)
claude mcp add shared-server -s project /path/to/server
claude mcp add my-user-server -s user /path/to/server

# Add from JSON configuration
claude mcp add-json weather-api '{"type":"stdio","command":"/path/to/weather-cli","args":["--api-key","abc123"],"env":{"CACHE_DIR":"/tmp"}}'
```

### Managing Servers

```bash
# List all configured servers
claude mcp list

# Get details for a specific server
claude mcp get my-server

# Remove a server
claude mcp remove my-server

# Reset project scope choices
claude mcp reset-project-choices

# Import servers from Claude Desktop
claude mcp add-from-claude-desktop
```

## JSON Schema Details

The MCP server configuration JSON schema includes these key properties:

1. **For Stdio Servers**:
   ```json
   {
     "type": "stdio",
     "command": "/path/to/executable",
     "args": ["arg1", "arg2"],
     "env": {
       "ENV_VAR1": "value1",
       "ENV_VAR2": "value2"
     }
   }
   ```

2. **For SSE Servers**:
   ```json
   {
     "type": "sse",
     "url": "https://example.com/sse-endpoint"
   }
   ```

## Scope Precedence

When multiple servers with the same name exist in different scopes, Claude Code follows this precedence order:
1. Local-scoped servers take highest precedence
2. Project-scoped servers (from `.mcp.json`) take precedence over user-scoped
3. User-scoped servers have lowest precedence

## Security Considerations

- Project-scoped servers (from `.mcp.json`) require approval before use
- You can reset your choices for project-scoped servers with `claude mcp reset-project-choices`
- Environment variables with sensitive information can be passed using the `-e` flag rather than hardcoding in JSON

## Examples of Common MCP Servers

1. **Postgres Database Server**:
   ```bash
   claude mcp add postgres-server /path/to/postgres-mcp-server --connection-string "postgresql://user:pass@localhost:5432/mydb"
   ```

2. **Weather API**:
   ```bash
   claude mcp add-json weather-api '{"type":"stdio","command":"/path/to/weather-cli","args":["--api-key","abc123"],"env":{"CACHE_DIR":"/tmp"}}'
   ```

3. **Claude Code as an MCP Server**:
   ```bash
   # Start Claude as an MCP server
   claude mcp serve
   
   # In another application like Claude Desktop, add:
   {
     "command": "claude",
     "args": ["mcp", "serve"],
     "env": {}
   }
   ```

This storage approach provides flexibility for individual developers to have their own configurations while also enabling team-wide sharing of servers through version control.

