# Using the --command Option with MCP Servers

This tutorial explains how to effectively use the `--command` option when setting up Model Context Protocol (MCP) servers in ChatX. MCP servers allow AI models to access external tools, databases, APIs, and other resources.

## Basic Concepts

The `--command` option specifies which command to execute when starting an MCP server. This is used with the `chatx mcp add` command to create a new MCP server configuration.

## Basic Usage

### Creating a Simple MCP Server

The most basic form of creating an MCP server using the `--command` option is:

```bash
chatx mcp add my-server --command "/path/to/server"
```

This will execute the server binary or script at the specified path when the MCP server is started.

### Using Script Interpreters

For script files, you need to specify both the interpreter and the script:

```bash
# Python script
chatx mcp add python-server --command "python" --arg "my_server.py"

# Node.js script
chatx mcp add node-server --command "node" --arg "server.js"

# Bash script
chatx mcp add shell-tools --command "bash" --arg "tools.sh"
```

## Advanced Usage

### Passing Command Line Arguments

Use the `--arg` option to pass arguments to your command:

```bash
chatx mcp add database --command "python" --arg "db_server.py" --arg "--port=5432" --arg "--debug"
```

This will execute `python db_server.py --port=5432 --debug` when the MCP server starts.

### Setting Environment Variables

You can provide environment variables using the `--env` option:

```bash
chatx mcp add weather --command "./weather_service" --env "API_KEY=abc123" --env "UNITS=metric"
```

### Specifying Working Directory

To run the command in a specific directory, use the `--working-dir` option:

```bash
chatx mcp add tools --command "python" --arg "tools_server.py" --working-dir "/path/to/tools"
```

## Real-World Examples

### Database Access Server

```bash
chatx mcp add postgres --command "python" --arg "db_connector.py" \
  --arg "--connection-string=postgresql://user:pass@localhost/mydb" \
  --arg "--schema=public" \
  --env "DB_PASSWORD=secret" \
  --working-dir "./database-tools"
```

### Web API Integration

```bash
chatx mcp add github-api --command "node" --arg "github_api.js" \
  --env "GITHUB_TOKEN=${GITHUB_TOKEN}" \
  --env "CACHE_DIR=/tmp/github-cache"
```

### Document Processing Tools

```bash
chatx mcp add document-tools --command "./doc_processor" \
  --arg "--docs-dir=/path/to/documents" \
  --arg "--ocr-enabled" \
  --arg "--max-size=10MB"
```

## Scopes and Persistence

MCP servers can be created in different scopes:

```bash
# Local scope (default) - available only in current directory
chatx mcp add local-tool --command "./local_tool"

# User scope - available to the current user in any directory
chatx mcp add shared-tool --command "/usr/local/bin/tool" --user

# Global scope - available to all users on the system
chatx mcp add system-tool --command "/opt/tools/system_tool" --global
```

## Best Practices

1. **Use Full Paths**: When possible, use full paths to ensure the command can be found regardless of the current directory.

2. **Add Error Handling**: Make sure your MCP server command handles errors gracefully and returns appropriate error messages.

3. **Secure Sensitive Information**: Use environment variables for sensitive information like API keys rather than passing them as command arguments.

4. **Document Your Servers**: Add comments or documentation about what each MCP server does and how it should be used.

5. **Choose the Right Scope**: Use local scope for project-specific servers and user or global scope for shared servers.

6. **Validate Before Using**: After adding an MCP server, test it with a simple query to make sure it works as expected.

## Troubleshooting

If your MCP server isn't working correctly:

1. **Check Command Execution**: Try running the command directly in your terminal to ensure it works.

2. **Verify Path**: Make sure the command path is correct and accessible.

3. **Check Permissions**: Ensure the command file is executable (`chmod +x` for scripts).

4. **Review Logs**: Check for error messages in the ChatX output.

5. **Update MCP Server**: If you've made changes to your MCP server implementation, you may need to remove and re-add the configuration.

## Related Options

- `--arg`: Pass arguments to your command
- `--env`: Set environment variables
- `--working-dir`: Specify the working directory
- `--url`: Use an SSE endpoint instead of a command (alternative to `--command`)

## Next Steps

Now that you understand how to use the `--command` option, you can:

- Create custom MCP servers for specific tools or data sources
- Connect ChatX to databases and APIs
- Implement document processing capabilities
- Build specialized tools that your AI assistant can use