---
hide:
- toc
---

--8<-- "snippets/ai-generated.md"

# Managing MCP Servers with CycoD

This tutorial will guide you through the process of managing Model Context Protocol (MCP) servers using the `cycod mcp list` command and related MCP commands.

## What are MCP Servers?

Model Context Protocol (MCP) servers enhance your AI assistant by providing access to:

- Databases and data sources
- Web APIs and services
- Custom tools and utilities
- File systems and documents

MCP servers run as separate processes that CycoD connects to when needed. They extend what your AI assistant can do beyond just conversation.

## Listing Your MCP Servers

The first step in managing MCP servers is knowing what servers are available. The `cycod mcp list` command shows all your configured MCP servers.

### Basic Usage

To see all MCP servers in all scopes:

```bash
cycod mcp list
```

This command displays servers from global, user, and local scopes, with the most details about each server.

### Understanding the Output

Here's an example output and what each part means:

```
MCP SERVERS (scope: user)
  postgres-server:                            # Server name
    command: python db_server.py              # Command to run
    args: ["--db", "mydb"]                    # Command arguments
    env: {"DB_PASSWORD": "******"}            # Environment variables (sensitive values masked)
    auto_start: true                          # Whether server starts automatically

  weather-api:
    url: https://example.com/mcp/weather      # URL for SSE servers
    auto_start: true
```

### Filtering by Scope

MCP servers can exist in three scopes:

- **Local**: Available only in the current directory
- **User**: Available to your user account in any directory
- **Global**: Available to all users on the system

To list only servers in a specific scope:

```bash
# List only local MCP servers
cycod mcp list --local

# List only user MCP servers
cycod mcp list --user

# List only global MCP servers
cycod mcp list --global
```

### Getting JSON Output

For scripting or integration with other tools, you can output the server list in JSON format:

```bash
cycod mcp list --json
```

## Practical Examples

### Scenario 1: Checking Available Tools

Before starting a data analysis project:

```bash
# Check what database servers are available
cycod mcp list
```

This helps you see if you need to set up new MCP servers for your project.

### Scenario 2: Troubleshooting

If an MCP server isn't working as expected:

```bash
# Check the server configuration
cycod mcp list --user
```

This lets you verify the server is correctly configured with the right command, arguments, and environment variables.

### Scenario 3: Environment Management

When setting up a new development environment:

```bash
# Check if any local MCP servers exist
cycod mcp list --local

# If none exist, you might need to add them
cycod mcp add postgres-dev --command python --arg db_server.py --arg --db=dev
```

## MCP Server Management Workflow

Here's a typical workflow for managing MCP servers:

1. **List existing servers**
   ```bash
   cycod mcp list
   ```

2. **Examine a specific server's details**
   ```bash
   cycod mcp get postgres-server
   ```

3. **Add a new server if needed**
   ```bash
   cycod mcp add new-server --command ./server.py --arg --port=8000
   ```

4. **Verify the new server appears in the list**
   ```bash
   cycod mcp list
   ```

5. **Use the server in a chat session**
   ```bash
   cycod --use-mcp new-server --question "What data can you access?"
   ```

6. **Remove the server when no longer needed**
   ```bash
   cycod mcp remove new-server
   ```
   
   For more details on removing servers, see [Removing MCP Servers](removing-mcp-servers.md).

## Best Practices

### Organizing Servers by Scope

- Use **local** scope for project-specific servers
- Use **user** scope for personal tools you need everywhere
- Use **global** scope for shared tools in team environments

### Server Naming Conventions

Create consistent naming patterns:

- `<service>-<environment>`: `postgres-dev`, `postgres-prod`
- `<function>-<type>`: `search-code`, `search-docs`
- `<project>-<tool>`: `blog-api`, `blog-db`

### Regular Inventory

Periodically list and clean up unused MCP servers:

```bash
# List all servers
cycod mcp list

# Remove unused servers
cycod mcp remove unused-server
```

For detailed instructions on effectively removing MCP servers, including working with different scopes and best practices, see our [Removing MCP Servers](removing-mcp-servers.md) tutorial.

## Troubleshooting

### No Servers Listed

If `cycod mcp list` shows no servers:

1. Check if you're looking in the right scope (`--local`, `--user`, `--global`)
2. Verify that you've added MCP servers before
3. Check if your MCP configuration files are accessible

### Server Listed but Not Working

If an MCP server is listed but doesn't work:

1. Check the server details with `cycod mcp get SERVER_NAME`
2. Verify the command path is correct and the file exists
3. Try running the command directly in your terminal
4. Check environment variables and arguments for correctness

## Next Steps

Now that you know how to manage and list MCP servers, you can:

- Create [custom MCP servers](../advanced/mcp.md) for your specific needs
- Explore how to [pass environment variables](env-variables-mcp.md) to your MCP servers

With effective MCP server management, your AI assistant becomes much more powerful and capable of working with your specific tools and data sources.