# --url

Specify the URL endpoint for an SSE-based Model Context Protocol (MCP) server.

## Syntax

```bash
chatx mcp add <server-name> --url <url-endpoint> [other options]
```

## Description

The `--url` option is used with the `chatx mcp add` command to specify the URL endpoint for an MCP server that communicates using Server-Sent Events (SSE) rather than STDIO. This allows ChatX to connect to remote MCP servers or services that implement the MCP protocol over HTTP.

SSE-based MCP servers provide AI models with capabilities through a web-based API, such as:
- Cloud-hosted tools and services
- Remote APIs and data sources
- Third-party integrations
- Cross-platform tools that are accessible via HTTP

This option is used as an alternative to `--command` when creating MCP servers that use the SSE transport instead of STDIO.

## Examples

### Basic Usage

```bash
chatx mcp add remote-tools --url "https://example.com/mcp/tools"
```

### With Authentication Header

```bash
chatx mcp add api-service --url "https://api.example.com/mcp" --env "Authorization=Bearer token123"
```

### Local Development Server

```bash
chatx mcp add local-dev --url "http://localhost:3000/sse"
```

### Create a Persistent Server in User Scope

```bash
chatx mcp add shared-service --url "https://company.example.com/mcp-service" --user
```

## Using SSE-Based MCP Servers

Once added, SSE-based MCP servers can be used just like any other MCP server:

```bash
# Use a specific SSE-based MCP server
chatx --use-mcp remote-tools

# Ask a question that might use the tool
chatx --question "Get the latest data from our API"
```

## Security Considerations

When adding SSE-based MCP servers:

1. **Verify the source**: Only add MCP servers from trusted sources
2. **Use HTTPS**: Prefer endpoints that use HTTPS for secure communication
3. **Protect credentials**: Use environment variables to pass authentication tokens rather than including them in the URL
4. **Check permissions**: Be aware that the AI will be able to interact with whatever capabilities the MCP server provides

## Related

- [--command](./command.md) - Specify command for STDIO-based MCP servers
- [--env](./env.md) - Set environment variables (useful for authentication headers)