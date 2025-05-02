# `--questions` Option

## Description

The `--questions` option is an alias for `--inputs` that also sets the flags `--interactive false` and `--quiet`. It allows you to provide multiple sequential questions or inputs to the AI model and receive answers without entering interactive mode, with minimal console output.

## Syntax

```bash
cycod --questions "QUESTION1" "QUESTION2" [...]
```

## Behavior

When using `--questions`, CycoD:

1. Sets `--interactive false` (disables interactive chat mode)
2. Sets `--quiet` (reduces console output)
3. Processes multiple inputs sequentially (like `--inputs`)

This makes it ideal for use in scripts or when you want minimal output formatting.

## Examples

### Example 1: Multiple Simple Questions

```bash
cycod --questions "What is the capital of France?" "What is the population of Paris?" "Name three famous landmarks in Paris"
```

The AI will answer each question sequentially with minimal interface elements.

### Example 2: Using in Scripts

```bash
#!/bin/bash

ANSWERS=$(cycod --questions "What time is it now?" "What day of the week is it?")
echo "$ANSWERS" > time_report.txt
```

### Example 3: Multiple Complex Questions

```bash
cycod --questions "Explain how photosynthesis works" "Compare photosynthesis to cellular respiration" "Why is photosynthesis important for life on Earth?"
```

### Example 4: Auto-Reading from stdin

If no arguments are provided, `--questions` will automatically read from standard input:

```bash
echo "What is quantum computing?" | cycod --questions
```

## Input Processing

When using `--questions`:

1. Each argument is treated as a separate user message
2. The AI model responds to each input sequentially
3. If an argument is a file path and the file exists, the file's content is used as the input
4. The output is minimal due to the `--quiet` flag

## See Also

- [`--inputs`](inputs.md): For providing multiple sequential inputs without setting `--interactive false` and `--quiet`
- [`--instructions`](instructions.md): Alias for `--inputs`
- [`--inpu`](inpu.md): Shortened alias for `--instructions`
- [`--question`](question.md): For asking a single question in non-interactive mode