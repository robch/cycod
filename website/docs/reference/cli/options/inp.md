# --inp

The `--inp` option is a shortened alias for `--input` that allows you to provide one or more lines of input to the AI model.

## Syntax

```bash
chatx --inp "TEXT" [other options]
```

You can provide one or more lines of input:

```bash
chatx --inp "TEXT1" "TEXT2" [other options]
```

## Description

The `--inp` option is a convenient shorthand for `--input`. It allows you to send instructions, questions, or content to the AI model. This is one of the primary ways to communicate with the model when using ChatX non-interactively.

When using this option:
- The provided text is sent as a user message to the AI model
- Multiple text arguments are joined with newline characters
- If a file path is provided and the file exists, its contents are read and used as input
- Unlike `--question`, this option does not implicitly disable interactive mode or quiet output

This option is particularly useful for:
- Sending concise instructions to the AI model
- Providing context or content to be processed
- Creating custom aliases with shorter command syntax

## Examples

### Basic Usage

Ask a simple question:

```bash
chatx --inp "What is the capital of France?"
```

### Multiple Line Input

Provide input with multiple lines:

```bash
chatx --inp "Please write a Python function" "that calculates the factorial of a number"
```

### Reading from Files

Read content from a file to use as input:

```bash
chatx --inp path/to/file.txt
```

### Combining with Other Options

Use with other options for more specific instructions:

```bash
chatx --inp "Summarize this text:" --add-system-prompt "Be very concise"
```

### Using Template Variables

Include template variables in your input:

```bash
chatx --inp "Hello, {name}!" --var name=Alice
```

### Using in an Alias

Create and use a custom alias:

```bash
chatx --inp "Translate the following to Spanish:" --save-alias translate-es
chatx --translate-es "Hello, how are you today?"
```

## Related Options

- [`--input`](input.md): The full form of this option (identical functionality)
- [`--instruction`](instruction.md): Another alias for `--input`
- [`--question`](question.md): Similar to `--input` but also sets `--interactive false --quiet`
- [`--inputs`](inputs.md): Provide multiple sequential inputs to the AI model