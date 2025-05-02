---
hide:
- toc
---

--8<-- "snippets/ai-generated.md"

# Getting Help in CycoD

CycoD provides an extensive built-in help system to guide you through its features and commands. This page covers various ways to access help information in CycoD.

## Basic Help Commands

### General Help

To get started with CycoD help:

```bash
cycod help
```

This displays a general overview of CycoD usage patterns and available commands.

### Topic-Specific Help

To get help on a specific topic:

```bash
cycod help [TOPIC]
```

Replace `[TOPIC]` with any valid help topic such as `config`, `alias`, or `options`.

## Exploring Available Help Topics

### Listing Topics

To see a list of all available help topics:

```bash
cycod help topics
```

This displays a concise list of all help topics available in CycoD.

### Viewing All Help Content with --expand

When you need comprehensive documentation for all help topics:

```bash
cycod help topics --expand
```

The `--expand` flag shows the complete help documentation for every topic instead of just a list of names. This provides an exhaustive reference of all CycoD functionality in one output.

**Example use cases for `--expand`:**

- When learning CycoD for the first time to explore all features
- When looking for a specific feature but unsure which help topic contains it
- When creating custom documentation or reference materials

Since the output can be lengthy, it's often useful to redirect it to a file:

```bash
cycod help topics --expand > full-cycod-help.txt
```

## Searching for Help

To search for specific terms across all help topics:

```bash
cycod help find "SEARCH TERMS"
```

For example:

```bash
cycod help find "azure"
```

## Help for Command-Line Options

For a comprehensive list of all command-line options:

```bash
cycod help options
```

## Common Help Patterns

| Command | Description |
|---------|-------------|
| `cycod help` | General usage help |
| `cycod help topics` | List available help topics |
| `cycod help topics --expand` | View complete documentation for all topics |
| `cycod help find "term"` | Search help for specific terms |
| `cycod help [topic]` | Get help on a specific topic |
| `cycod help examples` | See practical usage examples |

## Best Practices

1. Start with `cycod help` or `cycod help examples` if you're new to CycoD
2. Use `cycod help topics --expand` when you need a complete reference
3. Use `cycod help find` when looking for specific features
4. Check command-specific help (e.g., `cycod help config`) when working with particular commands