# Getting Help in ChatX

ChatX provides an extensive built-in help system to guide you through its features and commands. This page covers various ways to access help information in ChatX.

## Basic Help Commands

### General Help

To get started with ChatX help:

```bash
chatx help
```

This displays a general overview of ChatX usage patterns and available commands.

### Topic-Specific Help

To get help on a specific topic:

```bash
chatx help [TOPIC]
```

Replace `[TOPIC]` with any valid help topic such as `config`, `alias`, or `options`.

## Exploring Available Help Topics

### Listing Topics

To see a list of all available help topics:

```bash
chatx help topics
```

This displays a concise list of all help topics available in ChatX.

### Viewing All Help Content with --expand

When you need comprehensive documentation for all help topics:

```bash
chatx help topics --expand
```

The `--expand` flag shows the complete help documentation for every topic instead of just a list of names. This provides an exhaustive reference of all ChatX functionality in one output.

**Example use cases for `--expand`:**

- When learning ChatX for the first time to explore all features
- When looking for a specific feature but unsure which help topic contains it
- When creating custom documentation or reference materials

Since the output can be lengthy, it's often useful to redirect it to a file:

```bash
chatx help topics --expand > full-chatx-help.txt
```

## Searching for Help

To search for specific terms across all help topics:

```bash
chatx help find "SEARCH TERMS"
```

For example:

```bash
chatx help find "azure"
```

## Help for Command-Line Options

For a comprehensive list of all command-line options:

```bash
chatx help options
```

## Common Help Patterns

| Command | Description |
|---------|-------------|
| `chatx help` | General usage help |
| `chatx help topics` | List available help topics |
| `chatx help topics --expand` | View complete documentation for all topics |
| `chatx help find "term"` | Search help for specific terms |
| `chatx help [topic]` | Get help on a specific topic |
| `chatx help examples` | See practical usage examples |

## Best Practices

1. Start with `chatx help` or `chatx help examples` if you're new to ChatX
2. Use `chatx help topics --expand` when you need a complete reference
3. Use `chatx help find` when looking for specific features
4. Check command-specific help (e.g., `chatx help config`) when working with particular commands