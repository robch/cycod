# --arg

Pass an argument to a command when adding an MCP server.

## Syntax

```bash
cycod mcp add <server-name> --command <command> --arg <argument-value> [--arg <another-argument>]
```

## Description

The `--arg` option is used with the `cycod mcp add` command to specify command-line arguments that should be passed to the MCP server process when it's launched. You can use the `--arg` option multiple times to pass multiple arguments.

This option is typically used in conjunction with the `--command` option, which specifies the executable or script to run.

## Usage

Each `--arg` option adds one argument to the command. Arguments are passed to the command in the same order they are specified.

### Argument Types

The `--arg` option can handle different types of arguments:

- **Flag arguments**: Simple switches like `--verbose` or `-v`
- **Key-value arguments**: Options with values like `--port=8080` or `--config config.json`
- **Positional arguments**: Values that are positioned in a specific order

### Quoting Rules

When an argument contains spaces or special characters, enclose it in quotes:

```bash
# For arguments with spaces
cycod mcp add file-server --command "node" --arg "server.js" --arg "--path" --arg "C:\My Documents\Files"

# For arguments with special characters
cycod mcp add api-server --command "python" --arg "api.py" --arg "--url=https://api.example.com?key=abc&format=json"
```

## When to Use --arg vs --env

CYCOD offers two ways to provide configuration values to MCP servers:

| Use `--arg` when: | Use `--env` when: |
|-------------------|-------------------|
| Passing standard program arguments | Setting environment variables |
| Specifying configuration flags | Handling sensitive information (API keys, passwords) |
| Providing positional parameters | Configuring application behavior |
| Passing file paths or URLs | Working with applications that use ENV for configuration |

## Examples

### Pass multiple arguments to a Python script

```bash
cycod mcp add python-tools --command "python" --arg "script.py" --arg "--verbose" --arg "--port=8080"
```

This would run `python script.py --verbose --port=8080` when the MCP server is started.

### Pass configuration file path to a Node.js application

```bash
cycod mcp add api-server --command "node" --arg "server.js" --arg "--config" --arg "./config/production.json"
```

### Specify database connection parameters

```bash
cycod mcp add database --command "/path/to/db-server" --arg "--connection-string" --arg "postgresql://user:password@localhost:5432/mydatabase"
```

### Pass arguments to a Java application

```bash
cycod mcp add java-processor --command "java" --arg "-jar" --arg "processor.jar" --arg "-Xmx2g" --arg "--input" --arg "data.csv"
```

### Combine with environment variables for comprehensive configuration

```bash
cycod mcp add search-tool \
  --command "python" \
  --arg "search.py" \
  --arg "--cache-dir" \
  --arg "/tmp/cache" \
  --env "API_KEY=abc123" \
  --env "DEBUG=false" \
  --user
```

This adds a user-scoped MCP server that runs `python search.py --cache-dir /tmp/cache` with environment variables `API_KEY=abc123` and `DEBUG=false`.

### Provide multiple flag arguments

```bash
cycod mcp add code-analyzer --command "./analyzer" --arg "--verbose" --arg "--no-cache" --arg "--color" --arg "--format=json"
```

## Related

- [`cycod mcp add`](../mcp/add.md): Add a new MCP server
- [`--command`](./command.md): Specify the command to run
- [`--env`](./env.md): Set environment variables for the command