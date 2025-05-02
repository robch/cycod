# --interactive

The `--interactive` option controls whether CycoD runs in interactive chat mode, allowing for back-and-forth conversation, or non-interactive mode where it processes commands and exits.

## Syntax

```bash
cycod --interactive true|false [other options]
```

## Description

By default, CycoD runs in interactive mode, allowing you to have a continuous conversation with the AI model. When you disable interactive mode, CycoD will process your inputs and exit once complete, making it ideal for use in scripts or automation.

The interactive mode affects how CycoD behaves after receiving responses from the AI model:

- When `--interactive true` (default):
  - CycoD keeps the session open after receiving a response
  - You can continue the conversation by typing more inputs
  - The chat session persists until you explicitly end it

- When `--interactive false`:
  - CycoD processes all provided inputs sequentially
  - After receiving the final response, CycoD exits
  - No further input is accepted

## Default Behavior

The default value for `--interactive` is `true`, unless standard input (stdin) is redirected, in which case it defaults to `false`. This allows for intuitive behavior when piping content into CycoD.

## Examples

### Basic Usage

Start an interactive chat session (default):

```bash
cycod
```

Run in non-interactive mode with a single question:

```bash
cycod --interactive false --input "What time is it?"
```

### Combining with Other Options

Process multiple sequential inputs in non-interactive mode:

```bash
cycod --interactive false --inputs "What is an array?" "Give me example code of arrays in Python"
```

Use in scripts for automation:

```bash
#!/bin/bash
result=$(cycod --interactive false --quiet --input "Convert this date to Unix timestamp: 2025-01-01")
echo "Result: $result"
```

### When to Use Non-Interactive Mode

The non-interactive mode is particularly useful for:

- Scripts and automation
- Processing batch requests
- Generating content programmatically
- Using CycoD in pipelines

## Related Options

- [`--question`](question.md): Implicitly sets `--interactive false` and `--quiet`
- [`--questions`](questions.md): Implicitly sets `--interactive false` and `--quiet`
- [`--foreach`](foreach.md): Implicitly sets `--interactive false` for batch processing

## Notes

- Several options like `--question` and `--foreach` will automatically set `--interactive false`
- When stdin is redirected (e.g., piping content into CycoD), interactive mode defaults to false
- In non-interactive mode, CycoD will still process multiple sequential inputs provided via `--inputs`
- Interactive mode controls the chat session behavior after responses are received, not how inputs are processed