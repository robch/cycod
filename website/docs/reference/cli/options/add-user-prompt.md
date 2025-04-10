# --add-user-prompt

The `--add-user-prompt` option allows you to add user prompt(s) that are prepended to the first input, question, or instruction in your chat session.

## Syntax

```bash
chatx --add-user-prompt "TEXT" [...] [other options]
```

You can provide one or more text entries to add as user prompts:

```bash
chatx --add-user-prompt "TEXT1" "TEXT2" [other options]
```

## Description

In a chat session, there are system prompts that guide the AI's behavior and user inputs that represent your questions or instructions. The `--add-user-prompt` option lets you automatically insert text as user messages at the beginning of your conversation.

This is useful when you want to:

- Set the context for your conversation
- Provide background information
- Define specific roles or personas for the session
- Include standard instructions that should be processed before your main question

Unlike `--add-system-prompt` which affects how the AI behaves, `--add-user-prompt` adds content that the AI will treat as coming from you, the user.

## Examples

### Basic Usage

Add a simple context before asking a question:

```bash
chatx --add-user-prompt "I'm working with Python 3.10." --question "How do I use match statements?"
```

### Setting a Role

Define a specific role or persona for the conversation:

```bash
chatx --add-user-prompt "I'm a junior developer learning web development." --question "Explain how CSS flexbox works."
```

### Multiple Context Items

Add multiple pieces of context:

```bash
chatx --add-user-prompt "I use VS Code." "I'm on Windows 11." --question "How do I set up a Python virtual environment?"
```

### Language Preference

Specify your preferred language for responses:

```bash
chatx --add-user-prompt "Please respond in French." --question "What are the top tourist attractions in Paris?"
```

### Combining with System Prompts

You can combine `--add-user-prompt` with system prompt options:

```bash
chatx --add-system-prompt "Be concise." --add-user-prompt "I need help with coding." --question "How do I sort an array in Python?"
```

### Interactive Mode

When starting an interactive session with predefined context:

```bash
chatx --add-user-prompt "We're discussing advanced JavaScript concepts." --add-user-prompt "Focus on ES6+ features."
```

## Best Practices

1. **Be Clear and Specific**: Provide clear context that will help frame your questions appropriately.
2. **Don't Repeat Yourself**: Use `--add-user-prompt` for context that would otherwise need to be repeated in multiple questions.
3. **Consider Token Usage**: Remember that these prompts consume token context, so keep them concise when possible.
4. **Order Matters**: User prompts are processed in the order provided, so organize them logically.
5. **Distinguish from System Prompts**: Use system prompts for behavior instructions and user prompts for context or background information.

## Related Options

- [`--system-prompt`](system-prompt.md): Replace the default system prompt entirely
- [`--add-system-prompt`](add-system-prompt.md): Add text to the system prompt
- [`--input`](input.md): Provide one or more lines of inputs to the AI model
- [`--question`](question.md): Shorthand for providing a simple question