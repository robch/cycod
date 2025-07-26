# help

Access the CycoD help system.

## Syntax

```bash
cycod help [topics]
cycod help topics --expand
cycod help find "TERMS"
cycod help [TOPIC]
```

## Description

The `cycod help` command provides access to CycoD's built-in help system. It allows you to view documentation on various topics, explore all available commands and options, and search for specific information.

## Options

| Option | Description |
|--------|-------------|
| `topics` | Lists all available help topics |
| `topics --expand` | Shows complete help documentation for all topics |
| `find "TERMS"` | Searches for specific terms within the help system |
| `[TOPIC]` | Shows help for a specific topic (e.g., `config`, `alias`, `options`) |

## Examples

List all available help topics:

```bash
cycod help topics
```

View the complete help documentation for all topics:

```bash
cycod help topics --expand
```

Search for help on a specific term:

```bash
cycod help find "openai"
```

Get help on a specific topic:

```bash
cycod help config
```

Get help on a specific command:

```bash
cycod help config set
```

## Output

### Example output for `cycod help topics`

```
Available help topics:

  cycod help usage
  cycod help examples
  cycod help options
  cycod help provider
  cycod help alias
  cycod help aliases
  cycod help prompt
  cycod help prompts
  cycod help config
  cycod help configuration
  cycod help mcp
  cycod help github
  cycod help chat history
  cycod help slash commands
```

### Example output for specific topic

When requesting help on a specific topic, CycoD displays comprehensive information about that topic, including syntax, options, and examples.

## Related Topics

- [config](config/index.md) - Configuration commands
- [alias](alias/index.md) - Alias commands
- [prompt](prompt/index.md) - Prompt commands
- [options](options/index.md) - Command line options