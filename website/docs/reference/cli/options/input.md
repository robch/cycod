# --input

The `--input` option allows you to provide one or more lines of input to the AI model.

## Syntax

```bash
chatx --input "TEXT" [other options]
```

You can provide one or more lines of input:

```bash
chatx --input "TEXT1" "TEXT2" [other options]
```

## Description

The `--input` option is one of the primary ways to send instructions, questions, or content to the AI model when using ChatX non-interactively. It provides a flexible way to communicate with the model outside of interactive chat sessions.

When using this option:
- The provided text is sent as a user message to the AI model
- Multiple text arguments are joined with newline characters to form a single message
- If a file path is provided and the file exists, its contents are read and used as input
- The AI model will process your input and generate a response

This option is particularly useful for:
- Sending concise instructions to the AI model
- Providing context or content to be processed
- Creating custom aliases with specific instructions
- Scripting interactions with the AI model

## Examples

### Basic Usage

Ask a simple question:

```bash
chatx --input "What is the capital of France?"
```

### Multiple Line Input

Provide input with multiple lines:

```bash
chatx --input "Please write a Python function" "that calculates the factorial of a number"
```

### Reading from Files

Read content from a file to use as input:

```bash
chatx --input path/to/file.txt
```

### Combining with Other Options

Use with other options for more specific instructions:

```bash
chatx --input "Summarize this text:" --add-system-prompt "Be very concise"
```

### Using Template Variables

Include template variables in your input:

```bash
chatx --input "Hello, {name}!" --var name=Alice
```

### Using in an Alias

Create and use a custom alias:

```bash
chatx --input "Translate the following to Spanish:" --save-alias translate-es
chatx --translate-es "Hello, how are you today?"
```

## Related Options

- [`--inp`](inp.md): Shortened alias for `--input` (identical functionality)
- [`--instruction`](instruction.md): Another alias for `--input` 
- [`--inputs`](inputs.md): For providing multiple sequential inputs to the AI model
- [`--instructions`](instructions.md): Alias for `--inputs`
- [`--inpu`](inpu.md): Shortened alias for `--instructions`
- [`--question`](question.md): Similar to `--input` but also sets `--interactive false --quiet`
- [`--questions`](questions.md): Similar to `--inputs` but also sets `--interactive false --quiet`