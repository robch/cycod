# --env

Set environment variables for an MCP server process.

## Syntax

```bash
chatx mcp add <server-name> --command <command> --env <KEY=VALUE> [--env <ANOTHER_KEY=ANOTHER_VALUE>]
# OR using the short form
chatx mcp add <server-name> --command <command> -e <KEY=VALUE> [-e <ANOTHER_KEY=ANOTHER_VALUE>]
```

## Description

The `--env` option (or its short form `-e`) is used with the `chatx mcp add` command to specify environment variables that should be set for the MCP server process when it's launched. You can use the `--env` option multiple times to set multiple environment variables.

Environment variables are passed as key-value pairs in the format `KEY=VALUE`. This is useful for configuring MCP servers with API keys, configuration paths, settings, and other parameters without hardcoding them in your scripts.

## Usage

Each `--env` option adds one environment variable to the process. The environment variables will be available to the MCP server process when it runs.

## Examples

### Set an API key for a weather service

```bash
chatx mcp add weather-api --command "/path/to/weather-cli" --env "API_KEY=abc123"
```

This sets the environment variable `API_KEY` with value `abc123` when the MCP server is launched.

### Set multiple environment variables

```bash
chatx mcp add data-processor --command "./processor.js" --env "NODE_ENV=production" --env "DEBUG=false" --env "CACHE_DIR=/tmp/cache"
```

This sets three environment variables: `NODE_ENV`, `DEBUG`, and `CACHE_DIR`.

### Using the short form

```bash
chatx mcp add logger --command "python logger.py" -e "LOG_LEVEL=DEBUG" -e "LOG_PATH=/var/log/app"
```

This uses the short form `-e` to set environment variables.

### Combine with other MCP options

```bash
chatx mcp add search-tool --command "python" --arg "search.py" --working-dir "./tools" --env "API_KEY=abc123" --user
```

This adds a user-scoped MCP server that runs `python search.py` in the `./tools` directory with the environment variable `API_KEY=abc123`.

## Security Considerations

- Avoid passing sensitive information like API keys or passwords as command-line arguments, as they might be visible in process listings. Using environment variables with `--env` is more secure.
- For highly sensitive information, consider using a more secure credential management system and having your MCP server access it directly.

## Related

- [`chatx mcp add`](../mcp/add.md): Add a new MCP server
- [`--command`](./command.md): Specify the command to run
- [`--arg`](./arg.md): Pass arguments to the command