# --add-system-prompt

The `--add-system-prompt` option allows you to add additional text to the system prompt sent to the AI model without replacing the default system prompt entirely.

## Syntax

```bash
chatx --add-system-prompt "INSTRUCTION" [...] [other options]
```

You can provide one or more instructions to add to the system prompt:

```bash
chatx --add-system-prompt "INSTRUCTION1" "INSTRUCTION2" [other options]
```

## Description

The system prompt is a special set of instructions given to the AI model that defines its behavior, capabilities, and constraints. CHATX comes with a default system prompt that provides general guidance to the model.

While the `--system-prompt` option completely replaces the default system prompt, `--add-system-prompt` appends additional instructions to it. This is useful when you want to:

- Maintain the default system prompt's functionality
- Add specific behavior modifications or constraints
- Reinforce critical instructions without rewriting the entire system prompt

Multiple `--add-system-prompt` instructions are combined and appended to the default system prompt.

## Examples

### Basic Usage

Add a single instruction to the system prompt:

```bash
chatx --add-system-prompt "Always include examples in your answers." --question "What is an array?"
```

### Multiple Instructions

Add multiple instructions to the system prompt:

```bash
chatx --add-system-prompt "Answer in a casual, friendly tone." "Include examples in your responses." --question "Explain quantum computing."
```

### Security Constraint

Add a security constraint to the system prompt:

```bash
chatx --add-system-prompt "Never access files outside the current directory." --question "List the files in this directory."
```

### Language Constraint

Force the model to use a specific language:

```bash
chatx --add-system-prompt "Respond in Spanish only." --question "What is the weather like today?"
```

### Response Format

Control the format of responses:

```bash
chatx --add-system-prompt "Format your answers as bullet points." --question "What are the benefits of exercise?"
```

### Combining with Other Options

You can combine `--add-system-prompt` with other options:

```bash
chatx --add-system-prompt "Be concise." --add-user-prompt "I need help with coding." --question "How do I sort an array in Python?"
```

## Best Practices

1. **Be Specific**: Provide clear, specific instructions that the model can follow.
2. **Avoid Contradictions**: Make sure your added instructions don't contradict the default system prompt or each other.
3. **Test and Refine**: Experiment with different instructions to find what works best for your needs.
4. **Remember Context Limits**: Very long system prompts consume token context. Keep instructions concise.
5. **Prioritize Important Instructions**: Place the most important instructions first, as models tend to weigh earlier instructions more heavily.

## Related Options

- [`--system-prompt`](system-prompt.md): Replace the default system prompt entirely
- [`--add-user-prompt`](add-user-prompt.md): Add user prompt(s) prepended to the first input