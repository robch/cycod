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
| `--quiet` | Suppress non-essential output |
| `--interactive <true/false>` | Control whether to enter interactive mode (default: true) |
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
| `--trim-token-target <n>` | Set a target token count for trimming chat history when it gets too large |

## Help Options

These options control the help system:

| Option | Description |
|--------|-------------|
| `--expand` | Show expanded help topics with complete content |

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

### Managing Token Usage

If you're having long conversations that might exceed the AI model's context window:

```bash
chatx --trim-token-target 120000 --input-chat-history "long-conversation.jsonl" --output-chat-history "long-conversation.jsonl"
```

This will automatically trim tool call content when the history approaches the specified token target.

### Non-Interactive Mode

For scripting or automation:

```bash
chatx --interactive false --input "What is the current date?" > result.txt
```

## Command Line Input from Files

You can read input content from files in several ways:

### Single Input from a File

Use the @ symbol to read a single input from a file:

```bash
chatx --system-prompt @system-prompt.txt --input @question.txt
```

### Multiple Inputs from a File

Use @@ to read multiple inputs from a file (one per line):

```bash
chatx --inputs @@questions-list.txt
```

### Stdin Input

You can also pipe content to ChatX:

```bash
echo "What is the capital of France?" | chatx
```

## Environment Configuration

In addition to command line options, ChatX can be configured through environment variables. See the [Getting Started guide](getting-started.md) for more details on environment variables.

## Advanced Usage

### Combining Aliases with Options

You can combine aliases with additional options:

```bash
chatx --python-expert --input "Explain decorators in Python" --output-chat-history "python-learning.jsonl"
```

This applies all options from the `python-expert` alias and then adds the additional options specified.

### Debugging Problems

If you're having issues with ChatX, you can enable debug output:

```bash
chatx --debug --input "Test question"
```

This will show detailed information about API calls, token usage, and other internals.