--8<-- "snippets/ai-generated.md"

# Input Options

ChatX provides several options for sending input to the AI model. This page explains the different input options and when to use each one.

## Basic Input Options

### `--input` and Its Aliases (`--inp`, `--instruction`)

The most common way to provide content to the AI model is with the `--input` option. This option accepts one or more text arguments that are sent to the AI model as user messages.

```bash
chatx --input "What is the capital of France?"
```

For convenience, ChatX provides shorter aliases:

```bash
# Using --inp (shortest alias)
chatx --inp "What is the capital of France?"

# Using --instruction alias
chatx --instruction "What is the capital of France?"
```

All three options work identically, allowing you to choose the most readable or convenient form for your use case.

#### Multiple Lines

You can provide multiple arguments to create a multi-line input:

```bash
chatx --inp "Write a Python function" "that calculates the factorial of a number" "and handles edge cases properly"
```

The arguments will be joined with newlines before being sent to the model.

#### Reading from Files

If you provide a file path and the file exists, the content will be read and used as input:

```bash
chatx --inp my_document.txt
```

### `--question` (Non-Interactive Mode)

The `--question` option is similar to `--input`, but it also:
- Sets `--interactive false` (disables interactive chat mode)
- Sets `--quiet` (reduces console output)

This makes it perfect for quick, one-off questions in scripts or command-line use:

```bash
chatx --question "What time is it in Tokyo right now?"
```

## Sequential Input Options

### `--inputs` and Its Aliases (`--instructions`, `--inpu`, `--questions`)

If you want to send multiple sequential inputs to the model (like a conversation), use the plural forms:

```bash
chatx --inputs "What's today's date?" "Show me a calendar for this month"
```

This is equivalent to sending the first input, waiting for a response, and then sending the second input.

Similar to the singular forms, there are aliases:

```bash
# Using --instructions alias
chatx --instructions "Explain what a neural network is" "Now give me a simple example"

# Using --inpu alias (shorter version of --instructions)
chatx --inpu "Write a Python script to download images" "Add error handling"

# Using --questions alias (also sets --interactive false --quiet)
chatx --questions "How tall is Mount Everest?" "What's the second tallest mountain?"
```

#### Multi-Turn Conversations

The sequential input options are particularly powerful for creating multi-turn conversations in a single command. For example, you can progressively refine generated content:

```bash
chatx --inpu "Create a recipe for chocolate chip cookies" "Now make it vegan" "Add some spices to make it unique"
```

Or walk through a step-by-step problem-solving process:

```bash
chatx --instructions "I'm trying to debug this JavaScript code: [code snippet]" "What might be causing the error?" "How can I fix it?"
```

## Using Input Options in Scripts

Input options are particularly useful in scripts:

```bash
#!/bin/bash

# Get today's weather report
weather=$(chatx --question "What's the weather like today in $(curl -s ipinfo.io/city)" --no-templates)

# Process the result
echo "Weather report: $weather"
```

## Reading from Standard Input (stdin)

If you need to pipe content into ChatX, you can use standard input:

```bash
echo "What is the capital of France?" | chatx
```

Or for file content:

```bash
cat document.txt | chatx --inp "Summarize this text: @-"
```

The `@-` syntax tells ChatX to read from stdin.

## Combining with Template Variables

Input options work well with template variables:

```bash
chatx --foreach var name in Alice Bob Charlie --inp "Hello, {name}! How are you today?"
```

## Best Practices

1. **Choose the Right Option**: Use `--inp` for interactive sessions, `--question` for quick answers, and plural forms for multi-turn conversations.

2. **Be Specific**: Provide clear, specific instructions to get better responses.

3. **Use Context**: Add system prompts or context when needed.

4. **Consider Token Limits**: Keep inputs concise when possible to save on token usage.

5. **Use Templates**: For repeated patterns, consider creating templates with variables.

## See Also

- [Template Variables](templates-and-variables.md)
- [Creating Aliases](../advanced/aliases.md)
- [--input Option Reference](../reference/cli/options/input.md)
- [--inp Option Reference](../reference/cli/options/inp.md)
- [--instructions Option Reference](../reference/cli/options/instructions.md)
- [--inpu Option Reference](../reference/cli/options/inpu.md)