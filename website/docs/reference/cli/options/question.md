# `--question` Option

## Description

The `--question` option is an alias that sets `--interactive false`, `--quiet`, and `--input`. It allows you to ask a single question and get an answer without entering interactive mode, with minimal console output.

## Syntax

```bash
chatx --question "YOUR_QUESTION"
```

## Behavior

When using `--question`, ChatX:

1. Sets `--interactive false` (disables interactive chat mode)
2. Sets `--quiet` (reduces console output)
3. Uses the provided text as a single input (like `--input`)

This makes it ideal for quick command-line queries or when scripting with ChatX.

## Examples

### Example 1: Simple Question

```bash
chatx --question "What time is it?"
```

The AI will answer the question with minimal interface elements, returning just the answer.

### Example 2: Using in Scripts

```bash
#!/bin/bash

ANSWER=$(chatx --question "What is the capital of France?")
echo "The capital is: $ANSWER"
```

### Example 3: More Complex Question

```bash
chatx --question "Explain how quantum computing works in simple terms"
```

### Example 4: Using with File Input

If the question argument is a file path and the file exists, the file's content is used:

```bash
chatx --question my_question.txt
```

### Example 5: Reading from stdin

```bash
echo "What is the meaning of life?" | chatx --question @-
```

## Use Cases

The `--question` option is particularly useful for:

- Quick command-line inquiries
- Scripting where you need focused answers
- Simple integration with other tools
- Getting answers with minimal noise in the output

## See Also

- [`--input`](input.md): For providing input without setting `--interactive false` and `--quiet`
- [`--questions`](questions.md): For asking multiple sequential questions in non-interactive mode
- [`--inp`](inp.md): Shortened alias for `--input`
- [`--instruction`](instruction.md): Another alias for `--input`