---
hide:
- toc
---

--8<-- "snippets/ai-generated.md"

# Removing MCP Servers in ChatX

This tutorial explains how to effectively remove Model Context Protocol (MCP) servers from your ChatX configuration using the `chatx mcp remove` command.

## Understanding MCP Server Removal

When you no longer need an MCP server, it's good practice to remove it from your configuration. This helps keep your environment clean and prevents unused servers from appearing in your server list.

The `chatx mcp remove` command allows you to delete MCP server configurations from any scope - local, user, or global.

## Basic Syntax

The basic syntax for removing an MCP server is:

```bash
chatx mcp remove SERVER_NAME [--scope]
```

Where:
- `SERVER_NAME` is the name of the MCP server to remove
- `--scope` is an optional parameter specifying which scope to remove from

## Examples of Removing MCP Servers

### Example 1: Remove from Any Scope (Default)

To remove a server named "postgres-server" from the first scope it's found in:

```bash
chatx mcp remove postgres-server
```

This command searches for the server in the local scope first, then user scope, then global scope, and removes the first instance it finds.

### Example 2: Remove from a Specific Scope

To remove a server only from the user scope:

```bash
chatx mcp remove weather-api --user
```

This ensures the server is only removed from your user configuration, leaving any local or global configurations with the same name intact.

### Example 3: Removing Project-Specific Servers

When you're done with a project and want to clean up local MCP servers:

```bash
chatx mcp remove project-database --local
```

### Example 4: Removing a System-Wide Server

To remove a server from the global scope (requires appropriate permissions):

```bash
chatx mcp remove shared-tool --global
```

## Understanding Scopes When Removing Servers

It's important to understand how scopes work when removing MCP servers:

| Option | Result |
|--------|--------|
| `--local` or `-l` | Removes only from the current directory's configuration |
| `--user` or `-u` | Removes only from your user profile's configuration |
| `--global` or `-g` | Removes only from the system-wide configuration |
| `--any` or `-a` (default) | Removes from the first scope where the server is found |

## Common Use Cases

### 1. Cleaning Up After a Project

When you've finished a project, remove any project-specific MCP servers:

```bash
# List local MCP servers first
chatx mcp list --local

# Remove each project server
chatx mcp remove project-db --local
chatx mcp remove project-api --local
```

### 2. Replacing a Server with a New Version

When updating a server, you might want to remove the old version first:

```bash
# Remove the old server
chatx mcp remove old-server --user

# Add the updated server
chatx mcp add old-server --command /path/to/new/server --arg --version=2 --user
```

### 3. Regular Maintenance

Periodically clean up unused servers:

```bash
# List all servers to review
chatx mcp list

# Remove servers you no longer need
chatx mcp remove unused-server1
chatx mcp remove unused-server2
```

## Best Practices for Server Removal

1. **Verify Before Removing**: Use `chatx mcp get SERVER_NAME` to check server details before removing.

2. **Use Specific Scopes**: When possible, specify the scope (`--local`, `--user`, `--global`) to avoid accidentally removing the wrong server.

3. **Clean Up Regularly**: Periodically review your MCP servers with `chatx mcp list` and remove any that are no longer needed.

4. **Document Your Changes**: Keep track of which servers you've removed, especially in team environments.

5. **Check Dependencies**: Before removing a server, make sure no other systems or configurations depend on it.

## Troubleshooting

### Server Not Found

If you get an error saying the server wasn't found:

```
Error: MCP server 'unknown-server' not found in user scope.
```

Try checking other scopes:

```bash
# Check if the server exists in any scope
chatx mcp get unknown-server --any
```

### Removing the Wrong Server

If you accidentally remove the wrong server, you'll need to add it back using `chatx mcp add`. Make sure you have documented your server configurations or can otherwise recreate them.

## Next Steps

Now that you understand how to effectively remove MCP servers, you can:

- Learn how to [manage MCP servers](managing-mcp-servers.md) in general
- Explore how to [create custom MCP servers](../advanced/mcp.md)
- Set up [environment variables for MCP servers](env-variables-mcp.md)
- See the [command reference](../reference/cli/mcp/remove.md) for complete details on the `mcp remove` command

By maintaining a clean and well-organized set of MCP servers, you'll ensure that your ChatX environment stays efficient and easy to manage.