# --instructions

The `--instructions` option is an alias for [`--inputs`](inputs.md) and allows you to provide multiple sequential inputs to the AI model, creating a multi-turn conversation in a single command.

## Syntax

```bash
cycod --instructions "INPUT1" "INPUT2" [other options]
```

## Description

The `--instructions` option enables you to send multiple sequential inputs to the AI model, where each input is treated as a separate user message. The AI model will respond to each message in sequence, creating a conversation similar to what you would experience in interactive mode.

When using this option:
- Each string argument is sent as a separate user message to the AI model
- The AI model will respond to each message in sequence
- This creates a multi-turn conversation in a single command
- Unlike `--input` or `--instruction` (singular), which combines multiple arguments into a single message

> **Note:** Don't confuse `--instructions` (plural) with `--instruction` (singular). The plural form creates a multi-turn conversation, while the singular form combines arguments into a single message.

This option is particularly useful for:
- Scripting complex interactions with the AI model
- Creating tutorial-like sequences of questions and instructions
- Testing conversation flows non-interactively
- Batch processing of related but sequential queries

## Examples

### Multi-Step Instructions

Guide the AI through a multi-step process:

```bash
cycod --instructions "Create a recipe for chocolate cake" "Now modify it to be gluten-free" "Add suggestions for frosting options"
```

### Conversational Flow

Simulate a natural conversation flow:

```bash
cycod --instructions "Tell me about Mars" "How long would it take to travel there?" "What challenges would humans face living there?"
```

### Code Development

Develop code through iterative instructions:

```bash
cycod --instructions "Write a JavaScript function to sort an array of objects by a property" "Add error handling" "Now optimize it" "Add example usage"
```

### Debugging Assistant

Create a debugging workflow:

```bash
cycod --instructions "Here's my code with a bug: [code]" "What might be causing the issue?" "How can I fix it?"
```

### Project Analysis Workflow

Analyze a codebase in steps:

```bash
cycod --instructions "Look at the files in this folder. Summarize what they each do" "Which files should I focus on to understand the core functionality?" "Suggest improvements to the architecture based on what you've seen"
```

### Using Templates and Variables

Incorporate variables across multiple instructions:

```bash
cycod --var language=Java --var feature="error handling" --instructions "Create a {language} class for file operations" "Improve the {feature} in this code"
```

### Command Execution Workflow

Use tools and commands in sequence:

```bash
cycod --instructions "Use gh to find the open issues assigned to me" "Analyze these issues and suggest which ones I should tackle first" "Help me draft a comment for the highest priority issue"
```

## Related Options

- [`--inputs`](inputs.md): The main option that `--instructions` aliases to (identical functionality)
- [`--inpu`](inpu.md): Shortened alias for `--instructions` (identical functionality)
- [`--input`](input.md): For providing a single input (possibly with multiple lines)
- [`--inp`](inp.md): Shortened alias for `--input`
- [`--questions`](questions.md): Similar to `--instructions` but also sets `--interactive false --quiet`
- [`--instruction`](instruction.md): For providing a single instruction (alias for `--input`)