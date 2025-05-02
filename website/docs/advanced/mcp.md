---
hide:
- toc
---

--8<-- "snippets/ai-generated.md"

# Model Context Protocol (MCP)

Model Context Protocol (MCP) is an advanced feature in CYCOD that allows you to connect to custom model providers, giving AI models access to external tools, databases, and APIs.

## What is MCP?

MCP is a protocol that enables communication between language models and external tools. It allows models to:

- Access databases and external data sources
- Call APIs and web services
- Execute tools and commands
- Work with file systems and documents

In CYCOD, MCP servers provide these capabilities to the AI models during conversations.

## MCP Servers

An MCP server is a process that implements the Model Context Protocol and provides specific capabilities to AI models. CYCOD can connect to these servers to extend the functionality available during chat sessions.

### Types of MCP Servers

CYCOD supports two types of MCP servers:

1. **STDIO MCP servers**: Command-line processes that communicate via standard input/output
2. **SSE MCP servers**: Web services that use Server-Sent Events for communication

## Managing MCP Servers

### Listing MCP Servers

To list all available MCP servers:

```bash title="List MCP servers"
cycod mcp list
```

This shows MCP servers from all scopes (equivalent to using `--any`). To list servers from a specific scope:

```bash title="List user-level MCP servers"
cycod mcp list --user
```

### Viewing MCP Server Details

To see the details of a specific MCP server:

```bash title="View MCP server details"
cycod mcp get postgres-server
```

### Adding MCP Servers

To add a new MCP server:

```bash title="Add STDIO MCP server"
cycod mcp add postgres-server --command /path/to/postgres-mcp-server --arg --connection-string --arg "postgresql://user:pass@localhost:5432/mydb"
```

```bash title="Add SSE MCP server"
cycod mcp add weather-api --url https://example.com/mcp/weather
```

### Server Scopes

Like other CYCOD features, MCP servers can be added in different scopes:

```bash title="Add a user-level MCP server"
cycod mcp add shared-tool --command /usr/local/bin/tool-server --arg --config --arg /path/to/config.json --user
```

```bash title="Add a global MCP server"
cycod mcp add system-api --url https://internal-api.example.com/mcp --global
```

### Removing MCP Servers

To remove an MCP server:

```bash title="Remove MCP server"
cycod mcp remove postgres-server
```

This removes the server from the first scope it's found in (equivalent to using `--any`).

To remove from a specific scope:

```bash title="Remove from user scope"
cycod mcp remove shared-tool --user
```

For detailed instructions on removing MCP servers, including best practices and common scenarios, see our [Removing MCP Servers](../tutorials/removing-mcp-servers.md) tutorial.

## Using MCP Servers

To use an MCP server in a chat session:

```bash title="Use MCP server"
cycod --use-mcp postgres-server --question "What tables are in the database?"
```

You can use multiple MCP servers in the same session:

```bash title="Use multiple MCP servers"
cycod --use-mcp postgres-server --use-mcp weather-api --question "What's the weather like in cities where we have offices?"
```

## Creating Custom MCP Servers

### Basic STDIO MCP Server

Here's a simple example of how to create a basic STDIO MCP server in Python:

```python title="simple_mcp_server.py"
#!/usr/bin/env python3
import json
import sys

def main():
    # Print the server capabilities
    print(json.dumps({
        "protocol": "mcp-0.1",
        "capabilities": {
            "execute_commands": {
                "description": "Execute simple commands"
            }
        }
    }))
    sys.stdout.flush()

    # Process incoming messages
    while True:
        try:
            line = sys.stdin.readline()
            if not line:
                break
                
            message = json.loads(line)
            
            # Handle execute_commands capability
            if message.get("type") == "execute_commands":
                command = message.get("command", "")
                result = f"Executed: {command}"
                
                # Send response
                response = {
                    "type": "execute_commands_result",
                    "id": message.get("id"),
                    "result": result
                }
                print(json.dumps(response))
                sys.stdout.flush()
                
        except Exception as e:
            # Send error response
            error_response = {
                "type": "error",
                "id": message.get("id") if "message" in locals() else None,
                "error": str(e)
            }
            print(json.dumps(error_response))
            sys.stdout.flush()

if __name__ == "__main__":
    main()
```

