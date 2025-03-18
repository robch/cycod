# Command Line Options

ChatX provides a variety of command line options to customize its behavior. This document provides a comprehensive overview of all available options.

## Basic Usage

```
chatx [options]
```

## Global Options

These options apply to the overall behavior of ChatX:

| Option | Description |
|--------|-------------|
| `--debug` | Enable debug output |
| `--verbose` | Enable verbose output |
| `--save-alias <name>` | Save the current command options as a named alias |
| `--help` | Display help information |

## Chat Options

These options control the chat interaction:

| Option | Description |
|--------|-------------|
| `--system-prompt <prompt>` | Set a custom system prompt for the AI |
| `--input <text>` | Provide a single input/question to the AI |
| `--instruction <text>` | Alias for --input |
| `--question <text>` | Alias for --input |
| `--inputs <text...>` | Provide multiple inputs/questions to the AI |
| `--instructions <text...>` | Alias for --inputs |
| `--questions <text...>` | Alias for --inputs |
| `--input-chat-history <file>` | Load previous chat history from a file |
| `--output-chat-history <file>` | Save chat history to a file |

## Examples

### Basic Chat Session

```bash
chatx
```

### Using a Custom System Prompt

```bash
chatx --system-prompt "You are an expert Linux system administrator who gives concise answers."
```

### Asking a Specific Question

```bash
chatx --input "How do I find the largest files in a directory using bash?"
```

### Multiple Questions in Sequence

```bash
chatx --inputs "What is a shell script?" "How do I make a shell script executable?" "How do I add error handling to a shell script?"
```

### Loading and Saving Chat History

```bash
chatx --output-chat-history "programming-help.jsonl"
chatx --input-chat-history "programming-help.jsonl" --input "Can you explain that last example again?"
```

### Creating an Alias

```bash
chatx --system-prompt "You are a helpful coding assistant." --save-alias coding-helper
```

Using the alias:

```bash
chatx --coding-helper --input "Help me write a Python function to sort a list of dictionaries by a specific key."
```

## Command Line Input from Files

You can also use the @ symbol to read input from files:

```bash
chatx --system-prompt @system-prompt.txt --input @question.txt
```

Use @@ to read multiple inputs from a file (one per line):

```bash
chatx --inputs @@questions-list.txt
```