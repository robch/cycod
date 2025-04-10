# help

Access the ChatX help system.

## Syntax

```bash
chatx help [topics]
chatx help topics --expand
chatx help find "TERMS"
chatx help [TOPIC]
```

## Description

The `chatx help` command provides access to ChatX's built-in help system. It allows you to view documentation on various topics, explore all available commands and options, and search for specific information.

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
chatx help topics
```

View the complete help documentation for all topics:

```bash
chatx help topics --expand
```

Search for help on a specific term:

```bash
chatx help find "openai"
```

Get help on a specific topic:

```bash
chatx help config
```

Get help on a specific command:

```bash
chatx help config set
```

## Output

### Example output for `chatx help topics`

```
Available help topics:

  chatx help usage
  chatx help examples
  chatx help options
  chatx help provider
  chatx help alias
  chatx help aliases
  chatx help prompt
  chatx help prompts
  chatx help config
  chatx help configuration
  chatx help mcp
  chatx help github
  chatx help chat history
  chatx help slash commands
```

### Example output for specific topic

When requesting help on a specific topic, ChatX displays comprehensive information about that topic, including syntax, options, and examples.

## Related Topics

- [config](config/index.md) - Configuration commands
- [alias](alias/index.md) - Alias commands
- [prompt](prompt/index.md) - Prompt commands
- [options](options/index.md) - Command line options