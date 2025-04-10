# --working-dir

Set the working directory for a command when adding an MCP server.

## Syntax

```bash
chatx mcp add <server-name> --command <command> --working-dir <directory-path> [other options]
```

## Description

The `--working-dir` option is used with the `chatx mcp add` command to specify the working directory in which the MCP server process should run. This sets the current directory context for the command, affecting how relative paths are resolved and where file operations occur.

This option is particularly useful when:
- The MCP server requires specific file locations
- The command depends on configuration files in a particular directory
- You want to isolate the MCP server's file operations to a specific location
- Running scripts that assume a certain directory structure

## Examples

### Basic Usage

```bash
chatx mcp add python-tools --command "python" --arg "tools.py" --working-dir "/path/to/tools"
```

### With Relative Path

```bash
chatx mcp add local-server --command "./server" --working-dir "./server-directory"
```

### With Environment Variables

```bash
chatx mcp add data-processor --command "node" --arg "process.js" --working-dir "/data/processing" --env "API_KEY=abc123"
```

### Create in User Scope

```bash
chatx mcp add shared-tool --command "/usr/local/bin/tool" --working-dir "/usr/local/share/tool" --user
```

## Working Directory Resolution

The working directory path can be specified as:

- **Absolute path**: Starting with `/` (Unix) or a drive letter (Windows)
- **Relative path**: Relative to the current directory when running the `chatx mcp add` command

When the MCP server is later started by ChatX, the working directory will be applied before executing the command.

## Best Practices

1. **Use absolute paths** for predictable behavior across different environments
2. **Keep configuration files** in the same working directory
3. **Consider using environment variables** for dynamic paths
4. **Document required directory structure** for MCP servers shared with others

## Related

- [--command](./command.md) - Specify the command to run
- [--arg](./arg.md) - Specify arguments to pass to the command
- [--env](./env.md) - Set environment variables for the command