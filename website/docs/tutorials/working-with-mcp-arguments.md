--8<-- "snippets/ai-generated.md"

# Working with MCP Server Arguments

This tutorial explores how to effectively use the `--arg` option to configure Model Context Protocol (MCP) servers in ChatX.

## Understanding Command Arguments

When creating an MCP server with the `chatx mcp add` command, you often need to pass arguments to the underlying program or script. The `--arg` option allows you to specify these arguments.

## Basic Usage

### Adding Single Arguments

To add a simple argument to your MCP server command:

```bash
chatx mcp add my-server --command "./server" --arg "--verbose"
```

This will run `./server --verbose` when the MCP server starts.

### Adding Multiple Arguments

You can use `--arg` multiple times to pass several arguments:

```bash
chatx mcp add api-client --command "python" --arg "api.py" --arg "--port=8080" --arg "--log-level=info"
```

This will execute `python api.py --port=8080 --log-level=info`.

## Argument Types

### Flag Arguments

Flag arguments are simple switches that enable or disable features:

```bash
chatx mcp add analyzer --command "./analyze" --arg "--verbose" --arg "--no-cache" --arg "--debug"
```

### Key-Value Arguments

Key-value arguments pass specific values to parameters:

```bash
chatx mcp add processor --command "./process" --arg "--input=data.json" --arg "--output=results.json" --arg "--threads=4"
```

### Positional Arguments

Some commands require arguments in a specific order:

```bash
chatx mcp add converter --command "python" --arg "convert.py" --arg "input.csv" --arg "output.json"
```

This runs `python convert.py input.csv output.json`.

## Handling Complex Arguments

### Arguments with Spaces

For arguments containing spaces, enclose the entire argument in quotes:

```bash
chatx mcp add file-handler --command "./handler" --arg "--path" --arg "C:\My Documents\Files"
```

### Arguments with Special Characters

When arguments contain special characters, use proper quoting:

```bash
chatx mcp add url-fetcher --command "./fetch" --arg "--url=https://api.example.com?key=abc&format=json"
```

### Arguments with Quotes

To include quotes within an argument, use different quote types for nesting:

```bash
chatx mcp add messenger --command "./send" --arg "--message='Hello, world!'"
```

## Real-World Examples

### Database Connection

Connect to a PostgreSQL database:

```bash
chatx mcp add postgres --command "./pg-connector" \
  --arg "--host=localhost" \
  --arg "--port=5432" \
  --arg "--database=mydb" \
  --arg "--schema=public"
```

### AI Model Server

Configure a local AI model server:

```bash
chatx mcp add local-ai --command "python" --arg "model_server.py" \
  --arg "--model=llama3" \
  --arg "--max-tokens=4096" \
  --arg "--temperature=0.7" \
  --arg "--cache-dir=/tmp/model-cache"
```

### Document Processing

Set up a document processing server with complex options:

```bash
chatx mcp add doc-processor --command "./doc-process" \
  --arg "--input-dir=/path/to/documents" \
  --arg "--output-dir=/path/to/results" \
  --arg "--formats=pdf,docx,txt" \
  --arg "--ocr-enabled" \
  --arg "--languages=en,es,fr" \
  --arg "--max-file-size=10MB"
```

## Argument vs Environment Variables

ChatX offers two ways to pass configuration values to MCP servers:

1. **Command Arguments** (`--arg`): Visible in process listings, good for standard options
2. **Environment Variables** (`--env`): Better for sensitive information

### When to Use Arguments

Use the `--arg` option when:

- Configuring standard program options
- Specifying file paths or URLs
- Setting flags and operational parameters
- Working with positional arguments

```bash
chatx mcp add image-processor --command "./process" --arg "--quality=high" --arg "--resize=1024x768"
```

### When to Use Environment Variables

Use the `--env` option when:

- Handling sensitive information (API keys, passwords)
- Setting configuration that shouldn't appear in process listings
- Working with applications that expect environment variables

```bash
chatx mcp add api-client --command "./client" --env "API_KEY=secret123" --env "USE_CACHE=true"
```

### Combined Approach

Often, the best approach is to use both methods:

```bash
chatx mcp add search-engine --command "python" --arg "search.py" --arg "--max-results=50" --env "API_KEY=secret123"
```

## Common Patterns by Language

### Python Scripts

```bash
chatx mcp add python-tool --command "python" --arg "script.py" --arg "--config=config.json" --arg "--verbose"
```

### Node.js Applications

```bash
chatx mcp add node-server --command "node" --arg "server.js" --arg "--port=3000" --arg "--env=production"
```

### Java Applications

```bash
chatx mcp add java-app --command "java" --arg "-jar" --arg "application.jar" --arg "-Xmx2g" --arg "--server"
```

### Shell Scripts

```bash
chatx mcp add shell-tool --command "bash" --arg "tool.sh" --arg "-v" --arg "input.txt"
```

## Best Practices

1. **Use Clear Naming**: Make argument names descriptive.

2. **Group Related Arguments**: Keep related arguments together for readability.

3. **Document Argument Purpose**: Add comments or documentation about what each argument does.

4. **Test Directly First**: Run your command with arguments directly in a terminal before adding it as an MCP server.

5. **Be Careful with Quotes**: Understand how your shell handles quotes and escape characters.

6. **Use Full Paths**: Use absolute paths for file and directory arguments to avoid confusion.

7. **Secure Sensitive Data**: Don't include sensitive information in command arguments; use environment variables instead.

## Troubleshooting

If your MCP server isn't receiving arguments correctly:

1. **Check Argument Format**: Verify that arguments are formatted properly for the target application.

2. **Test Command Execution**: Run the complete command manually to ensure it works.

3. **Check for Quote Issues**: Make sure quotes and special characters are properly escaped.

4. **Verify Argument Order**: Some applications are sensitive to the order of arguments.

5. **Review Server Logs**: Look for error messages related to command-line parsing.

## Related

- [MCP Add Command](../reference/cli/mcp/add.md): Documentation for adding MCP servers
- [--arg Option Reference](../reference/cli/options/arg.md): Detailed reference for the `--arg` option
- [--env Option](../reference/cli/options/env.md): Setting environment variables for MCP servers
- [--command Option](../reference/cli/options/command.md): Specifying the command to run

## Next Steps

Now that you understand how to effectively use command arguments with MCP servers, you can:

- Create more sophisticated MCP servers with detailed configuration
- Build specialized tools with complex options
- Implement secure handling of configuration data
- Better understand how to divide configuration between arguments and environment variables