Add this server to CYCOD:

```bash
chmod +x simple_mcp_server.py
cycod mcp add simple-commands --command ./simple_mcp_server.py
```

## Example MCP Use Cases

### Database Access

Create an MCP server that connects to your database and allows the AI model to query data:

```bash
cycod mcp add database --command ./db_mcp_server.py --arg --db-url --arg "postgresql://user:pass@localhost/mydb"
cycod --use-mcp database --question "What were our sales figures for Q1 2025?"
```

### API Integration

Connect to a weather API and allow the AI model to access weather data:

```bash
cycod mcp add weather --command ./weather_mcp_server.py --arg --api-key --arg "YOUR_API_KEY"
cycod --use-mcp weather --question "What's the weather forecast for New York this weekend?"
```

### Document Processing

Set up an MCP server that can process and analyze documents:

```bash
cycod mcp add documents --command ./doc_processor.py --arg --docs-dir --arg "/path/to/documents"
cycod --use-mcp documents --question "What are the key points from our quarterly report?"
```

## Setting Environment Variables for MCP Servers

Environment variables provide a secure and flexible way to configure MCP servers without hardcoding values in scripts. With CYCOD, you can set environment variables using the `--env` option when adding an MCP server.

### Basic Usage

Use the `--env` option (or its short form `-e`) with a key-value pair:

```bash title="Setting an environment variable"
cycod mcp add database --command ./db_server.py --env "DB_PASSWORD=secure123"
```

You can set multiple environment variables:

```bash title="Setting multiple environment variables"
cycod mcp add api-server --command ./server.js \
  --env "PORT=3000" \
  --env "NODE_ENV=production" \
  --env "LOG_LEVEL=info"
```

### Common Use Cases for Environment Variables

1. **API Keys and Authentication**:
   ```bash
   cycod mcp add weather --command ./weather.py --env "API_KEY=your_api_key"
   ```

2. **Configuration Paths**:
   ```bash
   cycod mcp add toolkit --command ./toolkit.py --env "CONFIG_PATH=/etc/toolkit/config.json"
   ```

3. **Feature Flags and Operation Modes**:
   ```bash
   cycod mcp add processor --command ./processor.py --env "ENABLE_CACHE=true" --env "DEBUG_MODE=false"
   ```

4. **Connection Strings**:
   ```bash
   cycod mcp add database --command ./db_client.py --env "CONNECTION_STRING=mongodb://localhost:27017"
   ```

### Environment Variables vs. Command Arguments

For configuration, you can use either environment variables (`--env`) or command arguments (`--arg`). Here's when to use each:

| Use Environment Variables (`--env`) when | Use Command Arguments (`--arg`) when |
|------------------------------------------|-------------------------------------|
| Handling sensitive information (passwords, API keys) | Passing standard program options |
| Configuring the MCP server's runtime environment | Specifying input/output file paths |
| Setting general configuration values | Using flag arguments (e.g., `--verbose`) |
| Working with applications that expect ENV configuration | Passing positional arguments |

## Security Considerations

When working with MCP servers, keep these security considerations in mind:

1. **Access control**: MCP servers may have access to sensitive systems or data, so be careful about who can use them
2. **Input validation**: Ensure your MCP servers validate and sanitize all inputs from the AI model
3. **Rate limiting**: Implement rate limiting to prevent abuse of external services
4. **Credentials management**: Don't hardcode credentials in MCP server configuration; use environment variables or secure storage
5. **Scope isolation**: Use local scope for project-specific MCP servers to prevent unintended access

## MCP Server Best Practices

1. **Implement error handling**: Provide clear error messages when things go wrong
2. **Include documentation**: Document the capabilities your MCP server provides
3. **Log interactions**: Log important events for debugging and auditing
4. **Implement timeouts**: Prevent long-running operations from blocking the conversation
5. **Add authentication**: Secure your SSE MCP servers with appropriate authentication
6. **Use the right transport**: Choose STDIO for local tools and SSE for network services