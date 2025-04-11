---
hide:
- toc
---

--8<-- "snippets/ai-generated.md"

# Using the ChatX Help System

ChatX provides a comprehensive help system to assist you in learning and using its various features. This tutorial will guide you through the different ways to access help in ChatX.

## Basic Help Commands

To get general help with ChatX, use the base help command:

```bash
chatx help
```

This displays a summary of common usage patterns and available commands.

## Getting Help on Specific Topics

To see information about a specific topic:

```bash
chatx help [TOPIC]
```

For example:

```bash
chatx help config
```

## Listing All Help Topics

To see a list of all available help topics:

```bash
chatx help topics
```

This gives you a condensed list of all topics you can get help on.

## Exploring All Help Documentation with --expand

When you want to see the complete help documentation for all topics at once, use the `--expand` flag:

```bash
chatx help topics --expand
```

This command displays the full help content for every topic in ChatX, providing a comprehensive reference. This is especially useful when:

- You're new to ChatX and want to explore all its capabilities
- You need to find a specific command or option but aren't sure which help topic it belongs to
- You want to create your own custom reference or documentation for ChatX

The output will be extensive, so you might want to pipe it to a file:

```bash
chatx help topics --expand > chatx-full-docs.txt
```

## Searching for Help

To search for specific terms within the help system:

```bash
chatx help find "SEARCH TERMS"
```

For example, to find all help entries related to OpenAI:

```bash
chatx help find "openai"
```

## Tips for Using the Help System

- Use `chatx help options` to see all available command-line options
- Use `chatx help examples` for practical usage examples
- When you need detailed information on a specific command, use `chatx help [COMMAND]`
- Use `chatx help topics --expand` when you want to browse all available documentation

By mastering the help system, you'll be able to quickly find information about ChatX features and commands as you need them.